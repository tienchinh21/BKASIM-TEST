//using Hangfire;
//using Microsoft.EntityFrameworkCore;
//using MiniAppGIBA.Base.Dependencies.Zalo;
//using MiniAppGIBA.Base.Helpers;
//using MiniAppGIBA.Base.Interface;
//using MiniAppGIBA.Entities.ETM;
//using MiniAppGIBA.Entities.Memberships;
//using MiniAppGIBA.Entities.OmniTool;
//using MiniAppGIBA.Models.Requests.OmniTools.Templates;
//using MiniAppGIBA.Services.OmniTool.Omni;
//using MiniAppGIBA.Services.OmniTool.TokenManager;
//using MiniAppGIBA.Services.SystemSettings;
//using Newtonsoft.Json;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace MiniAppGIBA.Services.OmniTool.Notifications
//{
//    public class NotificationService(IConfiguration configuration,
//                                     ILogger<NotificationService> logger,
//                                     IUnitOfWork unitOfWork,
//                                     IOmniService omniService,
//                                     //IMappingDataService mappingDataService, 
//                                     ITokenManagerService tokenManagerService,
//                                     ISystemSettingService systemSettingService) : INotificationService
//    {
//        #region Execute Notification By Event

//        private readonly IRepository<EventLog> _eventLogRepo = unitOfWork.GetRepository<EventLog>();
//        private readonly IRepository<EventTemplate> _eventTemplateRepo = unitOfWork.GetRepository<EventTemplate>();
//        private readonly IRepository<EventTemplateLog> _eventTemplateLogRepo = unitOfWork.GetRepository<EventTemplateLog>();

//        #region Omni Template

//        public async Task ExecuteOrderEvent(string eventName, string orderId)
//        {
//            var order = await orderService.GetOrderByIdAsync(orderId);
//            if (order == null)
//            {
//                Console.WriteLine($"❌ [ERROR] ExecuteOrderEvent: Order with ID {orderId} not found.");
//                return;
//            }

//            var templateQuery = _eventTemplateRepo.AsQueryable().Where(x => x.EventName == eventName && x.IsEnable);
//            var templates = await templateQuery.ToListAsync();
//            if (templates == null || !templates.Any())
//            {
//                Console.WriteLine($"❌ [ERROR] ExecuteOrderEvent: No template found for event {eventName}.");
//                return;
//            }

//            var omniAccount = await systemSettingService.GetOmniAccountAsync();


//            var recipient = await _membershipRepo.AsQueryable().FirstOrDefaultAsync(x => x.UserZaloId == order.UserZaloId);
//            var membshipsExtendsDict = (await _membershipExtendRepo.AsQueryable()
//              .Where(x => !string.IsNullOrEmpty(x.Attribute) && !string.IsNullOrEmpty(x.Content) && x.UserZaloId == order.UserZaloId)
//              .ToListAsync())
//              .GroupBy(x => x.UserZaloId)
//              .ToDictionary(
//                  g => g.Key ?? string.Empty, // Đảm bảo UserZaloId không null
//                  g => g.ToDictionary(
//                      x => x.Attribute ?? string.Empty,  // Đảm bảo Attribute không null
//                      x => x.Content ?? string.Empty     // Đảm bảo Content không null
//                  )
//              );

//            foreach (var template in templates)
//            {
//                try
//                {
//                    var paramsConfig = JsonConvert.DeserializeObject<List<MappingParams>>(template.TemplateMapping);
//                    if (paramsConfig == null || !paramsConfig.Any())
//                    {
//                        Console.WriteLine($"❌ [ERROR] ExecuteOrderEvent: Invalid template mapping configuration for template {template.Id}.");
//                        continue;
//                    }

//                    // Create dictionary to store final parameter values
//                    var paramValues = new Dictionary<string, string>();
//                    // Map order properties to template parameters
//                    foreach (var param in paramsConfig)
//                    {
//                        if (string.IsNullOrEmpty(param.ParamName)) // || string.IsNullOrEmpty(config.MappingTableName) || string.IsNullOrEmpty(config.MappingColumnName))
//                        {
//                            continue;
//                        }

