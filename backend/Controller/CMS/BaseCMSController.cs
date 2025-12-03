using MiniAppGIBA.Constants;

namespace MiniAppGIBA.Controller.CMS
{
    public abstract class BaseCMSController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected string? GetCurrentUserId()
        {
            return User?.FindFirst("UserId")?.Value;
        }

        protected string? GetCurrentUserEmail()
        {
            return User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Check if current user is admin (GIBA role only)
        /// </summary>
        protected bool IsAdmin()
        {
            return User?.IsInRole(CTRole.GIBA) ?? false;
        }

        /// <summary>
        /// Check if current user is super admin (GIBA role)
        /// </summary>
        protected bool IsSuperAdmin()
        {
            return User?.IsInRole(CTRole.GIBA) ?? false;
        }

        /// <summary>
        /// Get list of GroupIds that current ADMIN user has permission to access
        /// GIBA has access to all groups, returns empty list to indicate "all"
        /// </summary>
        protected List<string> GetUserGroupIds()
        {
            if (User == null) return new List<string>();

            // GIBA has access to all groups, return empty list to indicate "all"
            if (IsAdmin()) return new List<string>();

            return new List<string>();
        }

        /// <summary>
        /// Check if current user has permission to access a specific group
        /// GIBA always returns true (full access to all groups)
        /// </summary>
        protected bool HasGroupPermission(string groupId)
        {
            if (User == null || string.IsNullOrEmpty(groupId)) return false;

            // GIBA has full access to all groups
            return IsAdmin();
        }

        /// <summary>
        /// Get current user's role (only GIBA supported)
        /// </summary>
        protected string? GetCurrentUserRole()
        {
            if (User == null) return null;

            if (User.IsInRole(CTRole.GIBA)) return CTRole.GIBA;

            return null;
        }

        /// <summary>
        /// Get user's group IDs or null if GIBA (indicating access to all groups)
        /// GIBA always returns null - no filtering needed
        /// </summary>
        protected List<string>? GetUserGroupIdsOrNull()
        {
            if (User == null) return new List<string>();

            // GIBA has full access - return null to indicate "no filter needed"
            if (IsAdmin()) return null;

            // Non-admin users: return empty list
            return new List<string>();
        }

        /// <summary>
        /// Get group type filter based on user role
        /// Always returns null - GIBA has full access, no type filtering needed
        /// </summary>
        [Obsolete("GetGroupTypeFilter is deprecated. GIBA has full access to all groups without type filtering.")]
        protected string? GetGroupTypeFilter()
        {
            // GIBA has full access - no type filter needed
            return null;
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }
    }
}

