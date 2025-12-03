using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Models.DTOs.Memberships;
using System.Text.Json;
namespace MiniAppGIBA.Services.Memberships
{
    public class MembershipProfileService : IMembershipProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IProfileTemplateService? _profileTemplateService;
        private readonly IProfileCustomFieldService? _profileCustomFieldService;
        private readonly IUrl _url;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MembershipProfileService> _logger;

        public MembershipProfileService(
            IUnitOfWork unitOfWork,
            IRepository<Membership> membershipRepository,
            IUrl url,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MembershipProfileService> logger,
            IProfileTemplateService? profileTemplateService = null,
            IProfileCustomFieldService? profileCustomFieldService = null)
        {
            _unitOfWork = unitOfWork;
            _membershipRepository = membershipRepository;
            _url = url;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _profileTemplateService = profileTemplateService;
            _profileCustomFieldService = profileCustomFieldService;
        }

        public async Task<Dictionary<string, object>?> GetProfileBySlugAsync(string slug, string? groupId = null)
        {
            var membership = await _membershipRepository.AsQueryable()
                .Where(m => m.Slug == slug && m.IsDelete != true)
                .FirstOrDefaultAsync();

            if (membership == null)
                return null;

            string? groupName = !string.IsNullOrEmpty(membership.UserZaloId)
                ? await GetGroupNameIfProvidedAsync(membership.UserZaloId, groupId)
                : null;

            var fieldDetails = new List<object>(); // Membership entity không có FieldIds
            var profileTemplate = !string.IsNullOrEmpty(membership.UserZaloId)
                ? await GetProfileTemplateAsync(membership.UserZaloId)
                : null;
            var (visibleFields, hiddenFields) = GetVisibleAndHiddenFields(profileTemplate);

            var basicInfo = BuildBasicInfo(membership, visibleFields, hiddenFields);
            var ratingInfo = BuildRatingInfo(membership, visibleFields, hiddenFields);
            var statusInfo = BuildStatusInfo(membership, visibleFields, hiddenFields);
            var fieldInfo = BuildFieldInfo(membership, fieldDetails, visibleFields, hiddenFields);
            var sortFieldInfo = new Dictionary<string, object>
            {
                ["sortField"] = membership.SortField ?? string.Empty
            };
            var groupInfo = !string.IsNullOrEmpty(membership.UserZaloId)
                ? await BuildGroupInfoAsync(membership.UserZaloId)
                : new List<Dictionary<string, object>>();
            var companyInfo = await BuildCompanyInfoAsync(membership, visibleFields, hiddenFields);
            var filteredProfileTemplate = await BuildFilteredProfileTemplateAsync(profileTemplate, visibleFields, hiddenFields);

            var allUserInfo = new Dictionary<string, object> { ["basicInfo"] = basicInfo };

            if (ratingInfo.Count > 0) allUserInfo["ratingInfo"] = ratingInfo;
            if (statusInfo.Count > 0) allUserInfo["statusInfo"] = statusInfo;
            if (fieldInfo.Count > 0) allUserInfo["fieldInfo"] = fieldInfo;
            if (sortFieldInfo.Count > 0) allUserInfo["sortFieldInfo"] = sortFieldInfo;
            if (groupInfo.Count > 0) allUserInfo["groupInfo"] = groupInfo;
            if (companyInfo.Count > 0) allUserInfo["companyInfo"] = companyInfo;

            allUserInfo["profileTemplate"] = filteredProfileTemplate;

            return allUserInfo;
        }

        #region Helper Methods

        private async Task<string?> GetGroupNameIfProvidedAsync(string userZaloId, string? groupId)
        {
            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(userZaloId))
                return null;

            var membershipGroupRepo = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Groups.MembershipGroup>();
            var membershipGroup = await membershipGroupRepo.AsQueryable()
                .Where(mg => mg.UserZaloId == userZaloId && mg.GroupId == groupId && mg.IsApproved == true)
                .FirstOrDefaultAsync();

            if (membershipGroup == null)
                return null;

