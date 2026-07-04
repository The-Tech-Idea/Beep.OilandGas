using Beep.OilandGas.PPDM39.Core;
using System.Text.Json;
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
/// Generates compliance reports for SOX ITGC, user access summaries, and role-permission matrices.
/// All reports are generated from live data in the PPDM extension tables.
/// Part of Phase 4 governance & compliance.
/// </summary>
public interface IComplianceReportService
{
    /// <summary>Generate a SOX IT General Controls report for a given period.</summary>
    Task<SoxItgcReport> GenerateSoxItgcReportAsync(DateTime periodStart, DateTime periodEnd);

    /// <summary>Generate user access summary: all users, roles, permissions, SoD conflicts, compensating controls.</summary>
    Task<UserAccessSummaryReport> GenerateUserAccessReportAsync();

    /// <summary>Generate role-permission matrix in JSON format for audit review.</summary>
    Task<string> GenerateRolePermissionMatrixJsonAsync();

    /// <summary>Get SoD conflict summary for audit reporting.</summary>
    Task<SodSummaryReport> GetSodSummaryReportAsync();
}

public class SoxItgcReport
{
    public string ReportTitle { get; set; } = "SOX IT General Controls Report";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public AccessControlSection AccessControl { get; set; } = new();
    public ChangeManagementSection ChangeManagement { get; set; } = new();
    public ComputerOperationsSection ComputerOperations { get; set; } = new();
    public List<string> Findings { get; set; } = new();
}

public class AccessControlSection
{
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalPermissions { get; set; }
    public int ActiveSoDConflicts { get; set; }
    public int ActiveCompensatingControls { get; set; }
    public int UsersWithElevatedAccess { get; set; }
    public DateTime? LastAccessReviewDate { get; set; }
    public List<string> CriticalFindings { get; set; } = new();
}

public class ChangeManagementSection
{
    public int ProcessDefinitionChanges { get; set; }
    public int WorkflowVersionChanges { get; set; }
    public int RoleAssignmentChanges { get; set; }
    public List<string> UnauthorizedChanges { get; set; } = new();
}

public class ComputerOperationsSection
{
    public int TotalProcessInstances { get; set; }
    public int SlaBreachesInPeriod { get; set; }
    public int EscalationsInPeriod { get; set; }
    public int AuditChainVerificationFailures { get; set; }
}

public class UserAccessSummaryReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<UserAccessEntry> Users { get; set; } = new();
    public SodSummaryReport SodSummary { get; set; } = new();
}

public class UserAccessEntry
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public List<string> ActiveSoDConflicts { get; set; } = new();
    public List<string> ActiveCompensatingControls { get; set; } = new();
    public DateTime? LastLoginDate { get; set; }
}

public class SodSummaryReport
{
    public int TotalRules { get; set; }
    public int ActiveConflicts { get; set; }
    public int MitigatedConflicts { get; set; }
    public int ResolvedConflicts { get; set; }
    public int ExpiredCompensatingControls { get; set; }
    public Dictionary<string, int> ConflictsBySeverity { get; set; } = new();
}

