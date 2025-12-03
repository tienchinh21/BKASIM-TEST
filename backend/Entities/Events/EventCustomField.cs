using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Enum;
namespace MiniAppGIBA.Entities.Events;
public class EventCustomField : BaseEntity
{
    public string EventId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;   
    public string FieldValue { get; set; } = string.Empty;
    public EEventFieldType? FieldType { get; set; } = null;
    public bool IsRequired { get; set; } = false;
}