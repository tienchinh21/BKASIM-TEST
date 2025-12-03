using MiniAppGIBA.Models.DTOs.Memberships;

namespace MiniAppGIBA.Services.Memberships
{
    public interface IProfileTemplateService
    {
        /// <summary>
        /// Lấy template profile của người dùng
        /// </summary>
        Task<GetProfileTemplateDto?> GetTemplateAsync(string userZaloId);

        /// <summary>
        /// Tạo hoặc cập nhật template profile
        /// </summary>
        Task<GetProfileTemplateDto> CreateOrUpdateTemplateAsync(string userZaloId, UpdateProfileTemplateDto dto);

        /// <summary>
        /// Lấy danh sách trường được phép hiển thị
        /// </summary>
        Task<List<string>> GetVisibleFieldsAsync(string userZaloId);

        /// <summary>
        /// Lấy danh sách trường bị ẩn
        /// </summary>
        Task<List<string>> GetHiddenFieldsAsync(string userZaloId);

        /// <summary>
        /// Cập nhật ảnh bìa profile
        /// </summary>
        Task<string> UpdateCoverImageAsync(string userZaloId, string imagePath);

        /// <summary>
        /// Xóa template profile
        /// </summary>
        Task<bool> DeleteTemplateAsync(string userZaloId);
    }
}
