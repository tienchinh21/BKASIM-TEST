# Sidebar Menu Integration Guide

## Overview

Menu item "Cấu Hình Form Tùy Chỉnh" (Configure Custom Forms) has been added to the admin sidebar for easy access to custom field management.

## Menu Location

**Path**: Sidebar → Nhóm (Groups) → Cấu Hình Form Tùy Chỉnh

**Visibility**: Only visible to GIBA (Super Admin) users

## Menu Structure

```
Nhóm (Groups)
├── Quản Lý Hội Nhóm (Manage Groups)
├── Quản Lý Gói Cước (Manage Subscription Plans) [GIBA only]
├── Cấu Hình Form Tùy Chỉnh (Configure Custom Forms) [GIBA only] ← NEW
└── Chờ Phê Duyệt (Pending Approvals)
```

## How to Access

### Method 1: Via Sidebar Menu

1. Login as GIBA (Super Admin)
2. Look for "Nhóm" section in the sidebar
3. Click "Cấu Hình Form Tùy Chỉnh"
4. Select a group from the list
5. Configure tabs and fields

### Method 2: Direct URL

- Group Selection: `/CustomFieldTab/SelectGroup`
- Tab Management: `/CustomFieldTab/Index?groupId={groupId}`
- Field Management: `/CustomField/Index?tabId={tabId}`

## User Flow

```
Sidebar Menu
    ↓
SelectGroup View (Choose Group)
    ↓
CustomFieldTab Index (Manage Tabs)
    ↓
CustomField Index (Manage Fields)
```

## Files Modified

1. **Views/Shared/Components/\_AdminSidebar.cshtml**

   - Added menu item for custom field configuration
   - Only visible to GIBA users
   - Placed under "Nhóm" section

2. **Controller/CMS/CustomFieldTabController.cs**

   - Added `SelectGroup()` action
   - Updated `Index()` action to redirect to SelectGroup if no groupId provided

3. **Views/CustomFieldTab/SelectGroup.cshtml** (NEW)
   - Group selection interface
   - Search and filter functionality
   - Quick access to group configuration

## Features

### SelectGroup View

- ✅ List all active groups
- ✅ Search by group name or ID
- ✅ Filter results in real-time
- ✅ Quick access buttons to configure each group
- ✅ Responsive design
- ✅ Empty state messaging

### Menu Integration

- ✅ Automatic active state highlighting
- ✅ Smooth collapse/expand animation
- ✅ Permission-based visibility
- ✅ Consistent styling with existing menu

## Security

- Only GIBA (Super Admin) can access
- Authorization checks on all actions
- CSRF token validation
- Input validation and sanitization

## Styling

The menu item uses:

- Remixicon icons (ri-subtract-line)
- Bootstrap styling
- Consistent with existing sidebar items
- Responsive on mobile devices

## Browser Compatibility

- Chrome/Edge: ✅ Full support
- Firefox: ✅ Full support
- Safari: ✅ Full support
- IE11: ⚠️ Limited support (no CSS Grid)

## Performance

- Lazy loading of groups
- AJAX-based search
- Minimal DOM manipulation
- Optimized CSS transitions

## Troubleshooting

### Menu item not visible

- Check if logged in as GIBA user
- Clear browser cache
- Refresh page

### SelectGroup page not loading

- Check network connection
- Verify API endpoint `/api/groups/active` is working
- Check browser console for errors

### Groups not appearing

- Verify groups exist in database
- Check if groups are marked as active
- Check user permissions

## Future Enhancements

1. **Quick Access**: Add recently used groups to sidebar
2. **Favorites**: Mark frequently used groups as favorites
3. **Bulk Operations**: Configure multiple groups at once
4. **Templates**: Save and reuse field configurations
5. **Shortcuts**: Keyboard shortcuts for quick access

## Related Documentation

- [UI Usage Guide](UI_USAGE_GUIDE.md)
- [Implementation Summary](IMPLEMENTATION_SUMMARY.md)
- [Feature Complete](FEATURE_COMPLETE.md)