//                        object value = param.DefaultValue ?? string.Empty;
//                        if (param.MappingTableName == "Membership") // lấy thông thin từ người dùng
//                        {
//                            var property = typeof(Membership).GetProperty(param.MappingColumnName);
//                            value = property?.GetValue(recipient)?.ToString() ?? value;
//                        }
//                        else if (param.MappingTableName == "MembershipExtend" && recipient != null) // lấy thông tin từ bảng mở rộng
//                        {
//                            var membershipExtendData = membshipsExtendsDict.GetValueOrDefault(recipient.UserZaloId) ?? new Dictionary<string, string>();
//                            if (membershipExtendData.TryGetValue(param.ParamName, out var extendValue) && !string.IsNullOrEmpty(extendValue))
//                            {
//                                value = extendValue;
//                            }
//                        }
//                        else if (param.MappingTableName == "Order")
//                        {
//                            value = GetOrderParameterValue(order, param.MappingColumnName);
//                        }
//                        paramValues[param.ParamName] = $"{value}";
//                    }

//                    if (!string.IsNullOrEmpty(template.Conditions))
//                    {
//                        var checkCondition = CheckCondition(template.Conditions, new
//                        {
//                            order
//                        });

//                        if (!checkCondition)
//                        {
//                            continue;
//                        }
//                    }

//                    // Get recipient phone number - either from template or from order
//                    string phoneNumber = !string.IsNullOrEmpty(template.PhoneNumber) && template.PhoneNumber != "MEMBERSHIP.PHONENUMBER"
//                        ? template.PhoneNumber
//                        : order.PhoneNumber ?? string.Empty;

//                    if (string.IsNullOrEmpty(phoneNumber))
//                    {
//                        Console.WriteLine($"❌ [ERROR] ExecuteOrderEvent: No phone number available for template {template.Id}.");
//                        continue;
//                    }

//                    // Send the Omni message with mapped parameters
//                    var response = await omniService.SendOmniMessageAsync(
//                        omniAccount,
//                        template.TemplateCode,
//                        phoneNumber,
//                        template.RoutingRule,
//                        paramValues, new List<string>() { });

//                    var metadata = $"{JsonConvert.SerializeObject(new
//                    {
//                        IdOMniMess = response.IdOmniMess,
//                        TelcoId = PhoneNumberHandler.GetIdTelco(phoneNumber),
//                        Status = response.Status.ToString(),
//                    })}";
//                    var requestString = $"{phoneNumber} - {template.TemplateCode} - {string.Join(",", template.RoutingRule)} - {JsonConvert.SerializeObject(paramValues)}";
//                    var responseString = JsonConvert.SerializeObject(response);

//                    // Log the result
//                    await SaveLog(phoneNumber, requestString, responseString, response.Status, "omni", metadata);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"❌ [ERROR] ExecuteOrderEvent: Error processing template {template.Id} - {ex.Message}");
//                }
//            }
//        }

//        // Helper method to extract parameter values from order data
//        private string GetOrderParameterValue(OrderDetailResponse order, string sourceField)
//        {
//            try
//            {
//                Dictionary<int, string> mappingStatus = new Dictionary<int, string>()
//                {
//                    { (int)EOrder.Pending, "Chờ xử lý" },
//                    { (int)EOrder.Confirmed, "Đã xác nhận" },
//                    { (int)EOrder.Completed, "Hoàn thành" },
//                    { (int)EOrder.Cancelled, "Đã hủy" }
//                };

//                return sourceField switch
//                {
//                    "OrderId" => order.OrderId,
//                    "CustomerName" => order.UserZaloName ?? string.Empty,
//                    "Phone" => order.PhoneNumber ?? string.Empty,
//                    "Address" => order.Address ?? string.Empty,
//                    "TotalAmount" => order.TotalAmount.ToString("N0"),
//                    "Status" => mappingStatus.ContainsKey(order.OrderStatus) ? mappingStatus[order.OrderStatus] : string.Empty,
//                    "OrderDate" => order.CreatedDate.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
//                    "ProductList" => GetFormattedProductList(order),
//                    "PaymentMethod" => order.PaymentMethod ?? string.Empty,
//                    "PointUsage" => order.PointUsage.ToString(),
//                    _ => string.Empty
//                };
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ [ERROR] GetOrderParameterValue: Failed to get value for field {sourceField} - {ex.Message}");
//                return string.Empty;
//            }
//        }

//        // Format product list for message templates
//        private string GetFormattedProductList(OrderDetailResponse order)
//        {
//            if (order.OrderDetails == null || !order.OrderDetails.Any())
//                return string.Empty;

