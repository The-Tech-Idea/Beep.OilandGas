using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services;

public class DefaultSecuritySeedService : IDefaultSecuritySeedService
{
    private const string SystemOrganizationBaId = "BEEP_ORG_SYSTEM";
    private const string SystemOrganizationId = "BEEP_OILGAS";
    private const string SystemUserBaId = "BEEP_USER_SYSTEM";
    private const string SystemUserId = "SYSTEM";

    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<DefaultSecuritySeedService>? _logger;

    public DefaultSecuritySeedService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<DefaultSecuritySeedService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<DefaultSecuritySeedResult> SeedDefaultsAsync(string userId = "SYSTEM")
    {
        var result = new DefaultSecuritySeedResult { Success = true };

        try
        {
            await SeedBusinessAssociatePrincipalsAsync(result, userId);

            var roleRepo = GetRepo<ROLE>("ROLE");
            var permissionRepo = GetRepo<PERMISSION>("PERMISSION");
            var rolePermissionRepo = GetRepo<ROLE_PERMISSION>("ROLE_PERMISSION");

            var existingRoles = (await roleRepo.GetAsync(new List<AppFilter>()))
                .OfType<ROLE>()
                .ToDictionary(r => r.ROLE_NAME, StringComparer.OrdinalIgnoreCase);

            var roleNames = new[]
            {
                RoleDefinitions.Viewer,
                RoleDefinitions.Manager,
                RoleDefinitions.PetroleumEngineer,
                RoleDefinitions.ReservoirEngineer,
                RoleDefinitions.Administrator,
                RoleDefinitions.Approver,
                RoleDefinitions.Auditor,
                "Admin",
                "Supervisor",
                "ReservesEngineer",
                "SafetyOfficer",
                "GateApprover",
                "Compliance"
            };

            foreach (var roleName in roleNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (existingRoles.ContainsKey(roleName))
                {
                    continue;
                }

                var role = new ROLE
                {
                    ROLE_ID = Guid.NewGuid().ToString(),
                    ROLE_NAME = roleName,
                    DESCRIPTION = $"Auto-seeded role: {roleName}"
                };

                await roleRepo.InsertAsync(role, userId);
                existingRoles[roleName] = role;
                result.RolesInserted++;
            }

            var existingPermissions = (await permissionRepo.GetAsync(new List<AppFilter>()))
                .OfType<PERMISSION>()
                .ToDictionary(p => p.PERMISSION_CODE, StringComparer.OrdinalIgnoreCase);

            var permissionCodes = new[]
            {
                PermissionConstants.Accounting.View,
                PermissionConstants.Accounting.PostJournal,
                PermissionConstants.Accounting.ApproveJournal,
                PermissionConstants.Accounting.EditSettings,
                PermissionConstants.Accounting.ViewReports,
                PermissionConstants.Accounting.ManagePeriods,
                PermissionConstants.Admin.ManageUsers,
                PermissionConstants.Admin.AssignRoles,
                PermissionConstants.Admin.ViewAuditLogs,
                PermissionConstants.Admin.ConfigureSystem,
                PermissionConstants.Tax.ViewProvision,
                PermissionConstants.Tax.Calculate,
                PermissionConstants.Tax.Adjust
            };

            foreach (var permissionCode in permissionCodes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (existingPermissions.ContainsKey(permissionCode))
                {
                    continue;
                }

                var permission = new PERMISSION
                {
                    PERMISSION_ID = Guid.NewGuid().ToString(),
                    PERMISSION_CODE = permissionCode,
                    DESCRIPTION = $"Auto-seeded permission: {permissionCode}"
                };

                await permissionRepo.InsertAsync(permission, userId);
                existingPermissions[permissionCode] = permission;
                result.PermissionsInserted++;
            }

            var existingRolePermissions = (await rolePermissionRepo.GetAsync(new List<AppFilter>()))
                .OfType<ROLE_PERMISSION>()
                .Select(rp => (rp.ROLE_ID, rp.PERMISSION_ID))
                .ToHashSet();

            var rolePermissionMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [RoleDefinitions.Viewer] = new[]
                {
                    PermissionConstants.Accounting.View,
                    PermissionConstants.Accounting.ViewReports,
                    PermissionConstants.Tax.ViewProvision
                },
                [RoleDefinitions.Manager] = permissionCodes,
                [RoleDefinitions.Administrator] = permissionCodes,
                ["Admin"] = permissionCodes,
                [RoleDefinitions.Auditor] = new[]
                {
                    PermissionConstants.Admin.ViewAuditLogs,
                    PermissionConstants.Accounting.ViewReports
                }
            };

            foreach (var kvp in rolePermissionMap)
            {
                if (!existingRoles.TryGetValue(kvp.Key, out var role))
                {
                    continue;
                }

                foreach (var permissionCode in kvp.Value.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!existingPermissions.TryGetValue(permissionCode, out var permission))
                    {
                        continue;
                    }

                    var mappingKey = (role.ROLE_ID, permission.PERMISSION_ID);
                    if (existingRolePermissions.Contains(mappingKey))
                    {
                        continue;
                    }

                    var mapping = new ROLE_PERMISSION
                    {
                        ROLE_PERMISSION_ID = Guid.NewGuid().ToString(),
                        ROLE_ID = role.ROLE_ID,
                        PERMISSION_ID = permission.PERMISSION_ID
                    };

                    await rolePermissionRepo.InsertAsync(mapping, userId);
                    existingRolePermissions.Add(mappingKey);
                    result.RolePermissionsInserted++;
                }
            }

