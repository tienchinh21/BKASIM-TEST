using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.SystemSettings;
using MiniAppGIBA.Services.OmniTool.TokenManager;
using MiniAppGIBA.Services.SystemSettings;

namespace MiniAppGIBA.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN, SUPER_ADMIN, EMPLOYEE")]
    public class SystemSettingsController(ILogger<SystemSettingsController> logger,
                                          ISystemSettingService systemSettingService,
                                          ITokenManagerService tokenManagerService) : ControllerBase
    {
        #region "Omni Account"

        [HttpPost("OmniAccount")]
        public async Task<IActionResult> CreateOrUpdateOmniAccount([FromBody] OmniAccountDTO model)
        {
            try
            {
                await systemSettingService.AddOrUpdateAccountOmniAsync(model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Cập nhật tài khoản omni thành công!"
                });
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        #endregion

        #region Oa token manager

        [HttpPost("TokenOa")]
        public async Task<IActionResult> UpdateToken([FromBody] OaInfoDTO oaInfo)
        {
            try
            {
                await tokenManagerService.UpdateToken(oaInfo.AccessToken, oaInfo.RefreshToken);
                return Ok(new
                {
                    Code = 0,
                    Message = "Thanh cong"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("AppInfo")]
        public async Task<IActionResult> UpdateApp([FromBody] OaInfoDTO oaInfo)
        {
            try
            {
                await tokenManagerService.UpdateAppInfo(oaInfo.AppId, oaInfo.SecretKey);
                return Ok(new
                {
                    Code = 0,
                    Message = "Thanh cong"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred");
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        private IActionResult ServerError(Exception ex)
        {
            logger.LogDebug(ex.Message);
            return Ok(new
            {
                Code = 1,
                Message = "Internal Server Errors"
            });
        }
    }

    public class CMSRefRequest
    {
        public string? BrandName { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? FaviIcon { get; set; }
    }
}
