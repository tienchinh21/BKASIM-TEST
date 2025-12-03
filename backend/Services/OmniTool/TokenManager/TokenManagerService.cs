using Hangfire;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Dependencies.Zalo;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Models.DTOs.SystemSettings;

namespace MiniAppGIBA.Services.OmniTool.TokenManager
{
    public class TokenManagerService(IUnitOfWork unitOfWork) : ITokenManagerService
    {
        private readonly IRepository<Common> _commonRepository = unitOfWork.GetRepository<Common>();

        public async Task<OaInfoDTO> GetAllInfo()
        {
            var appId = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "AppId");
            var secretKey = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "SecretKey");
            var accessToken = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "AccessToken");
            var refreshToken = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "RefreshToken");

            return new OaInfoDTO()
            {
                AppId = appId?.Content,
                SecretKey = secretKey?.Content,
                AccessToken = accessToken?.Content,
                RefreshToken = refreshToken?.Content,
            };
        }

        public async Task<string?> GetAccessToken()
        {
            var common = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "AccessToken");
            if (common != null) return common.Content;
            return string.Empty;
        }

        public async Task<string?> GetRefreshToken()
        {
            var common = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "RefreshToken");
            if (common != null) return common.Content;
            return string.Empty;
        }

        public async Task UpdateToken(string? access_token, string? refresh_token)
        {
            var accessToken = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "AccessToken");
            var refreshToken = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "RefreshToken");

            if (accessToken != null && !string.IsNullOrEmpty(access_token))
            {
                accessToken.Content = access_token;
            }
            else
            {
                _commonRepository.Add(new Common
                {
                    Name = "AccessToken",
                    Content = access_token ?? string.Empty
                });
            }
            var checkRefreshToken = refreshToken == null;
            if (refreshToken != null && !string.IsNullOrEmpty(refresh_token))
            {
                refreshToken.Content = refresh_token;
            }
            else
            {
                _commonRepository.Add(new Common
                {
                    Name = "RefreshToken",
                    Content = refresh_token ?? string.Empty
                });
            }
            var result = await unitOfWork.SaveChangesAsync();

            if (result > 0 && checkRefreshToken)
            {
                var options = new RecurringJobOptions { TimeZone = TimeZoneInfo.Local };
                RecurringJob.AddOrUpdate("UpdateAccessTokenDaily", () => RefreshToken(), Cron.Daily, options);
            }
        }

        public async Task UpdateAppInfo(string? appId, string? secretKey)
        {

            var _appId = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "AppId");
            var _secretKey = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "SecretKey");

            if (_appId != null && !string.IsNullOrEmpty(appId))
            {
                _appId.Content = appId; // Cập nhật giá trị
            }
            else
            {
                _commonRepository.Add(new Common()
                {
                    Name = "AppId",
                    Content = appId ?? string.Empty
                });
            }

            if (_secretKey != null && !string.IsNullOrEmpty(secretKey))
            {
                _secretKey.Content = secretKey; // Cập nhật giá trị
            }
            else
            {
                _commonRepository.Add(new Common()
                {
                    Name = "SecretKey",
                    Content = secretKey ?? string.Empty
                });
            }
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> RefreshToken()
        {
            var appId = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "AppId");
            var secretKey = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "SecretKey");
            var accessToken = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "AccessToken");
            var refreshToken = await _commonRepository.AsQueryable().FirstOrDefaultAsync(x => x.Name == "RefreshToken");

            if (refreshToken == null || appId == null || secretKey == null || accessToken == null) return false;

            var newTokenInfo = await ZaloAuthentication.GetAccessTokenFromRefreshToken(refreshToken.Content, appId.Content, secretKey.Content);
            if (newTokenInfo != null && !string.IsNullOrEmpty(newTokenInfo.access_token) && !string.IsNullOrEmpty(newTokenInfo.refresh_token))
            {
                accessToken.Content = newTokenInfo.access_token;
                refreshToken.Content = newTokenInfo.refresh_token;
                return await unitOfWork.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
