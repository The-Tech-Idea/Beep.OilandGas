# Beep.OilandGas.Web Architecture Documentation

## Overview

This document describes the architecture and data flow of the **Beep.OilandGas.Web** application, a Blazor Server application for managing PPDM39 oil and gas data management systems.

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         User Browser                                 │
│                    (Blazor Server Client)                            │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             │ SignalR / HTTP
                             │
┌────────────────────────────▼────────────────────────────────────────┐
│                    Beep.OilandGas.Web                                │
│                  (Blazor Server Application)                         │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  Blazor Pages (.razor)                                       │  │
│  │  - DatabaseManagement.razor                                  │  │
│  │  - PPDM39DatabaseWizard.razor                                │  │
│  │  - Production/Fields.razor, etc.                             │  │
│  └────────────────────┬─────────────────────────────────────────┘  │
│                       │                                              │
│  ┌────────────────────▼─────────────────────────────────────────┐  │
│  │  Services Layer                                               │  │
│  │  ┌────────────────────────────────────────────────────────┐  │  │
│  │  │  DataManagementService                                  │  │  │
│  │  │  - Manages current connection state                     │  │  │
│  │  │  - Caches connection list                               │  │  │
│  │  │  - Centralized connection management                    │  │  │
│  │  └──────────────┬─────────────────────────────────────────┘  │  │
│  │                 │                                              │  │
│  │  ┌──────────────▼─────────────────────────────────────────┐  │  │
│  │  │  ApiClient                                              │  │  │
│  │  │  - HTTP client for API service                          │  │  │
│  │  │  - JSON serialization/deserialization                   │  │  │
│  │  └──────────────┬─────────────────────────────────────────┘  │  │
│  │                 │                                              │  │
│  │  ┌──────────────▼─────────────────────────────────────────┐  │  │
│  │  │  ProgressTrackingClient                                 │  │  │
│  │  │  - SignalR client                                       │  │  │
│  │  │  - Real-time progress updates                           │  │  │
│  │  └──────────────┬─────────────────────────────────────────┘  │  │
│  └─────────────────┼────────────────────────────────────────────┘  │
│                    │                                                  │
└────────────────────┼──────────────────────────────────────────────────┘
                     │ HTTP REST API / SignalR
                     │
┌────────────────────▼──────────────────────────────────────────────────┐
│                  Beep.OilandGas.ApiService                            │
│                    (ASP.NET Core Web API)                             │
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  Controllers                                                 │   │
│  │  - PPDM39SetupController                                     │   │
│  │  - ProductionController, etc.                                │   │
│  └────────────────────┬────────────────────────────────────────┘   │
│                       │                                               │
│  ┌────────────────────▼─────────────────────────────────────────┐   │
│  │  Services                                                     │   │
│  │  ┌────────────────────────────────────────────────────────┐  │   │
│  │  │  PPDM39SetupService                                     │  │   │
│  │  │  - Database setup operations                            │  │   │
│  │  │  - Connection management                                │  │   │
│  │  │  - Script execution                                     │  │   │
│  │  └──────────────┬─────────────────────────────────────────┘  │   │
│  │                 │                                              │   │
│  │  ┌──────────────▼─────────────────────────────────────────┐  │   │
│  │  │  ProgressTrackingService                                │  │   │
│  │  │  - SignalR Hub (ProgressHub)                            │  │   │
│  │  │  - Progress broadcasting                                │  │   │
│  │  └─────────────────────────────────────────────────────────┘  │   │
│  └────────────────────┬─────────────────────────────────────────┘   │
│                       │                                               │
└───────────────────────┼───────────────────────────────────────────────┘
                        │
                        │ IDMEEditor Interface
                        │
