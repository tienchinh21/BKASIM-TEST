using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Models.DTOs.Refs;
using MiniAppGIBA.Models.Request.Refs;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Refs;
using MiniAppGIBA.Services.Logs;
using System.Text.Json;

namespace MiniAppGIBA.Services.Refs
{
    public class RefService : IRefService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RefService> _logger;
        private readonly IReferralLogService? _referralLogService;

        public RefService(
            IUnitOfWork unitOfWork,
            ILogger<RefService> logger,
            IReferralLogService? referralLogService = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _referralLogService = referralLogService;
        }

        public async Task<RefDTO> CreateRefAsync(string userZaloId, CreateRefRequest request)
        {
            try
            {
                // Lấy thông tin người gửi
                var fromMember = await _unitOfWork.GetRepository<Membership>()
                    .AsQueryable()
                    .FirstOrDefaultAsync(m => m.UserZaloId == userZaloId && m.IsDelete != true);

                if (fromMember == null)
                {
                    throw new Exception("Không tìm thấy người gửi");
                }

                // ===== VALIDATION VÀ LOGIC CHO TYPE 0 (Internal - Share TO member) =====
                if (request.Type == 0)
                {
                    // Type 0: RefTo phải là member trong hệ thống
                    if (string.IsNullOrEmpty(request.RefTo))
                    {
                        throw new Exception("Người nhận là bắt buộc cho Type 0");
                    }

                    var toMember = await _unitOfWork.GetRepository<Membership>()
                        .AsQueryable()
                        .FirstOrDefaultAsync(m => m.UserZaloId == request.RefTo && m.IsDelete != true);

                    if (toMember == null)
                    {
                        throw new Exception("Không tìm thấy người nhận");
                    }

                    // Xác định groupId cho người nhận (RefTo) - bắt buộc
                    var refToGroupIdForValidation = request.RefToGroupId ?? request.GroupId;
                    if (string.IsNullOrEmpty(refToGroupIdForValidation))
                    {
                        throw new Exception("RefToGroupId là bắt buộc cho Type 0");
                    }

                    // Kiểm tra recipient member có trong group được chọn không
                    var refToGroup = await _unitOfWork.GetRepository<Group>()
                        .AsQueryable()
                        .FirstOrDefaultAsync(g => g.Id == refToGroupIdForValidation);

                    if (refToGroup == null)
                    {
                        throw new Exception("Không tìm thấy nhóm của người nhận");
                    }

                    var memberInGroup = await _unitOfWork.Context.MembershipGroups
                        .FirstOrDefaultAsync(mg => mg.UserZaloId == request.RefTo 
                            && mg.GroupId == refToGroupIdForValidation 
                            && mg.IsApproved == true);

                    if (memberInGroup == null)
                    {
                        throw new Exception($"Thành viên {toMember.Fullname} không thuộc nhóm {refToGroup.GroupName}");
                    }

                    // Validate ShareType
                    var shareType = request.ShareType?.ToLower() ?? "external";
                    if (shareType != "own" && shareType != "member" && shareType != "external")
                    {
                        throw new Exception("ShareType phải là 'own', 'member' hoặc 'external'");
                    }

                    string? referralName = null;
                    string? referralPhone = null;
                    string? referralEmail = null;
                    string? referralAddress = null;
                    string? referredMemberId = null;

                    // Xử lý theo ShareType
                    if (shareType == "own")
                    {
                        // Share profile bản thân - chỉ lưu userZaloId, profile lấy từ Membership khi get
                        referredMemberId = userZaloId;
                    }
                    else if (shareType == "member")
                    {
                        // Share profile thành viên trong nhóm - chỉ lưu userZaloId, profile lấy từ Membership khi get
                        if (string.IsNullOrEmpty(request.ReferredMemberId))
                        {
                            throw new Exception("ReferredMemberId là bắt buộc khi ShareType = 'member'");
                        }

                        var referredMember = await _unitOfWork.GetRepository<Membership>()
                            .AsQueryable()
                            .FirstOrDefaultAsync(m => m.UserZaloId == request.ReferredMemberId && m.IsDelete != true);

                        if (referredMember == null)
                        {
                            throw new Exception("Không tìm thấy thành viên được chia sẻ");
                        }

                        // Xác định groupId cho người được share (ReferredMemberId)
                        var referredMemberGroupIdForValidation = request.ReferredMemberGroupId ?? request.GroupId;
                        if (string.IsNullOrEmpty(referredMemberGroupIdForValidation))
                        {
                            throw new Exception("GroupId cho người được share là bắt buộc khi ShareType = 'member'");
                        }

                        // Kiểm tra member có trong group được chọn không
                        var referredMemberGroup = await _unitOfWork.GetRepository<Group>()
                            .AsQueryable()
                            .FirstOrDefaultAsync(g => g.Id == referredMemberGroupIdForValidation);

                        if (referredMemberGroup == null)
                        {
                            throw new Exception("Không tìm thấy nhóm của người được share");
                        }

                        var referredMemberInGroup = await _unitOfWork.Context.MembershipGroups
                            .FirstOrDefaultAsync(mg => mg.UserZaloId == request.ReferredMemberId 
                                && mg.GroupId == referredMemberGroupIdForValidation 
                                && mg.IsApproved == true);

                        if (referredMemberInGroup == null)
                        {
                            throw new Exception($"Thành viên {referredMember.Fullname} không thuộc nhóm {referredMemberGroup.GroupName}");
                        }

                        referredMemberId = request.ReferredMemberId;
                    }
                    else // shareType == "external"
                    {
                        // Share text người ngoài - lưu text vì không có trong hệ thống
                        if (string.IsNullOrWhiteSpace(request.ReferralName) && string.IsNullOrWhiteSpace(request.ReferralPhone))
                        {
                            throw new Exception("Vui lòng nhập ít nhất Tên hoặc Số điện thoại của người được giới thiệu");
                        }

                        referralName = request.ReferralName;
                        referralPhone = request.ReferralPhone;
                        referralEmail = request.ReferralEmail;
                        referralAddress = request.ReferralAddress;
                        referredMemberId = null;
                    }

                    // Xác định groupId cho người nhận và người được share để lưu vào DB
                    var refToGroupId = request.RefToGroupId ?? request.GroupId;
                    var referredMemberGroupId = request.ReferredMemberGroupId ?? request.GroupId;

                    // Tạo ref mới cho Type 0
                    var refEntity = new Ref
                    {
                        RefFrom = userZaloId, // Lưu userZaloId người gửi
                        RefTo = request.RefTo, // Lưu userZaloId người nhận (thành viên trong nhóm)
                        Content = request.Content,
                        Status = 1, // Đã gửi
                        Value = 0,
                        RefToGroupId = refToGroupId, // Nhóm của người nhận (bắt buộc)
                        ReferredMemberGroupId = referredMemberId != null ? referredMemberGroupId : null, // Nhóm của người được share (nếu là member)
                        Type = 0,
                        ReferredMemberId = referredMemberId, // userZaloId nếu là member, null nếu là người ngoài
                        // Chỉ lưu text khi là người ngoài (external)
                        ReferralName = referredMemberId == null ? referralName : null,
                        ReferralPhone = referredMemberId == null ? referralPhone : null,
                        ReferralEmail = referredMemberId == null ? referralEmail : null,
                        ReferralAddress = referredMemberId == null ? referralAddress : null,
                        RecipientName = null, // Không dùng cho Type 0
                        RecipientPhone = null, // Không dùng cho Type 0
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _unitOfWork.GetRepository<Ref>().AddAsync(refEntity);
                    await _unitOfWork.SaveChangesAsync();

                    // ✅ LOG REFERRAL
                    if (_referralLogService != null)
                    {
                        try
                        {
                            await _referralLogService.LogReferralAsync(new CreateReferralLogDto
                            {
                                ReferrerId = userZaloId,
                                RefereeId = request.RefTo ?? string.Empty,
                                GroupId = request.GroupId ?? string.Empty,
                                ReferralCode = null,
                                Source = "REF",
                                Metadata = JsonSerializer.Serialize(new
                                {   
                                    refId = refEntity.Id,
                                    type = 0,
                                    shareType = shareType,
                                    content = request.Content,
                                    referrerName = fromMember.Fullname,
                                    recipientName = toMember.Fullname,
                                    referredMemberId = refEntity.ReferredMemberId,
                                    referralName = refEntity.ReferralName,
                                    referralPhone = refEntity.ReferralPhone,
                                    referralEmail = refEntity.ReferralEmail,
                                    referralAddress = refEntity.ReferralAddress,
                                    shareTime = DateTime.Now,
                                    status = refEntity.Status
                                })
                            });
                        }
                        catch (Exception logEx)
                        {
                            _logger.LogWarning(logEx, "Failed to log referral for ref {RefId}", refEntity.Id);
                        }
                    }

                    return await GetRefByIdAsync(refEntity.Id) ?? throw new Exception("Không thể tạo ref");
                }
                // ===== VALIDATION VÀ LOGIC CHO TYPE 1 (External - Share TO external) =====
                else if (request.Type == 1)
                {
                    // Type 1: Recipient là người ngoài, RefTo = null
                    if (string.IsNullOrEmpty(request.RecipientPhone))
                    {
                        throw new Exception("Số điện thoại người nhận là bắt buộc cho Type 1");
                    }

                    if (string.IsNullOrEmpty(request.RecipientName))
                    {
                        throw new Exception("Tên người nhận là bắt buộc cho Type 1");
                    }

                    // Validate ShareType
                    var shareType = request.ShareType?.ToLower() ?? "external";
                    if (shareType != "own" && shareType != "member" && shareType != "external")
                    {
                        throw new Exception("ShareType phải là 'own', 'member' hoặc 'external'");
                    }

                    string? referralName = null;
                    string? referralPhone = null;
                    string? referralEmail = null;
                    string? referralAddress = null;
                    string? referredMemberId = null;

                    // Xử lý theo ShareType
                    if (shareType == "own")
                    {
                        // Share profile bản thân - chỉ lưu userZaloId, profile lấy từ Membership khi get
                        referredMemberId = userZaloId;
                    }
                    else if (shareType == "member")
                    {
                        // Share profile thành viên trong nhóm - chỉ lưu userZaloId, profile lấy từ Membership khi get
                        if (string.IsNullOrEmpty(request.ReferredMemberId))
                        {
                            throw new Exception("ReferredMemberId là bắt buộc khi ShareType = 'member'");
                        }

                        var referredMember = await _unitOfWork.GetRepository<Membership>()
                            .AsQueryable()
                            .FirstOrDefaultAsync(m => m.UserZaloId == request.ReferredMemberId && m.IsDelete != true);

                        if (referredMember == null)
                        {
                            throw new Exception("Không tìm thấy thành viên được chia sẻ");
                        }

                        // Xác định groupId cho người được share (ReferredMemberId)
                        var referredMemberGroupIdForValidation = request.ReferredMemberGroupId ?? request.GroupId;
                        if (string.IsNullOrEmpty(referredMemberGroupIdForValidation))
                        {
                            throw new Exception("GroupId cho người được share là bắt buộc khi ShareType = 'member'");
                        }

                        // Kiểm tra member có trong group được chọn không
                        var referredMemberGroup = await _unitOfWork.GetRepository<Group>()
                            .AsQueryable()
                            .FirstOrDefaultAsync(g => g.Id == referredMemberGroupIdForValidation);

                        if (referredMemberGroup == null)
                        {
                            throw new Exception("Không tìm thấy nhóm của người được share");
                        }

                        var referredMemberInGroup = await _unitOfWork.Context.MembershipGroups
                            .FirstOrDefaultAsync(mg => mg.UserZaloId == request.ReferredMemberId 
                                && mg.GroupId == referredMemberGroupIdForValidation 
                                && mg.IsApproved == true);

                        if (referredMemberInGroup == null)
                        {
                            throw new Exception($"Thành viên {referredMember.Fullname} không thuộc nhóm {referredMemberGroup.GroupName}");
                        }

                        referredMemberId = request.ReferredMemberId;
                    }
                    else // shareType == "external"
                    {
                        // Share text người ngoài - lưu text vì không có trong hệ thống
                        if (string.IsNullOrWhiteSpace(request.ReferralName) && string.IsNullOrWhiteSpace(request.ReferralPhone))
                        {
                            throw new Exception("Vui lòng nhập ít nhất Tên hoặc Số điện thoại của người được giới thiệu");
                        }

                        referralName = request.ReferralName;
                        referralPhone = request.ReferralPhone;
                        referralEmail = request.ReferralEmail;
                        referralAddress = request.ReferralAddress;
                        referredMemberId = null;
                    }

                    // Xác định groupId cho người được share để lưu vào DB
                    var referredMemberGroupId = request.ReferredMemberGroupId ?? request.GroupId;

                    // Tạo ref mới cho Type 1
                    var refEntity = new Ref
                    {
                        RefFrom = userZaloId, // Lưu userZaloId người gửi
                        RefTo = null, // Type 1: recipient là người ngoài
                        Content = request.Content,
                        Status = 1, // Đã gửi
                        Value = 0,
                        RefToGroupId = null, // Type 1: không có người nhận là member
                        ReferredMemberGroupId = referredMemberId != null ? referredMemberGroupId : null, // Nhóm của người được share (nếu là member)
                        Type = 1,
                        ReferredMemberId = referredMemberId, // userZaloId nếu là member, null nếu là người ngoài
                        // Chỉ lưu text khi là người ngoài (external)
                        ReferralName = referredMemberId == null ? referralName : null,
                        ReferralPhone = referredMemberId == null ? referralPhone : null,
                        ReferralEmail = referredMemberId == null ? referralEmail : null,
                        ReferralAddress = referredMemberId == null ? referralAddress : null,
                        RecipientName = request.RecipientName, // Tên người nhận bên ngoài
                        RecipientPhone = request.RecipientPhone, // SĐT người nhận bên ngoài
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _unitOfWork.GetRepository<Ref>().AddAsync(refEntity);
                    await _unitOfWork.SaveChangesAsync();

                    // ✅ LOG REFERRAL
                    if (_referralLogService != null)
                    {
                        try
                        {
                            await _referralLogService.LogReferralAsync(new CreateReferralLogDto
                            {
                                ReferrerId = userZaloId,
                                RefereeId = string.Empty, // Type 1: external recipient (không phải member)
                                GroupId = request.GroupId ?? string.Empty,
                                ReferralCode = null,
                                Source = "REF_EXTERNAL",
                                Metadata = JsonSerializer.Serialize(new
                                {
                                    refId = refEntity.Id,
                                    type = 1,
                                    shareType = shareType,
                                    content = request.Content,
                                    referrerName = fromMember.Fullname,
                                    recipientName = request.RecipientName,
                                    recipientPhone = request.RecipientPhone,
                                    referredMemberId = refEntity.ReferredMemberId,
                                    referralName = refEntity.ReferralName,
                                    referralPhone = refEntity.ReferralPhone,
                                    referralEmail = refEntity.ReferralEmail,
                                    referralAddress = refEntity.ReferralAddress,
                                    shareTime = DateTime.Now,
                                    status = refEntity.Status
                                })
                            });
                        }
                        catch (Exception logEx)
                        {
                            _logger.LogWarning(logEx, "Failed to log referral for ref {RefId}", refEntity.Id);
                        }
                    }

                    return await GetRefByIdAsync(refEntity.Id) ?? throw new Exception("Không thể tạo ref");
                }
                else
                {
                    throw new Exception("Type không hợp lệ. Chỉ chấp nhận 0 (thành viên) hoặc 1 (bên ngoài)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ref from {UserZaloId}, Type: {Type}", userZaloId, request.Type);
                throw;
            }
        }

        public async Task<PagedResult<RefDTO>> GetMyRefsAsync(string userZaloId, int page = 1, int pageSize = 10, string? keyword = null, byte? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? type = null)
        {
            try
            {
                var baseQuery = _unitOfWork.GetRepository<Ref>()
                    .AsQueryable();

                // Filter by type (sent/received)
                if (type == "sent")
                {
                    baseQuery = baseQuery.Where(r => r.RefFrom == userZaloId);
                }
                else if (type == "received")
                {
                    baseQuery = baseQuery.Where(r => r.RefTo == userZaloId);
                }
                else
                {
                    // type = null hoặc "all" - lấy tất cả
                    baseQuery = baseQuery.Where(r => r.RefFrom == userZaloId || r.RefTo == userZaloId);
                }

                // Apply filters
                if (!string.IsNullOrEmpty(keyword))
                {
                    baseQuery = baseQuery.Where(r => r.Content != null && r.Content.Contains(keyword));
                }

                if (status.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.Status == status.Value);
                }

                if (fromDate.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.CreatedDate >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.CreatedDate <= toDate.Value.AddDays(1));
                }

                var totalCount = await baseQuery.CountAsync();
                var items = await baseQuery
                    .Include(r => r.FromMember)
                    .Include(r => r.ToMember)
                    .OrderByDescending(r => r.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Fetch group names for all refs
                var refDTOs = new List<RefDTO>();
                foreach (var refEntity in items)
                {
                    refDTOs.Add(await MapToDTO(refEntity, null, null));
                }

                return new PagedResult<RefDTO>
                {
                    Items = refDTOs,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refs for user {UserZaloId}", userZaloId);
                throw;
            }
        }

        public async Task<RefDTO> UpdateRefValueAsync(string refId, UpdateRefValueRequest request)
        {
            try
            {
                var refEntity = await _unitOfWork.GetRepository<Ref>()
                    .AsQueryable()
                    .Where(r => r.Id == refId)
                    .FirstOrDefaultAsync();

                if (refEntity == null)
                {
                    throw new Exception("Không tìm thấy ref");
                }

                refEntity.Value = request.Value;

                // Update rating and feedback
                if (request.Rating.HasValue)
                {
                    if (request.Rating < 1 || request.Rating > 5)
                        throw new Exception("Đánh giá phải từ 1 đến 5 sao");

                    refEntity.Rating = request.Rating.Value;
                    refEntity.RatingDate = DateTime.Now;
                }

                if (!string.IsNullOrWhiteSpace(request.Feedback))
                {
                    if (request.Feedback.Length > 500)
                        throw new Exception("Nhận xét không được vượt quá 500 ký tự");

                    refEntity.Feedback = request.Feedback.Trim();
                }

                refEntity.Status = 3; // Hoàn thành
                refEntity.UpdatedDate = DateTime.Now;

                _unitOfWork.GetRepository<Ref>().Update(refEntity);
                await _unitOfWork.SaveChangesAsync();

                // Recalculate average rating for sender (who created the ref)
                if (request.Rating.HasValue)
                {
                    _logger.LogInformation("Updating rating for sender: {RefFrom}", refEntity.RefFrom);
                    await UpdateMemberAverageRatingAsync(refEntity.RefFrom);
                }

                return await GetRefByIdAsync(refId) ?? throw new Exception("Không thể cập nhật ref");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ref value for {RefId}", refId);
                throw;
            }
        }

        public async Task<RefDTO?> GetRefByIdAsync(string refId)
        {
            try
            {
                var refEntity = await _unitOfWork.GetRepository<Ref>()
                    .AsQueryable()
                    .Where(r => r.Id == refId)
                    .Include(r => r.FromMember)
                    .Include(r => r.ToMember)
                    .FirstOrDefaultAsync();

                if (refEntity == null) return null;

                return await MapToDTO(refEntity, null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ref {RefId}", refId);
                throw;
            }
        }

        private async Task<RefDTO> MapToDTO(Ref refEntity, string? groupName = null, string? toMemberGroupNames = null)
        {
            // Xác định ShareType
            string? shareType = null;
            if (!string.IsNullOrEmpty(refEntity.ReferredMemberId))
            {
                // Có ReferredMemberId -> là member
                if (refEntity.ReferredMemberId == refEntity.RefFrom)
                {
                    shareType = "own"; // Profile bản thân
                }
                else
                {
                    shareType = "member"; // Profile thành viên
                }
            }
            else if (!string.IsNullOrWhiteSpace(refEntity.ReferralName) || !string.IsNullOrWhiteSpace(refEntity.ReferralPhone))
            {
                shareType = "external"; // Soạn text người ngoài
            }

            // Load profile người được share (nếu là member)
            Membership? referredMember = null;
            if (!string.IsNullOrEmpty(refEntity.ReferredMemberId))
            {
                referredMember = await _unitOfWork.GetRepository<Membership>()
                    .AsQueryable()
                    .FirstOrDefaultAsync(m => m.UserZaloId == refEntity.ReferredMemberId && m.IsDelete != true);
            }

            // Load group names
            var refToGroupName = await GetGroupNameByIdAsync(refEntity.RefToGroupId);
            var referredMemberGroupName = await GetGroupNameByIdAsync(refEntity.ReferredMemberGroupId);

            return new RefDTO
            {
                Id = refEntity.Id,
                RefFrom = refEntity.RefFrom, // userZaloId người gửi
                RefTo = refEntity.RefTo, // userZaloId người nhận (Type 0) hoặc null (Type 1)
                Content = refEntity.Content,
                Status = refEntity.Status,
                StatusText = GetStatusText(refEntity.Status),
                Value = refEntity.Value,
                RefToGroupId = refEntity.RefToGroupId,
                RefToGroupName = refToGroupName,
                ReferredMemberGroupId = refEntity.ReferredMemberGroupId,
                ReferredMemberGroupName = referredMemberGroupName,
                Type = refEntity.Type,
                TypeText = GetTypeText(refEntity.Type),
                ShareType = shareType,
                CreatedDate = refEntity.CreatedDate,
                UpdatedDate = refEntity.UpdatedDate,
                // Rating & Feedback fields
                Rating = refEntity.Rating,
                Feedback = refEntity.Feedback,
                RatingDate = refEntity.RatingDate,
                // Profile người gửi (từ RefFrom)
                FromMemberName = refEntity.FromMember?.Fullname,
                FromMemberCompany = null,
                FromMemberPosition = null,
                FromMemberPhone = refEntity.FromMember?.PhoneNumber,
                FromMemberEmail = null,
                FromMemberAvatar = refEntity.FromMember?.ZaloAvatar,
                FromMemberSlug = refEntity.FromMember?.Slug,
                // Profile người nhận (từ RefTo - chỉ Type 0)
                ToMemberName = refEntity.ToMember?.Fullname,
                ToMemberCompany = null,
                ToMemberPosition = null,
                ToMemberPhone = refEntity.ToMember?.PhoneNumber,
                ToMemberEmail = null,
                ToMemberAvatar = refEntity.ToMember?.ZaloAvatar,
                ToMemberSlug = refEntity.ToMember?.Slug,
                // Profile người được share (từ ReferredMemberId - nếu là member)
                ReferredMemberId = refEntity.ReferredMemberId,
                ReferredMemberName = referredMember?.Fullname,
                ReferredMemberCompany = null,
                ReferredMemberPosition = null,
                ReferredMemberPhone = referredMember?.PhoneNumber,
                ReferredMemberEmail = null,
                ReferredMemberAvatar = referredMember?.ZaloAvatar,
                ReferredMemberSlug = referredMember?.Slug,
                // Thông tin người ngoài (nếu ShareType = "external")
                ReferralName = refEntity.ReferralName,
                ReferralPhone = refEntity.ReferralPhone,
                ReferralEmail = refEntity.ReferralEmail,
                ReferralAddress = refEntity.ReferralAddress,
                // Thông tin người nhận bên ngoài (Type 1)
                RecipientName = refEntity.RecipientName,
                RecipientPhone = refEntity.RecipientPhone,
                // Group information
                FromMemberGroupIds = toMemberGroupNames,
                FromMemberGroupNames = toMemberGroupNames,
                ToMemberGroupIds = toMemberGroupNames,
                ToMemberGroupNames = toMemberGroupNames
            };
        }

        private static string GetStatusText(byte status)
        {
            return status switch
            {
                1 => "Đã gửi",
                2 => "Đã nhận",
                3 => "Hoàn thành",
                _ => "Không xác định"
            };
        }

        private static string GetTypeText(byte type)
        {
            return type switch
            {
                0 => "Gửi cho thành viên",
                1 => "Gửi cho bên ngoài",
                _ => "Không xác định"
            };
        }

        // Calculate and update average rating for member
        private async Task UpdateMemberAverageRatingAsync(string? userZaloId)
        {
            if (string.IsNullOrEmpty(userZaloId)) return;

            try
            {
                // Get all completed refs with ratings that this user sent (as sender)
                var completedRefsWithRatings = await _unitOfWork.GetRepository<Ref>()
                    .AsQueryable()
                    .Where(r => r.RefFrom == userZaloId
                             && r.Status == 3
                             && r.Rating.HasValue)
                    .ToListAsync();

                if (completedRefsWithRatings.Count == 0)
                {
                    // No ratings yet - AverageRating and TotalRatings fields removed from Membership
                    return;
                }

                // Calculate average rating - fields removed from Membership, just log
                var averageRating = (decimal)completedRefsWithRatings.Average(r => r.Rating!.Value);
                var totalRatings = completedRefsWithRatings.Count;

                _logger.LogInformation(
                    "Calculated average rating for {UserZaloId}: {AverageRating} ({TotalRatings} ratings) - Note: Rating fields removed from Membership",
                    userZaloId, averageRating, totalRatings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating average rating for {UserZaloId}", userZaloId);
                // Don't throw - this is a background calculation
            }
        }

        // Helper method to get user's group IDs
        public async Task<List<string>> GetUserGroupIdsAsync(string userZaloId)
        {
            try
            {
                return await _unitOfWork.Context.MembershipGroups
                    .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                    .Select(mg => mg.GroupId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user group IDs for {UserZaloId}", userZaloId);
                return new List<string>();
            }
        }

        // Helper method to get group name by GroupId
        private async Task<string?> GetGroupNameByIdAsync(string? groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return null;

            try
            {
                var group = await _unitOfWork.Context.Groups
                    .Where(g => g.Id == groupId)
                    .Select(g => g.GroupName)
                    .FirstOrDefaultAsync();

                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group name for {GroupId}", groupId);
                return null;
            }
        }

        // Helper method to get member's group names (comma-separated)
        private async Task<string?> GetMemberGroupNamesAsync(string? userZaloId)
        {
            if (string.IsNullOrEmpty(userZaloId))
                return null;

            try
            {
                var groupNames = await _unitOfWork.Context.MembershipGroups
                    .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                    .Include(mg => mg.Group)
                    .Select(mg => mg.Group.GroupName)
                    .ToListAsync();

                return groupNames.Any() ? string.Join(", ", groupNames) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member group names for {UserZaloId}", userZaloId);
                return null;
            }
        }

        // CMS Methods for viewing and managing refs
        public async Task<PagedResult<RefDTO>> GetRefsForCMSAsync(RefQueryParameters queryParameters, List<string>? allowedGroupIds = null)
        {
            try
            {
                IQueryable<Ref> query = _unitOfWork.GetRepository<Ref>()
                    .AsQueryable()
                    .Include(r => r.FromMember)
                    .Include(r => r.ToMember);

                // Filter by allowed group IDs (for ADMIN users)
                if (allowedGroupIds != null && allowedGroupIds.Any())
                {
                    var userZaloIdsInAllowedGroups = await _unitOfWork.Context.MembershipGroups
                        .Where(mg => allowedGroupIds.Contains(mg.GroupId) && mg.IsApproved == true)
                        .Select(mg => mg.UserZaloId)
                        .Distinct()
                        .ToListAsync();

                    query = query.Where(r =>
                        userZaloIdsInAllowedGroups.Contains(r.RefFrom ?? "") ||
                        userZaloIdsInAllowedGroups.Contains(r.RefTo ?? ""));
                }

                // Apply filters
                if (!string.IsNullOrEmpty(queryParameters.Keyword))
                {
                    query = query.Where(r =>
                        (r.FromMember != null && r.FromMember.Fullname.Contains(queryParameters.Keyword)) ||
                        (r.ToMember != null && r.ToMember.Fullname.Contains(queryParameters.Keyword)) ||
                        (r.Content != null && r.Content.Contains(queryParameters.Keyword)));
                }

                if (queryParameters.Status.HasValue)
                {
                    query = query.Where(r => r.Status == queryParameters.Status.Value);
                }

                if (queryParameters.Type.HasValue)
                {
                    query = query.Where(r => r.Type == queryParameters.Type.Value);
                }

                if (queryParameters.FromDate.HasValue)
                {
                    query = query.Where(r => r.CreatedDate >= queryParameters.FromDate.Value);
                }

                if (queryParameters.ToDate.HasValue)
                {
                    var toDateEnd = queryParameters.ToDate.Value.AddDays(1);
                    query = query.Where(r => r.CreatedDate < toDateEnd);
                }

                if (queryParameters.MinRating.HasValue)
                {
                    query = query.Where(r => r.Rating >= queryParameters.MinRating.Value);
                }

                if (queryParameters.MaxRating.HasValue)
                {
                    query = query.Where(r => r.Rating <= queryParameters.MaxRating.Value);
                }

                // Apply sorting
                query = queryParameters.SortBy switch
                {
                    "value" => queryParameters.SortOrder == "desc"
                        ? query.OrderByDescending(r => r.Value)
                        : query.OrderBy(r => r.Value),
                    "rating" => queryParameters.SortOrder == "desc"
                        ? query.OrderByDescending(r => r.Rating)
                        : query.OrderBy(r => r.Rating),
                    "date" => queryParameters.SortOrder == "desc"
                        ? query.OrderByDescending(r => r.CreatedDate)
                        : query.OrderBy(r => r.CreatedDate),
                    _ => query.OrderByDescending(r => r.CreatedDate)
                };

                // Get total count
                var totalItems = await query.CountAsync();

                // Apply pagination
                var refEntities = await query
                    .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                    .Take(queryParameters.PageSize)
                    .ToListAsync();

                // Fetch group information for each ref
                var items = new List<RefDTO>();
                foreach (var refEntity in refEntities)
                {
                    items.Add(await MapToDTO(refEntity, null, null));
                }

                return new PagedResult<RefDTO>
                {
                    Items = items,
                    TotalItems = totalItems,
                    Page = queryParameters.Page,
                    PageSize = queryParameters.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / queryParameters.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refs for CMS");
                throw;
            }
        }
    }
}