            var groupRepo = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Groups.Group>();
            var group = await groupRepo.FindByIdAsync(groupId);
            return group?.GroupName;
        }

        private async Task<List<object>> GetFieldDetailsAsync(string? fieldIds)
        {
            if (string.IsNullOrEmpty(fieldIds))
                return new List<object>();

            var fieldRepo = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Fields.Field>();
            var fieldIdList = fieldIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .ToList();

            var fields = await fieldRepo.AsQueryable()
                .Where(f => fieldIdList.Contains(f.Id))
                .ToListAsync();

            return fields.Select(f => new
            {
                id = f.Id,
                name = f.FieldName,
                isActive = f.IsActive
            }).Cast<object>().ToList();
        }

        private async Task<object?> GetProfileTemplateAsync(string userZaloId)
        {
            if (_profileTemplateService == null)
                return null;

            try
            {
                return await _profileTemplateService.GetTemplateAsync(userZaloId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get profile template for user {UserZaloId}", userZaloId);
                return null;
            }
        }

        private (List<string> visibleFields, List<string> hiddenFields) GetVisibleAndHiddenFields(object? profileTemplate)
        {
            var visibleFields = new List<string>();
            var hiddenFields = new List<string>();

            if (profileTemplate != null)
            {
                var visibleFieldsProperty = profileTemplate.GetType().GetProperty("VisibleFields");
                var hiddenFieldsProperty = profileTemplate.GetType().GetProperty("HiddenFields");

                if (visibleFieldsProperty?.GetValue(profileTemplate) is List<string> vf)
                    visibleFields = vf;

                if (hiddenFieldsProperty?.GetValue(profileTemplate) is List<string> hf)
                    hiddenFields = hf;
            }

            if (visibleFields.Count == 0 && hiddenFields.Count == 0)
            {
                visibleFields = new List<string>
                {
                    "userZaloName", "fullname", "slug", "oldSlugs", "phoneNumber", "zaloAvatar",
                    "averageRating", "totalRatings", "appPosition", "term", "sortField",
                    "companyFullName", "companyBrandName", "taxCode", "businessField", "businessType",
                    "headquartersAddress", "companyWebsite", "companyPhoneNumber", "companyEmail",
                    "legalRepresentative", "legalRepresentativePosition", "companyLogo",
                    "businessRegistrationNumber", "businessRegistrationDate", "businessRegistrationPlace",
                    "coverImage", "customDescription", "themeColor", "isPublic"
                };
            }
            else if (!visibleFields.Contains("themeColor") && !hiddenFields.Contains("themeColor"))
            {
                visibleFields.Add("themeColor");
            }

            return (visibleFields, hiddenFields);
        }

        private Dictionary<string, object> BuildBasicInfo(Membership membership, List<string> visibleFields, List<string> hiddenFields)
        {
            var basicInfo = new Dictionary<string, object>
            {
                ["id"] = membership.Id,
                ["userZaloId"] = membership.UserZaloId ?? string.Empty,
                ["slug"] = membership.Slug ?? string.Empty,
                ["createdDate"] = membership.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                ["updatedDate"] = membership.UpdatedDate.ToString("dd/MM/yyyy HH:mm"),
                ["code"] = membership.Code ?? string.Empty,
                ["fullName"] = membership.Fullname ?? string.Empty
            };

            AddFieldIfVisible(basicInfo, "userZaloName", membership.UserZaloName, visibleFields, hiddenFields);
            AddFieldIfVisible(basicInfo, "fullname", membership.Fullname, visibleFields, hiddenFields);
            AddFieldIfVisible(basicInfo, "phoneNumber", membership.PhoneNumber, visibleFields, hiddenFields);
            AddFieldIfVisible(basicInfo, "zaloAvatar", membership.ZaloAvatar, visibleFields, hiddenFields);
            AddFieldIfVisible(basicInfo, "slug", membership.Slug, visibleFields, hiddenFields);
            AddFieldIfVisible(basicInfo, "oldSlugs", membership.OldSlugs, visibleFields, hiddenFields);
            AddFieldIfVisible(basicInfo, "appPosition", membership.AppPosition, visibleFields, hiddenFields);
            AddFieldIfVisible(basicInfo, "term", membership.Term, visibleFields, hiddenFields);
            AddFieldIfVisible(basicInfo, "sortField", membership.SortField, visibleFields, hiddenFields);

            return basicInfo;
        }

        private Dictionary<string, object> BuildRatingInfo(Membership membership, List<string> visibleFields, List<string> hiddenFields)
        {
            var ratingInfo = new Dictionary<string, object>();
            AddFieldIfVisible(ratingInfo, "averageRating", membership.AverageRating, visibleFields, hiddenFields);
            AddFieldIfVisible(ratingInfo, "totalRatings", membership.TotalRatings, visibleFields, hiddenFields);
            return ratingInfo;
        }

        private Dictionary<string, object> BuildStatusInfo(Membership membership, List<string> visibleFields, List<string> hiddenFields)
        {
            // Membership entity không có các trường approval, trả về empty
            return new Dictionary<string, object>();
        }

        private Dictionary<string, object> BuildFieldInfo(Membership membership, List<object> fieldDetails, List<string> visibleFields, List<string> hiddenFields)
        {
            // Membership entity không có FieldIds, trả về empty
            return new Dictionary<string, object>();
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

        private async Task<Dictionary<string, object>> BuildCompanyInfoAsync(Membership membership, List<string> visibleFields, List<string> hiddenFields)
        {
            var companyInfo = new Dictionary<string, object>();
            AddFieldIfVisible(companyInfo, "companyFullName", membership.CompanyFullName, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "companyBrandName", membership.CompanyBrandName, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "taxCode", membership.TaxCode, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "businessField", membership.BusinessField, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "businessType", membership.BusinessType, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "headquartersAddress", membership.HeadquartersAddress, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "companyWebsite", membership.CompanyWebsite, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "companyPhoneNumber", membership.CompanyPhoneNumber, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "companyEmail", membership.CompanyEmail, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "legalRepresentative", membership.LegalRepresentative, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "legalRepresentativePosition", membership.LegalRepresentativePosition, visibleFields, hiddenFields);
            
            var companyLogoUrl = !string.IsNullOrEmpty(membership.CompanyLogo)
                ? await _url.ToFullUrl(membership.CompanyLogo, _httpContextAccessor.HttpContext)
                : string.Empty;
            AddFieldIfVisible(companyInfo, "companyLogo", companyLogoUrl, visibleFields, hiddenFields);
            
            AddFieldIfVisible(companyInfo, "businessRegistrationNumber", membership.BusinessRegistrationNumber, visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "businessRegistrationDate", membership.BusinessRegistrationDate?.ToString("dd/MM/yyyy"), visibleFields, hiddenFields);
            AddFieldIfVisible(companyInfo, "businessRegistrationPlace", membership.BusinessRegistrationPlace, visibleFields, hiddenFields);
            return companyInfo;
        }

        private async Task<Dictionary<string, object>> BuildFilteredProfileTemplateAsync(object? profileTemplate, List<string> visibleFields, List<string> hiddenFields)
        {
            var filteredProfileTemplate = new Dictionary<string, object>();

            if (profileTemplate == null)
            {
                filteredProfileTemplate["customFields"] = new List<object>();
                return filteredProfileTemplate;
            }

            var templateType = profileTemplate.GetType();
            AddTemplatePropertyIfVisible(filteredProfileTemplate, "visibleFields", "VisibleFields", templateType, profileTemplate, visibleFields, hiddenFields, new List<string>());
            AddTemplatePropertyIfVisible(filteredProfileTemplate, "hiddenFields", "HiddenFields", templateType, profileTemplate, visibleFields, hiddenFields, new List<string>());
            AddTemplatePropertyIfVisible(filteredProfileTemplate, "customDescription", "CustomDescription", templateType, profileTemplate, visibleFields, hiddenFields, "");
            AddTemplatePropertyIfVisible(filteredProfileTemplate, "coverImage", "CoverImage", templateType, profileTemplate, visibleFields, hiddenFields, "");
            AddTemplatePropertyIfVisible(filteredProfileTemplate, "themeColor", "ThemeColor", templateType, profileTemplate, visibleFields, hiddenFields, "");
            AddTemplatePropertyIfVisible(filteredProfileTemplate, "isPublic", "IsPublic", templateType, profileTemplate, visibleFields, hiddenFields, true);

            var customFields = await GetCustomFieldsAsync(profileTemplate);
            filteredProfileTemplate["customFields"] = customFields;

            return filteredProfileTemplate;
        }

        private void AddFieldIfVisible<T>(Dictionary<string, object> dict, string fieldName, T? value, List<string> visibleFields, List<string> hiddenFields)
        {
            if (visibleFields.Contains(fieldName) && !hiddenFields.Contains(fieldName))
            {
                dict[fieldName] = value ?? (object)string.Empty;
            }
        }

        private void AddTemplatePropertyIfVisible(Dictionary<string, object> dict, string key, string propertyName, Type templateType, object profileTemplate, List<string> visibleFields, List<string> hiddenFields, object defaultValue)
        {
            if (visibleFields.Contains(key) && !hiddenFields.Contains(key))
            {
                var property = templateType.GetProperty(propertyName);
                dict[key] = property?.GetValue(profileTemplate) ?? defaultValue;
            }
        }

        private async Task<List<object>> GetCustomFieldsAsync(object profileTemplate)
        {
            var templateType = profileTemplate.GetType();
            var customFieldsProperty = templateType.GetProperty("CustomFields");

            if (customFieldsProperty?.GetValue(profileTemplate) is ICollection<ProfileCustomField> profileCustomFields)
            {
                return profileCustomFields.Select(cf => new
                {
                    id = cf.Id,
                    fieldName = cf.FieldName,
                    fieldValue = cf.FieldValue,
                    fieldType = cf.FieldType,
                    displayOrder = cf.DisplayOrder,
                    isVisible = cf.IsVisible
                }).Cast<object>().ToList();
            }

            if (_profileCustomFieldService != null)
            {
                try
                {
                    var templateId = templateType.GetProperty("Id")?.GetValue(profileTemplate)?.ToString();
                    if (!string.IsNullOrEmpty(templateId))
                    {
                        var customFieldsFromService = await _profileCustomFieldService.GetCustomFieldsAsync(templateId);
                        return customFieldsFromService.Select(cf => new
                        {
                            id = cf.Id,
                            fieldName = cf.FieldName,
                            fieldValue = cf.FieldValue,
                            fieldType = cf.FieldType,
                            displayOrder = cf.DisplayOrder,
                            isVisible = cf.IsVisible
                        }).Cast<object>().ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get custom fields for template");
                }
            }

            return new List<object>();
        }

        #endregion
    }
}

