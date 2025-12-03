using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MiniAppGIBA.Service.Authencation
{
    public class AuthencationService(
        IConfiguration configuration,
        IUnitOfWork unitOfWork
        ) : IAuthencationService
    {
        private readonly IRepository<Membership> _membershipRepo = unitOfWork.GetRepository<Membership>();
        private readonly IRepository<Roles> _rolesRepo = unitOfWork.GetRepository<Roles>();

        #region CMS Authentication (Admin Login)

        public async Task<List<Claim>> GetCmsClaimsAsync(string username, string password)
        {
            // Find user by email only
            var user = await _membershipRepo.AsQueryable()
                .FirstOrDefaultAsync(m => m.Username == username && m.IsDelete != true);

            if (user == null || user.Username == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy người dùng.");
            }
            
            var passwordHash = AuthHelper.HashPassword(password);
            if (string.IsNullOrEmpty(user.Password) || !AuthHelper.VerifyPassword(password, user.Password))
            {
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng. Vui lòng kiểm tra lại.");
            }   

            var userRoles = await _rolesRepo.AsQueryable()
                .FirstOrDefaultAsync(r => r.Id == user.RoleId);

            var authClaims = new List<Claim>
            {
                new Claim("UserId", user.Id),
                // new Claim("UserZaloId", user.UserZaloId ?? ""),
                new Claim(ClaimTypes.Role, userRoles?.Name ?? ""),
                new Claim(ClaimTypes.Name, user.Fullname ?? user.Username ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("RoleId", user.RoleId ?? "")
            };

          
           

            // GIBA has full access - no GroupPermission claims needed
            // Non-GIBA users don't have admin access

            return authClaims;
        }

        public async Task<string> LoginCmsAsync(string username, string password)
        {
            var claims = await GetCmsClaimsAsync(username, password);
            return JwtGenerator.GenerateJwtToken(configuration, claims);
        }

        #endregion

        #region MiniApp Authentication (User Login via Zalo)

        public async Task<List<Claim>> GetMiniAppClaimsAsync(string phoneNumber, string userZaloId)
        {
            // Tìm membership theo phone và userZaloId
            var membership = await _membershipRepo.AsQueryable()
                .FirstOrDefaultAsync(m => m.PhoneNumber == phoneNumber && m.UserZaloId == userZaloId && m.IsDelete != true);

            if (membership == null)
            {
                throw new CustomException(404, $"Số điện thoại {phoneNumber} chưa đăng ký thành viên!");
            }

            var userRole = await _rolesRepo.AsQueryable()
                .FirstOrDefaultAsync(r => r.Id == membership.RoleId);

            var authClaims = new List<Claim>
            {
                new Claim("UserId", membership.Id),
                new Claim("UserZaloId", membership.UserZaloId),
                new Claim("PhoneNumber", membership.PhoneNumber),
                new Claim(ClaimTypes.Name, membership.Fullname),
                new Claim("ZaloAvatar", membership.ZaloAvatar ?? ""),
                new Claim(ClaimTypes.Role, userRole?.Name ?? ""),
                new Claim("RoleId", userRole?.Id ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return authClaims;
        }

        public async Task<string> LoginMiniAppAsync(string phoneNumber, string userZaloId)
        {
            var claims = await GetMiniAppClaimsAsync(phoneNumber, userZaloId);
            return JwtGenerator.GenerateJwtToken(configuration, claims);
        }

        #endregion
    }
}
