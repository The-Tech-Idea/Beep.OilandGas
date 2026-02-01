# Beep.OilandGas.Web - Simple Architecture Overview

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    USER (Browser)                            │
└────────────────────────────┬────────────────────────────────┘
                             │
                             │ Blazor Server
                             │
┌────────────────────────────▼────────────────────────────────┐
│              Beep.OilandGas.Web                              │
│           (Blazor Server Application)                        │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  UI Pages                                            │  │
│  │  - DatabaseManagement.razor                         │  │
│  │  - PPDM39DatabaseWizard.razor                       │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                   │
│  ┌───────────────────────▼──────────────────────────────┐  │
│  │  DataManagementService                              │  │
│  │  ⭐ MAIN SERVICE for connection management          │  │
│  │  - Current connection state                         │  │
│  │  - Connection list cache                            │  │
│  │  - Events for connection changes                    │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                   │
│                          │ HTTP API Calls                    │
│                          │                                   │
└──────────────────────────┼───────────────────────────────────┘
                           │
                           │
┌──────────────────────────▼───────────────────────────────────┐
│           Beep.OilandGas.ApiService                          │
│              (REST API + SignalR)                            │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  PPDM39SetupController                              │  │
│  │  - /api/ppdm39/setup/* endpoints                    │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                   │
│  ┌───────────────────────▼──────────────────────────────┐  │
│  │  PPDM39SetupService                                 │  │
│  │  - Database operations                              │  │
│  │  - Script execution                                 │  │
│  │  - Connection management                            │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                   │
└──────────────────────────┼───────────────────────────────────┘
                           │
                           │ Uses
                           │
┌──────────────────────────▼───────────────────────────────────┐
│              Beep Framework                                   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  IDMEEditor                                         │  │
│  │  - OpenDataSource(connectionName)                   │  │
│  │  - ConfigEditor.DataConnections                    │  │
│  │  - CurrentDataSource                               │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                   │
│  ┌───────────────────────▼──────────────────────────────┐  │
│  │  ConnectionProperties                               │  │
│  │  - ConnectionName                                   │  │
│  │  - DatabaseType, Host, Port, Database              │  │
│  │  - UserID, Password                                │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## DataManagementService - The Main Service

**Location**: `Beep.OilandGas.Web/Services/DataManagementService.cs`

**Purpose**: Central service for managing data sources and connections in the entire Web application.

```
┌─────────────────────────────────────────────────────────────┐
│         DataManagementService                                │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Cached State:                                       │  │
│  │  • _currentConnectionName                            │  │
│  │  • _connections (list)                               │  │
│  │  • _lastRefreshTime                                  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Key Methods:                                               │
│  • GetCurrentConnectionNameAsync()                         │
│  • SetCurrentConnectionAsync(name)                         │
│  • GetAllConnectionsAsync()                                │
│  • RefreshConnectionsAsync()                               │
│                                                              │
│  Events:                                                    │
│  • CurrentConnectionChanged                                │
└──────────────────────────────────────────────────────────────┘
```

---

## Connection Management Flow

### Setting Current Connection:

```
User clicks "Set Current" on connection
         │
         ▼
DataManagementService.SetCurrentConnectionAsync()
         │
         ▼
HTTP POST → /api/ppdm39/setup/set-current-connection
         │
         ▼
PPDM39SetupService.SetCurrentConnection()
         │
         ▼
IDMEEditor.OpenDataSource(connectionName)
         │
         ▼
ConfigEditor saves connection state
         │
         ▼
Response flows back → DataManagementService
         │
         ▼
Updates cache + Fires CurrentConnectionChanged event
         │
         ▼
UI updates automatically
```

---

## Key Relationships

### IDMEEditor → ConfigEditor → ConnectionProperties

```
IDMEEditor
    │
    ├── ConfigEditor
    │       │
    │       └── DataConnections: List<ConnectionProperties>
    │               │
    │               └── Each ConnectionProperties contains:
    │                   • ConnectionName
    │                   • DatabaseType
    │                   • Host, Port, Database
    │                   • UserID, Password
    │                   • ConnectionString
    │
    ├── OpenDataSource(name)  ← Sets current connection
    │
    └── CurrentDataSource     ← Gets current active data source
```

---

## Component Interaction

```
┌─────────────────────────────────────────────────────────────┐
│  Blazor Page (UI)                                           │
│  @inject IDataManagementService DataManagementService       │
│                                                              │
│  • Loads connections                                        │
│  • Displays current connection                              │
│  • Handles user actions                                     │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        │ Uses
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  DataManagementService                                      │
│                                                              │
│  • Caches connection state                                  │
│  • Makes API calls                                          │
│  • Fires events                                             │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        │ HTTP API
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  PPDM39SetupController (API)                                │
│                                                              │
│  • Handles REST endpoints                                   │
│  • Returns JSON responses                                   │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        │ Calls
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  PPDM39SetupService                                         │
│                                                              │
│  • Business logic                                           │
│  • Uses IDMEEditor                                          │
│  • Manages connections                                      │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        │ Uses
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  IDMEEditor (Beep Framework)                                │
│                                                              │
│  • ConfigEditor.DataConnections                             │
│  • OpenDataSource()                                         │
│  • CurrentDataSource                                        │
└─────────────────────────────────────────────────────────────┘
```

---

## Summary

**Beep.OilandGas.Web** is a **Blazor Server** application that:

1. **UI Layer**: Blazor pages for user interaction
2. **Service Layer**: `DataManagementService` manages all connection state
3. **API Layer**: REST API service handles business operations
4. **Framework Layer**: Beep Framework manages actual database connections

**Key Point**: `DataManagementService` is the **main service** for the entire Web application - it's the single source of truth for connection state in the Web layer.

**Data Flow**: User → UI → DataManagementService → API → PPDM39SetupService → IDMEEditor → Database

---

*For detailed architecture, see ARCHITECTURE.md*
