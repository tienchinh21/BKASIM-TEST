using MiniAppGIBA.Entities.Rules;
using MiniAppGIBA.Models.Request.Rules;

namespace MiniAppGIBA.Services.Rules
{
    public interface IBehaviorRuleService
    {
        Task<BehaviorRule> CreateAsync(CreateBehaviorRuleRequest request, IWebHostEnvironment env, HttpContext httpContext);
        Task<(BehaviorRule? Rule, int TotalPages)> GetByPageAsync(string type, string? groupId, int page);
        Task<BehaviorRule?> GetByIdAsync(string id);
        Task<BehaviorRule> UpdateAsync(UpdateBehaviorRuleRequest request, IWebHostEnvironment env, HttpContext httpContext);
        Task<bool> DeleteAsync(string id, IWebHostEnvironment env);
    }
}