namespace MiniAppGIBA.Base.Interface;

public interface IUrl
{
    Task<string> ToFullUrl(string relativePath, HttpContext? httpContext = null);
}