┌───────────────────────▼───────────────────────────────────────────────┐
│                      Beep Framework                                    │
│                   (Core Data Management)                               │
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  IDMEEditor                                                  │   │
│  │  - Main editor interface                                     │   │
│  │  - Data source management                                    │   │
│  │  - Connection lifecycle                                      │   │
│  └────────────────────┬────────────────────────────────────────┘   │
│                       │                                               │
│  ┌────────────────────▼─────────────────────────────────────────┐   │
│  │  IConfigEditor / ConfigEditor                                 │   │
│  │  - ConnectionProperties management                            │   │
│  │  - DataConnections collection                                 │   │
│  │  - SaveDataconnectionsValues()                                │   │
│  └────────────────────┬─────────────────────────────────────────┘   │
│                       │                                               │
│  ┌────────────────────▼─────────────────────────────────────────┐   │
│  │  IDataSource                                                  │   │
│  │  - Database operations                                        │   │
│  │  - Executesql(), GetDataTable(), SelectData()                │   │
│  └───────────────────────────────────────────────────────────────┘   │
└───────────────────────────────────────────────────────────────────────┘
```

---

## Core Components

### 1. DataManagementService (Web Layer)

**Purpose**: Central service for managing data sources and connections in the Web application.

**Key Responsibilities**:
- Maintains current connection state (`_currentConnectionName`)
- Caches connection list with 5-minute timeout
- Provides event notifications when current connection changes
- Acts as the single source of truth for connection state in the Web app

**Flow Diagram**:

```
┌─────────────────────────────────────────────────────────────┐
│              DataManagementService                           │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  State:                                              │  │
│  │  - _currentConnectionName (cached)                   │  │
│  │  - _connections (List<DatabaseConnectionListItem>)   │  │
│  │  - _lastRefreshTime                                   │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Methods:                                            │  │
│  │  1. GetCurrentConnectionNameAsync()                  │  │
│  │     → GET /api/ppdm39/setup/current-connection       │  │
│  │                                                       │  │
│  │  2. SetCurrentConnectionAsync(name)                  │  │
│  │     → POST /api/ppdm39/setup/set-current-connection  │  │
│  │     → Updates local cache                            │  │
│  │     → Fires CurrentConnectionChanged event           │  │
│  │                                                       │  │
│  │  3. GetAllConnectionsAsync()                         │  │
│  │     → Uses cache if recent (< 5 min)                 │  │
│  │     → Else GET /api/ppdm39/setup/connections         │  │
│  │                                                       │  │
│  │  4. RefreshConnectionsAsync()                        │  │
│  │     → GET /api/ppdm39/setup/connections              │  │
│  │     → Updates cache                                  │  │
│  │     → Refreshes current connection name              │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

### 2. Connection Management Flow

**Setting Current Connection**:

```
┌──────────────┐
│  Blazor Page │
│ (UI Action)  │
└──────┬───────┘
       │
       │ 1. SetCurrentConnectionAsync(connectionName)
       ▼
┌──────────────────────────────┐
│  DataManagementService       │
│  (Web Layer)                 │
└──────┬───────────────────────┘
       │
       │ 2. POST /api/ppdm39/setup/set-current-connection
       ▼
┌──────────────────────────────┐
│  PPDM39SetupController       │
│  (API Layer)                 │
└──────┬───────────────────────┘
       │
       │ 3. SetCurrentConnection(name)
       ▼
┌──────────────────────────────┐
│  PPDM39SetupService          │
│  (Service Layer)             │
└──────┬───────────────────────┘
       │
       │ 4. _editor.OpenDataSource(connectionName)
       │ 5. _currentConnectionName = connectionName
       ▼
┌──────────────────────────────┐
│  IDMEEditor                  │
│  (Beep Framework)            │
│  - Opens data source         │
│  - Sets active connection    │
└──────────────────────────────┘
       │
       │ 6. Response: SetCurrentDatabaseResult
       │    { Success, RequiresLogout, Message }
       ▼
┌──────────────────────────────┐
│  DataManagementService       │
│  - Updates local cache       │
│  - Fires event               │
└──────────────────────────────┘
       │
       │ 7. UI Updates
       ▼
┌──────────────────────────────┐
│  Blazor Page                 │
│  - Connection list refreshes │
│  - Current indicator updates │
└──────────────────────────────┘
```

