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
    private readonly ILogger<RoleAssignmentService>? _logger;

    public RoleAssignmentService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<RoleAssignmentService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    private PPDMGenericRepository GetRepo<T>(string tableName) =>
        new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(T), _connectionName, tableName, null);

    // ── Role assignments ────────────────────────────────────────────────────

    public async Task<AppUserRole> AssignRoleAsync(
        string userId, string roleId, string grantedByUserId, string? reason = null)
    {
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
}
