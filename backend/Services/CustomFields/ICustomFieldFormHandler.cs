using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.DTOs.CustomFields;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Service interface for handling form validation and submission with custom fields
    /// </summary>
    public interface ICustomFieldFormHandler
    {
        /// <summary>
        /// Validates a form submission against custom field definitions
        /// </summary>
        /// <param name="entityType">The type of entity being submitted</param>
        /// <param name="entityId">The ID of the entity (e.g., GroupId for membership registration)</param>
        /// <param name="submittedValues">Dictionary of field IDs to submitted values</param>
        /// <returns>Validation result with errors if validation fails</returns>
        Task<FormValidationResult> ValidateFormAsync(
            ECustomFieldEntityType entityType,
            string entityId,
            Dictionary<string, string> submittedValues);

        /// <summary>
        /// Validates and submits a form with custom field values
        /// </summary>
        /// <param name="entityType">The type of entity being submitted</param>
        /// <param name="entityId">The ID of the entity instance (e.g., MembershipGroupId for membership submission)</param>
        /// <param name="submittedValues">Dictionary of field IDs to submitted values</param>
        /// <returns>List of created CustomFieldValue DTOs if successful</returns>
        /// <exception cref="InvalidOperationException">Thrown if validation fails</exception>
        Task<List<CustomFieldValueDTO>> SubmitFormAsync(
            ECustomFieldEntityType entityType,
            string entityId,
            Dictionary<string, string> submittedValues);
    }
}
