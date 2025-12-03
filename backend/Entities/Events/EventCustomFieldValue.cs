using MiniAppGIBA.Entities.Commons;

namespace MiniAppGIBA.Entities.Events
{
    /// Entity lưu giá trị custom fields mà người dùng nhập khi đăng ký sự kiện
    /// Có thể liên kết với EventRegistration (đăng ký đơn lẻ) hoặc GuestList (khách trong nhóm)
    public class EventCustomFieldValue : BaseEntity
    {
        /// Reference đến EventCustomField (định nghĩa trường)
        public string EventCustomFieldId { get; set; } = string.Empty;

        /// Reference đến EventRegistration nếu là đăng ký đơn lẻ
        public string? EventRegistrationId { get; set; }

        /// Reference đến GuestList nếu là khách trong nhóm
        public string? GuestListId { get; set; }

        /// Tên trường (lưu lại để hiển thị khi EventCustomField bị xóa)
        public string FieldName { get; set; } = string.Empty;

        /// Giá trị mà người dùng nhập vào custom field
        public string FieldValue { get; set; } = string.Empty;
    }
}

