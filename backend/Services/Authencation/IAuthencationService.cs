using System.Security.Claims;

namespace MiniAppGIBA.Service.Authencation
{
    public interface IAuthencationService
    {
        // CMS Login (Username/Password)
        Task<string> LoginCmsAsync(string username, string password);
        Task<List<Claim>> GetCmsClaimsAsync(string username, string password);

        // MiniApp Login (UserZaloId/PhoneNumber)
        Task<string> LoginMiniAppAsync(string phoneNumber, string userZaloId);
        Task<List<Claim>> GetMiniAppClaimsAsync(string phoneNumber, string userZaloId);
    }
}