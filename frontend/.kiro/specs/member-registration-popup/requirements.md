# Requirements Document: Member Registration with Custom Form Fields

## Introduction

This feature enhances the member registration process in the BioQMiniapp Zalo Mini App. When users click "Đăng ký thành viên" on the home page, they are navigated to a dedicated registration page. The system retrieves custom form fields from the backend API organized in tabs. Users fill in basic information (name, phone, email, reason, company, position) and custom fields, then submit the registration. The system validates all required fields and submits the complete registration payload to the backend API.

## Glossary

- **Custom Fields**: Dynamic form fields defined by group administrators, organized in tabs
- **Custom Field Tab**: A grouping of related custom fields with a display name and order
- **Field Type**: The data type of a custom field (Text, Email, PhoneNumber, etc.)
- **Required Field**: A custom field that must be filled before form submission
- **Registration Page**: A dedicated page (`GroupRegisterPage.tsx`) for group member registration
- **Tabbed Interface**: UI component displaying multiple tabs of custom fields
- **Payload**: The JSON data structure sent to the registration API
- **Approval Status**: User membership status (0=pending, 1=approved, 2=rejected)
- **User Info**: Zalo user data including ID, name, avatar, and ID by OA
- **Phone Number**: User's phone number retrieved via Zalo SDK

## Requirements

### Requirement 1

**User Story:** As a user on the home page, I want to register as a member by clicking a button, so that I can navigate to a registration page and complete my membership application.

#### Acceptance Criteria

1. WHEN a user clicks the "Đăng ký thành viên" button in HelloCard THEN the system SHALL navigate to the GroupRegisterPage with the groupId and groupName as state parameters
2. WHEN the GroupRegisterPage loads THEN the system SHALL display the group name as the page title
3. WHEN the GroupRegisterPage loads THEN the system SHALL fetch custom form fields from the backend API using the groupId
4. WHEN custom fields are fetched successfully THEN the system SHALL display them organized in tabs with field names and types
5. WHEN the page loads THEN the system SHALL pre-populate basic fields (name, phone) with user information from Recoil state if available

### Requirement 2

**User Story:** As a user on the registration page, I want to fill in a form with basic information and custom fields, so that I can provide all required information for membership approval.

#### Acceptance Criteria

1. WHEN the registration form is displayed THEN the system SHALL show basic fields (name, phone, email, reason, company, position)
2. WHEN the registration form is displayed THEN the system SHALL show custom fields organized in tabs
3. WHEN a custom field is marked as required THEN the system SHALL display a visual indicator (asterisk) next to the field name
4. WHEN a user fills in a custom field THEN the system SHALL validate the input according to the field type
5. WHEN a user attempts to submit the form THEN the system SHALL validate all required fields are filled before allowing submission

### Requirement 3

**User Story:** As a system, I want to render different field types correctly, so that users can input data in the appropriate format.

#### Acceptance Criteria

1. WHEN a custom field has type Text THEN the system SHALL render a text input field
2. WHEN a custom field has type Email THEN the system SHALL render an email input field
3. WHEN a custom field has type PhoneNumber THEN the system SHALL render a phone number input field
4. WHEN a custom field has type LongText THEN the system SHALL render a textarea field
5. WHEN a custom field has type Integer or Decimal THEN the system SHALL render a number input field
6. WHEN a custom field has type Date or DateTime THEN the system SHALL render a date picker field
7. WHEN a custom field has type Boolean THEN the system SHALL render radio buttons with Yes/No options
8. WHEN a custom field has type Dropdown THEN the system SHALL render a select dropdown with options from fieldOptions
9. WHEN a custom field has type MultipleChoice THEN the system SHALL render checkboxes for each option from fieldOptions
10. WHEN a custom field has type URL THEN the system SHALL render a text input field and display the URL as a clickable link

### Requirement 4

**User Story:** As a system, I want to submit the registration with all collected data, so that the user is properly registered with their custom field information.

#### Acceptance Criteria

1. WHEN the user submits the registration form THEN the system SHALL construct a registration payload with basic fields (name, phoneNumber, email, reason, company, position) and custom field values
2. WHEN the registration payload is ready THEN the system SHALL submit it to the backend registration API at /api/groups/{groupId}/join
3. WHEN the registration API returns success THEN the system SHALL display a success message
4. WHEN the registration API returns success THEN the system SHALL navigate back to the previous page or home page
5. WHEN the registration is in progress THEN the system SHALL show a loading indicator and disable the submit button

### Requirement 5

**User Story:** As a system, I want to handle errors gracefully, so that the registration flow is robust and user-friendly.

#### Acceptance Criteria

1. IF a required field is empty THEN the system SHALL display an error message indicating which field is missing
2. IF the custom fields API fails to fetch THEN the system SHALL display an error message and allow the user to retry
3. IF the registration API returns an error THEN the system SHALL display the error message from the API response
4. IF the user is already registered THEN the system SHALL prevent navigation to the registration page
5. WHEN the registration is in progress THEN the system SHALL show a loading indicator to provide user feedback
