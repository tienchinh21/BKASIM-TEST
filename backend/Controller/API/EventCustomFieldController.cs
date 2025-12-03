using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Controller.API;
using MiniAppGIBA.Models.Request.Events;
using MiniAppGIBA.Services.Events;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class EventCustomFieldController : BaseAPIController
{
    private readonly IEventCustomFieldService _eventCustomFieldService;

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetEventCustomFields(string eventId)
    {
        var eventCustomFields = await _eventCustomFieldService.GetEventCustomFieldsAsync(eventId);
        return Ok(eventCustomFields);
    }
    [HttpPost]
    public async Task<IActionResult> CreateEventCustomField([FromBody] CreateEventCustomFieldRequest eventCustomField)
    {
        var result = await _eventCustomFieldService.CreateEventCustomFieldAsync(eventCustomField);
        return Success(result);
    }

   
    [HttpGet("ListValue")]
    public async Task<IActionResult> GetListValueCustomFields([FromQuery] string? GuestListId = null, [FromQuery] string? EventRegistrationId = null)
    {
        var result = await _eventCustomFieldService.GetListValueCustomFieldsAsync(GuestListId, EventRegistrationId);
        return Success(result);
    }
    
    /// Tạo danh sách giá trị custom fields mà người dùng nhập khi đăng ký sự kiện
    [HttpPost("Value/{GuestListId}")]
    public async Task<IActionResult> CreateEventCustomFieldValues(string GuestListId, [FromBody] List<CreateEventCustomFieldValueRequest>? requests = null)
    {
        if (!ModelState.IsValid)
        {
            return Error("Dữ liệu không hợp lệ");
        }

        try
        {

            return Success(await _eventCustomFieldService.CreateEventCustomFieldValuesAsync(GuestListId, requests), "Lưu giá trị custom fields thành công");



        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception)
        {
            return Error("Có lỗi xảy ra khi lưu giá trị custom fields");
        }
    }
}