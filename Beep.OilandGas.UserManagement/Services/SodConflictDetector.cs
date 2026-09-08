using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services;

/// <summary>
/// Detects Segregation of Duties conflicts at role assignment time (preventive)
/// and at transaction time (detective). Integrates with SodEvaluationEngine.
/// Part of Phase 4 governance & compliance.
/// </summary>
public interface ISodConflictDetector
{
    /// <summary>Called BEFORE a role is assigned. Returns conflicts. If blocking, assignment is rejected.</summary>
    Task<SodCheckResult> PreAssignCheckAsync(string userId, string newRoleId, List<string> newPermissionCodes);

    /// <summary>Called at transaction time to check if user performed both sides of a conflicting action.</summary>
    Task<SodCheckResult> PreTransactionCheckAsync(string userId, string action, string entityType, string entityId);

    /// <summary>Log all SoD evaluations for audit.</summary>
    Task LogSodEvaluationAsync(SodCheckResult result, string contextUserId);

    /// <summary>Record a detected SoD conflict in the SOD_CONFLICT table.</summary>
    Task<SOD_CONFLICT> RecordConflictAsync(string sodRuleId, string ruleName, string userId, string roleA, string roleB);
}

public class SodConflictDetector : ISodConflictDetector
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ISodEvaluationEngine _sodEngine;
    private readonly ILogger<SodConflictDetector> _logger;

    public SodConflictDetector(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ISodEvaluationEngine sodEngine,
        ILogger<SodConflictDetector>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _sodEngine = sodEngine;
        _logger = logger;
    }

    public async Task<SodCheckResult> PreAssignCheckAsync(
        string userId, string newRoleId, List<string> newPermissionCodes)
    {
        var result = new SodCheckResult();

        try
        {
            // Get user's existing permissions
            var existingPermissions = await GetUserEffectivePermissionsAsync(userId);
            var allPermissions = new List<string>(existingPermissions);
            allPermissions.AddRange(newPermissionCodes);

            var conflicts = await _sodEngine.EvaluatePermissionsAsync(allPermissions);

            foreach (var conflict in conflicts)
            {
                if (conflict.IsBlocking)
                {
                    result.Conflicts.Add(new SodConflictResult
                    {
                        RuleId = conflict.RuleId,
                        RuleName = conflict.RuleName,
                        PermissionA = conflict.PermissionA,
                        PermissionB = conflict.PermissionB,
                        Severity = conflict.Severity,
                        IsBlocking = true,
                        Description = conflict.Description,
                        RegulationReference = conflict.RegulationReference,
                        MitigationGuidance = conflict.MitigationGuidance,
                    });
                    result.HasBlockingConflict = true;
                }
                else
                {
                    result.Warnings.Add(conflict);
                }
            }

            result.HasConflict = result.Conflicts.Count > 0 || result.Warnings.Count > 0;

            if (result.HasBlockingConflict)
                _logger?.LogWarning("SoD blocking conflict detected for user {UserId}: {RuleNames}",
                    userId, string.Join(", ", result.Conflicts.Select(c => c.RuleName)));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SoD pre-assign check failed for user {UserId}", userId);
        }

        return result;
    }

    public async Task<SodCheckResult> PreTransactionCheckAsync(
        string userId, string action, string entityType, string entityId)
    {
        var result = new SodCheckResult();

        try
        {
            // Check if the same user performed a conflicting action on this entity
            var historyRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PROCESS_HISTORY), _connectionName, "PROCESS_HISTORY", null);

            // Look for prior actions by this user on this entity/process combination
            var filters = new List<AppFilter>
            {
                new() { FieldName = "USER_ID", FilterValue = userId },
            };
            var allHistory = (await historyRepo.GetAsync(filters))
                .OfType<PROCESS_HISTORY>().ToList();

            // For now, check if user performed both create and approve actions
            var createActions = allHistory.Where(h =>
                h.EVENT_TYPE == "STEP_COMPLETED" && h.DETAILS?.Contains("CREATE", StringComparison.OrdinalIgnoreCase) == true).ToList();
            var approveActions = allHistory.Where(h =>
                h.EVENT_TYPE == "APPROVAL" && h.FROM_STATUS == "PENDING" && h.TO_STATUS == "APPROVED").ToList();

            if (createActions.Any() && approveActions.Any())
            {
                result.Warnings.Add(new SodConflictResult
                {
                    RuleName = "CREATE_APPROVE_SAME_USER",
                    PermissionA = action,
                    PermissionB = "APPROVE",
                    Description = $"User performed both create and approve actions within the same context",
                });
                result.HasConflict = true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SoD pre-transaction check failed for user {UserId}", userId);
        }

        return result;
    }

    public async Task LogSodEvaluationAsync(SodCheckResult result, string contextUserId)
    {
        try
        {
            if (!result.HasConflict) return;

            var auditRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(Beep.OilandGas.UserManagement.Models.Audit.AuthorizationDecisionTrace),
                _connectionName, "AUTHORIZATION_DECISION_TRACE", null);

            foreach (var conflict in result.Conflicts)
            {
                var trace = new Beep.OilandGas.UserManagement.Models.Audit.AuthorizationDecisionTrace
                {
                    TRACE_ID = Guid.NewGuid().ToString(),
                    USER_ID = contextUserId,
                    POLICY_KEY = "SOD_CHECK",
                    ACTION = conflict.RuleName,
                    RESOURCE = $"PERMISSION:{conflict.PermissionA}+{conflict.PermissionB}",
                    DECISION = conflict.IsBlocking ? "DENY" : "WARN",
                    EVALUATED_UTC = DateTime.UtcNow,
                    CONTEXT_JSON = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        conflict.RuleId,
                        conflict.Severity,
                        conflict.RegulationReference,
                    }),
                    DENIAL_REASON_CODE = conflict.IsBlocking ? "SOD_BLOCKING" : null,
                };

                await auditRepo.InsertAsync(trace, contextUserId);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to log SoD evaluation for user {UserId}", contextUserId);
        }
    }

    public async Task<SOD_CONFLICT> RecordConflictAsync(
        string sodRuleId, string ruleName, string userId, string roleA, string roleB)
    {
        var repo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(SOD_CONFLICT), _connectionName, "SOD_CONFLICT", null);

        var conflict = new SOD_CONFLICT
        {
            SOD_RULE_ID = sodRuleId,
            RULE_NAME = ruleName,
            USER_ID = userId,
            ROLE_A = roleA,
            ROLE_B = roleB,
            DETECTED_DATE = DateTime.UtcNow,
            CONFLICT_STATUS = "ACTIVE",
        };

        await repo.InsertAsync(conflict, userId);

        _logger?.LogWarning("SoD conflict recorded: User={UserId}, Rule={RuleName}, Roles={RoleA}+{RoleB}",
            userId, ruleName, roleA, roleB);

        return conflict;
    }

    private async Task<HashSet<string>> GetUserEffectivePermissionsAsync(string userId)
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Get user's roles
        var userRoleRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(Beep.OilandGas.UserManagement.Models.Identity.AppUserRole),
            _connectionName, "APP_USER_ROLE", null);

        var userRoles = (await userRoleRepo.GetAsync(new List<AppFilter>
        {
            new() { FieldName = "USER_ID", FilterValue = userId }
        })).OfType<Beep.OilandGas.UserManagement.Models.Identity.AppUserRole>().ToList();

        // Get role-permission mappings
        var rpRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(Beep.OilandGas.Models.Data.Security.ROLE_PERMISSION),
            _connectionName, "ROLE_PERMISSION", null);

        var permRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(Beep.OilandGas.Models.Data.Security.PERMISSION),
            _connectionName, "PERMISSION", null);

        var allPerms = (await permRepo.GetAsync(new List<AppFilter>()))
            .OfType<Beep.OilandGas.Models.Data.Security.PERMISSION>().ToList();

        foreach (var userRole in userRoles)
        {
            var rps = (await rpRepo.GetAsync(new List<AppFilter>
            {
                new() { FieldName = "ROLE_ID", FilterValue = userRole.ROLE_ID }
            })).OfType<Beep.OilandGas.Models.Data.Security.ROLE_PERMISSION>().ToList();

            foreach (var rp in rps)
            {
                var perm = allPerms.FirstOrDefault(p => p.PERMISSION_ID == rp.PERMISSION_ID);
                if (perm is not null) permissions.Add(perm.PERMISSION_CODE);
            }
        }

        return permissions;
    }
}
