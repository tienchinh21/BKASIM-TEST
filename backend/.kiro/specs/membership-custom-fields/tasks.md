# Implementation Plan: Generic Custom Field System

## Overview

This implementation plan converts the feature design into actionable coding tasks. Each task builds incrementally on previous tasks, with no orphaned code. The plan follows a phased approach: create database entities and migrations, implement services, create controllers, integrate with membership registration, and finally add comprehensive tests.

---

## Phase 1: Database Entities and Migrations

- [x] 1. Create custom field entity classes and enums

  - Create `Entities/CustomFields/CustomFieldTab.cs` with properties: EntityType, EntityId, TabName, DisplayOrder
  - Create `Entities/CustomFields/CustomField.cs` with properties: CustomFieldTabId, EntityType, EntityId, FieldName, FieldType (using existing EEventFieldType), FieldOptions, IsRequired, DisplayOrder
  - Create `Entities/CustomFields/CustomFieldValue.cs` with properties: CustomFieldId, EntityType, EntityId, FieldName, FieldValue
  - Create `Enum/ECustomFieldEntityType.cs` with entity types (GroupMembership, EventRegistration)
  - Reuse existing `Enum/EEventFieldType.cs` for field types (no new enum needed)
  - _Requirements: 6.1, 6.2_

- [x] 2. Create EF Core migrations for custom field tables

  - Add migration `AddCustomFieldTables` to create CustomFieldTab, CustomField, CustomFieldValue tables
  - Add indexes on (EntityType, EntityId) for performance
  - Add foreign key constraints between tables
  - _Requirements: 6.1_

---

## Phase 2: Service Layer Implementation

- [x] 3. Implement CustomFieldTab service

  - Create `Services/CustomFields/ICustomFieldTabService.cs` interface with methods: GetTabsByEntityAsync, CreateTabAsync, UpdateTabAsync, DeleteTabAsync, ReorderTabsAsync
  - Create `Services/CustomFields/CustomFieldTabService.cs` implementation
  - Implement GetTabsByEntityAsync to retrieve tabs ordered by DisplayOrder
  - Implement CreateTabAsync with validation for required fields
  - Implement UpdateTabAsync with round-trip persistence
  - Implement DeleteTabAsync with cascade deletion of associated fields
  - Implement ReorderTabsAsync to update display orders
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 4. Implement CustomField service

  - Create `Services/CustomFields/ICustomFieldService.cs` interface with methods: GetFieldsByTabAsync, GetFieldsByEntityAsync, CreateFieldAsync, UpdateFieldAsync, DeleteFieldAsync, ReorderFieldsAsync
  - Create `Services/CustomFields/CustomFieldService.cs` implementation
  - Implement GetFieldsByTabAsync to retrieve fields ordered by DisplayOrder
  - Implement GetFieldsByEntityAsync to retrieve fields for an entity (with optional tab filtering)
  - Implement CreateFieldAsync with validation for required fields and field type support
  - Implement UpdateFieldAsync with round-trip persistence for all properties
  - Implement DeleteFieldAsync with value archival (preserve submitted values with field name)
  - Implement ReorderFieldsAsync to update display orders within a tab
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

- [x] 5. Implement CustomFieldValue service

  - Create `Services/CustomFields/ICustomFieldValueService.cs` interface with methods: GetValuesByEntityAsync, CreateValuesAsync, DeleteValueAsync, GetValuesByFieldAsync
  - Create `Services/CustomFields/CustomFieldValueService.cs` implementation
  - Implement GetValuesByEntityAsync to retrieve values for an entity, organized by tabs
  - Implement CreateValuesAsync to store submitted field values
  - Implement DeleteValueAsync to remove a single value
  - Implement GetValuesByFieldAsync to retrieve values for a specific field
  - _Requirements: 3.5, 4.1, 4.2_

- [x] 6. Implement form validation and submission handler

  - Create `Services/CustomFields/ICustomFieldFormHandler.cs` interface with methods: ValidateFormAsync, SubmitFormAsync
  - Create `Services/CustomFields/CustomFieldFormHandler.cs` implementation
  - Implement ValidateFormAsync to check all required fields are present and non-empty
  - Implement ValidateFormAsync to return detailed error messages for missing fields
  - Implement SubmitFormAsync to validate and store all field values
  - Implement SubmitFormAsync to handle Dropdown/MultipleChoice option validation
  - _Requirements: 3.3, 3.4, 3.5_

