using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.DTOs.CustomFields;
using MiniAppGIBA.Services.CustomFields;

namespace MiniAppGIBA.Controller.API
{
    /// <summary>
    /// API controller for managing group custom fields during registration
    /// </summary>
    [ApiController]
    [Route("api")]
    public class GroupCustomFieldController : BaseAPIController
    {
        private readonly IGroupCustomFieldService _groupCustomFieldService;
        private readonly ILogger<GroupCustomFieldController> _logger;

        public GroupCustomFieldController(
            IGroupCustomFieldService groupCustomFieldService,
            ILogger<GroupCustomFieldController> logger)
        {
            _groupCustomFieldService = groupCustomFieldService;
            _logger = logger;
        }

        [HttpGet("groups/{groupId}/custom-fields")]
        public async Task<IActionResult> GetGroupCustomFields(string groupId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                {
                    return Error("Group ID is required");
                }

                var result = await _groupCustomFieldService.GetGroupCustomFieldFormAsync(groupId);
                return Success(result, "Custom field form retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Group not found: {GroupId}", groupId);
                return Error(ex.Message, 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom fields for group {GroupId}", groupId);
                return Error("An error occurred while retrieving custom fields", 500);
            }
        }

        [HttpPost("groups/{groupId}/register-with-custom-fields")]
        public async Task<IActionResult> RegisterWithCustomFields(
            string groupId,
            [FromBody] RegisterGroupWithCustomFieldsRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                {
                    return Error("Group ID is required");
                }

                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrWhiteSpace(userZaloId))
                {
                    return Error("User not authenticated", 401);
                }

                var result = await _groupCustomFieldService.RegisterAndSubmitValuesAsync(
                    groupId,
                    userZaloId,
                    request?.Reason,
                    request?.Company,
                    request?.Position,
                    request?.Values ?? new Dictionary<string, string>());

                if (!result.Success)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Validation failed",
                        data = new { errors = result.Errors }
                    });
                }

                return Success(result, "Đăng ký nhóm thành công");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Group not found: {GroupId}", groupId);
                return Error(ex.Message, 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for group {GroupId}", groupId);
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering to group {GroupId}", groupId);
                return Error("An error occurred while registering to group", 500);
            }
        }

        [HttpPost("membership-groups/{membershipGroupId}/custom-field-values")]
        public async Task<IActionResult> SubmitCustomFieldValues(
            string membershipGroupId,
            [FromBody] SubmitCustomFieldValuesRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(membershipGroupId))
                {
                    return Error("Membership group ID is required");
                }

                if (request?.Values == null)
                {
                    return Error("Values are required");
                }

                var result = await _groupCustomFieldService.SubmitValuesAsync(membershipGroupId, request.Values);

                if (!result.Success)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Validation failed",
                        data = new
                        {
                            errors = result.Errors
                        }
                    });
                }

                return Success(result, "Custom field values submitted successfully");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Membership group not found: {MembershipGroupId}", membershipGroupId);
                return Error(ex.Message, 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting custom field values for membership group {MembershipGroupId}", membershipGroupId);
                return Error("An error occurred while submitting custom field values", 500);
            }
        }

        [HttpPut("membership-groups/{membershipGroupId}/custom-field-values")]
        public async Task<IActionResult> UpdateCustomFieldValues(
            string membershipGroupId,
            [FromBody] SubmitCustomFieldValuesRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(membershipGroupId))
                {
                    return Error("Membership group ID is required");
                }

                if (request?.Values == null)
                {
                    return Error("Values are required");
                }

                var result = await _groupCustomFieldService.UpdateValuesAsync(membershipGroupId, request.Values);

                if (!result.Success)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Validation failed",
                        data = new
                        {
                            errors = result.Errors
                        }
                    });
                }

                return Success(result, "Custom field values updated successfully");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Membership group not found: {MembershipGroupId}", membershipGroupId);
                return Error(ex.Message, 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating custom field values for membership group {MembershipGroupId}", membershipGroupId);
                return Error("An error occurred while updating custom field values", 500);
            }
        }

        [HttpGet("membership-groups/{membershipGroupId}/custom-field-values")]
        public async Task<IActionResult> GetCustomFieldValues(string membershipGroupId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(membershipGroupId))
                {
                    return Error("Membership group ID is required");
                }

                var result = await _groupCustomFieldService.GetMemberValuesAsync(membershipGroupId);
                return Success(result, "Custom field values retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Membership group not found: {MembershipGroupId}", membershipGroupId);
                return Error(ex.Message, 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom field values for membership group {MembershipGroupId}", membershipGroupId);
                return Error("An error occurred while retrieving custom field values", 500);
            }
        }

        /// <summary>
        /// Lấy custom field values cho profile công khai (chỉ lấy fields có IsProfile = true)
        /// Dùng cho API profile: api/memberships/giba/profile/slug/
        /// </summary>
        [HttpGet("membership-groups/{membershipGroupId}/profile-custom-fields")]
        public async Task<IActionResult> GetProfileCustomFieldValues(string membershipGroupId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(membershipGroupId))
                {
                    return Error("Membership group ID is required");
                }

                var result = await _groupCustomFieldService.GetProfileValuesAsync(membershipGroupId);
                return Success(result, "Profile custom field values retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Membership group not found: {MembershipGroupId}", membershipGroupId);
                return Error(ex.Message, 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile custom field values for membership group {MembershipGroupId}", membershipGroupId);
                return Error("An error occurred while retrieving profile custom field values", 500);
            }
        }
    }
}
