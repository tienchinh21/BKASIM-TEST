using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Services.Sponsors;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Models.Response.Common;

namespace MiniAppGIBA.Controller.API
{
    [Route("api/sponsors")]
    [ApiController]
    public class SponsorsController : BaseAPIController
    {
        private readonly ISponsorService _sponsorService;
        private readonly ILogger<SponsorsController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SponsorsController(
            ISponsorService sponsorService,
            ILogger<SponsorsController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _sponsorService = sponsorService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Lấy chi tiết nhà tài trợ
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSponsorDetail(string id)
        {
            try
            {
                var sponsor = await _sponsorService.GetSponsorByIdAsync(id);
                if (sponsor == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Code = 1,
                        Message = "Không tìm thấy nhà tài trợ"
                    });
                }

                // Convert relative URLs to full URLs
                var sponsorDetail = new
                {
                    id = sponsor.Id,
                    sponsorName = sponsor.SponsorName,
                    introduction = sponsor.Introduction,
                    image = GetFullUrl(sponsor.Image),
                    websiteURL = sponsor.WebsiteURL,
                    isActive = sponsor.IsActive,
                    createdDate = sponsor.CreatedDate,
                    updatedDate = sponsor.UpdatedDate
                };

                return Ok(new ApiResponse<object>
                {
                    Code = 0,
                    Message = "Lấy chi tiết nhà tài trợ thành công",
                    Data = sponsorDetail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sponsor detail {SponsorId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy chi tiết nhà tài trợ"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách nhà tài trợ hoạt động
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSponsors()
        {
            try
            {
                var sponsors = await _sponsorService.GetActiveSponsorsAsync();

                // Convert relative URLs to full URLs
                var sponsorsWithFullUrls = sponsors.Select(s => new
                {
                    id = s.Id,
                    sponsorName = s.SponsorName,
                    introduction = s.Introduction,
                    image = GetFullUrl(s.Image),
                    websiteURL = s.WebsiteURL,
                    isActive = s.IsActive,
                    createdDate = s.CreatedDate,
                    updatedDate = s.UpdatedDate
                }).ToList();

                return Ok(new ApiResponse<object>
                {
                    Code = 0,
                    Message = "Lấy danh sách nhà tài trợ thành công",
                    Data = sponsorsWithFullUrls
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sponsors");
                return StatusCode(500, new ApiResponse<object>
                {
                    Code = 1,
                    Message = "Có lỗi xảy ra khi lấy danh sách nhà tài trợ"
                });
            }
        }

        private string GetFullUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return relativePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{relativePath}";
        }
    }
}