- [ ] 7. Register custom field services in DI container
  - Update `Program.cs` or dependency injection configuration to register all custom field services
  - Register ICustomFieldTabService, ICustomFieldService, ICustomFieldValueService, ICustomFieldFormHandler
  - _Requirements: 6.1_

---

## Phase 3: Controller Layer Implementation

- [x] 8. Create CustomFieldTab CMS controller

  - Create `Controller/CMS/CustomFieldTabController.cs` inheriting from BaseCMSController
  - Implement GET endpoint to list tabs for a group
  - Implement POST endpoint to create a new tab
  - Implement PUT endpoint to update a tab
  - Implement DELETE endpoint to delete a tab
  - Implement POST endpoint to reorder tabs
  - Add authorization checks to ensure admin access
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 9. Create CustomField CMS controller

  - Create `Controller/CMS/CustomFieldController.cs` inheriting from BaseCMSController
  - Implement GET endpoint to list fields for a tab or entity
  - Implement POST endpoint to create a new field
  - Implement PUT endpoint to update a field
  - Implement DELETE endpoint to delete a field
  - Implement POST endpoint to reorder fields
  - Add authorization checks to ensure admin access
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

- [x] 10. Create CustomField API controller

  - Create `Controller/API/CustomFieldController.cs` inheriting from BaseAPIController
  - Implement GET endpoint to retrieve form structure (tabs and fields) for an entity
  - Implement POST endpoint to submit form values
  - Implement GET endpoint to retrieve submitted values for an entity
  - Add proper error handling and validation
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 4.1, 4.2_

---

## Phase 4: Membership Registration Integration

- [x] 11. Update MembershipGroup entity to support custom field values

  - Add navigation property to CustomFieldValue collection in MembershipGroup entity
  - Update MembershipGroup to track custom field submission status
  - _Requirements: 3.5, 4.4_

- [x] 12. Update membership registration flow to use custom fields

  - Modify membership registration form to load custom fields for the group
  - Update form submission to validate and store custom field values
  - Update membership approval flow to preserve custom field data
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 4.4_

- [x] 13. Hide legacy position and company fields from UI

  - Update membership registration views to exclude `position` and `company` fields
  - Update membership edit views to exclude these fields
  - Ensure legacy data is preserved in database but not displayed
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [x] 14. Create admin interface for viewing submitted membership data

  - Create view to display membership applications with custom field values organized by tabs
  - Display both field names and submitted values
  - Show archived field names for deleted fields
  - _Requirements: 4.1, 4.2, 4.3_

---

## Phase 5: Testing Implementation

- [ ] 15. Write unit tests for CustomFieldTab service

  - Test tab creation with valid data
  - Test tab creation with missing required fields
  - Test tab update and persistence
  - Test tab deletion with cascade deletion of fields
  - Test tab ordering by display order
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [ ]\* 15.1 Write property test for tab creation persistence

  - **Property 1: Tab creation persistence**
  - **Validates: Requirements 1.1**

- [ ]\* 15.2 Write property test for tab ordering consistency

  - **Property 2: Tab ordering consistency**
  - **Validates: Requirements 1.5**

- [ ]\* 15.3 Write property test for tab cascade deletion

  - **Property 3: Tab cascade deletion**
  - **Validates: Requirements 1.4**

- [ ] 16. Write unit tests for CustomField service

  - Test field creation with valid data
  - Test field creation with missing required fields
  - Test field creation with each supported field type
  - Test field update and persistence
  - Test field deletion with value archival
  - Test field ordering by display order
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

- [ ]\* 16.1 Write property test for custom field creation validation

  - **Property 4: Custom field creation validation**
  - **Validates: Requirements 2.1**

- [ ]\* 16.2 Write property test for field type support

  - **Property 5: Field type support**
  - **Validates: Requirements 2.3**

- [ ]\* 16.3 Write property test for dropdown options persistence

  - **Property 6: Dropdown options persistence**
  - **Validates: Requirements 2.4**

- [ ]\* 16.4 Write property test for custom field update round trip

  - **Property 7: Custom field update round trip**
  - **Validates: Requirements 2.5**

- [ ]\* 16.5 Write property test for field deletion with value archival

  - **Property 8: Field deletion with value archival**
  - **Validates: Requirements 2.6**

