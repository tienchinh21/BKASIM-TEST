# Membership Custom Fields Spec - Complete

## Status: ✅ APPROVED

This spec defines a generic, reusable custom field system for dynamic forms in MiniApp GIBA.

## Quick Links

- **Requirements**: [requirements.md](./requirements.md) - User stories and acceptance criteria
- **Design**: [design.md](./design.md) - Architecture, data models, and correctness properties
- **Tasks**: [tasks.md](./tasks.md) - Implementation plan with 25 actionable tasks

## Feature Overview

### What This Does

Enables administrators to create dynamic membership registration forms organized by tabs (topics). Each group can have its own custom form requirements. The system is generic and reusable for other entities (events, etc.) with minimal code changes.

### Key Components

1. **CustomFieldTab**: Organizes fields into tabs (only for group membership)
2. **CustomField**: Individual input fields with various data types
3. **CustomFieldValue**: Stores submitted values from members
4. **ECustomFieldEntityType**: Identifies entity type (GroupMembership, EventRegistration)
5. **EEventFieldType**: Reuses existing enum for field types

### Entity Types Supported

- **GroupMembership**: Tabs + custom fields for membership registration
- **EventRegistration**: Flat custom fields (no tabs) - reuses existing EventCustomField pattern

## Implementation Phases

### Phase 1: Database (2 tasks)

- Create entity classes
- Create migrations

### Phase 2: Services (7 tasks)

- CustomFieldTab service
- CustomField service
- CustomFieldValue service
- Form validation handler
- DI registration

### Phase 3: Controllers (3 tasks)

- CMS controllers for admin management
- API controller for form submission

### Phase 4: Integration (4 tasks)

- Update MembershipGroup entity
- Update membership registration flow
- Hide legacy fields (position, company)
- Create admin view for submitted data

### Phase 5: Testing (8 tasks)

- Unit tests for all services
- Optional property-based tests for correctness properties

### Phase 6: Verification (3 tasks)

- Integration tests
- Final checkpoint

## Key Design Decisions

1. **Reuses EEventFieldType**: Consistency with existing EventCustomField system
2. **Generic Entity Type Pattern**: Same tables support multiple entity types
3. **Optional Tabs**: Supports both tabbed and flat forms
4. **Archived Field Names**: Preserves submitted values even after field deletion
5. **Legacy Field Preservation**: `position` and `company` fields hidden but not deleted

## Correctness Properties

20 properties defined to ensure system correctness:

- Tab and field management (creation, update, deletion, ordering)
- Form validation and submission
- Data persistence and retrieval
- Entity type isolation
- Legacy field handling

## Next Steps

1. Open `tasks.md` and click "Start task" on the first task
2. Follow the implementation plan sequentially
3. Each task builds on previous tasks
4. Run tests after each phase checkpoint

## Notes

- All code follows existing project patterns and conventions
- Uses xUnit for unit tests
- Property-based tests use fast-check or similar library
- Minimum 100 iterations per property test
- Legacy fields preserved in database for backward compatibility
