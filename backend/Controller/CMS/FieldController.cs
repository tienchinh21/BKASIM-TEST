using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Models.Request.Fields;
using MiniAppGIBA.Services.Fields;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize]
    [Route("Field")]
    public class FieldController : BaseCMSController
    {
        private readonly IFieldService _fieldService;

        public FieldController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Only SUPER_ADMIN can create fields
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            ViewBag.IsEdit = false;
            return PartialView("Partials/_FieldForm", new CreateFieldRequest());
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            // Only SUPER_ADMIN can edit fields
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            var field = await _fieldService.GetFieldByIdAsync(id);
            if (field == null)
            {
                return NotFound();
            }

            ViewBag.IsEdit = true;
            ViewBag.FieldId = field.Id;
            ViewBag.FieldName = field.FieldName;
            ViewBag.Description = field.Description;
            ViewBag.DisplayOrderMiniApp = field.DisplayOrderMiniApp;
            ViewBag.IsActive = field.IsActive;

            var request = new CreateFieldRequest
            {
                FieldName = field.FieldName,
                Description = field.Description,
                DisplayOrderMiniApp = field.DisplayOrderMiniApp,
                IsActive = field.IsActive,
                Children = field.Children.Select(c => new CreateFieldChildRequest
                {
                    ChildName = c.ChildName,
                    Description = c.Description
                }).ToList()
            };

            return PartialView("Partials/_FieldForm", request);
        }

        [HttpGet("GetPage")]
        public async Task<IActionResult> GetPage(int page = 1, int pageSize = 10, string? keyword = null, string? status = null)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var queryParameters = new Models.Queries.Fields.FieldQueryParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    FieldName = keyword
                };

                // Parse status filter
                if (!string.IsNullOrEmpty(status) && bool.TryParse(status, out bool isActive))
                {
                    queryParameters.IsActive = isActive;
                }

                var result = await _fieldService.GetFieldsAsync(queryParameters);

                var responseData = result.Items.Select(f => new
                {
                    id = f.Id,
                    name = f.FieldName,
                    description = f.Description,
                    displayOrderMiniApp = f.DisplayOrderMiniApp,
                    status = f.IsActive,
                    createdDate = f.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    children = f.Children.Select(c => new
                    {
                        id = c.Id,
                        name = c.ChildName,
                        description = c.Description
                    }).ToList()
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = responseData,
                    totalItems = result.TotalItems,
                    totalPages = result.TotalPages,
                    currentPage = result.Page
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateFieldRequest request)
        {
            // Only SUPER_ADMIN can create fields
            if (!IsSuperAdmin())
            {
                return Json(new { success = false, message = "Chỉ SUPER_ADMIN mới có quyền tạo lĩnh vực!" });
            }

            try
            {
                Console.WriteLine("request.FieldName: " + request.FieldName);
                if (string.IsNullOrEmpty(request.FieldName))
                {
                    return Json(new { success = false, message = "Tên lĩnh vực là bắt buộc" });
                }

                var field = await _fieldService.CreateFieldAsync(request);
                return Json(new { success = true, message = "Tạo lĩnh vực thành công!", data = field });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] UpdateFieldRequest request)
        {
            // Only SUPER_ADMIN can edit fields
            if (!IsSuperAdmin())
            {
                return Json(new { success = false, message = "Chỉ SUPER_ADMIN mới có quyền chỉnh sửa lĩnh vực!" });
            }

            try
            {
                var field = await _fieldService.UpdateFieldAsync(request);
                return Json(new { success = true, message = "Cập nhật lĩnh vực thành công!", data = field });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                await _fieldService.DeleteFieldAsync(id);
                return Json(new { success = true, message = "Xóa lĩnh vực thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API lấy danh sách lĩnh vực cha (ParentId = null)
        /// </summary>
        [HttpGet("GetParentFields")]
        public async Task<IActionResult> GetParentFields()
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var fields = await _fieldService.GetParentFieldsAsync();
                return Json(new { success = true, data = fields });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetFieldDetails")]
        public async Task<IActionResult> GetFieldDetails(string id)
        {
            try
            {
                var field = await _fieldService.GetFieldByIdAsync(id);
                if (field == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục" });
                }

                return Json(new { 
                    success = true, 
                    data = new {
                        id = field.Id,
                        fieldName = field.FieldName,
                        description = field.Description,
                        displayOrderMiniApp = field.DisplayOrderMiniApp,
                        isActive = field.IsActive,
                        children = field.Children.Select(c => new {
                            id = c.Id,
                            childName = c.ChildName,
                            description = c.Description,
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