- [ ] 17. Write unit tests for CustomFieldValue service

  - Test value creation and storage
  - Test value retrieval by entity
  - Test value retrieval organized by tabs
  - Test value retrieval with archived field names
  - _Requirements: 3.5, 4.1, 4.2, 4.3_

- [ ]\* 17.1 Write property test for form submission persistence

  - **Property 10: Form submission persistence**
  - **Validates: Requirements 3.5**

- [ ]\* 17.2 Write property test for submitted data organization by tabs

  - **Property 11: Submitted data organization by tabs**
  - **Validates: Requirements 4.1**

- [ ]\* 17.3 Write property test for deleted field value retrieval

  - **Property 12: Deleted field value retrieval**
  - **Validates: Requirements 4.3**

- [ ] 18. Write unit tests for form validation and submission

  - Test validation with all required fields present
  - Test validation with missing required fields
  - Test validation error messages
  - Test form submission with valid data
  - Test form submission with invalid data
  - _Requirements: 3.3, 3.4, 3.5_

- [ ]\* 18.1 Write property test for form validation - required fields

  - **Property 9: Form validation - required fields**
  - **Validates: Requirements 3.3, 3.4**

- [ ] 19. Write unit tests for entity type isolation and filtering

  - Test that fields for different entity types are isolated
  - Test that fields for different entity IDs are isolated
  - Test filtering by entity type and entity ID
  - _Requirements: 6.1, 6.2, 6.3, 8.1, 8.4_

- [ ]\* 19.1 Write property test for entity type isolation

  - **Property 13: Entity type isolation**
  - **Validates: Requirements 6.1, 6.3**

- [ ]\* 19.2 Write property test for entity ID filtering

  - **Property 14: Entity ID filtering**
  - **Validates: Requirements 8.1, 8.4**

- [ ]\* 19.3 Write property test for group-specific configuration independence

  - **Property 18: Group-specific configuration independence**
  - **Validates: Requirements 8.3**

- [ ] 20. Write unit tests for field ordering and reordering

  - Test field display order assignment
  - Test field reordering and persistence
  - Test field ordering in form display
  - Test field deletion doesn't affect other field orders
  - _Requirements: 7.1, 7.2, 7.3, 7.4_

- [ ]\* 20.1 Write property test for field display order consistency

  - **Property 15: Field display order consistency**
  - **Validates: Requirements 7.3**

- [ ]\* 20.2 Write property test for field reordering persistence

  - **Property 16: Field reordering persistence**
  - **Validates: Requirements 7.2**

- [ ]\* 20.3 Write property test for field deletion order preservation

  - **Property 17: Field deletion order preservation**
  - **Validates: Requirements 7.4**

- [ ] 21. Write unit tests for legacy field handling

  - Test that position and company fields are not displayed in new forms
  - Test that legacy data is preserved in database
  - Test that legacy data can be retrieved for old records
  - Test that new memberships don't populate legacy fields
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [ ]\* 21.1 Write property test for legacy field non-display

  - **Property 19: Legacy field non-display**
  - **Validates: Requirements 5.1**

- [ ]\* 21.2 Write property test for legacy data preservation

  - **Property 20: Legacy data preservation**
  - **Validates: Requirements 5.2**

- [ ] 22. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

---

## Phase 6: Integration and Verification

- [ ] 23. Integration test for complete membership registration flow

  - Create a group with custom field tabs and fields
  - Submit a membership application with custom field values
  - Verify values are stored correctly
  - Verify admin can view submitted data organized by tabs
  - Verify approval preserves custom field data
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 4.1, 4.2, 4.4_

- [ ] 24. Integration test for entity type isolation

  - Create custom fields for GroupMembership entity type
  - Create custom fields for EventRegistration entity type
  - Verify they don't interfere with each other
  - Verify filtering by entity type works correctly
  - _Requirements: 6.1, 6.2, 6.3_

- [ ] 25. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

---

## Notes

- All services follow the existing pattern in the codebase (IService interface, Service<T> base class)
- All controllers follow the existing pattern (BaseCMSController, BaseAPIController)
- All DTOs follow the existing naming convention
- Database migrations use EF Core Code-First approach
- Tests use xUnit and property-based testing framework (fast-check or similar)
- Legacy `position` and `company` fields in MembershipGroup are NOT deleted, only hidden from UI
- The generic design allows future reuse for EventRegistration and other entities with minimal code changes
