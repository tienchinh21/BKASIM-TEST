using Newtonsoft.Json;

namespace MiniAppGIBA.Base.Dependencies.Zalo
{
    public class ValidationUserZalo
    {
        public static async Task<bool> CheckUserUserZaloIdByOA(string? accessToken, string? userUserZaloIdByOA)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(userUserZaloIdByOA)) return false;

            var httpClient = new HttpClient();
            var url = $"https://openapi.zalo.me/v3.0/oa/user/detail?data={{\"user_id\":\"{userUserZaloIdByOA}\"}}";
            httpClient.DefaultRequestHeaders.Add("access_token", accessToken);
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                if (responseObject?.error != 0)
                {
                    return false;
                }
                return true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}. Connection time out");
                return false;
            }
            catch (Exception ex)
            {
                // Bắt lỗi chung
                Console.WriteLine($"General error: {ex.Message}");
                return false;
            }
        }
    }
}
