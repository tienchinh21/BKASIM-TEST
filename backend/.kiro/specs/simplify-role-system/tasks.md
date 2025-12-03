# Implementation Plan

- [ ] 1. Cập nhật Constants và Base Controller

  - [x] 1.1 Cập nhật CTRole.cs - chỉ giữ GIBA constant

    - Xóa constants Club, NBD, Group
    - Giữ lại chỉ GIBA constant
    - _Requirements: 1.1, 1.2_

  - [x] 1.2 Cập nhật BaseCMSController.cs

    - Đơn giản hóa IsAdmin() - chỉ check GIBA
    - Đơn giản hóa GetCurrentUserRole() - chỉ return GIBA hoặc null
    - Đơn giản hóa GetUserGroupIdsOrNull() - luôn return null cho GIBA
    - Xóa hoặc đơn giản hóa GetGroupTypeFilter()
    - _Requirements: 2.1, 2.2, 2.3, 2.4_

  - [ ]\* 1.3 Write property test cho BaseCMSController
    - **Property 1: GIBA-Only Authorization**
    - **Property 6: GetUserGroupIdsOrNull Always Returns Null**
    - **Validates: Requirements 2.1, 2.2, 2.3**

- [x] 2. Cập nhật CMS Controllers

  - [x] 2.1 Cập nhật EventController.cs

    - Xóa logic xử lý Club/NBD/Group trong các methods
    - Đơn giản hóa role-based filtering
    - _Requirements: 2.1, 3.1_

- - [x] 2.2 Cập nhật GroupsController.cs

    - Xóa logic xử lý Club/NBD/Group

    - GIBA có full access đến tất cả groups

    - _Requirements: 2.1_

  - [x] 2.3 Cập nhật EventGuestsController.cs

    - Xóa logic xử lý Club/NBD/Group
    - _Requirements: 2.1_

  - [x] 2.4 Cập nhật GroupPermissionController.cs

    - Đánh dấu deprecated
    - _Requirements: 6.1, 6.2_

- [ ] 3. Checkpoint - Đảm bảo build thành công

  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Cập nhật Services

  - [x] 4.1 Cập nhật EventService.cs

    - Xóa case CTRole.NBD, CTRole.Club, CTRole.Group trong ApplyRoleBasedFiltersAsync
    - GIBA có full access - không filter theo role
    - _Requirements: 3.1_

  - [ ]\* 4.2 Write property test cho EventService

    - **Property 2: Full Access for GIBA in EventService**
    - **Validates: Requirements 3.1**

  - [x] 4.3 Cập nhật ArticleService.cs

    - Xóa logic filter theo CTRole.NBD, CTRole.Club, CTRole.Group
    - GIBA có full access đến tất cả articles
    - _Requirements: 3.2_

  - [ ]\* 4.4 Write property test cho ArticleService

    - **Property 3: Full Access for GIBA in ArticleService**
    - **Validates: Requirements 3.2**

  - [x] 4.5 Cập nhật ShowcaseService.cs

    - Xóa logic filter theo CTRole.Group
    - GIBA có full access
    - _Requirements: 3.3_

  - [ ]\* 4.6 Write property test cho ShowcaseService

    - **Property 4: Full Access for GIBA in ShowcaseService**
    - **Validates: Requirements 3.3**

  - [x] 4.7 Cập nhật MeetingService.cs

    - Xóa logic filter theo CTRole.Group
    - GIBA có full access
    - _Requirements: 3.4_

  - [ ]\* 4.8 Write property test cho MeetingService
    - **Property 5: Full Access for GIBA in MeetingService**
    - **Validates: Requirements 3.4**
  - [x] 4.9 Cập nhật AuthenticationService.cs

    - Xóa logic xử lý GroupPermission claims cho CTRole.Group
    - Chỉ handle GIBA role
    - _Requirements: 3.5_

