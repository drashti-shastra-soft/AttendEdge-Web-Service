# AttendEdgeWebService

AttendEdgeWebService is the backend API for AttendEdge, an attendance and company management platform. It exposes REST endpoints consumed by the AttendEdge Web Console (ASP.NET MVC) and is organized into separate Domain, Infrastructure, Service, and API projects. Data is persisted with Entity Framework and a SQL database.

## Features

- **Authentication** — sign-in and token/session issuance endpoints.
- **User management** — list, create, edit/save user details, basic information, metadata, email-existence checks, and user document/image upload, fetch, and deletion endpoints.
- **Attendance management** — attendance listing, creation, and deletion endpoints.
- **Leave requests** — list, create, view, and delete leave request endpoints.
- **Company management** — company records and company image upload endpoints.
- **Contacts** — contact submission and message endpoints (list, view, delete).
- **AMC management** — create, list, view, and delete AMC endpoints.
- **Holiday management** — create, list, view, and delete holiday endpoints.
- **Account support** — forgot-password and change-password endpoints.
- **Shared API infrastructure** — request validation, pagination helpers, exception handling middleware, and centralized response models.

> Feature descriptions are based on the controllers, domain models, services, and repositories present in this repository. Actual behavior may depend on the connected SQL database schema and configuration.

## Technology stack

- C# and .NET Framework 4.8 (or `<your target framework>`)
- ASP.NET Web API with attribute routing
- Entity Framework (Code First / Database First — `<pick one>`)
- SQL Server database
- Visual Studio solution (`AttendEdgeWebService.sln`)
- Dependency injection configured through Unity
- Swagger / `<any API docs tool>` for endpoint documentation (if applicable)

## Solution structure

```text
V4/
├── AttendEdgeWebService.sln
├── AttendEdgeWebService/                 # ASP.NET Web API application
│   ├── Controllers/                      # API request handling
│   ├── App_Start/                        # Routes, filters, Unity setup, Swagger
│   ├── Config/                           # Environment-specific app settings
│   └── Web.config
├── AttendEdgeWebService.Domain/          # Domain models and list/pagination models
├── AttendEdgeWebService.Infrastructure/  # Repository interfaces, helpers, constants, exceptions
└── AttendEdgeWebService.Service/         # Service implementations and data access (EF)