# Phase 3 — Constructor & DI Alignment

> **Status:** Not Started | **Depends on:** Phase 1 | **Est. Effort:** 1 week
> **Standards:** [standards.md](standards.md)

---

## Current Issues

1. **No centralized DI extension** — Accounting services are manually registered in Program.cs
2. **`AccountingServices` facade** — 72 constructor parameters, all concrete types
3. **Missing null-guards** — Many constructors don't validate injected dependencies
4. **Missing logger injection** — Some services don't accept `ILogger<T>`
5. **Inconsistent logger type** — Some use `ILogger<T>`, others use `ILoggerFactory`

---

## Task Details

### A3-01: Standardize All Accounting Constructors

Every Accounting service constructor must match the [standard pattern](standards.md#2-constructor-pattern-di):

```csharp
public class GLAccountService : IGLAccountService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<GLAccountService>? _logger;

    public GLAccountService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<GLAccountService>? logger = null)
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

### A3-02: Add Null-Guards to All Existing Constructors

For services that already have constructors but lack null checks, add `?? throw new ArgumentNullException(nameof(param))` to all required parameters.

### A3-03: Standardize LifeCycle Process Service Constructors

Domain process services accept `IProcessService` + domain service. Standardize to:
```csharp
public WellManagementProcessService(
    IProcessService processService,
    WellManagementService domainService,
    ILogger<WellManagementProcessService>? logger = null)
```

### A3-04: Create `AccountingServiceCollectionExtensions`

```csharp
public static class AccountingServiceCollectionExtensions
{
    public static IServiceCollection AddAccountingServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionName = configuration.GetValue("BeepOg:DatabaseConnectionName", "PPDM39");
        // Register all Accounting services here
        services.AddScoped<IGLAccountService>(sp => { ... });
        services.AddScoped<IBudgetService>(sp => { ... });
        // ... 20+ more
        return services;
    }
}
```

### A3-05: Refactor `AccountingServices` Facade

Replace 72 constructor params with DI resolution via `IServiceProvider`:
```csharp
public class AccountingServices
{
    public IGLAccountService GL { get; }
    public IBudgetService Budget { get; }
    // ... resolved via IServiceProvider
    public AccountingServices(IServiceProvider sp) { ... }
}
```

### A3-06: Verify All Services in DI

Audit `Program.cs` to ensure every service with an interface is registered.

---

## Tasks

| ID | Task |
|----|------|
| A3-01 | Standardize Accounting service constructors (~71 services) |
| A3-02 | Add null-guards to all constructors |
| A3-03 | Standardize LifeCycle process service constructors (12 services) |
| A3-04 | Create `AccountingServiceCollectionExtensions` |
| A3-05 | Refactor `AccountingServices` facade to use DI |
| A3-06 | Audit DI registrations — verify 100% coverage |
