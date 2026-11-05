using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Repository;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class RepositoryCompanyAccessTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly CompanyScope ScopeA = new("A", "operator");
    private static readonly CompanyScope ScopeB = new("B", "operator");

    [Fact]
    public async Task Initial_company_boundary_grants_only_its_creator_a_scoped_administrator_role()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Add(new AppUserRoleExtension { UserId = "user", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime }); await fixture.Db.SaveChangesAsync();
        var scope = await fixture.Assignments(services).CreateInitialCompanyAsync("NEW", "New tenant", "BA-1", "Initial setup", "11111111111111111111111111111111", "user");
        Assert.Equal(new CompanyScope("NEW", "BA-1"), scope);
        Assert.Contains("Administrator", (await fixture.Access.GetCompanyAccessAsync("user", scope))!.Roles);
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("other", scope));
        Assert.Equal("Initial setup", (await fixture.Db.Set<AppUserRoleExtension>().SingleAsync(x => x.TenantId == "NEW")).AssignmentReason);
        Assert.Equal(scope, await fixture.Assignments(services).CreateInitialCompanyAsync("NEW", "New tenant", "BA-1", "Initial setup", "11111111111111111111111111111111", "user"));
        var mismatch = await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services).CreateInitialCompanyAsync("NEW", "New tenant", "BA-1", "Different", "11111111111111111111111111111111", "user"));
        Assert.Equal(RefusalKind.Conflict, mismatch.Kind);
        Assert.Single(await fixture.Db.Set<AppUserRoleExtension>().Where(x => x.TenantId == "NEW").ToArrayAsync());
    }
    [Fact]
    public async Task Initial_company_boundary_refuses_existing_tenant_without_creating_membership()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Add(new AppUserRoleExtension { UserId = "user", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime }); await fixture.Db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services).CreateInitialCompanyAsync("A", "Other", "BA-1", "Setup", "11111111111111111111111111111111", "user"));
        Assert.Equal(RefusalKind.Conflict, error.Kind);
        Assert.False(await fixture.Db.Set<AppCompany>().AnyAsync(x => x.TenantId == "A" && x.BusinessAssociateId == "BA-1"));
    }
    [Fact]
    public async Task Initial_company_boundary_adds_another_company_without_cross_company_roles()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Add(new AppUserRoleExtension { UserId = "user", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime }); await fixture.Db.SaveChangesAsync();
        var service = fixture.Assignments(services);
        var added = await service.CreateInitialCompanyAsync("A", "Tenant A", "BA-NEW", "New company", "22222222222222222222222222222222", "user");
        Assert.Equal(new CompanyScope("A", "BA-NEW"), added);
        Assert.Contains("Administrator", (await fixture.Access.GetCompanyAccessAsync("user", added))!.Roles);
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!.Roles);
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", new("B", "BA-NEW")));
        Assert.Equal(added, await service.CreateInitialCompanyAsync("A", "Tenant A", "BA-NEW", "New company", "22222222222222222222222222222222", "user"));
        Assert.Single(await fixture.Db.Set<AppCompany>().Where(x => x.TenantId == "A" && x.BusinessAssociateId == "BA-NEW").ToArrayAsync());
    }
    [Fact]
    public async Task Initial_company_boundary_refuses_existing_company_and_inactive_tenant()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Add(new AppUserRoleExtension { UserId = "user", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime });
        (await fixture.Db.Set<AppTenant>().FindAsync("B"))!.IsActive = false; await fixture.Db.SaveChangesAsync();
        var service = fixture.Assignments(services);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.CreateInitialCompanyAsync("A", "Tenant A", "operator", "Setup", "22222222222222222222222222222222", "user"))).Kind);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.CreateInitialCompanyAsync("B", "Tenant B", "BA-NEW", "Setup", "22222222222222222222222222222222", "user"))).Kind);
        Assert.False(await fixture.Db.Set<AppCompany>().AnyAsync(x => x.TenantId == "B" && x.BusinessAssociateId == "BA-NEW"));
    }
    [Fact]
    public async Task Initial_company_boundary_requires_active_repository_administrator()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        (await fixture.Db.Users.FindAsync("user"))!.IsActive = false; await fixture.Db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services).CreateInitialCompanyAsync("NEW", "New", "BA-1", "Setup", "11111111111111111111111111111111", "user"));
        Assert.Equal(RefusalKind.Forbidden, error.Kind); Assert.False(await fixture.Db.Set<AppTenant>().AnyAsync(x => x.TenantId == "NEW"));
    }
    [Fact]
    public async Task Initial_company_boundary_refuses_an_unclassified_repository_grant()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        var error = await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services).CreateInitialCompanyAsync("NEW", "New", "BA-1", "Setup", "11111111111111111111111111111111", "user"));
        Assert.Equal(RefusalKind.Conflict, error.Kind); Assert.False(await fixture.Db.Set<AppTenant>().AnyAsync(x => x.TenantId == "NEW"));
        Assert.Contains("Administrator", (await fixture.Access.GetAccessAsync("user"))!.Roles);
    }
    [Fact]
    public async Task Initial_company_boundary_rolls_back_if_scoped_role_persistence_fails()
    {
        using var fixture = new Fixture(new FailNewCompanyGrant()); using var services = fixture.IdentityServices();
        fixture.Db.Add(new AppUserRoleExtension { UserId = "user", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime }); await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Assignments(services).CreateInitialCompanyAsync("NEW", "New", "BA-1", "Setup", "11111111111111111111111111111111", "user"));
        fixture.Db.ChangeTracker.Clear();
        Assert.False(await fixture.Db.Set<AppTenant>().AnyAsync(x => x.TenantId == "NEW"));
        Assert.False(await fixture.Db.Set<AppCompanyMembership>().AnyAsync(x => x.TenantId == "NEW"));
    }
    private sealed class FailNewCompanyGrant : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<AppUserRoleExtension>().Any(x => x.State == EntityState.Added && x.Entity.TenantId == "NEW"))
                throw new DbUpdateException("Injected failure");
            return ValueTask.FromResult(result);
        }
    }

    [Fact]
    public async Task Initial_company_review_is_read_only_and_confirms_only_the_original_request()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Add(new AppUserRoleExtension { UserId = "user", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime }); await fixture.Db.SaveChangesAsync();
        var service = fixture.Assignments(services);
        var request = new InitialCompanyProvisionRequest("NEW", "New", "BA", "Setup", "11111111111111111111111111111111");
        var before = await service.ReviewInitialCompanyAsync(request, "user"); Assert.Equal("NotRecorded", before.Status); Assert.Null(before.Company);
        Assert.False(await fixture.Db.Set<AppTenant>().AnyAsync(x => x.TenantId == "NEW"));
        await service.CreateInitialCompanyAsync(request.TenantId, request.TenantName, request.BusinessAssociateId, request.Reason, request.IntentId, "user");
        fixture.Db.ChangeTracker.Clear(); var roleStamp = (await fixture.Db.Roles.FindAsync("admin"))!.ConcurrencyStamp;
        var after = await service.ReviewInitialCompanyAsync(request, "user"); Assert.Equal("Recorded", after.Status); Assert.Equal(new CompanyScope("NEW", "BA"), after.Company);
        Assert.Equal(roleStamp, (await fixture.Db.Roles.FindAsync("admin"))!.ConcurrencyStamp);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.ReviewInitialCompanyAsync(request with { Reason = "Other" }, "user"))).Kind);
    }

    [Fact]
    public async Task Initial_company_endpoint_uses_the_resolved_actor_and_replays_original_request()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Add(new AppUserRoleExtension { UserId = "user", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime }); await fixture.Db.SaveChangesAsync();
        var controller = new Beep.OilandGas.ApiService.Controllers.Identity.InitialCompanyProvisionController(fixture.Assignments(services));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
        controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("party_id", "user")], "test"));
        var request = new InitialCompanyProvisionRequest("NEW", "New", "BA-1", "Initial setup", "11111111111111111111111111111111");
        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.Create(request, default));
        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.Create(request, default));
        Assert.Single(await fixture.Db.Set<AppCompanyMembership>().Where(x => x.TenantId == "NEW").ToArrayAsync());
        var guard = Assert.Single(typeof(Beep.OilandGas.ApiService.Controllers.Identity.InitialCompanyProvisionController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true).Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
        Assert.Equal("Administrator", guard.Roles);
    }

    [Fact]
    public async Task New_company_membership_creates_only_the_selected_scoped_grant_and_retries_exactly()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Users.Add(new OilGasUser { Id = "new-user", UserName = "new-user", IsActive = true }); await fixture.Db.SaveChangesAsync();
        var service = fixture.Assignments(services); var id = "22222222222222222222222222222222";
        var grant = await service.CreateCompanyMembershipAsync(ScopeB, "new-user", "engineer", "Approved hire", id, "user");
        Assert.Equal(id, grant.UserRoleId); Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("new-user", ScopeB))!.Roles);
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("new-user", ScopeA)); Assert.Empty((await fixture.Access.GetAccessAsync("new-user"))!.Roles);
        Assert.Equal(id, (await service.CreateCompanyMembershipAsync(ScopeB, "new-user", "engineer", "Approved hire", id, "user")).UserRoleId);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.CreateCompanyMembershipAsync(ScopeB, "new-user", "engineer", "Changed", id, "user"))).Kind);
    }
    [Fact]
    public async Task MembershipEndpointUsesAuthenticatedActorAndOriginalRequest()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Users.Add(new OilGasUser { Id = "new-user", UserName = "new-user", IsActive = true }); await fixture.Db.SaveChangesAsync();
        var controller = new Beep.OilandGas.ApiService.Controllers.Identity.RoleAssignmentController(fixture.Assignments(services),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.ApiService.Controllers.Identity.RoleAssignmentController>.Instance,
            Moq.Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
        controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("party_id", "user")], "test"));
        var request = new CompanyMembershipRequest("new-user", "engineer", "Approved hire", "22222222222222222222222222222222");
        var first = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.CreateCompanyMembership(ScopeB.TenantId, ScopeB.BusinessAssociateId, request, default));
        var role = Assert.IsType<RepositoryUserRole>(first.Value);
        Assert.Equal("user", role.GrantedByUserId); Assert.Equal(request.IntentId, role.UserRoleId);
        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.CreateCompanyMembership(ScopeB.TenantId, ScopeB.BusinessAssociateId, request, default));
        Assert.Single(await fixture.Db.Set<AppCompanyMembership>().Where(x => x.UserId == "new-user").ToArrayAsync());
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("new-user", ScopeA));
        var reviewed = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.ReviewCompanyMemberAccess(ScopeB.TenantId, ScopeB.BusinessAssociateId, "new-user", default));
        var access = Assert.IsType<CompanyMemberAccessReview>(reviewed.Value);
        Assert.Equal(ScopeB, access.Company); Assert.Equal("new-user", access.UserId); Assert.Contains("Engineer", access.Access!.Roles);
        Assert.Equal("user", access.Membership!.GrantedByUserId); Assert.Null(access.Membership.EffectiveToUtc);
        var extra = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.AssignCompanyRole(ScopeB.TenantId, ScopeB.BusinessAssociateId, "new-user", new AssignCompanyRoleRequest("admin", "Approved additional role"), default));
        var extraRole = Assert.IsType<RepositoryUserRole>(extra.Value);
        Assert.Equal("admin", extraRole.RoleId);
