using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Controller.CMS;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Services.OmniTool.TokenManager;
using MiniAppGIBA.Services.SystemSettings;

namespace MiniAppGIBA.Controllers.CMS
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class SystemSettingController(ISystemSettingService systemSettingService,
                                         ITokenManagerService tokenManagerService) : BaseCMSController
    {
        public async Task<IActionResult> Index()
        {
            var omniAccount = await systemSettingService.GetOmniAccountAsync();

            var oaInfo = await tokenManagerService.GetAllInfo();

            return View((omniAccount, oaInfo));
        }
    }
}
