using System.Text.Json;

namespace MiniAppGIBA.Base.Dependencies.Zalo
{
    public class ZaloAuthentication
    {
        public static async Task<ZaloTokenResponse?> GetAccessTokenFromRefreshToken(string refreshToken, string appId, string secretKey)
        {
            try
            {
                string apilink = "https://oauth.zaloapp.com/v4/oa/access_token";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("secret_key", secretKey);
                var dict = new Dictionary<string, string>();
                dict.Add("refresh_token", refreshToken);
                dict.Add("app_id", appId);
                dict.Add("grant_type", "refresh_token");
                var req = new HttpRequestMessage(HttpMethod.Post, apilink) { Content = new FormUrlEncodedContent(dict) };
                var res = await client.SendAsync(req);
                string responseBody = await res.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<ZaloTokenResponse>(responseBody);

                // Ghi log ra file
                await LogResponseToFileAsync(responseBody);
                return tokenResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await LogErrorToFileAsync(ex.Message);
                return null;
            }
        }

        private static async Task LogResponseToFileAsync(string response)
        {
            string logFilePath = "zalo_api.log";
            string logContent = $"[{DateTime.Now}] Zalo API Response: {response}{Environment.NewLine}";

            await File.AppendAllTextAsync(logFilePath, logContent);
        }

        private static async Task LogErrorToFileAsync(string error)
        {
            string logFilePath = "zalo_api.log";
            string logContent = $"[{DateTime.Now}] Zalo API Error: {error}{Environment.NewLine}";

            await File.AppendAllTextAsync(logFilePath, logContent);
        }
    }
}
