# RaceDay — Event Management System

**PROG6212 | Portfolio of Evidence — Part 3**

**Technology:** C# · ASP.NET Core MVC · ASP.NET Core Web API · Entity Framework Core · SQL Server · Azure Blob Storage

## 1. Project Overview

RaceDay is a sporting event management system developed as part of the PROG6212 Portfolio of Evidence (PoE).

The application provides a platform for event organizers to manage sporting events and for participants to discover events, enrol in race activities, and access their event information.

Part 2 focused on developing a RESTful API using ASP.NET Core Web API.

**Part 3 extends this solution by introducing an ASP.NET Core MVC frontend that communicates with the existing REST API.**

The MVC application acts as the presentation layer, while the API remains responsible for business logic, authentication, validation, and database operations.

## 2. Part 3 Objectives

The objectives of Part 3 are to:

- Develop an ASP.NET Core MVC application that consumes the existing RaceDay API.
- Separate the presentation layer from backend business logic.
- Implement participant and organizer interfaces.
- Support authentication and role-based navigation.
- Allow organizers to create, view, update, and delete events.
- Allow participants to browse sporting events and access enrolment functionality.
- Reuse shared API contracts where applicable.
- Maintain data persistence through Entity Framework Core and SQL Server.
- Test the solution and prepare it for containerized deployment.

## 3. Technologies Used

| Technology | Purpose |
|---|---|
| C# | Primary programming language |
| .NET 8 | Application target framework |
| ASP.NET Core MVC | Frontend application |
| ASP.NET Core Web API | RESTful backend |
| Entity Framework Core | Database access and migrations |
| Microsoft SQL Server | Relational database |
| ASP.NET Core Session | Session-based authentication state |
| HttpClient | MVC-to-API communication |
| RaceDay.Contracts | Shared contract project |
| Azure Blob Storage | Image storage |
| Bootstrap 5 | Responsive user interface |
| Swagger / OpenAPI | API documentation and testing |
| xUnit | Automated testing |
| Git and GitHub | Version control |
| Visual Studio 2022 | Development environment |

## 4. System Architecture

RaceDay follows a layered architecture.

```text
                    USER
                     |
                     v
             Web Browser
                     |
                     v
             RaceDay.Mvc
           Presentation Layer
                     |
                     | HTTP Requests
                     v
             RaceDay.Api
           RESTful Backend
              /          \
             v            v
       SQL Server     Azure Blob
        Database        Storage

        RaceDay.Contracts
       Shared API Contracts

        RaceDay.Api.Tests
        Automated Testing
```

### RaceDay.Mvc

The MVC application provides the user interface.

Its responsibilities include:

- Displaying pages and forms.
- Collecting user input.
- Sending HTTP requests to the API.
- Processing API responses.
- Maintaining the MVC user's session.
- Providing role-specific navigation.

### RaceDay.Api

The API handles backend operations, including:

- Authentication and authorization.
- Input validation.
- Event management.
- Participant registration.
- Database operations.
- Integration with Azure Blob Storage.

### RaceDay.Contracts

This project supports the separation of concerns by providing shared contracts used in the solution.

### RaceDay.Api.Tests

This project contains automated tests for the solution.

## 5. Solution Structure

```text
RaceDay.POE/
|
+-- RaceDay.Api/
|   |
|   +-- RaceDay.Api/
|   |   +-- Controllers/
|   |   +-- Data/
|   |   +-- Models/
|   |   +-- Migrations/
|   |   +-- Services/
|   |   +-- Program.cs
|   |   +-- appsettings.json
|   |
|   +-- RaceDay.Mvc/
|   |   +-- Controllers/
|   |   +-- Models/
|   |   +-- Services/
|   |   +-- Views/
|   |   +-- wwwroot/
|   |   +-- Program.cs
|   |   +-- appsettings.json
|   |
|   +-- RaceDay.Contracts/
|   |
|   +-- RaceDay.Api.Tests/
|   |
|   +-- RaceDay.Api.slnx
|
+-- README.md
```

## 6. User Roles and Functionality

RaceDay supports two primary user roles.

### 6.1 Participant

The participant interface provides access to:

- Account registration.
- Login and logout.
- Participant dashboard.
- Browsing available sporting events.
- Event enrolment functionality.
- Viewing personal enrolments.
- Profile navigation and management functionality.