//            return string.Join(", ", order.OrderDetails.Select(item =>
//                $"{item.ProductName} ({item.Quantity})"
//            ));
//        }

//        #endregion

//        #region Zalo UID Template

//        // cái này sẽ tối ưu hơn nếu sửa lại logic, cái này làm tạm nên tách ra các hàm xử lý như này
//        private readonly IRepository<CampaignItem> _campaignItemRepo = unitOfWork.GetRepository<CampaignItem>();

//        private readonly IRepository<Membership> _membershipRepo = unitOfWork.GetRepository<Membership>();
//        private readonly IRepository<MembershipExtend> _membershipExtendRepo = unitOfWork.GetRepository<MembershipExtend>();

//        private readonly IRepository<ZaloTemplateUid> _zaloTemplateUidRepo = unitOfWork.GetRepository<ZaloTemplateUid>();
//        private readonly IRepository<ZaloTemplateConfig> _zaloTemplateConfigRepo = unitOfWork.GetRepository<ZaloTemplateConfig>();

//        private async Task<CampaignItem?> GetRandomAvailableCampaignItemAync()
//        {
//            var campaignItem = await _campaignItemRepo.AsQueryable()
//                .FirstOrDefaultAsync(x => !x.IsUsed);
//            return campaignItem;
//        }

//        public async Task ExecuteTemplateUIDByEvent(string eventName, string triggerUserZaloId, string? formId)
//        {
//            // Lấy danh sách template theo event
//            var templates = await _eventTemplateRepo.AsQueryable()
//                .Where(x => x.EventName == eventName && x.IsEnable && x.Type.Contains("uid"))
//                .ToListAsync();

//            if (!templates.Any()) return;

//            // Lấy danh sách config template theo reference id
//            var referenceIds = templates.Select(x => x.ReferenceId).ToList();
//            var templateConfigs = await _zaloTemplateConfigRepo.AsQueryable()
//                .Where(x => referenceIds.Contains(x.Id))
//                .ToListAsync();

//            if (!templateConfigs.Any()) return;

//            // Gom tất cả Recipients từ các config
//            var allRecipients = templateConfigs
//                .SelectMany(c => c.Recipients.Split(","))
//                .Where(x => x != "trigger")
//                .ToHashSet(StringComparer.OrdinalIgnoreCase);
//            allRecipients.Add(triggerUserZaloId);

//            // danh sách khách hàng nhận thông báo này
//            var memberhips = await _membershipRepo.AsQueryable()
//                                            .Where(x => allRecipients.Contains(x.UserZaloId) || x.UserZaloId == triggerUserZaloId)
//                                            .ToListAsync();

//            // danh sách template content
//            var templateIds = templateConfigs.Select(x => x.TemplateId).ToList();
//            var templateContent = await _zaloTemplateUidRepo.AsQueryable()
//                                            .Where(x => templateIds.Contains(x.Id))
//                                            .ToListAsync();
//            var accessToken = await tokenManagerService.GetAccessToken();
//            if (string.IsNullOrEmpty(accessToken))
//            {
//                return;
//            }

//            var membshipsExtendsDict = (await _membershipExtendRepo.AsQueryable()
//               .Where(x => string.IsNullOrEmpty(x.Attribute) && string.IsNullOrEmpty(x.Content) && allRecipients.Contains(x.UserZaloId))
//               .ToListAsync())
//               .GroupBy(x => x.UserZaloId)
//               .ToDictionary(
//                   g => g.Key ?? string.Empty, // Đảm bảo UserZaloId không null
//                   g => g.ToDictionary(
//                       x => x.Attribute ?? string.Empty,  // Đảm bảo Attribute không null
//                       x => x.Content ?? string.Empty     // Đảm bảo Content không null
//                   )
//               );

//            foreach (var recipient in memberhips)
//            {
//                var membershipExtendData = membshipsExtendsDict.GetValueOrDefault(recipient.UserZaloId) ?? new Dictionary<string, string>();
//                // TODO: Thực hiện gửi tin dựa trên configs
//                foreach (var item in templateConfigs)
//                {
//                    // nội dung template
//                    var template = templateContent.FirstOrDefault(x => x.Id == item.TemplateId);
//                    if (template == null) continue;

