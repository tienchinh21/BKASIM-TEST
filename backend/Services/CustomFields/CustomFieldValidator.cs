using System.Text.RegularExpressions;
using MiniAppGIBA.Entities.CustomFields;
using MiniAppGIBA.Enum;
using Newtonsoft.Json;

namespace MiniAppGIBA.Services.CustomFields
{
    /// <summary>
    /// Implementation of custom field value validation
    /// </summary>
    public class CustomFieldValidator : ICustomFieldValidator
    {
        // Email regex pattern (RFC 5322 simplified)
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Phone regex pattern (allows digits, +, -, spaces, parentheses)
        private static readonly Regex PhoneRegex = new(
            @"^[\d\s\+\-\(\)\.]+$",
            RegexOptions.Compiled);

        // Date formats to try
        private static readonly string[] DateFormats = new[]
        {
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy",
            "yyyy/MM/dd"
        };

        /// <inheritdoc />
        public FieldValidationResult ValidateValue(CustomField field, string? value)
        {
            // Check required field
            if (field.IsRequired && string.IsNullOrWhiteSpace(value))
            {
                return FieldValidationResult.Failure($"Field '{field.FieldName}' is required");
            }

            // If value is empty and field is not required, skip type validation
            if (string.IsNullOrWhiteSpace(value))
            {
                return FieldValidationResult.Success();
            }

            // Validate based on field type
            return field.FieldType switch
            {
                EEventFieldType.Email => ValidateEmailWithFieldName(value, field.FieldName),
                EEventFieldType.PhoneNumber => ValidatePhoneWithFieldName(value, field.FieldName),
                EEventFieldType.Date => ValidateDateWithFieldName(value, field.FieldName),
                EEventFieldType.DateTime => ValidateDateTimeWithFieldName(value, field.FieldName),
                EEventFieldType.Dropdown => ValidateDropdownWithFieldName(value, GetFieldOptions(field.FieldOptions), field.FieldName),
                EEventFieldType.MultipleChoice => ValidateMultipleChoiceWithFieldName(value, GetFieldOptions(field.FieldOptions), field.FieldName),
                EEventFieldType.Integer => ValidateInteger(value, field.FieldName),
                EEventFieldType.Decimal => ValidateDecimal(value, field.FieldName),
                EEventFieldType.Url => ValidateUrl(value, field.FieldName),
                _ => FieldValidationResult.Success() // Text, LongText, Boolean, File, Image - no format validation
            };
        }

        /// <inheritdoc />
        public FieldValidationResult ValidateEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            return EmailRegex.IsMatch(value)
                ? FieldValidationResult.Success()
                : FieldValidationResult.Failure("Invalid email format");
        }

        /// <inheritdoc />
        public FieldValidationResult ValidatePhone(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            // Remove all whitespace for length check
            var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
            
            if (!PhoneRegex.IsMatch(value))
                return FieldValidationResult.Failure("Invalid phone number format");

            if (digitsOnly.Length < 8 || digitsOnly.Length > 15)
                return FieldValidationResult.Failure("Phone number must have 8-15 digits");

            return FieldValidationResult.Success();
        }

        /// <inheritdoc />
        public FieldValidationResult ValidateDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            foreach (var format in DateFormats)
            {
                if (DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out _))
                    return FieldValidationResult.Success();
            }

            return FieldValidationResult.Failure("Invalid date format");
        }

        /// <inheritdoc />
        public FieldValidationResult ValidateDropdown(string value, List<string> options)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            if (options == null || options.Count == 0)
                return FieldValidationResult.Failure("No options available for dropdown");

            return options.Contains(value, StringComparer.OrdinalIgnoreCase)
                ? FieldValidationResult.Success()
                : FieldValidationResult.Failure("Value must be one of the available options");
        }

        #region Private Helper Methods

        private FieldValidationResult ValidateEmailWithFieldName(string value, string fieldName)
        {
            var result = ValidateEmail(value);
            if (!result.IsValid)
                return FieldValidationResult.Failure($"Field '{fieldName}': {result.ErrorMessage}");
            return result;
        }

        private FieldValidationResult ValidatePhoneWithFieldName(string value, string fieldName)
        {
            var result = ValidatePhone(value);
            if (!result.IsValid)
                return FieldValidationResult.Failure($"Field '{fieldName}': {result.ErrorMessage}");
            return result;
        }

        private FieldValidationResult ValidateDateWithFieldName(string value, string fieldName)
        {
            var result = ValidateDate(value);
            if (!result.IsValid)
                return FieldValidationResult.Failure($"Field '{fieldName}': {result.ErrorMessage}");
            return result;
        }

        private FieldValidationResult ValidateDateTimeWithFieldName(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            if (DateTime.TryParse(value, out _))
                return FieldValidationResult.Success();

            return FieldValidationResult.Failure($"Field '{fieldName}': Invalid date/time format");
        }

        private FieldValidationResult ValidateDropdownWithFieldName(string value, List<string> options, string fieldName)
        {
            var result = ValidateDropdown(value, options);
            if (!result.IsValid)
                return FieldValidationResult.Failure($"Field '{fieldName}': {result.ErrorMessage}");
            return result;
        }

        private FieldValidationResult ValidateMultipleChoiceWithFieldName(string value, List<string> options, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            if (options == null || options.Count == 0)
                return FieldValidationResult.Failure($"Field '{fieldName}': No options available");

            // Multiple choice values are stored as JSON array or comma-separated
            var selectedValues = ParseMultipleChoiceValue(value);
            
            foreach (var selected in selectedValues)
            {
                if (!options.Contains(selected, StringComparer.OrdinalIgnoreCase))
                    return FieldValidationResult.Failure($"Field '{fieldName}': Value '{selected}' is not a valid option");
            }

            return FieldValidationResult.Success();
        }

        private FieldValidationResult ValidateInteger(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            return int.TryParse(value, out _)
                ? FieldValidationResult.Success()
                : FieldValidationResult.Failure($"Field '{fieldName}': Invalid integer format");
        }

        private FieldValidationResult ValidateDecimal(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            return decimal.TryParse(value, out _)
                ? FieldValidationResult.Success()
                : FieldValidationResult.Failure($"Field '{fieldName}': Invalid decimal format");
        }

        private FieldValidationResult ValidateUrl(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FieldValidationResult.Success();

            return Uri.TryCreate(value, UriKind.Absolute, out var uri) && 
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                ? FieldValidationResult.Success()
                : FieldValidationResult.Failure($"Field '{fieldName}': Invalid URL format");
        }

        private static List<string> GetFieldOptions(string? fieldOptionsJson)
        {
            if (string.IsNullOrWhiteSpace(fieldOptionsJson))
                return new List<string>();

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(fieldOptionsJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static List<string> ParseMultipleChoiceValue(string value)
        {
            try
            {
                // Try parsing as JSON array first
                var parsed = JsonConvert.DeserializeObject<List<string>>(value);
                if (parsed != null)
                    return parsed;
            }
            catch
            {
                // Fall back to comma-separated
            }

            return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(v => v.Trim())
                       .ToList();
        }

        #endregion
    }
}
