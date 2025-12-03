
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Request.Rules;
using MiniAppGIBA.Services.Rules;

namespace MiniAppGIBA.Controller.CMS
{
    [Route("Setting")]
    public class BehaviorRulesV2Controller : BaseCMSController
    {
        private readonly IBehaviorRuleService _behaviorRuleService;
        private readonly ILogger<BehaviorRulesV2Controller> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public BehaviorRulesV2Controller(
            IBehaviorRuleService behaviorRuleService,
            ILogger<BehaviorRulesV2Controller> logger,
            IUnitOfWork unitOfWork)
        {
            _behaviorRuleService = behaviorRuleService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("BehaviorRulesV2")]
        public IActionResult BehaviorRulesV2Page()
        {
            return View("~/Views/Setting/BehaviorRulesV2.cshtml");
        }

        /// <summary>
        /// Load form tạo mới quy tắc ứng xử
        /// </summary>
        [HttpGet("BehaviorRulesV2/Create")]
        public IActionResult Create(string type, string? groupId = null, string? groupName = null)
        {
            ViewBag.IsEdit = false;
            ViewBag.Type = type;
            ViewBag.GroupId = groupId;
            ViewBag.GroupName = groupName;

            var model = new CreateBehaviorRuleRequest
            {
                Type = type,
                GroupId = groupId,
                ContentType = "TEXT",
                SortOrder = 0
            };

            return PartialView("~/Views/Setting/Partials/_BehaviorRuleForm.cshtml", model);
        }

        /// <summary>
        /// Load form chỉnh sửa quy tắc ứng xử
        /// </summary>
        [HttpGet("BehaviorRulesV2/Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                // Get the rule data
                var rule = await _behaviorRuleService.GetByIdAsync(id);
                if (rule == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy quy tắc ứng xử" });
                }

                ViewBag.IsEdit = true;
                ViewBag.RuleId = id;
                ViewBag.Type = rule.Type;
                ViewBag.GroupId = rule.GroupId;

                // Get group name if applicable
                if (!string.IsNullOrEmpty(rule.GroupId))
                {
                    var groupRepo = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Groups.Group>();
                    var group = await groupRepo.AsQueryable()
                        .Where(g => g.Id == rule.GroupId)
                        .FirstOrDefaultAsync();
                    ViewBag.GroupName = group?.GroupName ?? "Unknown";
                }
                else
                {
                    ViewBag.GroupName = "Không";
                }

                // Map to model
                var model = new CreateBehaviorRuleRequest
                {
                    ContentType = rule.ContentType,
                    Type = rule.Type,
                    GroupId = rule.GroupId,
                    Content = rule.Content,
                    Title = rule.Title,
                    SortOrder = rule.SortOrder
                };

                return PartialView("~/Views/Setting/Partials/_BehaviorRuleForm.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading behavior rule edit form {RuleId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải form chỉnh sửa" });
            }
        }
    }
}