//                    // check condition của người dùng có đủ điều kiện để gửi tin này ko? 
//                    var eventTemplate = templates.FirstOrDefault(x => x.ReferenceId == item.Id);
//                    if (eventTemplate == null)
//                    {
//                        Console.WriteLine($"❌ [ERROR] ExecuteOrderEvent: Event template not found for config {item.Id}.");
//                        continue;
//                    }

//                    if (!string.IsNullOrEmpty(eventTemplate.Conditions))
//                    {
//                        var checkCondition = CheckCondition(eventTemplate.Conditions, new
//                        {
//                            formId,
//                            memberhip = recipient,
//                            membershipExtend = membershipExtendData
//                        });

//                        if (!checkCondition)
//                        {
//                            continue;
//                        }
//                    }
//                    else
//                    {
//                        if (!string.IsNullOrEmpty(formId))
//                        {
//                            Console.WriteLine($"⚠️ [WARNING] Skipping template with empty condition while formId exists. Config: {item.Id}");
//                            continue;
//                        }
//                    }

//                    // cấu hình thông tin template
//                    var paramsConfig = JsonConvert.DeserializeObject<List<MappingParams>>(item.TemplateMapping); // danh sách cấu hình
//                    if (paramsConfig == null || !paramsConfig.Any())
//                    {
//                        Console.WriteLine($"❌ [ERROR] ExecuteOrderEvent: Invalid template mapping configuration for template {template.Id}.");
//                        continue;
//                    }

//                    if (!string.IsNullOrEmpty(template.ListParams))
//                    {
//                        var existingParams = paramsConfig.Select(x => x.ParamName).ToList();
//                        var listTemplateParams = template.ListParams.Split(",")
//                                                         .Where(x => !existingParams.Contains(x))
//                                                         .Select(x => new MappingParams()
//                                                         {
//                                                             ParamName = x.Trim(),
//                                                             MappingColumnName = string.Empty
//                                                         }).ToList();
//                        paramsConfig.AddRange(listTemplateParams);
//                    }

//                    // Create dictionary to store final parameter values
//                    var paramValues = new Dictionary<string, string>();

//                    // TO DO :gọi hàm mapping data
//                    foreach (var config in paramsConfig)
//                    {
//                        if (string.IsNullOrEmpty(config.ParamName)) // || string.IsNullOrEmpty(config.MappingTableName) || string.IsNullOrEmpty(config.MappingColumnName))
//                        {
//                            continue;
//                        }

//                        object value = config.DefaultValue ?? string.Empty;
//                        if (config.MappingTableName == "Membership") // lấy thông thin từ người dùng
//                        {
//                            var property = typeof(Membership).GetProperty(config.MappingColumnName);
//                            value = property?.GetValue(recipient)?.ToString() ?? value;
//                        }
//                        else if (config.MappingTableName == "MembershipExtend") // lấy thông tin từ bảng mở rộng
//                        {
//                            if (membershipExtendData.TryGetValue(config.ParamName, out var extendValue) && !string.IsNullOrEmpty(extendValue))
//                            {
//                                value = extendValue;
//                            }
//                        }
//                        else if (config.MappingTableName == "CampaignItem")
//                        {
//                            var campaignItem = await GetRandomAvailableCampaignItemAync();
//                            if (campaignItem != null)
//                            {
//                                var property = typeof(CampaignItem).GetProperty(config.MappingColumnName);
//                                value = property?.GetValue(campaignItem)?.ToString() ?? value;

//                                campaignItem.IsUsed = true;
//                                _campaignItemRepo.Update(campaignItem);
//                                await unitOfWork.SaveChangesAsync();
//                            }
//                        }
//                        paramValues[config.ParamName] = $"{value}";
//                    }

//                    if (paramValues.Values.Any(string.IsNullOrEmpty))
//                    {
//                        // Gửi alert telegram nếu như paramValues thiếu giá trị
//                        var missingKeys = paramValues.Where(kvp => string.IsNullOrEmpty(kvp.Value)).Select(kvp => kvp.Key);
//                        var alertContent =
//                            $"[ALERT] Thiếu giá trị trong paramValues khi thực thi sự kiện!\n" +
//                            $"👉 Sự kiện: {eventName}\n" +
//                            $"📄 Mẫu UID: {template.Name}\n" +
//                            $"👤 Người nhận: {recipient.UserZaloName} - UID: {recipient.UserZaloIdByOA}\n" +
//                            $"❌ Thiếu giá trị cho các key: {string.Join(", ", missingKeys)}\n" +
//                            $"⏰ Thời gian: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
//                        await SendTelegramBot.SendMessageTelegramBot(configuration, alertContent);
//                        return;
//                    }

