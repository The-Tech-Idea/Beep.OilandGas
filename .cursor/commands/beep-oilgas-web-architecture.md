# Beep.OilandGas.Web Architecture Documentation

## Overview

The `Beep.OilandGas.Web` application is a Blazor Server application that provides a comprehensive web interface for managing oil and gas lifecycle operations, data management, and production accounting. The application follows a modern architecture pattern with clear separation of concerns, service-oriented design, and integration with the API service layer.

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Blazor Web Application                   │
│                  (Beep.OilandGas.Web)                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Pages      │  │  Components  │  │   Services   │     │
│  │   (Razor)    │  │   (Razor)    │  │   (C#)      │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
│         │                  │                  │            │
│         └──────────────────┴──────────────────┘            │
│                            │                                │
│                            ▼                                │
│                  ┌──────────────────┐                       │
│                  │   ApiClient      │                       │
│                  │  (HTTP Client)    │                       │
│                  └────────┬─────────┘                       │
└────────────────────────────┼─────────────────────────────────┘
                             │
                             │ HTTP/REST
                             │
┌────────────────────────────┼─────────────────────────────────┐
│                            ▼                                 │
│              ┌──────────────────────────┐                    │
│              │   API Service            │                    │
│              │ (Beep.OilandGas.ApiService)│                  │
│              └────────────┬─────────────┘                    │
│                           │                                   │
│              ┌────────────┴─────────────┐                    │
│              │                         │                      │
│    ┌──────────▼──────────┐  ┌──────────▼──────────┐          │
│    │  LifeCycle Services │  │ DataManagement      │          │
│    │  ProductionAccounting│ │ Services           │          │
│    └──────────┬──────────┘  └──────────┬──────────┘          │
│              │                         │                      │
│              └────────────┬────────────┘                      │
│                           │                                   │
│              ┌────────────▼─────────────┐                    │
│              │    IDMEEditor            │                    │
│              │  (Data Access Layer)     │                    │
│              └────────────┬─────────────┘                    │
│                           │                                   │
│              ┌────────────▼─────────────┐                    │
│              │   Database (PPDM39)      │                    │
│              └──────────────────────────┘                    │
└───────────────────────────────────────────────────────────────┘
```

### Technology Stack

- **Framework**: ASP.NET Core 8.0 (Blazor Server)
- **UI Library**: MudBlazor 8.x (Material Design components)
- **Authentication**: OpenID Connect (OIDC) with Identity Server
- **State Management**: Blazor Server SignalR connections
- **Local Storage**: Blazored.LocalStorage
- **HTTP Client**: System.Net.Http.HttpClient
- **JSON Serialization**: System.Text.Json

## Component Architecture

### 1. Pages Layer

Pages are the top-level components that represent different routes in the application. They are organized by functional area:

- **PPDM39 Pages**: Well management, field operations, production, exploration, development, decommissioning
- **Data Management Pages**: Quality dashboard, validation, versioning, audit
- **Accounting Pages**: Royalties, cost allocation, volume reconciliation
- **Admin Pages**: Access control, user roles, hierarchy configuration

### 2. Components Layer

Reusable components organized by functionality:

- **Layout Components**: MainLayout, role-based layouts (AccountantLayout, ManagerLayout, etc.)
- **Navigation Components**: NavMenu, role-specific navigation menus
- **Data Components**: PPDMDataGrid, PPDMEntityForm, PPDMMapView
- **Dialog Components**: Various edit dialogs (FacilityEditDialog, PoolEditDialog, etc.)
- **Progress Components**: ProgressDisplay, MultiOperationProgress, WorkflowProgress
- **Connection Components**: ConnectionCheck, ConnectionSetupDialog

### 3. Services Layer

Business logic and API integration services:

- **ApiClient**: Generic HTTP client for API service communication
- **DataManagementService**: Centralized data operations (CRUD, import/export, validation)
- **EntityMetadataService**: Entity metadata management
- **ProgressTrackingClient**: SignalR client for real-time progress updates
- **ThemeProvider**: Theme and branding management

## Data Flow

### Request Flow

1. **User Interaction** → Page/Component
2. **Component** → Service Layer (e.g., DataManagementService)
3. **Service** → ApiClient
4. **ApiClient** → HTTP Request to API Service
5. **API Service** → Business Logic → Data Access Layer
6. **Response** flows back through the chain

### Authentication Flow

1. **User** → Login Page
2. **Login** → Redirect to Identity Server (OIDC)
3. **Identity Server** → Authenticate user
4. **Callback** → Web App receives tokens
5. **Web App** → Stores authentication cookie
6. **Subsequent Requests** → Include authentication token

## Service Registration

Services are registered in `Program.cs`:

```csharp
// UI Services
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

// API Client
builder.Services.AddHttpClient<ApiClient>(client => {
    client.BaseAddress = new Uri(apiServiceUrl);
});

// Application Services
builder.Services.AddScoped<IDataManagementService, DataManagementService>();
builder.Services.AddScoped<IProgressTrackingClient, ProgressTrackingClient>();

// Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie()
.AddOpenIdConnect("oidc", options => {
    // OIDC configuration
});
```

## Component Relationships

### Layout Hierarchy

```
App.razor (Router)
  └── MainLayout.razor
      ├── MudAppBar (Navigation)
      ├── NavMenu (Navigation Menu)
      └── @Body (Page Content)
          └── Page Components
              └── Reusable Components
```

### Service Dependencies

```
Pages/Components
    ↓
Services (DataManagementService, etc.)
    ↓
ApiClient
    ↓
API Service (HTTP)
```

## Design Patterns

### 1. Service-Oriented Architecture

- Clear separation between UI and business logic
- Services encapsulate API communication
- Services are testable and reusable

### 2. Component Composition

- Small, focused components
- Components composed to build complex UIs
- Reusable across different pages

### 3. Dependency Injection

- All services registered in DI container
- Components receive dependencies via `@inject`
- Enables testing and loose coupling

### 4. Repository Pattern (via API)

- Data access abstracted through API service
- Web app doesn't directly access database
- API service implements repository pattern

## State Management

### Server-Side State

- **Blazor Server**: Maintains component state on server
- **SignalR**: Real-time updates via persistent connections
- **Session State**: User-specific state maintained per connection

### Client-Side State

- **LocalStorage**: User preferences (theme, settings)
- **Component State**: Component-level state via `@code` blocks
- **Cascading Parameters**: Shared state across component tree

## Security Architecture

### Authentication

- **OIDC**: OpenID Connect for authentication
- **Cookie Authentication**: Default scheme for Blazor
- **Token Management**: Tokens stored in cookies

### Authorization

- **Role-Based**: User roles from Identity Server
- **Policy-Based**: Authorization policies for resource access
- **Component-Level**: `[Authorize]` attributes on pages/components

### Data Security

- **HTTPS**: All communication over HTTPS
- **API Authentication**: Tokens included in API requests
- **Input Validation**: Client and server-side validation

## Error Handling

### Client-Side Error Handling

- **Try-Catch**: Error handling in service methods
- **Error Boundaries**: Error display components
- **User Feedback**: Snackbar notifications for errors

### Server-Side Error Handling

- **Exception Middleware**: Global exception handling
- **Error Pages**: Custom error pages for different scenarios
- **Logging**: Comprehensive error logging

## Performance Considerations

### Blazor Server Optimizations

- **Virtualization**: Virtual scrolling for large lists
- **Lazy Loading**: Load components on demand
- **Caching**: Cache frequently accessed data
- **SignalR Optimization**: Efficient SignalR message handling

### API Communication

- **Connection Pooling**: HTTP client connection pooling
- **Request Batching**: Batch multiple requests when possible
- **Response Caching**: Cache API responses where appropriate

## Configuration

### appsettings.json

```json
{
  "ApiService": {
    "BaseUrl": "https://localhost:7001"
  },
  "IdentityServer": {
    "BaseUrl": "https://localhost:7062/"
  }
}
```

### Environment-Specific Configuration

- **Development**: `appsettings.Development.json`
- **Production**: Environment variables or secure configuration

## Deployment Architecture

### Components

- **Web Application**: Blazor Server application
- **API Service**: RESTful API service
- **Identity Server**: Authentication and authorization server
- **Database**: PPDM39 database

### Deployment Options

- **IIS**: Windows Server deployment
- **Docker**: Containerized deployment
- **Azure**: Cloud deployment options
- **Kubernetes**: Container orchestration

## Related Documentation

- [API Integration](beep-oilgas-web-api-integration.md)
- [Service Layer](beep-oilgas-web-services.md)
- [Components](beep-oilgas-web-components.md)
- [Pages](beep-oilgas-web-pages.md)
- [Authentication](beep-oilgas-web-authentication.md)
- [Connection Management](beep-oilgas-web-connection-management.md)

