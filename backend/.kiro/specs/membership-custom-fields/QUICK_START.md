# Quick Start Guide - Custom Field Management

## 🚀 Get Started in 5 Minutes

### Prerequisites

- Logged in as GIBA (Super Admin)
- Application is running

### Step 1: Access the Menu (30 seconds)

1. Look at the left sidebar
2. Find "Nhóm" (Groups) section
3. Click "Cấu Hình Form Tùy Chỉnh"

**Result**: You'll see a list of all groups

### Step 2: Select a Group (30 seconds)

1. Find the group you want to configure
2. Click the "Cấu Hình" button
3. Or click anywhere on the group card

**Result**: You'll see the Tab Management page

### Step 3: Create Your First Tab (1 minute)

1. Click "Thêm Tab Mới" button
2. Enter tab name (e.g., "Thông tin cơ bản")
3. Click "Lưu Tab"

**Result**: Your first tab is created!

### Step 4: Add Fields to the Tab (2 minutes)

1. Click the settings icon (⚙️) on your tab
2. Click "Thêm Trường Mới"
3. Fill in the form:
   - **Tên Trường**: "Họ và tên"
   - **Loại Trường**: "Text"
   - **Bắt buộc**: Check the box
4. Click "Lưu Trường"

**Result**: Your first field is created!

### Step 5: View in Action (1 minute)

1. Go to "Chờ Phê Duyệt" (Pending Approvals)
2. Click "Xem chi tiết" on any membership application
3. Scroll down to see "Thông tin form tùy chỉnh"
4. Your custom fields will appear here!

## 📋 Common Tasks

### Create a Tab

```
Sidebar → Nhóm → Cấu Hình Form Tùy Chỉnh
→ Select Group → Thêm Tab Mới → Enter Name → Lưu Tab
```

### Add a Field

```
Tab Management → Click Settings on Tab → Thêm Trường Mới
→ Fill Form → Lưu Trường
```

### Reorder Tabs/Fields

```
Drag the handle (≡) icon and drop in new position
→ Automatically saved
```

### Delete a Tab/Field

```
Click Delete button (🗑️) → Confirm in modal
→ Deleted (values preserved)
```

### Edit a Tab/Field

```
Click Edit button (✏️) → Modify → Lưu
```

## 🎯 Example: Create a Job Application Form

### Tab 1: Thông tin cơ bản (Basic Info)

- Họ và tên (Text, Required)
- Email (Email, Required)
- Số điện thoại (PhoneNumber, Required)

### Tab 2: Kinh nghiệm (Experience)

- Năm kinh nghiệm (Integer, Required)
- Lĩnh vực chuyên môn (Dropdown, Required)
  - Options: IT, Sales, Marketing, HR, Other
- Mô tả kinh nghiệm (LongText, Optional)

### Tab 3: Tài liệu (Documents)

- CV (File, Required)
- Chứng chỉ (File, Optional)
- Portfolio (URL, Optional)

## 🔧 Field Types Quick Reference

| Type           | Use For          | Example             |
| -------------- | ---------------- | ------------------- |
| Text           | Short text       | Name, Company       |
| Email          | Email address    | user@example.com    |
| PhoneNumber    | Phone            | +84 123 456 789     |
| LongText       | Long text        | Description         |
| DateTime       | Date & time      | Meeting time        |
| Date           | Date only        | Birth date          |
| Integer        | Whole numbers    | Age, Years          |
| Decimal        | Decimals         | Price, Rating       |
| Boolean        | Yes/No           | Agree to terms      |
| URL            | Website          | Portfolio link      |
| Dropdown       | Single choice    | Country, Status     |
| MultipleChoice | Multiple choices | Skills, Interests   |
| File           | File upload      | Resume, Certificate |
| Image          | Image upload     | Profile photo       |

## ⚡ Pro Tips

1. **Organize Logically**: Group related fields in tabs
2. **Mark Required**: Only mark truly required fields
3. **Use Right Type**: Email for emails, PhoneNumber for phones
4. **Limit Options**: Keep Dropdown options under 20
5. **Order Matters**: Put important fields first
6. **Test First**: Create test group before production
7. **Document**: Note your field structure for reference

## ❌ Common Mistakes to Avoid

❌ Creating too many tabs (3-5 is ideal)
❌ Using Text for everything
❌ Marking all fields as required
❌ Creating too many fields (10-15 per tab)
❌ Using vague field names
❌ Forgetting to save changes

## 🆘 Troubleshooting

### Menu item not visible

→ Make sure you're logged in as GIBA

### Groups not loading

→ Check internet connection
→ Refresh the page

### Can't save field

→ Check all required fields are filled
→ For Dropdown, make sure options are entered

### Changes not saving

→ Check network connection
→ Try again after page refresh

## 📞 Need Help?

1. Check **UI_USAGE_GUIDE.md** for detailed instructions
2. Check **SIDEBAR_MENU_GUIDE.md** for menu help
3. Check **IMPLEMENTATION_SUMMARY.md** for technical details

## 🎓 Next Steps

After creating your first form:

1. Test it by submitting a membership application
2. View submitted data in Pending Approvals
3. Create more tabs and fields as needed
4. Customize for your specific needs

---

**You're all set!** 🎉

Start creating your custom forms now!
