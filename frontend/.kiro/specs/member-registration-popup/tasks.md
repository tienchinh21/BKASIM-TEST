# Implementation Plan: Member Registration with Custom Form Fields

- [x] 1. Create GroupRegisterPage component structure

  - Create `src/pagesGiba/GroupRegisterPage.tsx` with basic page layout
  - Set up page header with group name
  - Initialize state for form data, custom fields, and loading
  - Add navigation handling with groupId and groupName parameters
  - _Requirements: 1.1, 1.2_

- [ ]\* 1.1 Write unit tests for page initialization

  - Test page loads with correct group name
  - Test navigation state is properly received
  - _Requirements: 1.1, 1.2_

- [x] 2. Implement custom fields API integration

  - Create function to fetch custom fields from `/api/groups/{groupId}/custom-fields`
  - Parse response and organize fields by tabs
  - Handle API errors with appropriate error messages
  - _Requirements: 1.3, 1.4_

- [ ]\* 2.1 Write property test for custom fields fetch

  - **Property 1: Custom Fields Fetch on Page Load**
  - **Validates: Requirements 1.3, 1.4**

- [x] 3. Implement basic form fields rendering

  - Create input fields for name, phone, email, reason, company, position
  - Pre-populate name and phone from Recoil state if available
  - Add labels and required field indicators
  - _Requirements: 2.1, 1.5_

- [ ]\* 3.1 Write property test for form pre-population

  - **Property 8: Form Pre-population**
  - **Validates: Requirements 1.5**

- [x] 4. Implement tabbed interface for custom fields

  - Create tab component to display custom field tabs
  - Implement tab switching functionality
  - Display fields within each tab
  - _Requirements: 1.4, 2.2_

- [x] 5. Implement custom field rendering by type

  - Create `renderCustomField()` function to handle all field types
  - Implement Text input rendering
  - Implement Email input rendering
  - Implement PhoneNumber input rendering
  - Implement LongText textarea rendering
  - Implement Integer and Decimal number input rendering
  - Implement Date and DateTime picker rendering
  - Implement Boolean radio button rendering
  - Implement Dropdown select rendering
  - Implement MultipleChoice checkbox rendering
  - Implement URL input with clickable link rendering
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10_

- [ ]\* 5.1 Write property test for custom field type rendering

  - **Property 3: Custom Field Type Rendering**
  - **Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10**

- [x] 6. Implement form validation logic

  - Create validation function to check all required fields are filled
  - Validate email format
  - Display error messages for missing required fields
  - Prevent form submission if validation fails
  - _Requirements: 2.5, 5.1_

- [ ]\* 6.1 Write property test for required fields validation

  - **Property 2: Required Fields Validation**
  - **Validates: Requirements 2.5, 5.1**

- [ ] 7. Implement registration payload construction

  - Create function to build registration payload with basic and custom fields
  - Convert custom field values to string format
  - Ensure all required fields are included
  - _Requirements: 4.1_

- [ ]\* 7.1 Write property test for registration payload structure

  - **Property 4: Registration Payload Contains All Fields**
  - **Validates: Requirements 4.1**

- [ ] 8. Implement registration API submission

  - Call backend registration API at `/api/groups/{groupId}/join`
  - Handle success response with success message
  - Handle error response with error message display
  - Show loading indicator during submission
  - Disable submit button during loading
  - _Requirements: 4.2, 4.3, 4.5, 5.5_

- [ ]\* 8.1 Write property test for loading state during submission

  - **Property 7: Loading State During Submission**
  - **Validates: Requirements 4.5, 5.5**

- [ ] 9. Implement navigation after successful registration

  - Navigate back to previous page after successful registration
  - Display success message before navigation
  - Handle navigation state properly
  - _Requirements: 4.4_

- [ ]\* 9.1 Write property test for successful registration navigation

  - **Property 5: Successful Registration Navigation**
  - **Validates: Requirements 4.3, 4.4**

- [ ] 10. Implement error handling for API failures

  - Display error messages for custom fields fetch failures
  - Display error messages for registration API failures
  - Implement retry functionality for failed API calls
  - _Requirements: 5.2, 5.3_

- [ ]\* 10.1 Write property test for API error handling

  - **Property 6: API Error Handling**
  - **Validates: Requirements 5.2, 5.3**

- [ ] 11. Update HelloCard component for navigation

  - Update registration button click handler to navigate to GroupRegisterPage
  - Pass groupId and groupName as navigation state
  - Add check to prevent navigation if user is already registered
  - _Requirements: 1.1_

- [ ]\* 11.1 Write unit tests for HelloCard integration

  - Test clicking registration button navigates to GroupRegisterPage
  - Test groupId and groupName are passed correctly
  - Test button is disabled for already registered users
  - _Requirements: 1.1_

- [ ] 12. Checkpoint - Ensure all tests pass

  - Ensure all tests pass, ask the user if questions arise.

- [ ] 13. Test end-to-end registration flow

  - Test complete flow from button click to successful registration
  - Test form validation and error handling
  - Test custom field rendering for all types
  - Test tab switching functionality
  - Test API error scenarios and recovery
  - Verify success message displays
  - Verify navigation back to previous page
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 2.1, 2.2, 2.5, 3.1-3.10, 4.1-4.5, 5.1-5.5_

- [ ] 14. Final Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.
