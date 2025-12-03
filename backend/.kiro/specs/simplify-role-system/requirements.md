# Requirements Document

## Introduction

Dự án này nhằm đơn giản hóa hệ thống phân quyền của MiniApp GIBA bằng cách loại bỏ tất cả các role Club, NBD và Group, chỉ giữ lại role GIBA (super admin) làm role quản trị duy nhất. Việc này sẽ tạo ra một phiên bản đơn giản nhất của hệ thống, không cần cấu trúc phân cấp phức tạp - GIBA admin có toàn quyền quản lý tất cả.

## Glossary

- **GIBA**: Super Admin role - role quản trị duy nhất với toàn quyền truy cập hệ thống
- **NBD**: Network Business Development - role quản trị cấp trung (sẽ bị loại bỏ)
- **Club**: Club-level admin - role quản trị cấp club (sẽ bị loại bỏ)
- **Group**: Group-level admin - role quản trị cấp nhóm (sẽ bị loại bỏ)
- **GroupPermission**: Bảng phân quyền xác định admin nào có quyền quản lý nhóm nào (sẽ bị loại bỏ)
- **Group.Type**: Trường phân loại nhóm theo Club/NBD (sẽ bị loại bỏ)

## Requirements

### Requirement 1: Loại bỏ Role Constants

**User Story:** As a developer, I want to remove Club, NBD and Group role constants, so that the codebase only references GIBA role.

#### Acceptance Criteria

1. WHEN the system initializes THEN the CTRole class SHALL only contain GIBA constant
2. WHEN any code references CTRole.Club, CTRole.NBD or CTRole.Group THEN the system SHALL have those references removed or replaced with CTRole.GIBA

### Requirement 2: Cập nhật Logic Phân Quyền trong Controllers

**User Story:** As a developer, I want to update authorization logic in controllers, so that role checks only consider GIBA role.

#### Acceptance Criteria

1. WHEN BaseCMSController checks admin status THEN the IsAdmin method SHALL only check for GIBA role
2. WHEN GetCurrentUserRole is called THEN the method SHALL only return GIBA or null
3. WHEN GetUserGroupIdsOrNull is called THEN the method SHALL return null (GIBA has access to all groups)
4. WHEN GetGroupTypeFilter is called THEN the method SHALL be removed or always return null

### Requirement 3: Cập nhật Logic Phân Quyền trong Services

**User Story:** As a developer, I want to update service layer authorization, so that role-based filtering only considers GIBA role (full access).

#### Acceptance Criteria

1. WHEN EventService applies role-based filters THEN the service SHALL only handle GIBA case (full access to all events)
2. WHEN ArticleService filters by role THEN the service SHALL only handle GIBA case (full access to all articles)
3. WHEN ShowcaseService filters by role THEN the service SHALL only handle GIBA case (full access)
4. WHEN MeetingService filters by role THEN the service SHALL only handle GIBA case (full access)
5. WHEN AuthenticationService generates claims THEN the service SHALL only handle GIBA role

### Requirement 4: Loại bỏ Group.Type Field và GroupPermission

**User Story:** As a developer, I want to remove the Type field from Group entity and remove GroupPermission table, so that the permission system is simplified.

#### Acceptance Criteria

1. WHEN a Group entity is created or updated THEN the system SHALL not require or use the Type field
2. WHEN querying groups THEN the system SHALL not filter by Type field
3. WHEN the database schema is updated THEN the Type column SHALL be removed from Groups table via migration
4. WHEN checking permissions THEN the system SHALL not use GroupPermission table (GIBA has full access)

### Requirement 5: Cập nhật Views và UI

**User Story:** As a developer, I want to update all views, so that UI elements only reference GIBA role.

#### Acceptance Criteria

1. WHEN rendering admin sidebar THEN the view SHALL only check for GIBA role
2. WHEN rendering article views THEN the view SHALL only handle GIBA role logic
3. WHEN rendering event/sponsor views THEN the canEdit logic SHALL only check GIBA role

### Requirement 6: Loại bỏ GroupPermission Service và Controller

**User Story:** As a developer, I want to remove GroupPermission related code, so that the codebase is cleaner without unused permission logic.

#### Acceptance Criteria

1. WHEN the system is refactored THEN the GroupPermissionService SHALL be removed or deprecated
2. WHEN the system is refactored THEN the GroupPermissionController SHALL be removed or deprecated
3. WHEN admin views are rendered THEN the GroupPermission management UI SHALL be removed

### Requirement 7: Đảm bảo Backward Compatibility cho Data

**User Story:** As a developer, I want to handle existing data gracefully, so that the system continues to function after migration.

#### Acceptance Criteria

1. WHEN existing groups have Type = "Club" or "NBD" THEN the system SHALL treat them as regular groups
2. WHEN migrating data THEN the system SHALL set Type to null for all existing groups
3. WHEN existing GroupPermission records exist THEN the system SHALL ignore them (GIBA has full access)
