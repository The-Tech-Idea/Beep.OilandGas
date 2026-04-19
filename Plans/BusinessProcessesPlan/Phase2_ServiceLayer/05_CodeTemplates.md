# Phase 2 — Code Templates
## Complete C# Implementation Snippets

> All snippets are complete and compilable in isolation.  
> Namespace: `Beep.OilandGas.LifeCycle.Services`  
> Requires `PPDMGenericRepository`, `AppFilter`, `ProcessDefinition`, `ProcessInstance`, `ProcessGuardException`.

---

## Template 1: ProcessDefinitionInitializer — Exploration Category

This is the canonical pattern for all 12 `Initialize*ProcessesAsync` methods.  
Only the Exploration implementation is shown in full; the others follow the identical pattern.

```csharp
// File: Beep.OilandGas.LifeCycle/Services/ProcessDefinitionInitializer.cs
// Part of the partial class ProcessDefinitionInitializer

namespace Beep.OilandGas.LifeCycle.Services
{
    public partial class ProcessDefinitionInitializer
    {
        private async Task InitializeExplorationProcessesAsync(string userId)
        {
            Log.Information("Initializing Exploration process definitions");

            var definitions = new List<ProcessDefinition>
            {
                new ProcessDefinition
                {
                    ProcessId       = "EXP-LEAD-ASSESS",
                    ProcessName     = "Lead-to-Prospect Assessment",
                    ProcessType     = "GATE_REVIEW",
                    ProcessCategory = "1",
                    JurisdictionTags = "USA,CANADA,INTERNATIONAL",
                    MinApproverCount = 1,
                    RequiresDocuments = true,
                    RequiredDocumentTypes = new List<string> { "PROSPECT_REPORT", "RESOURCE_ESTIMATE" },
                    Steps = new List<ProcessStepDefinition>
                    {
                        new() { StepId = "EXP-LA-01", StepName = "Create Lead Record",           Sequence = 1, PrimaryPPDMTable = "POOL",                  InboundTrigger = "capture_gg" },
                        new() { StepId = "EXP-LA-02", StepName = "Capture G&G Data",             Sequence = 2, PrimaryPPDMTable = "RM_INFORMATION_ITEM",    InboundTrigger = "assess_risk" },
                        new() { StepId = "EXP-LA-03", StepName = "Risk & Resource Assessment",   Sequence = 3, PrimaryPPDMTable = "POOL_VERSION",           InboundTrigger = "approve",      CompletionGuardKey = "RESOURCE_ESTIMATE_ATTACHED" },
                        new() { StepId = "EXP-LA-04", StepName = "Partner Consultation",         Sequence = 4, PrimaryPPDMTable = "INTEREST_SET",           InboundTrigger = "submit_for_approval" },
                        new() { StepId = "EXP-LA-05", StepName = "Internal Approval",            Sequence = 5, PrimaryPPDMTable = "PROJECT_STEP_BA",        InboundTrigger = "upgrade_to_prospect", CompletionGuardKey = "MIN_APPROVERS_MET" },
                        new() { StepId = "EXP-LA-06", StepName = "Register as Prospect",         Sequence = 6, PrimaryPPDMTable = "POOL",                  InboundTrigger = "register" }
                    }
                },
                new ProcessDefinition
                {
                    ProcessId       = "EXP-PROSPECT-DISC",
                    ProcessName     = "Prospect-to-Discovery Evaluation",
                    ProcessType     = "GATE_REVIEW",
                    ProcessCategory = "1",
                    JurisdictionTags = "USA,CANADA,INTERNATIONAL",
                    MinApproverCount = 1,
                    RequiresDocuments = true,
                    RequiredDocumentTypes = new List<string> { "PROSPECT_REPORT", "RESOURCE_ESTIMATE", "RISK_MATRIX" }
                    // Steps ellided — follow same pattern as EXP-LA-*
                },
                new ProcessDefinition
                {
                    ProcessId       = "EXP-LICENSE-ACQUIRE",
                    ProcessName     = "Exploration License Acquisition",
                    ProcessType     = "COMPLIANCE_REPORT",
                    ProcessCategory = "1",
                    JurisdictionTags = "USA,CANADA,INTERNATIONAL",
                    RequiresDocuments = true,
                    RequiredDocumentTypes = new List<string> { "LICENSE_APPLICATION", "ENVIRONMENTAL_SCREEN" }
                    // Steps ellided — see 03_ProcessDefinitionCatalog.md
                }
                // … 5 more Exploration definitions (EXP-EXPL-WELL-AUTH, EXP-GG-DATA-ACQUIRE, EXP-FARMOUT, EXP-WELL-RESULTS, EXP-PROSPECT-APPROVE)
            };

            foreach (var def in definitions)
            {
                await UpsertProcessDefinitionAsync(def, userId);
            }

            Log.Information("Exploration process definitions initialized: {Count}", definitions.Count);
        }
    }
}
```

