using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository.Tests;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Tests;

public class HSEControllerTests
{
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign-plan")]
    [InlineData("changed-receipt")]
    public async Task VerificationDecisionRequiresExactPlanAndReceipt(string mode)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(fixture).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var selections = new Mock<ICompanyFieldSelectionService>(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(new FieldResponse { FieldId = context.FieldId, SelectionAvailable = true, SelectionContext = context });
        var plan = new CAPlanResolution(context, "local", "plan"); var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(plan);
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), "other", "Inspection completed");
        var original = new CAVerificationDecision(Guid.NewGuid(), new(context, "local", mode == "foreign-plan" ? "other" : "plan", new string('A', 64), [action]), "VERIFIED", "Independent inspection confirmed the repair.");
        var commands = new Mock<ICompanyCorrectiveActionCommands>(); commands.Setup(x => x.RecordVerificationAsync(original)).ReturnsAsync(mode == "changed-receipt" ? original with { Reason = "Different independent inspection evidence." } : original);
        var orchestrator = new Mock<IFieldOrchestrator>(); orchestrator.SetupGet(x => x.CurrentFieldId).Returns(context.FieldId); orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        if (mode == "valid") Assert.Equal(original, Assert.IsType<OkObjectResult>((await controller.RecordVerificationAsync("local", original, selections.Object, commands.Object)).Result).Value);
        else await Assert.ThrowsAsync<InvalidOperationException>(() => controller.RecordVerificationAsync("local", original, selections.Object, commands.Object));
        commands.Verify(x => x.RecordVerificationAsync(original), mode == "foreign-plan" ? Times.Never : Times.Once);
    }
    [Theory]
    [InlineData("close")]
    [InlineData("ca_done")]
    public async Task GenericTransitionCannotBypassVerificationAndClosure(string trigger)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        var controller = new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance);
        await Assert.ThrowsAsync<RefusalException>(() => controller.TransitionAsync("local", new TransitionIncidentRequest { Trigger = trigger }));
        orchestrator.VerifyNoOtherCalls();
        var status = new Beep.OilandGas.PPDM39.Models.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = trigger == "close" ? IncidentState.Closed : IncidentState.PendingCloseOut };
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Policy.ValidateMutationAsync(typeof(Beep.OilandGas.PPDM39.Models.HSE_INCIDENT_RESPONSE), "HSE_INCIDENT_RESPONSE", status, null, PPDMRowMutation.Insert, "actor"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClosureReviewRejectsAnotherPlan(bool foreign)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(fixture).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var selections = new Mock<ICompanyFieldSelectionService>(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(new FieldResponse { FieldId = context.FieldId, SelectionAvailable = true, SelectionContext = context });
        var plan = new CAPlanResolution(context, "local", "plan"); var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(plan);
        var review = new IncidentClosureReview(new(context, "local", foreign ? "other" : "plan", new string('A', 64), []), []);
        var commands = new Mock<ICompanyCorrectiveActionCommands>(); commands.Setup(x => x.ReviewClosureAsync(context, "local", "plan", "actor")).ReturnsAsync(review);
        var orchestrator = new Mock<IFieldOrchestrator>(); orchestrator.SetupGet(x => x.CurrentFieldId).Returns(context.FieldId); orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        if (foreign) await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ReviewClosureAsync("local", selections.Object, commands.Object));
        else Assert.Equal(review, Assert.IsType<OkObjectResult>((await controller.ReviewClosureAsync("local", selections.Object, commands.Object)).Result).Value);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task VerificationReviewRejectsAnotherPlan(bool foreign)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(fixture).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var current = new FieldResponse { FieldId = context.FieldId, SelectionAvailable = true, SelectionContext = context };
        var selections = new Mock<ICompanyFieldSelectionService>(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(current);
        var plan = new CAPlanResolution(context, "local", "plan"); var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(plan);
        var review = new CAVerificationReview(context, "local", foreign ? "other" : "plan", new string('A', 64), []);
        var commands = new Mock<ICompanyCorrectiveActionCommands>(); commands.Setup(x => x.ReviewVerificationAsync(context, "local", "plan", "actor")).ReturnsAsync(review);
        var orchestrator = new Mock<IFieldOrchestrator>(); orchestrator.SetupGet(x => x.CurrentFieldId).Returns(context.FieldId); orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        if (foreign) await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ReviewVerificationAsync("local", selections.Object, commands.Object));
        else Assert.Equal(review, Assert.IsType<OkObjectResult>((await controller.ReviewVerificationAsync("local", selections.Object, commands.Object)).Result).Value);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletionReviewRejectsAnotherAction(bool foreign)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(fixture).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var current = new FieldResponse { FieldId = context.FieldId, SelectionAvailable = true, SelectionContext = context };
        var selections = new Mock<ICompanyFieldSelectionService>(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(current);
        var plan = new CAPlanResolution(context, "local", "plan"); var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(plan);
        var review = new CACompletionReview(context, "local", "plan", foreign ? 4 : 3, "step", "Repair", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), new string('A', 64));
        var commands = new Mock<ICompanyCorrectiveActionCommands>(); commands.Setup(x => x.ReviewCompletionAsync(context, "local", "plan", 3, "actor")).ReturnsAsync(review);
        var orchestrator = new Mock<IFieldOrchestrator>(); orchestrator.SetupGet(x => x.CurrentFieldId).Returns(context.FieldId); orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        if (foreign) await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ReviewCompletionAsync("local", 3, selections.Object, commands.Object));
        else Assert.Equal(review, Assert.IsType<OkObjectResult>((await controller.ReviewCompletionAsync("local", 3, selections.Object, commands.Object)).Result).Value);
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("empty")]
    [InlineData("changed")]
    public async Task CompletionRequiresExplicitEvidenceAndTheCurrentPlan(string mode)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(fixture).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var current = new FieldResponse { FieldId = context.FieldId, SelectionAvailable = true, SelectionContext = context };
        var selections = new Mock<ICompanyFieldSelectionService>(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(current);
        var plan = new CAPlanResolution(context, "local", mode == "changed" ? "other" : "plan");
        var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(plan);
        var commands = new Mock<ICompanyCorrectiveActionCommands>();
        var orchestrator = new Mock<IFieldOrchestrator>(); orchestrator.SetupGet(x => x.CurrentFieldId).Returns(context.FieldId); orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        var review = new CACompletionReview(context, "local", "plan", 3, "step", "Repair", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), new string('A', 64));
        var submission = new CompleteCASubmission(review, mode == "empty" ? " " : "Valve inspected");
        if (mode == "valid") Assert.Equal(new CompleteCAReceipt(context, "local", "plan", 3, "step", submission.Notes), Assert.IsType<OkObjectResult>((await controller.CompleteCAAsync("local", 3, submission, selections.Object, commands.Object)).Result).Value);
        else if (mode == "empty") Assert.IsType<BadRequestObjectResult>((await controller.CompleteCAAsync("local", 3, submission, selections.Object, commands.Object)).Result);
        else await Assert.ThrowsAsync<InvalidOperationException>(() => controller.CompleteCAAsync("local", 3, submission, selections.Object, commands.Object));
        commands.Verify(x => x.RecordReviewedCompletionAsync(review, submission.Notes, "actor"), mode == "valid" ? Times.Once : Times.Never);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CorrectiveActionOptionsRequireTheCurrentIncidentPlan(bool foreign)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(fixture).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var current = new FieldResponse { FieldId = context.FieldId, SelectionAvailable = true, SelectionContext = context };
        var selections = new Mock<ICompanyFieldSelectionService>(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(current);
        var plan = new CAPlanResolution(context, "local", "plan");
        var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(plan);
        var options = new CorrectiveActionOptions(foreign ? context with { Company = new("tenant", "other") } : context, "local", "plan", new string('A', 64), ["TYPE"], ["person"]);
        var commands = new Mock<ICompanyCorrectiveActionCommands>(); commands.Setup(x => x.ReviewActionOptionsAsync(context, "local", "plan", "actor")).ReturnsAsync(options);
        var orchestrator = new Mock<IFieldOrchestrator>(); orchestrator.SetupGet(x => x.CurrentFieldId).Returns(context.FieldId); orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        if (foreign) await Assert.ThrowsAnyAsync<Exception>(() => controller.ReviewCAOptionsAsync("local", selections.Object, commands.Object));
        else Assert.Equal(options, Assert.IsType<OkObjectResult>((await controller.ReviewCAOptionsAsync("local", selections.Object, commands.Object)).Result).Value);
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("changed-before")]
    [InlineData("changed-after")]
    [InlineData("changed-options")]
    [InlineData("recovered")]
    public async Task CorrectiveActionRequiresTheOriginalCompanyAndFieldBeforeAndAfterWriting(string mode)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(fixture).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var request = CompanyCorrectiveActionCommandTests.Addition();
        var current = new FieldResponse { FieldId = context.FieldId, SelectionAvailable = true, SelectionContext = context };
        var changed = new FieldResponse { FieldId = context.FieldId, SelectionAvailable = true, SelectionContext = context with { Revision = "changed" } };
        var selections = new Mock<ICompanyFieldSelectionService>();
        selections.SetupSequence(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(mode == "changed-before" ? changed : current).ReturnsAsync(current).ReturnsAsync(mode == "changed-after" ? changed : current);
        var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(new CAPlanResolution(context, "local", "plan"));
        var options = new CorrectiveActionOptions(context, "local", "plan", new string(mode == "changed-options" ? 'B' : 'A', 64), [request.CAType], [request.ResponsibleBaId]);
        var commands = new Mock<ICompanyCorrectiveActionCommands>();
        commands.Setup(x => x.ReviewActionOptionsAsync(context, "local", "plan", "actor")).ReturnsAsync(options);
        commands.Setup(x => x.AddCorrectiveActionAsync(context, "local", "plan", request, "actor")).ReturnsAsync(request.ActionId.ToString("N"));
        if (mode == "recovered") commands.Setup(x => x.FindRecordedActionAsync(context, "local", "plan", request, "actor")).ReturnsAsync(request.ActionId.ToString("N"));
        var orchestrator = new Mock<IFieldOrchestrator>(); orchestrator.SetupGet(x => x.CurrentFieldId).Returns(context.FieldId); orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        var submission = new AddCASubmission(context, request, "plan", new string('A', 64));
        if (mode is "valid" or "recovered") Assert.Equal(new AddCAReceipt(context, "local", request.ActionId, request.ActionId.ToString("N")), Assert.IsType<OkObjectResult>((await controller.AddCAAsync("local", submission, selections.Object, commands.Object)).Result).Value);
        else if (mode == "changed-options") await Assert.ThrowsAsync<InvalidOperationException>(() => controller.AddCAAsync("local", submission, selections.Object, commands.Object));
        else await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.AddCAAsync("local", submission, selections.Object, commands.Object));
        commands.Verify(x => x.AddCorrectiveActionAsync(context, "local", "plan", request, "actor"), mode is "changed-before" or "changed-options" or "recovered" ? Times.Never : Times.Once);
        if (mode == "recovered") commands.Verify(x => x.ReviewActionOptionsAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task GetIncidentAsync_ReturnsNotFound_WhenMissing()
    {
        const string incidentId = "INC-404";
        var hse = new Mock<IFieldHSEService>(MockBehavior.Strict);
        hse.Setup(s => s.GetIncidentAsync(incidentId)).ReturnsAsync((HSEIncidentRecord?)null);

        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.Setup(s => s.GetHSEService()).Returns(hse.Object);

        var controller = new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance);

        var result = await controller.GetIncidentAsync(incidentId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        hse.VerifyAll();
        orchestrator.VerifyAll();
    }

    [Fact]
    public async Task TransitionAsync_LeavesAnUnexpectedFailureToTheApiHandler()
    {
        const string incidentId = "INC-MISSING";
        var hse = new Mock<IFieldHSEService>(MockBehavior.Strict);

        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.Setup(s => s.GetHSEService()).Returns(hse.Object);

        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        var request = new TransitionIncidentRequest { Trigger = "investigate", Reason = null };

        // OILGAS-CATCH-01: the controller no longer answers a failure itself; it reaches the API's exception handler,
        // which reports it and answers 500 with its reference and never its text (ExceptionAnswerTests).
        await Assert.ThrowsAsync<MockException>(() => controller.TransitionAsync(incidentId, request));
        orchestrator.VerifyAll();
    }

    [Fact]
    public async Task GetIncidentsAsync_ReturnsBadRequest_WhenCurrentFieldMissing()
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(s => s.CurrentFieldId).Returns((string?)null);

        var controller = new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance);

        var result = await controller.GetIncidentsAsync(null, null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        orchestrator.VerifyAll();
    }

    public static IEnumerable<object[]> WriteAccountCases =>
        from operation in typeof(HSEController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        where operation.GetCustomAttributes<HttpMethodAttribute>().Any(attribute => attribute.HttpMethods.Any(verb => verb != "GET"))
        from fault in new[] { "inactive", "unresolved", "anonymous" }
        select new object[] { operation.Name, fault };
    [Theory] [MemberData(nameof(WriteAccountCases))]
    public async Task HSE_actions_require_the_current_active_app_account_before_domain_calls(string operation, string fault)
    {
        using var fixture = await CompanyCorrectiveActionCommandTests.Ready();
        if (fault == "inactive") { fixture.Db.Users.Single().IsActive = false; await fixture.Db.SaveChangesAsync(); }
        else fixture.Request.User = fault == "anonymous" ? new ClaimsPrincipal(new ClaimsIdentity()) : new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "unresolved-account")], "fixture"));
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        var controller = SignedIn(new HSEController(orchestrator.Object, fixture.Account, NullLogger<HSEController>.Instance), fixture);
        var method = typeof(HSEController).GetMethod(operation)!;
        var args = method.GetParameters().Select(parameter => parameter.ParameterType == typeof(string) ? "local" : parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null).ToArray();
        var refusal = await Assert.ThrowsAsync<RefusalException>(() => (Task)method.Invoke(controller, args)!);
        Assert.Equal(RefusalKind.Forbidden, refusal.Kind); orchestrator.VerifyNoOtherCalls(); Assert.Empty(fixture.Writes);
    }

    private static HSEController SignedIn(HSEController controller, Fixture fixture)
    {
        fixture.Http.HttpContext = fixture.Request;
        controller.ControllerContext = new ControllerContext { HttpContext = fixture.Request };
        return controller;
    }
}
