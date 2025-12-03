# Custom Field Management UI - Usage Guide

## Quick Start

### Accessing the Custom Field Management Interface

1. **Navigate to Groups Management**

   - Go to Admin Dashboard
   - Select "Quản lý Hội Nhóm" (Groups Management)

2. **Access Custom Fields for a Group**
   - Find the group you want to configure
   - Click the "Cấu hình Trường Tùy Chỉnh" button (or similar)
   - This opens the Tab Management interface

## Tab Management (Topics)

### View All Tabs

- The main page shows all tabs for the selected group
- Each tab displays:
  - Tab name
  - Number of fields in the tab
  - Display order
  - Action buttons

### Create a New Tab

1. Click **"Thêm Tab Mới"** button (top right)
2. Enter the tab name (e.g., "Thông tin cơ bản", "Kinh nghiệm")
3. Click **"Lưu Tab"**
4. The new tab appears in the list

**Example Tab Names**:

- Thông tin cơ bản (Basic Information)
- Kinh nghiệm làm việc (Work Experience)
- Kỹ năng (Skills)
- Giáo dục (Education)
- Liên hệ (Contact Information)

### Edit a Tab

1. Click the **Edit button** (pencil icon) on the tab card
2. Modify the tab name
3. Click **"Lưu Tab"**

### Delete a Tab

1. Click the **Delete button** (trash icon) on the tab card
2. Confirm the deletion in the modal
3. **Note**: All fields in the tab will be deleted, but submitted values are preserved

### Reorder Tabs

1. Click and hold the **drag handle** (≡ icon) on a tab card
2. Drag the tab to the desired position
3. Release to drop
4. Order is automatically saved

## Field Management

### Access Field Management

1. From the Tab Management page
2. Click the **Settings button** (gear icon) on a tab card
3. This opens the Field Management page for that tab

### View All Fields

- The page shows all fields in the current tab
- Each field displays:
  - Field name
  - Field type (with icon)
  - Required status (if applicable)
  - Options preview (for Dropdown/MultipleChoice)
  - Display order

### Create a New Field

1. Click **"Thêm Trường Mới"** button (top right)
2. Fill in the field details:

   **Field Name** (required)

   - Enter a descriptive name (e.g., "Họ và tên", "Email")
   - Max 100 characters

   **Field Type** (required)

   - Select from 14 types:
     - Text (Văn bản)
     - Email
     - PhoneNumber (Số điện thoại)
     - LongText (Văn bản dài)
     - DateTime (Ngày giờ)
     - Date (Ngày)
     - Integer (Số nguyên)
     - Decimal (Số thập phân)
     - Boolean (Có/Không)
     - URL
     - Dropdown (Chọn một)
     - MultipleChoice (Chọn nhiều)
     - File
     - Image (Hình ảnh)

   **Options** (for Dropdown/MultipleChoice only)

   - Enter each option on a new line
   - Example:
     ```
     Tùy chọn 1
     Tùy chọn 2
     Tùy chọn 3
     ```

   **Required** (checkbox)

   - Check if this field must be filled by users
   - Unchecked = optional field

   **Display Order**

   - Set the order (0, 1, 2, etc.)
   - Can be adjusted by dragging later

3. Click **"Lưu Trường"**

### Edit a Field

1. Click the **Edit button** (pencil icon) on the field card
2. Modify the field details
3. Click **"Lưu Trường"**

### Delete a Field

1. Click the **Delete button** (trash icon) on the field card
2. Confirm the deletion in the modal
3. **Note**: The field definition is deleted, but submitted values are preserved with the archived field name

### Reorder Fields

1. Click and hold the **drag handle** (≡ icon) on a field card
2. Drag the field to the desired position
3. Release to drop
4. Order is automatically saved

## Field Type Reference

