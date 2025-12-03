using System.Security.Claims;
using MiniAppGIBA.Constants;
namespace MiniAppGIBA.Base.Dependencies.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static List<string> GetEmployeeBranches(this ClaimsPrincipal user)
        {
            return user.Claims
                .Where(c => c.Type == "BranchPermission")
                .Select(c => c.Value)
                .ToList();
        }



        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole("ADMIN");
        }

        public static bool HasBranchPermission(this ClaimsPrincipal user, string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId)) return false;

            if (user.IsAdmin()) return true;

            return user.GetEmployeeBranches().Contains(branchId);
        }

        public static List<string> GetRoles(this ClaimsPrincipal user)
        {
            return user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }
        public static List<string> GetRoleId(this ClaimsPrincipal user)
        {
            return user.Claims
                .Where(c => c.Type == "RoleId")
                .Select(c => c.Value)
                .ToList();
        }

        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user.IsInRole(role);
        }

        public static bool IsEditable(this ClaimsPrincipal user)
        {
            if (user.IsInRole(CTRole.GIBA)) return true;

            // Fallback: kiểm tra claim "Editable" (nếu có)
            var editableClaim = user.FindFirst("Editable")?.Value;
            return bool.TryParse(editableClaim, out var result) && result;
        }

        public static bool HasAnyBranch(this ClaimsPrincipal user, params string[] branchIds)
        {
            var userBranches = user.GetEmployeeBranches();
            return branchIds.Any(id => userBranches.Contains(id)) || user.IsAdmin();
        }
    }
}