---

### 3. IDMEEditor and ConnectionProperties

**IDMEEditor** is the main interface from the Beep Framework that provides:
- `ConfigEditor`: Access to connection configuration
- `OpenDataSource(connectionName)`: Opens and sets a data source as current
- `CurrentDataSource`: Gets the currently active data source
- Data source lifecycle management

**ConnectionProperties** (managed by ConfigEditor):
- `ConnectionName`: Unique identifier
- `DatabaseType`: Database type (SqlServer, PostgreSQL, etc.)
- `Host`, `Port`, `Database`: Connection details
- `UserID`, `Password`: Authentication
- `ConnectionString`: Full connection string
- `GuidID`: Unique GUID identifier

**Relationship**:

```
IDMEEditor
    │
    ├── ConfigEditor (IConfigEditor)
    │       │
    │       ├── DataConnections (List<ConnectionProperties>)
    │       │       │
    │       │       ├── ConnectionProperties 1
    │       │       │   - ConnectionName: "ProductionDB"
    │       │       │   - DatabaseType: DataSourceType.SqlServer
    │       │       │   - Host: "localhost"
    │       │       │   - Port: 1433
    │       │       │   - ...
    │       │       │
    │       │       ├── ConnectionProperties 2
    │       │       │   - ConnectionName: "TestDB"
    │       │       │   - ...
    │       │       │
    │       │       └── ...
    │       │
    │       └── SaveDataconnectionsValues()
    │               - Persists connections to config file
    │
    ├── OpenDataSource(connectionName)
    │       - Opens connection by name
    │       - Sets as current active data source
    │
    └── CurrentDataSource
            - Returns currently active IDataSource instance
```

---

### 4. Database Setup Wizard Flow

```
┌─────────────────────────────────────────────────────────────┐
│         PPDM39DatabaseWizard (Blazor Component)             │
│                                                              │
│  Step 1: Select Database Type                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  GET /api/ppdm39/setup/database-types                │  │
│  │  → User selects: SqlServer, PostgreSQL, MySQL, etc.  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Step 2: Connection Settings                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  - Enter host, port, database, credentials           │  │
│  │  - Check schema privileges                           │  │
│  │  POST /api/ppdm39/setup/check-schema-privileges      │  │
│  │  - Create schema if needed                           │  │
│  │  POST /api/ppdm39/setup/create-schema                │  │
│  │  - Test connection                                   │  │
│  │  POST /api/ppdm39/setup/test-connection              │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Step 3: Install Driver (NuGet Package)                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  POST /api/ppdm39/setup/install-driver               │  │
│  │  → Downloads and installs database driver NuGet      │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Step 4: Execute Scripts                                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  GET /api/ppdm39/setup/scripts/{databaseType}        │  │
│  │  → Lists available DDL scripts                       │  │
│  │                                                       │  │
│  │  POST /api/ppdm39/setup/execute-all-scripts          │  │
│  │  → Returns OperationId                               │  │
│  │  → Connects to SignalR hub                           │  │
│  │  → Receives real-time progress                       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Step 5: Save Connection                                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  POST /api/ppdm39/setup/save-connection              │  │
│  │  → Saves to ConfigEditor.DataConnections             │  │
│  │  → Updates DataManagementService cache               │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

### 5. Progress Tracking (SignalR)

**Architecture**:

```
┌─────────────────────────────────────────────────────────────┐
│              Long-Running Operation                         │
│         (Script Execution, Database Copy, etc.)             │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ Reports progress
                     ▼
┌─────────────────────────────────────────────────────────────┐
│         ProgressTrackingService (API Service)               │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  UpdateProgress(operationId, percentage, message)    │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
│                       │ Broadcast via SignalR                │
│                       ▼                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  ProgressHub (SignalR Hub)                           │  │
│  │  - Groups clients by operationId                     │  │
│  │  - Broadcasts ProgressUpdate to group                │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
└───────────────────────┼──────────────────────────────────────┘
                        │
                        │ SignalR Connection
                        │