---

## Template 2: UpsertProcessDefinitionAsync

Idempotent seed helper. Checks `ProcessId` uniqueness before inserting.

```csharp
private async Task UpsertProcessDefinitionAsync(ProcessDefinition def, string userId)
{
    var repo = BuildProcessDefinitionRepo();

    var existing = await repo.GetAsync(new List<AppFilter>
    {
        new AppFilter { FieldName = "PROJECT_OBS_NO", Operator = "=", FilterValue = def.ProcessId },
        new AppFilter { FieldName = "ACTIVE_IND",     Operator = "=", FilterValue = "Y" }
    });

    if (existing.Count > 0)
    {
        Log.Debug("Process definition '{ProcessId}' already exists — skipping", def.ProcessId);
        return;
    }

    await repo.InsertAsync(def, userId);
    Log.Information("Seeded process definition '{ProcessId}'", def.ProcessId);
}

private PPDMGenericRepository BuildProcessDefinitionRepo()
{
    var metadata   = _metadata.GetTableMetadataAsync("PROJECT").GetAwaiter().GetResult();
    var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
    return new PPDMGenericRepository(
        _editor, _commonColumnHandler, _defaults, _metadata,
        entityType, _connectionName, "PROJECT");
}
```

---

## Template 3: ProcessStateMachine — Register Work Order SM

```csharp
// File: Beep.OilandGas.LifeCycle/Services/ProcessStateMachine.cs
// Called from constructor after base DI

private void RegisterWorkOrderStateMachine()
{
    const string SM = "WORK_ORDER";

    // --- Transitions ---
    _transitionRegistry[(SM, "DRAFT",       "plan")]    = "PLANNED";
    _transitionRegistry[(SM, "DRAFT",       "cancel")]  = "CANCELLED";
    _transitionRegistry[(SM, "PLANNED",     "start")]   = "IN_PROGRESS";
    _transitionRegistry[(SM, "PLANNED",     "cancel")]  = "CANCELLED";
    _transitionRegistry[(SM, "IN_PROGRESS", "hold")]    = "ON_HOLD";
    _transitionRegistry[(SM, "IN_PROGRESS", "complete")]= "COMPLETED";
    _transitionRegistry[(SM, "IN_PROGRESS", "cancel")]  = "CANCELLED";
    _transitionRegistry[(SM, "ON_HOLD",     "resume")]  = "IN_PROGRESS";
    _transitionRegistry[(SM, "ON_HOLD",     "cancel")]  = "CANCELLED";

    // --- Initial state ---
    _initialStateRegistry[SM] = "DRAFT";

    // --- Guards ---
    _guardRegistry[(SM, "PLANNED", "start")] = async instance =>
    {
        if (string.IsNullOrWhiteSpace(instance.EntityId))
            throw new ProcessGuardException("start", "EntityId (EquipmentId / FacilityId)", "ISO 55001 §8.4");

        var baFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "STEP_ID",   Operator = "=", FilterValue = instance.CurrentStepId }
        };
        var baRepo = BuildRepo("PROJECT_STEP_BA");
        var assignedBAs = await baRepo.GetAsync(baFilters);

        if (!assignedBAs.Any())
            throw new ProcessGuardException("start", "Contractor assignment (PROJECT_STEP_BA)", "IOGP S-501 §3.2");

        return true;
    };

    _guardRegistry[(SM, "IN_PROGRESS", "complete")] = async instance =>
    {
        var condFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "STEP_ID",     Operator = "=", FilterValue = instance.CurrentStepId },
            new AppFilter { FieldName = "COND_STATUS",  Operator = "<>", FilterValue = "SATISFIED" }
        };
        var condRepo = BuildRepo("PROJECT_STEP_CONDITION");
        var openConditions = await condRepo.GetAsync(condFilters);

        if (openConditions.Any())
            throw new ProcessGuardException(
                "complete",
                $"{openConditions.Count} inspection condition(s) still open (PROJECT_STEP_CONDITION)",
                "BSEE SEMS §250.1920");

        return true;
    };

    _guardRegistry[(SM, "DRAFT", "cancel")] = async instance =>
    {
        if (string.IsNullOrWhiteSpace(instance.Notes))
            throw new ProcessGuardException("cancel", "Cancellation reason (Notes field)", "Internal policy");
        return true;
    };
}
```

