namespace MiniAppGIBA.Models.DTOs.Events
{
    public class EventGuestDTO
    {
        public string Id { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty; // Thêm tên sự kiện
        public string UserZaloId { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int GuestNumber { get; set; }
        public byte Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? RejectReason { get; set; }
        public string? CancelReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<GuestListDTO> GuestLists { get; set; } = new List<GuestListDTO>();

        // Thông tin người tạo đơn
        public string? MemberName { get; set; }
        public string? MemberPhone { get; set; }
        public string? MemberEmail { get; set; }
        public string? MemberCompany { get; set; }
        public string? MemberPosition { get; set; }
    }
}

