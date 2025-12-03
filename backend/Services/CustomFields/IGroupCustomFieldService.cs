using MiniAppGIBA.Models.DTOs.CustomFields;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service interface for managing group custom fields during registration
    /// </summary>
    public interface IGroupCustomFieldService
    {
        /// <summary>
        /// Gets the custom field form structure for a group
        /// </summary>
        Task<GroupCustomFieldFormDTO> GetGroupCustomFieldFormAsync(string groupId);

        /// <summary>
        /// Registers user to a group and submits custom field values in one transaction
        /// </summary>
        Task<SubmitCustomFieldValuesResult> RegisterAndSubmitValuesAsync(
            string groupId,
            string userZaloId,
            string? reason,
            string? company,
            string? position,
            Dictionary<string, string> values);

        /// <summary>
        /// Submits custom field values for an existing membership group
        /// </summary>
        Task<SubmitCustomFieldValuesResult> SubmitValuesAsync(string membershipGroupId, Dictionary<string, string> values);

        /// <summary>
        /// Updates custom field values for a membership group
        /// </summary>
        Task<SubmitCustomFieldValuesResult> UpdateValuesAsync(string membershipGroupId, Dictionary<string, string> values);

        /// <summary>
        /// Gets submitted custom field values for a membership group
        /// </summary>
        Task<MemberCustomFieldValuesDTO> GetMemberValuesAsync(string membershipGroupId);

        /// <summary>
        /// Gets profile custom field values (only IsProfile = true fields)
        /// For public profile display
        /// </summary>
        Task<MemberCustomFieldValuesDTO> GetProfileValuesAsync(string membershipGroupId);
    }
}
