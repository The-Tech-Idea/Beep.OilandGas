using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LeaseAcquisition.Models;
using Beep.OilandGas.ApiService.Controllers.Operations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

// Actual controller/contract checks. No stand-in IdentityServer/principal pipeline is used or claimed.
public sealed class LeaseAcquisitionControllerTests
{
    private static readonly LeaseDataScope Scope = new(new("tenant", "company"), new("selected", "fingerprint"), "actor", "lease", "component", "status", "UTC");
    private static CreateLeaseAcquisition Intent() => new() { CommandId = Guid.NewGuid(), PropertyId = "A", Context = new(Scope.Company, "fingerprint", "actor", "A", "lease", "component", "status", "UTC") };
    private static Mock<ICompanyLeaseDataAccess> Access()
    {
        var access = new Mock<ICompanyLeaseDataAccess>(MockBehavior.Strict);
        access.Setup(value => value.ResolveAsync()).ReturnsAsync(Scope);
        access.Setup(value => value.RequireFieldAsync(Scope, "A", "WRITE", "actor")).Returns(Task.CompletedTask);
        return access;
    }
    [Fact]
    public void One_explicit_FIELD_resource_has_standard_auth_and_Administrator_guards_on_all_mutations()
    {
        var controller = typeof(LeaseAcquisitionController);
        Assert.Equal("api/fields/{fieldId}/lease-acquisition", Assert.Single(controller.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>()).Template);
        Assert.Single(controller.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        foreach (var method in new[] { "CreateLeaseAcquisition", "ReconcileCreation", "UpdateLeaseStatus", "GetCreationContext" })
            Assert.Equal("Administrator", Assert.Single(controller.GetMethod(method)!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Roles);
    }
    [Theory]
    [InlineData("FieldId")] [InlineData("PropertyId")]
    public async Task A_query_cannot_replace_the_route_FIELD(string name)
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict); var access = new Mock<ICompanyLeaseDataAccess>(MockBehavior.Strict);
        var controller = new LeaseAcquisitionController(core.Object, access.Object);
        await Assert.ThrowsAsync<RefusalException>(() => controller.GetAvailableLeases("A", new() { [name] = "B" }));
        core.VerifyNoOtherCalls(); access.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task A_list_injects_the_exact_FIELD_without_mutating_the_callers_filters()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict); var filters = new Dictionary<string, string> { ["LeaseId"] = "lease" };
        core.Setup(value => value.GetAvailableLeasesAsync(It.Is<Dictionary<string, string>>(data => data["FieldId"] == "A" && data["LeaseId"] == "lease"))).ReturnsAsync([]);
        var controller = new LeaseAcquisitionController(core.Object, Mock.Of<ICompanyLeaseDataAccess>());
        Assert.IsType<OkObjectResult>((await controller.GetAvailableLeases("A", filters)).Result); Assert.False(filters.ContainsKey("FieldId")); core.VerifyAll();
    }
    [Theory]
    [InlineData("missing")] [InlineData("property")] [InlineData("context")]
    public async Task Create_and_reconcile_assert_the_route_before_any_actor_or_domain_call(string fault)
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict); var access = new Mock<ICompanyLeaseDataAccess>(MockBehavior.Strict); var intent = Intent();
        if (fault == "property") intent.PropertyId = "B";
        if (fault == "context") intent.Context = intent.Context! with { FieldId = "B" };
        var controller = new LeaseAcquisitionController(core.Object, access.Object);
        await Assert.ThrowsAsync<RefusalException>(() => controller.CreateLeaseAcquisition("A", fault == "missing" ? null : intent));
        await Assert.ThrowsAsync<RefusalException>(() => controller.ReconcileCreation("A", fault == "missing" ? null : intent));
        core.VerifyNoOtherCalls(); access.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Status_requires_the_live_gate_actor_and_passes_the_exact_FIELD_to_the_canonical_owner()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict); var access = Access();
        core.Setup(value => value.UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A")).Returns(Task.CompletedTask);
        var controller = new LeaseAcquisitionController(core.Object, access.Object);
        Assert.IsType<OkResult>(await controller.UpdateLeaseStatus("A", "lease", new() { Status = "INACTIVE" }));
        access.VerifyAll(); core.VerifyAll();
    }
    [Fact]
    public async Task An_actor_gate_refusal_prevents_a_status_service_call()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict); var access = Access();
        access.Setup(value => value.RequireFieldAsync(Scope, "A", "WRITE", "actor")).ThrowsAsync(RefusalException.Forbidden("Revoked"));
        var controller = new LeaseAcquisitionController(core.Object, access.Object);
        await Assert.ThrowsAsync<RefusalException>(() => controller.UpdateLeaseStatus("A", "lease", new() { Status = "ACTIVE" })); core.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Creation_freezes_the_existing_intent_before_async_actor_resolution_and_returns_the_typed_receipt()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict); var access = Access(); var intent = Intent(); var id = intent.CommandId;
        access.Setup(value => value.ResolveAsync()).Returns(() => { intent.CommandId = Guid.NewGuid(); intent.PropertyId = "B"; return Task.FromResult(Scope); });
        core.Setup(value => value.CreateLeaseAcquisitionAsync(It.Is<CreateLeaseAcquisition>(body => body.CommandId == id && body.PropertyId == "A"), "actor")).ReturnsAsync(id.ToString("N"));
        var result = await new LeaseAcquisitionController(core.Object, access.Object).CreateLeaseAcquisition("A", intent);
        var receipt = Assert.IsType<LeaseCreationReceipt>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(id, receipt.CommandId); Assert.Equal(id.ToString("N"), receipt.LeaseId); Assert.Equal("Recorded", receipt.State); core.VerifyAll();
    }
    [Fact]
    public async Task Domain_refusals_propagate_to_the_existing_API_handler()
    {
        var refusal = RefusalException.NotFound("No lease in FIELD A"); var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        core.Setup(value => value.EvaluateLeaseAsync("lease", "A")).ThrowsAsync(refusal);
        Assert.Same(refusal, await Assert.ThrowsAsync<RefusalException>(() => new LeaseAcquisitionController(core.Object, Mock.Of<ICompanyLeaseDataAccess>()).EvaluateLease("A", "lease")));
    }
}
