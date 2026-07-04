using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Audit;
using Beep.OilandGas.UserManagement.Models.Identity;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services;

public class RoleAssignmentService : IRoleAssignmentService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ISodConflictDetector? _sodDetector;
    private readonly ILogger<RoleAssignmentService>? _logger;

    public RoleAssignmentService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<RoleAssignmentService>? logger = null,
        ISodConflictDetector? sodDetector = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
        _sodDetector = sodDetector;
    }

    private PPDMGenericRepository GetRepo<T>(string tableName) =>
        new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(T), _connectionName, tableName, null);

    // ── Role assignments ────────────────────────────────────────────────────

    public async Task<AppUserRole> AssignRoleAsync(
        string userId, string roleId, string grantedByUserId, string? reason = null)
    {
        // Phase 4: SoD pre-assignment check
        if (_sodDetector is not null)
        {
            var newPermissions = await GetPermissionsForRoleAsync(roleId);
            var sodCheck = await _sodDetector.PreAssignCheckAsync(userId, roleId, newPermissions);
            await _sodDetector.LogSodEvaluationAsync(sodCheck, grantedByUserId);

            if (sodCheck.HasBlockingConflict)
            {
                var conflictNames = string.Join(", ", sodCheck.Conflicts.Select(c => c.RuleName));
                _logger?.LogWarning("Role assignment BLOCKED by SoD: User={UserId}, Role={RoleId}, Conflicts={Conflicts}",
                    userId, roleId, conflictNames);
                throw new InvalidOperationException(
                    $"Role assignment violates Segregation of Duties: {conflictNames}. " +
                    "A compensating control waiver is required before proceeding.");
            }

            if (sodCheck.HasConflict)
            {
                _logger?.LogWarning("Role assignment has SoD warnings: User={UserId}, Role={RoleId}", userId, roleId);
            }
        }

        var repo = GetRepo<AppUserRole>("APP_USER_ROLE");
        var assignment = new AppUserRole
        {
            USER_ROLE_ID = Guid.NewGuid().ToString(),
            USER_ID = userId,
            ROLE_ID = roleId,
            GRANTED_BY_USER_ID = grantedByUserId,
            ASSIGNMENT_REASON = reason,
            EFFECTIVE_FROM_UTC = DateTime.UtcNow,
            APPROVAL_STATUS = "Approved"
        };
        await repo.InsertAsync(assignment, grantedByUserId);
        await WriteAccessAuditEventAsync(userId, "RoleAssigned",
            $"APP_USER_ROLE/{assignment.USER_ROLE_ID}", "Success", grantedByUserId);
        return assignment;
    }

    public async Task<bool> RevokeRoleAsync(string userRoleId, string revokedByUserId)
    {
        var repo = GetRepo<AppUserRole>("APP_USER_ROLE");
        var existing = await repo.GetByIdAsync(userRoleId) as AppUserRole;
        if (existing is null) return false;

        existing.EFFECTIVE_TO_UTC = DateTime.UtcNow;
        existing.APPROVAL_STATUS = "Revoked";
        await repo.UpdateAsync(existing, revokedByUserId);
        await WriteAccessAuditEventAsync(existing.USER_ID, "RoleRevoked",
            $"APP_USER_ROLE/{userRoleId}", "Success", revokedByUserId);
        return true;
    }

    public async Task<List<AppUserRole>> GetUserRoleAssignmentsAsync(string userId)
    {
        var repo = GetRepo<AppUserRole>("APP_USER_ROLE");
        var results = await repo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId }
        });
        return results.OfType<AppUserRole>()
            .Where(r => r.EFFECTIVE_TO_UTC == null || r.EFFECTIVE_TO_UTC > DateTime.UtcNow)
            .ToList();
    }

    // ── Permission grants ───────────────────────────────────────────────────

    public async Task<AppRolePermission> GrantPermissionToRoleAsync(
        string roleId, string permissionId, string approvedByUserId)
    {
        var repo = GetRepo<AppRolePermission>("APP_ROLE_PERMISSION");
        var grant = new AppRolePermission
        {
            ROLE_PERMISSION_ID = Guid.NewGuid().ToString(),
            ROLE_ID = roleId,
            PERMISSION_ID = permissionId,
            EFFECTIVE_FROM_UTC = DateTime.UtcNow,
            SOURCE_SYSTEM = "Manual",
            APPROVED_BY_USER_ID = approvedByUserId,
            APPROVED_AT_UTC = DateTime.UtcNow
        };
        await repo.InsertAsync(grant, approvedByUserId);
        await WriteAccessAuditEventAsync(approvedByUserId, "PermissionGranted",
            $"APP_ROLE_PERMISSION/{grant.ROLE_PERMISSION_ID}", "Success", approvedByUserId);
        return grant;
    }

    public async Task<bool> RevokePermissionFromRoleAsync(string rolePermissionId, string revokedByUserId)
    {
        var repo = GetRepo<AppRolePermission>("APP_ROLE_PERMISSION");
        var existing = await repo.GetByIdAsync(rolePermissionId) as AppRolePermission;
        if (existing is null) return false;

        existing.EFFECTIVE_TO_UTC = DateTime.UtcNow;
        await repo.UpdateAsync(existing, revokedByUserId);
        await WriteAccessAuditEventAsync(revokedByUserId, "PermissionRevoked",
            $"APP_ROLE_PERMISSION/{rolePermissionId}", "Success", revokedByUserId);
        return true;
    }

    public async Task<List<AppRolePermission>> GetRolePermissionsAsync(string roleId)
    {
        var repo = GetRepo<AppRolePermission>("APP_ROLE_PERMISSION");
        var results = await repo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = roleId }
        });
        return results.OfType<AppRolePermission>()
            .Where(p => p.EFFECTIVE_TO_UTC == null || p.EFFECTIVE_TO_UTC > DateTime.UtcNow)
            .ToList();
    }

    // ── Catalog queries ─────────────────────────────────────────────────────

    public async Task<List<AppRole>> GetRoleCatalogAsync()
    {
        var repo = GetRepo<AppRole>("APP_ROLE");
        var results = await repo.GetAsync(new List<AppFilter>());
        return results.OfType<AppRole>().ToList();
    }

    public async Task<List<AppPermission>> GetPermissionCatalogAsync()
    {
        var repo = GetRepo<AppPermission>("APP_PERMISSION");
        var results = await repo.GetAsync(new List<AppFilter>());
        return results.OfType<AppPermission>().ToList();
    }

    // ── Audit ───────────────────────────────────────────────────────────────

    private async Task WriteAccessAuditEventAsync(
        string userId, string eventType, string targetResource, string result, string actorUserId)
    {
        try
        {
            var auditRepo = GetRepo<UserAccessAuditEvent>("USER_ACCESS_AUDIT_EVENT");
            var ev = new UserAccessAuditEvent
            {
                EVENT_ID = Guid.NewGuid().ToString(),
                USER_ID = userId,
                EVENT_TYPE = eventType,
                TARGET_RESOURCE = targetResource,
                RESULT = result,
                EVENT_UTC = DateTime.UtcNow,
                CORRELATION_ID = Guid.NewGuid().ToString()
            };
            await auditRepo.InsertAsync(ev, actorUserId);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex,
                "Failed to write access audit event {EventType} for user {UserId}", eventType, userId);
        }
    }

    private async Task<List<string>> GetPermissionsForRoleAsync(string roleId)
    {
        var perms = new List<string>();
        try
        {
            var rpRepo = GetRepo<Beep.OilandGas.Models.Data.Security.ROLE_PERMISSION>("ROLE_PERMISSION");
            var rps = (await rpRepo.GetAsync(new List<AppFilter>
            {
                new() { FieldName = "ROLE_ID", FilterValue = roleId }
            })).OfType<Beep.OilandGas.Models.Data.Security.ROLE_PERMISSION>().ToList();

            var permRepo = GetRepo<Beep.OilandGas.Models.Data.Security.PERMISSION>("PERMISSION");
            var allPerms = (await permRepo.GetAsync(new List<AppFilter>()))
                .OfType<Beep.OilandGas.Models.Data.Security.PERMISSION>().ToList();

            var permIds = rps.Select(r => r.PERMISSION_ID).ToHashSet();
            perms = allPerms.Where(p => permIds.Contains(p.PERMISSION_ID))
                .Select(p => p.PERMISSION_CODE).ToList();
        }
        catch { /* Non-blocking — SoD check degrades gracefully */ }
        return perms;
    }
}
