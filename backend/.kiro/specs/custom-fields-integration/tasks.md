# Custom Fields Integration - Tasks

## Phase 1: Preparation

- [ ] Task 1.1: Backup existing files (CustomFieldTab/Index.cshtml, CustomField/Index.cshtml, SelectGroup.cshtml)

- [ ] Task 1.2: Create \_CustomFieldsSection.cshtml partial

## Phase 2: UI Implementation

- [ ] Task 2.1: Create tab list UI with accordion/card style
- [ ] Task 2.2: Create "Thêm Tab Mới" button and modal
- [ ] Task 2.3: Create field list UI inside each tab
- [ ] Task 2.4: Create "Thêm Field" button and modal
- [ ] Task 2.5: Style all modals (centered, no gradient)
- [ ] Task 2.6: Add delete confirmation modals

## Phase 3: JavaScript Logic

- [ ] Task 3.1: Load tabs on form load
- [ ] Task 3.2: Implement tab CRUD (Create, Read, Update, Delete)
- [ ] Task 3.3: Implement field CRUD
- [ ] Task 3.4: Implement drag-drop for tab reordering
- [ ] Task 3.5: Implement drag-drop for field reordering
- [ ] Task 3.6: Add form validation

## Phase 4: Integration

- [ ] Task 4.1: Add \_CustomFieldsSection.cshtml to \_GroupForm.cshtml
- [ ] Task 4.2: Update Groups/Index.cshtml to load section
- [ ] Task 4.3: Test create group with custom fields
- [ ] Task 4.4: Test edit group with custom fields
- [ ] Task 4.5: Test delete tab/field

## Phase 5: Cleanup

- [ ] Task 5.1: Delete Views/CustomFieldTab/Index.cshtml
- [ ] Task 5.2: Delete Views/CustomField/Index.cshtml
- [ ] Task 5.3: Delete Views/CustomFieldTab/SelectGroup.cshtml
- [ ] Task 5.4: Remove routes from CustomFieldTabController if needed
- [ ] Task 5.5: Remove routes from CustomFieldController if needed

## Phase 6: Testing & Optimization

- [ ] Task 6.1: Test on mobile (responsive)
- [ ] Task 6.2: Test error handling
- [ ] Task 6.3: Test performance with many tabs/fields
- [ ] Task 6.4: Test AJAX calls (no CSRF token issues)
- [ ] Task 6.5: Final UI review (no gradient, clean design)

## Estimated Effort

- Phase 1: 15 min
- Phase 2: 1 hour
- Phase 3: 1.5 hours
- Phase 4: 30 min
- Phase 5: 15 min
- Phase 6: 30 min
- **Total: ~4 hours**
