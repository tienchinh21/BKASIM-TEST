# Requirements Document: Generic Custom Field System for Dynamic Forms

## Introduction

This feature enables administrators to create dynamic, reusable custom field systems for any entity (groups, memberships, events, etc.). Admins can configure tabs (topics) and custom fields that members must complete when joining a group or registering for an entity. Each group/entity can have its own independent form structure. The system uses a generic, entity-agnostic data model that can be reused across the application without code changes. The legacy `position` and `company` fields in MembershipGroup will be deprecated from the UI but retained in the database for backward compatibility.

## Glossary

- **Custom Field System**: A generic, reusable system for defining and managing dynamic form fields for any entity type
- **Tab (Topic)**: A logical grouping of related custom fields within a form
- **Custom Field**: An individual input field within a tab that collects specific information
- **Field Type**: The data type of a custom field (Text, Email, PhoneNumber, Date, etc.)
- **Required Field**: A custom field that must be filled during form submission
- **Entity Type**: The type of entity the custom field system applies to (e.g., "GroupMembership", "EventRegistration")
- **Entity ID**: The unique identifier of a specific entity instance (e.g., a specific group ID)
- **CustomFieldTab**: Database entity storing tab definitions for a specific entity
- **CustomField**: Database entity storing field definitions within a tab
- **CustomFieldValue**: Database entity storing actual values submitted by users for custom fields
- **Admin**: User with permission to create and manage custom field structures for entities

## Requirements

### Requirement 1

**User Story:** As an admin, I want to create tabs (topics) for a specific group's membership form, so that I can organize custom fields logically by category.

#### Acceptance Criteria

1. WHEN an admin creates a new tab for a group membership form THEN the system SHALL store the tab with a unique identifier, group association, name, and display order
2. WHEN an admin views the tab management interface for a group THEN the system SHALL display all tabs for that group with their names and field counts
3. WHEN an admin updates a tab name THEN the system SHALL persist the change and reflect it in the form structure
4. WHEN an admin deletes a tab THEN the system SHALL remove the tab and all associated custom fields from the form
5. WHEN tabs are displayed in a group membership form THEN the system SHALL order them by display order (ascending)
6. WHEN custom fields are configured for event registration THEN the system SHALL NOT use tabs (topics) - fields are displayed flat without grouping

### Requirement 2

**User Story:** As an admin, I want to create custom fields within tabs for a specific group, so that I can collect specific information from members joining that group.

#### Acceptance Criteria

1. WHEN an admin creates a custom field within a tab THEN the system SHALL require a field name, field type, and tab association
2. WHEN an admin creates a custom field THEN the system SHALL allow marking it as required or optional
3. WHEN an admin specifies a field type THEN the system SHALL support types: Text, Email, PhoneNumber, Date, DateTime, Integer, Decimal, LongText, Dropdown, MultipleChoice, Boolean, URL, File, Image
4. WHEN an admin creates a field with Dropdown or MultipleChoice type THEN the system SHALL allow defining available options
5. WHEN an admin updates a custom field THEN the system SHALL persist changes to name, type, required status, and options
6. WHEN an admin deletes a custom field THEN the system SHALL remove it from the form structure and archive related submitted values

### Requirement 3

**User Story:** As a member, I want to fill out a dynamic group membership registration form organized by tabs, so that I can provide required information when joining a group.

#### Acceptance Criteria

1. WHEN a member views the group membership registration form THEN the system SHALL display all tabs in order with their associated custom fields
2. WHEN a member encounters a required field THEN the system SHALL visually indicate it as mandatory
3. WHEN a member submits the membership registration form THEN the system SHALL validate that all required fields contain values
4. WHEN a member submits the form with missing required fields THEN the system SHALL reject the submission and display error messages
5. WHEN a member successfully submits the form THEN the system SHALL store all field values in CustomFieldValue entities linked to the membership application

### Requirement 4

**User Story:** As an admin, I want to view and manage submitted membership registration data for a group, so that I can review member information and approve memberships.

#### Acceptance Criteria

1. WHEN an admin views a membership application for a group THEN the system SHALL display all submitted custom field values organized by their tabs
2. WHEN an admin views submitted data THEN the system SHALL show field names and submitted values clearly
3. WHEN a custom field is deleted after submission THEN the system SHALL still display the submitted value with the archived field name
4. WHEN an admin approves a membership THEN the system SHALL mark the membership as approved and retain all submitted custom field data

### Requirement 5

**User Story:** As a system maintainer, I want to deprecate legacy fields without breaking existing data, so that I can modernize the membership system gradually.

#### Acceptance Criteria

1. WHEN the system loads the membership registration form THEN the system SHALL NOT display the `position` and `company` fields from MembershipGroup
2. WHEN existing membership records contain `position` and `company` data THEN the system SHALL preserve this data in the database
3. WHEN an admin views legacy membership records THEN the system SHALL display archived `position` and `company` data if present
4. WHEN a new membership is created THEN the system SHALL NOT require or populate `position` and `company` fields

### Requirement 6

**User Story:** As a developer, I want a generic, entity-agnostic custom field system, so that I can reuse it for different entity types without code duplication.

#### Acceptance Criteria

1. WHEN the system stores custom field definitions THEN the system SHALL use a generic structure with entity type and entity ID to support any entity
2. WHEN custom field values are stored THEN the system SHALL associate them with the appropriate entity through entity type and entity ID
3. WHEN querying custom field values THEN the system SHALL support filtering by entity type and entity ID
4. WHEN a custom field system is implemented for a new entity THEN the system SHALL reuse existing CustomFieldTab, CustomField, and CustomFieldValue entities with minimal code changes

### Requirement 7

**User Story:** As an admin, I want to manage field display order within tabs, so that I can control the form layout and user experience.

#### Acceptance Criteria

1. WHEN an admin creates custom fields THEN the system SHALL assign them a display order within their tab
2. WHEN an admin reorders fields within a tab THEN the system SHALL update their display order
3. WHEN fields are displayed in the form THEN the system SHALL order them by display order (ascending) within each tab
4. WHEN a field is deleted THEN the system SHALL not affect the display order of remaining fields

### Requirement 8

**User Story:** As an admin, I want each group to have independent custom field configurations, so that different groups can collect different information from their members.

#### Acceptance Criteria

1. WHEN an admin configures custom fields for a group THEN the system SHALL store these configurations separately for that group
2. WHEN a member joins different groups THEN the system SHALL display different custom field forms based on each group's configuration
3. WHEN an admin modifies custom fields for one group THEN the system SHALL NOT affect custom field configurations for other groups
4. WHEN querying custom fields for a group THEN the system SHALL return only fields configured for that specific group
