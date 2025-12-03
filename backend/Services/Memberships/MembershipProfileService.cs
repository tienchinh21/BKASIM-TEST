using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.DTOs.Memberships;

namespace MiniAppGIBA.Services.Memberships
{
    public class MembershipProfileService : IMembershipProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IUrl _url;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MembershipProfileService> _logger;

        public MembershipProfileService(
            IUnitOfWork unitOfWork,
            IRepository<Membership> membershipRepository,
            IUrl url,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MembershipProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _membershipRepository = membershipRepository;
            _url = url;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>?> GetProfileBySlugAsync(string slug, string? groupId = null)
        {
            var membership = await _membershipRepository.AsQueryable()
                .Where(m => m.Slug == slug && m.IsDelete != true)
                .FirstOrDefaultAsync();

            if (membership == null)
                return null;

            var basicInfo = new Dictionary<string, object>
            {
                ["id"] = membership.Id,
                ["userZaloId"] = membership.UserZaloId ?? string.Empty,
                ["userZaloName"] = membership.UserZaloName ?? string.Empty,
                ["fullname"] = membership.Fullname,
                ["slug"] = membership.Slug,
                ["phoneNumber"] = membership.PhoneNumber,
                ["zaloAvatar"] = membership.ZaloAvatar ?? string.Empty,
                ["createdDate"] = membership.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                ["updatedDate"] = membership.UpdatedDate.ToString("dd/MM/yyyy HH:mm")
            };

            var groupInfo = !string.IsNullOrEmpty(membership.UserZaloId)
                ? await BuildGroupInfoAsync(membership.UserZaloId)
                : new List<Dictionary<string, object>>();

            return new Dictionary<string, object>
            {
                ["basicInfo"] = basicInfo,
                ["groupInfo"] = groupInfo
            };
        }

        private async Task<List<Dictionary<string, object>>> BuildGroupInfoAsync(string userZaloId)
        {
            var membershipGroupRepo = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Groups.MembershipGroup>();
            var groups = await membershipGroupRepo.AsQueryable()
                .Include(mg => mg.Group)
                .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                .OrderBy(mg => mg.SortOrder ?? int.MaxValue)
                .ThenBy(mg => mg.CreatedDate)
                .ToListAsync();

            return groups
                .Where(g => g.Group != null)
                .Select(g => new Dictionary<string, object>
                {
                    ["groupId"] = g.GroupId,
                    ["groupName"] = g.Group!.GroupName,
                    ["groupPosition"] = g.GroupPosition ?? string.Empty,
                    ["sortOrder"] = g.SortOrder ?? 0
                })
                .ToList();
        }
    }
}
