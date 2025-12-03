using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Controllers.API;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.SystemSettings;
using Newtonsoft.Json;

namespace MiniAppGIBA.Services.SystemSettings
{
    public class SystemSettingService(IUnitOfWork unitOfWork) : ISystemSettingService
    {
        private readonly IRepository<Common> _commonRepository = unitOfWork.GetRepository<Common>();

        #region Omni Account

        public async Task<OmniAccountDTO> GetOmniAccountAsync()
        {
            try
            {
                var account = await _commonRepository.AsQueryable()
                .SingleOrDefaultAsync(x => x.Name == "OmniAccount");

                return string.IsNullOrEmpty(account?.Content)
                    ? new OmniAccountDTO()
                    : JsonConvert.DeserializeObject<OmniAccountDTO>(account.Content) ?? new OmniAccountDTO();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OmniAccountDTO();
            }
        }

        public async Task<int> AddOrUpdateAccountOmniAsync(OmniAccountDTO omniAccount)
        {
            var account = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "OmniAccount");
            if (account == null)
            {
                account = new Common
                {
                    Name = "OmniAccount",
                    Content = JsonConvert.SerializeObject(omniAccount)
                };
                _commonRepository.Add(account);
            }
            else
            {
                account.Content = JsonConvert.SerializeObject(omniAccount);
                _commonRepository.Update(account);
            }
            return await unitOfWork.SaveChangesAsync();
        }

        #endregion
    }
}
