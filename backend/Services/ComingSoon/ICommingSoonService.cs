using MiniAppGIBA.Models.Response.CommingSoon;
namespace MiniAppGIBA.Services.ComingSoon
{
    public interface ICommingSoonService
    {
        Task<CommingSoonResponse> GetComingSoon(string? userZaloId = null);
    }
}