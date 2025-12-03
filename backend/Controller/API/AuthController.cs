using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Service.Authencation;
using MiniAppGIBA.Models.Request.Auth;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Memberships;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Entities.Admins;

namespace MiniAppGIBA.Controller.API
{
    public class AuthController : BaseAPIController
    {
        private readonly IAuthencationService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IRepository<Roles> _rolesRepo;
        public AuthController(IAuthencationService authService, ILogger<AuthController> logger, IRepository<Membership> membershipRepository, IRepository<Roles> rolesRepo)
        {
            _authService = authService;
            _logger = logger;
            _membershipRepository = membershipRepository;
            _rolesRepo = rolesRepo;
        }

        /// <summary>
        /// MiniApp Login - Đăng nhập qua Zalo (UserZaloId + PhoneNumber)
        /// </summary>
        [HttpPost("miniapp-login")]
        public async Task<IActionResult> MiniAppLogin([FromForm] string userZaloId, [FromForm] string phoneNumber)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(userZaloId))
                {
                    return BadRequest(new
                    {
                        Code = 1,
                        Message = "UserZaloId là bắt buộc",
                        Data = (object?)null
                    });
                }

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return BadRequest(new
                    {
                        Code = 1,
                        Message = "Số điện thoại là bắt buộc",
                        Data = (object?)null
                    });
                }

                var token = await _authService.LoginMiniAppAsync(phoneNumber, userZaloId);

                // Lấy thông tin user từ database
                var membership = await _membershipRepository.AsQueryable()
                    .FirstOrDefaultAsync(m => m.PhoneNumber == phoneNumber && m.UserZaloId == userZaloId);
                var userRole = await _rolesRepo.AsQueryable()
                    .FirstOrDefaultAsync(r => r.Id == membership!.RoleId);
                return Success(new
                {
                    message = "Đăng nhập thành công!",
                    token = token,
                    userInfo = new
                    {
                        userZaloId = userZaloId,
                        phoneNumber = phoneNumber,
                        fullname = membership?.Fullname,
                        zaloAvatar = membership?.ZaloAvatar,
                        roleName = userRole?.Name
                    }
                });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"MiniApp login failed: {ex.Message}");

                // Nếu user chưa đăng ký, trả về lỗi
                if (ex.Message.Contains("chưa đăng ký"))
                {
                    return Error(ex.Message, 404);
                }

                // Nếu là lỗi về trạng thái tài khoản (code 200), trả về thông tin trạng thái
                if (ex.Code == 200)
                {
                    // Lấy thông tin user từ database
                    var membership = await _membershipRepository.AsQueryable()
                        .FirstOrDefaultAsync(m => m.PhoneNumber == phoneNumber && m.UserZaloId == userZaloId);

                    return Success(new
                    {
                        message = ex.Message,
                        isLoginSuccess = false,
                        userInfo = new
                        {
                            userZaloId = userZaloId,
                            phoneNumber = phoneNumber,
                            fullname = membership?.Fullname,
                            zaloAvatar = membership?.ZaloAvatar
                        }
                    });
                }

                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MiniApp login");
                return Error("Có lỗi xảy ra khi đăng nhập", 500);
            }
        }

        /// <summary>
        /// CMS Login - Đăng nhập Admin (Username + Password)
        /// </summary>
        [HttpPost("cms-auth")]
        public async Task<IActionResult> CmsLogin([FromForm] string username, [FromForm] string password)
        {
            try
            {
                var token = await _authService.LoginCmsAsync(username, password);
                Console.WriteLine($"Token: {token}");
                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công!",
                    Token = token
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"CMS login failed: {ex.Message}");
                return StatusCode(401, new
                {
                    Code = 1,
                    Message = ex.Message,
                    Data = (object?)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CMS login");
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }

        /// <summary>
        /// Get current user info from token
        /// </summary>
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            if (!IsAuthenticated())
            {
                return Error("Chưa đăng nhập", 401);
            }

            var userId = GetCurrentUserId();
            var userZaloId = GetCurrentUserZaloId();
            var name = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var roles = User?.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

            return Success(new
            {
                userId,
                userZaloId,
                name,
                roles
            });
        }

    }
}
