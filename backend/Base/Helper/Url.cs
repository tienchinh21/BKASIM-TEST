using Microsoft.AspNetCore.Http;
using MiniAppGIBA.Base.Interface;
namespace MiniAppGIBA.Base.Helper;

public class Url : IUrl
{   
    public async Task<string> ToFullUrl(string relativePath, HttpContext? httpContext = null)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            // If already full URL, return as is
            if (relativePath.StartsWith("http://") || relativePath.StartsWith("https://"))
                return relativePath;

            // Ensure path starts with /
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;

            // Build full URL
            var request = httpContext?.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return baseUrl + relativePath;
        }
}