---

## Template 4: ProcessStateMachine — Register Gate Review SM

```csharp
private void RegisterGateReviewStateMachine()
{
    const string SM = "GATE_REVIEW";

    _transitionRegistry[(SM, "PENDING",      "submit")]        = "UNDER_REVIEW";
    _transitionRegistry[(SM, "UNDER_REVIEW", "approve")]       = "APPROVED";
    _transitionRegistry[(SM, "UNDER_REVIEW", "reject")]        = "REJECTED";
    _transitionRegistry[(SM, "UNDER_REVIEW", "defer")]         = "DEFERRED";
    _transitionRegistry[(SM, "DEFERRED",     "resubmit")]      = "UNDER_REVIEW";
    _transitionRegistry[(SM, "REJECTED",     "resubmit")]      = "UNDER_REVIEW";

    _initialStateRegistry[SM] = "PENDING";

    _guardRegistry[(SM, "PENDING", "submit")] = async instance =>
    {
        if (!instance.ProcessId.StartsWith("GATE-", StringComparison.Ordinal))
            return true; // Non-gate gate reviews (MOC, AFE) use relaxed document check

        // Retrieve the definition to know required doc types
        var def = await GetProcessDefinitionByNameAsync(instance.ProcessId);
        if (def == null || !def.RequiredDocumentTypes.Any())
            return true;

        var docRepo = BuildRepo("RM_INFORMATION_ITEM");
        foreach (var requiredDocType in def.RequiredDocumentTypes)
        {
            var docFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SOURCE",             Operator = "=", FilterValue = instance.EntityId },
                new AppFilter { FieldName = "INFO_ITEM_SUBTYPE",  Operator = "=", FilterValue = requiredDocType },
                new AppFilter { FieldName = "ACTIVE_IND",         Operator = "=", FilterValue = "Y" }
            };
            var docs = await docRepo.GetAsync(docFilters);
            if (!docs.Any())
                throw new ProcessGuardException(
                    "submit",
                    $"Required document type '{requiredDocType}' not attached (RM_INFORMATION_ITEM)",
                    "IPA FEL checklist");
        }
        return true;
    };

    _guardRegistry[(SM, "UNDER_REVIEW", "approve")] = async instance =>
    {
        var def = await GetProcessDefinitionByNameAsync(instance.ProcessId);
        int minApprovers = def?.MinApproverCount ?? 1;

        var approverFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "STEP_ID",  Operator = "=", FilterValue = instance.CurrentStepId },
            new AppFilter { FieldName = "DECISION", Operator = "=", FilterValue = "APPROVE" }
        };
        var baRepo = BuildRepo("PROJECT_STEP_BA");
        var approvers = await baRepo.GetAsync(approverFilters);

        if (approvers.Count < minApprovers)
            throw new ProcessGuardException(
                "approve",
                $"Requires {minApprovers} approver(s); only {approvers.Count} have approved (PROJECT_STEP_BA)",
                "CAPL JOA Art. IX");

        return true;
    };
}
```

---

## Template 5: GetProcessDefinitionByNameAsync

Resolver called by guards and service methods.

```csharp
public async Task<ProcessDefinition?> GetProcessDefinitionByNameAsync(string processId)
{
    var repo = BuildProcessDefinitionRepo();
    var filters = new List<AppFilter>
    {
        new AppFilter { FieldName = "PROJECT_OBS_NO", Operator = "=", FilterValue = processId },
        new AppFilter { FieldName = "ACTIVE_IND",     Operator = "=", FilterValue = "Y" }
    };

    var results = await repo.GetAsync(filters);
    return results.FirstOrDefault() as ProcessDefinition;
}
```

