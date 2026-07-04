using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Evaluates user permissions against Segregation of Duties rules.
/// Detects conflicts and determines whether they are blocking (must prevent)
/// or advisory (warning only).
/// Part of Phase 4 governance & compliance.
/// </summary>
public interface ISodEvaluationEngine
{
    /// <summary>
    /// Evaluate a set of permission codes against all active SoD rules.
    /// Returns any conflicts found.
    /// </summary>
    Task<List<SodConflictResult>> EvaluatePermissionsAsync(
        List<string> permissionCodes, string? scopeContext = null);

    /// <summary>
    /// Check if assigning two specific roles together creates an SoD conflict.
    /// </summary>
    Task<SodCheckResult> CheckRoleCombinationAsync(
        string roleA, string roleB, string? scopeContext = null);

    /// <summary>
    /// Get all active SoD rules.
    /// </summary>
    Task<List<SOD_RULE>> GetAllRulesAsync(string? category = null);

    /// <summary>
    /// Seed the 25 default SoD rules.
    /// </summary>
    Task SeedDefaultRulesAsync(string userId);
}

public class SodConflictResult
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string PermissionA { get; set; } = string.Empty;
    public string PermissionB { get; set; } = string.Empty;
    public string Severity { get; set; } = "HIGH";
    public bool IsBlocking { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public string? RegulationReference { get; set; }
    public string? MitigationGuidance { get; set; }
}

public class SodCheckResult
{
    public bool HasConflict { get; set; }
    public bool HasBlockingConflict { get; set; }
    public List<SodConflictResult> Conflicts { get; set; } = new();
    public List<SodConflictResult> Warnings { get; set; } = new();
    public bool CanProceed => !HasBlockingConflict;
    public List<string> RequiredMitigations { get; set; } = new();
}

