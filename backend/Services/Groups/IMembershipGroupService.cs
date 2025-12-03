using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Groups;
using MiniAppGIBA.Models.Queries.Groups;
using MiniAppGIBA.Models.Request.Groups;

namespace MiniAppGIBA.Services.Groups
{
    public interface IMembershipGroupService
    {
        /// <summary>
        /// Lấy danh sách đơn xin tham gia hội nhóm với phân trang và filter
        /// </summary>
        Task<PagedResult<MembershipGroupDTO>> GetMembershipGroupsAsync(PendingApprovalQueryParameters query, List<string>? allowedGroupIds = null);

        /// <summary>
        /// Lấy thông tin chi tiết đơn xin tham gia
        /// </summary>
        Task<MembershipGroupDTO?> GetMembershipGroupByIdAsync(string id);
        /// <summary>
        /// Lấy thông tin chi tiết đơn xin tham gia theo group id
        /// </summary>
        Task<List<MembershipGroupDTO>> GetMembershipGroupByGroupIdAsync(string groupId);


        Task<List<Dictionary<string, object>>> GetMembershipByGroupIdAsync(string groupId);
        /// <summary>
        /// Phê duyệt hoặc từ chối đơn xin tham gia
        /// </summary>
        Task<MembershipGroupDTO> ApproveOrRejectAsync(ApproveRejectRequest request);

        /// <summary>
        /// Lấy số lượng đơn chờ duyệt
        /// </summary>
        Task<int> GetPendingCountAsync(List<string>? allowedGroupIds = null);

        /// <summary>
        /// Lấy danh sách MembershipGroup của user theo UserZaloId
        /// </summary>
        Task<List<MembershipGroupDTO>> GetMembershipGroupsByUserZaloIdAsync(string userZaloId);

        /// <summary>
        /// Thêm thành viên vào nhóm với trạng thái đã duyệt
        /// </summary>
        Task<MembershipGroupDTO> AddMemberToGroupAsync(string groupId, string userZaloId);
    }
}

