# Design Document: Fix Views After Entity Cleanup

## Overview

Sau khi đơn giản hóa các entity Membership và Group, nhiều Views vẫn còn tham chiếu đến các fields đã bị xóa. Document này mô tả các thay đổi cần thiết để sửa các Views.

## Architecture

Các Views trong ASP.NET Core MVC nhận data từ Controllers thông qua ViewBag, ViewData hoặc Model. Khi entity thay đổi, cần đảm bảo:
1. Controller trả về đúng data
2. View chỉ hiển thị các fields có trong data
3. JavaScript/DataTables chỉ reference các fields có trong API response

## Components and Interfaces

### 1. AdminManagement Views

**Vấn đề đã sửa:**
- DataTable columns reference `email` và `approvalStatus` không tồn tại trong API response
- Table header có column Email và Trạng Thái không khớp với data

**Giải pháp:**
- Xóa column `email` khỏi DataTable và table header
- Thay column `approvalStatus` bằng `createdDate`
- Cập nhật API GetById để trả về `role`
- Xóa field Email khỏi modal form

### 2. Các Views khác cần kiểm tra

| View | Potential Issues |
|------|-----------------|
| Membership/Index.cshtml | DataTable columns |
| MembershipApproval/*.cshtml | ApprovalStatus, Email fields |
| Groups/Index.cshtml | Type field |

## Data Models

### API Response Changes

**AdminManagement/GetPage:**
```json
{
  "id": "string",
  "username": "string",
  "fullName": "string",
  "phoneNumber": "string",
  "role": "string",
  "createdDate": "string"
}
```

**AdminManagement/GetById:**
```json
{
  "id": "string",
  "username": "string",
  "fullName": "string",
  "phoneNumber": "string",
  "role": "string",
  "createdDate": "string"
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system-essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: DataTable Column Consistency
*For any* DataTable in the application, all column definitions SHALL reference fields that exist in the corresponding API response.
**Validates: Requirements 1.1, 1.2, 2.1**

### Property 2: Form Field Consistency
*For any* form in the application, all input fields SHALL correspond to fields that exist in the entity or DTO being edited.
**Validates: Requirements 1.3, 2.2**

## Error Handling

- Nếu API không trả về field mà View cần: DataTables sẽ hiển thị warning
- Giải pháp: Đảm bảo View chỉ reference fields có trong API response

## Testing Strategy

### Unit Tests
- Test API responses contain expected fields
- Test Views render without errors

### Manual Testing
- Load AdminManagement/Index page - không có DataTables warning
- Create/Edit admin - form hoạt động bình thường
- Load các pages khác - không có JavaScript errors
