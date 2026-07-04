# Beep.OilandGas — Coding Standards & Conventions

> **Purpose:** Eliminate hallucinations and inconsistencies across Accounting, LifeCycle, and ProductionAccounting services.
> **Enforcement:** All new code must follow these standards. Existing code is revised per-phase to comply.

---

## 1. Naming Conventions

### 1.1 Services & Interfaces

| Rule | Example | Anti-Pattern |
|------|---------|-------------|
| Interface prefixed with `I` | `IRevenueService` | `RevenueServiceInterface` |
| Implementation = interface name minus `I` | `RevenueService : IRevenueService` | `RevenueServiceImpl` |
| Suffix `Service` for business logic | `TaxProvisionService` | `TaxProvisionManager` |
| Suffix `Repository` for data access | `PPDMGenericRepository` | Already correct |
| Suffix `Client` for HTTP clients | `AccountingServiceClient` | Already correct |

### 1.2 Methods

| Rule | Example | Anti-Pattern |
|------|---------|-------------|
| Async methods end with `Async` | `GetRevenueAsync()` | `GetRevenue()` (if async) |
| CRUD: `Get`, `Create`, `Update`, `Delete` | `CreateJournalEntryAsync()` | `AddJournalEntryAsync()` |
| Boolean queries: `Has`, `Is`, `Can` | `CanPostJournalAsync()` | `CheckIfCanPostJournal()` |
| List queries: plural or `List` | `GetJournalsAsync()` / `GetJournalListAsync()` | — |
| Action verbs for commands | `PostRevenueAsync()`, `ReconcileAsync()` | — |

### 1.3 Parameters

| Rule | Example |
|------|---------|
| `string userId` — last non-optional parameter | `CreateAsync(dto, userId)` |
| `CancellationToken` — always last, always named `cancellationToken` or `ct` | `...ct = default)` |
| `string connectionName` — default `"PPDM39"` | `connName = "PPDM39"` |
| `ILogger<T>? logger` — always nullable, always last injected | `ILogger<T>? logger = null` |

### 1.4 Private Fields

| Rule | Example |
|------|---------|
| Prefixed with `_` | `_editor`, `_logger`, `_connectionName` |
| Readonly when set in constructor | `private readonly IDMEEditor _editor;` |
| No Hungarian notation | ❌ `m_editor`, `strConnection` |

---

## 2. Constructor Pattern (DI)

### Mandatory Standard Pattern

Every service constructor MUST follow this exact order:

```csharp
public class SomeService : ISomeService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<SomeService>? _logger;

    public SomeService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<SomeService>? logger = null)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _connectionName = connectionName;
        _logger = logger;
    }
}
```

### Variations Allowed

- Services that only do HTTP calls: inject `HttpClient` instead of BeepDM deps
- Services that only do computation: inject only `ILogger`
- Singleton services: `IRoleHierarchyService` — must be thread-safe

---

## 3. Repository Pattern

### 3.1 PPDMGenericRepository Creation

```csharp
private PPDMGenericRepository GetRepo<T>(string tableName) =>
    new PPDMGenericRepository(
        _editor, _commonColumnHandler, _defaults, _metadata,
        typeof(T), _connectionName, tableName, null);
```

**Rules:**
- Always use this helper — NEVER inline `new PPDMGenericRepository(...)` in method bodies
- Always pass `null` for the logger parameter (last arg)

### 3.2 Query Pattern

```csharp
var filters = new List<AppFilter>
{
    new() { FieldName = "FIELD_ID", FilterValue = value },
};
var results = await repo.GetAsync(filters);
var items = results.OfType<ENTITY_TYPE>().ToList();
```

### 3.3 Insert/Update Pattern

```csharp
var entity = new ENTITY_TYPE { ... };
await repo.InsertAsync(entity, userId);
// or
entity.PROPERTY = newValue;
await repo.UpdateAsync(entity, userId);
```

---

## 4. Method Implementation Standards

