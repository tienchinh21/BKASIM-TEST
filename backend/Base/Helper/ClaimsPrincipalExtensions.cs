using System.Security.Claims;

namespace MiniAppGIBA.Base.Helper
{
    /// <summary>
    /// Extension methods for ClaimsPrincipal to work with GroupPermission claims
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Get all GroupPermission claims (GroupIds) for the current user
        /// </summary>
        public static List<string> GetGroupPermissions(this ClaimsPrincipal principal)
        {
            if (principal == null)
                return new List<string>();

            return principal.FindAll("GroupPermission")
                .Select(c => c.Value)
                .ToList();
        }

        /// <summary>
        /// Check if user has permission to access a specific group
        /// SUPER_ADMIN always has access to all groups
        /// ADMIN only has access to groups they are assigned to
        /// </summary>
        public static bool HasGroupPermission(this ClaimsPrincipal principal, string groupId)
        {
            if (principal == null || string.IsNullOrEmpty(groupId))
                return false;

            // SUPER_ADMIN has access to all groups
            if (principal.IsInRole("SUPER_ADMIN"))
                return true;

            // ADMIN needs to have the specific GroupPermission claim
            if (principal.IsInRole("ADMIN"))
            {
                var groupPermissions = principal.GetGroupPermissions();
                return groupPermissions.Contains(groupId);
            }

            return false;
        }

        /// <summary>
        /// Check if user has permission to access any of the specified groups
        /// </summary>
        public static bool HasAnyGroupPermission(this ClaimsPrincipal principal, List<string> groupIds)
        {
            if (principal == null || groupIds == null || !groupIds.Any())
                return false;

            // SUPER_ADMIN has access to all groups
            if (principal.IsInRole("SUPER_ADMIN"))
                return true;

            // ADMIN needs to have at least one matching GroupPermission claim
            if (principal.IsInRole("ADMIN"))
            {
                var userGroupPermissions = principal.GetGroupPermissions();
                return groupIds.Any(gid => userGroupPermissions.Contains(gid));
            }

            return false;
        }

        /// <summary>
        /// Check if user has permission to access all of the specified groups
        /// </summary>
        public static bool HasAllGroupPermissions(this ClaimsPrincipal principal, List<string> groupIds)
        {
            if (principal == null || groupIds == null || !groupIds.Any())
                return false;

            // SUPER_ADMIN has access to all groups
            if (principal.IsInRole("SUPER_ADMIN"))
                return true;

            // ADMIN needs to have all matching GroupPermission claims
            if (principal.IsInRole("ADMIN"))
            {
                var userGroupPermissions = principal.GetGroupPermissions();
                return groupIds.All(gid => userGroupPermissions.Contains(gid));
            }

            return false;
        }

        /// <summary>
        /// Get UserId from claims
        /// </summary>
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst("UserId")?.Value;
        }

        /// <summary>
        /// Get UserZaloId from claims (for MiniApp users)
        /// </summary>
        public static string? GetUserZaloId(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst("UserZaloId")?.Value;
        }

        /// <summary>
        /// Get Email from claims
        /// </summary>
        public static string? GetEmail(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Get FullName from claims
        /// </summary>
        public static string? GetFullName(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Check if user is SUPER_ADMIN
        /// </summary>
        public static bool IsSuperAdmin(this ClaimsPrincipal principal)
        {
            return principal?.IsInRole("SUPER_ADMIN") ?? false;
        }

        /// <summary>
        /// Check if user is ADMIN (not SUPER_ADMIN)
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal?.IsInRole("ADMIN") ?? false;
        }

        /// <summary>
        /// Check if user is either SUPER_ADMIN or ADMIN
        /// </summary>
        public static bool IsAdminOrSuperAdmin(this ClaimsPrincipal principal)
        {
            return principal?.IsInRole("SUPER_ADMIN") == true || principal?.IsInRole("ADMIN") == true;
        }
        
    }
}

