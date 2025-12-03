# Product Overview

MiniApp GIBA is a membership and event management platform for business associations and networking groups. The system manages memberships, groups, events, appointments, articles, and various engagement features.

## Core Features

- **Membership Management**: User profiles with custom fields, roles, and group associations
- **Group Management**: Hierarchical organization structure (GIBA > NBD > Club > Group)
- **Event Management**: Event creation, registration, guest lists, check-ins, and custom fields
- **Appointment System**: Scheduling and appointment management between members
- **Content Management**: Articles, showcases, and news with category organization
- **Referral System**: Member referral tracking and logging
- **Notification System**: Event triggers, Omni tools, and campaign management via Hangfire
- **Home Pins**: Configurable home screen content pinning

## User Roles

- **GIBA**: Super admin with full system access
- **NBD**: Network Business Development level access
- **Club**: Club-level administrative access
- **Group**: Group-level administrative access with specific group permissions

## Architecture

The application follows a layered architecture with:

- API controllers for mobile/external integrations
- CMS controllers for admin web interface
- Service layer for business logic
- Repository pattern for data access
- Entity Framework Core for ORM
