using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Base.Interface;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Helper;
namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommonController : BaseAPIController
    {
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CommonController(IRepository<Membership> membershipRepository, IUnitOfWork unitOfWork)
        {
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
        }
        [HttpGet("Gen-Code-Membership")]
        public async Task<IActionResult> GenCodeMembership()
        {
            // This endpoint is deprecated as Code field has been removed from Membership
            return Success(new { message = "Endpoint deprecated - Code field removed from Membership" });
        }
    }
}