using AutoMapper;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Models.Requests.OmniTools;
using Newtonsoft.Json;

namespace MiniAppGIBA.Services.OmniTool.EventTemplates
{
    public class EventTemplateService(IUnitOfWork unitOfWork, IMapper mapper) : Service<EventTemplate>(unitOfWork), IEventTemplateService
    {

        #region Zalo uid config

        private readonly IRepository<ZaloTemplateConfig> _zaloTemplateConfigRepository = unitOfWork.GetRepository<ZaloTemplateConfig>();

        public async Task<int> CreateAsync(ZaloUidConfigRequest zaloUidConfigRequest)
        {
            // thêm cấu hình của thẳng vào bảng ZaloTemplateConfig
            var zaloUidCofig = new ZaloTemplateConfig
            {
                Recipients = string.Join(",", zaloUidConfigRequest.Recipients),
                TemplateId = zaloUidConfigRequest.ZaloTemplateUid,
                TemplateMapping = zaloUidConfigRequest.ParamsConfig.Any() ? JsonConvert.SerializeObject(zaloUidConfigRequest.ParamsConfig) : string.Empty
            };
            _zaloTemplateConfigRepository.Add(zaloUidCofig);


            var eventTemplate = mapper.Map<EventTemplate>(zaloUidConfigRequest);
            eventTemplate.ReferenceId = zaloUidCofig.Id;

            eventTemplate.PhoneNumber = string.Empty;
            eventTemplate.RoutingRule = string.Empty;
            eventTemplate.TemplateCode = string.Empty;
            eventTemplate.TemplateMapping = string.Empty;

            return await base.CreateAsync(eventTemplate);
        }

        public async Task<int> UpdateAsync(string id, ZaloUidConfigRequest zaloUidConfigRequest)
        {
            var existingTemplate = await base.GetByIdAsync(id);
            if (existingTemplate == null)
            {
                throw new KeyNotFoundException($"EventTemplate with ID '{id}' not found.");
            }

            // xóa cái cấu hình cũ nếu như có
            if (!string.IsNullOrEmpty(existingTemplate.ReferenceId))
            {
                await _zaloTemplateConfigRepository.DeleteByIdAsync(existingTemplate.ReferenceId);
            }

            // thêm cấu hình của thẳng vào bảng ZaloTemplateConfig
            var zaloUidCofig = new ZaloTemplateConfig
            {
                Recipients = string.Join(",", zaloUidConfigRequest.Recipients),
                TemplateId = zaloUidConfigRequest.ZaloTemplateUid,
                TemplateMapping = zaloUidConfigRequest.ParamsConfig.Any() ? JsonConvert.SerializeObject(zaloUidConfigRequest.ParamsConfig) : string.Empty
            };
            _zaloTemplateConfigRepository.Add(zaloUidCofig);

            mapper.Map(zaloUidConfigRequest, existingTemplate);
            existingTemplate.ReferenceId = zaloUidCofig.Id;

            existingTemplate.PhoneNumber = string.Empty;
            existingTemplate.RoutingRule = string.Empty;
            existingTemplate.TemplateCode = string.Empty;
            existingTemplate.TemplateMapping = string.Empty;

            return await base.UpdateAsync(existingTemplate);
        }

        public async Task<ZaloTemplateConfig> GetZaloUidConfigById(string id)
        {
            var zaloUidConfig = await _zaloTemplateConfigRepository.FindByIdAsync(id);
            if (zaloUidConfig == null)
            {
                return new ZaloTemplateConfig
                {
                    Id = string.Empty,
                    TemplateId = string.Empty,
                    Recipients = string.Empty,
                    TemplateMapping = string.Empty
                };
            }
            return zaloUidConfig;
        }

        #endregion

        #region Omni message config

        private readonly IRepository<OmniTemplateConfig> _omniConfigTemplateRepository = unitOfWork.GetRepository<OmniTemplateConfig>();

        public async Task<int> CreateAsync(EventTemplateRequest eventTemplateRequest)
        {
            var eventTemplate = mapper.Map<EventTemplate>(eventTemplateRequest);
            eventTemplate.RoutingRule = string.Join(",", eventTemplateRequest.RoutingRule);
            eventTemplate.TemplateMapping = eventTemplateRequest.ParamsConfig.Any() ? JsonConvert.SerializeObject(eventTemplateRequest.ParamsConfig) : string.Empty;
            _repository.Add(eventTemplate);
            return await unitOfWork.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(string id, EventTemplateRequest eventTemplateRequest)
        {
            var existingTemplate = await base.GetByIdAsync(id);
            if (existingTemplate == null)
            {
                throw new KeyNotFoundException($"EventTemplate with ID '{id}' not found.");
            }

            // Cập nhật các trường từ request
            mapper.Map(eventTemplateRequest, existingTemplate); // nếu cấu hình AutoMapper đúng thì phần này sẽ hoạt động mượt
            existingTemplate.RoutingRule = string.Join(",", eventTemplateRequest.RoutingRule);
            existingTemplate.TemplateMapping = eventTemplateRequest.ParamsConfig.Any()
                ? JsonConvert.SerializeObject(eventTemplateRequest.ParamsConfig)
                : string.Empty;

            _repository.Update(existingTemplate);

            return await unitOfWork.SaveChangesAsync();
        }

        public async Task<OmniTemplateConfig> GetOmniConfigById(string id)
        {
            var omniTemplateConfig = await _omniConfigTemplateRepository.FindByIdAsync(id);
            if (omniTemplateConfig == null)
            {
                return new OmniTemplateConfig
                {
                    Id = string.Empty,
                    PhoneNumber = string.Empty,
                    RoutingRule = string.Empty,
                    TemplateCode = string.Empty,
                    TemplateMapping = string.Empty
                };
            }
            return omniTemplateConfig;
        }

        #endregion

    }
}
