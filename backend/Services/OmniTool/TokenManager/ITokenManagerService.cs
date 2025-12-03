using MiniAppGIBA.Models.DTOs.SystemSettings;

namespace MiniAppGIBA.Services.OmniTool.TokenManager
{
    public interface ITokenManagerService
    {
        Task<bool> RefreshToken();
        Task<string?> GetAccessToken();
        Task<string?> GetRefreshToken();
        Task<OaInfoDTO> GetAllInfo();

        Task UpdateAppInfo(string? appId, string? secretKey);
        Task UpdateToken(string? access_token, string? refresh_token);

    }
}
