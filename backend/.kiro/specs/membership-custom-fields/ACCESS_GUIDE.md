# How to Access Custom Field Management - Visual Guide

## 🎯 Main Entry Point

### Via Sidebar Menu

```
┌─────────────────────────────────────┐
│  MiniApp GIBA Admin Dashboard       │
├─────────────────────────────────────┤
│ 📊 Tổng Quan                        │
│ 👥 Danh Sách Tài Khoản              │
│ 📋 Lịch Sử Hoạt Động                │
│ ┌─ 👥 Nhóm                          │
│ │  ├─ Quản Lý Hội Nhóm              │
│ │  ├─ Quản Lý Gói Cước              │
│ │  ├─ ⭐ Cấu Hình Form Tùy Chỉnh    │ ← CLICK HERE
│ │  └─ Chờ Phê Duyệt                 │
│ ├─ 🏢 Lĩnh Vực                      │
│ ├─ 📅 Sự Kiện                       │
│ └─ ⚙️ Cài Đặt Hệ Thống              │
└─────────────────────────────────────┘
```

## 📍 Navigation Flow

### Step 1: Click Menu Item

```
Sidebar → Nhóm → Cấu Hình Form Tùy Chỉnh
                    ↓
```

### Step 2: Select Group

```
┌─────────────────────────────────────┐
│  Cấu Hình Form Tùy Chỉnh            │
│  Chọn hội nhóm để cấu hình...       │
├─────────────────────────────────────┤
│                                     │
│  🔍 [Tìm kiếm hội nhóm...]         │
│                                     │
│  ┌─────────────────────────────┐   │
│  │ 👥 Group 1                  │   │
│  │ ID: group-001               │   │
│  │              [Cấu Hình] ←───┼─── CLICK
│  └─────────────────────────────┘   │
│                                     │
│  ┌─────────────────────────────┐   │
│  │ 👥 Group 2                  │   │
│  │ ID: group-002               │   │
│  │              [Cấu Hình]     │   │
│  └─────────────────────────────┘   │
│                                     │
└─────────────────────────────────────┘
                    ↓
```

### Step 3: Manage Tabs

```
┌─────────────────────────────────────┐
│  Quản lý Tab Trường Tùy Chỉnh       │
│  [Thêm Tab Mới]                     │
├─────────────────────────────────────┤
│                                     │
│  ┌─────────────────────────────┐   │
│  │ 📁 Tab 1: Thông tin cơ bản  │   │
│  │ 5 trường                    │   │
│  │ [✏️] [⚙️] [🗑️]             │   │
│  └─────────────────────────────┘   │
│                                     │
│  ┌─────────────────────────────┐   │
│  │ 📁 Tab 2: Kinh nghiệm       │   │
│  │ 3 trường                    │   │
│  │ [✏️] [⚙️] [🗑️]             │   │
│  └─────────────────────────────┘   │
│                                     │
└─────────────────────────────────────┘
                    ↓
```

### Step 4: Manage Fields

```
┌─────────────────────────────────────┐
│  Quản lý Trường Tùy Chỉnh           │
│  [Thêm Trường Mới]                  │
├─────────────────────────────────────┤
│                                     │
│  ┌─────────────────────────────┐   │
│  │ 📝 Họ và tên                │   │
│  │ Text | Bắt buộc             │   │
│  │ [✏️] [🗑️]                  │   │
│  └─────────────────────────────┘   │
│                                     │
│  ┌─────────────────────────────┐   │
│  │ ✉️ Email                    │   │
│  │ Email | Bắt buộc            │   │
│  │ [✏️] [🗑️]                  │   │
│  └─────────────────────────────┘   │
│                                     │
└─────────────────────────────────────┘
```

## 🔗 Direct URLs

### Group Selection

```
/CustomFieldTab/SelectGroup
```

### Tab Management

```
/CustomFieldTab/Index?groupId={groupId}
```

### Field Management

```
/CustomField/Index?tabId={tabId}
```

## 📱 Mobile Access

Same menu structure works on mobile:

1. Tap hamburger menu (☰)
2. Tap "Nhóm"
3. Tap "Cấu Hình Form Tùy Chỉnh"
4. Select group and proceed

## 🔐 Permission Requirements

