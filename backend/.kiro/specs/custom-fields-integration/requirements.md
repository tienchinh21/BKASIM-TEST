# Custom Fields Integration - Requirements

## Overview

Tích hợp quản lý Custom Fields (Tab & Field) trực tiếp vào form tạo/chỉnh sửa hội nhóm, thay vì tách thành nhiều trang riêng.

## Acceptance Criteria

### AC1: Form tạo/chỉnh sửa hội nhóm có section quản lý tab

- Khi tạo hoặc chỉnh sửa hội nhóm, form hiển thị section "Cấu hình Form Tùy Chỉnh"
- Section này cho phép thêm, sửa, xóa tab
- Danh sách tab hiển thị dưới dạng accordion hoặc tab list
- Mỗi tab có nút: Thêm Field, Sửa, Xóa

### AC2: Quản lý Field trong Tab

- Khi click "Thêm Field" trong tab, mở modal để thêm field
- Modal cho phép chọn loại field (Text, Email, Dropdown, etc.)
- Có thể thêm nhiều field vào 1 tab
- Danh sách field hiển thị dưới tab, có thể kéo thả sắp xếp

### AC3: Lưu dữ liệu

- Khi lưu form hội nhóm, tất cả tab và field được lưu cùng lúc
- Không cần chuyển trang, không cần bước xác nhận riêng

### AC4: UI đơn giản, không gradient

- Sử dụng Shadcn style (neutral colors, no gradient)
- Modal centered, gọn gàng
- Responsive trên mobile

### AC5: Xóa trang riêng

- Xóa Views/CustomFieldTab/Index.cshtml
- Xóa Views/CustomField/Index.cshtml
- Xóa Views/CustomFieldTab/SelectGroup.cshtml
- Tất cả logic chuyển vào Groups/Index.cshtml form

## User Flow

1. Admin vào Groups/Index
2. Click "Tạo Hội Nhóm" hoặc "Chỉnh sửa"
3. Form hiển thị với section "Cấu hình Form Tùy Chỉnh"
4. Admin thêm tab, thêm field vào tab
5. Admin click "Lưu" - tất cả được lưu cùng lúc
6. Quay lại danh sách hội nhóm

## Technical Notes

- Sử dụng AJAX để load/save tab và field
- Không dùng ValidateAntiForgeryToken cho API endpoints
- Modal dùng Bootstrap modal-dialog-centered
- Kéo thả dùng jQuery UI sortable
