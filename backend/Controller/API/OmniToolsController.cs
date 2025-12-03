using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Requests.OmniTools;
using MiniAppGIBA.Services.OmniTool;
using MiniAppGIBA.Services.OmniTool.EventTemplates;
using MiniAppGIBA.Services.OmniTool.TemplateUids;

namespace MiniAppGIBA.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OmniToolsController(ILogger<OmniToolsController> logger,
                                     IOmniToolService omniToolService,
                                     ITemplateUidService templateUidService,
                                     IEventTemplateService eventTemplateService) : ControllerBase
    {
        #region Campaign

        //[HttpGet("Campaign")]
        //public async Task<IActionResult> GetPageCampaign([FromQuery] RequestQuery query)
        //{
        //    try
        //    {
        //        var templates = await omniToolService.GetPageCampaign(query);
        //        return Ok(new
        //        {
        //            Code = 0,
        //            Message = "Thành công",
        //            Data = templates.Items,
        //            totalPages = templates.TotalPages,
        //            totalCount = templates.TotalItems,
        //            currentPage = templates.Page,
        //            pageSize = templates.PageSize
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error fetching all promotions");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //[HttpPost("Campaign")]
        //public async Task<IActionResult> CreateCampaign([FromBody] CampaignRequest model)
        //{
        //    try
        //    {
        //        var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? string.Empty;
        //        await omniToolService.CreateCampaignAsync(userId, model);
        //        return Ok(new
        //        {
        //            Code = 0,
        //            Message = "Tạo mới thành công!"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is AlreadyExistsException)
        //        {
        //            return Ok(new { Code = 409, ex.Message });
        //        }

        //        logger.LogError(ex, "Error fetching all promotions");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //[HttpPut("Campaign/{id}")]
        //public async Task<IActionResult> UpdateCapaign(string id, [FromBody] CampaignRequest model)
        //{
        //    try
        //    {
        //        var updated = await omniToolService.UpdateCampaignAsync(id, model);
        //        return Ok(new
        //        {
        //            Code = 0,
        //            Message = "Cập nhật thành công!"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error fetching all promotions");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //[HttpDelete("Campaign/{id}")]
        //public async Task<IActionResult> DeleteCampaign(string id)
        //{
        //    try
        //    {
        //        await omniToolService.DeleteCampaignByIdAsync(id);
        //        return Ok(new
        //        {
        //            Code = 0,
        //            Message = "Xoá thành công!"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error fetching all promotions");
        //        return StatusCode(500, "Đã xảy ra lỗi. Vui lòng thử lại sau!");
        //    }
        //}

        //[HttpGet("Campaign/History")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetCampaignHistory([FromQuery] RequestQuery query, [FromQuery] string? campaignId, [FromQuery] short? status)
        //{
        //    try
        //    {
        //        var campaignPhones = await omniToolService.GetPageCampaignPhoneLogs(query, campaignId, status);
        //        return Ok(new
        //        {
        //            Code = 0,
        //            Message = "Thành công",
        //            Data = campaignPhones.Items,
        //            campaignPhones.TotalPages,
        //            TotalCount = campaignPhones.TotalItems
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error fetching all promotions");
        //        return Ok(new
        //        {
        //            Code = 1,
        //            Message = "Thất bại",
        //            Data = new List<object>(),
        //        });
        //    }
        //}

        #endregion

        #region Event Trigger Messaging

        [HttpGet("Template")]
        public async Task<IActionResult> GetPageTemplate([FromQuery] RequestQuery query)
        {
            try
            {
                var templates = await eventTemplateService.GetPage(query);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all promotions");
                return StatusCode(500, "Internal server error");
            }
        }


        #region Event Template V1

        [HttpPost("Template")]
        public async Task<IActionResult> CreateTemplate([FromBody] EventTemplateRequest model)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? string.Empty;
                await eventTemplateService.CreateAsync(model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Tạo mới thành công!"
                });
            }
            catch (Exception ex)
            {
                if (ex is AlreadyExistsException)
                {
                    return Ok(new { Code = 409, ex.Message });
                }

                logger.LogError(ex, "Error fetching all promotions");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("Template/{id}")]
        public async Task<IActionResult> UpdateTemplate(string id, [FromBody] EventTemplateRequest model)
        {
            try
            {
                var updated = await eventTemplateService.UpdateAsync(id, model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Cập nhật thành công!"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all promotions");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region Event Template V2

        [HttpPost("Template/uid")]
        public async Task<IActionResult> CreateTemplateV2([FromBody] ZaloUidConfigRequest model)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? string.Empty;
                await eventTemplateService.CreateAsync(model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Tạo mới thành công!"
                });
            }
            catch (Exception ex)
            {
                if (ex is AlreadyExistsException)
                {
                    return Ok(new { Code = 409, ex.Message });
                }

                logger.LogError(ex, "Error fetching all promotions");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("Template/{id}/uid")]
        public async Task<IActionResult> UpdateTemplateV2(string id, [FromBody] ZaloUidConfigRequest model)
        {
            try
            {
                var updated = await eventTemplateService.UpdateAsync(id, model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Cập nhật thành công!"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all promotions");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        [HttpDelete("Template/{id}")]
        public async Task<IActionResult> DeleteTemplate(string id)
        {
            try
            {
                await eventTemplateService.DeleteByIdAsync(id);
                return Ok(new
                {
                    Code = 0,
                    Message = "Xoá thành công!"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all promotions");
                return StatusCode(500, "Đã xảy ra lỗi. Vui lòng thử lại sau!");
            }
        }

        [HttpGet("Template/History")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMessageHistory([FromQuery] RequestQuery query, [FromQuery] string? status, [FromQuery] string? type,
                                                           [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var campaignPhones = await omniToolService.GetEventTemplateLogs(query, status, type, fromDate, toDate);
                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công",
                    Data = campaignPhones.Items,
                    campaignPhones.TotalPages,
                    TotalCount = campaignPhones.TotalItems
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all promotions");
                return Ok(new
                {
                    Code = 1,
                    Message = "Thất bại",
                    Data = new List<object>(),
                });
            }
        }

        [HttpGet("Template/ExportHistory")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportMessageHistory([FromQuery] string? status, [FromQuery] string? type,
                                                      [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
                                                      [FromQuery] string? keyword)
        {
            try
            {
                var excelData = await omniToolService.ExportEventTemplateLogsToExcel(status, type, fromDate, toDate, keyword);
                string fileName = $"MessageHistory_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error exporting message history");
                return BadRequest(new
                {
                    Code = 1,
                    Message = "Có lỗi khi xuất Excel",
                });
            }
        }

        #endregion

        #region Template UID

        [HttpGet("TemplateUid")]
        public async Task<IActionResult> GetPageTemplateUid([FromQuery] RequestQuery query)
        {
            try
            {
                var templates = await templateUidService.GetPage(query);
                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công",
                    Data = templates.Items,
                    totalPages = templates.TotalPages,
                    totalCount = templates.TotalItems,
                    currentPage = templates.Page,
                    pageSize = templates.PageSize
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error!"
                });
            }

        }

        [HttpPost("TemplateUid")]
        public async Task<IActionResult> CreateTemplateUid([FromBody] ZaloTemplateUid model)
        {
            try
            {
                var result = await templateUidService.CreateAsync(model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Tạo mẫu hóa đơn thành công!",
                    Data = result
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error!"
                });
            }
        }

        [HttpPut("TemplateUid/{id}")]
        public async Task<IActionResult> UpdateTemplateUid(string id, [FromBody] ZaloTemplateUid model)
        {
            try
            {
                await templateUidService.UpdateAsync(id, model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Cập nhật mẫu UID thành công!",
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error!"
                });
            }
        }

        [HttpDelete("TemplateUid/{id}")]
        public async Task<IActionResult> DeleteTemplateUid(string id)
        {
            try
            {
                await templateUidService.DeleteByIdAsync(id);
                return Ok(new
                {
                    Code = 0,
                    Message = "Xóa mẫu hóa UID thành công!",
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error!"
                });
            }
        }

        #endregion
    }
}
