namespace MiniAppGIBA.Models.DTOs.Refs
{
    public class RefDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? RefFrom { get; set; } // userZaloId người gửi
        public string? RefTo { get; set; } // userZaloId người nhận (Type 0) hoặc null (Type 1)
        public string? Content { get; set; }
        public byte Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public double Value { get; set; }
        public string? RefToGroupId { get; set; } // GroupId của người nhận (Type 0)
        public string? RefToGroupName { get; set; } // Tên nhóm của người nhận
        public string? ReferredMemberGroupId { get; set; } // GroupId của người được share
        public string? ReferredMemberGroupName { get; set; } // Tên nhóm của người được share
        
        /// <summary>
        /// Type: 0 - gửi cho thành viên; 1 - gửi cho bên ngoài
        /// </summary>
        public byte Type { get; set; }
        public string TypeText { get; set; } = string.Empty;

        /// <summary>
        /// ShareType: "own" - profile bản thân, "member" - profile thành viên, "external" - soạn text
        /// </summary>
        public string? ShareType { get; set; }
        
        public string? ReferredMemberId { get; set; } // userZaloId nếu là member, null nếu là người ngoài
        public string? ReferralName { get; set; }
        public string? ReferralPhone { get; set; }
        public string? ReferralEmail { get; set; }
        public string? ReferralAddress { get; set; }
        public string? RecipientName { get; set; } // Tên người nhận bên ngoài (Type 1)
        public string? RecipientPhone { get; set; } // SĐT người nhận bên ngoài (Type 1)
        
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // ✨ Rating & Feedback fields
        public byte? Rating { get; set; }
        public string? Feedback { get; set; }
        public DateTime? RatingDate { get; set; }

        // Thông tin profile người gửi (từ RefFrom)
        public string? FromMemberName { get; set; }
        public string? FromMemberCompany { get; set; }
        public string? FromMemberPosition { get; set; }
        public string? FromMemberPhone { get; set; }
        public string? FromMemberEmail { get; set; }
        public string? FromMemberAvatar { get; set; }
        public string? FromMemberSlug { get; set; }

        // Thông tin profile người nhận (từ RefTo - chỉ Type 0)
        public string? ToMemberName { get; set; }
        public string? ToMemberCompany { get; set; }
        public string? ToMemberPosition { get; set; }
        public string? ToMemberPhone { get; set; }
        public string? ToMemberEmail { get; set; }
        public string? ToMemberAvatar { get; set; }
        public string? ToMemberSlug { get; set; }

        // Thông tin profile người được share (từ ReferredMemberId - nếu là member)
        public string? ReferredMemberName { get; set; }
        public string? ReferredMemberCompany { get; set; }
        public string? ReferredMemberPosition { get; set; }
        public string? ReferredMemberPhone { get; set; }
        public string? ReferredMemberEmail { get; set; }
        public string? ReferredMemberAvatar { get; set; }
        public string? ReferredMemberSlug { get; set; }

        // 🆕 Thông tin nhóm
        public string? FromMemberGroupIds { get; set; }  // Comma-separated group IDs
        public string? FromMemberGroupNames { get; set; } // Comma-separated group names
        public string? ToMemberGroupIds { get; set; }    // Comma-separated group IDs
        public string? ToMemberGroupNames { get; set; }  // Comma-separated group names
    }
}