The participant dashboard provides shortcuts to Browse Events and My Enrolments.

### 6.2 Event Organizer

The organizer interface provides access to:

- Account registration.
- Login and logout.
- Organizer dashboard.
- Viewing events created by the organizer.
- Creating sporting events.
- Editing event information.
- Deleting events.
- Accessing event enrolment management.

The organizer dashboard also displays organization information.

### 6.3 Results Functionality

The application includes results-related backend functionality from the earlier API implementation.

The current participant and organizer dashboards display their results management cards as **Coming Soon**.

These dashboard features are not presented as completed Part 3 functionality.

## 7. Authentication and Session Management

RaceDay implements authentication through the API.

The application supports two roles:

- Participant
- Organizer

### Authentication Process

1. A user opens the MVC login page.
2. The user enters their credentials.
3. MVC sends an HTTP request to the API.
4. The API validates the credentials.
5. The API establishes the authenticated session.
6. MVC maintains the session information required for subsequent API requests.
7. The user is redirected to the appropriate dashboard.

### Session Information

The application uses session values such as:

- `UserId` — identifies the logged-in user.
- `Role` — identifies the user's account type.

The MVC API communication service forwards the stored API session cookie when making authenticated requests.

### Password Security

Passwords are hashed using Microsoft's `PasswordHasher` functionality.

Plain-text passwords are not stored in the SQL Server database.

### Role-Based Access

The application distinguishes between organizer and participant functionality.

For example:

- Organizers can access organizer-specific event management operations.
- Participants can access participant-specific features.
- Protected API operations enforce authorization restrictions.

## 8. MVC and API Communication

The MVC application communicates with the API using `HttpClient`.

A dedicated service, `RaceDayApiClient`, manages API requests.

The API base URL is configured in the MVC application's `appsettings.json`.

Example:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7125/"
  }
}
```

### Communication Flow

```text
User submits MVC form
         |
         v
MVC Controller
         |
         v
RaceDayApiClient
         |
         v
HTTP Request to API
         |
         v
API Controller
         |
         v
Database Operation
         |
         v
HTTP Response
         |
         v
MVC Controller
         |
         v