┌───────────────────────▼──────────────────────────────────────┐
│         ProgressTrackingClient (Web Layer)                   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  ConnectAsync()                                      │  │
│  │  JoinOperationAsync(operationId)                     │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
│                       │ OnProgressUpdate event               │
│                       ▼                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  ProgressDisplay Component                           │  │
│  │  - Shows progress bar                                │  │
│  │  - Displays status message                           │  │
│  │  - Updates in real-time                              │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## Data Flow Summary

### Connection Management Flow:

1. **User Action** → Blazor Page calls `DataManagementService`
2. **Web Service** → Makes HTTP call to API service
3. **API Controller** → Delegates to `PPDM39SetupService`
4. **Setup Service** → Uses `IDMEEditor.OpenDataSource()`
5. **Beep Framework** → Manages connection in `ConfigEditor`
6. **Response** → Flows back through layers
7. **Web Service** → Updates cache and fires events
8. **UI** → Updates automatically via Blazor reactivity

### Key Design Principles:

1. **Separation of Concerns**:
   - Web layer handles UI and state caching
   - API layer handles business logic
   - Framework layer handles data access

2. **Single Source of Truth**:
   - `DataManagementService` is the main service for connection management in Web app
   - API service manages actual connections via Beep Framework
   - ConfigEditor persists connection data

3. **Caching Strategy**:
   - Connection list cached for 5 minutes
   - Current connection name cached locally
   - Refresh on demand or when connection changes

4. **Event-Driven**:
   - `CurrentConnectionChanged` event notifies subscribers
   - UI components react to state changes
   - SignalR provides real-time updates for long operations

---

## File Structure

```
Beep.OilandGas.Web/
├── Services/
│   ├── DataManagementService.cs      ← Main connection management
│   ├── ApiClient.cs                   ← HTTP client for API
│   └── ProgressTrackingClient.cs      ← SignalR client
│
├── Pages/
│   └── PPDM39/
│       └── DatabaseManagement.razor   ← Connection management UI
│
└── Components/
    ├── PPDM39DatabaseWizard.razor     ← Database setup wizard
    └── ProgressDisplay.razor          ← Progress UI component

Beep.OilandGas.ApiService/
├── Controllers/
│   └── PPDM39/
│       └── PPDM39SetupController.cs   ← API endpoints
│
└── Services/
    ├── PPDM39SetupService.cs          ← Business logic
    └── ProgressTrackingService.cs     ← SignalR hub & service
```

---

## Key APIs

### Connection Management Endpoints:

- `GET /api/ppdm39/setup/current-connection` - Get current connection name
- `POST /api/ppdm39/setup/set-current-connection` - Set current connection
- `GET /api/ppdm39/setup/connections` - Get all connections
- `GET /api/ppdm39/setup/connection/{name}` - Get connection details
- `POST /api/ppdm39/setup/save-connection` - Save new connection
- `PUT /api/ppdm39/setup/connection` - Update connection
- `DELETE /api/ppdm39/setup/connection/{name}` - Delete connection

### SignalR Hub:

- `Hub URL`: `/progressHub`
- `JoinOperationGroup(operationId)` - Join progress group
- `LeaveOperationGroup(operationId)` - Leave progress group
- `JoinWorkflowGroup(workflowId)` - Join workflow group
- `LeaveWorkflowGroup(workflowId)` - Leave workflow group
- `ProgressUpdate` - Server-to-client progress messages
- `WorkflowProgress` - Server-to-client workflow progress messages
- `MultiOperationProgress` - Server-to-client multi-operation progress messages

---

## Enhanced DataManagementService Architecture

The `DataManagementService` has been significantly extended to orchestrate all PPDM39 data operations with comprehensive progress tracking, logging, and workflow support.

