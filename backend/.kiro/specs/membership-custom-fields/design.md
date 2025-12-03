# Design Document: Generic Custom Field System for Dynamic Forms

## Overview

This design implements a generic, entity-agnostic custom field system that allows administrators to dynamically configure form fields for any entity type in the application. The system supports:

- **Group Membership Registration**: Tabs (topics) + custom fields for collecting member information when joining a group
- **Event Registration**: Flat custom fields (no tabs) for collecting registration information
- **Future Entities**: Extensible to support custom fields for any new entity type

The system reuses a single set of database tables (CustomFieldTab, CustomField, CustomFieldValue) across all entity types, identified by entity type and entity ID. This approach minimizes code duplication and provides a consistent pattern for adding dynamic forms to new features.

## Architecture

### Layered Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Controllers (CMS/API)                │
│  - CustomFieldTabController (Group membership tabs)     │
│  - CustomFieldController (Field management)             │
│  - MembershipGroupController (Form submission)          │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                    Service Layer                        │
│  - ICustomFieldTabService / CustomFieldTabService       │
│  - ICustomFieldService / CustomFieldService             │
│  - ICustomFieldValueService / CustomFieldValueService   │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                  Repository Layer                       │
│  - IRepository<CustomFieldTab>                          │
│  - IRepository<CustomField>                             │
│  - IRepository<CustomFieldValue>                        │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                  Database Layer                         │
│  - CustomFieldTab, CustomField, CustomFieldValue        │
└─────────────────────────────────────────────────────────┘
```

### Entity Type Pattern

The system uses an entity type identifier to support multiple entity types:

```csharp
public enum ECustomFieldEntityType
{
    GroupMembership = 1,    // For group membership registration
    EventRegistration = 2   // For event registration (future)
}
```

## Components and Interfaces

### 1. CustomFieldTab Entity & Service

**Purpose**: Manages tabs (topics) for organizing custom fields. Only used for GroupMembership entity type.

**Interface**:

```csharp
public interface ICustomFieldTabService
{
    Task<List<CustomFieldTabDTO>> GetTabsByEntityAsync(ECustomFieldEntityType entityType, string entityId);
    Task<CustomFieldTabDTO> CreateTabAsync(CreateCustomFieldTabRequest request);
    Task<CustomFieldTabDTO> UpdateTabAsync(UpdateCustomFieldTabRequest request);
    Task<bool> DeleteTabAsync(string tabId);
    Task<bool> ReorderTabsAsync(string entityId, List<(string TabId, int Order)> reordering);
}
```

### 2. CustomField Entity & Service

**Purpose**: Manages individual custom fields within tabs or at the entity level.

**Interface**:

```csharp
public interface ICustomFieldService
{
    Task<List<CustomFieldDTO>> GetFieldsByTabAsync(string tabId);
    Task<List<CustomFieldDTO>> GetFieldsByEntityAsync(ECustomFieldEntityType entityType, string entityId);
    Task<CustomFieldDTO> CreateFieldAsync(CreateCustomFieldRequest request);
    Task<CustomFieldDTO> UpdateFieldAsync(UpdateCustomFieldRequest request);
    Task<bool> DeleteFieldAsync(string fieldId);
    Task<bool> ReorderFieldsAsync(string tabId, List<(string FieldId, int Order)> reordering);
}
```

### 3. CustomFieldValue Entity & Service

**Purpose**: Stores actual values submitted by users for custom fields.

**Interface**:

```csharp
public interface ICustomFieldValueService
{
    Task<List<CustomFieldValueDTO>> GetValuesByEntityAsync(ECustomFieldEntityType entityType, string entityId);
    Task<List<CustomFieldValueDTO>> CreateValuesAsync(CreateCustomFieldValuesRequest request);
    Task<bool> DeleteValueAsync(string valueId);
    Task<List<CustomFieldValueDTO>> GetValuesByFieldAsync(string fieldId);
}
```

### 4. Form Submission Handler

**Purpose**: Validates and processes form submissions with custom field values.

**Interface**:

```csharp
public interface ICustomFieldFormHandler
{
    Task<FormValidationResult> ValidateFormAsync(ECustomFieldEntityType entityType, string entityId, Dictionary<string, string> submittedValues);
    Task<List<CustomFieldValueDTO>> SubmitFormAsync(ECustomFieldEntityType entityType, string entityId, Dictionary<string, string> submittedValues);
}
```

## Data Models

### Database Entities

#### CustomFieldTab

```csharp
public class CustomFieldTab : BaseEntity
{
    public ECustomFieldEntityType EntityType { get; set; }
    public string EntityId { get; set; }              // e.g., GroupId for GroupMembership
    public string TabName { get; set; }
    public int DisplayOrder { get; set; }