Updated Razor View
```

This design prevents the MVC application from directly accessing the database for normal application operations.

## 9. Database Implementation

RaceDay uses Microsoft SQL Server and Entity Framework Core.

The database contains the following application entities:

| Entity | Description |
|---|---|
| Participant | Participant account information |
| EventOrganizer | Organizer account information |
| Event | Sporting event details |
| EventLocation | Event venue and location |
| Activity | Race activities and categories |
| Registration | Participant event registrations |
| Result | Race results |
| RouteInfo | Route information |
| Weather | Event weather information |

Entity Framework Core manages database access and relationships.

### Database Migrations

The application uses EF Core migrations to create and update the database schema.

The following migrations are present in the migrated project:

- `ModelsCompleted`
- `AddParticipantProfileImage`
- `AddEventBannerImage`

These migrations support the database structure and subsequent image-related model changes.

## 10. Azure Blob Storage

RaceDay integrates Azure Blob Storage for image-related functionality.

The backend includes a `BlobStorageService` that handles image uploads and retrieval.

The service supports validated image uploads, including JPEG, PNG, and WebP formats.

The application restricts uploads to a maximum file size of 5 MB.

### Configuration

The Azure Blob Storage connection string must be configured securely.

For local development, ASP.NET Core User Secrets can be used with the configuration key:

```text
AzureBlobStorage:ConnectionString
```

**Important:** Azure credentials and connection strings must not be committed to GitHub.

## 11. Installation and Setup

### Prerequisites

Before running RaceDay, install:

- Visual Studio 2022 with ASP.NET and web development support.
- .NET 8 SDK.
- Microsoft SQL Server or SQL Server Express.
- SQL Server Management Studio (recommended).
- Git.
- Access to the required Azure Blob Storage configuration.

### Step 1 — Clone the Repository

Part 3 repository:

https://github.com/TheAlchemist1400/RaceDayMVC

Clone the repository using Git or Visual Studio.

### Step 2 — Open the Solution

Open the solution file:

```text
RaceDay.Api/RaceDay.Api.slnx
```

The solution contains four projects:

1. RaceDay.Api
2. RaceDay.Mvc
3. RaceDay.Contracts
4. RaceDay.Api.Tests

### Step 3 — Configure SQL Server

Configure the API connection string in `RaceDay.Api/appsettings.json`.

Example for local SQL Server Express:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Replace the server name with the SQL Server instance installed on your computer.

This example uses Windows Integrated Authentication for local development.

### Step 4 — Configure Azure Blob Storage

Use Visual Studio's **Manage User Secrets** feature for the API project.

Add the required Azure Blob Storage connection string:

```json
{
  "AzureBlobStorage": {
    "ConnectionString": "YOUR_AZURE_STORAGE_CONNECTION_STRING"
  }
}
```

Do not store actual credentials in the repository.

### Step 5 — Restore NuGet Packages

In Visual Studio, restore the solution's NuGet packages.

### Step 6 — Apply Database Migrations

Use the Entity Framework Core migrations included in the API project to update the SQL Server database.

Ensure the API project is selected as the startup project when applying the migrations.

### Step 7 — Configure Multiple Startup Projects

In Visual Studio:

1. Open the solution properties.
2. Select **Multiple startup projects**.
3. Set `RaceDay.Api` to **Start**.
4. Set `RaceDay.Mvc` to **Start**.
5. Leave `RaceDay.Contracts` and `RaceDay.Api.Tests` without startup actions.
6. Select the HTTPS launch profiles.
7. Start debugging.

### Step 8 — Open the Applications

The development launch profiles use:

| Application | URL |
|---|---|
| RaceDay API | https://localhost:7125 |
| Swagger | https://localhost:7125/swagger |
| RaceDay MVC | https://localhost:7008 |

These are local development URLs and may differ depending on the launch configuration.

## 12. Testing and Verification

The solution contains an xUnit test project, `RaceDay.Api.Tests`.

During migration to the new development computer, the following checks completed successfully:

- NuGet package restoration.
- Solution compilation.
- Automated test execution — 1 test passed.
- SQL Server connection.
- EF Core database migrations.
- API startup.
- Swagger accessibility.
- MVC startup.
- MVC-to-API communication.
- User registration and login.
- Logout functionality.

### Functional Testing

The application should also be verified using the following scenarios:

| Test scenario | Expected result |
|---|---|
| Register participant | Participant account created |
| Register organizer | Organizer account created |
| Login with valid credentials | User redirected to appropriate dashboard |
| Login with invalid credentials | Authentication rejected |
| Organizer creates event | Event saved successfully |
| Organizer edits event | Event information updated |
| Organizer deletes event | Event deleted according to business rules |
| Participant browses events | Available events displayed |
| Participant enrols | Registration processed |
| Unauthorized access | Protected operation rejected |
| Logout | Session ended |

The functional test table represents the intended verification scenarios; it is not a claim that every scenario has been executed on the current machine.

## 13. Containerization

Docker containerization is the next deployment milestone for Part 3.

The planned deployment includes:

- An ASP.NET Core Web API container.
- An ASP.NET Core MVC container.
- SQL Server connectivity.
- Secure configuration of Azure Blob Storage credentials.
- Docker Compose for service orchestration.

Container configuration and validation will be documented once implemented and tested.

## 14. Challenges and Solutions

### MVC Session Configuration

**Challenge:** MVC initially encountered a session configuration exception when attempting to access session information.

**Solution:** Session services and middleware were configured in the MVC application.

### API Communication

**Challenge:** MVC could not communicate with the API when the API was not running.

**Solution:** Both projects were configured to start together through Visual Studio.

### Database Migration

**Challenge:** The project was transferred to a new development computer without the original local database.

**Solution:** SQL Server Express was configured and EF Core migrations were applied to recreate the database schema.

### Azure Blob Storage Configuration

**Challenge:** The API required an Azure Blob Storage connection string during startup.

**Solution:** The connection string was configured through ASP.NET Core User Secrets.

## 15. Part 2 and Part 3 Comparison

| Part 2 | Part 3 |
|---|---|
| RESTful Web API | MVC application consuming the API |
| Swagger-based API interaction | Browser-based user interface |
| Backend authentication | MVC login and role-aware navigation |
| API event management | Organizer event management pages

Youtube - https://youtu.be/HOm7rkG-TVo
