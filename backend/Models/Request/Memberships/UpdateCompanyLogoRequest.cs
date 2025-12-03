using Microsoft.AspNetCore.Http;

namespace MiniAppGIBA.Models.Request.Memberships
{
    public class UpdateCompanyLogoRequest
    {
        public IFormFile CompanyLogoFile { get; set; }
    }
}