- [-] 5. Checkpoint - Đảm bảo services hoạt động đúng

  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. Cập nhật Views

  - [x] 6.1 Cập nhật \_AdminSidebar.cshtml

    - Xóa biến isGroup, isNBD, isClub
    - Chỉ check isGiba
    - _Requirements: 5.1_

  - [x] 6.2 Cập nhật \_Article.cshtml
    - Xóa logic xử lý CTRole.NBD, CTRole.Club, CTRole.Group
    - Chỉ handle GIBA role
    - _Requirements: 5.2_
  - [x] 6.3 Cập nhật Event/Index.cshtml

    - Đơn giản hóa canEdit - chỉ check GIBA
    - _Requirements: 5.3_

  - [x] 6.4 Cập nhật Sponsor/Index.cshtml
    - Đơn giản hóa canEdit - chỉ check GIBA
    - _Requirements: 5.3_
  - [x] 6.5 Cập nhật SponsorshipTier/Index.cshtml

    - Đơn giản hóa canEdit - chỉ check GIBA
    - _Requirements: 5.3_

- [x] 7. Cập nhật Entity và Database

  - [x] 7.1 Cập nhật Group.cs entity

    - Xóa property Type
    - _Requirements: 4.1, 4.2_

  - [x] 7.2 Tạo migration để xóa column Type

    - Tạo migration RemoveGroupTypeColumn
    - Xóa column Type từ Groups table
    - _Requirements: 4.3_

- [x] 8. Cleanup - Xóa/Deprecate code không dùng



  - [ ] 8.1 Deprecate GroupPermissionService
    - Thêm [Obsolete] attribute hoặc xóa file
    - _Requirements: 6.1_
  - [ ] 8.2 Deprecate IGroupPermissionService
    - Thêm [Obsolete] attribute hoặc xóa file
    - _Requirements: 6.1_
  - [ ] 8.3 Xóa hoặc deprecate GroupPermission views (nếu có)
    - _Requirements: 6.3_

- [ ] 9. Clean up Membership Entity và các files liên quan

  - [x] 9.1 Cập nhật MembershipDTO.cs

    - Xóa các fields không còn trong entity: Email, Profile, DayOfBirth, Address, Company, Position, AppPosition, Term, FieldIds, FieldNames
    - Xóa Company information fields: CompanyFullName, CompanyBrandName, TaxCode, BusinessField, BusinessType, HeadquartersAddress, CompanyWebsite, CompanyPhoneNumber, CompanyEmail, LegalRepresentative, LegalRepresentativePosition, CompanyLogo, BusinessRegistrationNumber, BusinessRegistrationDate, BusinessRegistrationPlace
    - Xóa Rating fields: AverageRating, TotalRatings
    - Xóa Approval status fields: ApprovalStatus, ApprovalReason, ApprovedDate, ApprovedBy
    - Xóa các fields khác: Message, Reason, Object, Contribute, CareAbout, OtherContribute
    - _Requirements: Entity cleanup_

  - [x] 9.2 Cập nhật IMembershipService.cs

    - Xóa các method signatures không còn cần thiết
    - Cập nhật method signatures để match với entity mới
    - _Requirements: Entity cleanup_

  - [x] 9.3 Cập nhật MembershipService.cs

    - Xóa các projections đến fields không còn tồn tại
    - Cập nhật CreateMembershipAsync, UpdateMembershipAsync
    - Cập nhật GetMembershipsAsync, GetMembershipByIdAsync, GetMembershipByPhoneAsync, GetMembershipByUserZaloIdAsync
    - Cập nhật MapToMembershipDTO method
    - Xóa logic liên quan đến FieldIds, Fields
    - _Requirements: Entity cleanup_

  - [x] 9.4 Cập nhật MembershipQueryParameters.cs

    - Xóa các filter parameters không còn cần: FieldId, Email, Phone
    - _Requirements: Entity cleanup_

  - [x] 9.5 Cập nhật CreateMembershipRequest.cs và UpdateProfileRequest.cs

    - Xóa các fields không còn trong entity
    - _Requirements: Entity cleanup_

  - [x] 9.6 Cập nhật MembershipController.cs (CMS)

    - Xóa logic xử lý các fields không còn tồn tại
    - Cập nhật Edit action - xóa mapping các fields cũ
    - Cập nhật GetPage action - xóa response data fields cũ
    - Xóa các endpoints liên quan đến profile-template/custom-fields nếu không còn cần
    - _Requirements: Entity cleanup_

  - [x] 9.7 Cập nhật Views/Membership/Index.cshtml

    - Xóa các columns không còn cần: email, company, fields
    - Cập nhật DataTable columns
    - Xóa formattedData fields không còn tồn tại
    - _Requirements: Entity cleanup_

  - [x] 9.8 Cập nhật Views/Membership/Partials/\_MembershipForm.cshtml

    - Xóa các form fields không còn trong entity
    - Xóa sections: Company information, Rating, Custom Fields, Contribution info
    - _Requirements: Entity cleanup_

  - [x] 9.9 Cập nhật Views/Membership/Partials/\_MembershipCreateForm.cshtml

    - Xóa các form fields không còn trong entity
    - _Requirements: Entity cleanup_