### Service Responsibilities

The `IDataManagementService` interface and `DataManagementService` implementation now provide:

1. **Connection Management** (existing)
   - Current connection state
   - Connection list caching
   - Connection switching

2. **Entity Operations** (new)
   - CRUD operations for any PPDM39 entity
   - Generic entity queries with filters
   - Batch operations support

3. **Import/Export Operations** (new)
   - CSV/JSON import with progress tracking
   - CSV/JSON export with progress tracking
   - File upload/download handling

4. **Validation Operations** (new)
   - Single entity validation
   - Batch validation
   - Validation rules retrieval

5. **Data Quality Operations** (new)
   - Table quality metrics
   - Quality dashboard
   - Quality issues identification

6. **Versioning Operations** (new)
   - Create version snapshots
   - Version history retrieval
   - Version restoration

7. **Defaults Operations** (new)
   - Entity default values
   - Well status facets
   - System defaults

### Progress Tracking Integration

All long-running operations support real-time progress tracking via SignalR:

```
Operation Start
    ↓
API returns OperationStartResponse with OperationId
    ↓
DataManagementService subscribes to progress updates via ProgressTrackingClient
    ↓
ProgressTrackingClient.JoinOperationAsync(operationId)
    ↓
SignalR connection to /progressHub
    ↓
ProgressTrackingService broadcasts updates
    ↓
UI components (ProgressDisplay, MultiOperationProgress, WorkflowProgress) display updates
```

### Workflow Pipeline Support

The system supports workflow pipelines that chain multiple operations:

```
WorkflowDefinition
    ↓
PPDM39WorkflowService.ExecuteWorkflowAsync()
    ↓
For each WorkflowStep:
    - Check dependencies
    - Execute operation (ImportCsv, Validate, QualityCheck, etc.)
    - Update workflow progress
    - Handle errors (stop or continue based on StopOnError)
    ↓
WorkflowExecutionResult returned
```

### API Layer Structure

The API service now includes:

1. **PPDM39DataController** - Generic CRUD endpoints
2. **PPDM39ImportExportController** - Import/export with progress
3. **PPDM39ValidationController** - Data validation
4. **PPDM39DataQualityController** - Quality metrics and dashboard
5. **PPDM39VersioningController** - Version management
6. **PPDM39DefaultsController** - Defaults and well status facets
7. **PPDM39WorkflowController** - Workflow pipeline execution

### Service Layer

1. **PPDM39DataService** - Wraps PPDMGenericRepository for API layer
2. **PPDM39WorkflowService** - Orchestrates workflow pipelines
3. **ProgressTrackingService** - SignalR-based progress broadcasting (enhanced with workflow and multi-operation support)

### UI Components

1. **ProgressDisplay.razor** - Single operation progress (enhanced with history, error details, time estimates)
2. **MultiOperationProgress.razor** - Multiple concurrent operations
3. **WorkflowProgress.razor** - Workflow pipeline progress with step-by-step visualization

### Logging

All operations include structured logging at:
- **API Controllers**: Request start, parameters, response status
- **Services**: Operation start, progress milestones, completion/errors
- **Repositories**: Data access operations, query execution, batch sizes

Log context includes:
- OperationId for correlation
- TableName/EntityType
- UserId
- Progress percentage
- Error details

---

## Notes

- **Current Connection Storage**: Stored in memory in `PPDM39SetupService` (`_currentConnectionName` static field) and cached in `DataManagementService` (`_currentConnectionName` instance field).

- **Connection Persistence**: Connections are persisted via `ConfigEditor.SaveDataconnectionsValues()` which saves to configuration files.

- **State Management**: The Web application uses service-level caching and events for state management, avoiding direct dependency on Beep Framework components.

- **Authentication**: Uses OIDC with Identity Server for authentication. Switching connections may require logout if different from current connection.

---

*Last Updated: 2024-12-17*
