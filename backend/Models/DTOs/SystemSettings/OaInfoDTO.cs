namespace MiniAppGIBA.Models.DTOs.SystemSettings
{
    public class OaInfoDTO
    {
        public string? AppId { get; set; }

        public string? SecretKey { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
