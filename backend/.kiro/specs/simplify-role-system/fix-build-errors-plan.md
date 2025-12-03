# Kế hoạch sửa lỗi Build sau khi đơn giản hóa Entity

## Tổng quan
Các entity `Membership` và `Group` đã được đơn giản hóa, loại bỏ nhiều field. Điều này gây ra nhiều lỗi build trong toàn bộ codebase.

## Các field đã bị xóa

### Membership (đã xóa):
- Email, Profile, DayOfBirth, Address, Company, Position, AppPosition, Term
- FieldIds, SortField, Code
- AverageRating, TotalRatings
- ApprovalStatus, ApprovalReason, ApprovedDate, ApprovedBy
- CompanyFullName, CompanyBrandName, TaxCode, BusinessField, BusinessType
- HeadquartersAddress, CompanyWebsite, CompanyPhoneNumber, CompanyEmail
- LegalRepresentative, LegalRepresentativePosition, CompanyLogo
- BusinessRegistrationNumber, BusinessRegistrationDate, BusinessRegistrationPlace
- Message, Reason, Object, Contribute, CareAbout, OtherContribute

### Group (đã xóa):
- Type

## Trạng thái: ✅ HOÀN THÀNH

Build thành công - không còn lỗi CS.

## Các file đã sửa:

### Controllers:
1. ✅ Controller/API/CommonController.cs - Deprecated Gen-Code-Membership endpoint
2. ✅ Controller/API/MembershipsController.cs - Sửa nhiều methods
3. ✅ Controller/CMS/AdminManagementController.cs - Xóa tham chiếu Email, Profile, Address, Company, Position, DayOfBirth
4. ✅ Controller/CMS/GroupsController.cs - Xóa tham chiếu Type, Email, Company, Position, ApprovalStatus
5. ✅ Controller/CMS/GroupPermissionController.cs - Xóa tham chiếu Email, Phone
6. ✅ Controller/CMS/PendingApprovalController.cs - Xóa tham chiếu Email, Phone, Company, etc.

### Services:
7. ✅ Services/Authencation/AuthencationService.cs - Xóa ApprovalStatus checks
8. ✅ Services/EventGuests/EventGuestService.cs - Xóa tham chiếu Email, Company, Position, Group.Type
9. ✅ Services/Refs/RefService.cs - Xóa tham chiếu Email, Company, Position, AverageRating, TotalRatings
10. ✅ Services/Dashboard/DashboardService.cs - Xóa tham chiếu Profile, Address, Company, Position
11. ✅ Services/Dashboard/GroupDashboard/GroupDashboardService.cs - Xóa tham chiếu Company
12. ✅ Services/ComingSoon/CommingSoonService.cs - Xóa tham chiếu Email
13. ✅ Services/Groups/GroupService.cs - Xóa tham chiếu Email

### DTOs:
14. ✅ Models/DTOs/Memberships/MembershipDTO.cs - Đã đơn giản hóa
15. ✅ Models/DTOs/Groups/GroupDTO.cs - Đã đơn giản hóa
16. ✅ Models/DTOs/Admins/CreateAdminDto.cs - Xóa field Email
17. ✅ Models/Payload/ETMPayload.cs - Thêm Company, Position vào GroupPayload

### Database:
18. ✅ Base/Database/ApplicationDbContext.cs - Xóa SortField configuration
19. ✅ Constants/CTRole.cs - Thêm NBD, Club, Group constants (để backward compatibility)

### Views:
20. ✅ Views/Groups/Partials/_Groups.cshtml - Xóa Type dropdown
21. ✅ Views/Membership/Partials/_ApprovalForm.cshtml - Refactor toàn bộ
22. ✅ Views/MembershipApproval/Index.cshtml - Xóa Email column, ApprovalStatus logic
23. ✅ Views/MembershipApproval/Detail.cshtml - Refactor toàn bộ

## Lệnh kiểm tra:
```bash
dotnet build
```

## Kết quả cuối cùng:
- Tổng số lỗi ban đầu: ~505
- Số lỗi còn lại: 0
- Build: ✅ SUCCESS
