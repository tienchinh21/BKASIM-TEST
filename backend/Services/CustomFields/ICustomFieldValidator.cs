using MiniAppGIBA.Entities.CustomFields;
using MiniAppGIBA.Models.DTOs.CustomFields;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Interface for custom field value validation
    /// </summary>
    public interface ICustomFieldValidator
    {
        /// <summary>
        /// Validates a value against a custom field definition
        /// </summary>
        /// <param name="field">The custom field definition</param>
        /// <param name="value">The value to validate</param>
        /// <returns>Validation result with success status and error message if failed</returns>
        FieldValidationResult ValidateValue(CustomField field, string? value);

        /// <summary>
        /// Validates an email address format
        /// </summary>
        /// <param name="value">The email value to validate</param>
        /// <returns>Validation result</returns>
        FieldValidationResult ValidateEmail(string value);

        /// <summary>
        /// Validates a phone number format
        /// </summary>
        /// <param name="value">The phone number to validate</param>
        /// <returns>Validation result</returns>
        FieldValidationResult ValidatePhone(string value);

        /// <summary>
        /// Validates a date format
        /// </summary>
        /// <param name="value">The date string to validate</param>
        /// <returns>Validation result</returns>
        FieldValidationResult ValidateDate(string value);

        /// <summary>
        /// Validates a dropdown value against available options
        /// </summary>
        /// <param name="value">The selected value</param>
        /// <param name="options">List of valid options</param>
        /// <returns>Validation result</returns>
        FieldValidationResult ValidateDropdown(string value, List<string> options);
    }

    /// <summary>
    /// Result of a field validation
    /// </summary>
    public class FieldValidationResult
    {
        /// <summary>
        /// Whether the validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public static FieldValidationResult Success() => new() { IsValid = true };

        /// <summary>
        /// Creates a failed validation result with error message
        /// </summary>
        public static FieldValidationResult Failure(string errorMessage) => new() { IsValid = false, ErrorMessage = errorMessage };
    }
}
