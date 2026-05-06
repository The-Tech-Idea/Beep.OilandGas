using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Profile;
using Beep.OilandGas.UserManagement.Models.Scope;
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

            var permissionCodes = GetAllPermissionCodes();

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

            var rolePermissionMap = GetRolePermissionMap();

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

            await SeedDefaultPersonasAsync(result, userId);

            await SeedDefaultOrganizationScopesAsync(result, userId);

            await SeedDefaultUserScopeAssignmentsAsync(result, userId);

            await SeedDefaultUserAssetAccessAsync(result, userId);

            _logger?.LogInformation(
                "Default security seed completed. BusinessAssociatesInserted={BusinessAssociatesInserted}, BaOrganizationsInserted={BaOrganizationsInserted}, UsersInserted={UsersInserted}, UserRolesInserted={UserRolesInserted}, RolesInserted={RolesInserted}, PermissionsInserted={PermissionsInserted}, RolePermissionsInserted={RolePermissionsInserted}, PersonasInserted={PersonasInserted}, OrganizationScopesInserted={OrganizationScopesInserted}, UserScopeAssignmentsInserted={UserScopeAssignmentsInserted}, UserAssetAccessInserted={UserAssetAccessInserted}",
                result.BusinessAssociatesInserted,
                result.BaOrganizationsInserted,
                result.UsersInserted,
                result.UserRolesInserted,
                result.RolesInserted,
                result.PermissionsInserted,
                result.RolePermissionsInserted,
                result.PersonasInserted,
                result.OrganizationScopesInserted,
                result.UserScopeAssignmentsInserted,
                result.UserAssetAccessInserted);
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

    private async Task SeedDefaultPersonasAsync(DefaultSecuritySeedResult result, string userId)
    {
        var personaRepo = GetRepo<PersonaDefinition>("PERSONA_DEFINITION");

        var existingPersonas = (await personaRepo.GetAsync(new List<AppFilter>()))
            .OfType<PersonaDefinition>()
            .ToDictionary(p => p.PERSONA_CODE, StringComparer.OrdinalIgnoreCase);

        var seedPersonas = new[]
        {
            new { Code = "FIELD_ENGINEER", Name = "Field Engineer", Category = "Engineering", Route = "/field/dashboard", SortOrder = 10 },
            new { Code = "PRODUCTION_MANAGER", Name = "Production Manager", Category = "Management", Route = "/production/dashboard", SortOrder = 20 },
            new { Code = "RESERVOIR_ENGINEER", Name = "Reservoir Engineer", Category = "Engineering", Route = "/reservoir/dashboard", SortOrder = 30 },
            new { Code = "DRILLING_ENGINEER", Name = "Drilling Engineer", Category = "Engineering", Route = "/drilling/dashboard", SortOrder = 40 },
            new { Code = "HSE_OFFICER", Name = "HSE Officer", Category = "Safety", Route = "/hse/dashboard", SortOrder = 50 },
            new { Code = "FACILITIES_ENGINEER", Name = "Facilities Engineer", Category = "Engineering", Route = "/facilities/dashboard", SortOrder = 60 },
            new { Code = "DATA_ANALYST", Name = "Data Analyst", Category = "Analytics", Route = "/analytics/dashboard", SortOrder = 70 },
            new { Code = "ADMINISTRATOR", Name = "System Administrator", Category = "Administration", Route = "/admin/dashboard", SortOrder = 80 },
            // Lifecycle personas
            new { Code = "EXPLORATION_GEOLOGIST", Name = "Exploration Geologist", Category = "Exploration", Route = "/lifecycle/exploration/dashboard", SortOrder = 90 },
            new { Code = "DEVELOPMENT_PLANNER", Name = "Development Planner", Category = "Development", Route = "/lifecycle/development/dashboard", SortOrder = 100 },
            new { Code = "PRODUCTION_ENGINEER", Name = "Production Engineer", Category = "Production", Route = "/lifecycle/production/dashboard", SortOrder = 110 },
            new { Code = "DECOMMISSIONING_COORDINATOR", Name = "Decommissioning Coordinator", Category = "Decommissioning", Route = "/lifecycle/decommissioning/dashboard", SortOrder = 120 },
            new { Code = "HSE_COORDINATOR", Name = "HSE Coordinator", Category = "HSE", Route = "/lifecycle/hse/dashboard", SortOrder = 130 },
            new { Code = "FACILITY_OPERATOR", Name = "Facility Operator", Category = "Facilities", Route = "/lifecycle/facilities/dashboard", SortOrder = 140 },
            new { Code = "ASSET_MANAGER", Name = "Asset Manager", Category = "Management", Route = "/lifecycle/asset/dashboard", SortOrder = 150 },
            new { Code = "WORKFLOW_ADMINISTRATOR", Name = "Workflow Administrator", Category = "Administration", Route = "/lifecycle/workflow/admin", SortOrder = 160 },
        };

        foreach (var seedPersona in seedPersonas)
        {
            if (existingPersonas.ContainsKey(seedPersona.Code))
                continue;

            var persona = new PersonaDefinition
            {
                PERSONA_ID = Guid.NewGuid().ToString(),
                PERSONA_CODE = seedPersona.Code,
                PERSONA_NAME = seedPersona.Name,
                PERSONA_CATEGORY = seedPersona.Category,
                DESCRIPTION = $"Default persona: {seedPersona.Name}",
                ACTIVE_FLAG = "Y",
                DEFAULT_LANDING_ROUTE = seedPersona.Route,
                DISPLAY_SORT_ORDER = seedPersona.SortOrder
            };

            await personaRepo.InsertAsync(persona, userId);
            existingPersonas[seedPersona.Code] = persona;
            result.PersonasInserted++;
        }
    }

    private async Task SeedDefaultOrganizationScopesAsync(DefaultSecuritySeedResult result, string userId)
    {
        var scopeRepo = GetRepo<OrganizationScope>("ORGANIZATION_SCOPE");

        var existingScopes = (await scopeRepo.GetAsync(new List<AppFilter>()))
            .OfType<OrganizationScope>()
            .Any(s => string.Equals(s.ORGANIZATION_ID, SystemOrganizationId, StringComparison.OrdinalIgnoreCase));

        if (existingScopes)
            return;

        var orgScope = new OrganizationScope
        {
            ORG_SCOPE_ID = Guid.NewGuid().ToString(),
            ORGANIZATION_ID = SystemOrganizationId,
            ORG_NAME = "Beep Oil and Gas",
            HIERARCHY_TYPE = "TENANT",
            HIERARCHY_LEVEL = 1,
            ORG_BOUNDARY_TYPE = "ORGANIZATION",
            ORG_FULL_PATH = $"/{SystemOrganizationId}"
        };

        await scopeRepo.InsertAsync(orgScope, userId);
        result.OrganizationScopesInserted++;
    }

    private async Task SeedDefaultUserScopeAssignmentsAsync(DefaultSecuritySeedResult result, string userId)
    {
        var assignmentRepo = GetRepo<UserScopeAssignment>("USER_SCOPE_ASSIGNMENT");

        var existingAssignments = (await assignmentRepo.GetAsync(new List<AppFilter>()))
            .OfType<UserScopeAssignment>()
            .Any(a => string.Equals(a.USER_ID, SystemUserId, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(a.SCOPE_VALUE, SystemOrganizationId, StringComparison.OrdinalIgnoreCase));

        if (existingAssignments)
            return;

        var assignment = new UserScopeAssignment
        {
            ASSIGNMENT_ID = Guid.NewGuid().ToString(),
            USER_ID = SystemUserId,
            SCOPE_TYPE = "ORGANIZATION",
            SCOPE_VALUE = SystemOrganizationId,
            GRANTED_BY = "SYSTEM",
            EFFECTIVE_FROM_UTC = DateTime.UtcNow,
            INHERITED_IND = "N",
            SCOPE_LEVEL = "GLOBAL"
        };

        await assignmentRepo.InsertAsync(assignment, userId);
        result.UserScopeAssignmentsInserted++;
    }

    private async Task SeedDefaultUserAssetAccessAsync(DefaultSecuritySeedResult result, string userId)
    {
        var accessRepo = GetRepo<UserAssetAccess>("USER_ASSET_ACCESS");

        var existingAccess = (await accessRepo.GetAsync(new List<AppFilter>()))
            .OfType<UserAssetAccess>()
            .Any(a => string.Equals(a.USER_ID, SystemUserId, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(a.ASSET_TYPE, "ORGANIZATION", StringComparison.OrdinalIgnoreCase));

        if (existingAccess)
            return;

        var assetAccess = new UserAssetAccess
        {
            ACCESS_ID = Guid.NewGuid().ToString(),
            USER_ID = SystemUserId,
            ASSET_ID = SystemOrganizationId,
            ASSET_TYPE = "ORGANIZATION",
            ACCESS_LEVEL = "admin",
            DENY_OVERRIDE_IND = "N",
            GRANT_SOURCE_TYPE = "SYSTEM_SEED"
        };

        await accessRepo.InsertAsync(assetAccess, userId);
        result.UserAssetAccessInserted++;
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

    private static IEnumerable<string> GetAllPermissionCodes()
    {
        var codes = new List<string>();
        var permissionConstantsType = typeof(PermissionConstants);

        foreach (var nestedType in permissionConstantsType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            foreach (var field in nestedType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    var value = field.GetValue(null) as string;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        codes.Add(value);
                    }
                }
            }
        }

        return codes;
    }

    private static Dictionary<string, string[]> GetRolePermissionMap()
    {
        var allPermissions = GetAllPermissionCodes().ToArray();

        return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [RoleDefinitions.Viewer] = new[]
            {
                PermissionConstants.Accounting.View,
                PermissionConstants.Accounting.ViewReports,
                PermissionConstants.Tax.ViewProvision,
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.ViewWellStatus,
                PermissionConstants.Production.View,
                PermissionConstants.ProductionForecasting.View,
                PermissionConstants.Reservoir.View,
                PermissionConstants.Reservoir.ViewReserves,
                PermissionConstants.Drilling.View,
                PermissionConstants.Facilities.View,
                PermissionConstants.Facilities.ViewEquipment,
                PermissionConstants.Pipeline.View,
                PermissionConstants.NodalAnalysis.View,
                PermissionConstants.WellTestAnalysis.View,
                PermissionConstants.ChokeAnalysis.View,
                PermissionConstants.CompressorAnalysis.View,
                PermissionConstants.GasLift.View,
                PermissionConstants.PumpPerformance.View,
                PermissionConstants.SuckerRodPumping.View,
                PermissionConstants.PlungerLift.View,
                PermissionConstants.HydraulicPumps.View,
                PermissionConstants.EnhancedRecovery.View,
                PermissionConstants.HSE.View,
                PermissionConstants.HSE.ViewPermits,
                PermissionConstants.HSE.ViewRiskAssessments,
                PermissionConstants.HSE.ViewCertifications,
                PermissionConstants.Environmental.View,
                PermissionConstants.Environmental.ViewEmissions,
                PermissionConstants.Regulatory.View,
                PermissionConstants.Exploration.View,
                PermissionConstants.Exploration.ViewSeismic,
                PermissionConstants.ProspectIdentification.View,
                PermissionConstants.LeaseAcquisition.View,
                PermissionConstants.DevelopmentPlanning.View,
                PermissionConstants.Decommissioning.View,
                PermissionConstants.OilProperties.View,
                PermissionConstants.GasProperties.View,
                PermissionConstants.FlashCalculations.View,
                PermissionConstants.Drawing.View,
                PermissionConstants.Reporting.View,
                PermissionConstants.Dashboard.View
            },

            [RoleDefinitions.Manager] = new[]
            {
                PermissionConstants.Accounting.View,
                PermissionConstants.Accounting.PostJournal,
                PermissionConstants.Accounting.ViewReports,
                PermissionConstants.Accounting.ManagePeriods,
                PermissionConstants.Tax.ViewProvision,
                PermissionConstants.Tax.Calculate,
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.Create,
                PermissionConstants.WellManagement.Edit,
                PermissionConstants.WellManagement.Approve,
                PermissionConstants.WellManagement.ViewWellStatus,
                PermissionConstants.WellManagement.UpdateWellStatus,
                PermissionConstants.Production.View,
                PermissionConstants.Production.Create,
                PermissionConstants.Production.Edit,
                PermissionConstants.Production.Approve,
                PermissionConstants.Production.SubmitProduction,
                PermissionConstants.Production.AllocateProduction,
                PermissionConstants.ProductionForecasting.View,
                PermissionConstants.ProductionForecasting.Create,
                PermissionConstants.ProductionForecasting.RunDCA,
                PermissionConstants.Reservoir.View,
                PermissionConstants.Reservoir.Create,
                PermissionConstants.Reservoir.Edit,
                PermissionConstants.Reservoir.ViewReserves,
                PermissionConstants.Reservoir.UpdateReserves,
                PermissionConstants.Drilling.View,
                PermissionConstants.Drilling.Create,
                PermissionConstants.Drilling.Edit,
                PermissionConstants.Drilling.ViewDailyReports,
                PermissionConstants.Facilities.View,
                PermissionConstants.Facilities.Create,
                PermissionConstants.Facilities.Edit,
                PermissionConstants.Facilities.ViewEquipment,
                PermissionConstants.Pipeline.View,
                PermissionConstants.Pipeline.Create,
                PermissionConstants.Pipeline.Edit,
                PermissionConstants.NodalAnalysis.View,
                PermissionConstants.NodalAnalysis.Create,
                PermissionConstants.NodalAnalysis.RunIPR,
                PermissionConstants.NodalAnalysis.RunVLP,
                PermissionConstants.WellTestAnalysis.View,
                PermissionConstants.WellTestAnalysis.Create,
                PermissionConstants.WellTestAnalysis.RunPTA,
                PermissionConstants.ChokeAnalysis.View,
                PermissionConstants.ChokeAnalysis.Create,
                PermissionConstants.ChokeAnalysis.RunCorrelations,
                PermissionConstants.CompressorAnalysis.View,
                PermissionConstants.CompressorAnalysis.Create,
                PermissionConstants.CompressorAnalysis.RunCalculations,
                PermissionConstants.GasLift.View,
                PermissionConstants.GasLift.Create,
                PermissionConstants.GasLift.DesignValves,
                PermissionConstants.GasLift.Optimize,
                PermissionConstants.PumpPerformance.View,
                PermissionConstants.PumpPerformance.Create,
                PermissionConstants.PumpPerformance.RunAnalysis,
                PermissionConstants.SuckerRodPumping.View,
                PermissionConstants.SuckerRodPumping.Create,
                PermissionConstants.SuckerRodPumping.DesignRodString,
                PermissionConstants.PlungerLift.View,
                PermissionConstants.PlungerLift.Create,
                PermissionConstants.PlungerLift.Analyze,
                PermissionConstants.HydraulicPumps.View,
                PermissionConstants.HydraulicPumps.Create,
                PermissionConstants.HydraulicPumps.RunAnalysis,
                PermissionConstants.EnhancedRecovery.View,
                PermissionConstants.EnhancedRecovery.Create,
                PermissionConstants.EnhancedRecovery.ManageInjection,
                PermissionConstants.HSE.View,
                PermissionConstants.HSE.ReportIncident,
                PermissionConstants.HSE.ManageIncidents,
                PermissionConstants.HSE.ApproveIncident,
                PermissionConstants.HSE.ViewPermits,
                PermissionConstants.HSE.IssuePermit,
                PermissionConstants.HSE.ApprovePermit,
                PermissionConstants.HSE.ViewRiskAssessments,
                PermissionConstants.HSE.CreateRiskAssessment,
                PermissionConstants.HSE.ViewCertifications,
                PermissionConstants.HSE.ManageCertifications,
                PermissionConstants.Environmental.View,
                PermissionConstants.Environmental.Report,
                PermissionConstants.Environmental.ViewEmissions,
                PermissionConstants.Environmental.ReportEmissions,
                PermissionConstants.Regulatory.View,
                PermissionConstants.Regulatory.Submit,
                PermissionConstants.Regulatory.ViewCompliance,
                PermissionConstants.Exploration.View,
                PermissionConstants.Exploration.Create,
                PermissionConstants.Exploration.ViewSeismic,
                PermissionConstants.ProspectIdentification.View,
                PermissionConstants.ProspectIdentification.Create,
                PermissionConstants.ProspectIdentification.RunScreening,
                PermissionConstants.LeaseAcquisition.View,
                PermissionConstants.LeaseAcquisition.Create,
                PermissionConstants.LeaseAcquisition.Edit,
                PermissionConstants.DevelopmentPlanning.View,
                PermissionConstants.DevelopmentPlanning.Create,
                PermissionConstants.DevelopmentPlanning.RunFDP,
                PermissionConstants.Decommissioning.View,
                PermissionConstants.Decommissioning.Create,
                PermissionConstants.Decommissioning.PlanAbandonment,
                PermissionConstants.OilProperties.View,
                PermissionConstants.OilProperties.Create,
                PermissionConstants.OilProperties.RunCorrelations,
                PermissionConstants.GasProperties.View,
                PermissionConstants.GasProperties.Create,
                PermissionConstants.GasProperties.RunCalculations,
                PermissionConstants.FlashCalculations.View,
                PermissionConstants.FlashCalculations.Create,
                PermissionConstants.FlashCalculations.RunFlash,
                PermissionConstants.FlashCalculations.RunEOS,
                PermissionConstants.Drawing.View,
                PermissionConstants.Drawing.Create,
                PermissionConstants.Drawing.Edit,
                PermissionConstants.Drawing.Publish,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Create,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View,
                PermissionConstants.Dashboard.Customize,
                PermissionConstants.Workflow.View,
                PermissionConstants.Workflow.Start,
                PermissionConstants.Workflow.Execute,
                PermissionConstants.Workflow.Approve,
                PermissionConstants.Process.View,
                PermissionConstants.Process.ViewHistory,
                PermissionConstants.Process.ViewApprovals
            },

            [RoleDefinitions.PetroleumEngineer] = new[]
            {
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.Create,
                PermissionConstants.WellManagement.Edit,
                PermissionConstants.WellManagement.ViewWellStatus,
                PermissionConstants.WellManagement.UpdateWellStatus,
                PermissionConstants.Production.View,
                PermissionConstants.Production.Create,
                PermissionConstants.Production.Edit,
                PermissionConstants.Production.SubmitProduction,
                PermissionConstants.ProductionForecasting.View,
                PermissionConstants.ProductionForecasting.Create,
                PermissionConstants.ProductionForecasting.RunDCA,
                PermissionConstants.NodalAnalysis.View,
                PermissionConstants.NodalAnalysis.Create,
                PermissionConstants.NodalAnalysis.RunIPR,
                PermissionConstants.NodalAnalysis.RunVLP,
                PermissionConstants.WellTestAnalysis.View,
                PermissionConstants.WellTestAnalysis.Create,
                PermissionConstants.WellTestAnalysis.RunPTA,
                PermissionConstants.ChokeAnalysis.View,
                PermissionConstants.ChokeAnalysis.Create,
                PermissionConstants.ChokeAnalysis.RunCorrelations,
                PermissionConstants.GasLift.View,
                PermissionConstants.GasLift.Create,
                PermissionConstants.GasLift.DesignValves,
                PermissionConstants.GasLift.Optimize,
                PermissionConstants.PumpPerformance.View,
                PermissionConstants.PumpPerformance.Create,
                PermissionConstants.PumpPerformance.RunAnalysis,
                PermissionConstants.SuckerRodPumping.View,
                PermissionConstants.SuckerRodPumping.Create,
                PermissionConstants.SuckerRodPumping.DesignRodString,
                PermissionConstants.PlungerLift.View,
                PermissionConstants.PlungerLift.Create,
                PermissionConstants.PlungerLift.Analyze,
                PermissionConstants.HydraulicPumps.View,
                PermissionConstants.HydraulicPumps.Create,
                PermissionConstants.HydraulicPumps.RunAnalysis,
                PermissionConstants.OilProperties.View,
                PermissionConstants.OilProperties.Create,
                PermissionConstants.OilProperties.RunCorrelations,
                PermissionConstants.GasProperties.View,
                PermissionConstants.GasProperties.Create,
                PermissionConstants.GasProperties.RunCalculations,
                PermissionConstants.FlashCalculations.View,
                PermissionConstants.FlashCalculations.Create,
                PermissionConstants.FlashCalculations.RunFlash,
                PermissionConstants.FlashCalculations.RunEOS,
                PermissionConstants.Drawing.View,
                PermissionConstants.Drawing.Create,
                PermissionConstants.Drawing.Edit,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Create,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View,
                PermissionConstants.Dashboard.Customize
            },

            [RoleDefinitions.ReservoirEngineer] = new[]
            {
                PermissionConstants.Reservoir.View,
                PermissionConstants.Reservoir.Create,
                PermissionConstants.Reservoir.Edit,
                PermissionConstants.Reservoir.ViewReserves,
                PermissionConstants.Reservoir.UpdateReserves,
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.ViewWellStatus,
                PermissionConstants.Production.View,
                PermissionConstants.ProductionForecasting.View,
                PermissionConstants.ProductionForecasting.Create,
                PermissionConstants.ProductionForecasting.RunDCA,
                PermissionConstants.EnhancedRecovery.View,
                PermissionConstants.EnhancedRecovery.Create,
                PermissionConstants.EnhancedRecovery.ManageInjection,
                PermissionConstants.NodalAnalysis.View,
                PermissionConstants.NodalAnalysis.Create,
                PermissionConstants.NodalAnalysis.RunIPR,
                PermissionConstants.NodalAnalysis.RunVLP,
                PermissionConstants.WellTestAnalysis.View,
                PermissionConstants.WellTestAnalysis.Create,
                PermissionConstants.WellTestAnalysis.RunPTA,
                PermissionConstants.OilProperties.View,
                PermissionConstants.OilProperties.Create,
                PermissionConstants.OilProperties.RunCorrelations,
                PermissionConstants.GasProperties.View,
                PermissionConstants.GasProperties.Create,
                PermissionConstants.GasProperties.RunCalculations,
                PermissionConstants.FlashCalculations.View,
                PermissionConstants.FlashCalculations.Create,
                PermissionConstants.FlashCalculations.RunFlash,
                PermissionConstants.FlashCalculations.RunEOS,
                PermissionConstants.Drawing.View,
                PermissionConstants.Drawing.Create,
                PermissionConstants.Drawing.Edit,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Create,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View,
                PermissionConstants.Dashboard.Customize
            },

            ["ReservesEngineer"] = new[]
            {
                PermissionConstants.Reservoir.View,
                PermissionConstants.Reservoir.ViewReserves,
                PermissionConstants.Reservoir.UpdateReserves,
                PermissionConstants.Production.View,
                PermissionConstants.ProductionForecasting.View,
                PermissionConstants.ProductionForecasting.Create,
                PermissionConstants.ProductionForecasting.RunDCA,
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.ViewWellStatus,
                PermissionConstants.NodalAnalysis.View,
                PermissionConstants.NodalAnalysis.Create,
                PermissionConstants.NodalAnalysis.RunIPR,
                PermissionConstants.NodalAnalysis.RunVLP,
                PermissionConstants.WellTestAnalysis.View,
                PermissionConstants.WellTestAnalysis.Create,
                PermissionConstants.WellTestAnalysis.RunPTA,
                PermissionConstants.OilProperties.View,
                PermissionConstants.OilProperties.Create,
                PermissionConstants.GasProperties.View,
                PermissionConstants.GasProperties.Create,
                PermissionConstants.FlashCalculations.View,
                PermissionConstants.FlashCalculations.Create,
                PermissionConstants.FlashCalculations.RunFlash,
                PermissionConstants.FlashCalculations.RunEOS,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Create,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View,
                PermissionConstants.Dashboard.Customize
            },

            [RoleDefinitions.Administrator] = allPermissions,

            ["Admin"] = allPermissions,

            [RoleDefinitions.Auditor] = new[]
            {
                PermissionConstants.Admin.ViewAuditLogs,
                PermissionConstants.Accounting.View,
                PermissionConstants.Accounting.ViewReports,
                PermissionConstants.Tax.ViewProvision,
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.ViewWellStatus,
                PermissionConstants.Production.View,
                PermissionConstants.ProductionForecasting.View,
                PermissionConstants.Reservoir.View,
                PermissionConstants.Reservoir.ViewReserves,
                PermissionConstants.Drilling.View,
                PermissionConstants.Facilities.View,
                PermissionConstants.Pipeline.View,
                PermissionConstants.NodalAnalysis.View,
                PermissionConstants.WellTestAnalysis.View,
                PermissionConstants.ChokeAnalysis.View,
                PermissionConstants.CompressorAnalysis.View,
                PermissionConstants.GasLift.View,
                PermissionConstants.PumpPerformance.View,
                PermissionConstants.SuckerRodPumping.View,
                PermissionConstants.PlungerLift.View,
                PermissionConstants.HydraulicPumps.View,
                PermissionConstants.EnhancedRecovery.View,
                PermissionConstants.HSE.View,
                PermissionConstants.HSE.ViewPermits,
                PermissionConstants.HSE.ViewRiskAssessments,
                PermissionConstants.HSE.ViewCertifications,
                PermissionConstants.Environmental.View,
                PermissionConstants.Environmental.ViewEmissions,
                PermissionConstants.Regulatory.View,
                PermissionConstants.Regulatory.ViewCompliance,
                PermissionConstants.Exploration.View,
                PermissionConstants.Exploration.ViewSeismic,
                PermissionConstants.ProspectIdentification.View,
                PermissionConstants.LeaseAcquisition.View,
                PermissionConstants.DevelopmentPlanning.View,
                PermissionConstants.Decommissioning.View,
                PermissionConstants.OilProperties.View,
                PermissionConstants.GasProperties.View,
                PermissionConstants.FlashCalculations.View,
                PermissionConstants.Drawing.View,
                PermissionConstants.Reporting.View,
                PermissionConstants.Dashboard.View
            },

            ["SafetyOfficer"] = new[]
            {
                PermissionConstants.HSE.View,
                PermissionConstants.HSE.ReportIncident,
                PermissionConstants.HSE.ManageIncidents,
                PermissionConstants.HSE.ApproveIncident,
                PermissionConstants.HSE.ViewPermits,
                PermissionConstants.HSE.IssuePermit,
                PermissionConstants.HSE.ApprovePermit,
                PermissionConstants.HSE.ViewRiskAssessments,
                PermissionConstants.HSE.CreateRiskAssessment,
                PermissionConstants.HSE.ViewCertifications,
                PermissionConstants.HSE.ManageCertifications,
                PermissionConstants.Environmental.View,
                PermissionConstants.Environmental.Report,
                PermissionConstants.Environmental.ViewEmissions,
                PermissionConstants.Environmental.ReportEmissions,
                PermissionConstants.Regulatory.View,
                PermissionConstants.Regulatory.ViewCompliance,
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.ViewWellStatus,
                PermissionConstants.Production.View,
                PermissionConstants.Facilities.View,
                PermissionConstants.Facilities.ViewEquipment,
                PermissionConstants.Drawing.View,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Create,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View
            },

            ["Supervisor"] = new[]
            {
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.Create,
                PermissionConstants.WellManagement.Edit,
                PermissionConstants.WellManagement.Approve,
                PermissionConstants.WellManagement.ViewWellStatus,
                PermissionConstants.WellManagement.UpdateWellStatus,
                PermissionConstants.Production.View,
                PermissionConstants.Production.Create,
                PermissionConstants.Production.Edit,
                PermissionConstants.Production.Approve,
                PermissionConstants.Production.SubmitProduction,
                PermissionConstants.Production.AllocateProduction,
                PermissionConstants.Drilling.View,
                PermissionConstants.Drilling.Create,
                PermissionConstants.Drilling.Edit,
                PermissionConstants.Drilling.ViewDailyReports,
                PermissionConstants.Facilities.View,
                PermissionConstants.Facilities.Create,
                PermissionConstants.Facilities.Edit,
                PermissionConstants.Facilities.ViewEquipment,
                PermissionConstants.Pipeline.View,
                PermissionConstants.Pipeline.Create,
                PermissionConstants.Pipeline.Edit,
                PermissionConstants.HSE.View,
                PermissionConstants.HSE.ReportIncident,
                PermissionConstants.HSE.ManageIncidents,
                PermissionConstants.HSE.ApproveIncident,
                PermissionConstants.HSE.ViewPermits,
                PermissionConstants.HSE.IssuePermit,
                PermissionConstants.HSE.ApprovePermit,
                PermissionConstants.HSE.ViewRiskAssessments,
                PermissionConstants.HSE.CreateRiskAssessment,
                PermissionConstants.Environmental.View,
                PermissionConstants.Environmental.Report,
                PermissionConstants.Regulatory.View,
                PermissionConstants.Regulatory.Submit,
                PermissionConstants.Drawing.View,
                PermissionConstants.Drawing.Create,
                PermissionConstants.Drawing.Edit,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Create,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View,
                PermissionConstants.Dashboard.Customize
            },

            ["GateApprover"] = new[]
            {
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.Approve,
                PermissionConstants.Production.View,
                PermissionConstants.Production.Approve,
                PermissionConstants.Drilling.View,
                PermissionConstants.Drilling.Approve,
                PermissionConstants.Facilities.View,
                PermissionConstants.Facilities.Approve,
                PermissionConstants.HSE.View,
                PermissionConstants.HSE.ApproveIncident,
                PermissionConstants.HSE.ApprovePermit,
                PermissionConstants.Environmental.View,
                PermissionConstants.Environmental.Approve,
                PermissionConstants.Regulatory.View,
                PermissionConstants.Regulatory.Approve,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View
            },

            ["Compliance"] = new[]
            {
                PermissionConstants.HSE.View,
                PermissionConstants.HSE.ViewPermits,
                PermissionConstants.HSE.ViewRiskAssessments,
                PermissionConstants.HSE.ViewCertifications,
                PermissionConstants.Environmental.View,
                PermissionConstants.Environmental.ViewEmissions,
                PermissionConstants.Regulatory.View,
                PermissionConstants.Regulatory.ViewCompliance,
                PermissionConstants.Regulatory.ManageFilings,
                PermissionConstants.WellManagement.View,
                PermissionConstants.Production.View,
                PermissionConstants.Drilling.View,
                PermissionConstants.Facilities.View,
                PermissionConstants.Admin.ViewAuditLogs,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View
            },

            [RoleDefinitions.Approver] = new[]
            {
                PermissionConstants.WellManagement.View,
                PermissionConstants.WellManagement.Approve,
                PermissionConstants.Production.View,
                PermissionConstants.Production.Approve,
                PermissionConstants.ProductionForecasting.View,
                PermissionConstants.ProductionForecasting.Approve,
                PermissionConstants.Reservoir.View,
                PermissionConstants.Reservoir.Approve,
                PermissionConstants.Drilling.View,
                PermissionConstants.Drilling.Approve,
                PermissionConstants.Facilities.View,
                PermissionConstants.Facilities.Approve,
                PermissionConstants.Pipeline.View,
                PermissionConstants.Pipeline.Approve,
                PermissionConstants.WellTestAnalysis.View,
                PermissionConstants.WellTestAnalysis.Approve,
                PermissionConstants.EnhancedRecovery.View,
                PermissionConstants.EnhancedRecovery.Approve,
                PermissionConstants.Accounting.View,
                PermissionConstants.Accounting.ApproveJournal,
                PermissionConstants.HSE.View,
                PermissionConstants.HSE.ApproveIncident,
                PermissionConstants.HSE.ApprovePermit,
                PermissionConstants.Environmental.View,
                PermissionConstants.Environmental.Approve,
                PermissionConstants.Regulatory.View,
                PermissionConstants.Regulatory.Approve,
                PermissionConstants.Drawing.View,
                PermissionConstants.Drawing.Approve,
                PermissionConstants.Reporting.View,
                PermissionConstants.Reporting.Export,
                PermissionConstants.Dashboard.View,
                PermissionConstants.Workflow.View,
                PermissionConstants.Workflow.Approve,
                PermissionConstants.Process.View,
                PermissionConstants.Process.ViewApprovals
            }
        };
    }
}
