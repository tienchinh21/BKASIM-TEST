using Hangfire;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Dependencies.Cache;
using MiniAppGIBA.Base.Helpers;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.OmniTool;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Requests.OmniTools;
using MiniAppGIBA.Models.Requests.OmniTools.Templates;
using MiniAppGIBA.Services.OmniTool.Omni;
using MiniAppGIBA.Services.OmniTool.Omni.Models;
using MiniAppGIBA.Services.SystemSettings;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;


namespace MiniAppGIBA.Services.OmniTool
{
    public class OmniToolService(IUnitOfWork unitOfWork,
                                 IConfiguration configuration,
                                 ICacheService cacheService,
                                 ISystemSettingService systemSettingService, IOmniService omniService) : IOmniToolService
    {
        //private readonly IRepository<WebHookLogs> _webHookLogRepo = unitOfWork.GetRepository<WebHookLogs>();
        //private readonly IRepository<CampaignTag> _campaignTagRepo = unitOfWork.GetRepository<CampaignTag>();
        //private readonly IRepository<CampaignCSKH> _campaignCSKHRepo = unitOfWork.GetRepository<CampaignCSKH>();
        //private readonly IRepository<CampaignConfig> _campaignConfigRepo = unitOfWork.GetRepository<CampaignConfig>();
        //private readonly IRepository<CampaignPhoneCSKH> _campaignPhoneCSKHRepo = unitOfWork.GetRepository<CampaignPhoneCSKH>();
        //private readonly IRepository<CampaignPhoneCSKHTemp> _campaignPhoneCSKHTempRepo = unitOfWork.GetRepository<CampaignPhoneCSKHTemp>();

        //private readonly IRepository<Membership> _membershipRepo = unitOfWork.GetRepository<Membership>();
        //private readonly IRepository<MembershipExtend> _membershipExtendRepo = unitOfWork.GetRepository<MembershipExtend>();

        #region Campaign

        //public async Task<int> CreateCampaignAsync(string accountId, CampaignRequest campaignRequest)
        //{
        //    // tạo campaign
        //    var campaign = new CampaignCSKH
        //    {
        //        AccountCmsId = accountId,
        //        CampaignCode = Guid.NewGuid().ToString("N"),
        //        CampaignName = campaignRequest.Name,
        //        ScheduleTime = campaignRequest.ScheduleTime,
        //        TemplateCode = campaignRequest.TemplateCode,
        //        RoutingRule = string.Join(",", campaignRequest.RoutingRule),
        //        Status = 1 //pending
        //    };
        //    _campaignCSKHRepo.Add(campaign);

        //    // lưu campaing config
        //    var campaignConfig = new CampaignConfig()
        //    {
        //        CampaignId = campaign.Id,
        //        TagContent = string.Join(",", campaignRequest.Tags),
        //        VariableContent = JsonConvert.SerializeObject(campaignRequest.ParamsConfig),
        //    };
        //    _campaignConfigRepo.Add(campaignConfig);

        //    // lưu danh sách tag campaign
        //    var campaignTags = campaignRequest.Tags.Select(tag => new CampaignTag
        //    {
        //        TagID = tag,
        //        CampaignId = campaign.Id,
        //    });
        //    _campaignTagRepo.AddRange(campaignTags);

        //    var result = await unitOfWork.SaveChangesAsync();
        //    if (result > 0)
        //    {
        //        // sau khi tạo campaign và config thì sau 60s sẽ chạy job để khởi tạo data lưu vào bảng campaignPhoneTemp
        //        BackgroundJob.Schedule(() => ExecuteProcessDataForCampagin(campaign.Id), TimeSpan.FromSeconds(10));

        //        // thêm vào hangfire chạy khi nào 
        //        var delay = campaign.ScheduleTime - DateTime.Now;
        //        if (delay.TotalSeconds > 0)
        //        {
        //            campaign.IdJob = BackgroundJob.Schedule(() => ExecuteScheduledCampaignsAsync(campaign.Id), delay);

        //            _campaignCSKHRepo.Update(campaign);
        //            await unitOfWork.SaveChangesAsync();
        //        }
        //    }

        //    return result;
        //}

        //public async Task<int> UpdateCampaignAsync(string id, CampaignRequest campaignRequest)
        //{
        //    // Tìm campaign theo ID
        //    var campaign = await GetCampaignCSKHByIdAsync(id);
        //    if (campaign == null)
        //    {
        //        // Không tìm thấy dữ liệu
        //        return 0;
        //    }

        //    // Cập nhật thông tin campaign
        //    campaign.CampaignName = campaignRequest.Name;
        //    campaign.ScheduleTime = campaignRequest.ScheduleTime;
        //    campaign.TemplateCode = campaignRequest.TemplateCode;
        //    campaign.RoutingRule = string.Join(",", campaignRequest.RoutingRule);

        //    _campaignCSKHRepo.Update(campaign);

        //    // Cập nhật CampaignConfig
        //    var campaignConfig = await _campaignConfigRepo.AsQueryable().FirstOrDefaultAsync(c => c.CampaignId == campaign.Id);
        //    if (campaignConfig != null)
        //    {
        //        campaignConfig.TagContent = string.Join(",", campaignRequest.Tags);
        //        campaignConfig.VariableContent = JsonConvert.SerializeObject(campaignRequest.ParamsConfig);
        //        _campaignConfigRepo.Update(campaignConfig);
        //    }

        //    // Cập nhật CampaignTags
        //    var existingTags = await _campaignTagRepo.AsQueryable().Where(c => c.CampaignId == campaign.Id).ToListAsync();
        //    _campaignTagRepo.DeleteRange(existingTags);

        //    var newTags = campaignRequest.Tags.Select(tag => new CampaignTag
        //    {
        //        TagID = tag,
        //        CampaignId = campaign.Id
        //    });
        //    _campaignTagRepo.AddRange(newTags);

        //    await _campaignPhoneCSKHTempRepo.AsQueryable()
        //           .Where(x => x.CampaignCSHKId == id)
        //           .ExecuteDeleteAsync();

        //    // Lưu thay đổi vào database
        //    var result = await unitOfWork.SaveChangesAsync();

        //    if (!string.IsNullOrEmpty(campaign.IdJob))
        //    {
        //        StopJob(new List<string> { campaign.IdJob });
        //    }

        //    if (result > 0 && campaign.Status == 1)
        //    {
        //        // Xóa job cũ nếu có
        //        if (!string.IsNullOrEmpty(campaign.IdJob))
        //        {
        //            StopJob(new List<string> { campaign.IdJob });
        //        }

        //        // sau khi tạo campaign và config thì sau 60s sẽ chạy job để khởi tạo data lưu vào bảng campaignPhoneTemp
        //        BackgroundJob.Schedule(() => ExecuteProcessDataForCampagin(campaign.Id), TimeSpan.FromSeconds(10));

        //        // Lên lịch job mới nếu có thay đổi thời gian chạy
        //        var delay = campaign.ScheduleTime - DateTime.Now;
        //        if (delay.TotalSeconds > 0)
        //        {
        //            campaign.IdJob = BackgroundJob.Schedule(() => ExecuteScheduledCampaignsAsync(campaign.Id), delay);
        //            _campaignCSKHRepo.Update(campaign);

        //        }
        //    }

        //    return await unitOfWork.SaveChangesAsync(); ;
        //}

        //public async Task<int> DeleteCampaignByIdAsync(string id)
        //{
        //    var existingCampaign = await _campaignCSKHRepo.FindByIdAsync(id);
        //    if (existingCampaign == null)
        //    {
        //        throw new CustomException(200, "Campaign is not found");
        //    }

        //    await _campaignPhoneCSKHTempRepo.AsQueryable()
        //                .Where(x => x.CampaignCSHKId == id)
        //                .ExecuteDeleteAsync();

        //    await _campaignConfigRepo.AsQueryable()
        //                .Where(x => x.CampaignId == id)
        //                .ExecuteDeleteAsync();

        //    // Xóa job cũ nếu có
        //    if (!string.IsNullOrEmpty(existingCampaign.IdJob))
        //    {
        //        BackgroundJob.Delete(existingCampaign.IdJob);
        //    }

        //    _campaignCSKHRepo.Delete(existingCampaign);
        //    return await unitOfWork.SaveChangesAsync();
        //}

        //public async Task<List<string>> GetTagsByCampaignId(string campaignId)
        //{
        //    return await _campaignTagRepo.AsQueryable().Where(x => x.CampaignId == campaignId).Select(x => x.TagID!).ToListAsync();
        //}

        //public async Task<PagedResult<CampaignCSKH>> GetPageCampaign(RequestQuery query)
        //{
        //    var campains = _campaignCSKHRepo.AsQueryable();

        //    var totalItems = await campains.CountAsync();
        //    var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

        //    var items = await campains
        //        .OrderByDescending(x => x.CreatedDate)
        //        .Skip(query.Skip)
        //        .Take(query.PageSize)
        //        .ToListAsync();

        //    return new PagedResult<CampaignCSKH>
        //    {
        //        Data = items,
        //        TotalPages = totalPages,
        //        TotalCount = totalItems,
        //        CurrentPage = query.Page,
        //        PageSize = query.PageSize
        //    };
        //}

        //public async Task<CampaignCSKH?> GetCampaignCSKHByIdAsync(string id)
        //{
        //    return await _campaignCSKHRepo.FindByIdAsync(id);
        //}

        //public async Task<(CampaignCSKH?, CampaignConfig?)> GetDetailCampaignById(string id)
        //{
        //    var campaign = await GetCampaignCSKHByIdAsync(id);
        //    if (campaign == null)
        //    {
        //        return (null, null);
        //    }
        //    var campaignConfig = await _campaignConfigRepo.AsQueryable().FirstOrDefaultAsync(x => x.CampaignId == id);
        //    return (campaign, campaignConfig);
        //}

        //#endregion

        //#region Get histoy web omni log

        //public async Task<PagedResult<WebHookLogs>> GetPageWebHookLogs(RequestQuery query, string? campaignId, short? status)
        //{
        //    var webHookLogs = _webHookLogRepo.AsQueryable();
        //    var totalItems = await webHookLogs.CountAsync();
        //    var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);
        //    var items = await webHookLogs
        //        .OrderByDescending(x => x.CreatedDate)
        //        .Skip(query.Skip)
        //        .Take(query.PageSize)
        //        .ToListAsync();
        //    return new PagedResult<WebHookLogs>
        //    {
        //        Items = items,
        //        TotalPages = totalPages,
        //        TotalItems = totalItems,
        //    };
        //}

        //public async Task<PagedResult<CampaignPhoneCSKH>> GetPageCampaignPhoneLogs(RequestQuery query, string? campaignId, short? status)
        //{
        //    var campaignPhoneCSKHs = _campaignPhoneCSKHRepo.AsQueryable();

        //    if (!string.IsNullOrEmpty(campaignId))
        //    {
        //        campaignPhoneCSKHs = campaignPhoneCSKHs.Where(x => x.CampaignCSKHId == campaignId);
        //    }

        //    if (status.HasValue)
        //    {
        //        campaignPhoneCSKHs = campaignPhoneCSKHs.Where(x => x.Status == status);
        //    }

        //    var totalItems = await campaignPhoneCSKHs.CountAsync();
        //    var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);
        //    var items = await campaignPhoneCSKHs
        //        .OrderByDescending(x => x.CreatedDate)
        //        .Skip(query.Skip)
        //        .Take(query.PageSize)
        //        .ToListAsync();
        //    return new PagedResult<CampaignPhoneCSKH>
        //    {
        //        Items = items,
        //        TotalPages = totalPages,
        //        TotalItems = totalItems,
        //    };
        //}

        //#endregion

        //#region Processing Data Campaign

        //// xử lý, mapping data và lưu vào bảng campaignCSKHTemp
        //public async Task ExecuteProcessDataForCampagin(string campaignId)
        //{
        //    var campaign = await GetCampaignCSKHByIdAsync(campaignId);
        //    if (campaign == null)
        //    {
        //        Console.WriteLine($"\n\n\n CampaignId: {campaignId} not found \n\n\n");
        //        return;
        //    }

        //    var omniAccount = await systemSettingService.GetOmniAccountAsync();
        //    if (string.IsNullOrEmpty(omniAccount.Username) || string.IsNullOrEmpty(omniAccount.Password))
        //    {
        //        Console.WriteLine("Omni Account is not found");
        //        return;
        //    }

        //    var templatesCache = await cacheService.GetValueAsync<List<TemplateResponse>>("OmniTemplates") ?? new List<TemplateResponse>();
        //    var template = templatesCache.FirstOrDefault(x => x.TemplateCode == campaign.TemplateCode);
        //    if (template == null)
        //    {
        //        templatesCache = (await omniService.GetAllOwnedTemplate(omniAccount)).ListTemp;
        //        template = templatesCache.FirstOrDefault(x => x.TemplateCode == campaign.TemplateCode)
        //            ?? throw new Exception("Template is not found");
        //    }

        //    // **1️⃣ Lấy danh sách tag từ CampaignConfig**
        //    var campaignConfig = await _campaignConfigRepo.AsQueryable().FirstOrDefaultAsync(x => x.CampaignId == campaignId);
        //    if (campaignConfig == null || string.IsNullOrEmpty(campaignConfig.TagContent))
        //    {
        //        return;
        //    }

        //    var tags = campaignConfig.TagContent.Split(',').Select(t => t.Trim()).ToList();
        //    var campaignTags = await GetTagsByCampaignId(campaignId);

        //    var campaingParams = JsonConvert.DeserializeObject<List<MappingParams>>(campaignConfig?.VariableContent ?? "") ?? new List<MappingParams>();

        //    // **2️⃣ Xử lý theo batch với scheduling**
        //    int batchSize = configuration.GetSection("OmniTool:BatchSize").Get<int?>() ?? 200;
        //    int batchPerGroup = configuration.GetSection("OmniTool:Groups").Get<int?>() ?? 10;

        //    var membershipsQuery = _membershipRepo.AsQueryable();
        //    var membershipTagsQuery = _membershipTagRepo.AsQueryable().Where(x => campaignTags.Contains(x.TagId)).Select(x => x.UserZaloId).Distinct();

        //    int totalRecords = await membershipTagsQuery.CountAsync();

        //    int batchCount = 0;
        //    int delayInMinutes = 0;
        //    int totalBatches = (int)Math.Ceiling((double)totalRecords / batchSize);

        //    // xử lý tin temporary theo batch
        //    for (int i = 0; i < totalBatches; i++)
        //    {
        //        var batch = await membershipTagsQuery
        //             .Skip(i * batchSize)
        //             .Take(batchSize)
        //             .ToListAsync();  // Fetch data trước khi kiểm tra

        //        if (!batch.Any()) break;

        //        var memberships = await membershipsQuery
        //            .Where(m => batch.Contains(m.UserZaloId))
        //            .ToListAsync();

        //        if (memberships.Any())  // Chỉ schedule nếu có dữ liệu
        //        {
        //            BackgroundJob.Schedule(() => ProcessMembershipsAsync(memberships, campaign, campaingParams), TimeSpan.FromMinutes(delayInMinutes));
        //        }

        //        batchCount++;

        //        // Sau mỗi nhóm batchPerGroup, tăng delay
        //        if (batchCount >= batchPerGroup)
        //        {
        //            delayInMinutes++;  // Tăng thời gian delay để tránh quá tải
        //            batchCount = 0;    // Reset batch count
        //        }
        //    }
        //}

        //// xử lý data mapped với các table khác
        //public Dictionary<string, object> GetMappedDataAsync(Membership membership, Dictionary<string, string> membershipExtendsData, List<MappingParams> campaignConfig)
        //{
        //    var result = new Dictionary<string, object>();
        //    var lstParamsMembership = campaignConfig
        //        .Where(x => !string.IsNullOrEmpty(x.MappingTableName) && x.MappingTableName == "Membership")
        //        .ToList();

        //    var lstParamsMembershipExtend = campaignConfig
        //        .Where(x => !string.IsNullOrEmpty(x.MappingTableName) && x.MappingTableName == "MembershipExtend")
        //        .ToList();
        //    foreach (var config in campaignConfig)
        //    {
        //        if (string.IsNullOrEmpty(config.ParamName) ||
        //            string.IsNullOrEmpty(config.MappingTableName) ||
        //            string.IsNullOrEmpty(config.MappingColumnName))
        //        {
        //            continue;
        //        }

        //        object value = config.DefaultValue ?? string.Empty;

        //        if (config.MappingTableName == "Membership")
        //        {
        //            var property = typeof(Membership).GetProperty(config.MappingColumnName);
        //            value = property?.GetValue(membership)?.ToString() ?? value;
        //        }
        //        else if (config.MappingTableName == "MembershipExtend")
        //        {
        //            if (membershipExtendsData.TryGetValue(config.ParamName, out var extendValue) && !string.IsNullOrEmpty(extendValue))
        //            {
        //                value = extendValue;
        //            }
        //        }
        //        result[config.ParamName] = value;
        //    }

        //    return result;
        //}

        //// xử lý tạo data với mapping
        //public async Task ProcessMembershipsAsync(List<Membership> memberships, CampaignCSKH campaign, List<MappingParams> campaignConfig)
        //{
        //    if (campaign == null)
        //    {
        //        return;
        //    }
        //    var campaignPhoneList = new List<CampaignPhoneCSKHTemp>();
        //    var lstMembership = memberships.Select(m => m.UserZaloId).ToList();

        //    var membshipsExtendsDict = (await _membershipExtendRepo.AsQueryable()
        //        .Where(x => string.IsNullOrEmpty(x.Attribute) && string.IsNullOrEmpty(x.Content) && lstMembership.Contains(x.UserZaloId))
        //        .ToListAsync())
        //        .GroupBy(x => x.UserZaloId)
        //        .ToDictionary(
        //            g => g.Key ?? string.Empty, // Đảm bảo UserZaloId không null
        //            g => g.ToDictionary(
        //                x => x.Attribute ?? string.Empty,  // Đảm bảo Attribute không null
        //                x => x.Content ?? string.Empty     // Đảm bảo Content không null
        //            )
        //        );

        //    // loop qua thông tin của mỗi memberships và thêm vào temporary campaign phone 
        //    foreach (var membership in memberships)
        //    {
        //        var membershipExtendData = membshipsExtendsDict.GetValueOrDefault(membership.UserZaloId) ?? new Dictionary<string, string>();
        //        var mappedData = GetMappedDataAsync(membership, membershipExtendData, campaignConfig);
        //        campaignPhoneList.Add(new CampaignPhoneCSKHTemp
        //        {
        //            AccountId = campaign.AccountCmsId,
        //            TemplateCode = campaign.TemplateCode,
        //            RoutingRule = campaign.RoutingRule,
        //            CampaignCSHKId = campaign.Id,
        //            PhoneNumber = membership.PhoneNumber,
        //            ParamContent = JsonConvert.SerializeObject(mappedData),
        //            Status = 0,
        //        });
        //    }

        //    if (campaignPhoneList.Any())
        //    {
        //        _campaignPhoneCSKHTempRepo.AddRange(campaignPhoneList);
        //        await unitOfWork.SaveChangesAsync();
        //    }
        //}

        //#endregion

        //#region Execute Scheduled Campaign Phone CSKH Temp

        //public void StopJob(List<string> jobIds)
        //{
        //    foreach (var jobId in jobIds)
        //    {
        //        BackgroundJob.Delete(jobId); // Xóa từng job theo ID
        //        Console.WriteLine($"Job {jobId} đã bị xóa.");
        //    }
        //}

        //public async Task ProcessBatchAsync(List<CampaignPhoneCSKHTemp> batch)
        //{
        //    // Lấy tài khoản Omni để gửi tin
        //    var omniAccount = await systemSettingService.GetOmniAccountAsync();
        //    if (string.IsNullOrEmpty(omniAccount.Username) || string.IsNullOrEmpty(omniAccount.Password))
        //    {
        //        Console.WriteLine($"❌ [ERROR] ProcessBatchAsync: Omni Account is not found.");
        //        // throw new Exception("Omni Account is not found");
        //        return;
        //    }
        //    var listWebLogs = new List<WebHookLogs>();
        //    var listMessagePhoneCSKH = new List<CampaignPhoneCSKH>();
        //    foreach (var item in batch)
        //    {
        //        try
        //        {
        //            // Gửi tin nhắn qua OmniService
        //            var listParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ParamContent);
        //            var omniRes = await omniService.SendOmniMessageAsync(omniAccount, item.TemplateCode, item.PhoneNumber, item.RoutingRule, listParams, new List<string>());

        //            var telCoId = PhoneNumberHandler.GetIdTelco(item.PhoneNumber);
        //            // Lưu log vào DB
        //            listMessagePhoneCSKH.Add(new CampaignPhoneCSKH
        //            {
        //                Status = omniRes.Status,
        //                TelcoID = telCoId,
        //                AccountId = item.AccountId,
        //                PhoneNumber = item.PhoneNumber,
        //                RoutingRule = item.RoutingRule,
        //                ParamContent = item.ParamContent,
        //                TemplateCode = item.TemplateCode,
        //                CampaignCSKHId = item.CampaignCSHKId,
        //                IdOmniMess = omniRes.IdOmniMess,
        //                ErrorCode = omniRes.Code,
        //            });

        //            listWebLogs.Add(new WebHookLogs
        //            {
        //                TelcoId = telCoId,
        //                ErrorCode = omniRes.Code,
        //                Status = omniRes.Status.ToString(),
        //                IdOmniMess = omniRes.IdOmniMess,
        //                Response = JsonConvert.SerializeObject(omniRes),
        //            });

        //            // Đánh dấu tin nhắn đã gửi thành công
        //            item.Status = omniRes.Status;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"❌ [ERROR] ProcessBatchAsync: Lỗi khi gửi tin nhắn tới {item.PhoneNumber} - {ex.Message}");
        //            // await LogWebhookAsync(item, "Error", ex.Message);
        //        }
        //    }

        //    // Cập nhật trạng thái batch, lưu log vào DB
        //    // _webHookLogRepo.AddRange(listWebLogs);
        //    _campaignPhoneCSKHTempRepo.UpdateRange(batch);

        //    // sau khi chạy job thì gửi tin dựa trên temp thì chạy job đồng bộ status và xóa tin nhắn ở temp
        //    _campaignPhoneCSKHRepo.AddRange(listMessagePhoneCSKH);

        //    await unitOfWork.SaveChangesAsync();
        //}

        //public async Task ExecuteScheduledCampaignsAsync(string campaignId)
        //{
        //    var campaign = await GetCampaignCSKHByIdAsync(campaignId);
        //    if (campaign == null)
        //    {
        //        Console.WriteLine($"❌ [ERROR] ExecuteScheduledCampaignsAsync: CampaignId {campaignId} not found.");
        //        return;
        //    }

        //    switch (campaign.Status)
        //    {
        //        case 2:
        //            Console.WriteLine($"❌ [ERROR] ExecuteScheduledCampaignsAsync: CampaignId {campaignId} is running.");
        //            return;
        //        case 3:
        //            Console.WriteLine($"❌ [ERROR] ExecuteScheduledCampaignsAsync: CampaignId {campaignId} is completed.");
        //            return;
        //        case 4:
        //            Console.WriteLine($"❌ [ERROR] ExecuteScheduledCampaignsAsync: CampaignId {campaignId} is not cancelled.");
        //            return;
        //        default:
        //            break;
        //    }

        //    campaign.Status = 2; // running
        //    await unitOfWork.SaveChangesAsync();

        //    var jobIds = new List<string>();

        //    var batchSize = configuration.GetSection("OmniTool:BatchSize").Get<int?>() ?? 200;
        //    var batchPerGroup = configuration.GetSection("OmniTool:Groups").Get<int?>() ?? 10;

        //    var campaignPhoneCSKHTempQuery = _campaignPhoneCSKHTempRepo.AsQueryable().Where(x => x.CampaignCSHKId == campaignId && x.Status == 0);

        //    int batchCount = 0;
        //    int delayInMinutes = 0;

        //    // Đếm tổng số bản ghi cần xử lý
        //    var totalRecords = await campaignPhoneCSKHTempQuery.CountAsync();
        //    int totalBatches = (int)Math.Ceiling((double)totalRecords / batchSize);

        //    for (int i = 0; i < totalBatches; i++)
        //    {
        //        // Chỉ load batchSize dòng mỗi lần
        //        var batch = await campaignPhoneCSKHTempQuery.OrderBy(x => x.Id).Skip(i * batchSize).Take(batchSize).ToListAsync();
        //        if (!batch.Any())
        //        {
        //            // Ghi log lại là xử lý xong nè
        //            break;
        //        }

        //        // Schedule job gửi batch này với độ trễ hiện tại
        //        var jobId = BackgroundJob.Schedule(() => ProcessBatchAsync(batch), TimeSpan.FromMinutes(delayInMinutes));
        //        jobIds.Add(jobId);

        //        batchCount++;

        //        // Sau mỗi nhóm batchPerGroup, tăng độ trễ
        //        if (batchCount >= batchPerGroup)
        //        {
        //            delayInMinutes++;  // Tăng thời gian delay mỗi khi đủ batchPerGroup
        //            batchCount = 0;    // Reset batch count
        //        }
        //    }

        //    // chạy đồng bộ campaign status và xóa tin nhắn ở bảng temp
        //    await SyncCamphonePhoneTempToCampaignPhone(campaign);
        //}

        //public async Task<int> SyncCamphonePhoneTempToCampaignPhone(CampaignCSKH campaignCSKH)
        //{
        //    var recordsToSync = _campaignPhoneCSKHRepo.AsQueryable().Where(c => c.Status == 1 && c.CampaignCSKHId == campaignCSKH.Id);
        //    campaignCSKH.TotalSuccess = await recordsToSync.CountAsync();
        //    var campaignPhoneTempToRemove = await _campaignPhoneCSKHTempRepo.AsQueryable()
        //                                        .Where(c => c.CampaignCSHKId == campaignCSKH.Id && recordsToSync.Any(x => x.PhoneNumber == c.PhoneNumber))
        //                                        .ToListAsync();
        //    if (campaignPhoneTempToRemove.Any())
        //    {
        //        _campaignPhoneCSKHTempRepo.DeleteRange(campaignPhoneTempToRemove);
        //    }

        //    campaignCSKH.Status = 3;
        //    campaignCSKH.TotalSuccess = await recordsToSync.CountAsync();
        //    _campaignCSKHRepo.Update(campaignCSKH);
        //    return await unitOfWork.SaveChangesAsync();
        //}

        #endregion

        #region Event Template Log

        private readonly IRepository<EventTemplateLog> _eventTemplateLogRepo = unitOfWork.GetRepository<EventTemplateLog>();
        public async Task<PagedResult<EventTemplateLog>> GetEventTemplateLogs(RequestQuery query, string? status, string? type, DateTime? fromDate, DateTime? toDate)
        {
            var templateLogs = _eventTemplateLogRepo.AsQueryable();

            // Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                templateLogs = templateLogs.Where(x =>
                    x.Recipient.Contains(query.Keyword) ||
                    x.RequestBody.Contains(query.Keyword) ||
                    x.ResponseBody.Contains(query.Keyword));
            }

