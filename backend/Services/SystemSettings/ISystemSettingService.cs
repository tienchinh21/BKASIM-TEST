using MiniAppGIBA.Models.DTOs.SystemSettings;

namespace MiniAppGIBA.Services.SystemSettings
{
    public interface ISystemSettingService
    {
        Task<OmniAccountDTO> GetOmniAccountAsync();
        Task<int> AddOrUpdateAccountOmniAsync(OmniAccountDTO omniAccount);
    }
}
