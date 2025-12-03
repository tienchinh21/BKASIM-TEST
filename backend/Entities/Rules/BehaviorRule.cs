using MiniAppGIBA.Entities.Commons;
namespace  MiniAppGIBA.Entities.Rules;

public class BehaviorRule : BaseEntity
{
    public int? SortOrder { get; set; }
    public string? Content { get; set; }
    public string? GroupId { get; set; }
    public string? Title { get; set; }
    public bool IsActive { get; set; } = true;
     /// Content type: "TEXT" or "FILE"
    public string ContentType { get; set; } = string.Empty;
    /// Type of behavior rule: "APP" or "GROUP"
    
    public string Type { get; set; } = string.Empty;
}