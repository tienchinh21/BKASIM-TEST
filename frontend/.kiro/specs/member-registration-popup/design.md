# Design Document: Member Registration with Custom Form Fields

## Overview

This design describes the member registration flow with custom form fields for the BioQMiniapp. When users click "Đăng ký thành viên" on the home page, they navigate to a dedicated registration page. The system fetches custom form fields organized in tabs from the backend API. Users fill in basic information and custom fields, then submit the registration. The system validates all required fields and submits the complete registration payload to the backend API.

## Architecture

The registration flow consists of three main layers:

1. **Presentation Layer**: GroupRegisterPage component with tabbed interface for custom fields
2. **API Integration Layer**: Custom fields API and registration API
3. **State Management Layer**: Recoil atoms for user info, membership info, and registration status

### Data Flow

```
User clicks "Đăng ký thành viên" in HelloCard
  ↓
Navigate to GroupRegisterPage with groupId and groupName
  ↓
Fetch custom fields from /api/groups/{groupId}/custom-fields
  ↓
Display form with basic fields and custom fields in tabs
  ↓
User fills in all required fields
  ↓
User submits form
  ↓
Validate all required fields are filled
  ↓
Construct registration payload with basic and custom field values
  ↓
Submit registration API to /api/groups/{groupId}/join
  ↓
Display success message
  ↓
Navigate back to previous page
```

## Components and Interfaces

### GroupRegisterPage Component

**Location**: `src/pagesGiba/GroupRegisterPage.tsx`

**Props**:

- `groupId`: string - The ID of the group to register for
- `groupName`: string - The name of the group (passed via navigation state)

**State**:

- `customFields`: CustomFieldTab[] - Array of tabs with custom fields
- `formData`: RegistrationFormData - Basic form fields (name, phone, email, reason, company, position)
- `customFieldValues`: Record<string, any> - Values for custom fields keyed by field ID
- `loading`: boolean - Loading state during API calls
- `activeTab`: string - Currently active tab ID

**Methods**:

- `fetchCustomFields()`: Fetch custom fields from backend API
- `handleInputChange()`: Update form data for basic fields
- `handleCustomFieldChange()`: Update custom field values
- `validateForm()`: Validate all required fields are filled
- `handleSubmit()`: Submit registration with all data
- `renderCustomField()`: Render appropriate input component based on field type
- `handleTabChange()`: Switch between custom field tabs

### HelloCard Component Update

**Location**: `src/pagesGiba/home/components/HelloCard.jsx`

**Changes**:

- Update registration button click handler to navigate to GroupRegisterPage instead of calling Zalo SDK APIs
- Pass groupId and groupName as navigation state

### API Integration

**Backend APIs**:

- `GET /api/groups/{groupId}/custom-fields`: Fetch custom form fields organized in tabs
- `POST /api/groups/{groupId}/join`: Submit registration with basic and custom field values

**Custom Fields Payload Structure**:

```typescript
{
  groupId: string;
  groupName: string;
  tabs: CustomFieldTab[];
}

interface CustomFieldTab {
  id: string;
  tabName: string;
  displayOrder: number;
  fields: CustomField[];
}

interface CustomField {
  id: string;
  customFieldTabId: string;
  entityId: string;
  fieldName: string;
  fieldType: string; // Text, Email, PhoneNumber, etc.
  fieldTypeText: string;
  fieldOptions: string | null; // Comma-separated options for Dropdown/MultipleChoice
  isRequired: boolean;
  displayOrder: number;
  createdDate: string;
  updatedDate: string;
}
```

**Registration Payload Structure**:

```typescript
{
  name: string; // Basic field
  phoneNumber: string; // Basic field
  email: string; // Basic field
  reason: string; // Basic field
  company: string; // Basic field
  position: string; // Basic field
  customFields: CustomFieldValue[];
}

interface CustomFieldValue {
  eventCustomFieldId: string; // Field ID
  fieldValue: string; // Stringified value
}
```

## Data Models

### Registration Form Data

```typescript
interface RegistrationFormData {
  name: string; // User's full name
  phoneNumber: string; // User's phone number
  email: string; // User's email (optional)
  reason: string; // Reason for joining the group
  company: string; // User's company (optional)
  position: string; // User's position (optional)
}
```

### Custom Field Tab

```typescript
interface CustomFieldTab {
  id: string;
  tabName: string;
  displayOrder: number;
  fields: CustomField[];
}
```

### Custom Field

```typescript
interface CustomField {
  id: string;
  customFieldTabId: string;
  entityId: string;
  fieldName: string;
  fieldType: string; // Text, Email, PhoneNumber, LongText, Integer, Decimal, Date, DateTime, Boolean, Dropdown, MultipleChoice, URL, YearOfBirth
  fieldTypeText: string;
  fieldOptions: string | null; // Comma-separated options for Dropdown/MultipleChoice
  isRequired: boolean;
  displayOrder: number;
  createdDate: string;
  updatedDate: string;
}
```

