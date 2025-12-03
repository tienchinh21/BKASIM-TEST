using MiniAppGIBA.Entities.Memberships;

namespace MiniAppGIBA.Entities.Commons
{
    public class Ref : BaseEntity
    {
        public string? RefFrom { get; set; }
        public string? Content { get; set; }
        public string? RefTo { get; set; }
        public byte Status { get; set; }
        public double Value { get; set; }
        /// GroupId của người nhận (RefTo) - chỉ dùng cho Type 0, bắt buộc
        public string? RefToGroupId { get; set; }
        /// GroupId của người được share (ReferredMemberId) - dùng cho cả Type 0 và Type 1 khi ShareType = "member"
        public string? ReferredMemberGroupId { get; set; }

        /// Type: 0 - gửi cho thành viên; 1 - gửi cho bên ngoài
        public byte Type { get; set; } = 0;
        public string? ReferredMemberId { get; set; }

        // ===== FIELDS CHO TYPE=0 (Internal - Share referral TO member) =====
        /// Thông tin referral (người được giới thiệu)
        public string? ReferralName { get; set; }
        public string? ReferralPhone { get; set; }
        public string? ReferralEmail { get; set; }
        public string? ReferralAddress { get; set; }

        // ===== FIELDS CHO TYPE=1 (External - Share member TO external) =====
        /// Thông tin recipient external (người nhận bên ngoài)
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }  // UID identifier cho notification

        //  Rating & Feedback fields
        public byte? Rating { get; set; }           // 1-5 stars (nullable)
        public string? Feedback { get; set; }       // Comment (nullable, max 500 chars)
        public DateTime? RatingDate { get; set; }   // When rating was submitted

        // Navigation properties
        public virtual Membership? FromMember { get; set; }
        public virtual Membership? ToMember { get; set; }
    }
}
