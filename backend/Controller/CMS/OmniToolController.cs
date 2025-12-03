using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Controller.CMS;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Entities.OmniTool;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Requests.OmniTools.Templates;
using MiniAppGIBA.Services.OmniTool;
using MiniAppGIBA.Services.OmniTool.EventTemplates;
using MiniAppGIBA.Services.OmniTool.Omni;
using MiniAppGIBA.Services.OmniTool.TemplateUids;
using MiniAppGIBA.Services.SystemSettings;
//using MiniAppGIBA.Services.Tags;
using Newtonsoft.Json;

namespace MiniAppGIBA.Controllers.CMS
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class OmniToolController(IOmniService omniService,
                                    //ITagService tagService,
                                    //IOmniToolService omniToolService,
                                    ISystemSettingService systemSettingService,
                                    ITemplateUidService templatUidService,
                                    IEventTemplateService eventTemplateService) : BaseCMSController
    {
        #region Campaign
        //public IActionResult IndexCampaign()
        //{
        //    return View("Campaign/Index");
        //}

        //[HttpGet("Campaign/History")]
        //public IActionResult HistoryCampaign()
        //{
        //    return View("Campaign/History");
        //}

        //[HttpGet("Campaign/Create")]
        //public async Task<IActionResult> CreateCampaign()
        //{
        //    try
        //    {
        //        var campaign = new CampaignCSKH()
        //        {
        //            Id = string.Empty,
        //            TemplateCode = string.Empty,
        //            RoutingRule = string.Empty,
        //            CampaignCode = string.Empty,
        //            CampaignName = string.Empty,
        //            Status = 1,
        //            ScheduleTime = DateTime.Now
        //        };

        //        var accountOmni = await systemSettingService.GetOmniAccountAsync();

        //        ViewBag.Tags = await tagService.GetAllAsync() ?? new List<Tag>();
        //        ViewBag.Templates = (await omniService.GetAllOwnedTemplate(accountOmni)).ListTemp;

        //        ViewBag.Button = "Lưu";
        //        ViewBag.Title = "Thêm mới cài đặt";

        //        return PartialView("Campaign/_Campaign", (campaign, new CampaignConfig(), new List<string>()));

        //    } 
        //    catch(CustomException ex)
        //    {
        //        return BadRequest(new
        //        {
        //            Code = 1,
        //            ex.Message
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        return BadRequest(new
        //        {
        //            Code = 1,
        //            Message = "Internal Server Errors"
        //        });
        //    }
        //}

        //[HttpGet("Campaign/Detail/{id}")]
        //public async Task<IActionResult> DetailCampaign(string id)
        //{
        //    var (campaign, campaingConfig) = await omniToolService.GetDetailCampaignById(id);
        //    if (campaign == null || campaingConfig == null)
        //    {
        //        return RedirectToAction("Create");
        //    }
        //    var selectedTags = await omniToolService.GetTagsByCampaignId(id);

        //    ViewBag.Title = "Chi tiết cài đặt";
        //    ViewBag.Button = "Lưu";

        //    var accountOmni = await systemSettingService.GetOmniAccountAsync();

        //    ViewBag.Tags = await tagService.GetAllAsync() ?? new List<Tag>();
        //    ViewBag.Templates = (await omniService.GetAllOwnedTemplate(accountOmni)).ListTemp;

        //    return PartialView("Campaign/_Campaign", (campaign, campaingConfig, selectedTags));
        //}

        #endregion

        #region Event Template

        public IActionResult IndexEventTemplate()
        {
            return View("EventTemplate/Index");
        }

        //[HttpGet("EventTrigger/History")]
        //public IActionResult HistoryEventTemplate()
        //{
        //    return View("EventTemplate/History");
        //}

        [HttpGet("EventTrigger/Create")]
        public async Task<IActionResult> CreateEventTemplate()
        {
            try
            {
                ViewBag.Button = "Lưu";
                ViewBag.Title = "Thêm mới cài đặt";

                var accountOmni = await systemSettingService.GetOmniAccountAsync();
                ViewBag.Templates = (await omniService.GetAllOwnedTemplate(accountOmni)).ListTemp;

                var template = new EventTemplate()
                {
                    Id = string.Empty,
                    Type = string.Empty,
                    EventName = string.Empty,
                    PhoneNumber = string.Empty,
                    RoutingRule = string.Empty,
                    TemplateCode = string.Empty,
                    TemplateMapping = string.Empty,
                    IsEnable = true
                };

                return PartialView("EventTemplate/_Template", template);

            }
            catch (CustomException ex)
            {
                return BadRequest(new
                {
                    Code = 1,
                    ex.Message
                });
            }
            catch (Exception)
            {
                return BadRequest(new
                {
                    Code = 1,
                    Message = "Internal Server Errors"
                });
            }
        }

        [HttpGet("EventTrigger/Detail/{id}")]
        public async Task<IActionResult> DetailEventTemplate(string id)
        {
            ViewBag.Button = "Lưu";
            ViewBag.Title = "Chi tiết cài đặt";

            var template = await eventTemplateService.GetByIdAsync(id);
            if (template == null)
            {
                return RedirectToAction("Create");
            }
            var accountOmni = await systemSettingService.GetOmniAccountAsync();
            var allOmniTemplates = (await omniService.GetAllOwnedTemplate(accountOmni)).ListTemp;
            ViewBag.Templates = allOmniTemplates;

            if (!string.IsNullOrEmpty(template.ReferenceId) && template.Type == "uid")
            {
                var config = await eventTemplateService.GetZaloUidConfigById(template.ReferenceId);
                ViewBag.SelectedTemplateUid = await templatUidService.GetByIdAsync(config.TemplateId);
            }

            if (!string.IsNullOrEmpty(template.ReferenceId) && template.Type == "omni")
            {
                ViewBag.SelectedTemplate = allOmniTemplates.FirstOrDefault(x => x.TemplateCode == template.TemplateCode);
            }

            return PartialView("EventTemplate/_Template", template);
        }

        #endregion

        #region Table Params

        [HttpGet("TableParams")]
        public async Task<IActionResult> TableParams([FromQuery] string code, [FromQuery] string? type, [FromQuery] string? referenceId)
        {
            var campaignParamsConfig = new List<MappingParams>();
            if (!string.IsNullOrEmpty(type) && type == "uid")
            {
                var templateUID = await templatUidService.GetByIdAsync(code);
                if (templateUID != null && !string.IsNullOrEmpty(templateUID.ListParams))
                {
                    campaignParamsConfig = templateUID.ListParams.Split(",").Select(x => new MappingParams()
                    {
                        ParamName = x.Trim(),
                        MappingColumnName = string.Empty,
                    }).ToList();
                }
            }
            else
            {
                var accountOmni = await systemSettingService.GetOmniAccountAsync();
                var template = (await omniService.GetAllOwnedTemplate(accountOmni)).ListTemp.FirstOrDefault(x => x.TemplateCode == code);
                var paramNames = template?.ParamsFormat?.Keys.Select(x => x!).ToList() ?? new List<string>();
                campaignParamsConfig = paramNames.Select(x => new MappingParams()
                {
                    ParamName = x,
                    MappingColumnName = string.Empty,
                }).ToList();
            }

            if (!string.IsNullOrEmpty(referenceId))
            {
                var selectedConfigParams = await eventTemplateService.GetZaloUidConfigById(referenceId);
                ViewBag.configuredTable = JsonConvert.DeserializeObject<List<MappingParams>>(selectedConfigParams.TemplateMapping);
            }

            ViewBag.Button = "Lưu";
            ViewBag.Title = "Chi tiết cài đặt";

            //ViewBag.AttributesExtend = await systemSettingService.GetAttributeFromExtendInfo();
            return PartialView("_TemplateParams", campaignParamsConfig);
        }

        #endregion

        #region CRUD Template UID

        public IActionResult IndexTemplateUid()
        {
            return View("ZaloTemplateUid/Index");
        }

        [HttpGet("TemplateUid/Create")]
        public IActionResult CreateTemplateUid()
        {
            var template = new ZaloTemplateUid()
            {
                Id = string.Empty,
                Name = string.Empty,
                Message = string.Empty,
                ListParams = string.Empty,
            };
            ViewBag.Title = "Thêm mới mẫu tin";
            ViewBag.Button = "Lưu";
            return PartialView("ZaloTemplateUid/_TemplateUid", template);
        }

        [HttpGet("TemplateUid/Detail/{id}")]
        public async Task<IActionResult> DetailTemplateUid(string id)
        {
            ViewBag.Title = "Cập nhật mẫu tin";
            ViewBag.Button = "Cập nhật";
            var result = await templatUidService.GetByIdAsync(id);
            if (result == null)
            {
                return RedirectToAction("CreateTemplateUid");
            }
            return PartialView("ZaloTemplateUid/_TemplateUid", result);
        }

        #endregion
    }
}
