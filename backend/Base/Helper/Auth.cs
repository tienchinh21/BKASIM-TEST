using BCrypt.Net;
namespace MiniAppGIBA.Base.Helper;
public class AuthHelper
{
    // Hash password
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Verify password
    public static bool VerifyPassword(string password, string hashed)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashed);
    }
}