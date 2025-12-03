using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Response.CommingSoon;
using MiniAppGIBA.Services.ComingSoon;

namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComingSoonController : BaseAPIController
    {
        private readonly ICommingSoonService _comingSoonService;

        public ComingSoonController(ICommingSoonService comingSoonService)
        {
            _comingSoonService = comingSoonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetComingSoon()
        {
            try
            {
                // Chỉ lấy userZaloId, không check role
                var userZaloId = GetCurrentUserZaloId();

                var result = await _comingSoonService.GetComingSoon(userZaloId);

                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy dữ liệu sắp diễn ra",
                    Error = ex.Message
                });
            }
        }
    }
}