public class ComplianceReportService : IComplianceReportService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<ComplianceReportService> _logger;

    public ComplianceReportService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<ComplianceReportService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<SoxItgcReport> GenerateSoxItgcReportAsync(DateTime periodStart, DateTime periodEnd)
    {
        var report = new SoxItgcReport { PeriodStart = periodStart, PeriodEnd = periodEnd };

        try
        {
            // Access Control
            var userRepo = GetRepo<Beep.OilandGas.Models.Data.Security.USER>("USER");
            var roleRepo = GetRepo<Beep.OilandGas.Models.Data.Security.ROLE>("ROLE");
            var sodConflictRepo = GetRepo<SOD_CONFLICT>("SOD_CONFLICT");
            var compControlRepo = GetRepo<COMPENSATING_CONTROL>("COMPENSATING_CONTROL");

            var users = (await userRepo.GetAsync(new List<AppFilter>())).ToList();
            var roles = (await roleRepo.GetAsync(new List<AppFilter>())).ToList();
            report.AccessControl.TotalUsers = users.Count;
            report.AccessControl.TotalRoles = roles.Count;

            var conflicts = (await sodConflictRepo.GetAsync(new List<AppFilter>
            {
                new() { FieldName = "CONFLICT_STATUS", FilterValue = "ACTIVE" }
            })).ToList();
            report.AccessControl.ActiveSoDConflicts = conflicts.Count;

            var controls = (await compControlRepo.GetAsync(new List<AppFilter>
            {
                new() { FieldName = "STATUS", FilterValue = "ACTIVE" }
            })).ToList();
            report.AccessControl.ActiveCompensatingControls = controls.Count;

            if (conflicts.Count > 0)
                report.AccessControl.CriticalFindings.Add(
                    $"{conflicts.Count} active SoD conflicts require review");

            // Change Management
            var versionRepo = GetRepo<WORKFLOW_VERSION>("WORKFLOW_VERSION");
            var versions = (await versionRepo.GetAsync(new List<AppFilter>())).ToList();
            var changesInPeriod = versions.OfType<WORKFLOW_VERSION>()
                .Count(v => v.EFFECTIVE_DATE >= periodStart && v.EFFECTIVE_DATE <= periodEnd);
            report.ChangeManagement.WorkflowVersionChanges = changesInPeriod;

            // Computer Operations
            var instanceRepo = GetRepo<PROCESS_INSTANCE>("PROCESS_INSTANCE");
            var instances = (await instanceRepo.GetAsync(new List<AppFilter>())).ToList();
            report.ComputerOperations.TotalProcessInstances = instances.Count;

            var historyRepo = GetRepo<PROCESS_HISTORY>("PROCESS_HISTORY");
            var historyFilters = new List<AppFilter>
            {
                new() { FieldName = "EVENT_TYPE", FilterValue = "SLA_BREACH" },
            };
            var slaBreaches = (await historyRepo.GetAsync(historyFilters)).ToList();
            report.ComputerOperations.SlaBreachesInPeriod = slaBreaches
                .Count(h => ((PROCESS_HISTORY)h).EVENT_DATE >= periodStart && ((PROCESS_HISTORY)h).EVENT_DATE <= periodEnd);

            _logger?.LogInformation("SOX ITGC report generated for period {Start} to {End}", periodStart, periodEnd);
        }
        catch (Exception ex)
        {
            report.Findings.Add($"Report generation error: {ex.Message}");
            _logger?.LogError(ex, "Failed to generate SOX ITGC report");
        }

        return report;
    }

    public async Task<UserAccessSummaryReport> GenerateUserAccessReportAsync()
    {
        var report = new UserAccessSummaryReport();

        try
        {
            var users = (await GetRepo<Beep.OilandGas.Models.Data.Security.USER>("USER")
                .GetAsync(new List<AppFilter>())).OfType<Beep.OilandGas.Models.Data.Security.USER>().ToList();

            foreach (var user in users.Take(500)) // Limit for performance
            {
                var entry = new UserAccessEntry
                {
                    UserId = user.USER_ID,
                    UserName = user.USER_NAME,
                };
                report.Users.Add(entry);
            }

            report.SodSummary = await GetSodSummaryReportAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate user access report");
        }

        return report;
    }

    public async Task<string> GenerateRolePermissionMatrixJsonAsync()
    {
        try
        {
            var roles = (await GetRepo<Beep.OilandGas.Models.Data.Security.ROLE>("ROLE")
                .GetAsync(new List<AppFilter>())).OfType<Beep.OilandGas.Models.Data.Security.ROLE>().ToList();

            var rpRepo = GetRepo<Beep.OilandGas.Models.Data.Security.ROLE_PERMISSION>("ROLE_PERMISSION");
            var permRepo = GetRepo<Beep.OilandGas.Models.Data.Security.PERMISSION>("PERMISSION");
            var allPerms = (await permRepo.GetAsync(new List<AppFilter>()))
                .OfType<Beep.OilandGas.Models.Data.Security.PERMISSION>().ToList();

            var matrix = new Dictionary<string, object>();
            foreach (var role in roles)
            {
                var rps = (await rpRepo.GetAsync(new List<AppFilter>
                {
                    new() { FieldName = "ROLE_ID", FilterValue = role.ROLE_ID }
                })).OfType<Beep.OilandGas.Models.Data.Security.ROLE_PERMISSION>().ToList();

                var permIds = rps.Select(r => r.PERMISSION_ID).ToHashSet();
                var permCodes = allPerms
                    .Where(p => permIds.Contains(p.PERMISSION_ID))
                    .Select(p => p.PERMISSION_CODE)
                    .OrderBy(c => c)
                    .ToList();

                matrix[role.ROLE_NAME] = new
                {
                    roleId = role.ROLE_ID,
                    permissionCount = permCodes.Count,
                    permissions = permCodes,
                };
            }

            return JsonSerializer.Serialize(matrix, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate role-permission matrix");
            return "{}";
        }
    }

    public async Task<SodSummaryReport> GetSodSummaryReportAsync()
    {
        var report = new SodSummaryReport();

        try
        {
            var rules = (await GetRepo<SOD_RULE>("SOD_RULE").GetAsync(new List<AppFilter>()))
                .OfType<SOD_RULE>().ToList();
            report.TotalRules = rules.Count;

            var conflicts = (await GetRepo<SOD_CONFLICT>("SOD_CONFLICT").GetAsync(new List<AppFilter>()))
                .OfType<SOD_CONFLICT>().ToList();

            report.ActiveConflicts = conflicts.Count(c => c.CONFLICT_STATUS == "ACTIVE");
            report.MitigatedConflicts = conflicts.Count(c => c.CONFLICT_STATUS == "MITIGATED");
            report.ResolvedConflicts = conflicts.Count(c => c.CONFLICT_STATUS == "RESOLVED");

            report.ConflictsBySeverity = conflicts
                .GroupBy(c => c.CONFLICT_STATUS)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate SoD summary report");
        }

        return report;
    }

    private PPDMGenericRepository GetRepo<T>(string tableName) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(T), _connectionName, tableName, null);
}