            if (!string.IsNullOrEmpty(type))
            {
                templateLogs = templateLogs.Where(x => !string.IsNullOrEmpty(x.Type) && x.Type.ToLower() == type.ToLower());
            }

            if (!string.IsNullOrEmpty(status))
            {
                string expectedCode = (type == "omni") ? "1" : "0";
                bool isSuccess = status == "success";

                templateLogs = isSuccess
                    ? templateLogs.Where(x => x.ResultCode == expectedCode)
                    : templateLogs.Where(x => x.ResultCode != expectedCode);
            }

            if (fromDate.HasValue)
            {
                templateLogs = templateLogs.Where(x => x.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                templateLogs = templateLogs.Where(x => x.CreatedDate <= toDate.Value);
            }

            var totalItems = await templateLogs.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);
            var items = await templateLogs
                .OrderByDescending(x => x.CreatedDate)
                .Skip(query.Skip)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<EventTemplateLog>
            {
                Items = items,
                TotalPages = totalPages,
                TotalItems = totalItems,
            };
        }

        public async Task<byte[]> ExportEventTemplateLogsToExcel(string? status, string? type, DateTime? fromDate, DateTime? toDate, string? keyword = null)
        {
            var templateLogs = _eventTemplateLogRepo.AsQueryable();

            // Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(keyword))
            {
                templateLogs = templateLogs.Where(x =>
                    x.Recipient.Contains(keyword) ||
                    x.RequestBody.Contains(keyword) ||
                    x.ResponseBody.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(type))
            {
                templateLogs = templateLogs.Where(x => !string.IsNullOrEmpty(x.Type) && x.Type.ToLower() == type.ToLower());
            }

            if (!string.IsNullOrEmpty(status))
            {
                string expectedCode = (type == "omni") ? "1" : "0";
                bool isSuccess = status == "success";

                templateLogs = isSuccess
                    ? templateLogs.Where(x => x.ResultCode == expectedCode)
                    : templateLogs.Where(x => x.ResultCode != expectedCode);
            }

            if (fromDate.HasValue)
            {
                templateLogs = templateLogs.Where(x => x.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                templateLogs = templateLogs.Where(x => x.CreatedDate <= toDate.Value);
            }

            // Thêm bộ lọc theo thời gian mà chưa được dùng trong phương thức gốc
            templateLogs = templateLogs.Where(x => x.CreatedDate >= fromDate && x.CreatedDate <= toDate);

            // Lấy tất cả dữ liệu (không phân trang)
            var items = await templateLogs
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            // Tạo Excel file sử dụng EPPlus
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Event Template Logs");

                // Tạo tiêu đề
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Thời gian";
                worksheet.Cells[1, 3].Value = "Loại";
                worksheet.Cells[1, 4].Value = "Người nhận";
                worksheet.Cells[1, 5].Value = "Kết quả";
                worksheet.Cells[1, 6].Value = "Mã kết quả";
                worksheet.Cells[1, 7].Value = "Nội dung gửi";
                worksheet.Cells[1, 8].Value = "Phản hồi";

                // Format header
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Điền dữ liệu
                for (int i = 0; i < items.Count; i++)
                {
                    var row = i + 2;
                    var log = items[i];

                    worksheet.Cells[row, 1].Value = i + 1;
                    worksheet.Cells[row, 2].Value = log.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cells[row, 3].Value = log.Type;
                    worksheet.Cells[row, 4].Value = log.Recipient;

                    string _status = "Thất bại";
                    if ((log.Type?.ToLower() == "omni" && log.ResultCode == "1") ||
                        (log.Type?.ToLower() != "omni" && log.ResultCode == "0"))
                    {
                        _status = "Thành công";
                    }

                    worksheet.Cells[row, 5].Value = _status;
                    worksheet.Cells[row, 6].Value = log.ResultCode;
                    worksheet.Cells[row, 7].Value = log.RequestBody;
                    worksheet.Cells[row, 8].Value = log.ResponseBody;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Giới hạn chiều rộng tối đa cho các cột có nội dung dài
                worksheet.Column(7).Width = Math.Min(worksheet.Column(7).Width, 50);
                worksheet.Column(8).Width = Math.Min(worksheet.Column(8).Width, 50);

                // Trả về file Excel dưới dạng byte array
                return package.GetAsByteArray();
            }
        }

        #endregion
    }
}