//                    // replace nội dung template với các biến
//                    var content = $@"{{ 
//                        ""recipient"": {{
//                            ""user_id"": ""{recipient.UserZaloIdByOA}""
//                        }},
//                        ""message"": {template.Message}
//                    }}";
//                    foreach (var kv in paramValues)
//                    {
//                        var keyReplace = "{" + kv.Key.Trim() + "}";
//                        content = content.Replace(keyReplace, kv.Value ?? string.Empty);
//                    }
//                    // TODO: gửi content tới recipient.UserZaloId + lưu log
//                    var paramValuesString = string.Join(", ", paramValues.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
//                    await SendUidMessage(accessToken, content, recipient, eventName, paramValuesString, template.Name);
//                }
//            }
//        }

//        public async Task SendUidMessage(string accessToken, string jsonContent, Membership recipient, string eventName, string paramValuesString, string? templateName = null)
//        {
//            try
//            {
//                if (!string.IsNullOrEmpty(accessToken))
//                {
//                    var client = new HttpClient();
//                    var url = "https://openapi.zalo.me/v3.0/oa/message/transaction";
//                    var request = new HttpRequestMessage(HttpMethod.Post, url)
//                    {
//                        Headers = { { "access_token", accessToken } },
//                        Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
//                    };

//                    var response = await client.SendAsync(request);
//                    var responseString = await response.Content.ReadAsStringAsync();
//                    var zaloMessageResponse = JsonConvert.DeserializeObject<ZaloDataResponse>(responseString);

//                    await SaveLog(recipient.UserZaloId, jsonContent, responseString, zaloMessageResponse.error);

//                    // gửi alert cảnh báo về cho telegram
//                    var appName = $"{configuration["NotificationSetting:AppInfo:AppName"]} {configuration["NotificationSetting:AppInfo:Version"]}";
//                    var alertContent =
//                     $"🚨 *{appName}* 🚨\n" +
//                     $"📌 *Status:* {(zaloMessageResponse?.error != 0 ? "❌ Thất bại" : "✅ Thành công")}\n" +
//                     $"📣 *Event:* {eventName}\n" +
//                     $"📄 *Mẫu UID*: {templateName}\n" +
//                     $"👤 *Recipient:* {recipient.UserZaloName} (UID: {recipient.UserZaloIdByOA})\n" +
//                    //$"📤 *Payload:* {jsonContent}\n" + // uncomment nếu cần debug kỹ hơn
//                     $"📦 *List Params:* {paramValuesString}\n" +
//                     $"📥 *HTTP Status:* {(int)response.StatusCode} - {response.StatusCode}\n" +
//                     $"💬 *Response:* {responseString}\n" +
//                     $"🕒 *Time:* {DateTime.Now:dd-MM-yyyy HH:mm:ss}";

//                    await SendTelegramBot.SendMessageTelegramBot(configuration, alertContent);
//                    return;
//                }
//            }
//            catch (Exception ex)
//            {
//                string logFilePath = "send_catch.log";
//                string logContent = $"[{DateTime.Now}] catch: {ex.Message}{Environment.NewLine}";
//                await File.AppendAllTextAsync(logFilePath, logContent);
//                logger.LogError(ex.Message);
//            }
//        }

//        private async Task SaveLog(string recipient, string requestString, string responseString, int? statusCode, string type = "uid", string metadata = "")
//        {
//            // Tạo đối tượng NotificationLog
//            var log = new EventTemplateLog()
//            {
//                Type = type,
//                Metadata = metadata,
//                Recipient = recipient,
//                RequestBody = requestString,
//                ResponseBody = responseString,
//                ResultCode = $"{statusCode}",
//            };

//            // Lưu vào database
//            _eventTemplateLogRepo.Add(log);
//            var result = await unitOfWork.SaveChangesAsync();
//            Console.WriteLine($"Số bản ghi được lưu khi chạy job: {result}");
//        }

//        #endregion

//        #region Mapping Value Async 

//        private async Task<Dictionary<string, string>> MapTemplateParamsAsync(
//                List<MappingParams> paramConfigs,
//                Membership recipient,
//                Dictionary<string, string> membershipExtendData,
//                Func<Task<CampaignItem?>>? getCampaignItemFunc = null)
//        {
//            var paramValues = new Dictionary<string, string>();

