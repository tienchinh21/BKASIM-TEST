using MiniAppGIBA.Models.DTOs.Memberships;

namespace MiniAppGIBA.Services.Memberships
{
    /// <summary>
    /// Service quản lý trường tùy chỉnh trong profile template
    /// </summary>
    public interface IProfileCustomFieldService
    {
        /// <summary>
        /// Thêm trường tùy chỉnh vào template
        /// </summary>
        Task<CustomFieldDto> AddCustomFieldAsync(string profileTemplateId, AddCustomFieldDto dto);

        /// <summary>
        /// Lấy danh sách trường tùy chỉnh của template
        /// </summary>
        Task<List<CustomFieldDto>> GetCustomFieldsAsync(string profileTemplateId);
        Task<List<CustomFieldDto>> GetCustomFieldsByUserZaloIdAsync(string userZaloId);

        /// <summary>
        /// Cập nhật trường tùy chỉnh
        /// </summary>
        Task<CustomFieldDto> UpdateCustomFieldAsync(string fieldId, UpdateCustomFieldDto dto);

        /// <summary>
        /// Xóa trường tùy chỉnh
        /// </summary>
        Task<bool> DeleteCustomFieldAsync(string fieldId);

        /// <summary>
        /// Lấy trường tùy chỉnh theo ID
        /// </summary>
        Task<CustomFieldDto?> GetCustomFieldByIdAsync(string fieldId);
    }
}
