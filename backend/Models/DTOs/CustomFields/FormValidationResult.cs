namespace MiniAppGIBA.Models.DTOs.CustomFields
{
    /// <summary>
    /// Result of form validation containing success status and error messages
    /// </summary>
    public class FormValidationResult
    {
        /// <summary>
        /// Whether the form validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Dictionary of field IDs to error messages for failed validation
        /// </summary>
        public Dictionary<string, string> Errors { get; set; } = [];

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public static FormValidationResult Success() => new() { IsValid = true };

        /// <summary>
        /// Creates a failed validation result with errors
        /// </summary>
        public static FormValidationResult Failure(Dictionary<string, string> errors) =>
            new() { IsValid = false, Errors = errors };

        /// <summary>
        /// Creates a failed validation result with a single error
        /// </summary>
        public static FormValidationResult Failure(string fieldId, string errorMessage) =>
            new() { IsValid = false, Errors = new Dictionary<string, string> { { fieldId, errorMessage } } };
    }
}
