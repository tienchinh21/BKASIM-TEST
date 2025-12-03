namespace MiniAppGIBA.Models.Payload
{
    public class MembershipPayload
    {
        public string? UserZaloId { get; set; }
        public string? UserZaloName { get; set; }
        public string? UserZaloIdByOA { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class GroupPayload : MembershipPayload
    {
        public string? Reason { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public bool? IsApproved { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? GroupName { get; set; }
    }

    public class EventPayload : MembershipPayload
    {
        public string? Note { get; set; }
        public int GuestNumber { get; set; } = 0;
        public byte Status { get; set; } = 1;
        public string? RejectReason { get; set; }
        public string? CancelReason { get; set; }
        public string? EventTitle { get; set; }
    }
}