//            foreach (var config in paramConfigs)
//            {
//                if (string.IsNullOrEmpty(config.ParamName))
//                    continue;

//                object value = config.DefaultValue ?? string.Empty;

//                switch (config.MappingTableName)
//                {
//                    case "Membership":
//                        var property = typeof(Membership).GetProperty(config.MappingColumnName);
//                        value = property?.GetValue(recipient)?.ToString() ?? value;
//                        break;

//                    case "MembershipExtend":
//                        if (membershipExtendData.TryGetValue(config.ParamName, out var extendValue) && !string.IsNullOrEmpty(extendValue))
//                            value = extendValue;
//                        break;

//                    case "CampaignItem":
//                        if (getCampaignItemFunc != null)
//                        {
//                            var campaignItem = await getCampaignItemFunc();
//                            if (campaignItem != null)
//                            {
//                                var prop = typeof(CampaignItem).GetProperty(config.MappingColumnName);
//                                value = prop?.GetValue(campaignItem)?.ToString() ?? value;

//                                campaignItem.IsUsed = true;
//                                _campaignItemRepo.Update(campaignItem);
//                                await unitOfWork.SaveChangesAsync();
//                            }
//                        }
//                        break;
//                }

//                paramValues[config.ParamName] = $"{value}";
//            }

//            return paramValues;
//        }

//        #endregion

//        #region Helpers Check Condition

//        private bool CheckCondition(string conditions, object data)
//        {

//            // Nếu không có điều kiện => luôn đúng
//            if (string.IsNullOrWhiteSpace(conditions))
//                return true;

//            // Tách chuỗi điều kiện theo từ khóa 'or' thành các nhóm điều kiện
//            var orParts = conditions.Split([" or "], StringSplitOptions.RemoveEmptyEntries);

//            foreach (var orPart in orParts)
//            {
//                // Với mỗi nhóm 'or', tiếp tục tách theo 'and'
//                var andParts = orPart.Split([" and "], StringSplitOptions.RemoveEmptyEntries);
//                bool allAndTrue = true;

//                foreach (var condition in andParts)
//                {
//                    // Gọi hàm kiểm tra điều kiện đơn (ví dụ: "status = 1")
//                    var result = EvaluateSingleCondition(condition.Trim(), data);

//                    // Nếu 1 điều kiện trong nhóm 'and' sai => cả nhóm sai
//                    if (!result)
//                    {
//                        allAndTrue = false;
//                        break;
//                    }
//                }

//                // Nếu 1 nhóm 'or' đúng hoàn toàn => trả về true
//                if (allAndTrue)
//                    return true;
//            }

//            // Nếu không có nhóm nào đúng => trả về false
//            return false;
//        }

//        private bool EvaluateSingleCondition(string condition, object data)
//        {
//            string[] operators = new[] { "!=", ">=", "<=", ">", "<", "=", "startsWith", "contains", "in" };

//            foreach (var op in operators.OrderByDescending(o => o.Length)) // dài trước để tránh nhầm
//            {
//                int opIndex = condition.IndexOf(op, StringComparison.OrdinalIgnoreCase);
//                if (opIndex > 0)
//                {
//                    var left = condition.Substring(0, opIndex).Trim();
//                    var right = condition.Substring(opIndex + op.Length).Trim().Trim('\"', '\'');
//                    var actualValue = GetNestedPropertyValue(data, left)?.ToString() ?? "";

//                    switch (op.ToLower())
//                    {
//                        case "=": return actualValue == right;
//                        case "!=": return actualValue != right;
//                        case ">": return double.TryParse(actualValue, out var av1) && double.TryParse(right, out var rv1) && av1 > rv1;
//                        case "<": return double.TryParse(actualValue, out var av2) && double.TryParse(right, out var rv2) && av2 < rv2;
//                        case "startsWith": return actualValue.StartsWith(right, StringComparison.OrdinalIgnoreCase);
//                        case "contains": return actualValue.IndexOf(right, StringComparison.OrdinalIgnoreCase) >= 0;
//                        case "in":
//                            var options = Regex.Split(right, @"[;|]", RegexOptions.IgnoreCase)
//                                               .Select(o => o.Trim())
//                                               .Where(o => !string.IsNullOrEmpty(o));
//                            return options.Contains(actualValue);
//                    }
//                }
//            }

