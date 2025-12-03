using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Groups;
using MiniAppGIBA.Models.Queries.Groups;
using MiniAppGIBA.Models.Request.Groups;

namespace MiniAppGIBA.Service.Groups
{
    public interface IGroupService
    {
        /// <summary>
        /// Lấy danh sách hội nhóm với phân trang và filter
        /// </summary>
        Task<PagedResult<GroupDTO>> GetGroupsAsync(GroupQueryParameters query, string? userZaloId = null, List<string>? allowedGroupIds = null, string? groupType = null);

        /// <summary>
        /// Lấy tất cả hội nhóm với phân trang - hiển thị tất cả groups, isJoined dựa trên việc user đã tham gia hay chưa
        /// </summary>
        Task<PagedResult<GroupDTO>> GetAllGroupsAsync(GroupQueryParameters query, string? userZaloId = null, List<string>? allowedGroupIds = null);

        /// <summary>
        /// Lấy thông tin chi tiết hội nhóm
        /// </summary>
        Task<GroupDetailDTO?> GetGroupByIdAsync(string id);

        /// <summary>
        /// Tạo hội nhóm mới
        /// </summary>
        Task<GroupDTO> CreateGroupAsync(CreateGroupRequest request);

        /// <summary>
        /// Cập nhật thông tin hội nhóm
        /// </summary>
        Task<GroupDTO> UpdateGroupAsync(string id, UpdateGroupRequest request);

        /// <summary>
        /// Xóa hội nhóm
        /// </summary>
        Task<bool> DeleteGroupAsync(string id);

        /// <summary>
        /// Thay đổi trạng thái hội nhóm
        /// </summary>
        Task<bool> ToggleGroupStatusAsync(string id);

        /// <summary>
        /// Kiểm tra tên hội nhóm có tồn tại không
        /// </summary>
        Task<bool> IsGroupNameExistsAsync(string name, string? excludeId = null);

        /// <summary>
        /// Lấy thống kê hội nhóm
        /// </summary>
        Task<GroupStatisticsDTO> GetGroupStatisticsAsync();

        /// <summary>
        /// Lấy danh sách hội nhóm đang hoạt động
        /// </summary>
        Task<List<GroupDTO>> GetActiveGroupsAsync(List<string>? allowedGroupIds = null, string? groupType = null);

        /// <summary>
        /// [MINI APP] Lấy thông tin hội nhóm cho người ngoài (chưa tham gia) - chỉ hiển thị sự kiện công khai
        /// </summary>
        Task<object?> GetGroupPublicAsync(string id, string? userZaloId = null);

        /// <summary>
        /// [MINI APP] Lấy thông tin hội nhóm cho thành viên - hiển thị đầy đủ sự kiện và danh sách thành viên
        /// </summary>
        Task<object?> GetGroupMemberAsync(string id, string userZaloId);

        /// <summary>
        /// [MINI APP] Lấy chi tiết sự kiện - hiển thị đầy đủ thông tin sự kiện, quà, nhà tài trợ
        /// </summary>
        Task<object?> GetEventDetailAsync(string eventId, string? userZaloId = null);

        /// <summary>
        /// [MINI APP] Lấy danh sách tất cả hội nhóm với phân trang - hiển thị tất cả groups, isJoined dựa trên việc user đã tham gia hay chưa
        /// </summary>
        Task<PagedResult<GroupDTO>> GuestGetPage(string? type, bool? isActive, int page, int pageSize);

        /// <summary>
        /// [MINI APP] Lấy tất cả hội nhóm mà user đã tham gia (IsApproved = true)
        /// </summary>
        Task<PagedResult<GroupDTO>> GetAllGroupsForUserAsync(GroupQueryParameters query, string userZaloId, List<string> joinedGroupIds);
    }
}