            await SeedDefaultUsersAndRoleAssignmentsAsync(result, existingRoles, userId);

            _logger?.LogInformation(
                "Default security seed completed. BusinessAssociatesInserted={BusinessAssociatesInserted}, BaOrganizationsInserted={BaOrganizationsInserted}, UsersInserted={UsersInserted}, UserRolesInserted={UserRolesInserted}, RolesInserted={RolesInserted}, PermissionsInserted={PermissionsInserted}, RolePermissionsInserted={RolePermissionsInserted}",
                result.BusinessAssociatesInserted,
                result.BaOrganizationsInserted,
                result.UsersInserted,
                result.UserRolesInserted,
                result.RolesInserted,
                result.PermissionsInserted,
                result.RolePermissionsInserted);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
            _logger?.LogError(ex, "Default security seeding failed for connection {ConnectionName}", _connectionName);
        }

        return result;
    }

    private async Task SeedBusinessAssociatePrincipalsAsync(DefaultSecuritySeedResult result, string userId)
    {
        var businessAssociateRepo = await TryGetDynamicRepoAsync("BUSINESS_ASSOCIATE");
        if (businessAssociateRepo == null)
        {
            _logger?.LogDebug("BUSINESS_ASSOCIATE table metadata was not found; skipping BA principal seeding.");
            return;
        }

        var existingBusinessAssociates = await businessAssociateRepo.GetAsync(new List<AppFilter>());
        var existingBaIds = existingBusinessAssociates
            .Select(item => GetStringProperty(item, "BUSINESS_ASSOCIATE_ID"))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seedBusinessAssociates = new[]
        {
            new
            {
                BusinessAssociateId = SystemOrganizationBaId,
                ShortName = "Beep System Org",
                LongName = "Beep Oil and Gas System Organization",
                BaType = "ORGANIZATION",
                FirstName = string.Empty,
                LastName = string.Empty
            },
            new
            {
                BusinessAssociateId = SystemUserBaId,
                ShortName = "System User",
                LongName = "Beep Oil and Gas System User",
                BaType = "PERSON",
                FirstName = "System",
                LastName = "User"
            }
        };

        foreach (var seedBa in seedBusinessAssociates)
        {
            if (existingBaIds.Contains(seedBa.BusinessAssociateId))
            {
                continue;
            }

            var entity = Activator.CreateInstance(businessAssociateRepo.EntityType);
            if (entity == null)
            {
                continue;
            }

            SetPropertyIfExists(entity, "BUSINESS_ASSOCIATE_ID", seedBa.BusinessAssociateId);
            SetPropertyIfExists(entity, "BA_SHORT_NAME", seedBa.ShortName);
            SetPropertyIfExists(entity, "BA_LONG_NAME", seedBa.LongName);
            SetPropertyIfExists(entity, "BA_TYPE", seedBa.BaType);
            SetPropertyIfExists(entity, "CURRENT_STATUS", "ACTIVE");
            SetPropertyIfExists(entity, "ACTIVE_IND", "Y");
            SetPropertyIfExists(entity, "FIRST_NAME", seedBa.FirstName);
            SetPropertyIfExists(entity, "LAST_NAME", seedBa.LastName);
            SetPropertyIfExists(entity, "SOURCE", "DefaultSecuritySeedService");

            await businessAssociateRepo.InsertAsync(entity, userId);
            existingBaIds.Add(seedBa.BusinessAssociateId);
            result.BusinessAssociatesInserted++;
        }

        var baOrganizationRepo = await TryGetDynamicRepoAsync("BA_ORGANIZATION");
        if (baOrganizationRepo == null)
        {
            _logger?.LogDebug("BA_ORGANIZATION table metadata was not found; skipping BA_ORGANIZATION seeding.");
            return;
        }

        var existingBaOrganizations = await baOrganizationRepo.GetAsync(new List<AppFilter>());
        var orgAlreadySeeded = existingBaOrganizations.Any(item =>
            string.Equals(GetStringProperty(item, "BUSINESS_ASSOCIATE_ID"), SystemOrganizationBaId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(GetStringProperty(item, "ORGANIZATION_ID"), SystemOrganizationId, StringComparison.OrdinalIgnoreCase));

        if (orgAlreadySeeded)
        {
            return;
        }

        var organizationEntity = Activator.CreateInstance(baOrganizationRepo.EntityType);
        if (organizationEntity == null)
        {
            return;
        }

        SetPropertyIfExists(organizationEntity, "BUSINESS_ASSOCIATE_ID", SystemOrganizationBaId);
        SetPropertyIfExists(organizationEntity, "ORGANIZATION_ID", SystemOrganizationId);
        SetPropertyIfExists(organizationEntity, "ORGANIZATION_SEQ_NO", 1m);
        SetPropertyIfExists(organizationEntity, "ORGANIZATION_NAME", "Beep Oil and Gas");
        SetPropertyIfExists(organizationEntity, "ORGANIZATION_TYPE", "TENANT");
        SetPropertyIfExists(organizationEntity, "ACTIVE_IND", "Y");
        SetPropertyIfExists(organizationEntity, "SOURCE", "DefaultSecuritySeedService");

        await baOrganizationRepo.InsertAsync(organizationEntity, userId);
        result.BaOrganizationsInserted++;
    }

    private async Task SeedDefaultUsersAndRoleAssignmentsAsync(
        DefaultSecuritySeedResult result,
        Dictionary<string, ROLE> existingRoles,
        string userId)
    {
        var userRepo = GetRepo<USER>("USER");
        var userRoleRepo = GetRepo<USER_ROLE>("USER_ROLE");

        var existingUsers = (await userRepo.GetAsync(new List<AppFilter>()))
            .OfType<USER>()
            .ToDictionary(u => u.USER_ID, StringComparer.OrdinalIgnoreCase);

        if (!existingUsers.ContainsKey(SystemUserId))
        {
            var systemUser = new USER
            {
                USER_ID = SystemUserId,
                USERNAME = "SYSTEM",
                EMAIL = "system@beep.local",
                PASSWORD_HASH = "seeded-no-login",
                IS_ACTIVE = true,
                TENANT_ID = string.Empty,
                BUSINESS_ASSOCIATE_ID = SystemUserBaId
            };

            await userRepo.InsertAsync(systemUser, userId);
            existingUsers[SystemUserId] = systemUser;
            result.UsersInserted++;
        }

        var existingUserRoles = (await userRoleRepo.GetAsync(new List<AppFilter>()))
            .OfType<USER_ROLE>()
            .Select(ur => (ur.USER_ID, ur.ROLE_ID))
            .ToHashSet();

        var adminRole = existingRoles.TryGetValue(RoleDefinitions.Administrator, out var administrator)
            ? administrator
            : existingRoles.TryGetValue("Admin", out var admin)
                ? admin
                : null;

        if (adminRole == null)
        {
            return;
        }

        var adminUserRoleKey = (SystemUserId, adminRole.ROLE_ID);
        if (existingUserRoles.Contains(adminUserRoleKey))
        {
            return;
        }

        var userRole = new USER_ROLE
        {
            USER_ROLE_ID = Guid.NewGuid().ToString(),
            USER_ID = SystemUserId,
            ROLE_ID = adminRole.ROLE_ID,
            BUSINESS_ASSOCIATE_ID = SystemUserBaId,
            EFFECTIVE_DATE = DateTime.UtcNow
        };

        await userRoleRepo.InsertAsync(userRole, userId);
        result.UserRolesInserted++;
    }

    private async Task<PPDMGenericRepository?> TryGetDynamicRepoAsync(string tableName)
    {
        try
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
            {
                return null;
            }

            var entityType = ResolveEntityType(metadata.EntityTypeName);
            if (entityType == null)
            {
                return null;
            }

            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                entityType,
                _connectionName,
                tableName,
                null);
        }
        catch
        {
            return null;
        }
    }

    private static Type? ResolveEntityType(string? entityTypeName)
    {
        if (string.IsNullOrWhiteSpace(entityTypeName))
        {
            return null;
        }

        var direct = Type.GetType($"Beep.OilandGas.PPDM39.Models.{entityTypeName}")
            ?? Type.GetType(entityTypeName);
        if (direct != null)
        {
            return direct;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType($"Beep.OilandGas.PPDM39.Models.{entityTypeName}", throwOnError: false, ignoreCase: false)
                ?? assembly.GetType(entityTypeName, throwOnError: false, ignoreCase: false);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private static string? GetStringProperty(object target, string propertyName)
    {
        var value = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(target);
        return value?.ToString();
    }

    private static void SetPropertyIfExists(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.CanWrite != true)
        {
            return;
        }

        if (value == null)
        {
            property.SetValue(target, null);
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (targetType.IsAssignableFrom(value.GetType()))
        {
            property.SetValue(target, value);
            return;
        }

        property.SetValue(target, Convert.ChangeType(value, targetType));
    }

    private PPDMGenericRepository GetRepo<T>(string tableName)
    {
        return new PPDMGenericRepository(
            _editor,
            _commonColumnHandler,
            _defaults,
            _metadata,
            typeof(T),
            _connectionName,
            tableName,
            null);
    }
}