public class SodEvaluationEngine : ISodEvaluationEngine
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<SodEvaluationEngine> _logger;

    public SodEvaluationEngine(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<SodEvaluationEngine>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<List<SodConflictResult>> EvaluatePermissionsAsync(
        List<string> permissionCodes, string? scopeContext = null)
    {
        var rules = await GetAllRulesAsync();
        var conflicts = new List<SodConflictResult>();
        var permSet = new HashSet<string>(permissionCodes, StringComparer.OrdinalIgnoreCase);

        foreach (var rule in rules)
        {
            var hasA = permSet.Contains(rule.CONFLICTING_PERMISSION_A);
            var hasB = permSet.Contains(rule.CONFLICTING_PERMISSION_B);

            if (hasA && hasB)
            {
                conflicts.Add(new SodConflictResult
                {
                    RuleId = rule.SOD_RULE_ID,
                    RuleName = rule.RULE_NAME,
                    PermissionA = rule.CONFLICTING_PERMISSION_A,
                    PermissionB = rule.CONFLICTING_PERMISSION_B,
                    Severity = rule.SEVERITY,
                    IsBlocking = string.Equals(rule.IS_BLOCKING, "Y", StringComparison.OrdinalIgnoreCase),
                    Description = rule.CONFLICT_DESCRIPTION,
                    RegulationReference = rule.REGULATION_REFERENCE,
                    MitigationGuidance = rule.MITIGATION_GUIDANCE,
                });
            }
        }

        return conflicts;
    }

    public async Task<SodCheckResult> CheckRoleCombinationAsync(
        string roleA, string roleB, string? scopeContext = null)
    {
        var result = new SodCheckResult();

        // Resolve permissions from both roles
        var permA = await GetPermissionsForRoleAsync(roleA);
        var permB = await GetPermissionsForRoleAsync(roleB);
        var allPerms = new HashSet<string>(permA, StringComparer.OrdinalIgnoreCase);
        foreach (var p in permB) allPerms.Add(p);

        var conflicts = await EvaluatePermissionsAsync(allPerms.ToList(), scopeContext);

        foreach (var conflict in conflicts)
        {
            // Only flag if the conflicting permissions come from different roles
            var aInA = permA.Contains(conflict.PermissionA, StringComparer.OrdinalIgnoreCase);
            var bInA = permA.Contains(conflict.PermissionB, StringComparer.OrdinalIgnoreCase);
            var aInB = permB.Contains(conflict.PermissionA, StringComparer.OrdinalIgnoreCase);
            var bInB = permB.Contains(conflict.PermissionB, StringComparer.OrdinalIgnoreCase);

            var permissionASource = aInA ? roleA : roleB;
            var permissionBSource = bInA ? roleA : roleB;

            if ((aInA && bInB) || (aInB && bInA))
            {
                if (conflict.IsBlocking)
                {
                    result.Conflicts.Add(conflict);
                    result.HasBlockingConflict = true;
                }
                else
                {
                    result.Warnings.Add(conflict);
                }
            }
        }

        result.HasConflict = result.Conflicts.Count > 0 || result.Warnings.Count > 0;
        return result;
    }

    public async Task<List<SOD_RULE>> GetAllRulesAsync(string? category = null)
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "ACTIVE_IND", FilterValue = "Y" },
        };

        if (!string.IsNullOrWhiteSpace(category))
        {
            filters.Add(new AppFilter { FieldName = "RULE_CATEGORY", FilterValue = category });
        }

        var results = await repo.GetAsync(filters);
        return results.OfType<SOD_RULE>().ToList();
    }

    public async Task SeedDefaultRulesAsync(string userId)
    {
        var repo = GetRepo();
        var existing = (await repo.GetAsync(new List<AppFilter>()))
            .OfType<SOD_RULE>()
            .ToDictionary(r => r.RULE_NAME, StringComparer.OrdinalIgnoreCase);

        var rules = new (string name, string cat, string permA, string permB, string severity, string reg, string desc)[]
        {
            ("AFE_CREATE_APPROVE", "FINANCIAL", "WellManagement.Create", "WellManagement.Approve", "CRITICAL", "SOX 404", "Cannot create and approve the same AFE"),
            ("AFE_COMMIT_SPEND", "FINANCIAL", "WellManagement.Approve", "Accounting.PostJournal", "CRITICAL", "SOX 404", "Cannot approve AFE and post resulting journal entries"),
            ("PRODUCTION_RECORD_RECONCILE", "FINANCIAL", "Production.SubmitProduction", "Accounting.Reconcile", "CRITICAL", "SOX 404", "Cannot record production volumes and reconcile revenue"),
            ("REVENUE_POST_APPROVE", "FINANCIAL", "Accounting.PostJournal", "Accounting.ApproveJournal", "CRITICAL", "SOX 404", "Cannot post and approve the same journal entry"),
            ("JOURNAL_CREATE_APPROVE", "FINANCIAL", "Accounting.PostJournal", "Accounting.ApproveJournal", "CRITICAL", "SOX 404", "Cannot create and approve journal entries"),
            ("ROYALTY_CALCULATE_DISBURSE", "FINANCIAL", "ProductionAccounting.Allocate", "Accounting.PostJournal", "HIGH", "SOX 404", "Cannot calculate royalties and disburse payments"),
            ("INCIDENT_REPORT_CLOSE", "SAFETY", "HSE.ReportIncident", "HSE.ManageIncidents", "HIGH", "ISO 45001", "Cannot report and close the same incident"),
            ("PERMIT_ISSUE_APPROVE", "SAFETY", "HSE.IssuePermit", "HSE.ApprovePermit", "HIGH", "OSHA", "Cannot issue and approve the same permit to work"),
            ("RESERVES_ESTIMATE_APPROVE", "FINANCIAL", "Reservoir.UpdateReserves", "Reservoir.Approve", "CRITICAL", "SEC", "Cannot estimate and approve the same reserves revision"),
            ("ACCESS_GRANT_REVIEW", "SECURITY", "Security.ManagePermissions", "Admin.ViewAuditLogs", "CRITICAL", "SOX 404", "Cannot grant access and review access logs"),
            ("ROLE_ASSIGN_APPROVE", "SECURITY", "Admin.AssignRoles", "Admin.ManageUsers", "CRITICAL", "SOX 404", "Cannot assign roles and manage user accounts"),
            ("CONFIG_CHANGE_APPROVE", "SECURITY", "Admin.ConfigureSystem", "Admin.ViewAuditLogs", "HIGH", "SOX 404", "Cannot change system configuration and review audit logs"),
            ("USER_CREATE_APPROVE", "SECURITY", "Admin.ManageUsers", "Admin.AssignRoles", "CRITICAL", "SOX 404", "Cannot create users and assign their roles"),
            ("AUDIT_LOG_VIEW_MODIFY", "SECURITY", "Admin.ViewAuditLogs", "Admin.ConfigureSystem", "CRITICAL", "ISO 27001", "Cannot view and modify audit logs"),
            ("DATA_IMPORT_APPROVE", "OPERATIONAL", "DataManagement.ImportData", "DataManagement.ApproveData", "HIGH", "GDPR", "Cannot import and approve the same data batch"),
            ("TAX_CALCULATE_FILE", "FINANCIAL", "Tax.Calculate", "Regulatory.Submit", "HIGH", "SOX 404", "Cannot calculate tax and submit regulatory filings"),
            ("DECOMMISSIONING_PLAN_APPROVE", "OPERATIONAL", "Decommissioning.PlanAbandonment", "Decommissioning.Approve", "HIGH", "Regulatory", "Cannot plan and approve decommissioning"),
            ("PRODUCTION_ALLOCATE_ADJUST", "OPERATIONAL", "ProductionAccounting.Allocate", "ProductionAccounting.Adjust", "HIGH", "SOX 404", "Cannot allocate and adjust production"),
            ("COST_CLASSIFY_POST", "FINANCIAL", "Accounting.PostJournal", "Accounting.ManagePeriods", "MEDIUM", "SOX 404", "Cannot post costs and manage accounting periods"),
            ("PURCHASE_ORDER_APPROVE", "FINANCIAL", "Facilities.ManageEquipment", "WellManagement.Approve", "MEDIUM", "SOX 404", "Cannot order equipment and approve expenditure"),
            ("CONTRACT_CREATE_APPROVE", "FINANCIAL", "LeaseAcquisition.Create", "LeaseAcquisition.Approve", "HIGH", "SOX 404", "Cannot create and approve contracts"),
            ("EMISSION_REPORT_VERIFY", "OPERATIONAL", "Environmental.ReportEmissions", "Environmental.ViewCompliance", "MEDIUM", "EPA", "Cannot report and verify emissions data"),
            ("WELL_STATUS_UPDATE_APPROVE", "OPERATIONAL", "WellManagement.UpdateWellStatus", "WellManagement.Approve", "MEDIUM", null, "Cannot update and approve well status changes"),
            ("RISK_ASSESS_APPROVE", "SAFETY", "HSE.CreateRiskAssessment", "HSE.ApprovePermit", "MEDIUM", "ISO 31000", "Cannot assess risk and approve permits for the same activity"),
            ("SAFETY_DRILL_EVALUATE", "SAFETY", "HSE.CreateRiskAssessment", "HSE.ConductAudit", "MEDIUM", "ISO 45001", "Cannot plan and evaluate the same safety drill"),
        };

        foreach (var (name, cat, permA, permB, severity, reg, desc) in rules)
        {
            if (existing.ContainsKey(name)) continue;

            var rule = new SOD_RULE
            {
                RULE_NAME = name,
                RULE_CATEGORY = cat,
                CONFLICTING_PERMISSION_A = permA,
                CONFLICTING_PERMISSION_B = permB,
                SEVERITY = severity,
                REGULATION_REFERENCE = reg,
                CONFLICT_DESCRIPTION = desc,
                IS_BLOCKING = severity is "CRITICAL" or "HIGH" ? "Y" : "N",
                SCOPE_TYPE = "GLOBAL",
            };

            await repo.InsertAsync(rule, userId);
            _logger?.LogInformation("Seeded SoD rule: {RuleName}", name);
        }
    }

    private async Task<HashSet<string>> GetPermissionsForRoleAsync(string roleName)
    {
        var permRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(Beep.OilandGas.Models.Data.Security.PERMISSION),
            _connectionName, "PERMISSION", null);

        var rpRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(Beep.OilandGas.Models.Data.Security.ROLE_PERMISSION),
            _connectionName, "ROLE_PERMISSION", null);

        var roleRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(Beep.OilandGas.Models.Data.Security.ROLE),
            _connectionName, "ROLE", null);

        var roles = (await roleRepo.GetAsync(new List<AppFilter>
        {
            new() { FieldName = "ROLE_NAME", FilterValue = roleName }
        })).OfType<Beep.OilandGas.Models.Data.Security.ROLE>().ToList();

        if (roles.Count == 0) return new HashSet<string>();

        var roleId = roles[0].ROLE_ID;
        var rps = (await rpRepo.GetAsync(new List<AppFilter>
        {
            new() { FieldName = "ROLE_ID", FilterValue = roleId }
        })).OfType<Beep.OilandGas.Models.Data.Security.ROLE_PERMISSION>().ToList();

        var permIds = rps.Select(rp => rp.PERMISSION_ID).ToHashSet();
        var allPerms = (await permRepo.GetAsync(new List<AppFilter>()))
            .OfType<Beep.OilandGas.Models.Data.Security.PERMISSION>().ToList();

        return allPerms.Where(p => permIds.Contains(p.PERMISSION_ID))
            .Select(p => p.PERMISSION_CODE)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private PPDMGenericRepository GetRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(SOD_RULE), _connectionName, "SOD_RULE", null);
}
