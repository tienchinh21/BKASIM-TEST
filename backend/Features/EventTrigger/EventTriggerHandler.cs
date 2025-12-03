using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Dependencies.Zalo;
using MiniAppGIBA.Base.Helpers;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.Notifications;
using MiniAppGIBA.Entities.Notifications.Templates;
using MiniAppGIBA.Models;
using MiniAppGIBA.Models.DTOs.SystemSettings;
using MiniAppGIBA.Models.Payload;
using MiniAppGIBA.Services.OmniTool.Omni;
using MiniAppGIBA.Services.OmniTool.TokenManager;
//using MiniAppGIBA.Services.Orders;
using MiniAppGIBA.Services.SystemSettings;
using Newtonsoft.Json;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniAppGIBA.Features.EventTrigger
{
    public class EmitEventArgs : IRequest<int>
    {
        public required string EventName { get; set; }

        public string? TriggerZaloIdByOA { get; set; }
        public string? TriggerPhoneNumber { get; set; }

        public required object Payload { get; set; }
    }

    public class FunctionCalling
    {
        public string FunctionName { get; set; } = string.Empty;
        public string OutputField { get; set; } = string.Empty;
        public List<object> Parameters { get; set; } = new List<object>();
    }

    public class EventTriggerHandler(IUnitOfWork unitOfWork, IOmniService omniService, ITokenManagerService tokenManagerService, ISystemSettingService systemSetting) : IRequestHandler<EmitEventArgs, int>
    {
        private readonly IRepository<EventTriggerSetting> _eventTriggerSettingRepository = unitOfWork.GetRepository<EventTriggerSetting>();

        private readonly IRepository<OmniTemplate> _omniTemplateRepo = unitOfWork.GetRepository<OmniTemplate>();
        private readonly IRepository<Entities.ETM.ZaloTemplateUid> _zaloTemplateUidRepo = unitOfWork.GetRepository<Entities.ETM.ZaloTemplateUid>();
        private readonly IRepository<DatasourceETM> _datasourceETMRep = unitOfWork.GetRepository<DatasourceETM>();

        public async Task<int> Handle(EmitEventArgs request, CancellationToken cancellationToken)
        {
            // Danh sách các event setting đang hoạt động
            var eventTriggerSetting = await _eventTriggerSettingRepository.AsQueryable().Where(x => x.EventName == request.EventName && x.IsActive).ToListAsync();

            var httpConfigIDs = eventTriggerSetting.Where(x => x.Type == 3).Select(x => x.ReferenceId).ToList();
            var zaloTemplateUid = eventTriggerSetting.Where(x => x.Type == 1).Select(x => x.ReferenceId).ToList();
            var omniTemplateIDs = eventTriggerSetting.Where(x => x.Type == 2).Select(x => x.ReferenceId).ToList();

            var omniTemplates = await _omniTemplateRepo.AsQueryable().Where(x => omniTemplateIDs.Contains(x.Id)).ToListAsync();


            var zaloTemplateConfig = await unitOfWork.GetRepository<Entities.ETM.ZaloTemplateConfig>()
                                                     .AsQueryable()
                                                     .Where(x => zaloTemplateUid.Contains(x.Id))
                                                     .ToListAsync();

            var httpConfig = await unitOfWork.GetRepository<HttpConfig>()
                                                     .AsQueryable()
                                                     .Where(x => httpConfigIDs.Contains(x.Id))
                                                     .ToListAsync();

            var zaloTemplateConfigIds = zaloTemplateConfig.Select(x => x.TemplateId).ToList();
            var zaloTemplates = await _zaloTemplateUidRepo.AsQueryable().Where(x => zaloTemplateConfigIds.Contains(x.Id)).ToListAsync();

            var totalSent = 0;
            var accountDto = await systemSetting.GetOmniAccountAsync();
            // Xử lý từng event setting
            foreach (var setting in eventTriggerSetting)
            {
                #region Handle data & conditions

                // 0. Kiểm tra điều kiện trước khi xử lý
                if (!string.IsNullOrWhiteSpace(setting.Conditions))
                {
                    var conditionMet = EvaluateCondition(setting.Conditions, request.Payload);
                    if (!conditionMet)
                    {
                        continue; // Bỏ qua setting này nếu điều kiện không thỏa mãn
                    }
                }

                // 1. Lấy đúng template và chuỗi JSON
                string? json = setting.Type switch
                {
                    1 => zaloTemplateConfig
                           .FirstOrDefault(x => x.Id == setting.ReferenceId)
                           ?.TemplateMapping,
                    2 => omniTemplates
                           .FirstOrDefault(x => x.Id == setting.ReferenceId)
                           ?.TemplateMapping,
                    _ => null
                };


                string routingRule = setting.Type switch
                {
                    2 => omniTemplates
                           .FirstOrDefault(x => x.Id == setting.ReferenceId)
                           ?.RoutingRule ?? string.Empty,
                    _ => string.Empty
                };

                string templateCode = setting.Type switch
                {
                    1 => zaloTemplateConfig
                              .FirstOrDefault(x => x.Id == setting.ReferenceId)
                              ?.TemplateId,
                    2 => omniTemplates
                           .FirstOrDefault(x => x.Id == setting.ReferenceId)
                           ?.TemplateCode,
                    _ => null
                } ?? string.Empty;

                // 2. Deserialize về List<MappingParams>
                var paramsConfig = !string.IsNullOrWhiteSpace(json)
                    ? JsonConvert.DeserializeObject<List<MappingParams>>(json)
                    : new List<MappingParams>();

                var mappedData = new Dictionary<string, string>(paramsConfig!.Count);
                var payloadDict = ConvertToDictionary(request.Payload);
                List<string> datasourceETMId = new List<string>();

                if (payloadDict != null && payloadDict.Any())
                {
                    foreach (var param in paramsConfig)
                    {
                        // Khởi tạo từ default value
                        string current = param.DefaultValue?.ToString() ?? string.Empty;

                        // Lookup trực tiếp trong payloadDict thay vì reflection
                        var mappingKey = param.MappingColumnName ?? string.Empty;
                        if (payloadDict.ContainsKey(mappingKey))
                        {
                            current = payloadDict[mappingKey] ?? string.Empty;
                        }
                        else
                        {
                            var parts = param.MappingColumnName?.Split('.', StringSplitOptions.RemoveEmptyEntries);
                            if (parts != null && parts.Length == 2)
                            {
                                var code = parts[0];
                                var key = parts[1];

                                // Truy vấn DatasourceETM
                                var dsValue = await _datasourceETMRep.AsQueryable()
                                    .Where(x => x.Code == code && x.Key == key && !x.IsUsed)
                                    .FirstOrDefaultAsync();

                                if (dsValue != null)
                                {
                                    datasourceETMId.Add(dsValue.Id);
                                    current = dsValue.Value;
                                    _datasourceETMRep.Update(dsValue);
                                    await unitOfWork.SaveChangesAsync();
                                }
                            }
                        }

                        // Gán vào dictionary
                        mappedData[param.ParamName!] = current;
                    }
                }
                else
                {
                    // Nếu payloadDict null hoặc rỗng, gán toàn bộ default
                    foreach (var param in paramsConfig)
                        mappedData[param.ParamName!] = param.DefaultValue?.ToString() ?? string.Empty;
                }

                /*
                
                var payload = request.Payload;
                if (payload != null)
                {
                    var payloadType = payload.GetType();
                    var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

                    foreach (var param in paramsConfig)
                    {
                        // khởi tạo luôn dưới dạng string
                        string current = param.DefaultValue?.ToString() ?? string.Empty;

                        // thử đọc property
                        var prop = payloadType.GetProperty(param.MappingColumnName ?? "", flags);
                        if (prop != null)
                        {
                            var raw = prop.GetValue(payload);
                            if (raw != null)
                                current = raw.ToString() ?? string.Empty;
                        }

                        // gán vào dictionary
                        mappedData[param.ParamName ?? string.Empty] = current;
                    }
                }
                else
                {
                    // nếu payload null thì gán toàn bộ về default
                    foreach (var param in paramsConfig)
                        mappedData[param.ParamName!] = param.DefaultValue?.ToString() ?? string.Empty;
                } 
                
                */

                // function calling pre-processing mappedData
                mappedData = ProcessingData(setting.ProcessingStep ?? string.Empty, mappedData);

                #endregion

                #region Handle recipient

                totalSent++;
                if (setting.Type == 1)
                {
                    // gửi Zalo template
                    // --- Gửi notification ---
                    var allRecipients = ParseRecipientsUserZaloIdByOa(setting.Recipients);

                    if (!string.IsNullOrWhiteSpace(request.TriggerZaloIdByOA))
                        allRecipients.Add(request.TriggerZaloIdByOA);

                    var templateContent = zaloTemplates
                        .FirstOrDefault(x => x.Id == templateCode)
                        ?.Message ?? string.Empty;

                    allRecipients = allRecipients
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Distinct()
                        .ToList();
                    foreach (var to in allRecipients)
                    {
                        BackgroundJob.Enqueue<EventTriggerHandler>(x => x.SendZaloUidMessage(templateCode, templateContent, to, mappedData, datasourceETMId));
                    }
                }
                else if (setting.Type == 2)
                {
                    // gửi Omni message
                    var recipients = ParseRecipientsPhoneNumber(setting.Recipients);

                    // luôn thêm người kích hoạt (phone)
                    if (!string.IsNullOrWhiteSpace(request.TriggerPhoneNumber))
                        recipients.Add(PhoneNumberHandler.FixFormatPhoneNumber(request.TriggerPhoneNumber)!);

                    // loại bỏ trùng
                    var allRecipients = recipients
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Distinct()
                        .ToList();

                    // --- Gửi notification ---
                    foreach (var to in allRecipients)
                    {
                        BackgroundJob.Enqueue<EventTriggerHandler>(x => x.SendOmniMessage(accountDto, templateCode, to, routingRule, mappedData, datasourceETMId));
                    }
                }
                else if (setting.Type == 3)
                {
                    // function calling pre-processing payloadDict
                    var processedPayload = ProcessingData(setting.ProcessingStep ?? string.Empty, payloadDict);

                    // Xử lý HTTP config: Lấy HttpConfig tương ứng và enqueue job gửi request
                    var httpConfigItem = httpConfig.FirstOrDefault(x => x.Id == setting.ReferenceId);
                    if (httpConfigItem != null)
                    {
                        BackgroundJob.Enqueue<EventTriggerHandler>(x => x.SendHttpRequest(httpConfigItem, processedPayload, datasourceETMId));
                    }
                }

                #endregion
            }

            return totalSent;
        }

        private List<string> ParseRecipientsPhoneNumber(string recipients)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(recipients))
                return result.ToList();
            var parts = recipients
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0);
            foreach (var part in parts)
            {
                if (PhoneNumberHandler.IsValidPhoneNumber(part))
                {
                    var formatted = PhoneNumberHandler.FixFormatPhoneNumber(part);
                    if (!string.IsNullOrWhiteSpace(formatted))
                    {
                        result.Add(formatted);
                    }
                }
            }
            return result.ToList();
        }

        private List<string> ParseRecipientsUserZaloIdByOa(string recipients)
        {
            if (string.IsNullOrWhiteSpace(recipients))
                return new List<string>();

            // 1. Tách, trim, loại bỏ các giá trị rỗng hoặc "trigger"
            var parts = recipients
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x)
                            && !x.Equals("trigger", StringComparison.OrdinalIgnoreCase)
                            && !x.Equals("receiver", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 2. Query lên DB: lọc theo parts, bỏ null/empty, DISTINCT đơn giản
            var interimList = unitOfWork.GetRepository<Membership>()
                .AsQueryable()
                .Where(x => parts.Contains(x.Id))
                .Select(x => x.UserZaloIdByOA)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()        // đây là SQL DISTINCT
                .ToList();         // lên DB rồi mới materialize

            // 3. Nếu cần distinct case-insensitive, xử lý tiếp ở client:
            var finalList = interimList
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<string>()
                .ToList();

            return finalList;
        }

        #region "Send Message Method & Save log"

        public async Task SendOmniMessage(OmniAccountDTO omniAccount, string templateCode, string phoneNumber, string routingRule, Dictionary<string, string> paramValues, List<string> datasourceETMId)
        {
            var response = await omniService.SendOmniMessageAsync(
                        omniAccount,
                        templateCode,
                        phoneNumber,
                        routingRule,
                        paramValues,
                        datasourceETMId);

            var metadata = $"{JsonConvert.SerializeObject(new
            {
                EventType = "StartTrip",
                IdOMniMess = response.IdOmniMess,
                TelcoId = PhoneNumberHandler.GetIdTelco(phoneNumber),
                Status = response.Status.ToString(),
            })}";

            var requestString = $"{phoneNumber} - {templateCode} - {string.Join(",", routingRule)} - {JsonConvert.SerializeObject(paramValues)}";
            var responseString = JsonConvert.SerializeObject(response);
            await SaveLog(phoneNumber, requestString, responseString, response.Status, "2", metadata);
        }

        public async Task SendZaloUidMessage(string templateId, string templateContent, string userZaloIdByOA, Dictionary<string, string> paramValues, List<string> datasourceETMId)
        {
            var accessToken = await tokenManagerService.GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }
            // replace nội dung template với các biến
            var content = $@"{{ 
                        ""recipient"": {{
                            ""user_id"": ""{userZaloIdByOA}""
                        }},
                        ""message"": {templateContent}
                    }}";
            foreach (var kv in paramValues)
            {
                var keyReplace = "{" + kv.Key.Trim() + "}";
                content = content.Replace(keyReplace, kv.Value ?? string.Empty);
            }
            // TODO: gửi content tới recipient.UserZaloId + lưu log
            var paramValuesString = string.Join(", ", paramValues.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

            var client = new HttpClient();
            var url = "https://openapi.zalo.me/v3.0/oa/message/transaction";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers = { { "access_token", accessToken } },
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                await HandleStatusDatasourceETM(datasourceETMId, false);
                await SaveLog(userZaloIdByOA, content, errorBody, (int)response.StatusCode, "1", "HTTP request failed");
                return;
            }
            var responseString = await response.Content.ReadAsStringAsync();
            var zaloMessageResponse = JsonConvert.DeserializeObject<ZaloDataResponse>(responseString);
            if (zaloMessageResponse?.error == 0)
            {
                await HandleStatusDatasourceETM(datasourceETMId, true);
            }
            else
            {
                await HandleStatusDatasourceETM(datasourceETMId, false);
            }

            var requestString = $"{userZaloIdByOA} - {templateId} - {JsonConvert.SerializeObject(paramValues)}";
            await SaveLog(userZaloIdByOA, content, responseString, zaloMessageResponse?.error, "1", requestString);
        }

        public async Task SendHttpRequest(HttpConfig httpConfig, Dictionary<string, string> paramValues, List<string> datasourceETMId)
        {
            try
            {
                // Thay thế placeholders trong BodyJson và HeadersJson
                string bodyJson = httpConfig.BodyJson ?? string.Empty;
                string headersJson = httpConfig.HeadersJson ?? string.Empty;

                foreach (var kv in paramValues)
                {
                    var placeholder = "{" + kv.Key.Trim() + "}";
                    bodyJson = bodyJson.Replace(placeholder, kv.Value ?? string.Empty);
                    headersJson = headersJson.Replace(placeholder, kv.Value ?? string.Empty);
                }

                // Deserialize headers nếu có
                var headers = !string.IsNullOrWhiteSpace(headersJson)
                    ? JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson)
                    : new Dictionary<string, string>();

                // Xây dựng HttpRequestMessage
                var client = new HttpClient();
                var request = new HttpRequestMessage(new HttpMethod(httpConfig.Method ?? "GET"), httpConfig.Endpoint);

                // Thêm headers
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                // Thêm body nếu method không phải GET/HEAD
                if (!string.IsNullOrWhiteSpace(bodyJson) && !new[] { "GET", "HEAD" }.Contains(httpConfig.Method?.ToUpper()))
                {
                    request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                }

                // Gửi request
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await HandleStatusDatasourceETM(datasourceETMId, false);
                }
                // Chuẩn bị log
                var requestString = $"{httpConfig.Method} {httpConfig.Endpoint} - Headers: {headersJson} - Body: {bodyJson}";
                var statusCode = (int)response.StatusCode;
                await SaveLog(httpConfig.Endpoint ?? "N/A", requestString, responseString, statusCode, "3", "");
            }
            catch (Exception ex)
            {
                // Log lỗi nếu gửi thất bại
                var errorMessage = $"Error sending HTTP request: {ex.Message}";
                await SaveLog(httpConfig.Endpoint ?? "N/A", "N/A", errorMessage, -1, "3", "");
            }
        }

        private async Task SaveLog(string recipient, string requestString, string responseString, int? statusCode, string type = "2", string metadata = "")
        {
            var log = new EventTriggerLog()
            {
                Type = type,
                Metadata = metadata,
                Recipient = recipient,
                RequestBody = requestString,
                ResponseBody = responseString,
                ResultCode = $"{statusCode}",
            };

            unitOfWork.GetRepository<EventTriggerLog>().Add(log);
            var result = await unitOfWork.SaveChangesAsync();
            Console.WriteLine($"Số bản ghi được lưu khi chạy job: {result}");
        }

        #endregion

        #region Processing Condition

        /*
         
        private bool EvaluateCondition(string condition, object payload)
        {
            if (string.IsNullOrWhiteSpace(condition) || payload == null)
            {
                return true;
            }

            try
            {
                var payloadType = payload.GetType();
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

                // Tách các điều kiện bằng OR
                var orConditions = condition.Split(new[] { " or ", " OR " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var orCondition in orConditions)
                {
                    var andConditions = orCondition.Split(new[] { " and ", " AND " }, StringSplitOptions.RemoveEmptyEntries);
                    bool allAndConditionsMet = true;

                    foreach (var andCondition in andConditions)
                    {
                        var trimmedCondition = andCondition.Trim();

                        // Phân tích điều kiện đơn giản: PropertyName = Value hoặc PropertyName != Value
                        if (trimmedCondition.Contains("!="))
                        {
                            var parts = trimmedCondition.Split("!=", 2);
                            if (parts.Length == 2)
                            {
                                var propName = parts[0].Trim();
                                var expectedValue = parts[1].Trim();

                                var prop = payloadType.GetProperty(propName, flags);
                                if (prop != null)
                                {
                                    var actualValue = prop.GetValue(payload)?.ToString() ?? string.Empty;
                                    if (actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        allAndConditionsMet = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (trimmedCondition.Contains("="))
                        {
                            var parts = trimmedCondition.Split("=", 2);
                            if (parts.Length == 2)
                            {
                                var propName = parts[0].Trim();
                                var expectedValue = parts[1].Trim();

                                var prop = payloadType.GetProperty(propName, flags);
                                if (prop != null)
                                {
                                    var actualValue = prop.GetValue(payload)?.ToString() ?? string.Empty;
                                    if (!actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        allAndConditionsMet = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    // Nếu không tìm thấy property, coi như điều kiện không thỏa mãn
                                    allAndConditionsMet = false;
                                    break;
                                }
                            }
                        }
                    }

                    // Nếu tất cả AND conditions trong một OR condition được thỏa mãn
                    if (allAndConditionsMet)
                    {
                        return true;
                    }
                }

                // Nếu không có OR condition nào được thỏa mãn
                return false;
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error evaluating condition '{condition}': {ex.Message}");
                return false;
            }
        }
        
         
        */
        private async Task<int> HandleStatusDatasourceETM(List<string> datasourceETMId, bool isSuccess = true)
        {
            int result = 0;
            if (datasourceETMId.Count > 0)
            {
                try
                {

                    var listDt = _datasourceETMRep.AsQueryable().Where(x => datasourceETMId.Contains(x.Id)).ToList();
                    foreach (var item in listDt)
                    {
                        item.IsUsed = isSuccess;
                    }
                    _datasourceETMRep.UpdateRange(listDt);
                    result = await unitOfWork.SaveChangesAsync();

                }
                catch (Exception e)
                {

                }
            }

            return result;
        }

        private bool EvaluateCondition(string condition, object payload)
        {
            if (string.IsNullOrWhiteSpace(condition)) return true;

            // Tách điều kiện bằng "or" (case-insensitive)
            var orConditions = condition.Split(new[] { " or " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var orCond in orConditions)
            {
                // Tách điều kiện con bằng "and" (case-insensitive)
                var andConditions = orCond.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
                bool andResult = true;
                foreach (var cond in andConditions)
                {
                    if (!EvaluateSingleCondition(cond.Trim(), payload))
                    {
                        andResult = false;
                        break;
                    }
                }
                if (andResult) return true; // Nếu bất kỳ nhóm AND nào đúng, trả về true
            }
            return false;
        }

        private bool EvaluateSingleCondition(string condition, object payload)
        {
            // Regex để tách field, operator, value (hỗ trợ Contains, StartsWith, EndsWith)
            var match = Regex.Match(condition, @"^(.+?)\s*(=|!=|>|<|>=|<=|Contains|StartsWith|EndsWith)\s*(.+)$");
            if (!match.Success) return false;

            string field = match.Groups[1].Value.Trim();
            string op = match.Groups[2].Value.Trim();
            string value = match.Groups[3].Value.Trim().Trim('\''); // Loại bỏ dấu nháy đơn nếu có

            // Dùng reflection để lấy giá trị từ payload
            var property = payload.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return false;

            var fieldValue = property.GetValue(payload);
            if (fieldValue == null) return op == "!="; // Nếu null, chỉ đúng với !=

            // Xác định kiểu và so sánh
            if (op == "Contains" || op == "StartsWith" || op == "EndsWith")
            {
                if (fieldValue is string strValue)
                {
                    return op switch
                    {
                        "Contains" => strValue.Contains(value, StringComparison.OrdinalIgnoreCase),
                        "StartsWith" => strValue.StartsWith(value, StringComparison.OrdinalIgnoreCase),
                        "EndsWith" => strValue.EndsWith(value, StringComparison.OrdinalIgnoreCase),
                        _ => false
                    };
                }
                return false;
            }

            // So sánh số hoặc ngày
            if (decimal.TryParse(value, out decimal numValue))
            {
                if (fieldValue is decimal decField) return CompareNumbers(decField, numValue, op);
                if (fieldValue is int intField) return CompareNumbers(intField, numValue, op);
                if (fieldValue is short shortField) return CompareNumbers(shortField, numValue, op);
            }
            else if (DateTime.TryParse(value, out DateTime dateValue))
            {
                if (fieldValue is DateTime dateField) return CompareDates(dateField, dateValue, op);
                if (fieldValue is string strField && DateTime.TryParse(strField, out DateTime parsedDate)) return CompareDates(parsedDate, dateValue, op);
            }

            // So sánh string mặc định (cho =, !=)
            string strFieldValue = fieldValue.ToString();
            return op switch
            {
                "=" => strFieldValue.Equals(value, StringComparison.OrdinalIgnoreCase),
                "!=" => !strFieldValue.Equals(value, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        private bool CompareNumbers(decimal fieldValue, decimal value, string op)
        {
            return op switch
            {
                "=" => fieldValue == value,
                "!=" => fieldValue != value,
                ">" => fieldValue > value,
                "<" => fieldValue < value,
                ">=" => fieldValue >= value,
                "<=" => fieldValue <= value,
                _ => false
            };
        }

        private bool CompareDates(DateTime fieldValue, DateTime value, string op)
        {
            return op switch
            {
                "=" => fieldValue.Date == value.Date,
                "!=" => fieldValue.Date != value.Date,
                ">" => fieldValue > value,
                "<" => fieldValue < value,
                ">=" => fieldValue >= value,
                "<=" => fieldValue <= value,
                _ => false
            };
        }

        #endregion

        #region "Process Data"

        private Dictionary<string, string> ConvertToDictionary(object obj)
        {
            var result = new Dictionary<string, string>();
            if (obj == null)
                return result;

            FlattenObject(obj, result, string.Empty);
            return result;
        }

        private void FlattenObject(object obj, Dictionary<string, string> result, string prefix)
        {
            if (obj == null)
                return;

            var properties = obj.GetType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.CanRead);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

                if (value == null)
                {
                    result[key] = string.Empty;
                    continue;
                }

                var valueType = value.GetType();

                // Nếu là Dictionary<string, string> thì flatten từng phần tử
                if (valueType.IsGenericType &&
                    valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                    valueType.GetGenericArguments()[0] == typeof(string) &&
                    valueType.GetGenericArguments()[1] == typeof(string))
                {
                    var dict = (Dictionary<string, string>)value;
                    foreach (var kv in dict)
                    {
                        result[$"{key}.{kv.Key}"] = kv.Value ?? string.Empty;
                    }
                }
                // Nếu là primitive, string, hoặc enum, convert thành string
                else if (valueType.IsPrimitive || valueType == typeof(string) || valueType.IsEnum)
                {
                    result[key] = value.ToString() ?? string.Empty;
                }
                // Nếu là DateTime hoặc các kiểu có thể convert
                else if (valueType == typeof(DateTime) || valueType == typeof(DateTimeOffset))
                {
                    result[key] = value.ToString();
                }
                // Nếu là object, đệ quy
                else if (!valueType.IsGenericType && !valueType.IsArray)
                {
                    FlattenObject(value, result, key);
                }
                // Nếu là collection hoặc array, xử lý từng phần tử
                else if (valueType.IsArray || (valueType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(valueType) && valueType != typeof(string)))
                {
                    var enumerable = value as IEnumerable;
                    if (enumerable != null)
                    {
                        int index = 0;
                        foreach (var item in enumerable)
                        {
                            var itemKey = $"{key}[{index}]";
                            if (item == null)
                            {
                                result[itemKey] = string.Empty;
                            }
                            else
                            {
                                var itemType = item.GetType();
                                if (itemType.IsPrimitive || itemType == typeof(string) || itemType.IsEnum)
                                {
                                    result[itemKey] = item.ToString() ?? string.Empty;
                                }
                                else if (itemType == typeof(DateTime) || itemType == typeof(DateTimeOffset))
                                {
                                    result[itemKey] = item.ToString();
                                }
                                else
                                {
                                    // Nếu item là object, đệ quy
                                    FlattenObject(item, result, itemKey);
                                }
                            }
                            index++;
                        }
                    }
                }
                // Trường hợp khác (nếu có), serialize JSON
                else
                {
                    result[key] = JsonConvert.SerializeObject(value);
                }
            }
        }

        private Dictionary<string, string> ProcessingData(string processingStep, Dictionary<string, string> rawData)
        {
            if (string.IsNullOrWhiteSpace(processingStep))
            {
                return rawData;  // Trả về nguyên bản nếu không có processingStep
            }

            try
            {
                var functionsCalling = JsonConvert.DeserializeObject<List<FunctionCalling>>(processingStep);
                if (functionsCalling == null || !functionsCalling.Any())
                {
                    return rawData;
                }

                foreach (var func in functionsCalling)
                {
                    switch (func.FunctionName.ToLower())
                    {
                        case "concat":
                            if (func.Parameters.Count > 0)
                            {
                                var parts = func.Parameters.Select(p =>
                                {
                                    if (p is string paramStr && rawData.ContainsKey(paramStr))
                                    {
                                        return rawData[paramStr] ?? string.Empty;  // Lấy từ rawData nếu là key
                                    }
                                    return p?.ToString() ?? string.Empty;  // Nếu không phải key, dùng literal
                                }).ToArray();
                                var result = string.Join("", parts);
                                rawData[func.OutputField] = result;
                            }
                            break;

                        case "split":
                            if (func.Parameters.Count >= 3 &&
                                func.Parameters[0] is string field &&
                                func.Parameters[1] is string delimiter &&
                                func.Parameters[2] is int index &&
                                rawData.ContainsKey(field))
                            {
                                var value = rawData[field] ?? string.Empty;
                                var parts = value.Split(new[] { delimiter }, StringSplitOptions.None);
                                if (index >= 0 && index < parts.Length)
                                {
                                    rawData[func.OutputField] = parts[index];
                                }
                                else
                                {
                                    rawData[func.OutputField] = JsonConvert.SerializeObject(parts);  // Nếu index không hợp lệ, gán empty
                                }
                            }
                            break;

                        case "formatnumeric":
                            if (func.Parameters.Count >= 1 &&
                                func.Parameters[0] is string _field &&
                                rawData.ContainsKey(_field))
                            {
                                var value = rawData[_field] ?? string.Empty;
                                if (decimal.TryParse(value, out decimal num))
                                {
                                    rawData[func.OutputField] = $"{num:N0}";  // Format số với dấu phẩy và thêm " VND"
                                }
                                else
                                {
                                    rawData[func.OutputField] = value;  // Nếu không phải số, giữ nguyên
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }

            return rawData;
        }

        #endregion
    }
}
