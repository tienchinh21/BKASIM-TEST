using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Features.EventTrigger;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Payload;

namespace MiniAppGIBA.Services.Memberships
{
    public class MembershipApprovalService : IMembershipApprovalService
    {
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MembershipApprovalService> _logger;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public MembershipApprovalService(
            IRepository<Membership> membershipRepository,
            IUnitOfWork unitOfWork,
            IMediator mediator,
            IConfiguration configuration,
            ILogger<MembershipApprovalService> logger)
        {
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
            _configuration = configuration;
        }

        public async Task<PagedResult<Membership>> GetPendingMembershipsAsync(int page = 1, int pageSize = 20, string? keyword = null)
        {
            var query = _membershipRepository.AsQueryable()
                .Where(m => m.IsDelete != true);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(m => m.Fullname.Contains(keyword) || m.PhoneNumber.Contains(keyword));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var items = await query
                .OrderByDescending(m => m.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Membership> { Items = items, TotalPages = totalPages };
        }

        public async Task<int> ApproveMembershipAsync(string membershipId, string approvedBy)
        {
            var membership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.IsDelete != true);
            if (membership == null) throw new Exception("Không tìm thấy thành viên");

            membership.UpdatedDate = DateTime.Now;
            _membershipRepository.Update(membership);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                var delayInSeconds = _configuration.GetSection("Hangfire:DelayInSeconds").Get<int?>() ?? 10;
                var payload = CreateMembershipPayloadFromData(membership);
                BackgroundJob.Schedule<MembershipApprovalService>(
                    x => x.TriggerMembershipEvent("Membership.UpdateMiniAppStatus", payload),
                    TimeSpan.FromSeconds(delayInSeconds)
                );
            }
            return result;
        }

        public async Task<int> RejectMembershipAsync(string membershipId, string reason, string rejectedBy)
        {
            var membership = await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.IsDelete != true);
            if (membership == null) throw new Exception("Không tìm thấy thành viên");

            membership.UpdatedDate = DateTime.Now;
            _membershipRepository.Update(membership);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                var delayInSeconds = _configuration.GetSection("Hangfire:DelayInSeconds").Get<int?>() ?? 10;
                var payload = CreateMembershipPayloadFromData(membership);
                BackgroundJob.Schedule<MembershipApprovalService>(
                    x => x.TriggerMembershipEvent("Membership.UpdateMiniAppStatus", payload),
                    TimeSpan.FromSeconds(delayInSeconds)
                );
            }
            return result;
        }

        public async Task<Membership?> GetMembershipDetailAsync(string membershipId)
        {
            return await _membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.IsDelete != true);
        }

        public async Task<PagedResult<Membership>> GetApprovedMembershipsAsync(int page = 1, int pageSize = 20, string? keyword = null)
        {
            var query = _membershipRepository.AsQueryable().Where(m => m.IsDelete != true);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(m => m.Fullname.Contains(keyword) || m.PhoneNumber.Contains(keyword));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var items = await query
                .OrderByDescending(m => m.UpdatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Membership> { Items = items, TotalPages = totalPages };
        }

        public async Task<PagedResult<Membership>> GetRejectedMembershipsAsync(int page = 1, int pageSize = 20, string? keyword = null)
        {
            var query = _membershipRepository.AsQueryable().Where(m => m.IsDelete != true);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(m => m.Fullname.Contains(keyword) || m.PhoneNumber.Contains(keyword));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var items = await query
                .OrderByDescending(m => m.UpdatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Membership> { Items = items, TotalPages = totalPages };
        }

        public async Task TriggerMembershipEvent(string eventName, MembershipPayload payload)
        {
            await _mediator.Send(new EmitEventArgs
            {
                EventName = eventName,
                TriggerPhoneNumber = string.IsNullOrEmpty(payload.PhoneNumber) ? null : payload.PhoneNumber,
                TriggerZaloIdByOA = string.IsNullOrEmpty(payload.UserZaloIdByOA) ? null : payload.UserZaloIdByOA,
                Payload = payload
            });
        }

        public MembershipPayload CreateMembershipPayloadFromData(Membership membership)
        {
            return new MembershipPayload
            {
                UserZaloId = membership.UserZaloId,
                UserZaloName = membership.UserZaloName,
                UserZaloIdByOA = membership.UserZaloIdByOA,
                PhoneNumber = membership.PhoneNumber
            };
        }

        public async Task<PagedResult<Membership>> GetAllMembershipsAsync(int page = 1, int pageSize = 20, string? keyword = null)
        {
            var query = _membershipRepository.AsQueryable().Where(m => m.IsDelete != true);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(m => m.Fullname.Contains(keyword) || m.PhoneNumber.Contains(keyword));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var items = await query
                .OrderByDescending(m => m.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Membership> { Items = items, TotalPages = totalPages };
        }
    }
}