//            return false;
//        }

//        private object? GetNestedPropertyValue(object? obj, string path)
//        {
//            if (obj == null || string.IsNullOrWhiteSpace(path))
//                return null;

//            var parts = path.Split('.');

//            foreach (var part in parts)
//            {
//                if (obj == null) return null;

//                var prop = obj.GetType().GetProperties()
//                       .FirstOrDefault(p => string.Equals(p.Name, part, StringComparison.OrdinalIgnoreCase));
//                if (prop == null) return null;

//                obj = prop.GetValue(obj);
//            }
//            return obj;
//        }

//        #endregion

//        #endregion

//        #region Execute Scheduled Campaign Phone CSKH Temp

//        private readonly IRepository<WebHookLogs> _webHookLogRepo = unitOfWork.GetRepository<WebHookLogs>();
//        private readonly IRepository<CampaignTag> _campaignTagRepo = unitOfWork.GetRepository<CampaignTag>();
//        private readonly IRepository<CampaignCSKH> _campaignCSKHRepo = unitOfWork.GetRepository<CampaignCSKH>();
//        private readonly IRepository<CampaignConfig> _campaignConfigRepo = unitOfWork.GetRepository<CampaignConfig>();
//        private readonly IRepository<CampaignPhoneCSKH> _campaignPhoneCSKHRepo = unitOfWork.GetRepository<CampaignPhoneCSKH>();
//        private readonly IRepository<CampaignPhoneCSKHTemp> _campaignPhoneCSKHTempRepo = unitOfWork.GetRepository<CampaignPhoneCSKHTemp>();

//        public async Task ExecuteScheduledCampaignsAsync(string campaignId)
//        {
//            var campaign = await _campaignCSKHRepo.FindByIdAsync(campaignId);
//            if (campaign == null)
//            {
//                Console.WriteLine($"❌ [ERROR] ExecuteScheduledCampaignsAsync: CampaignId {campaignId} not found.");
//                return;
//            }

//            switch (campaign.Status)
//            {
//                case 2:
//                    Console.WriteLine($"❌ [ERROR] ExecuteScheduledCampaignsAsync: CampaignId {campaignId} is running.");
//                    return;
//                case 3:
//                    Console.WriteLine($"❌ [ERROR] ExecuteScheduledCampaignsAsync: CampaignId {campaignId} is completed.");
//                    return;
//                case 4:
//                    Console.WriteLine($"❌ [ERROR] ExecuteScheduledCampaignsAsync: CampaignId {campaignId} is not cancelled.");
//                    return;
//                default:
//                    break;
//            }

//            campaign.Status = 2; // running
//            await unitOfWork.SaveChangesAsync();

//            var jobIds = new List<string>();

//            var batchSize = configuration.GetSection("OmniTool:BatchSize").Get<int?>() ?? 200;
//            var batchPerGroup = configuration.GetSection("OmniTool:Groups").Get<int?>() ?? 10;

//            var campaignPhoneCSKHTempQuery = _campaignPhoneCSKHTempRepo.AsQueryable().Where(x => x.CampaignCSHKId == campaignId && x.Status == 0);

//            int batchCount = 0;
//            int delayInMinutes = 0;

//            // Đếm tổng số bản ghi cần xử lý
//            var totalRecords = await campaignPhoneCSKHTempQuery.CountAsync();
//            int totalBatches = (int)Math.Ceiling((double)totalRecords / batchSize);

//            for (int i = 0; i < totalBatches; i++)
//            {
//                // Chỉ load batchSize dòng mỗi lần
//                var batch = await campaignPhoneCSKHTempQuery.OrderBy(x => x.Id).Skip(i * batchSize).Take(batchSize).ToListAsync();
//                if (!batch.Any())
//                {
//                    // Ghi log lại là xử lý xong nè
//                    break;
//                }

//                // Schedule job gửi batch này với độ trễ hiện tại
//                var jobId = BackgroundJob.Schedule(() => ProcessBatchAsync(batch), TimeSpan.FromMinutes(delayInMinutes));
//                jobIds.Add(jobId);

//                batchCount++;

//                // Sau mỗi nhóm batchPerGroup, tăng độ trễ
//                if (batchCount >= batchPerGroup)
//                {
//                    delayInMinutes++;  // Tăng thời gian delay mỗi khi đủ batchPerGroup
//                    batchCount = 0;    // Reset batch count
//                }
//            }