### Custom Fields Response

```typescript
interface CustomFieldsResponse {
  success: boolean;
  message: string;
  data: {
    groupId: string;
    groupName: string;
    tabs: CustomFieldTab[];
  };
}
```

### Registration Request

```typescript
interface RegistrationRequest {
  name: string;
  phoneNumber: string;
  email: string;
  reason: string;
  company: string;
  position: string;
  customFields: CustomFieldValue[];
}

interface CustomFieldValue {
  eventCustomFieldId: string;
  fieldValue: string;
}
```

### Registration Response

```typescript
interface RegistrationResponse {
  success: boolean;
  message: string;
  code: number; // 0 = success, 1 = pending approval
  data: {
    id: string;
    groupId: string;
    userId: string;
    status: number; // 0 = pending, 1 = approved, 2 = rejected
  };
}
```

## Correctness Properties

A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.

### Property 1: Custom Fields Fetch on Page Load

_For any_ GroupRegisterPage load with a valid groupId, the system SHALL fetch custom fields from the backend API and display them organized in tabs.

**Validates: Requirements 1.3, 1.4**

### Property 2: Required Fields Validation

_For any_ form submission attempt, if a required field is empty, the system SHALL prevent submission and display an error message indicating which field is missing.

**Validates: Requirements 2.5, 5.1**

### Property 3: Custom Field Type Rendering

_For any_ custom field with a specific fieldType, the system SHALL render the appropriate input component (Text → text input, Email → email input, PhoneNumber → phone input, etc.).

**Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10**

### Property 4: Registration Payload Contains All Fields

_For any_ registration submission, the API request payload SHALL contain all basic fields (name, phoneNumber, email, reason, company, position) and all custom field values with non-empty required fields.

**Validates: Requirements 4.1**

### Property 5: Successful Registration Navigation

_For any_ successful registration API response, the system SHALL display a success message and navigate back to the previous page.

**Validates: Requirements 4.3, 4.4**

### Property 6: API Error Handling

_For any_ failed API response (custom fields fetch or registration), the system SHALL display an error message from the API response or a generic error message.

**Validates: Requirements 5.2, 5.3**

### Property 7: Loading State During Submission

_For any_ registration submission in progress, the system SHALL display a loading indicator and disable the submit button.

**Validates: Requirements 4.5, 5.5**

### Property 8: Form Pre-population

_For any_ GroupRegisterPage load, if user information exists in Recoil state, the system SHALL pre-populate the name and phone fields with that information.

**Validates: Requirements 1.5**

## Error Handling

The system handles the following error scenarios:

1. **Missing Required Field**: User attempts to submit form without filling required fields

   - Display: "Vui lòng nhập {fieldName}"
   - Action: Prevent submission, highlight missing field

2. **Custom Fields Fetch Error**: Backend fails to fetch custom fields

   - Display: Error message from API or generic "Không thể tải biểu mẫu. Vui lòng thử lại!"
   - Action: Show retry button

3. **Invalid Email Format**: User enters invalid email format

   - Display: "Vui lòng nhập email hợp lệ"
   - Action: Prevent submission

4. **Registration API Error**: Backend returns error response

   - Display: Error message from API response
   - Action: Show error message and allow retry

5. **Network Error**: Network request fails

   - Display: "Có lỗi xảy ra. Vui lòng thử lại!"
   - Action: Allow retry

6. **Already Registered**: User attempts to navigate to registration page when already registered
   - Display: Prevent navigation or show message "Bạn đã là thành viên"
   - Action: Redirect to home page

## Testing Strategy

### Unit Testing

Unit tests verify specific examples and edge cases:

- Test custom fields fetch and display
- Test form field rendering for each field type
- Test required field validation
- Test email format validation
- Test registration payload construction with basic and custom fields
- Test error message display for various error scenarios
- Test form pre-population with user data
- Test tab switching functionality
- Test custom field value updates
- Test loading state during submission

### Property-Based Testing

Property-based tests verify universal properties using the fast-check library:

- **Property 1**: Custom fields always fetch and display on page load
- **Property 2**: Required fields always prevent submission when empty
- **Property 3**: Custom field types always render correct input components
- **Property 4**: Registration payload always contains all required fields
- **Property 5**: Successful registration always navigates back
- **Property 6**: API errors always display error messages
- **Property 7**: Loading state always shows during submission
- **Property 8**: Form always pre-populates with available user data

**Testing Framework**: fast-check (property-based testing library for TypeScript)

**Configuration**: Minimum 100 iterations per property test

**Test Annotation Format**: Each property test will be tagged with:

```typescript
// **Feature: member-registration-popup, Property {number}: {property_text}**
// **Validates: Requirements {requirement_numbers}**
```
