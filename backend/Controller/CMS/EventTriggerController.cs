using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Controller.CMS;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Notifications;
using MiniAppGIBA.Entities.Notifications.Templates;
using MiniAppGIBA.Models;
using MiniAppGIBA.Models.DTOs.SystemSettings;
using MiniAppGIBA.Models.Requests.OmniTools;
using MiniAppGIBA.Services.OmniTool.Omni;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace MiniAppGIBA.Controllers.CMS
{
    public class TestHttpRequestModel
    {
        public string Method { get; set; }
        public string Endpoint { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
    }

    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class EventTriggerController(IUnitOfWork unitOfWork, IOmniService omniService) : BaseCMSController
    {
        private readonly IRepository<Entities.ETM.HttpConfig> _httpConfigRepository = unitOfWork.GetRepository<Entities.ETM.HttpConfig>();
        private readonly IRepository<OmniTemplate> _omniTemplateRepository = unitOfWork.GetRepository<OmniTemplate>();
        private readonly IRepository<EventTriggerSetting> _eventTriggerRepository = unitOfWork.GetRepository<EventTriggerSetting>();
        private readonly IRepository<Entities.ETM.ZaloTemplateUid> _zaloTemplateUidRepository = unitOfWork.GetRepository<Entities.ETM.ZaloTemplateUid>();
        private readonly IRepository<Entities.ETM.DatasourceETM> _datasourceETMRep = unitOfWork.GetRepository<Entities.ETM.DatasourceETM>();

        #region "TESTING"
        [HttpPost]
        public async Task<IActionResult> TestHttpRequest([FromBody] TestHttpRequestModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    // Set headers
                    if (model.Headers != null)
                    {
                        foreach (var header in model.Headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }

                    HttpResponseMessage response;
                    StringContent? content = null;
                    if (!string.IsNullOrEmpty(model.Body))
                    {
                        content = new StringContent(model.Body, Encoding.UTF8, "application/json");
                    }

                    // Send request based on method
                    switch (model.Method.ToUpper())
                    {
                        case "GET":
                            response = await client.GetAsync(model.Endpoint);
                            break;
                        case "POST":
                            response = await client.PostAsync(model.Endpoint, content);
                            break;
                        case "PUT":
                            response = await client.PutAsync(model.Endpoint, content);
                            break;
                        case "PATCH":
                            response = await client.PatchAsync(model.Endpoint, content);
                            break;
                        default:
                            return Json(new { success = false, message = "Method không hỗ trợ!" });
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Json(new
                    {
                        success = true,
                        statusCode = (int)response.StatusCode,
                        reasonPhrase = response.ReasonPhrase,
                        content = responseContent
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi gửi request: {ex.Message}" });
            }
        }

        #endregion

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEventTriggers(int page = 1, int pageSize = 10, string keyword = "")
        {
            try
            {
                var query = _eventTriggerRepository.AsQueryable();

                // Search filter
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.EventName.Contains(keyword) || x.ReferenceId.Contains(keyword));
                }

                var totalRecords = await query.CountAsync();
                var data = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        id = x.Id,
                        eventName = x.EventName,
                        type = x.Type == 1 ? "Zalo UID" : x.Type == 2 ? "Omni ZNS" : "Http Request",
                        referenceId = x.ReferenceId,
                        recipients = x.Recipients,
                        isActive = x.IsActive,
                        createdDate = x.CreatedDate
                    })
                    .ToListAsync();

                return Json(new
                {
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadForm(string id = "")
        {
            var ets = !string.IsNullOrWhiteSpace(id)
                        ? await _eventTriggerRepository.AsQueryable()
                                .FirstOrDefaultAsync(x => x.Id == id)
                        : new EventTriggerSetting { Recipients = "", IsActive = true, Id = "" };

            return PartialView("Partials/_EventTriggerModal", ets);
        }

        [HttpPost]
        public async Task<IActionResult> Save(EventTriggerSetting model, string templateData)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                //}

                EventTriggerSetting? entity;
                string referenceId = model.ReferenceId;

                if (string.IsNullOrEmpty(model.Id))
                {
                    // Create new
                    entity = new EventTriggerSetting
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        EventName = model.EventName,
                        Type = model.Type,
                        Conditions = model.Conditions,
                        Recipients = model.Recipients,
                        IsActive = model.IsActive,
                        CreatedDate = DateTime.Now,
                        ProcessingStep = model.ProcessingStep
                    };

                    // Xử lý tạo template tương ứng và lấy ReferenceId
                    referenceId = await CreateOrUpdateTemplate(model.Type, templateData, null);
                    entity.ReferenceId = referenceId;

                    _eventTriggerRepository.Add(entity);
                }
                else
                {
                    // Update existing
                    entity = await _eventTriggerRepository.FindByIdAsync(model.Id);
                    if (entity == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy bản ghi!" });
                    }

                    entity.EventName = model.EventName;
                    entity.Type = model.Type;
                    entity.Conditions = model.Conditions;
                    entity.Recipients = model.Recipients;
                    entity.IsActive = model.IsActive;
                    entity.UpdatedDate = DateTime.Now;
                    entity.ProcessingStep = model.ProcessingStep;

                    // Cập nhật template tương ứng
                    referenceId = await CreateOrUpdateTemplate(model.Type, templateData, entity.ReferenceId);
                    entity.ReferenceId = referenceId;

                    _eventTriggerRepository.Update(entity);
                }

                var result = await unitOfWork.SaveChangesAsync();
                return Json(new
                {
                    success = result > 0,
                    message = result > 0 ? "Lưu thành công!" : "Không thể lưu!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplate(int type)
        {
            var listName = new List<Dictionary<string, string>>();
            if (type == 1)
            {
                var uidTemplates = await _zaloTemplateUidRepository.AsQueryable().ToListAsync();
                foreach (var item in uidTemplates)
                {
                    listName.Add(new Dictionary<string, string>()
                    {
                        {"id", item.Id },
                        {"name", $"{item.Name}" }
                    });
                }
            }

            if (type == 2)
            {
                var omniTemplates = await omniService.GetAllOwnedTemplate();
                foreach (var item in omniTemplates.ListTemp)
                {
                    listName.Add(new Dictionary<string, string>()
                    {
                        {"id", item.TemplateCode },
                        { "name", item.TemplateCode }
                    });
                }
            }

            return Ok(listName);
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplateReferrence(int type, string? templateCode, string referrenceId = "")
        {
            var tableParams = new List<MappingParams>();
            // table params UID
            if (type == 1)
            {

                var zaloUidReference = new Entities.ETM.ZaloTemplateConfig()
                {
                    TemplateId = string.Empty,
                    Recipients = string.Empty,
                    TemplateMapping = string.Empty
                };
                var listParams = new List<MappingParams>();
                if (!string.IsNullOrEmpty(referrenceId))
                {
                    zaloUidReference = await unitOfWork.GetRepository<Entities.ETM.ZaloTemplateConfig>().FindByIdAsync(referrenceId);
                    if (zaloUidReference != null)
                    {
                        listParams = JsonConvert.DeserializeObject<List<MappingParams>>(zaloUidReference.TemplateMapping);
                    }
                }

                ViewBag.Templates = await _zaloTemplateUidRepository.AsQueryable().Select(x => new { x.Id, x.Name }).ToListAsync();
                return PartialView("Partials/_ZaloConfigPartial", (template: zaloUidReference, tableParams: listParams));
            }
            else if (type == 3)
            {
                var httpConfig = new Entities.ETM.HttpConfig();
                if (!string.IsNullOrEmpty(referrenceId))
                {
                    httpConfig = await _httpConfigRepository.FindByIdAsync(referrenceId);
                }
                return PartialView("Partials/_HttpConfig", httpConfig);
            }

            // table params ZNS
            var omniTemplates = await omniService.GetAllOwnedTemplate();
            var referrenceOmni = new OmniTemplate()
            {
                PhoneNumber = string.Empty,
                RoutingRule = string.Empty,
                TemplateCode = string.Empty,
                TemplateMapping = string.Empty
            };
            if (!string.IsNullOrEmpty(referrenceId))
            {
                referrenceOmni = await _omniTemplateRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == referrenceId);
                if (referrenceOmni != null && !string.IsNullOrEmpty(referrenceOmni.TemplateMapping))
                {
                    tableParams = JsonConvert.DeserializeObject<List<MappingParams>>(referrenceOmni.TemplateMapping);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(templateCode))
                {
                    var accountOmni = omniTemplates.ListTemp.FirstOrDefault(x => x.TemplateCode == templateCode);
                }
            }

            ViewBag.Templates = omniTemplates.ListTemp;
            return PartialView($"Partials/_IncomConfigPartial", (referrenceOmni, tableParams));
        }

        [HttpGet]
        public async Task<IActionResult> GetTableParams(int type, string id)
        {
            var tableParams = new List<MappingParams>();

            // table params UID
            if (type == 1)
            {
                var templateUID = await _zaloTemplateUidRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
                if (templateUID != null && !string.IsNullOrEmpty(templateUID.ListParams))
                {
                    tableParams = templateUID.ListParams.Split(",").Select(x => new MappingParams()
                    {
                        ParamName = x.Trim(),
                        MappingColumnName = string.Empty,
                    }).ToList();
                }
            }

            if (type == 2)
            {
                var omniTemplates = await omniService.GetAllOwnedTemplate();
                if (!string.IsNullOrEmpty(id))
                {
                    var accountOmni = omniTemplates.ListTemp.FirstOrDefault(x => x.TemplateCode == id);
                    tableParams = accountOmni?.ParamsFormat?.Select(x => new MappingParams
                    {
                        ParamName = x.Key,
                        MappingColumnName = string.Empty,
                    }).ToList() ?? new List<MappingParams>();
                }
            }

            return PartialView($"Partials/_TemplateParams", tableParams);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var exist = await _eventTriggerRepository.FindByIdAsync(id);
                if (exist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bản ghi!" });
                }

                // Xóa template tương ứng
                if (!string.IsNullOrEmpty(exist.ReferenceId))
                {
                    await DeleteRelatedTemplate(exist.Type, exist.ReferenceId);
                }

                await _eventTriggerRepository.DeleteByIdAsync(id);
                var result = await unitOfWork.SaveChangesAsync();
                return Json(new { success = result > 0, message = result > 0 ? "Xóa thành công!" : "Không thể xóa!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        private async Task DeleteRelatedTemplate(int type, string referenceId)
        {
            if (type == 1) // Zalo UID
            {
                await _zaloTemplateUidRepository.DeleteByIdAsync(referenceId);
            }
            else if (type == 2) // Omni Template
            {
                await _omniTemplateRepository.DeleteByIdAsync(referenceId);
            }
        }

        private async Task<string> CreateNewOmniTemplate(OmniTemplateConfig config)
        {
            var template = new OmniTemplate
            {
                Id = Guid.NewGuid().ToString("N"),
                PhoneNumber = config.PhoneNumber,
                RoutingRule = string.Join(",", config.RoutingRules ?? new List<string>()),
                TemplateCode = config.TemplateCode,
                TemplateMapping = JsonConvert.SerializeObject(config.TemplateMapping ?? new List<MappingParams>()),
                CreatedDate = DateTime.Now
            };

            _omniTemplateRepository.Add(template);
            await Task.CompletedTask;
            return template.Id;
        }

        private async Task<string> CreateNewZaloTemplateConfig(ZaloUidConfigRequest zaloUidConfigRequest)
        {
            var zaloUidCofig = new Entities.ETM.ZaloTemplateConfig
            {
                Recipients = string.Join(",", zaloUidConfigRequest.Recipients),
                TemplateId = zaloUidConfigRequest.ZaloTemplateUid,
                TemplateMapping = zaloUidConfigRequest.ParamsConfig.Any() ? JsonConvert.SerializeObject(zaloUidConfigRequest.ParamsConfig) : string.Empty
            };

            unitOfWork.GetRepository<Entities.ETM.ZaloTemplateConfig>().Add(zaloUidCofig);
            await unitOfWork.SaveChangesAsync();
            return zaloUidCofig.Id;
        }

        private async Task<string> CreateNewHttpConfig(Entities.ETM.HttpConfig httpConfig)
        {
            var newHttpConfig = new Entities.ETM.HttpConfig
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = httpConfig.Method,
                Endpoint = httpConfig.Endpoint,
                HeadersJson = httpConfig.HeadersJson,
                BodyJson = httpConfig.BodyJson,
                CreatedDate = DateTime.Now
            };

            unitOfWork.GetRepository<Entities.ETM.HttpConfig>().Add(newHttpConfig);
            await unitOfWork.SaveChangesAsync();
            return newHttpConfig.Id;
        }

        private async Task<string> CreateOrUpdateTemplate(int type, string templateData, string? existingReferenceId)
        {
            if (type == 1) // Zalo UID
            {
                var templateConfig = JsonConvert.DeserializeObject<ZaloUidConfigRequest>(templateData);
                if (templateConfig == null) throw new ArgumentNullException(nameof(templateConfig));

                Entities.ETM.ZaloTemplateConfig? zaloUidConfig;
                if (!string.IsNullOrEmpty(existingReferenceId))
                {
                    // Update existing
                    zaloUidConfig = await unitOfWork.GetRepository<Entities.ETM.ZaloTemplateConfig>()
                                                    .FindByIdAsync(existingReferenceId);
                    if (zaloUidConfig != null)
                    {
                        zaloUidConfig.TemplateId = templateConfig.ZaloTemplateUid;
                        zaloUidConfig.TemplateMapping = templateConfig.ParamsConfig.Any()
                                                            ? JsonConvert.SerializeObject(templateConfig.ParamsConfig)
                                                            : string.Empty;
                        zaloUidConfig.Recipients = string.Join(",", templateConfig.Recipients);
                        zaloUidConfig.UpdatedDate = DateTime.Now;
                        unitOfWork.GetRepository<Entities.ETM.ZaloTemplateConfig>().Update(zaloUidConfig);
                    }
                    else
                    {
                        return await CreateNewZaloTemplateConfig(templateConfig);
                    }
                }
                else
                {
                    return await CreateNewZaloTemplateConfig(templateConfig);
                }

                return zaloUidConfig.Id;
            }

            else if (type == 2) // Omni Template
            {
                var templateConfig = JsonConvert.DeserializeObject<OmniTemplateConfig>(templateData);

                OmniTemplate? template;
                if (!string.IsNullOrEmpty(existingReferenceId))
                {
                    // Update existing
                    template = await _omniTemplateRepository.FindByIdAsync(existingReferenceId);
                    if (template != null)
                    {
                        template.PhoneNumber = templateConfig.PhoneNumber;
                        template.RoutingRule = string.Join(",", templateConfig.RoutingRules ?? new List<string>());
                        template.TemplateCode = templateConfig.TemplateCode;
                        template.TemplateMapping = JsonConvert.SerializeObject(templateConfig.TemplateMapping ?? new List<MappingParams>());
                        template.UpdatedDate = DateTime.Now;
                        _omniTemplateRepository.Update(template);
                    }
                    else
                    {
                        return await CreateNewOmniTemplate(templateConfig);
                    }
                }
                else
                {
                    return await CreateNewOmniTemplate(templateConfig);
                }

                return template.Id;
            }

            else if (type == 3)
            {
                var _httpConfig = JsonConvert.DeserializeObject<Entities.ETM.HttpConfig>(templateData);
                if (_httpConfig == null) throw new ArgumentNullException(nameof(_httpConfig));

                Entities.ETM.HttpConfig? httpConfig;
                if (!string.IsNullOrEmpty(existingReferenceId))
                {
                    // Update existing
                    httpConfig = await unitOfWork.GetRepository<Entities.ETM.HttpConfig>()
                                                    .FindByIdAsync(existingReferenceId);
                    if (httpConfig != null)
                    {
                        httpConfig.Method = _httpConfig.Method;
                        httpConfig.Endpoint = _httpConfig.Endpoint;
                        httpConfig.HeadersJson = _httpConfig.HeadersJson;
                        httpConfig.BodyJson = _httpConfig.BodyJson;
                        httpConfig.UpdatedDate = DateTime.Now;
                        unitOfWork.GetRepository<Entities.ETM.HttpConfig>().Update(httpConfig);
                    }
                    else
                    {
                        return await CreateNewHttpConfig(_httpConfig);
                    }
                }
                else
                {
                    return await CreateNewHttpConfig(_httpConfig);
                }

                return httpConfig.Id;
            }

            return string.Empty; // Invalid type, handle as needed
        }

        #region History

        public IActionResult History()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEventLogs(int draw, int start, int length, string keyword = "", string type = "", string resultCode = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var logRepository = unitOfWork.GetRepository<EventTriggerLog>();
                var query = logRepository.AsQueryable();

                // Search filters
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.Message.Contains(keyword) || x.Recipient.Contains(keyword));
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(x => x.Type == type);
                }

                if (!string.IsNullOrEmpty(resultCode))
                {
                    query = query.Where(x => x.ResultCode == resultCode);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate <= toDate.Value.AddDays(1));
                }

                var totalRecords = await query.CountAsync();

                // Apply pagination
                var data = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(start)
                    .Take(length)
                    .Select(x => new
                    {
                        id = x.Id,
                        type = x.Type,
                        message = x.Message,
                        recipient = x.Recipient,
                        resultCode = x.ResultCode,
                        createdDate = x.CreatedDate,
                        metadata = x.Metadata
                    })
                    .ToListAsync();

                return Json(new
                {
                    Draw = draw,
                    RecordsTotal = totalRecords,
                    RecordsFiltered = totalRecords,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats(string keyword = "", string type = "", string resultCode = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var logRepository = unitOfWork.GetRepository<EventTriggerLog>();
                var query = logRepository.AsQueryable();

                // Apply same filters as GetEventLogs
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.Message.Contains(keyword) || x.Recipient.Contains(keyword));
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(x => x.Type == type);
                }

                if (!string.IsNullOrEmpty(resultCode))
                {
                    query = query.Where(x => x.ResultCode == resultCode);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate <= toDate.Value.AddDays(1));
                }

                var totalSent = await query.CountAsync();
                var successCount = await query.Where(x => x.ResultCode == "200" || x.ResultCode == "0").CountAsync();
                var errorCount = totalSent - successCount;

                var typeStats = await query
                    .GroupBy(x => x.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                var dailyStats = await query
                    .Where(x => x.CreatedDate >= DateTime.Now.AddDays(-7))
                    .GroupBy(x => x.CreatedDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                return Json(new
                {
                    TotalSent = totalSent,
                    SuccessCount = successCount,
                    ErrorCount = errorCount,
                    successRate = totalSent > 0 ? Math.Round((double)successCount / totalSent * 100, 2) : 0,
                    TypeStats = typeStats,
                    DailyStats = dailyStats
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewLogDetail(string id)
        {
            try
            {
                var logRepository = unitOfWork.GetRepository<EventTriggerLog>();
                var log = await logRepository.FindByIdAsync(id);

                if (log == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy log!" });
                }

                return PartialView("Partials/_LogDetailModal", log);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Omni Account Configuration
        private readonly IRepository<Common> _commonRepository = unitOfWork.GetRepository<Common>();
        private const string OMNI_ACCOUNT_KEY = "OMNI_ACCOUNT_CONFIG";

        [HttpGet]
        public IActionResult OmniAccount()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetOmniAccountConfig()
        {
            try
            {
                var accountConfig = await _commonRepository.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Name == OMNI_ACCOUNT_KEY);

                OmniAccountDTO omniAccount = new OmniAccountDTO();

                if (accountConfig != null && !string.IsNullOrEmpty(accountConfig.Content))
                {
                    omniAccount = JsonConvert.DeserializeObject<OmniAccountDTO>(accountConfig.Content) ?? new OmniAccountDTO();
                }

                var model = new
                {
                    omniAccount = omniAccount,
                    isConfigured = accountConfig != null
                };

                return Json(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOmniAccountConfig(string envUrl, string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(envUrl) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin!" });
                }

                var omniAccount = new OmniAccountDTO
                {
                    EnvUrl = envUrl,
                    Username = username,
                    Password = password
                };

                var accountJson = JsonConvert.SerializeObject(omniAccount);

                var existingConfig = await _commonRepository.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Name == OMNI_ACCOUNT_KEY);

                if (existingConfig == null)
                {
                    // Create new
                    var newConfig = new Common
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Name = OMNI_ACCOUNT_KEY,
                        Content = accountJson,
                        CreatedDate = DateTime.Now
                    };
                    _commonRepository.Add(newConfig);
                }
                else
                {
                    // Update existing
                    existingConfig.Content = accountJson;
                    existingConfig.UpdatedDate = DateTime.Now;
                    _commonRepository.Update(existingConfig);
                }

                var result = await unitOfWork.SaveChangesAsync();
                return Json(new
                {
                    success = result > 0,
                    message = result > 0 ? "Lưu cấu hình thành công!" : "Không thể lưu cấu hình!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestOmniConnection()
        {
            try
            {
                var omniAccount = await GetOmniAccountFromConfig();
                if (omniAccount == null)
                {
                    return Json(new { success = false, message = "Chưa cấu hình tài khoản Omni!" });
                }

                // Test connection by getting templates
                var templates = await omniService.GetAllOwnedTemplate(omniAccount);

                return Json(new
                {
                    success = true,
                    message = "Kết nối thành công!",
                    templateCount = templates.ListTemp?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Kết nối thất bại: {ex.Message}" });
            }
        }

        private async Task<OmniAccountDTO?> GetOmniAccountFromConfig()
        {
            var accountConfig = await _commonRepository.AsQueryable()
                .FirstOrDefaultAsync(x => x.Name == OMNI_ACCOUNT_KEY);

            if (accountConfig == null || string.IsNullOrEmpty(accountConfig.Content))
                return null;

            return JsonConvert.DeserializeObject<OmniAccountDTO>(accountConfig.Content);
        }

        #endregion


        #region xử lý Datasource ETM

        public IActionResult Datasource()
        {
            // Tính thống kê
            var total = _datasourceETMRep.AsQueryable().Count();
            var used = _datasourceETMRep.AsQueryable().Count(x => x.IsUsed);
            var unused = total - used;

            ViewBag.StatisticDataSource = new
            {
                Total = total,
                Active = unused, // Đang hoạt động = chưa sử dụng
                Inactive = used, // Không hoạt động = đã sử dụng
                Used = used,
                Unused = unused
            };

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPageDataSource(bool? isUsed, int page = 1, int pageSize = 10, string keyword = "")
        {
            try
            {
                var query = _datasourceETMRep.AsQueryable();

                // Search filter
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.Key.Contains(keyword) || x.Code.Contains(keyword) || x.Value.Contains(keyword));
                }

                if (isUsed.HasValue)
                {
                    query = query.Where(x => x.IsUsed == isUsed);
                }

                var totalRecords = await query.CountAsync();
                var data = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                //return Json(new
                //{
                //    recordsTotal = totalRecords,
                //    recordsFiltered = totalRecords,
                //    data = data
                //});

                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công!",
                    data,
                    totalPages,
                    totalCount = totalRecords
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet("EventTrigger/GetDataSourceStats")]
        public async Task<IActionResult> GetDataSourceStats()
        {
            try
            {
                var query = _datasourceETMRep.AsQueryable();

                var total = await query.CountAsync();
                var used = await query.CountAsync(x => x.IsUsed == true);
                var unused = total - used;

                return Ok(new
                {
                    total = total,
                    used = used,
                    unused = unused
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet("EventTrigger/CreateDTSForm")]
        public async Task<IActionResult> CreateDTSForm(string id = "")
        {
            Entities.ETM.DatasourceETM? data;
            if (!string.IsNullOrEmpty(id))
            {
                data = await _datasourceETMRep.FindByIdAsync(id);
                if (data == null)
                {
                    data = new Entities.ETM.DatasourceETM() { Id = "", Code = "", Key = "", Value = "" };
                }
                ViewBag.Button = "Cập nhật";
                ViewBag.Title = "Cập nhật dữ liệu ETM";
            }
            else
            {
                data = new Entities.ETM.DatasourceETM()
                {
                    Id = string.Empty,
                    Code = string.Empty,
                    Key = string.Empty,
                    Value = string.Empty,
                };
                ViewBag.Button = "Lưu";
                ViewBag.Title = "Thêm mới dữ liệu ETM";
            }

            return PartialView("Partials/_DatasourceETM", data);
        }

        [HttpPost("EventTrigger/CreateDataSource")]
        public async Task<IActionResult> CreateDataSource(string code, string key, string value)
        {
            if (_datasourceETMRep.AsQueryable().Any(x => x.Value == value && x.Code == code && x.Key == key))
            {
                return Ok(new
                {
                    code = 1,
                    message = "Dòng dữ liệu đã tồn tại trong hệ thống"
                });
            }

            var data = new Entities.ETM.DatasourceETM()
            {
                Code = code,
                Key = key,
                Value = value,
            };

            _datasourceETMRep.Add(data);
            int rs = await unitOfWork.SaveChangesAsync();

            if (rs > 0)
            {
                return Ok(new
                {
                    code = 0,
                    message = "Thêm mới dữ liệu thành công"
                });
            }

            return Ok(new
            {
                code = 2,
                message = "Thêm mới dữ liệu thất bại"
            });
        }

        [HttpPut("EventTrigger/UpdateDataSource")]
        public async Task<IActionResult> UpdateDataSource(string id, string code, string key, string value, bool isUsed)
        {
            var existing = await _datasourceETMRep.FindByIdAsync(id);
            if (existing == null)
            {
                return Ok(new { code = 1, message = "Không tìm thấy dữ liệu" });
            }

            if (_datasourceETMRep.AsQueryable().Any(x => x.Value == value && x.Code == code && x.Key == key && x.Id != id))
            {
                return Ok(new { code = 1, message = "Dòng dữ liệu đã tồn tại trong hệ thống" });
            }

            existing.Code = code;
            existing.Key = key;
            existing.Value = value;
            existing.IsUsed = isUsed;
            existing.UpdatedDate = DateTime.Now;
            _datasourceETMRep.Update(existing);
            int rs = await unitOfWork.SaveChangesAsync();

            if (rs > 0)
            {
                return Ok(new { code = 0, message = "Cập nhật dữ liệu thành công" });
            }

            return Ok(new { code = 2, message = "Cập nhật dữ liệu thất bại" });
        }

        [HttpDelete("EventTrigger/DeleteDataSource/{id}")]
        public async Task<IActionResult> DeleteDataSource(string id)
        {
            var existing = await _datasourceETMRep.FindByIdAsync(id);
            if (existing == null)
            {
                return Ok(new { code = 1, message = "Không tìm thấy dữ liệu" });
            }

            await _datasourceETMRep.DeleteByIdAsync(id);
            int rs = await unitOfWork.SaveChangesAsync();

            if (rs > 0)
            {
                return Ok(new { code = 0, message = "Xóa dữ liệu thành công" });
            }

            return Ok(new { code = 2, message = "Xóa dữ liệu thất bại" });
        }

        [HttpGet("EventTrigger/ImportTemplateExcel")]
        public async Task<IActionResult> ImportTemplateExcel()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("DatasourceETM");

            // Tạo header
            worksheet.Cells[1, 1].Value = "Code";
            worksheet.Cells[1, 2].Value = "Key";
            worksheet.Cells[1, 3].Value = "Value";

            // Định dạng header
            using (var range = worksheet.Cells[1, 1, 1, 11])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Dòng mẫu (optional)
            worksheet.Cells[2, 1].Value = "DKTVM";
            worksheet.Cells[2, 2].Value = "otp";
            worksheet.Cells[2, 3].Value = "789123";


            worksheet.Cells.AutoFitColumns();
            worksheet.Cells[1, 1, 3, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;

            var fileName = $"Import_Datasource_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(stream, contentType, fileName);
        }

        [HttpPost("ImportDatasourceETMExcel")]
        public async Task<IActionResult> ImportDatasourceETMExcel([FromForm] IFormFile file)
        {
            int importCount = 0;
            var messages = new List<string>();
            var culture = new CultureInfo("vi-VN");

            // Track codes in the current import file
            var importCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            int rows = worksheet.Dimension.Rows;

            for (int i = 2; i <= rows; i++)
            {
                try
                {
                    string code = worksheet.Cells[i, 1].Text?.Trim() ?? "";
                    string key = worksheet.Cells[i, 2].Text?.Trim() ?? "";
                    string value = worksheet.Cells[i, 3].Text?.Trim() ?? "";

                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(value))
                        continue;
                    if (code.Length > 50 || key.Length > 50 || value.Length > 200)
                    {
                        messages.Add($"Dòng {i}: Vượt quá độ dài cho phép (Code/Key ≤ 50 ký tự, Value ≤ 200 ký tự)");
                        continue;
                    }

                    if (!Regex.IsMatch(code, @"^[a-zA-Z0-9]+$") || !Regex.IsMatch(key, @"^[a-zA-Z0-9]+$"))
                    {
                        messages.Add($"Dòng {i}: Code hoặc Key chứa ký tự không hợp lệ. Chỉ cho phép chữ và số, không dấu, không khoảng trắng, không ký tự đặc biệt.");
                        continue;
                    }

                    string compositeKey = $"{code}|||{key}|||{value}";
                    if (importCodes.Contains(compositeKey))
                    {
                        messages.Add($"Dòng {i}: Bị trùng với dòng trước trong file import - '{code}', '{key}', '{value}'");
                        continue;
                    }

                    if (_datasourceETMRep.AsQueryable().Any(x => x.Value == value && x.Code == code && x.Key == key))
                    {
                        messages.Add($"Dòng {i}: Tên chiến dịch: '{code}' - Tên bến: '{key}' - Giá trị: '{value}' đã tồn tại trong hệ thống");
                        continue;
                    }

                    var data = new Entities.ETM.DatasourceETM()
                    {
                        Code = code,
                        Key = key,
                        Value = value,
                    };

                    _datasourceETMRep.Add(data);
                    importCodes.Add(compositeKey);
                    importCount++;
                }
                catch (Exception ex)
                {
                    messages.Add($"Lỗi ở dòng {i}: {ex.Message}");
                }
            }
            if (messages.Count == 0)
            {

                var total = await unitOfWork.SaveChangesAsync();
            }

            return Ok(new
            {
                Code = 0,
                Message = "Nhập dữ liệu thành công!",
                Errors = messages,
                Count = importCount

            });
        }

        [HttpGet("ExportDatasource")]
        public async Task<IActionResult> ExportDatasource(bool? isUsed, string keyword = "")
        {
            try
            {

                var query = _datasourceETMRep.AsQueryable();

                // Search filter
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.Key.Contains(keyword) || x.Code.Contains(keyword) || x.Value.Contains(keyword));
                }

                if (isUsed.HasValue)
                {
                    query = query.Where(x => x.IsUsed == isUsed);
                }
                var data = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("DatasourceETM");

                worksheet.Cells[1, 1].Value = "Tên chiến dịch";
                worksheet.Cells[1, 2].Value = "Tên biến";
                worksheet.Cells[1, 3].Value = "Giá trị biến";
                worksheet.Cells[1, 4].Value = "Trạng thái";


                // Style header
                using (var range = worksheet.Cells[1, 1, 1, 11])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Add data
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    int row = i + 2;

                    worksheet.Cells[row, 1].Value = item.Code;
                    worksheet.Cells[row, 2].Value = item.Key;
                    worksheet.Cells[row, 3].Value = item.Value;
                    worksheet.Cells[row, 4].Value = item.IsUsed ?
                         "Không hoạt động"
                        : "Đang hoạt động";

                }

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream);
                stream.Position = 0;

                var fileName = $"Export_Datasource_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Lỗi khi xuất file Excel"
                });
            }
        }


        #endregion

    }
}