//            // chạy đồng bộ campaign status và xóa tin nhắn ở bảng temp
//            await SyncCamphonePhoneTempToCampaignPhone(campaign);
//        }

//        public async Task ProcessBatchAsync(List<CampaignPhoneCSKHTemp> batch)
//        {
//            // Lấy tài khoản Omni để gửi tin
//            var omniAccount = await systemSettingService.GetOmniAccountAsync();
//            if (string.IsNullOrEmpty(omniAccount.Username) || string.IsNullOrEmpty(omniAccount.Password))
//            {
//                Console.WriteLine($"❌ [ERROR] ProcessBatchAsync: Omni Account is not found.");
//                // throw new Exception("Omni Account is not found");
//                return;
//            }
//            var listWebLogs = new List<WebHookLogs>();
//            var listMessagePhoneCSKH = new List<CampaignPhoneCSKH>();
//            foreach (var item in batch)
//            {
//                try
//                {
//                    // Gửi tin nhắn qua OmniService
//                    var listParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ParamContent);
//                    var omniRes = await omniService.SendOmniMessageAsync(omniAccount, item.TemplateCode, item.PhoneNumber, item.RoutingRule, listParams, new List<string>() { });

//                    var telCoId = PhoneNumberHandler.GetIdTelco(item.PhoneNumber);
//                    // Lưu log vào DB
//                    listMessagePhoneCSKH.Add(new CampaignPhoneCSKH
//                    {
//                        Status = omniRes.Status,
//                        TelcoID = telCoId,
//                        AccountId = item.AccountId,
//                        PhoneNumber = item.PhoneNumber,
//                        RoutingRule = item.RoutingRule,
//                        ParamContent = item.ParamContent,
//                        TemplateCode = item.TemplateCode,
//                        CampaignCSKHId = item.CampaignCSHKId,
//                        IdOmniMess = omniRes.IdOmniMess,
//                        ErrorCode = omniRes.Code,
//                    });

//                    listWebLogs.Add(new WebHookLogs
//                    {
//                        TelcoId = telCoId,
//                        ErrorCode = omniRes.Code,
//                        Status = omniRes.Status.ToString(),
//                        IdOmniMess = omniRes.IdOmniMess,
//                        Response = JsonConvert.SerializeObject(omniRes),
//                    });

//                    // Đánh dấu tin nhắn đã gửi thành công
//                    item.Status = omniRes.Status;
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"❌ [ERROR] ProcessBatchAsync: Lỗi khi gửi tin nhắn tới {item.PhoneNumber} - {ex.Message}");
//                    // await LogWebhookAsync(item, "Error", ex.Message);
//                }
//            }

//            // Cập nhật trạng thái batch, lưu log vào DB
//            // _webHookLogRepo.AddRange(listWebLogs);
//            _campaignPhoneCSKHTempRepo.UpdateRange(batch);

//            // sau khi chạy job thì gửi tin dựa trên temp thì chạy job đồng bộ status và xóa tin nhắn ở temp
//            _campaignPhoneCSKHRepo.AddRange(listMessagePhoneCSKH);

//            await unitOfWork.SaveChangesAsync();
//        }

//        public async Task<int> SyncCamphonePhoneTempToCampaignPhone(CampaignCSKH campaignCSKH)
//        {
//            var recordsToSync = _campaignPhoneCSKHRepo.AsQueryable().Where(c => c.Status == 1 && c.CampaignCSKHId == campaignCSKH.Id);
//            campaignCSKH.TotalSuccess = await recordsToSync.CountAsync();
//            var campaignPhoneTempToRemove = await _campaignPhoneCSKHTempRepo.AsQueryable()
//                                                .Where(c => c.CampaignCSHKId == campaignCSKH.Id && recordsToSync.Any(x => x.PhoneNumber == c.PhoneNumber))
//                                                .ToListAsync();
//            if (campaignPhoneTempToRemove.Any())
//            {
//                _campaignPhoneCSKHTempRepo.DeleteRange(campaignPhoneTempToRemove);
//            }

//            campaignCSKH.Status = 3;
//            campaignCSKH.TotalSuccess = await recordsToSync.CountAsync();
//            _campaignCSKHRepo.Update(campaignCSKH);
//            return await unitOfWork.SaveChangesAsync();
//        }

//        #endregion
//    }
//}
