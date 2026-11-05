using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public async Task AssignedActionsHttpEnforcesCurrentCompanyFieldAndAppAuthorityBeforePrivateRead()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile);
        using var boundary = new Boundary(f, profile);
        var access = new UserAssetAccessService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object,
            new RepositoryApplicationAuthorizationReader(f.Gate), Mock.Of<IApplicationRolePermissionStore>(),
            new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()),
            () => f.Gate.ResolveAsync(), _ => f.Policy, Mock.Of<ICompanyFieldInterestReview>());
        var contractor = new Mock<IContractorManagementService>();
        var identities = contractor.As<ICompanyPersonnelIdentityOwner>();
        var now = DateTime.UtcNow; var revision = new string('A', 64);
        var catalogReads = 0; var incidentReads = 0; var actionReads = 0;
        var missingIdentity = false; var revokeDuringRead = false;
        var view = new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.View" };
        identities.Setup(x => x.ReadPersonnelIdentitiesAsync(It.IsAny<CompanyPersonnelPolicyContext>(), It.IsAny<Func<Task>>()))
            .Returns(async (CompanyPersonnelPolicyContext context, Func<Task> guard) =>
            {
                await guard(); ++catalogReads;
                var link = new CompanyPersonnelIdentityVersion(context.Company, context.Database,
                    CompanyPersonnelIdentityCommand.RecordIdFor(context.Company, context.ActorUserId, 1), context.ActorUserId, "person", "EMPLOYEE", 1,
                    now.AddDays(-2), now.AddDays(2), "DO_NOT_DISCLOSE_IDENTITY_REFERENCE", "identity-maker", now.AddDays(-3), revision);
                return CompanyPersonnelIdentityCatalog.Create(context, missingIdentity ? [] : [link], DateTime.UtcNow);
            });
        identities.Setup(x => x.ReadIdentityPersonAsync(It.IsAny<CompanyPersonnelPolicyContext>(), "person", It.IsAny<Func<Task>>()))
            .Returns(async (CompanyPersonnelPolicyContext context, string person, Func<Task> guard) =>
            {
                await guard();
                return new CompanyNativePersonFacts(context.Company, person, "NATIVE_PERSON_PRIVATE_NAME", "EMPLOYEE", now.AddDays(-5), now.AddDays(5),
                    new(context.Company.BusinessAssociateId, person, 1, "Technician", "ACTIVE", now.AddDays(-5), now.AddDays(5), revision), revision);
            });
        var hse = new Mock<IFieldHSEService>(MockBehavior.Strict);
        hse.Setup(x => x.GetIncidentsAsync(null)).Returns(async () =>
        {
            ++incidentReads;
            if (revokeDuringRead)
            {
                revokeDuringRead = false; f.Db.RoleClaims.Remove(view); await f.Db.SaveChangesAsync();
            }
            return new List<HSEIncidentRecord> { new() { IncidentId = "case/شركة?one#2", FieldId = "A", CurrentState = IncidentState.PendingCorrectiveAction } };
        });
        hse.Setup(x => x.GetCorrectiveActionsAsync("case/شركة?one#2")).Returns(() =>
        {
            ++actionReads;
            return Task.FromResult(new List<CAStatus>
            {
                new(1, "PRIVATE_CURRENT_ACTOR_ACTION", "CORRECTIVE", "OPEN", now.AddHours(-2), true, "person", null),
                new(2, "PRIVATE_OTHER_PERSON_ACTION", "CORRECTIVE", "OPEN", now.AddHours(-2), true, "another-person", null),
                new(3, "PRIVATE_FUTURE_ACTION", "CORRECTIVE", "OPEN", now.AddHours(2), false, "person", null),
                new(4, "PRIVATE_COMPLETED_ACTION", "CORRECTIVE", "COMPLETED", now.AddHours(-2), true, "person", now.AddHours(-1))
            });
        });
        boundary.Orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        void Configure(IServiceCollection services)
        {
            services.AddSingleton(f.CompanySelection);
            var accounts = new RepositoryAccessService(f.Db);
            services.AddSingleton<IRepositoryAccessService>(accounts);
            services.AddSingleton(new RepositoryCompanyContextResolver(accounts));
            services.AddSingleton(new ModuleBindingService(f.Db, f.Editor.Object));
            services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
            services.AddSingleton(new CompanyHseAssignedActionReader(f.Gate, profile, boundary.Orchestrator.Object, contractor.Object));
        }
        await using var host = await HttpHost.Start(f, profile, boundary, true, typeof(HseAssignedActionsController), Configure,
            accessOverride: access, companyMiddleware: true);
        var url = host.Url + "/api/field/current/hse/assigned-overdue-actions";
        using (var missingSelectors = await host.Http.GetAsync(url)) Assert.Equal(HttpStatusCode.BadRequest, missingSelectors.StatusCode);
        Assert.Equal(0, catalogReads); Assert.Equal(0, incidentReads);
        host.Http.DefaultRequestHeaders.Add(CompanyRequestHeaders.Tenant, f.Gate.Company.Scope.TenantId);
        host.Http.DefaultRequestHeaders.Add(CompanyRequestHeaders.Company, "unavailable-company");
        using (var foreign = await host.Http.GetAsync(url)) Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
        Assert.Equal(0, catalogReads); Assert.Equal(0, incidentReads);
        host.Http.DefaultRequestHeaders.Remove(CompanyRequestHeaders.Company);
        host.Http.DefaultRequestHeaders.Add(CompanyRequestHeaders.Company, "company");
        using (var noPermission = await host.Http.GetAsync(url)) Assert.Equal(HttpStatusCode.Forbidden, noPermission.StatusCode);
        Assert.Equal(0, catalogReads); Assert.Equal(0, incidentReads);
        f.Db.RoleClaims.Add(view); await f.Db.SaveChangesAsync();
        async Task ReadCurrent()
        {
            using var response = await host.Http.GetAsync(url);
            Assert.True(response.StatusCode == HttpStatusCode.OK, response.StatusCode + " " + await response.Content.ReadAsStringAsync());
            Assert.True(response.Headers.CacheControl?.NoStore == true);
            var result = (await response.Content.ReadFromJsonAsync<CompanyHseAssignedActions>())!; result.Require();
            Assert.Equal(f.Gate.Company.Scope, result.Context.Company); Assert.Equal("actor", result.Context.ActorUserId); Assert.Equal("A", result.Context.FieldId);
            Assert.Equal("PRIVATE_CURRENT_ACTOR_ACTION", Assert.Single(result.Actions).Description);
            var text = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("PRIVATE_OTHER_PERSON_ACTION", text); Assert.DoesNotContain("PRIVATE_FUTURE_ACTION", text);
            Assert.DoesNotContain("PRIVATE_COMPLETED_ACTION", text); Assert.DoesNotContain("NATIVE_PERSON_PRIVATE_NAME", text);
            Assert.DoesNotContain("DO_NOT_DISCLOSE_IDENTITY_REFERENCE", text);
        }
        await ReadCurrent();
        var reads = catalogReads;
        using (var otherField = await host.Http.GetAsync(url + "?fieldId=B")) Assert.Equal(HttpStatusCode.Conflict, otherField.StatusCode);
        Assert.Equal(reads, catalogReads);
        missingIdentity = true; var before = incidentReads;
        using (var unmapped = await host.Http.GetAsync(url)) Assert.Equal(HttpStatusCode.Conflict, unmapped.StatusCode);
        Assert.Equal(before, incidentReads); missingIdentity = false;
        revokeDuringRead = true; var actionsBefore = actionReads;
        using (var revoked = await host.Http.GetAsync(url))
        {
            Assert.Equal(HttpStatusCode.Forbidden, revoked.StatusCode);
            Assert.DoesNotContain("PRIVATE_CURRENT_ACTOR_ACTION", await revoked.Content.ReadAsStringAsync());
        }
        Assert.Equal(actionsBefore, actionReads);
        view = new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.View" };
        f.Db.RoleClaims.Add(view); await f.Db.SaveChangesAsync();
        var grant = await f.Db.Set<AppUserAssetAccess>().SingleAsync(row => row.AssetType == "FIELD" && row.AssetId == "A");
        grant.IsActive = false; await f.Db.SaveChangesAsync(); reads = catalogReads;
        using (var noField = await host.Http.GetAsync(url)) Assert.Equal(HttpStatusCode.BadRequest, noField.StatusCode);
        Assert.Equal(reads, catalogReads); grant.IsActive = true; await f.Db.SaveChangesAsync();
        await ReadCurrent();
        await using (var anonymous = await HttpHost.Start(f, profile, boundary, false, typeof(HseAssignedActionsController), Configure,
            accessOverride: access, companyMiddleware: true))
        {
            reads = catalogReads;
            using var denied = await anonymous.Http.GetAsync(anonymous.Url + "/api/field/current/hse/assigned-overdue-actions");
            Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode); Assert.Equal(reads, catalogReads);
        }
        Assert.Empty(f.Writes);
    }
}
#endif
