#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.Permits;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PermitsAndApplications.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public Task PermitProviderHttpUsesFreshCompanyAuthorityAndPrivateSameOriginalRecovery() =>
        new PermitProviderQualificationTests().WithInstalledLedgerAsync(async (_, native) =>
    {
        using var f = await Fixture.Create();
        f.Rows.Add(new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" });
        await f.Grant("FIELD", "A", "WRITE", inherit: true);
        await f.Select("other-company");
        await f.Grant("FIELD", "A", "WRITE", inherit: true);
        await f.Select("company");
        foreach (var permission in new[] { PermissionConstants.HSE.ViewPermits, PermissionConstants.HSE.IssuePermit })
            f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = permission });
        var connection = Assert.IsType<ConnectionProperties>(native.Dataconnection.ConnectionProp);
        f.Editor.Object.ConfigEditor.DataConnections.Add(connection);
        f.Editor.Setup(editor => editor.GetDataSource(connection.ConnectionName)).Returns(native);
        f.Editor.Setup(editor => editor.OpenDataSource(connection.ConnectionName)).Returns(System.Data.ConnectionState.Open);
        f.Db.Add(new AppModuleConnection { TenantId = "tenant", ConnectionKey = "PERMIT-PROVIDER", AssignedByUserId = "actor", PhysicalTargetKey = ModuleDatabaseTarget.Key(f.Editor.Object, connection) });
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "PERMITS", ConnectionName = connection.ConnectionName, ConnectionKey = "PERMIT-PROVIDER", ConcurrencyStamp = "permit-http-v1" });
        await f.Db.SaveChangesAsync();
        var columns = new CommonColumnHandler();
        f.Columns.Setup(handler => handler.PrepareForInsert(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) => columns.PrepareForInsert(row, actor));
        f.Columns.Setup(handler => handler.PrepareForUpdate(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) => columns.PrepareForUpdate(row, actor));
        f.Defaults.Setup(defaults => defaults.GetActiveIndicatorYes()).Returns("Y");
        foreach (var type in new[] { typeof(PERMIT_APPLICATION), typeof(PERMIT_STATUS_HISTORY), typeof(PERMIT_APPLICATION_COMMAND) })
        {
            var key = type.Name + "_ID";
            f.Metadata.Setup(metadata => metadata.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata { TableName = type.Name, EntityTypeName = type.FullName!, PrimaryKeyColumn = key });
            f.Metadata.Setup(metadata => metadata.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([key]);
        }
        const string localId = "local-نموذج/?A";
        foreach (var (id, company, applicant) in new[] { (localId, "company", "Local applicant"), ("foreign", "other-company", "PRIVATE OTHER COMPANY") })
        {
            var row = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = id, OWNER_TENANT_ID = "tenant", OWNER_COMPANY_ID = company, FIELD_ID = "A", APPLICANT_ID = applicant, ACTIVE_IND = "Y" };
            columns.PrepareForInsert(row, "actor");
            Assert.Equal(Errors.Ok, native.InsertEntity(nameof(PERMIT_APPLICATION), row).Flag);
        }
        var placeholder = Profile(f);
        using var boundary = new Boundary(f, placeholder);
        await using var host = await HttpHost.Start(f, placeholder, boundary, true, typeof(PermitsController), services => RegisterPermitProviderHttp(services, f), companyMiddleware: true);
        // Prepare each company's actual stored preference in a separately validated DI scope.
        foreach (var company in new[] { "company", "other-company" })
        {
            using var scope = host.Services.CreateScope();
            var selected = await scope.ServiceProvider.GetRequiredService<RepositoryCompanyContextResolver>().ResolveAsync("actor", new("tenant", company));
            Assert.NotNull(selected);
            scope.ServiceProvider.GetRequiredService<RepositoryTenantContext>().SelectCompany(selected);
            await Save(scope.ServiceProvider.GetRequiredService<RepositoryUserProfileService>());
        }
        async Task<HttpResponseMessage> Send(HttpMethod method, string suffix, string? company, object? body = null)
        {
            using var request = new HttpRequestMessage(method, host.Url + "/api/field/current/permits/" + suffix);
            if (company is not null) { request.Headers.Add(CompanyRequestHeaders.Tenant, "tenant"); request.Headers.Add(CompanyRequestHeaders.Company, company); }
            if (body is not null) request.Content = JsonContent.Create(body, body.GetType());
            return await host.Http.SendAsync(request);
        }
        async Task<PermitApplicationSourceReview> Read(string company, string id)
        {
            using var response = await Send(HttpMethod.Get, "application-review?applicationId=" + Uri.EscapeDataString(id), company);
            Assert.True(response.IsSuccessStatusCode, $"Permit review failed {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            return (await response.Content.ReadFromJsonAsync<PermitApplicationSourceReview>())!;
        }
        using (var missing = await Send(HttpMethod.Get, "application-review?applicationId=foreign", null)) Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        var reviewed = await Read("company", localId);
        Assert.Equal("company", reviewed.Context.Field.Company.BusinessAssociateId);
        Assert.Equal("other-company", (await Read("other-company", "foreign")).Context.Field.Company.BusinessAssociateId);
        foreach (var (company, id, secret) in new[] { ("company", "foreign", "PRIVATE OTHER COMPANY"), ("other-company", localId, "Local applicant") })
        {
            using var response = await Send(HttpMethod.Get, "application-review?applicationId=" + Uri.EscapeDataString(id), company);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.DoesNotContain(secret, await response.Content.ReadAsStringAsync());
        }
        var original = new PermitApplicationCommand(Guid.NewGuid(), reviewed, "SUBMIT", "Reviewed through the current company API");
        using (var foreign = await Send(HttpMethod.Post, "application-command/recovery", "other-company", original)) Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
        using var recorded = await Send(HttpMethod.Post, "application-command", "company", original);
        Assert.True(recorded.IsSuccessStatusCode, $"Permit command failed {(int)recorded.StatusCode}: {await recorded.Content.ReadAsStringAsync()}");
        var outcome = (await recorded.Content.ReadFromJsonAsync<PermitApplicationCommandOutcome>())!;
        outcome.Require(); Assert.Equal(original, outcome.Original);
        f.Db.RoleClaims.Remove(await f.Db.RoleClaims.SingleAsync(claim => claim.ClaimValue == PermissionConstants.HSE.IssuePermit));
        await f.Db.SaveChangesAsync();
        var deniedOriginal = original with { IntentId = Guid.NewGuid() };
        using (var denied = await Send(HttpMethod.Post, "application-command", "company", deniedOriginal)) Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using (var absent = await Send(HttpMethod.Post, "application-command/recovery", "company", deniedOriginal))
        {
            Assert.Equal(HttpStatusCode.OK, absent.StatusCode);
            var result = (await absent.Content.ReadFromJsonAsync<PermitApplicationCommandRecovery>())!;
            result.Require(deniedOriginal); Assert.Null(result.Outcome);
        }
        using (var recovery = await Send(HttpMethod.Post, "application-command/recovery", "company", original))
        {
            Assert.Equal(HttpStatusCode.OK, recovery.StatusCode);
            var result = (await recovery.Content.ReadFromJsonAsync<PermitApplicationCommandRecovery>())!;
            result.Require(original); Assert.Equal(outcome, result.Outcome);
        }
        using (var replay = await Send(HttpMethod.Post, "application-command", "company", original))
        {
            Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
            Assert.Equal(outcome, await replay.Content.ReadFromJsonAsync<PermitApplicationCommandOutcome>());
        }
        var current = await Read("company", localId);
        Assert.Single(current.ReadHistory());
        Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted, current.ReadApplication().STATUS);
        using (var foreign = await Send(HttpMethod.Post, "application-command/recovery", "other-company", original)) Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
        Assert.Empty(f.Writes); // Core FIELD evidence is read-only; native permit writes use the real provider.
    });

    private static void RegisterPermitProviderHttp(IServiceCollection services, Fixture f)
    {
        services.AddSingleton<RepositoryDbContext>(f.Db);
        services.AddSingleton<IDMEEditor>(f.Editor.Object);
        services.AddSingleton(f.Columns.Object); services.AddSingleton(f.Defaults.Object); services.AddSingleton(f.Metadata.Object);
        services.AddScoped<IRepositoryAccessService, RepositoryAccessService>();
        services.AddScoped<RepositoryTenantContext>(); services.AddScoped<RepositoryCompanyContextResolver>();
        services.AddScoped<ModuleConnectionResolver>(); services.AddScoped<ModuleBindingService>(); services.AddScoped<CompanyModuleConnection>();
        services.AddScoped<IApplicationAuthorizationReader, RepositoryApplicationAuthorizationReader>();
        services.AddSingleton<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        services.AddScoped<IUserAssetAccessStore>(sp => new RepositoryAssetAccessStore(f.Db, sp.GetRequiredService<CompanyModuleConnection>(), Mock.Of<IAssetGrantTargetVerifier>()));
        services.AddScoped<ICompanyFieldEligibility>(sp => new PPDMCompanyFieldEligibility(sp.GetRequiredService<CompanyModuleConnection>(), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, Options.Create(f.EligibilityConfiguration)));
        services.AddScoped<IPPDMRowAccessPolicy>(sp => new CompanyPPDMRowAccessPolicy(sp.GetRequiredService<CompanyModuleConnection>(), sp.GetRequiredService<IUserAssetAccessStore>(), f.Metadata.Object, sp.GetRequiredService<ICompanyFieldEligibility>(), f.Editor.Object, f.Columns.Object, f.Defaults.Object, Options.Create(f.EligibilityConfiguration)));
        services.AddScoped<IPPDM39DataService>(sp => new PPDM39GenericDataService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, sp.GetRequiredService<CompanyModuleConnection>(), NullLogger<PPDM39GenericDataService>.Instance, sp.GetRequiredService<IPPDMRowAccessPolicy>()));
        services.AddScoped<RepositoryUserProfileService>();
        services.AddScoped<ICompanyFieldSelectionService>(sp => sp.GetRequiredService<RepositoryUserProfileService>());
        services.AddScoped<ICompanyFieldSelectionAuthority>(sp => sp.GetRequiredService<RepositoryUserProfileService>());
        services.AddScoped<ICompanyPermitApplicationDataAccess>(sp => new CompanyPermitApplicationDataAccess(sp.GetRequiredService<CompanyModuleConnection>(), sp.GetRequiredService<ModuleConnectionResolver>(), sp.GetRequiredService<ICompanyFieldSelectionService>(), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, sp.GetRequiredService<IUserAssetAccessStore>(), sp.GetRequiredService<IPPDMRowAccessPolicy>()));
        services.AddScoped<IPermitApplicationLifecycleService, PermitApplicationLifecycleService>();
        services.AddSingleton(Mock.Of<IPermitApplicationWorkflowService>()); services.AddSingleton(Mock.Of<IPermitComplianceCheckService>()); services.AddSingleton(Mock.Of<IPermitStatusHistoryService>());
        services.AddScoped<IAccessControlService>(sp => new UserAssetAccessService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, sp.GetRequiredService<IApplicationAuthorizationReader>(), Mock.Of<IApplicationRolePermissionStore>(), sp.GetRequiredService<IUserAssetAccessStore>(), () => sp.GetRequiredService<CompanyModuleConnection>().ResolveAsync(), _ => sp.GetRequiredService<IPPDMRowAccessPolicy>(), Mock.Of<ICompanyFieldInterestReview>()));
    }
}
#endif