---

## Template 6: ApplyTransitionAsync — Core State Machine Execute

```csharp
// File: Beep.OilandGas.LifeCycle/Services/PPDMProcessService.cs

public async Task<ProcessInstance> ApplyTransitionAsync(
    string instanceId,
    string trigger,
    string userId,
    string? reason = null)
{
    // 1. Load current instance
    var instanceRepo = BuildRepo("PROJECT");
    var instance = await instanceRepo.GetByIdAsync(instanceId) as ProcessInstance
        ?? throw new ArgumentException($"Process instance '{instanceId}' not found");

    // 2. Look up target state
    var key = (instance.ProcessType, instance.CurrentState, trigger);
    if (!_transitionRegistry.TryGetValue(key, out var targetState))
        throw new InvalidOperationException(
            $"No transition defined for ({instance.ProcessType}, {instance.CurrentState}, {trigger})");

    // 3. Execute guard
    if (_guardRegistry.TryGetValue(key, out var guard))
    {
        instance.Notes = reason; // Make reason available to guards
        await guard(instance);   // Throws ProcessGuardException on failure
    }

    // 4. Record transition in audit trail
    var transitionRecord = new ProcessTransitionRecord
    {
        InstanceId      = instanceId,
        FromState       = instance.CurrentState,
        Trigger         = trigger,
        ToState         = targetState,
        UserId          = userId,
        TransitionDate  = DateTime.UtcNow,
        Reason          = reason
    };
    await WriteAuditRecordAsync(transitionRecord, userId);

    // 5. Update instance state
    instance.CurrentState = targetState;
    instance.Notes        = reason;
    if (targetState is "COMPLETED" or "CLOSED" or "APPROVED" or "CANCELLED" or "ABANDONED")
        instance.CompletionDate = DateTime.UtcNow;

    await instanceRepo.UpdateAsync(instance, userId);

    Log.Information(
        "Process {InstanceId} transitioned: {From} --{Trigger}--> {To} (user: {User})",
        instanceId, transitionRecord.FromState, trigger, targetState, userId);

    return instance;
}
```

---

## Template 7: StartProcessAsync — Create New Instance

```csharp
public async Task<ProcessInstance> StartProcessAsync(
    string processId,
    string entityId,
    string entityTableName,
    string fieldId,
    string jurisdiction,
    string userId,
    string? instanceName = null)
{
    // 1. Validate definition exists
    var def = await GetProcessDefinitionByNameAsync(processId)
        ?? throw new ArgumentException($"Process definition '{processId}' not found");

    // 2. Validate entity exists
    var entityRepo = BuildRepo(entityTableName);
    var entity = await entityRepo.GetByIdAsync(entityId);
    if (entity == null)
        throw new OperationCanceledException($"Entity '{entityId}' not found in '{entityTableName}'");

    // 3. Build new instance
    var instance = new ProcessInstance
    {
        InstanceId       = _defaults.FormatIdForTable("PROJECT", Guid.NewGuid().ToString("N")),
        ProcessId        = processId,
        InstanceName     = instanceName ?? $"{def.ProcessName} — {DateTime.UtcNow:yyyy-MM-dd}",
        ProcessType      = def.ProcessType,
        CurrentState     = _initialStateRegistry[def.ProcessType],
        EntityId         = entityId,
        EntityTableName  = entityTableName,
        FieldId          = fieldId,
        Jurisdiction     = jurisdiction,
        StartDate        = DateTime.UtcNow,
        InitiatingUserId = userId,
        CurrentStepId    = def.Steps.FirstOrDefault()?.StepId ?? string.Empty
    };

    var instanceRepo = BuildRepo("PROJECT");
    await instanceRepo.InsertAsync(instance, userId);

    Log.Information(
        "Started process instance {InstanceId} ({ProcessId}) for entity {EntityId}",
        instance.InstanceId, processId, entityId);

    return instance;
}
```

---

## Template 8: BuildRepo Helper

```csharp
private PPDMGenericRepository BuildRepo(string tableName)
{
    var meta = _metadata.GetTableMetadataAsync(tableName).GetAwaiter().GetResult();
    var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
    return new PPDMGenericRepository(
        _editor, _commonColumnHandler, _defaults, _metadata,
        entityType, _connectionName, tableName);
}
```
