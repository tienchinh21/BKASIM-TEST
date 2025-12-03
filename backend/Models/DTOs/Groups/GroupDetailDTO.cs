using MiniAppGIBA.Models.DTOs.Groups;

namespace MiniAppGIBA.Models.DTOs.Groups
{
    public class GroupDetailDTO : GroupDTO
    {
        public List<GroupMemberDTO> Members { get; set; } = new List<GroupMemberDTO>();
    }

    public class GroupMemberDTO
    {
        public string MembershipId { get; set; } = string.Empty;
        public string MembershipName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool IsApproved { get; set; }
        
        /// <summary>
        /// Whether custom fields have been submitted for this membership group
        /// </summary>
        public bool HasCustomFieldsSubmitted { get; set; }
    }
}
