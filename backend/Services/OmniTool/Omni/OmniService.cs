using MiniAppGIBA.Base.Dependencies.Cache;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.DTOs.SystemSettings;
using MiniAppGIBA.Services.OmniTool.Omni.Models;
using MiniAppGIBA.Services.SystemSettings;
using Newtonsoft.Json;
using System.Text;

namespace MiniAppGIBA.Services.OmniTool.Omni
{
    public class OmniService(ISystemSettingService systemSetting, ICacheService cacheService, IUnitOfWork unitOfWork) : IOmniService
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task<TemplateOmniResponse> GetAllOwnedTemplate()
        {
            var accountDTO = await systemSetting.GetOmniAccountAsync();
            return await GetAllOwnedTemplate(accountDTO);
        }

        public async Task<TemplateOmniResponse> GetAllOwnedTemplate(OmniAccountDTO omniAccountDTO)
        {
            var client = new HttpClient();

            if (string.IsNullOrEmpty(omniAccountDTO.Username) || string.IsNullOrEmpty(omniAccountDTO.Password))
            {
                throw new CustomException(1, "Không tìm thấy tài khoản Omni. Vui lòng cấu hình tài khoản để sử dụng!");
                //return default;
            }

            string cacheKey = $"OmniTemplate_{omniAccountDTO.Username}";
            var allCached = cacheService.GetAllKeyValues();
            var cachedData = await cacheService.GetValueAsync<TemplateOmniResponse>(cacheKey);

            if (cachedData != null && cachedData.ListTemp.Any())
            {
                return new TemplateOmniResponse { ListTemp = cachedData.ListTemp };
            }

            var payload = new TemplateOmniRequest
            {
                Username = omniAccountDTO.Username,
                Password = omniAccountDTO.Password,
                TemplateCode = string.Empty,
                Page = 1,
                ItemCount = 10
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var apiEndpoint = string.IsNullOrEmpty(omniAccountDTO.EnvUrl) ? "https://pocsite.incom.vn" : omniAccountDTO.EnvUrl;
            var response = await client.PostAsync(apiEndpoint + "/api/TemplateOmni/GetTemplate", content);

            if (!response.IsSuccessStatusCode)
            {
                return new TemplateOmniResponse()
                {
                    ListTemp = new List<TemplateResponse>() { }
                };
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<TemplateOmniResponse>(responseString) ?? new TemplateOmniResponse();
            await cacheService.SetValueAsync(cacheKey, responseObj, TimeSpan.FromMinutes(2));
            return responseObj;
        }

        public async Task<SendOmniMessgeResponse> SendOmniMessageAsync(string templateCode, string phoneNumber, string routeRule, Dictionary<string, string> listParams)
        {
            try
            {
                var omniAccountDTO = await systemSetting.GetOmniAccountAsync();
                if (string.IsNullOrEmpty(omniAccountDTO.Username) || string.IsNullOrEmpty(omniAccountDTO.Password))
                {
                    // throw new CustomException(200, "Tài khoản omni không hợp lệ");
                    return default;
                }

                var omniRequest = new OmniRequestModel
                {
                    username = omniAccountDTO.Username,
                    password = omniAccountDTO.Password,
                    phonenumber = phoneNumber,
                    routerule = routeRule.Split(",").ToList(),
                    templatecode = templateCode,
                    list_param = listParams
                };

                var json = JsonConvert.SerializeObject(omniRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiEndpoint = string.IsNullOrEmpty(omniAccountDTO.EnvUrl)
                    ? "https://pocsite.incom.vn"
                    : omniAccountDTO.EnvUrl;

                //logger.Information("Sending request to {Url} with payload: {Payload}", $"{apiEndpoint}/api/OmniMessage/SendMessage", json);

                var response = await _client.PostAsync($"{apiEndpoint}/api/OmniMessage/SendMessage", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    //_logger.Error("Failed to send message. Status: {StatusCode}, Response: {Response}", response.StatusCode, errorResponse);

                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                //_logger.Information("Received response: {Response}", responseString);

                return JsonConvert.DeserializeObject<SendOmniMessgeResponse>(responseString) ?? new SendOmniMessgeResponse();
            }
            catch (Exception)
            {
                //_logger.Error("Error in SendOmniMessageAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<SendOmniMessgeResponse> SendOmniMessageAsync(OmniAccountDTO omniAccountDTO, string templateCode, string phoneNumber, string routeRule, Dictionary<string, string> listParams, List<string> datasourceETMId)
        {
            try
            {
                if (string.IsNullOrEmpty(omniAccountDTO.Username) || string.IsNullOrEmpty(omniAccountDTO.Password))
                {
                    // throw new CustomException(200, "Tài khoản omni không hợp lệ");
                    return default;
                }

                var omniRequest = new OmniRequestModel
                {
                    username = omniAccountDTO.Username,
                    password = omniAccountDTO.Password,
                    phonenumber = phoneNumber,
                    routerule = routeRule.Split(",").ToList(),
                    templatecode = templateCode,
                    list_param = listParams
                };

                var json = JsonConvert.SerializeObject(omniRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiEndpoint = string.IsNullOrEmpty(omniAccountDTO.EnvUrl)
                    ? "https://pocsite.incom.vn"
                    : omniAccountDTO.EnvUrl;

                //logger.Information("Sending request to {Url} with payload: {Payload}", $"{apiEndpoint}/api/OmniMessage/SendMessage", json);

                var response = await _client.PostAsync($"{apiEndpoint}/api/OmniMessage/SendMessage", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    //_logger.Error("Failed to send message. Status: {StatusCode}, Response: {Response}", response.StatusCode, errorResponse);
                    await HandleStatusDatasourceETM(datasourceETMId, false);
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                //_logger.Information("Received response: {Response}", responseString);
                var rs = JsonConvert.DeserializeObject<SendOmniMessgeResponse>(responseString) ?? new SendOmniMessgeResponse();
                if (rs.Status == 1)
                {
                    await HandleStatusDatasourceETM(datasourceETMId, true);
                }
                else
                {
                    await HandleStatusDatasourceETM(datasourceETMId, false);
                }
                return JsonConvert.DeserializeObject<SendOmniMessgeResponse>(responseString) ?? new SendOmniMessgeResponse();
            }
            catch (Exception)
            {
                //_logger.Error("Error in SendOmniMessageAsync: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<int> HandleStatusDatasourceETM(List<string> datasourceETMId, bool isSuccess = true)
        {
            int result = 0;
            if (datasourceETMId.Count > 0)
            {
                try
                {
                    IRepository<DatasourceETM> _datasourceETMRep = unitOfWork.GetRepository<DatasourceETM>();

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
    }
}
