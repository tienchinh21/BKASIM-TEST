namespace MiniAppGIBA.Models.DTOs.Memberships
{
    /// <summary>
    /// DTO cho trường tùy chỉnh
    /// </summary>
    public class CustomFieldDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProfileTemplateId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; }
        public string FieldType { get; set; } = "text";
        public int DisplayOrder { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    /// <summary>
    /// DTO để thêm trường tùy chỉnh
    /// </summary>
    public class AddCustomFieldDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; }
        public string FieldType { get; set; } = "text";
        public int DisplayOrder { get; set; } = 0;
        public bool IsVisible { get; set; } = true;
    }

    /// <summary>
    /// DTO để cập nhật trường tùy chỉnh
    /// </summary>
    public class UpdateCustomFieldDto
    {
        public string? FieldName { get; set; }
        public string? FieldValue { get; set; }
        public string? FieldType { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsVisible { get; set; }
    }
}
