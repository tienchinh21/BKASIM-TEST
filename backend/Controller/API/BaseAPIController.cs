using Microsoft.AspNetCore.Mvc;

namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseAPIController : ControllerBase
    {
        protected string? GetCurrentUserId()
        {
            return User?.FindFirst("UserId")?.Value;
        }

        protected string? GetCurrentUserZaloId()
        {
            return User?.FindFirst("UserZaloId")?.Value;
        }

        protected bool IsAuthenticated()
        {
            return User?.Identity?.IsAuthenticated ?? false;
        }

        protected bool IsAdmin()
        {
            return User?.IsInRole("ADMIN") ?? false;
        }

        protected IActionResult Success<T>(T data, string message = "Thành công")
        {
            return Ok(new
            {
                success = true,
                message,
                data
            });
        }

        protected IActionResult Error(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message,
                data = (object?)null
            });
        }
    }
}