#if WORKFLOW_INBOX_FOCUSED
        // The source-linked harness supplies a controlled machine-principal boundary.
        controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([
            new System.Security.Claims.Claim("party_id", "user"), new System.Security.Claims.Claim("client_kind", "machine")], "test"));
        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(await controller.RevokeCompanyRole(ScopeB.TenantId, ScopeB.BusinessAssociateId, extraRole.UserRoleId, default));
        controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("party_id", "user")], "test"));
#endif
        Assert.Contains("Administrator", (await fixture.Access.GetCompanyAccessAsync("new-user", ScopeB))!.Roles);
        Assert.DoesNotContain("Administrator", (await fixture.Access.GetAccessAsync("new-user"))!.Roles);
        var foreign = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.ReviewCompanyMemberAccess(ScopeA.TenantId, ScopeA.BusinessAssociateId, "new-user", default));
        Assert.Null(Assert.IsType<CompanyMemberAccessReview>(foreign.Value).Access);
        (await fixture.Db.Set<AppCompanyMembership>().SingleAsync(x => x.UserId == "new-user")).EffectiveToUtc = Now.UtcDateTime;
        await fixture.Db.SaveChangesAsync();
        var expired = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.ReviewCompanyMemberAccess(ScopeB.TenantId, ScopeB.BusinessAssociateId, "new-user", default));
        var expiredReview = Assert.IsType<CompanyMemberAccessReview>(expired.Value);
        Assert.Null(expiredReview.Access); Assert.Equal(Now.UtcDateTime, expiredReview.Membership!.EffectiveToUtc);
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services).ReviewCompanyMemberAccessAsync(ScopeB, "new-user", "new-user"))).Kind);
    }
    [Fact]
    public async Task New_company_membership_refuses_disabled_account_without_granting_identity_role()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Users.Add(new OilGasUser { Id = "disabled", UserName = "disabled", IsActive = false }); await fixture.Db.SaveChangesAsync();
        var refusal = await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services).CreateCompanyMembershipAsync(ScopeB, "disabled", "engineer", "Approved", "22222222222222222222222222222222", "user"));
        Assert.Equal(RefusalKind.Conflict, refusal.Kind); Assert.False(await fixture.Db.UserRoles.AnyAsync(x => x.UserId == "disabled"));
    }

    [Fact]
    public async Task Colliding_company_ids_do_not_grant_another_tenants_access()
    {
        using var fixture = new Fixture();
        Assert.Equal(new[] { ScopeA }, await fixture.Access.GetCompaniesAsync("user"));
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", ScopeB));
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!.Roles);
    }

    [Theory][InlineData("disable")][InlineData("delete")]
    public async Task Active_account_lookup_rechecks_the_same_service_after_account_state_changes(string change)
    {
        using var fixture=new Fixture();
        var user=new OilGasUser{Id="isolated-account",UserName="isolated-account",IsActive=true};
        fixture.Db.Users.Add(user);await fixture.Db.SaveChangesAsync();
        Assert.True(await fixture.Access.IsAccountActiveAsync(user.Id));
        Assert.Empty((await fixture.Access.GetAccessAsync(user.Id))!.Roles);
        if(change=="disable")user.IsActive=false;else fixture.Db.Users.Remove(user);
        await fixture.Db.SaveChangesAsync();Assert.False(await fixture.Access.IsAccountActiveAsync(user.Id));
        Assert.False(await fixture.Access.IsAccountActiveAsync("unknown-account"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>fixture.Access.IsAccountActiveAsync("user",new CancellationToken(true)));
    }

    [Fact]
    public async Task Profile_tenant_and_unscoped_administrator_role_cannot_grant_company_access()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(new AppUserExtension { UserId = "user", TenantId = "B" });
        await fixture.Db.SaveChangesAsync();
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", ScopeB));
        var access = (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!;
        Assert.DoesNotContain("Administrator", access.Roles);
        Assert.Equal(new[] { "study.read" }, access.Permissions);
    }

    [Fact]
    public async Task Scoped_roles_and_permissions_do_not_become_repository_wide_authority()
    {
        using var fixture = new Fixture();
        var access = (await fixture.Access.GetAccessAsync("user"))!;
        Assert.Equal(new[] { "Administrator" }, access.Roles);
        Assert.Equal(new[] { "setup.manage" }, access.Permissions);
    }

    [Fact]
    public async Task Company_membership_alone_grants_no_roles()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(fixture.Membership("B"));
        await fixture.Db.SaveChangesAsync();
        var access = (await fixture.Access.GetCompanyAccessAsync("user", ScopeB))!;
        Assert.Empty(access.Roles);
        Assert.Empty(access.Permissions);
        Assert.Equal(2, (await fixture.Access.GetCompaniesAsync("user")).Length);
    }

    [Theory]
    [InlineData("tenant")]
    [InlineData("company")]
    [InlineData("user")]
    public async Task Disabled_owners_fail_closed(string owner)
    {
        using var fixture = new Fixture();
        if (owner == "tenant") (await fixture.Db.Set<AppTenant>().FindAsync("A"))!.IsActive = false;
        if (owner == "company") (await fixture.Db.Set<AppCompany>().FindAsync("A", "operator"))!.IsActive = false;
        if (owner == "user") (await fixture.Db.Users.FindAsync("user"))!.IsActive = false;
        await fixture.Db.SaveChangesAsync();
        Assert.Empty(await fixture.Access.GetCompaniesAsync("user"));
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", ScopeA));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Membership_effective_boundaries_are_enforced(bool future)
    {
        using var fixture = new Fixture();
        var membership = (await fixture.Db.Set<AppCompanyMembership>().FindAsync("A", "operator", "user"))!;
        if (future) membership.EffectiveFromUtc = Now.UtcDateTime.AddTicks(1);
        else membership.EffectiveToUtc = Now.UtcDateTime;
        await fixture.Db.SaveChangesAsync();
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", ScopeA));
        Assert.Empty(await fixture.Access.GetCompaniesAsync("user"));
    }

    [Theory]
    [InlineData("Expired")]
    [InlineData("Future")]
    [InlineData("Pending")]
    [InlineData("Revoked")]
    public async Task Non_current_or_unapproved_scoped_grants_do_not_authorize(string state)
    {
        using var fixture = new Fixture();
        var assignment = await fixture.Db.Set<AppUserRoleExtension>().SingleAsync();
        if (state == "Expired") assignment.EffectiveToUtc = Now.UtcDateTime;
        else if (state == "Future") assignment.EffectiveFromUtc = Now.UtcDateTime.AddTicks(1);
        else assignment.ApprovalStatus = state;
        await fixture.Db.SaveChangesAsync();
        var access = (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!;
        Assert.Empty(access.Roles);
        Assert.Empty(access.Permissions);
    }

    [Fact]
    public async Task Removing_standard_role_membership_revokes_scoped_roles_and_permissions()
    {
        using var fixture = new Fixture();
        fixture.Db.UserRoles.Remove(await fixture.Db.UserRoles.SingleAsync(x => x.RoleId == "engineer"));
        await fixture.Db.SaveChangesAsync();
        var access = (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!;
        Assert.Empty(access.Roles);
        Assert.Empty(access.Permissions);
    }
    [Theory]
    [InlineData("account")]
    [InlineData("membership")]
    [InlineData("standard-role")]
    [InlineData("scoped-role")]
    [InlineData("permission")]
    [InlineData("expiry")]
    public async Task Reused_company_query_plans_always_observe_current_rows_and_clock(string change)
    {
        using var fixture=new Fixture();var clock=new MovingClock{Instant=Now};var service=new RepositoryAccessService(fixture.Db,clock);
        var assignment=await fixture.Db.Set<AppUserRoleExtension>().SingleAsync();
        if(change=="expiry"){assignment.EffectiveToUtc=Now.AddMinutes(1).UtcDateTime;await fixture.Db.SaveChangesAsync();}
        var first=(await service.GetCompanyAccessAsync("user",ScopeA))!;
        Assert.Equal(new[]{"Engineer"},first.Roles);Assert.Equal(new[]{"study.read"},first.Permissions);
        if(change=="account")(await fixture.Db.Users.FindAsync("user"))!.IsActive=false;
        if(change=="membership")(await fixture.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc=Now.UtcDateTime;
        if(change=="standard-role")fixture.Db.UserRoles.Remove(await fixture.Db.UserRoles.SingleAsync(row=>row.RoleId=="engineer"));
        if(change=="scoped-role")assignment.ApprovalStatus="Revoked";
        if(change=="permission")fixture.Db.RoleClaims.Remove(await fixture.Db.RoleClaims.SingleAsync(row=>row.RoleId=="engineer"));
        await fixture.Db.SaveChangesAsync();if(change=="expiry")clock.Instant=Now.AddMinutes(1);
        var current=await service.GetCompanyAccessAsync("user",ScopeA);
        if(change is "account" or "membership")Assert.Null(current);
        else{Assert.NotNull(current);Assert.Empty(current.Permissions);Assert.Equal(change=="permission"?new[]{"Engineer"}:Array.Empty<string>(),current.Roles);}
    }

    [Fact]
    public async Task Database_rejects_a_scoped_assignment_without_matching_company_membership()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(new AppUserRoleExtension
        {
            UserRoleId = "forged", UserId = "user", RoleId = "engineer", TenantId = "B", BusinessAssociateId = "operator",
            EffectiveFromUtc = Now.UtcDateTime
        });
        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task Current_company_snapshot_keeps_permission_claims_independent_of_a_null_role_name()
    {
        using var fixture=new Fixture();
        (await fixture.Db.Roles.SingleAsync(row=>row.Id=="engineer")).Name=null;
        fixture.Db.RoleClaims.AddRange(
            new IdentityRoleClaim<string>{RoleId="engineer",ClaimType="permission",ClaimValue="study.read"},
            new IdentityRoleClaim<string>{RoleId="engineer",ClaimType="permission",ClaimValue=null},
            new IdentityRoleClaim<string>{RoleId="engineer",ClaimType="unrelated",ClaimValue="setup.manage"});
        await fixture.Db.SaveChangesAsync();
        var access=(await fixture.Access.GetCompanyAccessAsync("user",ScopeA))!;
        Assert.Empty(access.Roles);Assert.Equal(new[]{"study.read"},access.Permissions);
    }

    [Theory][InlineData("binary")][InlineData("both")][InlineData("roles")][InlineData("permissions")]
    public async Task Current_company_snapshot_preserves_database_distinct_and_ordering_for_each_authority_column(string collation)
    {
        using var fixture=new Fixture(collation:collation);
        foreach(var (id,name) in new[]{("lower","engineer"),("zed","Zed"),("alpha","alpha")})
        {
            fixture.Db.Roles.Add(new IdentityRole{Id=id,Name=name,NormalizedName="TEST_"+id.ToUpperInvariant()});
            fixture.Db.UserRoles.Add(new IdentityUserRole<string>{UserId="user",RoleId=id});
            fixture.Db.Add(new AppUserRoleExtension{UserRoleId="scoped-"+id,UserId="user",RoleId=id,TenantId="A",BusinessAssociateId="operator",EffectiveFromUtc=Now.UtcDateTime});
        }
        fixture.Db.RoleClaims.AddRange(
            new IdentityRoleClaim<string>{RoleId="engineer",ClaimType="permission",ClaimValue="study.read"},
            new IdentityRoleClaim<string>{RoleId="lower",ClaimType="permission",ClaimValue="STUDY.READ"},
            new IdentityRoleClaim<string>{RoleId="zed",ClaimType="permission",ClaimValue="Z.read"},
            new IdentityRoleClaim<string>{RoleId="alpha",ClaimType="permission",ClaimValue="a.read"});
        await fixture.Db.SaveChangesAsync();
        // The existing database owns DISTINCT and ordering; roles and permission values may use different collations.
        var roles=await (from standard in fixture.Db.UserRoles.AsNoTracking()
            join assigned in fixture.Db.Set<AppUserRoleExtension>().AsNoTracking() on new{standard.UserId,standard.RoleId} equals new{assigned.UserId,assigned.RoleId}
            join role in fixture.Db.Roles.AsNoTracking() on standard.RoleId equals role.Id
            where standard.UserId=="user" && assigned.TenantId=="A" && assigned.BusinessAssociateId=="operator" && assigned.ApprovalStatus=="Approved" &&
                assigned.EffectiveFromUtc<=Now.UtcDateTime && (assigned.EffectiveToUtc==null || assigned.EffectiveToUtc>Now.UtcDateTime) && role.Name!=null
            select role.Name!).Distinct().OrderBy(name=>name).ToArrayAsync();
        var permissions=await (from standard in fixture.Db.UserRoles.AsNoTracking()
            join assigned in fixture.Db.Set<AppUserRoleExtension>().AsNoTracking() on new{standard.UserId,standard.RoleId} equals new{assigned.UserId,assigned.RoleId}
            join claim in fixture.Db.RoleClaims.AsNoTracking() on standard.RoleId equals claim.RoleId
            where standard.UserId=="user" && assigned.TenantId=="A" && assigned.BusinessAssociateId=="operator" && assigned.ApprovalStatus=="Approved" &&
                assigned.EffectiveFromUtc<=Now.UtcDateTime && (assigned.EffectiveToUtc==null || assigned.EffectiveToUtc>Now.UtcDateTime) && claim.ClaimType=="permission" && claim.ClaimValue!=null
            select claim.ClaimValue!).Distinct().OrderBy(value=>value).ToArrayAsync();
        var actual=(await fixture.Access.GetCompanyAccessAsync("user",ScopeA))!;
        Assert.Equal(roles,actual.Roles);Assert.Equal(permissions,actual.Permissions);
        Assert.DoesNotContain("Administrator",actual.Roles);Assert.DoesNotContain("setup.manage",actual.Permissions);
    }

    [Fact]
    public async Task Unknown_user_and_scope_are_not_disclosed()
    {
        using var fixture = new Fixture();
        Assert.Empty(await fixture.Access.GetCompaniesAsync("missing"));
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("missing", ScopeA));
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", new("A", "missing")));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Access.GetCompanyAccessAsync("user", new("", "operator")));
    }

    [Fact]
    public async Task Cancelled_reads_do_not_return_access()
    {
        using var fixture = new Fixture();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            fixture.Access.GetCompanyAccessAsync("user", ScopeA, new CancellationToken(true)));
    }

    [Fact]
    public async Task Repository_role_assignment_does_not_reuse_company_scoped_metadata()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        var assignment = await service.AssignRoleAsync("user", "engineer", "user", "Repository grant");
        Assert.NotEqual("scoped", assignment.UserRoleId);
        Assert.Equal(2, await fixture.Db.Set<AppUserRoleExtension>().CountAsync());
        Assert.Contains("Engineer", (await fixture.Access.GetAccessAsync("user"))!.Roles);
    }

    [Fact]
    public async Task Repository_role_revocation_cannot_target_a_company_assignment()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services).RevokeRoleAsync("scoped", "user"));
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!.Roles);
    }

    [Fact]
    public async Task Repository_role_listing_handles_the_same_role_scoped_to_multiple_companies()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(fixture.Membership("B"));
        fixture.Db.Add(new AppUserRoleExtension
        {
            UserRoleId = "scope-b", UserId = "user", RoleId = "engineer", TenantId = "B", BusinessAssociateId = "operator",
            EffectiveFromUtc = Now.UtcDateTime
        });
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        var assignments = await fixture.Assignments(services).GetUserRoleAssignmentsAsync("user");
        Assert.Single(assignments);
        Assert.DoesNotContain(assignments, x => x.UserRoleId is "scope-b" or "scoped");
    }

    [Fact]
    public async Task Company_grant_creates_standard_membership_without_repository_authority()
    {
        using var fixture = new Fixture();
        fixture.Db.Users.Add(new OilGasUser { Id = "new-user", UserName = "new-user", IsActive = true });
        fixture.Db.Add(new AppCompanyMembership
        {
            TenantId = "B", BusinessAssociateId = "operator", UserId = "new-user", GrantedByUserId = "user",
            EffectiveFromUtc = Now.UtcDateTime, ChangedUtc = Now.UtcDateTime
        });
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        var grant = await fixture.Assignments(services).AssignCompanyRoleAsync(ScopeB, "new-user", new("engineer", "Approved hire"), "user");
        Assert.Equal("B", grant.TenantId);
        Assert.Equal("user", grant.GrantedByUserId);
        Assert.Equal(Now.UtcDateTime, grant.EffectiveFromUtc);
        Assert.True(await fixture.Db.UserRoles.AnyAsync(x => x.UserId == "new-user" && x.RoleId == "engineer"));
        Assert.Empty((await fixture.Access.GetAccessAsync("new-user"))!.Roles);
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("new-user", ScopeA));
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("new-user", ScopeB))!.Roles);
    }

    [Fact]
    public async Task Company_grant_requires_active_membership_in_the_selected_tenant()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        var refusal = await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services)
            .AssignCompanyRoleAsync(ScopeB, "user", new("engineer", "Approved transfer"), "user"));
        Assert.Equal(RefusalKind.Forbidden, refusal.Kind);
        Assert.Single(await fixture.Db.Set<AppUserRoleExtension>().ToArrayAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Company_only_or_disabled_administrator_cannot_manage_grants(bool companyOnly)
    {
        using var fixture = new Fixture();
        if (companyOnly)
            fixture.Db.Add(new AppUserRoleExtension { UserRoleId = "company-admin", UserId = "user", RoleId = "admin",
                TenantId = "A", BusinessAssociateId = "operator", EffectiveFromUtc = Now.UtcDateTime });
        else (await fixture.Db.Users.FindAsync("user"))!.IsActive = false;
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() =>
            service.AssignCompanyRoleAsync(ScopeA, "user", new("engineer", "Test"), "user"))).Kind);
        await Assert.ThrowsAsync<RefusalException>(() => service.RevokeCompanyRoleAsync(ScopeA, "scoped", "user"));
        await Assert.ThrowsAsync<RefusalException>(() => service.GetCompanyRoleAssignmentsAsync(ScopeA, "user", "user"));
    }

    [Fact]
    public async Task Company_grant_cannot_silently_reclassify_unannotated_repository_membership()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() =>
            service.AssignCompanyRoleAsync(ScopeA, "user", new("admin", "Explicit approval"), "user"))).Kind);
        Assert.Contains("Administrator", (await fixture.Access.GetAccessAsync("user"))!.Roles);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task Company_role_expiry_must_be_future_and_within_membership(int days)
    {
        using var fixture = new Fixture();
        var membership = fixture.Membership("B");
        membership.EffectiveToUtc = Now.AddDays(1).UtcDateTime;
        fixture.Db.Add(membership);
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        var refusal = await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services)
            .AssignCompanyRoleAsync(ScopeB, "user", new("engineer", "Approved grant", Now.AddDays(days)), "user"));
        Assert.Equal(RefusalKind.Invalid, refusal.Kind);
        Assert.Single(await fixture.Db.Set<AppUserRoleExtension>().ToArrayAsync());
    }

    [Fact]
    public async Task Company_grant_is_capped_to_known_membership_expiry()
    {
        using var fixture = new Fixture();
        var membership = fixture.Membership("B");
        membership.EffectiveToUtc = Now.AddDays(1).UtcDateTime;
        fixture.Db.Add(membership);
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        var grant = await fixture.Assignments(services).AssignCompanyRoleAsync(ScopeB, "user", new("engineer", "Approved grant"), "user");
        Assert.Equal(membership.EffectiveToUtc, grant.EffectiveToUtc);
    }

    [Fact]
    public async Task Duplicate_company_grant_is_a_conflict_and_preserves_original_actor_and_reason()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services)
            .AssignCompanyRoleAsync(ScopeA, "user", new("engineer", "Different reason"), "user"));
        Assert.Single(await fixture.Db.Set<AppUserRoleExtension>().ToArrayAsync());
        Assert.Null((await fixture.Db.Set<AppUserRoleExtension>().SingleAsync()).AssignmentReason);
    }

    [Fact]
    public async Task Repository_revocation_preserves_company_grant_and_identity_membership()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        var grant = await service.AssignRoleAsync("user", "engineer", "user", "Repository assignment");
        Assert.True(await service.RevokeRoleAsync(grant.UserRoleId, "user"));
        Assert.True(await fixture.Db.UserRoles.AnyAsync(x => x.RoleId == "engineer"));
        Assert.DoesNotContain("Engineer", (await fixture.Access.GetAccessAsync("user"))!.Roles);
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!.Roles);
        Assert.Equal("user", (await fixture.Db.Set<AppUserRoleExtension>().FindAsync(grant.UserRoleId))!.RevokedByUserId);
    }

    [Fact]
    public async Task Membership_retirement_endpoint_records_the_authenticated_actor()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        var membership = await fixture.Db.Set<AppCompanyMembership>().AsNoTracking().SingleAsync(x => x.TenantId == "A" && x.UserId == "user");
        var controller = new Beep.OilandGas.ApiService.Controllers.Identity.RoleAssignmentController(fixture.Assignments(services),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.ApiService.Controllers.Identity.RoleAssignmentController>.Instance,
            Moq.Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
        controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("party_id", "user")], "test"));
        var request = new CompanyMembershipRetirementRequest("55555555555555555555555555555555", membership.ConcurrencyStamp, "Transfer complete");
        var result = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.RetireCompanyMembership(ScopeA.TenantId, ScopeA.BusinessAssociateId, "user", request, default));
        Assert.Equal("user", Assert.IsType<CompanyMembershipRetirement>(result.Value).ActorUserId);
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", ScopeA));
        var history = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.ReviewMembershipRetirements(ScopeA.TenantId, ScopeA.BusinessAssociateId, "user", 0, 50, default));
        var page = Assert.IsType<CompanyMembershipRetirementPage>(history.Value);
        Assert.Equal(ScopeA, page.Company); Assert.Equal("Transfer complete", Assert.Single(page.Items).Reason);
    }

    [Fact]
    public async Task Membership_retirement_is_audited_idempotent_and_does_not_affect_other_company()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Add(fixture.Membership("B")); await fixture.Db.SaveChangesAsync();
        var service = fixture.Assignments(services);
        await service.AssignCompanyRoleAsync(ScopeB, "user", new("engineer", "Other company"), "user");
        fixture.Db.Add(new AppUserRoleExtension { UserRoleId = "future-company-role", UserId = "user", RoleId = "engineer",
            TenantId = ScopeA.TenantId, BusinessAssociateId = ScopeA.BusinessAssociateId, ApprovalStatus = "Approved",
            EffectiveFromUtc = Now.AddDays(7).UtcDateTime });
        await fixture.Db.SaveChangesAsync();
        var existing = await fixture.Db.Set<AppCompanyMembership>().AsNoTracking().SingleAsync(x => x.TenantId == "A" && x.UserId == "user");
        var request = new CompanyMembershipRetirementRequest("55555555555555555555555555555555", existing.ConcurrencyStamp, "Transfer complete");
        var retired = await service.RetireCompanyMembershipAsync(ScopeA, "user", request, "user");
        Assert.Equal("user", retired.ActorUserId); Assert.Equal(request.ExpectedRevision, retired.PreviousRevision);
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", ScopeA));
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("user", ScopeB))!.Roles);
        Assert.Contains("Administrator", (await fixture.Access.GetAccessAsync("user"))!.Roles);
        Assert.Equal(retired.IntentId, (await service.RetireCompanyMembershipAsync(ScopeA, "user", request, "user")).IntentId);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.RetireCompanyMembershipAsync(ScopeA, "user", request with { Reason = "Different" }, "user"))).Kind);
        Assert.Single(await fixture.Db.Set<CompanyMembershipRetirement>().AsNoTracking().ToListAsync());
        Assert.Equal(0, (await service.ReviewMembershipRetirementsAsync(ScopeB, "user", "user", 0, 50)).Total);
        Assert.Empty((await service.ReviewMembershipRetirementsAsync(ScopeA, "user", "user", 1, 1)).Items);
        var oldRole = await fixture.Db.Set<AppUserRoleExtension>().AsNoTracking().SingleAsync(x => x.UserRoleId == "scoped");
        Assert.Equal("Revoked", oldRole.ApprovalStatus); Assert.Equal("user", oldRole.RevokedByUserId);
        var futureRole = await fixture.Db.Set<AppUserRoleExtension>().AsNoTracking().SingleAsync(x => x.UserRoleId == "future-company-role");
        Assert.Equal("Revoked", futureRole.ApprovalStatus); Assert.Equal(futureRole.EffectiveFromUtc, futureRole.EffectiveToUtc);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.CreateCompanyMembershipAsync(ScopeA, "user", "engineer", "Return", "66666666666666666666666666666666", "user"))).Kind);
    }

    [Fact]
    public async Task Membership_retirement_cancels_future_interval_without_creating_a_negative_interval()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        var membership = await fixture.Db.Set<AppCompanyMembership>().SingleAsync(x => x.TenantId == "A" && x.UserId == "user");
        membership.EffectiveFromUtc = Now.AddDays(1).UtcDateTime; await fixture.Db.SaveChangesAsync();
        var request = new CompanyMembershipRetirementRequest("77777777777777777777777777777777", membership.ConcurrencyStamp, "Cancel future access");
        var service = fixture.Assignments(services);
        var result = await service.RetireCompanyMembershipAsync(ScopeA, "user", request, "user");
        Assert.Equal(result.EffectiveFromUtc, result.EffectiveToUtc);
        Assert.Null(await fixture.Access.GetCompanyAccessAsync("user", ScopeA));
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.RetireCompanyMembershipAsync(ScopeA, "user", new("88888888888888888888888888888888", result.ResultRevision, "Again"), "user"))).Kind);
    }

    [Fact]
    public async Task Membership_retirement_rejects_stale_revision_and_non_administrator()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        fixture.Db.Users.Add(new OilGasUser { Id = "other", UserName = "other", IsActive = true }); await fixture.Db.SaveChangesAsync();
        var service = fixture.Assignments(services);
        var request = new CompanyMembershipRetirementRequest("55555555555555555555555555555555", Guid.NewGuid().ToString(), "Transfer complete");
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => service.RetireCompanyMembershipAsync(ScopeA, "user", request, "other"))).Kind);
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => service.ReviewMembershipRetirementsAsync(ScopeA, "user", "other", 0, 50))).Kind);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.RetireCompanyMembershipAsync(ScopeA, "user", request, "user"))).Kind);
        Assert.NotNull(await fixture.Access.GetCompanyAccessAsync("user", ScopeA));
        Assert.Empty(await fixture.Db.Set<CompanyMembershipRetirement>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Company_membership_retirement_evidence_is_append_only()
    {
        using var fixture = new Fixture();
        var evidence = new CompanyMembershipRetirement
        {
            IntentId = "44444444444444444444444444444444", TenantId = ScopeA.TenantId,
            BusinessAssociateId = ScopeA.BusinessAssociateId, UserId = "user", ActorUserId = "user",
            Reason = "Membership ended", PreviousRevision = Guid.NewGuid().ToString(), ResultRevision = Guid.NewGuid().ToString(),
            EffectiveFromUtc = Now.AddDays(-1).UtcDateTime, EffectiveToUtc = Now.UtcDateTime, RetiredUtc = Now.UtcDateTime
        };
        fixture.Db.Add(evidence); await fixture.Db.SaveChangesAsync();
        evidence.Reason = "Changed";
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Db.SaveChangesAsync());
        fixture.Db.Entry(evidence).State = EntityState.Unchanged;
        fixture.Db.Remove(evidence);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task Disabled_company_member_can_have_scoped_role_removed_by_administrator()
    {
        using var fixture = new Fixture(); using var services = fixture.IdentityServices();
        var member = new OilGasUser { Id = "disabled-member", UserName = "disabled-member", IsActive = true };
        fixture.Db.Users.Add(member); await fixture.Db.SaveChangesAsync();
        var service = fixture.Assignments(services);
        var grant = await service.CreateCompanyMembershipAsync(ScopeB, member.Id, "engineer", "Approved", "33333333333333333333333333333333", "user");
        member.IsActive = false; await fixture.Db.SaveChangesAsync();
        Assert.Null(await fixture.Access.GetCompanyAccessAsync(member.Id, ScopeB));
        Assert.True(await service.RevokeCompanyRoleAsync(ScopeB, grant.UserRoleId, "user"));
        Assert.Equal("Revoked", (await fixture.Db.Set<AppUserRoleExtension>().SingleAsync(x => x.UserRoleId == grant.UserRoleId)).ApprovalStatus);
        Assert.Contains("Administrator", (await fixture.Access.GetAccessAsync("user"))!.Roles);
    }

    [Fact]
    public async Task Company_revocation_preserves_repository_grant()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        await service.AssignRoleAsync("user", "engineer", "user", "Repository assignment");
        Assert.True(await service.RevokeCompanyRoleAsync(ScopeA, "scoped", "user"));
        Assert.Empty((await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!.Roles);
        Assert.Contains("Engineer", (await fixture.Access.GetAccessAsync("user"))!.Roles);
    }

    [Fact]
    public async Task Company_revocation_preserves_other_company_and_history_is_scope_bound()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(fixture.Membership("B"));
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        var grant = await service.AssignCompanyRoleAsync(ScopeB, "user", new("engineer", "Transfer"), "user");
        Assert.False(await service.RevokeCompanyRoleAsync(ScopeA, grant.UserRoleId, "user"));
        Assert.True(await service.RevokeCompanyRoleAsync(ScopeA, "scoped", "user"));
        Assert.Empty((await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!.Roles);
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("user", ScopeB))!.Roles);
        var history = Assert.Single(await service.GetCompanyRoleAssignmentsAsync(ScopeA, "user", "user"));
        Assert.Equal("Revoked", history.ApprovalStatus);
        Assert.Equal("user", history.RevokedByUserId);
        Assert.Equal(Now.UtcDateTime, history.EffectiveToUtc);
        Assert.Equal(grant.UserRoleId, Assert.Single(await service.GetCompanyRoleAssignmentsAsync(ScopeB, "user", "user")).UserRoleId);
    }

    [Fact]
    public async Task Final_company_revocation_removes_identity_membership_and_retry_preserves_history()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        Assert.True(await service.RevokeCompanyRoleAsync(ScopeA, "scoped", "user"));
        Assert.False(await service.RevokeCompanyRoleAsync(ScopeA, "scoped", "user"));
        Assert.False(await fixture.Db.UserRoles.AnyAsync(x => x.RoleId == "engineer"));
        Assert.Single(await fixture.Db.Set<AppUserRoleExtension>().ToArrayAsync());
        Assert.Equal("user", (await fixture.Db.Set<AppUserRoleExtension>().SingleAsync()).RevokedByUserId);
    }

    [Fact]
    public async Task Future_company_grant_keeps_standard_identity_membership_after_other_scope_is_revoked()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(fixture.Membership("B"));
        fixture.Db.Add(new AppUserRoleExtension { UserRoleId = "future", UserId = "user", RoleId = "engineer",
            TenantId = "B", BusinessAssociateId = "operator", EffectiveFromUtc = Now.AddDays(1).UtcDateTime });
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        await fixture.Assignments(services).RevokeCompanyRoleAsync(ScopeA, "scoped", "user");
        Assert.True(await fixture.Db.UserRoles.AnyAsync(x => x.RoleId == "engineer"));
        Assert.Empty((await fixture.Access.GetCompanyAccessAsync("user", ScopeB))!.Roles);
        Assert.DoesNotContain("Engineer", (await fixture.Access.GetAccessAsync("user"))!.Roles);
    }

    [Fact]
    public async Task Scoped_administrator_is_not_counted_as_a_replacement_for_last_repository_administrator()
    {
        using var fixture = new Fixture();
        fixture.Db.Users.Add(new OilGasUser { Id = "company-admin", UserName = "company-admin", IsActive = true });
        fixture.Db.Add(new AppCompanyMembership { TenantId = "A", BusinessAssociateId = "operator", UserId = "company-admin",
            GrantedByUserId = "user", EffectiveFromUtc = Now.UtcDateTime });
        fixture.Db.Add(new IdentityUserRole<string> { UserId = "company-admin", RoleId = "admin" });
        fixture.Db.Add(new AppUserRoleExtension { UserId = "company-admin", RoleId = "admin", TenantId = "A",
            BusinessAssociateId = "operator", EffectiveFromUtc = Now.UtcDateTime });
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        var administrator = Assert.Single(await service.GetUserRoleAssignmentsAsync("user"));
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() =>
            service.RevokeRoleAsync(administrator.UserRoleId, "user"))).Kind);
        Assert.Contains("Administrator", (await fixture.Access.GetAccessAsync("user"))!.Roles);
    }

    [Fact]
    public async Task Expired_unscoped_metadata_does_not_reappear_as_implicit_identity_authority()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(new AppUserRoleExtension { UserId = "user", RoleId = "admin", EffectiveFromUtc = Now.AddDays(-1).UtcDateTime,
            EffectiveToUtc = Now.UtcDateTime });
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        Assert.Empty((await fixture.Access.GetAccessAsync("user"))!.Roles);
        Assert.Empty(await fixture.Assignments(services).GetUserRoleAssignmentsAsync("user"));
    }

    [Fact]
    public async Task Company_grant_cancellation_and_missing_reason_do_not_write()
    {
        using var fixture = new Fixture();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.AssignCompanyRoleAsync(ScopeA,
            "user", new("engineer", "Approved"), "user", new CancellationToken(true)));
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() =>
            service.AssignCompanyRoleAsync(ScopeA, "user", new("engineer", ""), "user"))).Kind);
        Assert.Single(await fixture.Db.Set<AppUserRoleExtension>().ToArrayAsync());
    }

    [Fact]
    public async Task Time_limited_repository_assignment_can_be_revoked_without_touching_company_access()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(new AppUserRoleExtension { UserRoleId = "limited", UserId = "user", RoleId = "engineer",
            EffectiveFromUtc = Now.UtcDateTime, EffectiveToUtc = Now.AddDays(1).UtcDateTime });
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        var service = fixture.Assignments(services);
        var replay = await service.AssignRoleAsync("user", "engineer", "user", "Retry");
        Assert.Equal("limited", replay.UserRoleId);
        Assert.True(await service.RevokeRoleAsync("limited", "user"));
        Assert.DoesNotContain("Engineer", (await fixture.Access.GetAccessAsync("user"))!.Roles);
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!.Roles);
    }

    [Fact]
    public async Task Pending_repository_assignment_cannot_be_reported_as_a_successful_approved_grant()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(new AppUserRoleExtension { UserRoleId = "pending", UserId = "user", RoleId = "engineer",
            EffectiveFromUtc = Now.UtcDateTime, ApprovalStatus = "Pending" });
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => fixture.Assignments(services)
            .AssignRoleAsync("user", "engineer", "user", "Retry"))).Kind);
        Assert.DoesNotContain("Engineer", (await fixture.Access.GetAccessAsync("user"))!.Roles);
        Assert.Equal("Pending", (await fixture.Db.Set<AppUserRoleExtension>().FindAsync("pending"))!.ApprovalStatus);
    }

    [Fact]
    public async Task Company_metadata_save_failure_rolls_back_standard_identity_membership()
    {
        using var fixture = new Fixture(new FailCompanyGrant());
        fixture.Db.Users.Add(new OilGasUser { Id = "new-user", UserName = "new-user", IsActive = true });
        fixture.Db.Add(new AppCompanyMembership { TenantId = "B", BusinessAssociateId = "operator", UserId = "new-user",
            GrantedByUserId = "user", EffectiveFromUtc = Now.UtcDateTime });
        await fixture.Db.SaveChangesAsync();
        using var services = fixture.IdentityServices();
        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Assignments(services)
            .AssignCompanyRoleAsync(ScopeB, "new-user", new("engineer", "Approved"), "user"));
        fixture.Db.ChangeTracker.Clear();
        Assert.False(await fixture.Db.UserRoles.AnyAsync(x => x.UserId == "new-user"));
        Assert.False(await fixture.Db.Set<AppUserRoleExtension>().AnyAsync(x => x.UserId == "new-user"));
        Assert.Empty((await fixture.Access.GetAccessAsync("new-user"))!.Roles);
        Assert.Equal(new[] { "Engineer" }, (await fixture.Access.GetCompanyAccessAsync("user", ScopeA))!.Roles);
    }

    private sealed class FailCompanyGrant : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<AppUserRoleExtension>()
                .Any(x => x.State == EntityState.Added && x.Entity.TenantId == "B"))
                throw new DbUpdateException("Injected company metadata save failure.");
            return ValueTask.FromResult(result);
        }
    }

    [Fact]
    public async Task Resolved_company_context_is_immutable_and_response_mutation_cannot_change_it()
    {
        using var fixture = new Fixture();
        var context = (await new RepositoryCompanyContextResolver(fixture.Access).ResolveAsync("user", ScopeA))!;
        Assert.Equal("user", context.UserId);
        Assert.Equal(ScopeA, context.Scope);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)context.Roles)[0] = "Administrator");
        Assert.Throws<NotSupportedException>(() => ((IList<string>)context.Permissions)[0] = "setup.manage");
        var response = context.ToAccess();
        response.Roles[0] = "Administrator";
        response.Permissions[0] = "setup.manage";
        Assert.Equal(new[] { "Engineer" }, context.Roles);
        Assert.Equal(new[] { "study.read" }, context.Permissions);
    }

    [Fact]
    public async Task Company_context_rechecks_revoked_membership_and_does_not_reuse_previous_selection()
    {
        using var fixture = new Fixture();
        var resolver = new RepositoryCompanyContextResolver(fixture.Access);
        Assert.NotNull(await resolver.ResolveAsync("user", ScopeA));
        Assert.Null(await resolver.ResolveAsync("user", ScopeB));
        var membership = (await fixture.Db.Set<AppCompanyMembership>().FindAsync("A", "operator", "user"))!;
        membership.EffectiveToUtc = Now.UtcDateTime;
        await fixture.Db.SaveChangesAsync();
        Assert.Null(await resolver.ResolveAsync("user", ScopeA));
        Assert.Null(await resolver.ResolveAsync("missing-user", ScopeA));
    }

    [Fact]
    public async Task Switching_company_context_retains_no_previous_roles_or_permissions()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(fixture.Membership("B"));
        await fixture.Db.SaveChangesAsync();
        var resolver = new RepositoryCompanyContextResolver(fixture.Access);
        var a = (await resolver.ResolveAsync("user", ScopeA))!;
        var b = (await resolver.ResolveAsync("user", ScopeB))!;
        Assert.NotSame(a, b);
        Assert.Equal(ScopeB, b.Scope);
        Assert.Empty(b.Roles);
        Assert.Empty(b.Permissions);
        Assert.Equal(new[] { "Engineer" }, a.Roles);
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private sealed class MovingClock : TimeProvider
    {
        public DateTimeOffset Instant {get;set;}
        public override DateTimeOffset GetUtcNow()=>Instant;
    }
    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);
    private sealed class CaseFoldContext(DbContextOptions<CaseFoldContext> options) : RepositoryDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<IdentityRole>().Property(row=>row.Name).UseCollation("NOCASE");
            builder.Entity<IdentityRoleClaim<string>>().Property(row=>row.ClaimValue).UseCollation("NOCASE");
        }
    }
    private sealed class RoleFoldContext(DbContextOptions<RoleFoldContext> options) : RepositoryDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);builder.Entity<IdentityRole>().Property(row=>row.Name).UseCollation("NOCASE");
        }
    }
    private sealed class PermissionFoldContext(DbContextOptions<PermissionFoldContext> options) : RepositoryDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);builder.Entity<IdentityRoleClaim<string>>().Property(row=>row.ClaimValue).UseCollation("NOCASE");
        }
    }
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:;Foreign Keys=True");
        public RepositoryDbContext Db { get; }
        public RepositoryAccessService Access { get; }
        public ServiceProvider IdentityServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(Db);
            services.AddIdentityCore<OilGasUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<RepositoryDbContext>();
            return services.BuildServiceProvider();
        }
        public RepositoryRoleAssignmentService Assignments(ServiceProvider services) => new(Db,
            services.GetRequiredService<UserManager<OilGasUser>>(), services.GetRequiredService<RoleManager<IdentityRole>>(),
            NullLogger<RepositoryRoleAssignmentService>.Instance, new FixedClock());
        public AppCompanyMembership Membership(string tenant) => new()
        {
            TenantId = tenant, BusinessAssociateId = "operator", UserId = "user", GrantedByUserId = "user",
            EffectiveFromUtc = Now.UtcDateTime, ChangedUtc = Now.UtcDateTime
        };
        private static DbContextOptions<T> ContextOptions<T>(SqliteConnection connection,SaveChangesInterceptor? interceptor) where T:DbContext
        {
            var options=new DbContextOptionsBuilder<T>().UseSqlite(connection);
            if(interceptor is not null)options.AddInterceptors(interceptor);
            return options.Options;
        }
        public Fixture(SaveChangesInterceptor? interceptor = null,string collation="binary")
        {
            connection.Open();
            Db=collation switch
            {
                "both"=>new CaseFoldContext(ContextOptions<CaseFoldContext>(connection,interceptor)),
                "roles"=>new RoleFoldContext(ContextOptions<RoleFoldContext>(connection,interceptor)),
                "permissions"=>new PermissionFoldContext(ContextOptions<PermissionFoldContext>(connection,interceptor)),
                _=>new TestContext(ContextOptions<TestContext>(connection,interceptor))
            };
            Db.Database.EnsureCreated();
            Db.Users.Add(new OilGasUser { Id = "user", UserName = "user", IsActive = true });
            Db.Roles.AddRange(new IdentityRole { Id = "engineer", Name = "Engineer", NormalizedName = "ENGINEER" },
                new IdentityRole { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
            Db.UserRoles.AddRange(new IdentityUserRole<string> { UserId = "user", RoleId = "engineer" },
                new IdentityUserRole<string> { UserId = "user", RoleId = "admin" });
            Db.RoleClaims.AddRange(new IdentityRoleClaim<string> { RoleId = "engineer", ClaimType = "permission", ClaimValue = "study.read" },
                new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "setup.manage" });
            Db.AddRange(new AppTenant { TenantId = "A", Name = "Tenant A" }, new AppTenant { TenantId = "B", Name = "Tenant B" });
            Db.AddRange(new AppCompany { TenantId = "A", BusinessAssociateId = "operator" },
                new AppCompany { TenantId = "B", BusinessAssociateId = "operator" });
            Db.Add(Membership("A"));
            Db.Add(new AppUserRoleExtension
            {
                UserRoleId = "scoped", UserId = "user", RoleId = "engineer", TenantId = "A", BusinessAssociateId = "operator",
                EffectiveFromUtc = Now.UtcDateTime
            });
            Db.SaveChanges();
            Access = new RepositoryAccessService(Db, new FixedClock());
        }
        public void Dispose() { Db.Dispose(); connection.Dispose(); }
    }
}
