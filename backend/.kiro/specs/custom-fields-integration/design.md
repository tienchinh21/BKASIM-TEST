# Custom Fields Integration - Design

## Architecture

### Components

1. **Groups/Index.cshtml** - Main page with group management table
2. **Groups/Partials/\_GroupForm.cshtml** - Form tạo/chỉnh sửa hội nhóm (existing)
3. **Groups/Partials/\_CustomFieldsSection.cshtml** - NEW: Section quản lý tab & field
4. **JavaScript** - Inline trong \_CustomFieldsSection.cshtml

### Data Flow

```
Groups/Index.cshtml
├── Modal: groupModal
│   └── _GroupForm.cshtml
│       └── _CustomFieldsSection.cshtml (NEW)
│           ├── Tab List (AJAX)
│           ├── Modal: tabModal (thêm/sửa tab)
│           ├── Modal: fieldModal (thêm/sửa field)
│           └── Modal: deleteConfirmModal
```

## UI Structure

### Section: Cấu hình Form Tùy Chỉnh

```
┌─────────────────────────────────────────┐
│ Cấu hình Form Tùy Chỉnh                 │
│ Quản lý các tab và trường tùy chỉnh     │
├─────────────────────────────────────────┤
│ [+ Thêm Tab Mới]                        │
├─────────────────────────────────────────┤
│ ┌─ Tab 1: Thông tin cơ bản ─────────┐  │
│ │ [✎ Sửa] [⚙ Quản lý Field] [✕ Xóa]│  │
│ │                                    │  │
│ │ Danh sách Field:                   │  │
│ │ • Field 1 (Text)                   │  │
│ │ • Field 2 (Email)                  │  │
│ │ [+ Thêm Field]                     │  │
│ └────────────────────────────────────┘  │
│                                         │
│ ┌─ Tab 2: Kinh nghiệm ──────────────┐  │
│ │ [✎ Sửa] [⚙ Quản lý Field] [✕ Xóa]│  │
│ │ ...                                │  │
│ └────────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

### Modal: Thêm/Sửa Tab

```
┌──────────────────────────────┐
│ Tạo Tab Mới                  │
├──────────────────────────────┤
│ Tên Tab: [____________]      │
│                              │
│ [Hủy] [Lưu Tab]              │
└──────────────────────────────┘
```

### Modal: Thêm/Sửa Field

```
┌──────────────────────────────┐
│ Tạo Trường Mới               │
├──────────────────────────────┤
│ Tên Trường: [____________]   │
│ Loại Trường: [Dropdown ▼]    │
│ Tùy Chọn: [textarea]         │
│ ☐ Bắt buộc                   │
│ Thứ tự: [0]                  │
│                              │
│ [Hủy] [Lưu Trường]           │
└──────────────────────────────┘
```

## API Endpoints (Existing)

- GET /CustomFieldTab/GetTabs?groupId=xxx
- POST /CustomFieldTab/CreateTab
- PUT /CustomFieldTab/UpdateTab
- DELETE /CustomFieldTab/DeleteTab?tabId=xxx
- POST /CustomFieldTab/ReorderTabs
- GET /CustomField/GetFieldsByTab?tabId=xxx
- POST /CustomField/CreateField
- PUT /CustomField/UpdateField
- DELETE /CustomField/DeleteField?fieldId=xxx
- POST /CustomField/ReorderFields

## Styling

- No gradient backgrounds
- Neutral colors: #f9fafb, #e5e7eb, #6b7280
- Border: 1px solid #e5e7eb
- Hover: border-color #0d6efd, box-shadow 0 1px 3px
- Modal: modal-dialog-centered
- Buttons: btn-primary, btn-secondary, btn-outline-\*

## State Management

- currentGroupId: ID của hội nhóm đang edit
- allTabs: Danh sách tab từ API
- currentEditingTabId: Tab đang edit
- currentEditingFieldId: Field đang edit
- isSortMode: Chế độ sắp xếp

## Error Handling

- Hiển thị toast/alert khi lỗi
- Reload dữ liệu nếu lỗi
- Validate input trước khi submit
