using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Request.Memberships;

namespace MiniAppGIBA.Services.Memberships
{
    public interface IMembershipApprovalService
    {
        /// <summary>
        /// Lấy danh sách thành viên chờ phê duyệt (phân trang)
        /// </summary>
        Task<PagedResult<Membership>> GetPendingMembershipsAsync(int page = 1, int pageSize = 20, string? keyword = null);

        /// <summary>
        /// Phê duyệt thành viên
        /// </summary>
        Task<int> ApproveMembershipAsync(string membershipId, string approvedBy);

        /// <summary>
        /// Từ chối thành viên
        /// </summary>
        Task<int> RejectMembershipAsync(string membershipId, string reason, string rejectedBy);

        /// <summary>
        /// Lấy thông tin chi tiết thành viên
        /// </summary>
        Task<Membership?> GetMembershipDetailAsync(string membershipId);

        /// <summary>
        /// Lấy danh sách thành viên đã phê duyệt (phân trang)
        /// </summary>
        Task<PagedResult<Membership>> GetApprovedMembershipsAsync(int page = 1, int pageSize = 20, string? keyword = null);

        /// <summary>
        /// Lấy danh sách thành viên bị từ chối (phân trang)
        /// </summary>
        Task<PagedResult<Membership>> GetRejectedMembershipsAsync(int page = 1, int pageSize = 20, string? keyword = null);

        /// <summary>
        /// Lấy tất cả thành viên (phân trang)
        /// </summary>
        Task<PagedResult<Membership>> GetAllMembershipsAsync(int page = 1, int pageSize = 20, string? keyword = null);
    }
}

