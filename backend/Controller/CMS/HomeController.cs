using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Service.Authencation;
using System.Security.Claims;
using System.Text;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class HomeController(IConfiguration configuration, IAuthencationService authencationService) : Microsoft.AspNetCore.Mvc.Controller
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            ViewBag.Error = TempData["Error"] ?? "Vui lòng đăng nhập để tiếp tục.";
            return View("Views/Login/Login.cshtml");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        [ActionName("Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                Console.WriteLine($"Attempting login for username: {username}");
                var claims = await authencationService.GetCmsClaimsAsync(username, password);
                Console.WriteLine($"Login successful, claims count: {claims.Count}");
                var accessToken = JwtGenerator.GenerateJwtToken(configuration, claims);
                var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(accessToken));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.Now.AddHours(12),
                    AllowRefresh = true
                };
                TempData["access_token"] = encodedToken;

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrinciple, authProperties);
                Console.WriteLine("Authentication cookie set, redirecting to Dashboard");
                Console.WriteLine($"Cookie scheme: {CookieAuthenticationDefaults.AuthenticationScheme}");
                Console.WriteLine($"User authenticated after signin: {HttpContext.User.Identity?.IsAuthenticated}");

                return Redirect("/");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                TempData["Error"] = "Sai tên đăng nhập hoặc mật khẩu";
                return Redirect("/Login");
            }
        }

        [HttpGet]
        [Route("Logout")]
        [ActionName("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Sign out from cookie authentication
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Clear all cookies
                HttpContext.Response.Cookies.Delete(".AspNetCore.Cookies");
                HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");

                // Clear session
                HttpContext.Session.Clear();

                // Redirect to login with return URL
                return Redirect("/Login?ReturnUrl=%2F");
            }
            catch (Exception ex)
            {
                // Log error if needed
                return Redirect("/Login?ReturnUrl=%2F");
            }
        }

        public IActionResult Index()
        {
            // Redirect to Dashboard if authenticated, otherwise to Login
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public IActionResult Affiliate()
        {
            return View();
        }

        public IActionResult Setting()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
