using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Request.Memberships;
using MiniAppGIBA.Service.Authencation;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Services.Commons;
using MiniAppGIBA.Services.Logs;
using MiniAppGIBA.Services.Memberships;
using MiniAppGIBA.Models.DTOs.Memberships;
using MiniAppGIBA.Models.DTOs.Logs;
using System.Text.Json;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Services.Groups;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Base.Helpers;
using Internal;
namespace MiniAppGIBA.Controller.API
{

    [Route("api/memberships")]
    [ApiController]
    public class MembershipsController : BaseAPIController
    {
        private readonly ILogger<MembershipsController> _logger;
        private readonly IAuthencationService _authService;
        private readonly IRepository<Membership> _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly ISystemConfigService _systemConfigService;
        private readonly IProfileShareLogService _profileShareLogService;
        private readonly IProfileTemplateService _profileTemplateService;
        private readonly IProfileCustomFieldService _profileCustomFieldService;
        private readonly IMembershipGroupService _membershipGroupService;
        private readonly IMembershipProfileService _membershipProfileService;

        public MembershipsController(
            ILogger<MembershipsController> logger,
            IAuthencationService authService,
            IRepository<Membership> membershipRepository,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment env,
            ISystemConfigService systemConfigService,
            IMembershipGroupService membershipGroupService,
            IMembershipProfileService membershipProfileService,
            IProfileShareLogService? profileShareLogService = null,
            IProfileTemplateService? profileTemplateService = null,
            IProfileCustomFieldService? profileCustomFieldService = null
            )
        {
            _logger = logger;
            _authService = authService;
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _env = env;
            _systemConfigService = systemConfigService;
            _profileShareLogService = profileShareLogService;
            _profileTemplateService = profileTemplateService;
            _profileCustomFieldService = profileCustomFieldService;
            _membershipGroupService = membershipGroupService;
            _membershipProfileService = membershipProfileService;
        }


        [HttpGet("get-membership-by-group")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetMembershipByGroup([FromQuery] string groupId)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return Error("GroupId không được để trống", 400);
                }

                var memberships = await _membershipGroupService.GetMembershipGroupByGroupIdAsync(groupId);

