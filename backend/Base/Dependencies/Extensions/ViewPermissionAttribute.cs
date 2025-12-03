using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MiniAppGIBA.Base.Dependencies.Extensions
{
    public class ViewPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _viewId;
        private readonly string? _subViewId;

        public ViewPermissionAttribute(string viewId, string? subViewId = null)
        {
            _viewId = viewId;
            _subViewId = subViewId;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            var permissions = user.Claims
                .Where(c => c.Type == "ViewPermission")
                .Select(c => c.Value)
                .ToList();

            bool hasPermission = permissions.Any(p =>
            {
                var parts = p.Split(':');
                if (parts.Length == 2)
                    return parts[0] == _viewId && (_subViewId == null || parts[1] == _subViewId);
                return parts[0] == _viewId && _subViewId == null;
            });

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
