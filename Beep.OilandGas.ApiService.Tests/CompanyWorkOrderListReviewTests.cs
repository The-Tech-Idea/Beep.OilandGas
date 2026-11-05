using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.WorkOrder;
using Xunit;
using Beep.OilandGas.ApiService.Controllers.WorkOrder;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyWorkOrderListReviewTests
{
    private static WorkOrderSummary Order(string id, string field = "field") => new() { InstanceId = id, FieldId = field, Source = "WORK_ORDER" };

    [Fact]
    public async Task ReturnsOnlyIndividuallyAuthorizedOrdersInTheSelectedField()
    {
        var rows = new List<WorkOrderSummary> { Order("allowed"), Order("denied") }; var reads = 0;
        var result = await CompanyWorkOrderListReview.ReadAsync("field", () => { reads++; return Task.FromResult(rows); },
            id => Task.FromResult(id == "allowed"), () => Task.CompletedTask, default);
        Assert.Equal("allowed", Assert.Single(result).InstanceId); Assert.Equal(2, reads);
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("duplicate")]
    [InlineData("source-changed")]
    [InlineData("access-changed")]
    public async Task RefusesInconsistentSourceOrChangingAuthority(string failure)
    {
        var reads = 0; var checks = 0;
        Task<List<WorkOrderSummary>> Read()
        {
            reads++;
            return Task.FromResult(failure switch
            {
                "foreign" => new List<WorkOrderSummary> { Order("one", "other") },
                "duplicate" => new List<WorkOrderSummary> { Order("one"), Order("one") },
                "source-changed" => new List<WorkOrderSummary> { Order(reads == 1 ? "one" : "two") },
                _ => new List<WorkOrderSummary> { Order("one") }
            });
        }
        await Assert.ThrowsAsync<RefusalException>(() => CompanyWorkOrderListReview.ReadAsync("field", Read,
            _ => Task.FromResult(failure != "access-changed" || ++checks == 1), () => Task.CompletedTask, default));
    }

    [Fact]
    public async Task ControllerUsesCurrentSqliteCompanyMembershipBeforeReturningAllowedOrders()
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        fixture.Db.Users.Add(new OilGasUser { Id = "planner", UserName = "planner", IsActive = true });
        fixture.Db.Roles.Add(new IdentityRole { Id = "role", Name = "Planner", NormalizedName = "PLANNER" });
        fixture.Db.UserRoles.Add(new IdentityUserRole<string> { UserId = "planner", RoleId = "role" });
        fixture.Db.Add(new AppCompany { TenantId = "tenant", BusinessAssociateId = "company" });
        var membership = new AppCompanyMembership { TenantId = "tenant", BusinessAssociateId = "company", UserId = "planner", GrantedByUserId = "planner" };
        fixture.Db.Add(membership);
        fixture.Db.Add(new AppUserRoleExtension { TenantId = "tenant", BusinessAssociateId = "company", UserId = "planner", RoleId = "role" });
        await fixture.Db.SaveChangesAsync();
        var companies = new RepositoryCompanyContextResolver(new RepositoryAccessService(fixture.Db));
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await companies.ResolveAsync("planner", new("tenant", "company")))!);
        var company = new CompanyModuleConnection(new ModuleConnectionResolver(fixture.Db, fixture.Drafts.Native.Editor.Object, tenant), tenant, companies);
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("field");
        var access = new Mock<IAccessControlService>();
        access.Setup(x => x.CheckAssetAccessAsync("planner", "field", "FIELD", "READ", null)).ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        access.Setup(x => x.CheckAssetAccessAsync("planner", It.IsAny<string>(), "WORK_ORDER", "READ", null))
            .ReturnsAsync((string _, string id, string _, string? _, string? _) => new AccessCheckResponse { HasAccess = id == "allowed" });
        var orders = new Mock<IWorkOrderService>(); orders.Setup(x => x.GetByFieldAsync("field", null, null)).ReturnsAsync([Order("allowed"), Order("denied")]);
        orders.Setup(x => x.GetByFieldSampleAsync("field", 20, It.IsAny<CancellationToken>())).ReturnsAsync([Order("allowed"), Order("denied")]);
        var controller = new WorkOrderController(orders.Object, Mock.Of<ISchedulingService>(), Mock.Of<IContractorManagementService>(), Mock.Of<IInspectionService>(), field.Object,
            NullLogger<WorkOrderController>.Instance, access.Object, company, Mock.Of<IProcessService>()) { ControllerContext = new ControllerContext { HttpContext = fixture.Http.HttpContext! } };
        var response = Assert.IsType<OkObjectResult>((await controller.GetListAsync()).Result);
        Assert.Equal("allowed", Assert.Single(Assert.IsType<List<WorkOrderSummary>>(response.Value)).InstanceId);
        var sample = Assert.IsType<OkObjectResult>((await controller.GetSampleAsync()).Result);
        Assert.Equal("allowed", Assert.Single(Assert.IsType<List<WorkOrderSummary>>(sample.Value)).InstanceId);
        membership.EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1); await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAnyAsync<Exception>(() => controller.GetListAsync());
        orders.Verify(x => x.GetByFieldAsync("field", null, null), Times.Exactly(2));
        orders.Verify(x => x.GetByFieldSampleAsync("field", 20, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
#endif
