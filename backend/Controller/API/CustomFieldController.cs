using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.DTOs.CustomFields;
using MiniAppGIBA.Services.CustomFields;

namespace MiniAppGIBA.Controller.API
{
    /// <summary>
    /// API controller for managing custom fields and form submissions
    /// Handles retrieval of form structures, form submission, and submitted value retrieval
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomFieldController : BaseAPIController
    {
        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomFieldTabService _customFieldTabService;
        private readonly ICustomFieldValueService _customFieldValueService;
        private readonly ICustomFieldFormHandler _formHandler;

        public CustomFieldController(
            ICustomFieldService customFieldService,
            ICustomFieldTabService customFieldTabService,
            ICustomFieldValueService customFieldValueService,
            ICustomFieldFormHandler formHandler)
        {
            _customFieldService = customFieldService;
            _customFieldTabService = customFieldTabService;
            _customFieldValueService = customFieldValueService;
            _formHandler = formHandler;
        }

        /// <summary>
        /// Retrieves the form structure (tabs and fields) for a specific entity
        /// </summary>
        /// <param name="entityType">The type of entity (e.g., "GroupMembership", "EventRegistration")</param>
        /// <param name="entityId">The ID of the entity (e.g., GroupId for membership registration)</param>
        /// <returns>Form structure with tabs and fields organized by display order</returns>
        /// <remarks>
        /// Requirements: 3.1, 3.2
        /// </remarks>
        [HttpGet("form-structure")]
        public async Task<IActionResult> GetFormStructure(
            [FromQuery] string entityType,
            [FromQuery] string entityId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                {
                    return Error("Entity type and entity ID are required");
                }

                // Parse entity type
                if (!System.Enum.TryParse<MiniAppGIBA.Enum.ECustomFieldEntityType>(entityType, true, out var parsedEntityType))
                {
                    return Error($"Invalid entity type: {entityType}");
                }

                // Get tabs for the entity
                var tabs = await _customFieldTabService.GetTabsByEntityAsync(parsedEntityType, entityId);

                // Get fields for the entity
                var fields = await _customFieldService.GetFieldsByEntityAsync(parsedEntityType, entityId);

                // Organize fields by tab
                var formStructure = new
                {
                    entityType = parsedEntityType.ToString(),
                    entityId,
                    tabs = tabs.OrderBy(t => t.DisplayOrder).Select(tab => new
                    {
                        tab.Id,
                        tab.TabName,
                        tab.DisplayOrder,
                        fields = fields
                            .Where(f => f.CustomFieldTabId == tab.Id)
                            .OrderBy(f => f.DisplayOrder)
                            .ToList()
                    }).ToList(),
                    flatFields = fields
                        .Where(f => f.CustomFieldTabId == null)
                        .OrderBy(f => f.DisplayOrder)
                        .ToList()
                };

                return Success(formStructure, "Form structure retrieved successfully");
            }
            catch (Exception ex)
            {
                return Error($"Error retrieving form structure: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Validates a form submission against custom field definitions
        /// </summary>
        /// <param name="entityType">The type of entity being submitted</param>
        /// <param name="entityId">The ID of the entity (e.g., GroupId for membership registration)</param>
        /// <param name="request">Dictionary of field IDs to submitted values</param>
        /// <returns>Validation result with errors if validation fails</returns>
        /// <remarks>
        /// /// Requirements: 3.3, 3.4
        /// </remarks>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateForm(
            [FromQuery] string entityType,
            [FromQuery] string entityId,
            [FromBody] Dictionary<string, string> request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                {
                    return Error("Entity typand entity ID are required");
                }

                if (request == null || request.Count == 0)
                {
                    return Error("Form values are required");
                }

                // Parse entity type
                if (!System.Enum.TryParse<MiniAppGIBA.Enum.ECustomFieldEntityType>(entityType, true, out var parsedEntityType))
                {
                    return Error($"Invalid entity type: {entityType}");
                }

                // Validate form
                var validationResult = await _formHandler.ValidateFormAsync(
                    parsedEntityType,
                    entityId,
                    request);

                if (!validationResult.IsValid)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Form validation failed",
                        data = new
                        {
                            isValid = false,
                            errors = validationResult.Errors
                        }
                    });
                }

                return Success(new { isValid = true, errors = new Dictionary<string, string>() }, "Form validation passed");
            }
            catch (Exception ex)
            {
                return Error($"Error validating form: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Submits a form with custom field values
        /// </summary>
        /// <param name="entityType">The type of entity being submitted</param>
        /// <param name="entityId">The ID of the entity instance (e.g., MembershipGroupId for membership submission)</param>
        /// <param name="request">Dictionary of field IDs to submitted values</param>
        /// <returns>List of created CustomFieldValue DTOs if successful</returns>
        /// <remarks>
        /// Requirements: 3.5
        /// </remarks>
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitForm(
            [FromQuery] string entityType,
            [FromQuery] string entityId,
            [FromBody] Dictionary<string, string> request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                {
                    return Error("Entity type and entity ID are required");
                }

                if (request == null || request.Count == 0)
                {
                    return Error("Form values are required");
                }

                // Parse entity type
                if (!System.Enum.TryParse<MiniAppGIBA.Enum.ECustomFieldEntityType>(entityType, true, out var parsedEntityType))
                {
                    return Error($"Invalid entity type: {entityType}");
                }

                // Submit form
                var submittedValues = await _formHandler.SubmitFormAsync(
                    parsedEntityType,
                    entityId,
                    request);

                return Success(submittedValues, "Form submitted successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                return Error($"Error submitting form: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// /// Retrieves submitted custom field values for a specific entity
        /// </summary>
        /// <param name="entityType">The type of entity</param>
        /// <param name="entityId">The ID of the entity instance</param>
        /// <returns>List of submitted custom field values organized by tabs</returns>
        /// <remarks>
        /// Requirements: 4.1, 4.2
        /// </remarks>
        [HttpGet("submitted-values")]
        public async Task<IActionResult> GetSubmittedValues(
            [FromQuery] string entityType,
            [FromQuery] string entityId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                {
                    return Error("Entity type and entity ID are required");
                }

                // Parse entity type
                if (!System.Enum.TryParse<MiniAppGIBA.Enum.ECustomFieldEntityType>(entityType, true, out var parsedEntityType))
                {
                    return Error($"Invalid entity type: {entityType}");
                }

                // Get submitted values
                var values = await _customFieldValueService.GetValuesByEntityAsync(parsedEntityType, entityId);

                // Get tabs and fields to organize values
                var tabs = await _customFieldTabService.GetTabsByEntityAsync(parsedEntityType, entityId);
                var fields = await _customFieldService.GetFieldsByEntityAsync(parsedEntityType, entityId);

                // Organize values by tab
                var organizedValues = new
                {
                    entityType = parsedEntityType.ToString(),
                    entityId,
                    tabs = tabs.OrderBy(t => t.DisplayOrder).Select(tab => new
                    {
                        tab.Id,
                        tab.TabName,
                        tab.DisplayOrder,
                        values = values
                            .Where(v => fields.FirstOrDefault(f => f.Id == v.CustomFieldId)?.CustomFieldTabId == tab.Id)
                            .ToList()
                    }).ToList(),
                    flatValues = values
                        .Where(v => fields.FirstOrDefault(f => f.Id == v.CustomFieldId)?.CustomFieldTabId == null)
                        .ToList()
                };

                return Success(organizedValues, "Submitted values retrieved successfully");
            }
            catch (Exception ex)
            {
                return Error($"Error retrieving submitted values: {ex.Message}", 500);
            }
        }
    }
}
