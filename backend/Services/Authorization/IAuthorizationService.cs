using System.Security.Claims;

namespace MiniAppGIBA.Services.Authorization
{
    /// <summary>
    /// Service for handling authorization and permission checks
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Get list of group IDs that the user has permission to access
        /// Returns null for SUPER_ADMIN (access to all groups)
        /// Returns list of assigned group IDs for ADMIN
        /// </summary>
        List<string>? GetUserGroupIds(ClaimsPrincipal user);

        /// <summary>
        /// Check if user has permission to access a specific group
        /// </summary>
        bool HasGroupPermission(ClaimsPrincipal user, string groupId);

        /// <summary>
        /// Check if user can create/edit/delete groups
        /// Only SUPER_ADMIN can perform these operations
        /// </summary>
        bool CanManageGroups(ClaimsPrincipal user);

        /// <summary>
        /// Check if user can create/edit/delete fields
        /// Only SUPER_ADMIN can perform these operations
        /// </summary>
        bool CanManageFields(ClaimsPrincipal user);

        /// <summary>
        /// Check if user can create/edit/delete events for a specific group
        /// SUPER_ADMIN: always true
        /// ADMIN: only if they have permission for the group
        /// </summary>
        bool CanManageEvent(ClaimsPrincipal user, string groupId);

        /// <summary>
        /// Check if user can create/edit/delete sponsors for a specific group
        /// SUPER_ADMIN: always true
        /// ADMIN: only if they have permission for the group
        /// </summary>
        bool CanManageSponsor(ClaimsPrincipal user, string groupId);

        /// <summary>
        /// Check if user can approve membership requests for a specific group
        /// SUPER_ADMIN: always true
        /// ADMIN: only if they have permission for the group
        /// </summary>
        bool CanApproveMembership(ClaimsPrincipal user, string groupId);
    }
}

