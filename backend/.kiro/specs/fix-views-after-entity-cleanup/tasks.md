# Implementation Plan

- [x] 1. Sửa AdminManagement Views

  - [x] 1.1 Sửa Views/AdminManagement/Index.cshtml
    - Xóa column Email khỏi table header
    - Xóa column email khỏi DataTable columns
    - Thay column approvalStatus bằng createdDate
    - Xóa field Email khỏi modal form
    - Xóa email từ các JavaScript functions (openCreateModal, editAdmin, saveAdmin, fillUserInfo)
    - _Requirements: 1.1, 1.2, 1.3_

  - [x] 1.2 Cập nhật AdminManagementController.cs
    - Thêm role vào GetById API response
    - _Requirements: 5.1_

- [x] 2. Kiểm tra và sửa Membership Views






  - [x] 2.1 Kiểm tra Views/Membership/Index.cshtml


    - Verify DataTable columns match API response
    - _Requirements: 2.1_


  - [x] 2.2 Kiểm tra Views/Membership/Partials/_MembershipForm.cshtml


    - Verify form fields match entity
    - _Requirements: 2.2_



  - [ ] 2.3 Kiểm tra Views/MembershipApproval/Index.cshtml
    - Verify không còn reference đến removed fields

    - _Requirements: 2.3_

  - [ ] 2.4 Kiểm tra Views/MembershipApproval/Detail.cshtml
    - Verify không còn reference đến removed fields
    - _Requirements: 2.3_

- [ ] 3. Kiểm tra và sửa Group Views

  - [ ] 3.1 Kiểm tra Views/Groups/Index.cshtml
    - Verify không còn reference đến Group.Type
    - _Requirements: 3.1_

  - [ ] 3.2 Kiểm tra Views/Groups/Partials/_Groups.cshtml
    - Verify form không còn Type dropdown
    - _Requirements: 3.2_

- [ ] 4. Kiểm tra các Views khác

  - [ ] 4.1 Kiểm tra Event views
    - _Requirements: 4.1_

  - [ ] 4.2 Kiểm tra Sponsor views
    - _Requirements: 4.2_

  - [ ] 4.3 Kiểm tra Dashboard views
    - _Requirements: 4.3_

- [ ] 5. Final Checkpoint
  - Ensure all pages load without JavaScript errors
  - Ensure all DataTables work correctly