| Type           | Icon | Use Case            | Example             |
| -------------- | ---- | ------------------- | ------------------- |
| Text           | 📝   | Short text input    | Name, Company       |
| Email          | ✉️   | Email address       | user@example.com    |
| PhoneNumber    | ☎️   | Phone number        | +84 123 456 789     |
| LongText       | 📄   | Multi-line text     | Description, Bio    |
| DateTime       | 📅🕐 | Date and time       | Meeting time        |
| Date           | 📅   | Date only           | Birth date          |
| Integer        | 🔢   | Whole numbers       | Age, Years          |
| Decimal        | 🧮   | Decimal numbers     | Price, Rating       |
| Boolean        | ☑️   | Yes/No              | Agree to terms      |
| URL            | 🔗   | Website URL         | Portfolio link      |
| Dropdown       | ▼    | Single selection    | Country, Status     |
| MultipleChoice | ☑️☑️ | Multiple selections | Skills, Interests   |
| File           | 📎   | File upload         | Resume, Certificate |
| Image          | 🖼️   | Image upload        | Profile photo       |

## Viewing Submitted Data

### Access Membership Applications

1. Go to **"Chờ Phê Duyệt"** (Pending Approvals)
2. Find the membership application
3. Click **"Xem chi tiết"** (View Details)

### View Custom Field Values

In the detail modal:

1. Scroll to **"Thông tin form tùy chỉnh"** section
2. Click on each tab to expand it
3. View the submitted values:
   - Field name
   - Submitted value
   - Submission date/time

### Archived Fields

- If a field was deleted after submission, the value still appears
- Shows the archived field name
- Helps track what information was collected

## Best Practices

### Tab Organization

✅ **Do**:

- Group related fields together
- Use clear, descriptive tab names
- Keep tabs focused on one topic
- Order tabs logically (basic info first, then details)

❌ **Don't**:

- Create too many tabs (3-5 is ideal)
- Use vague tab names
- Mix unrelated fields in one tab

### Field Configuration

✅ **Do**:

- Use descriptive field names
- Mark truly required fields as required
- Use appropriate field types
- Provide clear options for Dropdown/MultipleChoice
- Order fields logically

❌ **Don't**:

- Mark all fields as required
- Use generic names like "Field 1", "Field 2"
- Use Text for data that should be Email or PhoneNumber
- Create too many fields (10-15 per tab is reasonable)

### Field Types

✅ **Do**:

- Use Email type for email addresses (enables validation)
- Use PhoneNumber for phone numbers
- Use Dropdown for predefined options
- Use MultipleChoice when users can select multiple items
- Use LongText for longer responses

❌ **Don't**:

- Use Text for everything
- Use Dropdown when there are too many options (>20)
- Mix different data types in one field

## Troubleshooting

### Tab not appearing

- Refresh the page
- Check if you have permission (GIBA/Super Admin only)
- Verify the group ID is correct

### Field not saving

- Check that all required fields are filled
- Verify field name is not empty
- For Dropdown/MultipleChoice, ensure options are provided
- Check browser console for errors

### Drag-and-drop not working

- Ensure JavaScript is enabled
- Try refreshing the page
- Check browser compatibility (works in modern browsers)

### Changes not persisting

- Check network connection
- Verify CSRF token is present
- Check browser console for errors
- Try again after page refresh

## Keyboard Shortcuts

| Shortcut | Action                       |
| -------- | ---------------------------- |
| Tab      | Navigate between form fields |
| Enter    | Submit form (in modals)      |
| Escape   | Close modal                  |
| Ctrl+S   | Save (if supported)          |

## Tips & Tricks

1. **Bulk Operations**: Create all tabs first, then add fields to each tab
2. **Copy Structure**: Note field configurations to reuse for other groups
3. **Testing**: Create a test group to experiment with field types
4. **Backup**: Document your field structure before major changes
5. **Naming**: Use consistent naming conventions across groups

## Support

For issues or questions:

1. Check this guide first
2. Review the error message
3. Check browser console (F12)
4. Contact system administrator
