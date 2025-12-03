using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MiniAppGIBA.Controller.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZaloHelperApiController : ControllerBase
    {
        private readonly ILogger<ZaloHelperApiController> _logger;

        public ZaloHelperApiController(ILogger<ZaloHelperApiController> logger)
        {
            _logger = logger;
        }

        [HttpPost("GetPhoneNumber")]
        public async Task<IActionResult> GetPhoneNumber(DataForm dataPost)
        {
            try
            {
                const string endpoint = "https://graph.zalo.me/v2.0/me/info";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("access_token", dataPost.accessToken);
                    client.DefaultRequestHeaders.Add("code", dataPost.tokenNumber);
                    client.DefaultRequestHeaders.Add("secret_key", dataPost.secretKey);

                    HttpResponseMessage response = await client.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return Ok(JsonConvert.DeserializeObject<ResponseZaloDTO>(responseBody));
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode);
                    }
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi server!");
            }
        }

    }

    public class DataForm
    {
        public required string accessToken { get; set; }

        public required string tokenNumber { get; set; }

        public required string secretKey { get; set; }

        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Keyword { get; set; }
    }

    public class ResponseZaloDTO
    {
        public Dictionary<string, dynamic>? data { get; set; }
        public int error { get; set; }
        public string? message { get; set; }
    }
}
