namespace MiniAppGIBA.Base.Dependencies.Zalo
{
    public class ZaloTokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public string expires_in { get; set; } = string.Empty;
        public string refresh_token_expires_in { get; set; } = string.Empty;
    }
}
