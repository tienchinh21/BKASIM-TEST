using System.Security.Claims;
using MiniAppGIBA.Base.Helper;

namespace MiniAppGIBA.Services.Authorization
{
    /// <summary>
    /// Service for handling authorization and permission checks
    /// Centralizes all RBAC logic for ADMIN vs SUPER_ADMIN
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        public List<string>? GetUserGroupIds(ClaimsPrincipal user)
        {
            if (user == null) return new List<string>();

            // SUPER_ADMIN has access to all groups - return null to indicate "no filter"
            if (user.IsInRole("SUPER_ADMIN"))
                return null;

            // ADMIN - return their assigned group IDs from GroupPermission claims
            if (user.IsInRole("ADMIN"))
                return user.GetGroupPermissions();

            // Regular users have no group access
            return new List<string>();
        }

        public bool HasGroupPermission(ClaimsPrincipal user, string groupId)
        {
            if (user == null || string.IsNullOrEmpty(groupId))
                return false;

            return user.HasGroupPermission(groupId);
        }

        public bool CanManageGroups(ClaimsPrincipal user)
        {
            // Only SUPER_ADMIN can create/edit/delete groups
            return user?.IsInRole("SUPER_ADMIN") ?? false;
        }

        public bool CanManageFields(ClaimsPrincipal user)
        {
            // Only SUPER_ADMIN can create/edit/delete fields
            return user?.IsInRole("SUPER_ADMIN") ?? false;
        }

        public bool CanManageEvent(ClaimsPrincipal user, string groupId)
        {
            if (user == null || string.IsNullOrEmpty(groupId))
                return false;

            // SUPER_ADMIN can manage all events
            if (user.IsInRole("SUPER_ADMIN"))
                return true;

            // ADMIN can manage events only for their assigned groups
            if (user.IsInRole("ADMIN"))
                return user.HasGroupPermission(groupId);

            return false;
        }

        public bool CanManageSponsor(ClaimsPrincipal user, string groupId)
        {
            if (user == null || string.IsNullOrEmpty(groupId))
                return false;

            // SUPER_ADMIN can manage all sponsors
            if (user.IsInRole("SUPER_ADMIN"))
                return true;

            // ADMIN can manage sponsors only for their assigned groups
            if (user.IsInRole("ADMIN"))
                return user.HasGroupPermission(groupId);

            return false;
        }

        public bool CanApproveMembership(ClaimsPrincipal user, string groupId)
        {
            if (user == null || string.IsNullOrEmpty(groupId))
                return false;

            // SUPER_ADMIN can approve all membership requests
            if (user.IsInRole("SUPER_ADMIN"))
                return true;

            // ADMIN can approve membership requests only for their assigned groups
            if (user.IsInRole("ADMIN"))
                return user.HasGroupPermission(groupId);

            return false;
        }
    }
}

