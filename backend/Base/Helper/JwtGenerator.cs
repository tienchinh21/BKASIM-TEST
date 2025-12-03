using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiniAppGIBA.Base.Helper
{
    public class JwtGenerator
    {
        public static string GenerateJwtToken(IConfiguration config, List<Claim> claims)
        {
            var jwtKey = config["Jwt:SecretKey"];

            if (config == null || string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("Lỗi khi generate token!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
