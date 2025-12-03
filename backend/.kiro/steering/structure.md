# Project Structure

## Folder Organization

### `/Base` - Core Infrastructure

- **Common/**: Base classes (Service, Mapper, Injector, SlugGenerator)
- **Database/**: DbContext, Repository, UnitOfWork implementations
- **DataSeeder/**: Database seeding logic (UserAdmin)
- **Dependencies/**: External service integrations (Cache, Storage, Zalo)
- **Helper/**: Utility classes (Auth, JWT, FileHandler, ExportHandler)
- **Interface/**: Core interfaces (IRepository, IService, IUnitOfWork)

### `/Controller` - HTTP Endpoints

- **API/**: REST API controllers for mobile/external clients
  - Route pattern: `api/[controller]`
  - Inherit from `BaseAPIController`
  - Return JSON responses with Success/Error helpers
- **CMS/**: Web admin controllers for management interface
  - Route pattern: `{controller}/{action}/{id?}`
  - Inherit from `BaseCMSController`
  - Return Views with TempData messages

### `/Entities` - Domain Models

Organized by feature domain:

- **Commons/**: BaseEntity, ApplicationUser, Ref, SystemConfig
- **Memberships/**: Membership, ProfileTemplate, ProfileCustomField
- **Groups/**: Group, MembershipGroup
- **Events/**: Event, EventRegistration, EventGuest, GuestList, EventGift, EventCustomField
- **Articles/**: Article, ArticleCategory
- **Appointments/**: Appointment
- **Sponsors/**: Sponsor, SponsorshipTier, EventSponsor
- **Subscriptions/**: SubscriptionPlan, GroupPackageConfig, MemberSubscription
- **Logs/**: ActivityLog, ReferralLog, ProfileShareLog
- **HomePins/**: HomePin, HomePinConfiguration
- **Rules/**: BehaviorRule
- **Meetings/**: Meeting
- **ShowCase/**: Showcase
- **OmniTool/**: Campaign and webhook entities
- **ETM/**: Event template and notification entities

### `/Services` - Business Logic

Organized by feature, each with:

- Service implementation (e.g., `MembershipService.cs`)
- Interface definition (e.g., `IMembershipService.cs`)
- Inherit from `Service<T>` base class
- Registered automatically via `Injector.cs`

### `/Models` - Data Transfer Objects

- **Common/**: BaseQueryParameters, PagedResult, RequestQuery, Result
- **DTOs/**: Data transfer objects by feature
- **Queries/**: Query parameter models for filtering/sorting
- **Request/**: API request models
- **Response/**: API response models

### `/Migrations` - EF Core Migrations

- Auto-generated migration files
- Designer files for each migration
- `ApplicationDbContextModelSnapshot.cs` for current schema

### `/Views` - Razor Views

- Organized by controller name
- **Shared/**: Layout, components, validation scripts
- **Pages/**: Razor Pages (Login, Test)

### `/Constants` - Application Constants

- `CTRole.cs`: Role definitions (GIBA, NBD, Club, Group)
- `CTTypeAppointment.cs`: Appointment type constants

### `/Enum` - Enumerations

Status and type enums for various entities

### `/Exceptions` - Custom Exceptions

- `CustomException`, `NotFoundException`, `AlreadyExistsException`

### `/Features` - Feature-specific Logic

- Complex feature implementations (e.g., EventTrigger)

## Naming Conventions

### Files & Classes

- **Controllers**: `{Feature}Controller.cs` (e.g., `MembershipsController.cs`)
- **Services**: `{Feature}Service.cs` and `I{Feature}Service.cs`
- **Entities**: Singular noun (e.g., `Membership.cs`, `Event.cs`)
- **DTOs**: `{Feature}DTO.cs` or `{Feature}Request.cs`

### Database

- **Tables**: Plural (e.g., `Memberships`, `Events`)
- **Primary Keys**: `Id` (string, 32 chars, GUID without hyphens)
- **Foreign Keys**: `{Entity}Id` (e.g., `GroupId`, `MembershipId`)
- **Timestamps**: `CreatedDate`, `UpdatedDate` (DateTime)

### Code Style

- **Async methods**: Suffix with `Async` (e.g., `GetByIdAsync`)
- **Private fields**: Prefix with `_` (e.g., `_repository`)
- **Interfaces**: Prefix with `I` (e.g., `IService`)

## Key Patterns

### Repository Pattern

```csharp
IRepository<T> -> Repository<T> -> UnitOfWork
```

### Service Pattern

```csharp
IService<T> -> Service<T> -> Concrete Service Implementation
```

### Controller Pattern

```csharp
BaseAPIController/BaseCMSController -> Feature Controller
```

### Entity Pattern

```csharp
BaseEntity (Id, CreatedDate, UpdatedDate) -> Domain Entity
```

## Configuration Files

- **Program.cs**: Application startup and DI configuration
- **appsettings.json**: Configuration settings
- **MiniAppGIBA.csproj**: Project dependencies
- **docker-compose.yml**: Docker orchestration
- **Dockerfile**: Container definition

## Static Assets

- **/wwwroot**: Static files (CSS, JS, images, vendor libraries)
- **/uploads**: User-uploaded files
