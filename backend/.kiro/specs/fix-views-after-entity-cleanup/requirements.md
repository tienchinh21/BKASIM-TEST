# Requirements Document

## Introduction

Sau khi đơn giản hóa các entity Membership và Group (xóa nhiều fields như Email, Company, Position, ApprovalStatus, Type...), nhiều Views trong hệ thống vẫn còn tham chiếu đến các fields đã bị xóa, gây ra lỗi runtime như DataTables warning. Spec này nhằm kiểm tra và sửa tất cả các Views để đảm bảo chúng hoạt động đúng với entity mới.

## Glossary

- **DataTables**: jQuery plugin để hiển thị bảng dữ liệu với pagination, sorting, filtering
- **Entity Cleanup**: Quá trình xóa các fields không cần thiết khỏi entity Membership và Group
- **Runtime Error**: Lỗi xảy ra khi chạy ứng dụng, không phải lỗi compile

## Requirements

### Requirement 1: Sửa AdminManagement Views

**User Story:** As a developer, I want to fix AdminManagement views, so that the admin list page works without DataTables errors.

#### Acceptance Criteria

1. WHEN the AdminManagement/Index page loads THEN the DataTable SHALL not show warning about unknown parameter 'email'
2. WHEN the DataTable columns are defined THEN the columns SHALL only reference fields that exist in the API response (id, username, fullName, phoneNumber, role, createdDate)
3. WHEN the table header is rendered THEN the header SHALL match the actual columns being displayed

### Requirement 2: Kiểm tra và sửa Membership Views

**User Story:** As a developer, I want to verify all Membership views work correctly, so that membership management functions properly.

#### Acceptance Criteria

1. WHEN Membership/Index page loads THEN the DataTable SHALL only reference existing fields
2. WHEN Membership forms are rendered THEN the forms SHALL not contain fields that no longer exist in entity
3. WHEN MembershipApproval views are rendered THEN the views SHALL not reference removed fields like ApprovalStatus, Email, Company

### Requirement 3: Kiểm tra và sửa Group Views

**User Story:** As a developer, I want to verify all Group views work correctly, so that group management functions properly.

#### Acceptance Criteria

1. WHEN Groups/Index page loads THEN the view SHALL not reference Group.Type field
2. WHEN Group forms are rendered THEN the forms SHALL not contain Type dropdown

### Requirement 4: Kiểm tra các Views khác

**User Story:** As a developer, I want to verify all other views work correctly, so that the entire CMS functions properly.

#### Acceptance Criteria

1. WHEN Event views are rendered THEN the views SHALL not reference removed Membership fields
2. WHEN Sponsor views are rendered THEN the views SHALL work with simplified role system
3. WHEN Dashboard views are rendered THEN the views SHALL not reference removed fields

### Requirement 5: Đảm bảo consistency giữa API và Views

**User Story:** As a developer, I want API responses and Views to be consistent, so that there are no runtime errors.

#### Acceptance Criteria

1. WHEN a controller returns data THEN the data fields SHALL match what the View expects
2. WHEN a View references a field THEN that field SHALL exist in the corresponding DTO or API response
