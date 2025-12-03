namespace MiniAppGIBA.Models.DTOs.Memberships
{
    /// <summary>
    /// DTO để lấy cấu hình profile template
    /// </summary>
    public class GetProfileTemplateDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserZaloId { get; set; } = string.Empty;
        public List<string>? VisibleFields { get; set; }
        public List<string>? HiddenFields { get; set; }
        public string? CustomDescription { get; set; }
        public string? CoverImage { get; set; }
        public string? ThemeColor { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<CustomFieldDto>? CustomFields { get; set; }
    }

    /// <summary>
    /// DTO để cập nhật profile template
    /// </summary>
    public class UpdateProfileTemplateDto
    {
        public List<string>? VisibleFields { get; set; }
        public List<string>? HiddenFields { get; set; }
        public string? CustomDescription { get; set; }
        public string? ThemeColor { get; set; }
        public bool IsPublic { get; set; } = true;
    }

    /// <summary>
    /// DTO response cho profile template
    /// </summary>
    public class ProfileTemplateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GetProfileTemplateDto? Data { get; set; }
    }
}
