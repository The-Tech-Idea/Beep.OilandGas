using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class RolePersonaRoutingTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task RecipientsRequireActiveUsersWithRoleMembershipAndActivePersonas()
    {
        var database = $"BeepOilGas_PersonaRouting_{Guid.NewGuid():N}";
        output.WriteLine($"Retained persona routing test database: {database}");
        await using var db = new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options);
        await db.Database.MigrateAsync();
        db.Roles.Add(new IdentityRole { Id = "r", Name = "Reviewer", NormalizedName = "REVIEWER" });
        db.AddRange(new AppPersona { Code = "ENGINEER", Name = "Engineer" },
            new AppPersona { Code = "DISABLED_USER", Name = "Disabled user" },
            new AppPersona { Code = "INACTIVE", Name = "Inactive", IsActive = false });
        db.Add(new AppTenant { TenantId="tenant", Name="Tenant" });
        db.Add(new AppCompany { TenantId="tenant",BusinessAssociateId="company" });
        for (var i = 1; i <= 5; i++)
        {
            var id = $"u{i}";
            db.Users.Add(new OilGasUser { Id = id, UserName = id, IsActive = i != 3 });
            db.Add(new AppUserPersona { UserId = id, TenantId = "tenant", BusinessAssociateId = "company", ChangedBy = "u1", ChangedUtc = DateTime.UtcNow,
                PersonaCode = i == 3 ? "DISABLED_USER" : i == 4 ? "INACTIVE" : "ENGINEER" });
            if (i != 5) db.UserRoles.Add(new IdentityUserRole<string> { RoleId = "r", UserId = id });
        }
        await db.SaveChangesAsync();
        var inboxRouter = new Mock<ICrossPersonaTaskRouter>(MockBehavior.Strict);
        inboxRouter.Setup(x => x.GetTasksForPersonaAsync("ENGINEER")).ReturnsAsync(new List<CROSS_PERSONA_TASK>
        {
            new() { CROSS_TASK_ID = "allowed", PROCESS_INSTANCE_ID="process", PROCESS_STEP_INSTANCE_ID="step", ENTITY_ID="well", ENTITY_TYPE="WELL", TASK_STATUS="PENDING", TARGET_PERSONA_CODE = "ENGINEER", ASSIGNED_ROLE = "Reviewer", TASK_TYPE = "APPROVAL", PRIORITY = 1 },
            new() { CROSS_TASK_ID = "other-role", TARGET_PERSONA_CODE = "ENGINEER", ASSIGNED_ROLE = "Administrator", PRIORITY = 1 },
            new() { CROSS_TASK_ID = "other-persona", TARGET_PERSONA_CODE = "INACTIVE", ASSIGNED_ROLE = "Reviewer" }
        });
        db.Add(new AppCompanyMembership { TenantId="tenant",BusinessAssociateId="company",UserId="u1",GrantedByUserId="u1",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1) });
        db.Add(new AppUserRoleExtension { TenantId="tenant",BusinessAssociateId="company",UserId="u1",RoleId="r",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1) });
        db.RoleClaims.Add(new() { RoleId="r",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="ENGINEER" });
        await db.SaveChangesAsync();
        var authority = new RepositoryAccessService(db);
        var tenant = new RepositoryTenantContext();
        tenant.SelectCompany((await new RepositoryCompanyContextResolver(authority).ResolveAsync("u1",new("tenant","company")))!);
        var reader = new RepositoryRolePersonaReader(authority, tenant);
        Assert.Equal(new[] { "ENGINEER" }, await reader.GetActivePersonasAsync("Reviewer"));
        Assert.Empty(await reader.GetActivePersonasAsync("reviewer"));
        var resource = new Mock<IAccessControlService>(MockBehavior.Strict);
        resource.Setup(x => x.CheckAssetAccessAsync("u1","field","FIELD","READ",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse { HasAccess=true });
        resource.Setup(x => x.CheckAssetAccessAsync("u1","well","WELL","READ",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse { HasAccess=true });
        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(x => x.GetProcessInstanceAsync("process")).ReturnsAsync(new Beep.OilandGas.Models.Processes.ProcessInstance { InstanceId="process",FieldId="field",EntityId="well",EntityType="WELL",StepInstances=[new() { StepInstanceId="step" }] });
        var controller = new WorkflowTasksController(db, inboxRouter.Object, tenant, authority, new RepositoryPersonaService(db), resource.Object, process.Object, null!, new Mock<IDevelopmentAssignedReviewReader>(MockBehavior.Strict).Object, new Mock<IDevelopmentInvestmentAssignedReader>(MockBehavior.Strict).Object, new Mock<IDrillingProgramAssignedReviewReader>(MockBehavior.Strict).Object, new Mock<TheTechIdeaWeb.Diagnostics.IFailureReporter>().Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("party_id", "u1"), new Claim(ClaimTypes.Role, "Administrator")
            }, "Test")) } }
        };
        var inbox = Assert.IsType<UnifiedInbox>(Assert.IsType<OkObjectResult>(await controller.Inbox(null, default)).Value);
        Assert.Equal("allowed", Assert.Single(inbox.CriticalTasks).TaskId);
        Assert.Equal(1, inbox.Counts.TotalPending);
        var counts = Assert.IsType<InboxCounts>(Assert.IsType<OkObjectResult>(await controller.Counts(null, default)).Value);
        Assert.Equal(1, counts.Approvals);
        Assert.IsType<ForbidResult>(await controller.Inbox("INACTIVE", default));
        db.UserRoles.RemoveRange(await db.UserRoles.Where(x => x.UserId == "u1" || x.UserId == "u2").ToListAsync());
        await db.SaveChangesAsync();
        Assert.Empty(await reader.GetActivePersonasAsync("Reviewer"));
        Assert.IsType<ForbidResult>(await controller.Inbox(null, default));
        var user = await db.Users.SingleAsync(x => x.Id == "u1");
        user.IsActive = false;
        await db.SaveChangesAsync();
        Assert.IsType<ForbidResult>(await controller.Inbox(null, default));
        inboxRouter.Verify(x => x.GetTasksForPersonaAsync("ENGINEER"), Times.Exactly(2));
        inboxRouter.VerifyNoOtherCalls();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => reader.GetActivePersonasAsync("missing"));
    }

    [Fact]
    public async Task MissingBindingAndMissingIdentityReaderFailBeforePersistence()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, null, null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetTasksForPersonaAsync("ENGINEER"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetTaskCountsByPersonaAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RouteTaskAsync("p", "s", "Reviewer", "WELL", "w", null, "actor"));
        service = Create(editor.Object, () => Task.FromResult("selected"), null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RouteTaskAsync("p", "s", "Reviewer", "WELL", "w", null, "actor"));
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task IdentityRecipientsAndModuleTasksUseSeparateStoresWithOneModuleResolution()
    {
        var reader = new Mock<IRolePersonaReader>(MockBehavior.Strict);
        reader.Setup(x => x.GetActivePersonasAsync("Reviewer")).ReturnsAsync(new List<string> { "ENGINEER", "MANAGER" });
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("PROCESS_STEP_INSTANCE", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<PROCESS_STEP_INSTANCE> { new() { PROCESS_STEP_INSTANCE_ID = "s" } });
        source.Setup(x => x.InsertEntity("CROSS_PERSONA_TASK", It.IsAny<object>())).Returns(new ErrorsInfo { Flag = Errors.Ok });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("selected")).Returns(source.Object);
        var resolutions = 0;
        var service = Create(editor.Object, () => Task.FromResult(++resolutions == 1 ? "selected" : "wrong"), reader.Object);
        var tasks = await service.RouteTaskAsync("p", "s", "Reviewer", "WELL", "w", null, "actor");
        Assert.Equal(new[] { "ENGINEER", "MANAGER" }, tasks.Select(x => x.TARGET_PERSONA_CODE));
        Assert.All(tasks, x => Assert.Equal("Reviewer", x.ASSIGNED_ROLE));
        Assert.Equal(1, resolutions);
        source.Verify(x => x.GetEntityAsync("PROCESS_STEP_INSTANCE", It.IsAny<List<AppFilter>>()), Times.Once);
        source.Verify(x => x.InsertEntity("CROSS_PERSONA_TASK", It.IsAny<object>()), Times.Exactly(2));
        editor.Verify(x => x.GetDataSource("selected"), Times.Exactly(3));
        editor.VerifyNoOtherCalls();
        reader.VerifyAll();
    }

    private static CrossPersonaTaskRouter Create(IDMEEditor editor, Func<Task<string>>? connection, IRolePersonaReader? reader) =>
        new(editor, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), connection, personas: reader);
}
