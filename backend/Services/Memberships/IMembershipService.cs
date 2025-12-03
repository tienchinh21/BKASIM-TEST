using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Memberships;
using MiniAppGIBA.Models.Queries.Memberships;
using MiniAppGIBA.Models.Request.Memberships;

namespace MiniAppGIBA.Services.Memberships
{
    public interface IMembershipService
    {
        Task<PagedResult<MembershipDTO>> GetMembershipsAsync(MembershipQueryParameters queryParameters, List<string>? allowedGroupIds = null);
        Task<MembershipDTO?> GetMembershipByIdAsync(string id);
        Task<MembershipDTO?> GetMembershipByPhoneAsync(string phone);
        Task<MembershipDTO?> GetMembershipByUserZaloIdAsync(string userZaloId);
        Task<MembershipDTO> CreateMembershipAsync(CreateMembershipRequest request);
        Task<MembershipDTO> UpdateMembershipAsync(string id, UpdateMembershipRequest request);
        Task<bool> DeleteMembershipAsync(string id);
        Task<List<MembershipDTO>> GetActiveMembershipsAsync(List<string>? allowedGroupIds = null);
        Task<List<MembershipDTO>> SearchMembersForGroupAsync(string searchTerm);
        Task<List<MembershipDTO>> GetMembershipsByRoleAsync(string roleName);
        Task<string> GetRoleNameAsync(string roleId);
        Task<MembershipDTO> CreateAdminMembershipAsync(string fullName, string username, string password, string phoneNumber, string roleName);
        Task<bool> ResetPasswordAsync(string userId, string newPassword);
    }
}