    public virtual ICollection<CustomField> CustomFields { get; set; }
}
```

#### CustomField

```csharp
public class CustomField : BaseEntity
{
    public string? CustomFieldTabId { get; set; }     // Null for flat fields (e.g., EventRegistration)
    public ECustomFieldEntityType EntityType { get; set; }
    public string EntityId { get; set; }              // e.g., EventId for EventRegistration
    public string FieldName { get; set; }
    public EEventFieldType FieldType { get; set; }    // Reuses existing enum from EventCustomField
    public string? FieldOptions { get; set; }         // JSON for Dropdown/MultipleChoice options
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public virtual CustomFieldTab? CustomFieldTab { get; set; }
    public virtual ICollection<CustomFieldValue> CustomFieldValues { get; set; }
}
```

#### CustomFieldValue

```csharp
public class CustomFieldValue : BaseEntity
{
    public string CustomFieldId { get; set; }
    public ECustomFieldEntityType EntityType { get; set; }
    public string EntityId { get; set; }              // e.g., MembershipGroupId for membership submission
    public string FieldName { get; set; }             // Archived field name for deleted fields
    public string FieldValue { get; set; }

    public virtual CustomField CustomField { get; set; }
}
```

#### ECustomFieldType Enum

```csharp
public enum EEventFieldType : byte
{
    Text = 1,
    Integer = 2,
    Decimal = 3,
    YearOfBirth = 4,
    Boolean = 5,
    DateTime = 6,
    Date = 7,
    Email = 8,
    PhoneNumber = 9,
    Url = 10,
    LongText = 11,
    Dropdown = 12,
    MultipleChoice = 13,
    File = 14,
    Image = 15
}
```

#### ECustomFieldEntityType Enum

```csharp
public enum ECustomFieldEntityType
{
    GroupMembership = 1,
    EventRegistration = 2
}
```

### DTOs

#### CustomFieldTabDTO

```csharp
public class CustomFieldTabDTO
{
    public string Id { get; set; }
    public string EntityId { get; set; }
    public string TabName { get; set; }
    public int DisplayOrder { get; set; }
    public int FieldCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
```

#### CustomFieldDTO

```csharp
public class CustomFieldDTO
{
    public string Id { get; set; }
    public string? CustomFieldTabId { get; set; }
    public string EntityId { get; set; }
    public string FieldName { get; set; }
    public EEventFieldType FieldType { get; set; }
    public string FieldTypeText { get; set; }
    public List<string>? FieldOptions { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
```

#### CustomFieldValueDTO

```csharp
public class CustomFieldValueDTO
{
    public string Id { get; set; }
    public string CustomFieldId { get; set; }
    public string EntityId { get; set; }
    public string FieldName { get; set; }
    public string FieldValue { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
```

## Correctness Properties

A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.

### Property 1: Tab Creation Persistence

_For any_ group and tab data, creating a tab should result in a stored entity with all properties (name, display order, entity association) correctly persisted and retrievable.
**Validates: Requirements 1.1**

### Property 2: Tab Ordering Consistency

_For any_ set of tabs for a group, when displayed, tabs should be ordered by display order in ascending order.
**Validates: Requirements 1.5**

### Property 3: Tab Cascade Deletion

_For any_ tab with associated custom fields, deleting the tab should remove both the tab and all its associated fields from the database.
**Validates: Requirements 1.4**

### Property 4: Custom Field Creation Validation

_For any_ custom field creation request, if required properties (field name, field type, tab/entity association) are missing, the creation should be rejected.
**Validates: Requirements 2.1**

### Property 5: Field Type Support

_For any_ supported field type (Text, Email, PhoneNumber, Date, DateTime, Integer, Decimal, LongText, Dropdown, MultipleChoice, Boolean, URL, File, Image), creating a field with that type should succeed and persist the type correctly.
**Validates: Requirements 2.3**

### Property 6: Dropdown Options Persistence

_For any_ Dropdown or MultipleChoice field with defined options, the options should be stored and retrievable as a list.
**Validates: Requirements 2.4**

### Property 7: Custom Field Update Round Trip

_For any_ custom field, updating its properties (name, type, required status, options) should persist all changes and subsequent queries should return the updated values.
**Validates: Requirements 2.5**

### Property 8: Field Deletion with Value Archival

_For any_ custom field with submitted values, deleting the field should remove the field definition but preserve all submitted values with the archived field name.
**Validates: Requirements 2.6**

### Property 9: Form Validation - Required Fields

_For any_ form submission with missing required fields, validation should fail and return error messages identifying the missing fields.
**Validates: Requirements 3.3, 3.4**

### Property 10: Form Submission Persistence

_For any_ valid form submission, all submitted field values should be stored in CustomFieldValue entities and be retrievable by entity type and entity ID.
**Validates: Requirements 3.5**

### Property 11: Submitted Data Organization by Tabs

_For any_ membership application with submitted custom field values, when retrieved, values should be organized by their associated tabs in display order.
**Validates: Requirements 4.1**

### Property 12: Deleted Field Value Retrieval

_For any_ custom field value submitted before the field was deleted, the value should still be retrievable with the archived field name even after field deletion.
**Validates: Requirements 4.3**

### Property 13: Entity Type Isolation

_For any_ custom fields created for different entity types (GroupMembership vs EventRegistration), querying fields for one entity type should not return fields from other entity types.
**Validates: Requirements 6.1, 6.3**

### Property 14: Entity ID Filtering

_For any_ custom fields for a specific entity (e.g., GroupId), querying by entity type and entity ID should return only fields for that specific entity.
**Validates: Requirements 8.1, 8.4**

### Property 15: Field Display Order Consistency

_For any_ set of fields within a tab, when displayed, fields should be ordered by display order in ascending order.
**Validates: Requirements 7.3**

### Property 16: Field Reordering Persistence

_For any_ field reordering operation, the new display order should be persisted and subsequent queries should return fields in the new order.
**Validates: Requirements 7.2**

### Property 17: Field Deletion Order Preservation

_For any_ set of ordered fields, deleting one field should not affect the display order of remaining fields.
**Validates: Requirements 7.4**

### Property 18: Group-Specific Configuration Independence

_For any_ two different groups, modifying custom fields for one group should not affect the custom field configuration of the other group.
**Validates: Requirements 8.3**

### Property 19: Legacy Field Non-Display

_For any_ membership registration form, the `position` and `company` fields from MembershipGroup should not be included in the custom field form.
**Validates: Requirements 5.1**

### Property 20: Legacy Data Preservation

_For any_ existing membership record with `position` and `company` data, this data should remain in the database and be retrievable.
**Validates: Requirements 5.2**

## Error Handling

### Validation Errors

- Missing required fields in form submission
- Invalid field type specified
- Duplicate tab names within an entity
- Invalid entity type or entity ID

### Business Logic Errors

- Attempting to create fields without a parent tab (for GroupMembership)
- Attempting to delete a tab that doesn't exist
- Attempting to submit values for non-existent fields

### Database Errors

- Constraint violations
- Concurrency issues during reordering

## Testing Strategy

### Unit Testing

Unit tests verify specific examples and edge cases:

- Creating tabs with various names and display orders
- Creating fields with each supported field type
- Validating form submissions with missing required fields
- Retrieving custom fields for specific entities
- Deleting fields and verifying value archival
- Reordering tabs and fields

### Property-Based Testing

Property-based tests verify universal properties that should hold across all inputs:

- **Property 1**: Tab creation persistence - generate random tab data, create, verify storage
- **Property 2**: Tab ordering consistency - generate multiple tabs with random orders, verify ascending order
- **Property 3**: Tab cascade deletion - generate tabs with fields, delete, verify removal
- **Property 4**: Custom field creation validation - generate invalid requests, verify rejection
- **Property 5**: Field type support - generate fields with each type, verify persistence
- **Property 6**: Dropdown options persistence - generate options, verify storage
- **Property 7**: Custom field update round trip - generate updates, verify persistence
- **Property 8**: Field deletion with value archival - generate values, delete field, verify archival
- **Property 9**: Form validation - generate submissions with missing fields, verify validation
- **Property 10**: Form submission persistence - generate valid submissions, verify storage
- **Property 11**: Submitted data organization - generate submissions, verify tab organization
- **Property 12**: Deleted field value retrieval - generate values, delete field, verify retrieval
- **Property 13**: Entity type isolation - generate fields for different types, verify isolation
- **Property 14**: Entity ID filtering - generate fields for different entities, verify filtering
- **Property 15**: Field display order consistency - generate fields with random orders, verify ascending order
- **Property 16**: Field reordering persistence - generate reorderings, verify persistence
- **Property 17**: Field deletion order preservation - generate ordered fields, delete one, verify order preservation
- **Property 18**: Group-specific configuration independence - modify one group, verify others unaffected
- **Property 19**: Legacy field non-display - verify position/company not in form
- **Property 20**: Legacy data preservation - verify old data still in database

### Testing Framework

- **Unit Tests**: xUnit with Moq for mocking
- **Property-Based Tests**: fast-check for C# (or similar PBT library)
- **Minimum iterations**: 100 per property test

### Test Configuration

Each property-based test will be tagged with:

```csharp
// **Feature: membership-custom-fields, Property 1: Tab creation persistence**
// **Validates: Requirements 1.1**
```

## Migration Strategy

### Phase 1: Create New Generic Tables

- Create CustomFieldTab, CustomField, CustomFieldValue entities
- Create migrations
- Deploy to database

### Phase 2: Implement Generic Services

- Implement CustomFieldTabService, CustomFieldService, CustomFieldValueService
- Implement form validation and submission handlers
- Register services in DI container

### Phase 3: Implement Group Membership Integration

- Create controllers for tab and field management
- Update MembershipGroup registration flow to use custom fields
- Hide legacy `position` and `company` fields from UI

### Phase 4: Verify and Deprecate Legacy Code

- Run comprehensive tests
- Verify all custom field functionality works
- Keep legacy fields in database for backward compatibility
- Remove legacy field display from UI

### Phase 5: Future Entity Integration

- Reuse CustomField/CustomFieldValue for EventRegistration (without tabs)
- Minimal code changes required due to generic design

## Implementation Notes

### Key Design Decisions

1. **Generic Entity Type Pattern**: Using ECustomFieldEntityType enum allows the same tables to support multiple entity types without duplication
2. **Optional Tabs**: CustomFieldTabId is nullable to support both tabbed (GroupMembership) and flat (EventRegistration) forms
3. **Archived Field Names**: Storing field names in CustomFieldValue allows displaying deleted field names
4. **Display Order**: Explicit display order field enables flexible reordering without relying on creation order
5. **Soft Delete Not Used**: Custom fields are hard-deleted to keep the system clean; values are preserved separately

### Performance Considerations

- Index on (EntityType, EntityId) for fast filtering
- Index on CustomFieldTabId for tab-based queries
- Consider caching tab/field definitions if they're frequently accessed

### Security Considerations

- Validate entity ownership before allowing field modifications
- Ensure users can only submit values for their own entity instances
- Sanitize field values to prevent injection attacks