| Action              | Required Role |
| ------------------- | ------------- |
| View menu           | GIBA          |
| Access SelectGroup  | GIBA          |
| Create tabs         | GIBA          |
| Edit tabs           | GIBA          |
| Delete tabs         | GIBA          |
| Create fields       | GIBA          |
| Edit fields         | GIBA          |
| Delete fields       | GIBA          |
| View submitted data | GIBA          |

## 🎯 Quick Access Shortcuts

### From Pending Approvals

```
Sidebar → Nhóm → Chờ Phê Duyệt
    ↓
Click "Xem chi tiết" on any application
    ↓
Scroll to "Thông tin form tùy chỉnh"
    ↓
View submitted custom field values
```

### From Groups Management

```
Sidebar → Nhóm → Quản Lý Hội Nhóm
    ↓
Find group
    ↓
Look for "Cấu Hình Form" button (if added)
    ↓
Configure custom fields
```

## 📊 Feature Access Map

```
┌─────────────────────────────────────────────────────┐
│           Custom Field Management System            │
├─────────────────────────────────────────────────────┤
│                                                     │
│  SelectGroup                                        │
│  ├─ List all groups                                │
│  ├─ Search groups                                  │
│  └─ Quick access to configuration                  │
│                                                     │
│  CustomFieldTab (Tab Management)                   │
│  ├─ Create tabs                                    │
│  ├─ Edit tabs                                      │
│  ├─ Delete tabs                                    │
│  ├─ Reorder tabs (drag-drop)                       │
│  └─ Access field management                        │
│                                                     │
│  CustomField (Field Management)                    │
│  ├─ Create fields                                  │
│  ├─ Edit fields                                    │
│  ├─ Delete fields                                  │
│  ├─ Reorder fields (drag-drop)                     │
│  └─ Configure options                              │
│                                                     │
│  Approval View (View Submitted Data)               │
│  ├─ Display custom field values                    │
│  ├─ Organize by tabs                               │
│  ├─ Show field names & values                      │
│  └─ Show archived field names                      │
│                                                     │
└─────────────────────────────────────────────────────┘
```

## 🚀 Getting Started

### First Time Setup

1. Login as GIBA
2. Click "Cấu Hình Form Tùy Chỉnh" in sidebar
3. Select a group
4. Click "Thêm Tab Mới"
5. Enter tab name and save
6. Click settings icon on tab
7. Click "Thêm Trường Mới"
8. Configure field and save
9. Done! Your form is ready

### Testing Your Form

1. Go to "Chờ Phê Duyệt"
2. Click "Xem chi tiết" on any application
3. Scroll to "Thông tin form tùy chỉnh"
4. See your custom fields displayed

## 💡 Pro Tips

### Keyboard Navigation

- Tab: Move between fields
- Enter: Submit forms
- Escape: Close modals
- Arrow keys: Navigate lists

### Mouse Shortcuts

- Drag handle (≡): Reorder items
- Click card: Select item
- Double-click: Edit item
- Right-click: Context menu (if available)

### Search Tips

- Search by group name
- Search by group ID
- Partial matches work
- Case-insensitive

## ❓ Frequently Accessed Pages

| Page              | URL                         | Purpose          |
| ----------------- | --------------------------- | ---------------- |
| Group Selection   | /CustomFieldTab/SelectGroup | Choose group     |
| Tab Management    | /CustomFieldTab/Index       | Manage tabs      |
| Field Management  | /CustomField/Index          | Manage fields    |
| Pending Approvals | /Membership/PendingApproval | View submissions |

## 🔄 Common Workflows

### Create a New Form

```
SelectGroup → Choose Group → Create Tab → Add Fields
```

### Edit Existing Form

```
SelectGroup → Choose Group → Edit Tab/Fields
```

### View Submissions

```
Pending Approvals → View Detail → Scroll to Custom Fields
```

### Reorder Elements

```
Drag handle (≡) → Drop in new position → Auto-save
```

## 📞 Need Help?

1. **Can't find menu?** → Make sure you're logged in as GIBA
2. **Groups not loading?** → Check internet connection
3. **Can't save changes?** → Check all required fields are filled
4. **Drag-drop not working?** → Try refreshing the page

---

**Ready to get started?** 🎉

Follow the navigation flow above to access the custom field management system!
