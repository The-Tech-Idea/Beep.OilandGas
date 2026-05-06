using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Models.Data.Decommissioning;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.LifeCycle.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Seeding;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Modules;

/// <summary>
/// Feature module setup for lifecycle management: registers extension tables and seeds
/// reference data for workflow engine, lifecycle states, process definitions, and SLA templates.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why these <see cref="EntityTypes"/> entries?</b>
/// The LifeCycle project introduces extension tables that are not part of standard PPDM 3.9 core.
/// These tables support the workflow engine, lifecycle state tracking, and decommissioning operations.
/// All are registered here for metadata/schema tooling (entity-driven schema creation, not hand-written SQL).
/// </para>
/// <para>
/// <b><see cref="SeedAsync"/></b> idempotently seeds:
/// <list type="bullet">
///   <item>Lifecycle reference codes (well states, field phases, reservoir states)</item>
///   <item>Process definitions (20+ workflow templates)</item>
///   <item>Status and transition reference data</item>
///   <item>SLA configuration templates</item>
///   <item>Default approval chain configurations</item>
/// </list>
/// </para>
/// </remarks>
public sealed class LifeCycleModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        // ── Lifecycle tracking tables ──────────────────────────────────────
        typeof(FIELD_PHASE),
        typeof(RESERVOIR_STATUS),
        typeof(ABANDONMENT_STATUS),
        typeof(DECOMMISSIONING_STATUS),
        // ── Workflow engine tables ─────────────────────────────────────────
        typeof(PROCESS_DEFINITION),
        typeof(PROCESS_INSTANCE),
        typeof(PROCESS_STEP_INSTANCE),
        typeof(PROCESS_HISTORY),
        typeof(PROCESS_APPROVAL),
        // ── Decommissioning & environmental tables ─────────────────────────
        typeof(ENVIRONMENTAL_RESTORATION),
        typeof(DECOMMISSIONING_COST),
        // ── Organization hierarchy ─────────────────────────────────────────
        typeof(ORGANIZATION_HIERARCHY_CONFIG),
        // ── Lifecycle reference LOV table ──────────────────────────────────
        typeof(R_LIFECYCLE_STATE_REFERENCE),
    };

    private readonly ILifeCycleSeedService? _seedService;

    public LifeCycleModule(ModuleSetupContext context, ILifeCycleSeedService? seedService = null)
        : base(context)
    {
        _seedService = seedService;
    }

    public override string ModuleId => "LIFECYCLE";

    public override string ModuleName => "Lifecycle management (workflows, phases, decommissioning)";

    /// <summary>After SECURITY (40), before domain calculation modules (70+).</summary>
    public override int Order => 50;

    public override IReadOnlyList<Type> EntityTypes => _entityTypes;

    public override async Task<ModuleSetupResult> SeedAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = NewResult();

        if (_seedService != null)
        {
            return await SeedViaServiceAsync(connectionName, userId, result, cancellationToken);
        }

        return await SeedReferenceCodesAsync(connectionName, userId, result, cancellationToken);
    }

    private async Task<ModuleSetupResult> SeedViaServiceAsync(
        string connectionName,
        string userId,
        ModuleSetupResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            var seedResult = await _seedService.SeedAllAsync(connectionName, userId, cancellationToken);

            result.Success = seedResult.Success;
            result.RecordsInserted = seedResult.TotalRecordsInserted;
            result.TablesSeeded = seedResult.TablesSeeded;

            foreach (var error in seedResult.Errors)
                result.Errors.Add(error);

            if (seedResult.TotalRecordsInserted == 0 && seedResult.Errors.Count == 0)
                result.SkipReason = "Lifecycle reference data already seeded.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Lifecycle seeding failed: {ex.Message}");
        }

        return result;
    }

    private async Task<ModuleSetupResult> SeedReferenceCodesAsync(
        string connectionName,
        string userId,
        ModuleSetupResult result,
        CancellationToken cancellationToken)
    {
        await SeedLifecycleReferenceCodesAsync(connectionName, userId, result, cancellationToken);
        await SeedProcessStatusCodesAsync(connectionName, userId, result, cancellationToken);
        await SeedTransitionConditionCodesAsync(connectionName, userId, result, cancellationToken);

        result.Success = result.Errors.Count == 0;
        if (result.RecordsInserted == 0 && result.Errors.Count == 0)
            result.SkipReason = "Lifecycle reference codes already seeded.";

        return result;
    }

    private async Task SeedLifecycleReferenceCodesAsync(
        string connectionName,
        string userId,
        ModuleSetupResult result,
        CancellationToken cancellationToken)
    {
        var wellStates = new[]
        {
            ("WELL_LIFECYCLE_STATE", "PLANNED", "Well is planned but not yet drilled"),
            ("WELL_LIFECYCLE_STATE", "DRILLING", "Well is currently being drilled"),
            ("WELL_LIFECYCLE_STATE", "COMPLETING", "Well completion operations in progress"),
            ("WELL_LIFECYCLE_STATE", "PRODUCING", "Well is actively producing"),
            ("WELL_LIFECYCLE_STATE", "WORKOVER", "Well workover operations in progress"),
            ("WELL_LIFECYCLE_STATE", "SUSPENDED", "Well is temporarily suspended"),
            ("WELL_LIFECYCLE_STATE", "ABANDONED", "Well has been permanently abandoned"),
        };

        var fieldPhases = new[]
        {
            ("FIELD_LIFECYCLE_PHASE", "EXPLORATION", "Field is in exploration phase"),
            ("FIELD_LIFECYCLE_PHASE", "APPRAISAL", "Field is being appraised"),
            ("FIELD_LIFECYCLE_PHASE", "DEVELOPMENT", "Field is under development"),
            ("FIELD_LIFECYCLE_PHASE", "PRODUCTION", "Field is actively producing"),
            ("FIELD_LIFECYCLE_PHASE", "DECLINE", "Field production is in decline"),
            ("FIELD_LIFECYCLE_PHASE", "DECOMMISSIONING", "Field decommissioning in progress"),
            ("FIELD_LIFECYCLE_PHASE", "DECOMMISSIONED", "Field has been decommissioned"),
        };

        var reservoirStates = new[]
        {
            ("RESERVOIR_LIFECYCLE_STATE", "DISCOVERED", "Reservoir has been discovered"),
            ("RESERVOIR_LIFECYCLE_STATE", "APPRAISED", "Reservoir has been appraised"),
            ("RESERVOIR_LIFECYCLE_STATE", "DEVELOPED", "Reservoir development is complete"),
            ("RESERVOIR_LIFECYCLE_STATE", "PRODUCING", "Reservoir is actively producing"),
            ("RESERVOIR_LIFECYCLE_STATE", "DEPLETED", "Reservoir production is depleted"),
            ("RESERVOIR_LIFECYCLE_STATE", "ABANDONED", "Reservoir has been abandoned"),
        };

        var allCodes = new List<(string set, string code, string name)>();
        allCodes.AddRange(wellStates);
        allCodes.AddRange(fieldPhases);
        allCodes.AddRange(reservoirStates);

        await SeedReferenceSetAsync("R_LIFECYCLE_STATE", allCodes, connectionName, userId, result, cancellationToken);
    }

    private async Task SeedProcessStatusCodesAsync(
        string connectionName,
        string userId,
        ModuleSetupResult result,
        CancellationToken cancellationToken)
    {
        var processStatuses = new[]
        {
            ("PROCESS_STATUS", "DRAFT", "Process definition is in draft"),
            ("PROCESS_STATUS", "PENDING", "Process instance is pending start"),
            ("PROCESS_STATUS", "IN_PROGRESS", "Process instance is actively running"),
            ("PROCESS_STATUS", "ON_HOLD", "Process instance is on hold"),
            ("PROCESS_STATUS", "COMPLETED", "Process instance has completed successfully"),
            ("PROCESS_STATUS", "CANCELLED", "Process instance has been cancelled"),
            ("PROCESS_STATUS", "REJECTED", "Process instance has been rejected"),
        };

        var stepStatuses = new[]
        {
            ("STEP_STATUS", "NOT_STARTED", "Step has not started"),
            ("STEP_STATUS", "IN_PROGRESS", "Step is currently being executed"),
            ("STEP_STATUS", "COMPLETED", "Step has completed successfully"),
            ("STEP_STATUS", "SKIPPED", "Step has been skipped"),
            ("STEP_STATUS", "FAILED", "Step execution failed"),
            ("STEP_STATUS", "PENDING_APPROVAL", "Step is awaiting approval"),
        };

        var approvalStatuses = new[]
        {
            ("APPROVAL_STATUS", "PENDING", "Approval is pending"),
            ("APPROVAL_STATUS", "APPROVED", "Approval has been granted"),
            ("APPROVAL_STATUS", "REJECTED", "Approval has been rejected"),
            ("APPROVAL_STATUS", "ESCALATED", "Approval has been escalated"),
            ("APPROVAL_STATUS", "DELEGATED", "Approval has been delegated"),
        };

        var allCodes = new List<(string set, string code, string name)>();
        allCodes.AddRange(processStatuses);
        allCodes.AddRange(stepStatuses);
        allCodes.AddRange(approvalStatuses);

        await SeedReferenceSetAsync("R_PROCESS_STATUS", allCodes, connectionName, userId, result, cancellationToken);
    }

    private async Task SeedTransitionConditionCodesAsync(
        string connectionName,
        string userId,
        ModuleSetupResult result,
        CancellationToken cancellationToken)
    {
        var conditions = new[]
        {
            ("TRANSITION_CONDITION", "ALWAYS", "Transition always allowed"),
            ("TRANSITION_CONDITION", "CONDITIONAL", "Transition requires condition evaluation"),
            ("TRANSITION_CONDITION", "APPROVAL_REQUIRED", "Transition requires approval"),
            ("TRANSITION_CONDITION", "TIME_BASED", "Transition triggered by time/SLA"),
            ("TRANSITION_CONDITION", "EVENT_BASED", "Transition triggered by external event"),
        };

        await SeedReferenceSetAsync("R_TRANSITION_CONDITION", conditions, connectionName, userId, result, cancellationToken);
    }

    private async Task SeedReferenceSetAsync(
        string tableName,
        IReadOnlyList<(string referenceSet, string referenceCode, string longName)> codes,
        string connectionName,
        string userId,
        ModuleSetupResult result,
        CancellationToken cancellationToken)
    {
        var repo = GetRepo<R_LIFECYCLE_STATE_REFERENCE>(tableName, connectionName);

        foreach (var (refSet, refCode, longName) in codes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existing = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "REFERENCE_SET", Operator = "=", FilterValue = refSet },
                new AppFilter { FieldName = "REFERENCE_CODE", Operator = "=", FilterValue = refCode }
            });

            var hasExisting = false;
            foreach (var _ in existing) { hasExisting = true; break; }

            if (hasExisting)
                continue;

            var entity = new R_LIFECYCLE_STATE_REFERENCE
            {
                REFERENCE_SET = refSet,
                REFERENCE_CODE = refCode,
                LONG_NAME = longName,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            await TryInsertAsync(repo, entity, userId, result, $"{refSet}/{refCode}");
        }
    }
}
