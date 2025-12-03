namespace MiniAppGIBA.Base.Interface
{
    public interface IRequestQuery
    {
        int Page { get; set; }
        int PageSize { get; set; }
        string? Keyword { get; set; }
        string? OrderBy { get; set; }
        string? OrderType { get; set; }
    }
}