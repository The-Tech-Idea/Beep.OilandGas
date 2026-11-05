using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

namespace Beep.OilandGas.ApiService.Tests.Infrastructure;

internal static class TestCompanyModules
{
    internal static async Task<CompanyModuleConnection> CreateAsync(RepositoryDbContext db, string actor,
        string organization = "company", string tenantId = "tenant", ConnectionProperties? connectionTarget = null, Action<RepositoryTenantContext>? selectionCaptured = null)
    {
        if (!await db.Set<AppTenant>().AnyAsync(value => value.TenantId == tenantId)) db.Add(new AppTenant { TenantId = tenantId });
        if (!await db.Set<AppCompany>().AnyAsync(value => value.TenantId == tenantId && value.BusinessAssociateId == organization))
            db.Add(new AppCompany { TenantId = tenantId, BusinessAssociateId = organization });
        foreach (var user in await db.Users.ToArrayAsync())
            if (!await db.Set<AppCompanyMembership>().AnyAsync(value => value.TenantId == tenantId && value.BusinessAssociateId == organization && value.UserId == user.Id))
                db.Add(new AppCompanyMembership { TenantId = tenantId, BusinessAssociateId = organization, UserId = user.Id, GrantedByUserId = actor });
        foreach (var role in await db.UserRoles.ToArrayAsync())
            if (!await db.Set<AppUserRoleExtension>().AnyAsync(value => value.TenantId == tenantId && value.BusinessAssociateId == organization && value.UserId == role.UserId && value.RoleId == role.RoleId))
                db.Add(new AppUserRoleExtension { TenantId = tenantId, BusinessAssociateId = organization, UserId = role.UserId, RoleId = role.RoleId });
        var configured = connectionTarget ?? new ConnectionProperties { ConnectionName = "selected-" + tenantId, DatabaseType = DataSourceType.SqlServer, Host = "server", Database = "assets-" + tenantId };
        var config = new Mock<IConfigEditor>(); config.SetupGet(value => value.DataConnections).Returns([configured]);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(value => value.ConfigEditor).Returns(config.Object);
        if (!await db.ModuleDatabases.AnyAsync(value => value.TenantId == tenantId))
        {
            db.Add(new AppModuleConnection { TenantId = tenantId, ConnectionKey = configured.ConnectionName.ToUpperInvariant(), AssignedByUserId = actor, PhysicalTargetKey = ModuleDatabaseTarget.Key(editor.Object, configured) });
            db.Add(new ModuleDatabaseBinding { TenantId = tenantId, ModuleId = "PPDM_CORE", ConnectionName = configured.ConnectionName, ConnectionKey = configured.ConnectionName.ToUpperInvariant(), ConcurrencyStamp = "v1" });
        }
        await db.SaveChangesAsync();
        var companies = new RepositoryCompanyContextResolver(new RepositoryAccessService(db));
        var selected = await companies.ResolveAsync(actor, new(tenantId, organization)) ?? throw new InvalidOperationException("The test actor must be an active company member.");
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(selected);
        selectionCaptured?.Invoke(tenant);
        return new(new(db, editor.Object, tenant), tenant, companies);
    }
}
