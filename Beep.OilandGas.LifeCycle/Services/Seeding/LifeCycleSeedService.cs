using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.LifeCycle.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using ProcessDefinitionEntity = Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_DEFINITION;
using ProcessStepEntity = Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_STEP_INSTANCE;

namespace Beep.OilandGas.LifeCycle.Services.Seeding;

public class LifeCycleSeedService : ILifeCycleSeedService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<LifeCycleSeedService>? _logger;

    public LifeCycleSeedService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<LifeCycleSeedService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<LifeCycleSeedResult> SeedAllAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = new LifeCycleSeedResult { Success = true };

        try
        {
            await SeedLifecycleReferenceCodesAsync(connectionName, userId, result, cancellationToken);
            await SeedProcessDefinitionsAsync(connectionName, userId, result, cancellationToken);

            // Phase 2: Seed DoA thresholds
            await SeedDoAThresholdsAsync(connectionName, userId, result, cancellationToken);

            // Phase 3: Seed business event triggers
            await SeedBusinessEventTriggersAsync(connectionName, userId, result, cancellationToken);

            // Phase 4: Seed SoD rules
            await SeedSodRulesAsync(connectionName, userId, result, cancellationToken);

            result.Success = result.Errors.Count == 0;
            result.TablesSeeded = result.TotalRecordsInserted > 0 ? 5 : 0;

            _logger?.LogInformation(
                "Lifecycle seed completed. States={States}, Definitions={Defs}, Steps={Steps}, Total={Total}",
                result.LifecycleStatesInserted,
                result.ProcessDefinitionsInserted,
                result.ProcessStepsInserted,
                result.TotalRecordsInserted);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
            _logger?.LogError(ex, "Lifecycle seeding failed for connection {ConnectionName}", connectionName);
        }

        return result;
    }

    private async Task SeedLifecycleReferenceCodesAsync(
        string connectionName,
        string userId,
        LifeCycleSeedResult result,
        CancellationToken cancellationToken)
    {
        var repo = GetRepo<R_LIFECYCLE_STATE_REFERENCE>("R_LIFECYCLE_STATE_REFERENCE", connectionName);

        var codes = GetAllLifecycleReferenceCodes();

        foreach (var code in codes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existing = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "REFERENCE_SET", Operator = "=", FilterValue = code.ReferenceSet },
                new AppFilter { FieldName = "REFERENCE_CODE", Operator = "=", FilterValue = code.ReferenceCode }
            });

            var hasExisting = false;
            foreach (var _ in existing) { hasExisting = true; break; }

            if (hasExisting)
                continue;

            var entity = new R_LIFECYCLE_STATE_REFERENCE
            {
                REFERENCE_SET = code.ReferenceSet,
                REFERENCE_CODE = code.ReferenceCode,
                LONG_NAME = code.LongName,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            try
            {
                await repo.InsertAsync(entity, userId);
                result.LifecycleStatesInserted++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"[R_LIFECYCLE_STATE_REFERENCE/{code.ReferenceSet}/{code.ReferenceCode}] {ex.Message}");
            }
        }
    }

    private async Task SeedProcessDefinitionsAsync(
        string connectionName,
        string userId,
        LifeCycleSeedResult result,
        CancellationToken cancellationToken)
    {
        var defRepo = GetRepo<ProcessDefinitionEntity>("PROCESS_DEFINITION", connectionName);
        var stepRepo = GetRepo<ProcessStepEntity>("PROCESS_STEP_INSTANCE", connectionName);

        var definitions = GetAllProcessDefinitions();

        foreach (var def in definitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingDef = await defRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "PROCESS_NAME", Operator = "=", FilterValue = def.Name },
                new AppFilter { FieldName = "PROCESS_TYPE", Operator = "=", FilterValue = def.Type }
            });

            var hasExistingDef = false;
            foreach (var _ in existingDef) { hasExistingDef = true; break; }

            if (hasExistingDef)
                continue;

            var definition = new ProcessDefinitionEntity
            {
                PROCESS_DEFINITION_ID = Guid.NewGuid().ToString(),
                PROCESS_NAME = def.Name,
                PROCESS_TYPE = def.Type,
                DESCRIPTION = def.Description,
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            try
            {
                await defRepo.InsertAsync(definition, userId);
                result.ProcessDefinitionsInserted++;

                for (int i = 0; i < def.Steps.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var step = new ProcessStepEntity
                    {
                        PROCESS_STEP_INSTANCE_ID = Guid.NewGuid().ToString(),
                        PROCESS_INSTANCE_ID = string.Empty,
                        STEP_ID = $"STEP_{def.Name.ToUpper().Replace(" ", "_")}_{i + 1}",
                        STEP_SEQUENCE = i + 1,
                        STATUS = "NOT_STARTED",
                        ROW_CREATED_BY = userId,
                        ROW_CREATED_DATE = DateTime.UtcNow
                    };

                    await stepRepo.InsertAsync(step, userId);
                    result.ProcessStepsInserted++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"[PROCESS_DEFINITION/{def.Name}] {ex.Message}");
            }
        }
    }

    private static IEnumerable<(string ReferenceSet, string ReferenceCode, string LongName)> GetAllLifecycleReferenceCodes()
    {
        var codes = new List<(string, string, string)>();

        codes.Add(("WELL_LIFECYCLE_STATE", "PLANNED", "Well is planned but not yet drilled"));
        codes.Add(("WELL_LIFECYCLE_STATE", "DRILLING", "Well is currently being drilled"));
        codes.Add(("WELL_LIFECYCLE_STATE", "COMPLETING", "Well completion operations in progress"));
        codes.Add(("WELL_LIFECYCLE_STATE", "PRODUCING", "Well is actively producing"));
        codes.Add(("WELL_LIFECYCLE_STATE", "WORKOVER", "Well workover operations in progress"));
        codes.Add(("WELL_LIFECYCLE_STATE", "SUSPENDED", "Well is temporarily suspended"));
        codes.Add(("WELL_LIFECYCLE_STATE", "ABANDONED", "Well has been permanently abandoned"));

        codes.Add(("FIELD_LIFECYCLE_PHASE", "EXPLORATION", "Field is in exploration phase"));
        codes.Add(("FIELD_LIFECYCLE_PHASE", "APPRAISAL", "Field is being appraised"));
        codes.Add(("FIELD_LIFECYCLE_PHASE", "DEVELOPMENT", "Field is under development"));
        codes.Add(("FIELD_LIFECYCLE_PHASE", "PRODUCTION", "Field is actively producing"));
        codes.Add(("FIELD_LIFECYCLE_PHASE", "DECLINE", "Field production is in decline"));
        codes.Add(("FIELD_LIFECYCLE_PHASE", "DECOMMISSIONING", "Field decommissioning in progress"));
        codes.Add(("FIELD_LIFECYCLE_PHASE", "DECOMMISSIONED", "Field has been decommissioned"));

        codes.Add(("RESERVOIR_LIFECYCLE_STATE", "DISCOVERED", "Reservoir has been discovered"));
        codes.Add(("RESERVOIR_LIFECYCLE_STATE", "APPRAISED", "Reservoir has been appraised"));
        codes.Add(("RESERVOIR_LIFECYCLE_STATE", "DEVELOPED", "Reservoir development is complete"));
        codes.Add(("RESERVOIR_LIFECYCLE_STATE", "PRODUCING", "Reservoir is actively producing"));
        codes.Add(("RESERVOIR_LIFECYCLE_STATE", "DEPLETED", "Reservoir production is depleted"));
        codes.Add(("RESERVOIR_LIFECYCLE_STATE", "ABANDONED", "Reservoir has been abandoned"));

        codes.Add(("PROCESS_STATUS", "DRAFT", "Process definition is in draft"));
        codes.Add(("PROCESS_STATUS", "PENDING", "Process instance is pending start"));
        codes.Add(("PROCESS_STATUS", "IN_PROGRESS", "Process instance is actively running"));
        codes.Add(("PROCESS_STATUS", "ON_HOLD", "Process instance is on hold"));
        codes.Add(("PROCESS_STATUS", "COMPLETED", "Process instance has completed successfully"));
        codes.Add(("PROCESS_STATUS", "CANCELLED", "Process instance has been cancelled"));
        codes.Add(("PROCESS_STATUS", "REJECTED", "Process instance has been rejected"));

        codes.Add(("STEP_STATUS", "NOT_STARTED", "Step has not started"));
        codes.Add(("STEP_STATUS", "IN_PROGRESS", "Step is currently being executed"));
        codes.Add(("STEP_STATUS", "COMPLETED", "Step has completed successfully"));
        codes.Add(("STEP_STATUS", "SKIPPED", "Step has been skipped"));
        codes.Add(("STEP_STATUS", "FAILED", "Step execution failed"));
        codes.Add(("STEP_STATUS", "PENDING_APPROVAL", "Step is awaiting approval"));

        codes.Add(("APPROVAL_STATUS", "PENDING", "Approval is pending"));
        codes.Add(("APPROVAL_STATUS", "APPROVED", "Approval has been granted"));
        codes.Add(("APPROVAL_STATUS", "REJECTED", "Approval has been rejected"));
        codes.Add(("APPROVAL_STATUS", "ESCALATED", "Approval has been escalated"));
        codes.Add(("APPROVAL_STATUS", "DELEGATED", "Approval has been delegated"));

        codes.Add(("TRANSITION_CONDITION", "ALWAYS", "Transition always allowed"));
        codes.Add(("TRANSITION_CONDITION", "CONDITIONAL", "Transition requires condition evaluation"));
        codes.Add(("TRANSITION_CONDITION", "APPROVAL_REQUIRED", "Transition requires approval"));
        codes.Add(("TRANSITION_CONDITION", "TIME_BASED", "Transition triggered by time/SLA"));
        codes.Add(("TRANSITION_CONDITION", "EVENT_BASED", "Transition triggered by external event"));

        return codes;
    }

    private static IEnumerable<(string Name, string Type, string Description, string[] Steps)> GetAllProcessDefinitions()
    {
        var definitions = new List<(string, string, string, string[])>();

        definitions.Add(("Lead-to-Prospect", "EXPLORATION", "Convert exploration lead to defined prospect",
            new[] { "Lead Capture", "Geological Review", "Geophysical Analysis", "Prospect Definition", "Screening", "Approval" }));

        definitions.Add(("Prospect-to-Discovery", "EXPLORATION", "Evaluate prospect and declare discovery",
            new[] { "Prospect Ranking", "Well Planning", "Drilling", "Evaluation", "Discovery Declaration" }));

        definitions.Add(("Discovery-to-Development", "DEVELOPMENT", "Appraise discovery and prepare for development",
            new[] { "Appraisal Planning", "Appraisal Drilling", "Reservoir Characterization", "FDP", "Sanction" }));

        definitions.Add(("Well Development", "DEVELOPMENT", "Design and execute well development",
            new[] { "Well Design", "Drilling Program", "Completion Design", "Execution", "Commissioning" }));

        definitions.Add(("Facility Development", "DEVELOPMENT", "Design and construct production facilities",
            new[] { "FEED", "Detailed Design", "Procurement", "Construction", "Commissioning" }));

        definitions.Add(("Pipeline Development", "DEVELOPMENT", "Design and construct pipeline infrastructure",
            new[] { "Route Selection", "Design", "Permitting", "Construction", "Commissioning" }));

        definitions.Add(("Well Startup", "PRODUCTION", "Bring well online and establish production",
            new[] { "Pre-Startup Check", "Well Testing", "Production Allocation", "Monitoring" }));

        definitions.Add(("Production Operations", "PRODUCTION", "Daily production management and optimization",
            new[] { "Daily Monitoring", "Optimization", "Performance Review", "Reporting" }));

        definitions.Add(("Decline Management", "PRODUCTION", "Manage production decline and optimization",
            new[] { "Decline Analysis", "Forecast Update", "Optimization Study", "Implementation" }));

        definitions.Add(("Well Abandonment", "DECOMMISSIONING", "Permanently abandon well",
            new[] { "Planning", "Regulatory Approval", "Plugging", "Verification", "Certification" }));

        definitions.Add(("Facility Decommissioning", "DECOMMISSIONING", "Decommission and remove facilities",
            new[] { "Planning", "Permitting", "Removal", "Site Restoration", "Certification" }));

        definitions.Add(("HSE Incident Management", "HSE", "Manage HSE incidents from report to closure",
            new[] { "Incident Report", "Investigation", "RCA", "Corrective Action", "Closure" }));

        definitions.Add(("Work Order Management", "WORKORDER", "Manage work orders from request to closure",
            new[] { "Request", "Approval", "Scheduling", "Execution", "Verification", "Closure" }));

        definitions.Add(("Maintenance Planning", "MAINTENANCE", "Plan and execute maintenance activities",
            new[] { "Inspection", "Condition Assessment", "Work Order Creation", "Execution", "Verification" }));

        definitions.Add(("Inspection Management", "INSPECTION", "Manage inspection programs",
            new[] { "Schedule", "Execute", "Report", "Findings", "Recommendations", "Follow-up" }));

        definitions.Add(("Permit Management", "PERMIT", "Manage permit applications and compliance",
            new[] { "Application", "Review", "Approval", "Issuance", "Compliance", "Closure" }));

        return definitions;
    }

    private PPDMGenericRepository GetRepo<T>(string tableName, string connectionName) =>
        new PPDMGenericRepository(
            _editor,
            _commonColumnHandler,
            _defaults,
            _metadata,
            typeof(T),
            connectionName,
            tableName);

    // ══════════════════════════════════════════════════════════════════════════
    // Phase 2: Seed Delegation of Authority Thresholds
    // ══════════════════════════════════════════════════════════════════════════

    private async Task SeedDoAThresholdsAsync(
        string connectionName, string userId, LifeCycleSeedResult result, CancellationToken ct)
    {
        var repo = GetRepo<DELEGATION_OF_AUTHORITY>("DELEGATION_OF_AUTHORITY", connectionName);
        var existing = (await repo.GetAsync(new List<AppFilter>()))
            .OfType<DELEGATION_OF_AUTHORITY>()
            .Select(d => d.DOA_NAME + "|" + d.APPROVAL_LEVEL)
            .ToHashSet();

        var thresholds = new (string name, string entity, string field, string op, decimal val, decimal? max, string level, string role, int seq, string processType)[]
        {
            // AFE Approval Limits (5 levels)
            ("AFE Standard Limits", "AFE", "ESTIMATED_COST", "GREATER_THAN", 0, null, "LEVEL_1", "Supervisor", 1, "AFE_APPROVAL"),
            ("AFE Standard Limits", "AFE", "ESTIMATED_COST", "GREATER_THAN", 50000, null, "LEVEL_2", "Manager", 2, "AFE_APPROVAL"),
            ("AFE Standard Limits", "AFE", "ESTIMATED_COST", "GREATER_THAN", 500000, null, "LEVEL_3", "SeniorManager", 3, "AFE_APPROVAL"),
            ("AFE Standard Limits", "AFE", "ESTIMATED_COST", "GREATER_THAN", 5000000, null, "LEVEL_4", "Executive", 4, "AFE_APPROVAL"),
            ("AFE Standard Limits", "AFE", "ESTIMATED_COST", "GREATER_THAN", 50000000, null, "LEVEL_5", "Board", 5, "AFE_APPROVAL"),
            // Cost Transaction Limits
            ("Cost Transaction Limits", "COST_TRANSACTION", "AMOUNT", "GREATER_THAN", 0, null, "LEVEL_1", "Supervisor", 1, "COST_APPROVAL"),
            ("Cost Transaction Limits", "COST_TRANSACTION", "AMOUNT", "GREATER_THAN", 25000, null, "LEVEL_2", "Manager", 2, "COST_APPROVAL"),
            ("Cost Transaction Limits", "COST_TRANSACTION", "AMOUNT", "GREATER_THAN", 250000, null, "LEVEL_3", "SeniorManager", 3, "COST_APPROVAL"),
            // Revenue Recognition Limits
            ("Revenue Recognition Limits", "REVENUE_TRANSACTION", "GROSS_REVENUE", "GREATER_THAN", 0, null, "LEVEL_1", "Accountant", 1, "REVENUE_APPROVAL"),
            ("Revenue Recognition Limits", "REVENUE_TRANSACTION", "GROSS_REVENUE", "GREATER_THAN", 100000, null, "LEVEL_2", "Manager", 2, "REVENUE_APPROVAL"),
            ("Revenue Recognition Limits", "REVENUE_TRANSACTION", "GROSS_REVENUE", "GREATER_THAN", 1000000, null, "LEVEL_3", "Executive", 3, "REVENUE_APPROVAL"),
            // Journal Entry Limits
            ("Journal Entry Limits", "JOURNAL_ENTRY", "TOTAL_DEBIT", "GREATER_THAN", 0, null, "LEVEL_1", "Accountant", 1, "JOURNAL_APPROVAL"),
            ("Journal Entry Limits", "JOURNAL_ENTRY", "TOTAL_DEBIT", "GREATER_THAN", 50000, null, "LEVEL_2", "Manager", 2, "JOURNAL_APPROVAL"),
            ("Journal Entry Limits", "JOURNAL_ENTRY", "TOTAL_DEBIT", "GREATER_THAN", 500000, null, "LEVEL_3", "Executive", 3, "JOURNAL_APPROVAL"),
        };

        foreach (var (name, entity, field, op, val, max, level, role, seq, processType) in thresholds)
        {
            ct.ThrowIfCancellationRequested();
            var key = $"{name}|{level}";
            if (existing.Contains(key)) continue;

            var doa = new DELEGATION_OF_AUTHORITY
            {
                DOA_NAME = name, ENTITY_TYPE = entity, FIELD_NAME = field,
                COMPARISON_OPERATOR = op, THRESHOLD_VALUE = val, THRESHOLD_VALUE_MAX = max,
                APPROVAL_LEVEL = level, REQUIRED_ROLE = role, APPROVAL_SEQUENCE = seq,
                PROCESS_TYPE = processType, CURRENCY_CODE = "USD",
                ESCALATION_HOURS = level == "LEVEL_1" ? 72 : 48,
                ESCALATION_ROLE = seq switch { 1 => "Manager", 2 => "SeniorManager", 3 => "Executive", _ => "Board" },
            };
            await repo.InsertAsync(doa, userId);
            result.DelegationRulesInserted++;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Phase 3: Seed Business Event Triggers
    // ══════════════════════════════════════════════════════════════════════════

    private async Task SeedBusinessEventTriggersAsync(
        string connectionName, string userId, LifeCycleSeedResult result, CancellationToken ct)
    {
        var repo = GetRepo<BUSINESS_EVENT_TRIGGER>("BUSINESS_EVENT_TRIGGER", connectionName);
        var existing = (await repo.GetAsync(new List<AppFilter>()))
            .OfType<BUSINESS_EVENT_TRIGGER>()
            .Select(t => t.TRIGGER_NAME)
            .ToHashSet();

        var triggers = new (string name, string eventType, string entityType, string? condition, string targetProcess)[]
        {
            ("Production Posted → Revenue Recognition", "STATUS_CHANGED", "PDEN_VOL_SUMMARY", "NewStatus == 'POSTED'", "CRW_PRODUCTION_TO_REVENUE"),
            ("AFE Approved → Cost Tracking", "STATUS_CHANGED", "AFE", "NewStatus == 'APPROVED'", "CRW_AFE_COST_TRACKING"),
            ("Revenue Posted → Royalty Calculation", "STATUS_CHANGED", "REVENUE_TRANSACTION", "NewStatus == 'POSTED'", "CRW_ROYALTY_CALCULATION"),
            ("Incident Reported → Investigation", "ENTITY_CREATED", "HSE_INCIDENT", null, "HSE_INCIDENT_REPORTING"),
            ("Well Completed → Production Handoff", "STATUS_CHANGED", "WELL", "NewStatus == 'COMPLETED'", "CRW_WELL_HANDOFF_DRILLING_PROD"),
            ("Production Declining → Workover Proposal", "STATUS_CHANGED", "WELL", "NewStatus == 'DECLINING'", "CRW_WORKOVER_PROPOSAL"),
            ("Period Close → Journal Entry Review", "STATUS_CHANGED", "JOURNAL_ENTRY", "NewStatus == 'DRAFT'", "CRW_PERIOD_CLOSE"),
            ("Reserves Estimate → Executive Review", "STATUS_CHANGED", "RESERVES_ESTIMATE", "NewStatus == 'DRAFT'", "CRW_RESERVES_REVISION"),
        };

        foreach (var (name, eventType, entityType, condition, targetProcess) in triggers)
        {
            ct.ThrowIfCancellationRequested();
            if (existing.Contains(name)) continue;

            var trigger = new BUSINESS_EVENT_TRIGGER
            {
                TRIGGER_NAME = name, EVENT_TYPE = eventType, ENTITY_TYPE = entityType,
                CONDITION_EXPRESSION = condition, TARGET_PROCESS_DEF_ID = targetProcess,
                IS_ACTIVE = "Y", PRIORITY = 10,
            };
            await repo.InsertAsync(trigger, userId);
            result.BusinessEventTriggersInserted++;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Phase 4: Seed SoD Rules (delegates to SodEvaluationEngine)
    // ══════════════════════════════════════════════════════════════════════════

    private async Task SeedSodRulesAsync(
        string connectionName, string userId, LifeCycleSeedResult result, CancellationToken ct)
    {
        try
        {
            var sodEngine = new Processes.SodEvaluationEngine(
                _editor, _commonColumnHandler, _defaults, _metadata, connectionName,
                _logger as ILogger<Processes.SodEvaluationEngine>);

            result.SodRulesInserted += await sodEngine.SeedDefaultRulesWithCountAsync(userId);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "SoD rule seeding skipped (may already exist): {Message}", ex.Message);
        }
    }

}