### 4.1 Every public method MUST have:

1. XML doc comment (`<summary>` at minimum)
2. Null checks on required parameters
3. Try/catch with logger (unless propagating to caller)

### 4.2 Logging Standards

```csharp
_logger?.LogInformation("Operation completed: {EntityId}, User={UserId}", entityId, userId);
_logger?.LogWarning("Operation had issues: {Reason}", reason);
_logger?.LogError(ex, "Operation failed: {EntityId}", entityId);
```

**Rules:**
- Always use structured logging (template placeholders, not string interpolation)
- Always null-conditional (`_logger?.`)
- Information for state changes, Warning for recoverable issues, Error for failures

### 4.3 Return Types

| Scenario | Return Type |
|----------|------------|
| Single entity | `Task<T?>` (nullable for not-found) |
| List of entities | `Task<List<T>>` |
| Success/failure | `Task<bool>` |
| Complex result | `Task<ResultType>` with `Success`, `Errors` properties |
| Void command | `Task` (not `void`) |

---

## 5. Entity Conventions

### 5.1 ModelEntityBase

All entities extending `ModelEntityBase`:
- Use explicit backing fields + `SetProperty()` for properties
- `[Key]` on primary key property
- PPDM audit columns inherited from `ModelEntityBase`

### 5.2 New Entity Checklist

- [ ] Extends `ModelEntityBase`
- [ ] Has `[Key]` attribute on ID property
- [ ] Registered in appropriate module's `EntityTypes` list
- [ ] SQL DDL script created in `Scripts/Sqlserver/`
- [ ] Seeded via `ISeedService` if reference data

---

## 6. Module Registration

### 6.1 Entity Registration

```csharp
typeof(NEW_ENTITY)  // in the module's _entityTypes list
```

### 6.2 Order Convention

| Range | Purpose |
|-------|---------|
| 0-20 | Core PPDM references |
| 30-40 | Security (UserManagement) |
| 41-49 | RBAC/Admin extensions |
| 50-60 | LifeCycle/Workflow |
| 70+ | Domain calculation modules |

---

## 7. Cross-Project Dependencies

```
PPDM.Models  ←  PPDM39  ←  PPDM39.DataManagement
                                    ↑
Models  ←  UserManagement  ←  (Web, ApiService)
   ↑              ↑
LifeCycle ────────┘
   ↑
(All domain modules: Accounting, ChokeAnalysis, etc.)
```

**Rules:**
- Domain modules reference `PPDM39.DataManagement`, `Models`, `PPDM.Models`, `PPDM39`
- `UserManagement` references `LifeCycle` (for SoD/workflow integration)
- NEVER reference `Web` or `ApiService` from a domain module

---

## 8. Hallucination Prevention Checklist

Before writing ANY code, verify:

1. **Does this type exist?** Check the actual file, not memory.
   - `grep "class ClassName"` or `grep "interface IName"`
2. **Is the namespace correct?** Read line 1 of the source file.
   - `head -1 path/to/file.cs` to see the namespace
3. **Does this method exist?** Read the interface/class.
   - `grep "MethodName" path/to/interface.cs`
4. **What are the actual parameter types?** Never guess parameter names.
   - Read the method signature from the source
5. **Is this project referenced?** Check the `.csproj` file.
   - `grep "ProjectReference" project.csproj`

### Common Hallucination Patterns to Avoid

| Hallucination | Reality Check |
|--------------|---------------|
| "The method is `CreateAsync(entity, userId)`" | Read the interface — it might be `InsertAsync(entity, userId)` |
| "The class is in namespace `X.Y.Z`" | Read line 1 — namespaces change during refactors |
| "The entity has property `STATUS`" | Read the entity file — it might be `CURRENT_STATUS` |
| "The project references `X`" | Check the csproj — transitive refs don't count |
| "The type `AppFilter` is in `Beep.Report`" | It IS in `TheTechIdea.Beep.Report` — but check! |
