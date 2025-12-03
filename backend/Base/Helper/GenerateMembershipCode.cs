
namespace MiniAppGIBA.Base.Helper
{
    public class GenerateMembershipCode
    {
        private static readonly char[] CodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private static readonly Random Random = new();
        public static string GenerateCode(int length = 5)
        {
            var buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = CodeChars[Random.Next(CodeChars.Length)];
            }
            var code = new string(buffer);
            return code;
        }
    }
}