- [x] 10. Sửa các file còn lỗi build

  - [x] 10.1 Sửa AdminManagementController.cs
    - Xóa tham chiếu đến Email, Profile, Address, Company, Position, DayOfBirth
    - Cập nhật CreateAdminMembershipAsync call
    - _Requirements: Entity cleanup_

  - [x] 10.2 Sửa Views/Membership/Partials/_ApprovalForm.cshtml
    - Xóa hoặc refactor toàn bộ form vì các fields đã bị xóa
    - _Requirements: Entity cleanup_

  - [x] 10.3 Sửa Views/MembershipApproval/Index.cshtml
    - Xóa các columns không còn tồn tại
    - _Requirements: Entity cleanup_

  - [x] 10.4 Sửa Views/MembershipApproval/Detail.cshtml
    - Xóa các fields không còn tồn tại
    - _Requirements: Entity cleanup_

  - [x] 10.5 Sửa Services/EventGuests/EventGuestService.cs
    - Xóa tham chiếu đến Membership.Email, Membership.Company
    - _Requirements: Entity cleanup_

  - [x] 10.6 Sửa Models/DTOs/Admins/CreateAdminDto.cs
    - Xóa field Email không còn cần thiết
    - _Requirements: Entity cleanup_

- [x] 12. Đơn giản hóa trang Article - Bỏ phạm vi/role

  - [x] 12.1 Cập nhật Views/Article/Index.cshtml
    - Bỏ filter theo loại (filter-type dropdown)
    - Bỏ cột "Phạm Vi" trong DataTable
    - Bỏ gửi type parameter trong AJAX call
    - Bỏ các function JS liên quan đến role: toggleRoleDropdown, onGIBARoleChange, toggleGIBARoleOptions, toggleRoleOptions, loadGroupsByRole, loadGroupsByRoleGIBA
    - Đơn giản hóa HandleSaveOrUpdate - bỏ logic groupCategory
    - _Requirements: 5.2_

  - [x] 12.2 Cập nhật Views/Article/_Article.cshtml
    - Bỏ phần chọn role (roleDropdownContainer)
    - Bỏ phần chọn phạm vi áp dụng (gibaRoleOptionsContainer)
    - Bỏ phần chọn hội nhóm (groupsDropdownContainerGIBA)
    - Đơn giản hóa form - không cần chọn role khi tạo/chỉnh sửa
    - _Requirements: 5.2_

  - [x] 12.3 Cập nhật Controller/API/ArticlesController.cs
    - Bỏ xử lý GroupCategory và GroupIds trong CreateArticle và UpdateArticle
    - Bỏ method ProcessGroupCategoryAndIds
    - _Requirements: 3.2_

- [x] 11. Final Checkpoint - Đảm bảo toàn bộ hệ thống hoạt động
  - Build thành công, không còn lỗi CS