                // Return memberships (already filtered to only approved members)
                return Success(new
                {
                    message = "Thành công",
                    membership = memberships
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership by group");
                return Error("Internal Server Error", 500);
            }
        }
        /// <summary>
        /// Cập nhật logo công ty
        /// </summary>
        [HttpPut("update-company-logo")]
        public async Task<IActionResult> UpdateCompanyLogo([FromForm] UpdateCompanyLogoRequest request)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Error("Chưa đăng nhập", 401);
                }

                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Error("Không tìm thấy thông tin người dùng", 401);
                }

                if (request.CompanyLogoFile == null || request.CompanyLogoFile.Length == 0)
                {
                    return Error("Vui lòng chọn file logo", 400);
                }

                // Lấy membership hiện tại
                var membership = await _membershipRepository.AsQueryable()
                    .Where(m => m.UserZaloId == userZaloId && m.IsDelete != true)
                    .FirstOrDefaultAsync();

                if (membership == null)
                {
                    return Error("Không tìm thấy thông tin thành viên", 404);
                }

                // Upload file mới
                var newLogoPath = await ProcessCompanyLogoUpload(request.CompanyLogoFile);
                if (string.IsNullOrEmpty(newLogoPath))
                {
                    return Error("Lỗi khi upload logo", 500);
                }

                membership.UpdatedDate = DateTime.Now;

                await _unitOfWork.SaveChangesAsync();

                // Trả về URL đầy đủ
                var companyLogoUrl = ToFullUrl(newLogoPath);

                return Success(new
                {
                    message = "Cập nhật logo công ty thành công",
                    companyLogo = companyLogoUrl
                });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Update company logo failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company logo");
                return Error("Có lỗi xảy ra khi cập nhật logo", 500);
            }
        }

        [HttpGet("check-user-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserAdmin([FromQuery] string phoneNumber)
        {
            try
            {
                phoneNumber = PhoneNumberHandler.FixFormatPhoneNumber(phoneNumber);
                var existingMembership = await _membershipRepository.AsQueryable()
                    .Where(m => m.PhoneNumber == phoneNumber && m.IsDelete != true)
                    .Select(m => new
                    {
                        m.Id,
                        m.PhoneNumber,
                        m.Fullname,
                        m.ZaloAvatar,
                        m.RoleId
                    })
                    .FirstOrDefaultAsync();

                if (existingMembership != null && existingMembership.RoleId == null)
                {
                    var userInfo = new
                    {
                        id = existingMembership.Id,
                        phoneNumber = existingMembership.PhoneNumber ?? string.Empty,
                        fullname = existingMembership.Fullname ?? string.Empty,
                        avatar = existingMembership.ZaloAvatar ?? string.Empty
                    };
                    return Success(new
                    {
                        message = "Thành viên đã tồn tại!",
                        userInfo = userInfo
                    });
                }
                else if (existingMembership != null && existingMembership.RoleId != null)
                {
                    return Success(new
                    {
                        message = "Thành viên đã là admin! Vui lòng đăng nhập với tài khoản admin",
                    });
                }
                return Success(new
                {
                    message = "Thành viên không tồn tại!",
                });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Check user failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user");
                return Error("Internal Server Error", 500);
            }
        }
        [HttpGet("check-user")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUser([FromQuery] string phoneNumber, [FromQuery] string type)
        {
            try
            {
                phoneNumber = PhoneNumberHandler.FixFormatPhoneNumber(phoneNumber);
                Console.WriteLine("phoneNumber: " + phoneNumber);
                var query = _membershipRepository.AsQueryable().Where(
                    m => m.PhoneNumber == phoneNumber && m.IsDelete != true);
                var existingMembership = await query.FirstOrDefaultAsync();

                if (existingMembership != null)
                {
                    var userInfo = new
                    {
                        id = existingMembership.Id,
                        phoneNumber = existingMembership.PhoneNumber,
                        fullname = existingMembership.Fullname,
                        avatar = existingMembership.ZaloAvatar
                    };
                    return Success(new
                    {
                        userInfo = userInfo
                    });
                }
                return Error("no data!", 400);
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Check user failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user");
                return Error("Internal Server Error", 500);
            }
        }
        /// <summary>
        /// Đăng ký thành viên mới (Register from MiniApp)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterMemberRequest request)
        {
            try
            {
                var phone = PhoneNumberHandler.FixFormatPhoneNumber(request.PhoneNumber);

                // Kiểm tra xem user đã đăng ký chưa (cả PhoneNumber và UserZaloId phải khớp)
                var existingMembership = await _membershipRepository.GetFirstOrDefaultAsync(
                    m => m.PhoneNumber == phone && m.UserZaloId == request.UserZaloId && m.IsDelete != true);

                if (existingMembership != null)
                {
                    throw new CustomException(400, "Thành viên đã tồn tại!");
                }

                // Generate unique slug from Fullname
                var slug = await GenerateUniqueSlugAsync(request.Fullname);
                // Tạo membership mới với thông tin từ request (có thể null)
                Console.WriteLine("phone: " + phone);
                var membership = new Membership
                {
                    UserZaloId = request.UserZaloId,
                    UserZaloName = request.UserZaloName,
                    Fullname = request.Fullname,
                    Slug = slug,
                    PhoneNumber = phone,
                    ZaloAvatar = request.ZaloAvatar,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _membershipRepository.AddAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                return Success(new
                {
                    message = "Đăng ký thành công!",
                    membership = new
                    {
                        id = membership.Id,
                        userZaloId = membership.UserZaloId,
                        phoneNumber = membership.PhoneNumber,
                        fullname = membership.Fullname
                    }
                });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Register failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return Error("Internal Server Error", 500);
            }
        }
        [HttpPost("register-by-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterByAdmin([FromForm] RegisterMemberByAdminRequest request)
        {
            try
            {
                var existingMembership = await _membershipRepository.GetFirstOrDefaultAsync(
                    m => m.PhoneNumber == request.PhoneNumber && m.IsDelete != true);

                if (existingMembership != null)
                {
                    throw new CustomException(400, "Thành viên đã tồn tại!");
                }
                var userZaloId = Guid.NewGuid().ToString("N");
                var userZaloIdByOA = Guid.NewGuid().ToString("N");
                var slug = await GenerateUniqueSlugAsync(request.Fullname);
                var membership = new Membership
                {
                    UserZaloId = userZaloId,
                    UserZaloName = request.UserZaloName,
                    Fullname = request.Fullname,
                    Slug = slug,
                    PhoneNumber = request.PhoneNumber,
                    ZaloAvatar = request.ZaloAvatar,
                    UserZaloIdByOA = userZaloIdByOA,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _membershipRepository.AddAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                return Success(new
                {
                    message = "Đăng ký thành công!",
                    membership = new
                    {
                        id = membership.Id,
                        userZaloId = membership.UserZaloId,
                        phoneNumber = membership.PhoneNumber,
                        fullname = membership.Fullname
                    }
                });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Register failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Lấy thông tin profile của user hiện tại
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Error("Chưa đăng nhập", 401);
                }

                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Error("Không tìm thấy thông tin người dùng", 401);
                }

                var membership = await _membershipRepository.AsQueryable()
                    .Where(m => m.UserZaloId == userZaloId && m.IsDelete != true)
                    .FirstOrDefaultAsync();

                if (membership == null)
                {
                    return Error("Không tìm thấy thông tin thành viên", 404);
                }

                return Success(new
                {
                    id = membership.Id,
                    userZaloId = membership.UserZaloId,
                    userZaloName = membership.UserZaloName,
                    fullname = membership.Fullname,
                    slug = membership.Slug,
                    oldSlugs = membership.OldSlugs,
                    phoneNumber = membership.PhoneNumber,
                    zaloAvatar = membership.ZaloAvatar,
                    isDelete = membership.IsDelete,
                    createdDate = membership.CreatedDate,
                    updatedDate = membership.UpdatedDate,
                    // Personal information fields
                    fieldIds = membership.FieldIds,
                    profile = membership.Profile,
                    dayOfBirth = membership.DayOfBirth,
                    address = membership.Address,
                    position = membership.Position,
                    // Company information fields
                    companyFullName = membership.CompanyFullName,
                    companyBrandName = membership.CompanyBrandName,
                    taxCode = membership.TaxCode,
                    businessField = membership.BusinessField,
                    businessType = membership.BusinessType,
                    headquartersAddress = membership.HeadquartersAddress,
                    companyWebsite = membership.CompanyWebsite,
                    companyPhoneNumber = membership.CompanyPhoneNumber,
                    companyEmail = membership.CompanyEmail,
                    legalRepresentative = membership.LegalRepresentative,
                    legalRepresentativePosition = membership.LegalRepresentativePosition,
                    companyLogo = membership.CompanyLogo,
                    businessRegistrationNumber = membership.BusinessRegistrationNumber,
                    businessRegistrationDate = membership.BusinessRegistrationDate,
                    businessRegistrationPlace = membership.BusinessRegistrationPlace,
                    // Rating fields
                    averageRating = membership.AverageRating,
                    totalRatings = membership.TotalRatings,
                    // Other fields
                    appPosition = membership.AppPosition,
                    term = membership.Term,
                    code = membership.Code,
                    sortField = membership.SortField
                });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Get profile failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile");
                return Error("Internal Server Error", 500);
            }
        }
        private string ToFullUrl(string? relativePath)
        {
            if (relativePath == null)
                return null;
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            // If already full URL, return as is
            if (relativePath.StartsWith("http://") || relativePath.StartsWith("https://"))
                return relativePath;

            // Ensure path starts with /
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;

            // Build full URL
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return baseUrl + relativePath;
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrEmpty(userZaloId) && request.PhoneNumber == null)
                {
                    return Error("Không tìm thấy thông tin người dùng", 401);
                }

                var membership = await _membershipRepository.AsQueryable()
                    .Where(m => (m.UserZaloId == userZaloId || m.PhoneNumber == request.PhoneNumber) && m.IsDelete != true)
                    .FirstOrDefaultAsync();

                if (membership == null)
                {
                    return Error("Không tìm thấy thông tin thành viên", 404);
                }
                
                // Update UserZaloId using raw SQL to bypass EF Core change tracking
                if (request.UserZaloId != null && !string.IsNullOrEmpty(membership.UserZaloId) && request.UserZaloId != membership.UserZaloId)
                {
                    var oldUserZaloId = membership.UserZaloId;
                    var newUserZaloId = request.UserZaloId;
                    
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        await _unitOfWork.ExecuteSqlRawAsync(
                            "UPDATE Memberships SET UserZaloId = {0} WHERE Id = {1}",
                            newUserZaloId, membership.Id);
                        
                        await _unitOfWork.ExecuteSqlRawAsync(
                            "UPDATE MembershipGroups SET UserZaloId = {0} WHERE UserZaloId = {1}",
                            newUserZaloId, oldUserZaloId);
                        
                        await _unitOfWork.ExecuteSqlRawAsync(
                            "UPDATE EventRegistrations SET UserZaloId = {0} WHERE UserZaloId = {1}",
                            newUserZaloId, oldUserZaloId);
                        
                        await _unitOfWork.ExecuteSqlRawAsync(
                            "UPDATE Refs SET RefFrom = {0} WHERE RefFrom = {1}",
                            newUserZaloId, oldUserZaloId);
                        
                        await _unitOfWork.ExecuteSqlRawAsync(
                            "UPDATE Refs SET RefTo = {0} WHERE RefTo = {1}",
                            newUserZaloId, oldUserZaloId);
                        
                        await _unitOfWork.CommitAsync();
                        
                        var reloadedMembership = await _membershipRepository.AsQueryable()
                            .Where(m => m.Id == membership.Id && m.IsDelete != true)
                            .FirstOrDefaultAsync();
                        
                        if (reloadedMembership != null)
                        {
                            membership = reloadedMembership;
                        }
                    }
                    catch
                    {
                        await _unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                
                if (request.UserZaloName != null)
                    membership.UserZaloName = request.UserZaloName;
                if (request.UserZaloIdByOA != null)
                    membership.UserZaloIdByOA = request.UserZaloIdByOA;
                if (!string.IsNullOrEmpty(request.Fullname))
                    membership.Fullname = request.Fullname;
                if (request.ZaloAvatar != null)
                    membership.ZaloAvatar = request.ZaloAvatar;
                if (request.PhoneNumber != null)
                    membership.PhoneNumber = request.PhoneNumber;
                if (request.RoleId != null)
                    membership.RoleId = request.RoleId;
                
                // Personal information fields
                if (request.FieldIds != null)
                    membership.FieldIds = request.FieldIds;
                if (request.Profile != null)
                    membership.Profile = request.Profile;
                if (request.DayOfBirth != null)
                    membership.DayOfBirth = request.DayOfBirth;
                if (request.Address != null)
                    membership.Address = request.Address;
                if (request.Position != null)
                    membership.Position = request.Position;
                
                // Company information fields
                if (request.CompanyFullName != null)
                    membership.CompanyFullName = request.CompanyFullName;
                if (request.CompanyBrandName != null)
                    membership.CompanyBrandName = request.CompanyBrandName;
                if (request.TaxCode != null)
                    membership.TaxCode = request.TaxCode;
                if (request.BusinessField != null)
                    membership.BusinessField = request.BusinessField;
                if (request.BusinessType != null)
                    membership.BusinessType = request.BusinessType;
                if (request.HeadquartersAddress != null)
                    membership.HeadquartersAddress = request.HeadquartersAddress;
                if (request.CompanyWebsite != null)
                    membership.CompanyWebsite = request.CompanyWebsite;
                if (request.CompanyPhoneNumber != null)
                    membership.CompanyPhoneNumber = request.CompanyPhoneNumber;
                if (request.CompanyEmail != null)
                    membership.CompanyEmail = request.CompanyEmail;
                if (request.LegalRepresentative != null)
                    membership.LegalRepresentative = request.LegalRepresentative;
                if (request.LegalRepresentativePosition != null)
                    membership.LegalRepresentativePosition = request.LegalRepresentativePosition;
                if (request.CompanyLogo != null)
                    membership.CompanyLogo = request.CompanyLogo;
                if (request.BusinessRegistrationNumber != null)
                    membership.BusinessRegistrationNumber = request.BusinessRegistrationNumber;
                if (request.BusinessRegistrationDate != null)
                    membership.BusinessRegistrationDate = request.BusinessRegistrationDate;
                if (request.BusinessRegistrationPlace != null)
                    membership.BusinessRegistrationPlace = request.BusinessRegistrationPlace;
                
                // Other fields
                if (request.AppPosition != null)
                    membership.AppPosition = request.AppPosition;
                if (request.Term != null)
                    membership.Term = request.Term;
                if (request.SortField != null)
                    membership.SortField = request.SortField;

                membership.UpdatedDate = DateTime.Now;

                await _unitOfWork.SaveChangesAsync();

                return Success(new
                {
                    message = "Cập nhật thông tin thành công!",
                    data = new
                    {
                        id = membership.Id,
                        fullname = membership.Fullname,
                        phoneNumber = membership.PhoneNumber,
                        zaloAvatar = membership.ZaloAvatar
                    }
                });
            }
            catch (CustomException ex)
            {
                _logger.LogWarning($"Update profile failed: {ex.Message}");
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return Error("Internal Server Error", 500);
            }
        }
        [HttpGet("{userZaloId}")]
        public async Task<IActionResult> GetMemberProfile(string userZaloId)
        {
            try
            {
                var membership = await _membershipRepository.AsQueryable()
                    .Where(m => m.UserZaloId == userZaloId && m.IsDelete != true)
                    .FirstOrDefaultAsync();

                if (membership == null)
                {
                    return Error("Không tìm thấy thông tin thành viên", 404);
                }

                return Success(new
                {
                    id = membership.Id,
                    userZaloId = membership.UserZaloId,
                    userZaloName = membership.UserZaloName,
                    fullname = membership.Fullname,
                    slug = membership.Slug,
                    oldSlugs = membership.OldSlugs,
                    phoneNumber = membership.PhoneNumber,
                    zaloAvatar = membership.ZaloAvatar,
                    isDelete = membership.IsDelete,
                    createdDate = membership.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    updatedDate = membership.UpdatedDate.ToString("dd/MM/yyyy HH:mm"),
                    // Personal information fields
                    fieldIds = membership.FieldIds,
                    profile = membership.Profile,
                    dayOfBirth = membership.DayOfBirth,
                    address = membership.Address,
                    position = membership.Position,
                    // Company information fields
                    companyFullName = membership.CompanyFullName,
                    companyBrandName = membership.CompanyBrandName,
                    taxCode = membership.TaxCode,
                    businessField = membership.BusinessField,
                    businessType = membership.BusinessType,
                    headquartersAddress = membership.HeadquartersAddress,
                    companyWebsite = membership.CompanyWebsite,
                    companyPhoneNumber = membership.CompanyPhoneNumber,
                    companyEmail = membership.CompanyEmail,
                    legalRepresentative = membership.LegalRepresentative,
                    legalRepresentativePosition = membership.LegalRepresentativePosition,
                    companyLogo = membership.CompanyLogo,
                    businessRegistrationNumber = membership.BusinessRegistrationNumber,
                    businessRegistrationDate = membership.BusinessRegistrationDate,
                    businessRegistrationPlace = membership.BusinessRegistrationPlace,
                    // Rating fields
                    averageRating = membership.AverageRating,
                    totalRatings = membership.TotalRatings,
                    // Other fields
                    appPosition = membership.AppPosition,
                    term = membership.Term,
                    code = membership.Code,
                    sortField = membership.SortField
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member profile for UserZaloId: {UserZaloId}", userZaloId);
                return Error("Internal Server Error", 500);
            }
        }


        /// <summary>
        /// </summary>
        [AllowAnonymous]
        [HttpGet("profile/slug/{slug}")]
        public async Task<IActionResult> GetProfileBySlug(string slug, [FromQuery] string? groupId = null, [FromQuery] string? shareMethod = null)
        {
            try
            {
                var membership = await _membershipRepository.AsQueryable()
                    .Where(m => m.Slug == slug && m.IsDelete != true)
                    .FirstOrDefaultAsync();

                if (membership == null)
                {
                    return Error("Không tìm thấy thành viên", 404);
                }
                string? groupName = null;
                if (!string.IsNullOrEmpty(groupId))
                {
                    var groupRepo = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Groups.Group>();
                    var membershipGroupRepo = _unitOfWork.GetRepository<MiniAppGIBA.Entities.Groups.MembershipGroup>();

                    var membershipGroup = await membershipGroupRepo.AsQueryable()
                        .Where(mg => mg.UserZaloId == membership.UserZaloId
                                  && mg.GroupId == groupId
                                  && mg.IsApproved == true)
                        .FirstOrDefaultAsync();

                    if (membershipGroup != null)
                    {
                        var group = await groupRepo.FindByIdAsync(groupId);
                        groupName = group?.GroupName;
                    }
                }

                if (_profileShareLogService != null && !string.IsNullOrEmpty(shareMethod))
                {
                    try
                    {
                        var sharedData = new
                        {
                            profileInfo = new
                            {
                                userZaloId = membership.UserZaloId,
                                fullname = membership.Fullname,
                                phone = membership.PhoneNumber
                            },
                            accessTime = DateTime.Now,
                            shareUrl = $"/api/memberships/profile/slug/{membership.Slug}"
                        };

                        await _profileShareLogService.LogProfileShareAsync(new CreateProfileShareLogDto
                        {
                            SharerId = membership.UserZaloId,
                            ReceiverId = "EXTERNAL",
                            GroupId = groupId ?? "EXTERNAL",
                            SharedData = JsonSerializer.Serialize(sharedData),
                            ShareMethod = shareMethod,
                            Metadata = JsonSerializer.Serialize(new
                            {
                                accessTime = DateTime.Now,
                                shareUrl = $"/api/memberships/profile/slug/{membership.Slug}",
                                isProfileAccess = true,
                                platform = shareMethod
                            })
                        });
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogWarning(logEx, "Failed to log profile access for {Slug}", slug);
                    }
                }

                return Success(new
                {
                    id = membership.Id,
                    slug = membership.Slug,
                    oldSlugs = membership.OldSlugs,
                    userZaloId = membership.UserZaloId,
                    userZaloName = membership.UserZaloName,
                    fullname = membership.Fullname,
                    phoneNumber = membership.PhoneNumber,
                    avatar = membership.ZaloAvatar,
                    groupName = groupName,
                    isDelete = membership.IsDelete,
                    createdDate = membership.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    updatedDate = membership.UpdatedDate.ToString("dd/MM/yyyy HH:mm"),
                    // Personal information fields
                    fieldIds = membership.FieldIds,
                    profile = membership.Profile,
                    dayOfBirth = membership.DayOfBirth,
                    address = membership.Address,
                    position = membership.Position,
                    // Company information fields
                    companyFullName = membership.CompanyFullName,
                    companyBrandName = membership.CompanyBrandName,
                    taxCode = membership.TaxCode,
                    businessField = membership.BusinessField,
                    businessType = membership.BusinessType,
                    headquartersAddress = membership.HeadquartersAddress,
                    companyWebsite = membership.CompanyWebsite,
                    companyPhoneNumber = membership.CompanyPhoneNumber,
                    companyEmail = membership.CompanyEmail,
                    legalRepresentative = membership.LegalRepresentative,
                    legalRepresentativePosition = membership.LegalRepresentativePosition,
                    companyLogo = membership.CompanyLogo,
                    businessRegistrationNumber = membership.BusinessRegistrationNumber,
                    businessRegistrationDate = membership.BusinessRegistrationDate,
                    businessRegistrationPlace = membership.BusinessRegistrationPlace,
                    // Rating fields
                    averageRating = membership.AverageRating,
                    totalRatings = membership.TotalRatings,
                    // Other fields
                    appPosition = membership.AppPosition,
                    term = membership.Term,
                    code = membership.Code,
                    sortField = membership.SortField
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile by slug: {Slug}", slug);
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Lấy thông tin profile đầy đủ theo slug cho template (Public API - No Auth Required)
        /// Trả về toàn bộ thông tin người dùng với các giá trị thực tế
        /// </summary>
        [AllowAnonymous]
        [HttpGet("giba/profile/slug/{slug}")]
        public async Task<IActionResult> GetGibaProfileBySlug(string slug, [FromQuery] string? groupId = null)
        {
            try
            {
                var profileData = await _membershipProfileService.GetProfileBySlugAsync(slug, groupId);

                if (profileData == null)
                {
                    return Error("Không tìm thấy thành viên", 404);
                }

                return Success(new
                {
                    success = true,
                    message = "Lấy thông tin profile thành công",
                    data = profileData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Giba profile by slug: {Slug}", slug);
                return Error("Internal Server Error", 500);
            }
        }

        #region ADMIN APIs

        /// <summary>
        /// [ADMIN] Lấy danh sách thành viên
        /// </summary>
        [HttpGet]
        [Authorize(Roles = CTRole.GIBA)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // TODO: Implement GetPagedMemberships service
                // var result = await _membershipService.GetPagedAsync(page, pageSize);

                return Success(new
                {
                    data = new List<object>(),
                    totalPages = 0,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memberships");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// [ADMIN] Cập nhật thông tin thành viên
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = CTRole.GIBA)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateProfileRequest request)
        {
            try
            {
                // TODO: Implement UpdateMembership service
                // await _membershipService.UpdateAsync(id, request);

                return Success("Cập nhật thành công!");
            }
            catch (CustomException ex)
            {
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating membership");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// [ADMIN] Xóa thành viên
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = CTRole.GIBA)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // TODO: Implement DeleteMembership service
                // await _membershipService.DeleteAsync(id);

                return Success("Xóa thành công!");
            }
            catch (CustomException ex)
            {
                return Error(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting membership");
                return Error("Internal Server Error", 500);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generate unique slug from fullname
        /// </summary>
        private async Task<string> GenerateUniqueSlugAsync(string fullname)
        {
            // Generate base slug
            var baseSlug = SlugGenerator.ToSlug(fullname);

            // If empty or reserved, use fallback
            if (string.IsNullOrEmpty(baseSlug) || SlugGenerator.IsReservedSlug(baseSlug))
            {
                baseSlug = $"user-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            }

            // Check uniqueness
            var slug = baseSlug;
            var counter = 1;

            while (await _membershipRepository.AsQueryable().AnyAsync(m => m.Slug == slug && m.IsDelete != true))
            {
                slug = SlugGenerator.AppendNumber(baseSlug, counter);
                counter++;

                // Prevent infinite loop (max 1000 attempts)
                if (counter > 1000)
                {
                    slug = $"{baseSlug}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    break;
                }
            }

            return slug;
        }

        private async Task<string?> ProcessCompanyLogoUpload(IFormFile? logoFile)
        {
            if (logoFile == null || logoFile.Length == 0)
                return null;

            try
            {
                var savePath = Path.Combine(_env.WebRootPath, "uploads", "images", "company-logos");
                var fileName = await FileHandler.SaveFile(logoFile, savePath);
                return $"/uploads/images/company-logos/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing company logo upload");
                throw new Exception($"Lỗi khi upload logo công ty: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy quy tắc ứng xử cho Mini App
        /// </summary>
        [AllowAnonymous]
        [HttpGet("behavior-rules")]
        public async Task<IActionResult> GetBehaviorRules()
        {
            try
            {
                var rule = await _systemConfigService.GetByTypeAsync("BehaviorRules");

                if (rule == null || string.IsNullOrEmpty(rule.Content))
                {
                    return Success<object>(null);
                }

                return Success(new
                {
                    content = rule.Content,
                    lastUpdated = rule.UpdatedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting behavior rules");
                return Error("Internal Server Error", 500);
            }
        }

        #endregion

        #region Profile Template APIs

        /// <summary>
        /// Lấy cấu hình profile template của người dùng hiện tại
        /// </summary>
        [HttpGet("profile-template")]
        public async Task<IActionResult> GetProfileTemplate()
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Error("Chưa đăng nhập", 401);
                }

                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Error("Không tìm thấy thông tin người dùng", 401);
                }

                if (_profileTemplateService == null)
                {
                    return Error("Service không khả dụng", 500);
                }

                var template = await _profileTemplateService.GetTemplateAsync(userZaloId);
                if (template == null)
                {
                    return Success(new { message = "Chưa có cấu hình template" });
                }

                return Success(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile template");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Cập nhật cấu hình profile template
        /// </summary>
        [HttpPut("profile-template")]
        public async Task<IActionResult> UpdateProfileTemplate([FromBody] UpdateProfileTemplateDto dto)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Error("Chưa đăng nhập", 401);
                }

                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Error("Không tìm thấy thông tin người dùng", 401);
                }

                if (_profileTemplateService == null)
                {
                    return Error("Service không khả dụng", 500);
                }

                var template = await _profileTemplateService.CreateOrUpdateTemplateAsync(userZaloId, dto);
                return Success(new { message = "Cập nhật cấu hình thành công", data = template });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile template");
                return Error("Internal Server Error", 500);
            }
        }
        [HttpGet("profile-template/custom-fields/{UserZaloId}")]
        public async Task<IActionResult> GetCustomFields(string userZaloId)
        {
            try
            {
                var customFields = await _profileCustomFieldService.GetCustomFieldsByUserZaloIdAsync(userZaloId);
                return Success(customFields);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom fields");
                return Error("Internal Server Error", 500);
            }
        }
        /// <summary>
        /// Thêm trường tùy chỉnh vào profile template
        /// </summary>
        [HttpPost("profile-template/custom-fields")]
        public async Task<IActionResult> AddCustomField([FromBody] AddCustomFieldDto dto)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Error("Chưa đăng nhập", 401);
                }

                var userZaloId = GetCurrentUserZaloId();
                if (string.IsNullOrEmpty(userZaloId))
                {
                    return Error("Không tìm thấy thông tin người dùng", 401);
                }


                var template = await _profileTemplateService.GetTemplateAsync(userZaloId);
                if (template == null)
                {
                    return Error("Chưa có cấu hình template", 404);
                }

                var customField = await _profileCustomFieldService.AddCustomFieldAsync(template.Id, dto);
                return Success(new { message = "Thêm trường tùy chỉnh thành công", data = customField });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding custom field");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Cập nhật trường tùy chỉnh
        /// </summary>
        [HttpPut("profile-template/custom-fields/{fieldId}")]
        public async Task<IActionResult> UpdateCustomField(string fieldId, [FromBody] UpdateCustomFieldDto dto)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Error("Chưa đăng nhập", 401);
                }

                if (_profileCustomFieldService == null)
                {
                    return Error("Service không khả dụng", 500);
                }

                var customField = await _profileCustomFieldService.UpdateCustomFieldAsync(fieldId, dto);
                return Success(new { message = "Cập nhật trường tùy chỉnh thành công", data = customField });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating custom field");
                return Error("Internal Server Error", 500);
            }
        }

        /// <summary>
        /// Xóa trường tùy chỉnh
        /// </summary>
        [HttpDelete("profile-template/custom-fields/{fieldId}")]
        public async Task<IActionResult> DeleteCustomField(string fieldId)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Error("Chưa đăng nhập", 401);
                }

                if (_profileCustomFieldService == null)
                {
                    return Error("Service không khả dụng", 500);
                }

                var result = await _profileCustomFieldService.DeleteCustomFieldAsync(fieldId);
                if (!result)
                {
                    return Error("Không tìm thấy trường tùy chỉnh", 404);
                }

                return Success(new { message = "Xóa trường tùy chỉnh thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting custom field");
                return Error("Internal Server Error", 500);
            }
        }


        #endregion
    }
}





