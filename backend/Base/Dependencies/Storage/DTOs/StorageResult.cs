namespace MiniAppGIBA.Base.Dependencies.Storage.DTOs
{
    public class StorageResult
    {
        public string? FileName { get; set; } = string.Empty;      // Tên file sinh ra
        public string? PublicUrl { get; set; } = string.Empty;     // (Tuỳ chọn) public URL, nếu service đã biết host
        public string? RelativePath { get; set; } = string.Empty;  // Đường dẫn relative so với webroot, dùng để compose URL
    }
}
