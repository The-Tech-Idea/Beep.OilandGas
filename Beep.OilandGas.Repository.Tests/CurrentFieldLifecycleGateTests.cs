using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.LifeCycle.Services.HSE;
using System.Text;
using System.Security.Claims;
using System.Net;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

// Actual filter/profile/company/registry/Beep row policy; orchestrator and permission decision are explicit boundaries.
[Collection(CurrentCompanyHttpAcceptanceCollection.Name)]
public sealed partial class CurrentFieldLifecycleGateTests
{
    private static RepositoryUserProfileService Profile(Fixture f)
    { f.Http.HttpContext = f.Request; return new(f.Db, new RepositoryApplicationAuthorizationReader(f.Gate), new UpperInvariantLookupNormalizer(), f.Account, f.Gate, f.Service); }
    private static async Task<Fixture> Ready()
    {
        var f = await Fixture.Create(); await f.Grant("FIELD", "A"); await f.Grant("FIELD", "B");
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" }]); return f;
    }
    private static async Task Save(RepositoryUserProfileService owner, string field = "A") => await owner.SetCurrentFieldSelectionAsync(new() { FieldId = field, Context = (await owner.GetCurrentFieldSelectionAsync()).SelectionContext });
    private sealed class Boundary : IDisposable
    {
        public Mock<IFieldOrchestrator> Orchestrator { get; } = new(MockBehavior.Strict);
        public Mock<IAccessControlService> Access { get; } = new(MockBehavior.Strict);
        public Mock<IAuthorizationObservabilityService> Observations { get; } = new();
        public AuthorizationFilterContext Context { get; }
        public string? Field;
        public Func<Task>? AfterRestore;
        public Boundary(Fixture f, ICompanyFieldSelectionService owner, bool permitted = true)
        {
            f.Http.HttpContext = f.Request;
            var services = new ServiceCollection(); services.AddSingleton(f.Account); services.AddSingleton(f.Gate);
            services.AddSingleton(owner); services.AddSingleton(Access.Object); services.AddSingleton(Orchestrator.Object); services.AddSingleton(Observations.Object);
            f.Request.RequestServices = services.BuildServiceProvider();
            Orchestrator.SetupGet(x => x.CurrentFieldId).Returns(() => Field);
            Orchestrator.Setup(x => x.SetActiveFieldAsync(It.IsAny<string>())).Returns(async (string id) => { Field = id; if (AfterRestore is not null) await AfterRestore(); return true; });
            Orchestrator.Setup(x => x.ClearActiveField()).Callback(() => Field = null);
            Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", It.IsAny<string?>(), null)).ReturnsAsync(new AccessCheckResponse { HasAccess = permitted });
            Context = new(new ActionContext(f.Request, new RouteData(), new ActionDescriptor()), []);
        }
        public void Dispose() { (Context.HttpContext.RequestServices as IDisposable)?.Dispose(); }
    }
    [Fact]
    public async Task IncidentVerificationAndClosureEvidenceHttpMustNotBeStored()
    {
        using var f = await CompanyCorrectiveActionCommandTests.Ready();
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var context = (await owner.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var plan = new CAPlanResolution(context, "local", "plan");
        var verification = new CAVerificationReview(context, "local", "plan", new string('A', 64),
            [new(1, "step", "Inspect the recorded valve", "COMPLETED", "operator", DateTime.UnixEpoch.AddDays(2), DateTime.UnixEpoch.AddDays(1), "operator", "Valve pressure test confirmed.")]);
        var decision = new CAVerificationDecision(Guid.NewGuid(), verification, "VERIFIED", "Independent review confirms the recorded corrective evidence.");
        var closure = new IncidentClosureReview(verification, [decision]);
        var hse = new Mock<IFieldHSEService>(MockBehavior.Strict);
        hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(plan);
        var commands = new Mock<ICompanyCorrectiveActionCommands>(MockBehavior.Strict);
        commands.Setup(x => x.ReviewVerificationAsync(context, "local", "plan", "actor")).ReturnsAsync(verification);
        commands.Setup(x => x.RecordVerificationAsync(It.Is<CAVerificationDecision>(d => d.IntentId == decision.IntentId))).ReturnsAsync(decision);
        commands.Setup(x => x.ReviewClosureAsync(context, "local", "plan", "actor")).ReturnsAsync(closure);
        using var boundary = new Boundary(f, owner);
        boundary.Orchestrator.Setup(x => x.GetHSEService()).Returns(hse.Object);
        await using var host = await HttpHost.Start(f, owner, boundary, true, typeof(HSEController), services =>
        {
            services.AddSingleton(commands.Object);
            services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
            services.AddAuthorization(options => options.AddPolicy(Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents,
                policy => policy.RequireAuthenticatedUser().RequireRole("Administrator")));
        });
        var root = host.Url + "/api/field/current/hse/incidents/local";
        static async Task<T> RequirePrivate<T>(HttpResponseMessage response)
        {
            using (response)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(response.Headers.CacheControl?.NoStore);
                Assert.True(response.Headers.CacheControl?.NoCache);
                Assert.Contains("no-cache", response.Headers.Pragma.Select(value => value.Name));
                return (await response.Content.ReadFromJsonAsync<T>())!;
            }
        }
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(verification), System.Text.Json.JsonSerializer.Serialize(await RequirePrivate<CAVerificationReview>(await host.Http.GetAsync(root + "/cas/verification-review"))));
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(decision), System.Text.Json.JsonSerializer.Serialize(await RequirePrivate<CAVerificationDecision>(await host.Http.PostAsJsonAsync(root + "/cas/verification-decision", decision))));
        var actual = await RequirePrivate<IncidentClosureReview>(await host.Http.GetAsync(root + "/closure-review"));
        Assert.Equal(context, actual.Actions.Context);
        Assert.Equal(decision.IntentId, Assert.Single(actual.Decisions).IntentId);
        commands.VerifyAll();
        Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("success")]
    [InlineData("stale")]
    [InlineData("denied")]
    [InlineData("anonymous")]
    public async Task ReviewedCorrectiveCompletionHttpUsesTheNativeOwner(string scenario)
    {
        using var f = await CompanyCorrectiveActionCommandTests.Ready(); CompanyCorrectiveActionCommandTests.Target(f);
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var context = (await owner.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var request = CompanyCorrectiveActionCommandTests.Addition();
        await commands.AddCorrectiveActionAsync(context, "local", CompanyCorrectiveActionAuthorityTests.Plan, request, "actor");
        var action = f.Rows.OfType<PROJECT_STEP>().Single(row => row.STEP_ID == request.ActionId.ToString("N"));
        var sequence = checked((int)action.STEP_SEQ_NO); var count = f.Writes.Count;
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy, commands);
        using var boundary = new Boundary(f, owner, scenario != "denied");
        boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(facade);
        await using var host = await HttpHost.Start(f, owner, boundary, scenario != "anonymous", typeof(HSEController), services =>
        {
            services.AddSingleton<ICompanyCorrectiveActionCommands>(commands);
            services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
            services.AddAuthorization(options => options.AddPolicy(Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents,
                policy => policy.RequireAuthenticatedUser().RequireRole("Administrator")));
        });
        host.Http.Timeout = TimeSpan.FromMinutes(4);
        var root = host.Url + $"/api/field/current/hse/incidents/local/cas/{sequence}";
        using var response = await host.Http.GetAsync(root + "/completion-review");
        if (scenario is "denied" or "anonymous")
        {
            Assert.Equal(scenario == "denied" ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(count, f.Writes.Count); return;
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var review = (await response.Content.ReadFromJsonAsync<CACompletionReview>())!;
        Assert.Equal(action.STEP_ID, review.StepId); Assert.Equal("recorded-target", review.ResponsiblePerson);
        if (scenario == "stale") action.DUE_DATE = request.DueDate.AddDays(1);
        var submitted = new CompleteCASubmission(review, "Valve inspected and tested");
        using var first = await host.Http.PostAsJsonAsync(root + "/complete", submitted);
        if (scenario == "stale") { Assert.Equal(HttpStatusCode.Conflict, first.StatusCode); Assert.Equal(count, f.Writes.Count); return; }
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(new CompleteCAReceipt(context, "local", review.PlanId, sequence, review.StepId, submitted.Notes), await first.Content.ReadFromJsonAsync<CompleteCAReceipt>());
        Assert.Equal(count + 2, f.Writes.Count);
        using var retry = await host.Http.PostAsJsonAsync(root + "/complete", submitted);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode); Assert.Equal(count + 2, f.Writes.Count);
    }
    [Fact]
    public async Task A_fresh_lifecycle_request_restores_the_actual_profile_FIELD_for_the_resolved_account()
    {
        using var f = await Ready(); var owner = Profile(f); await Save(owner); using var boundary = new Boundary(f, owner);
        f.Request.User.AddIdentity(new ClaimsIdentity([new(ClaimTypes.Name, "display"), new("sub", "external-subject")], "external"));
        await new RequireCurrentFieldAccessAttribute("ViewProduction").OnAuthorizationAsync(boundary.Context);
        Assert.Null(boundary.Context.Result); Assert.Equal("A", boundary.Field);
        boundary.Access.Verify(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", "ViewProduction", null), Times.Exactly(2));
        boundary.Orchestrator.Verify(x => x.SetActiveFieldAsync("A"), Times.Once);
        boundary.Observations.Verify(x => x.RecordPolicyEvaluationAsync(It.Is<AuthorizationObservation>(row => row.Decision == "Allowed" && row.UserId == "actor" && row.AssetId == "A")), Times.Once);
        Assert.All(f.Connections, name => Assert.Equal("selected-tenant", name)); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task First_selection_is_explicitly_required_and_never_uses_the_orchestrator_as_a_default()
    {
        using var f = await Ready(); var owner = Profile(f); using var boundary = new Boundary(f, owner); boundary.Field = "B";
        await new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(boundary.Context);
        Assert.IsType<BadRequestObjectResult>(boundary.Context.Result); boundary.Orchestrator.Verify(x => x.SetActiveFieldAsync(It.IsAny<string>()), Times.Never); boundary.Access.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task An_anonymous_request_is_rejected_before_profile_or_domain_calls()
    {
        using var f = await Ready(); var owner = Profile(f); using var boundary = new Boundary(f, owner); f.Request.User = new(new ClaimsIdentity());
        await new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(boundary.Context); Assert.IsType<UnauthorizedResult>(boundary.Context.Result);
        boundary.Orchestrator.VerifyNoOtherCalls(); boundary.Access.VerifyNoOtherCalls(); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task A_missing_application_account_never_falls_back_to_external_subject_or_display_name()
    {
        using var f = await Ready(); var owner = Profile(f); using var boundary = new Boundary(f, owner);
        f.Request.User = new(new ClaimsIdentity([new("sub", "actor"), new(ClaimTypes.NameIdentifier, "actor"), new(ClaimTypes.Name, "actor")], "external"));
        await Assert.ThrowsAsync<RefusalException>(() => new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(boundary.Context));
        boundary.Access.VerifyNoOtherCalls(); boundary.Orchestrator.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Independent_application_permission_refusal_never_restores_the_lifecycle_FIELD()
    {
        using var f = await Ready(); var owner = Profile(f); await Save(owner); using var boundary = new Boundary(f, owner, false);
        await new RequireCurrentFieldAccessAttribute("ViewProduction").OnAuthorizationAsync(boundary.Context); Assert.IsType<ForbidResult>(boundary.Context.Result);
        boundary.Orchestrator.Verify(x => x.SetActiveFieldAsync(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Permission_revocation_during_restoration_clears_the_FIELD_and_never_reports_allowed()
    {
        using var f = await Ready(); var owner = Profile(f); await Save(owner); using var boundary = new Boundary(f, owner);
        var permitted = true;
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", "ViewProduction", null))
            .ReturnsAsync(() => new AccessCheckResponse { HasAccess = permitted });
        boundary.AfterRestore = () => { permitted = false; return Task.CompletedTask; };
        await Assert.ThrowsAsync<RefusalException>(() => new RequireCurrentFieldAccessAttribute("ViewProduction").OnAuthorizationAsync(boundary.Context));
        Assert.Null(boundary.Field);
        boundary.Observations.Verify(x => x.RecordPolicyEvaluationAsync(It.Is<AuthorizationObservation>(row => row.Decision == "Allowed")), Times.Never);
    }
    [Theory] [InlineData("route")] [InlineData("query")] [InlineData("multiple")]
    public async Task Caller_FIELD_assertions_cannot_override_the_reviewed_preference(string location)
    {
        using var f = await Ready(); var owner = Profile(f); await Save(owner); using var boundary = new Boundary(f, owner);
        if (location == "route") boundary.Context.RouteData.Values["fieldId"] = "B";
        else f.Request.Request.QueryString = new(location == "multiple" ? "?fieldId=A&fieldId=A" : "?fieldId=B");
        await Assert.ThrowsAsync<RefusalException>(() => new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(boundary.Context));
        boundary.Access.VerifyNoOtherCalls(); boundary.Orchestrator.Verify(x => x.SetActiveFieldAsync(It.IsAny<string>()), Times.Never);
    }
    [Theory] [InlineData("preferences")] [InlineData("grant")] [InlineData("account")] [InlineData("binding")]
    public async Task Changes_during_restoration_clear_the_request_context_and_cannot_be_reported_allowed(string fault)
    {
        using var f = await Ready(); var owner = Profile(f); await Save(owner); using var boundary = new Boundary(f, owner);
        boundary.AfterRestore = async () => {
            if (fault == "preferences") await owner.UpdateUserPreferencesAsync("actor", "{\"density\":\"changed\"}");
            if (fault == "grant") (await f.Db.Set<AppUserAssetAccess>().SingleAsync(row => row.AssetId == "A")).IsActive = false;
            if (fault == "account") (await f.Db.Users.SingleAsync()).IsActive = false;
            if (fault == "binding") (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(boundary.Context));
        Assert.Null(boundary.Field); boundary.Observations.Verify(x => x.RecordPolicyEvaluationAsync(It.Is<AuthorizationObservation>(row => row.Decision == "Allowed")), Times.Never);
    }
    [Fact]
    public async Task Failed_provider_evidence_is_not_reported_as_a_missing_preference()
    {
        using var f = await Ready(); var owner = Profile(f); await Save(owner); using var boundary = new Boundary(f, owner);
        f.AfterRead = () => throw new InvalidOperationException("recording provider failed");
        await Assert.ThrowsAnyAsync<Exception>(() => new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(boundary.Context));
        Assert.Null(boundary.Context.Result); boundary.Orchestrator.Verify(x => x.SetActiveFieldAsync(It.IsAny<string>()), Times.Never);
        boundary.Observations.Verify(x => x.RecordPolicyEvaluationAsync(It.Is<AuthorizationObservation>(row => row.Decision == "Error")), Times.Once);
    }

    [Fact]
    public async Task Missing_guard_dependencies_fail_closed()
    {
        var http = new DefaultHttpContext { User = new(new ClaimsIdentity([new("party_id", "actor")], "test")), RequestServices = new ServiceCollection().BuildServiceProvider() };
        using var services = (ServiceProvider)http.RequestServices;
        var context = new AuthorizationFilterContext(new ActionContext(http, new RouteData(), new ActionDescriptor()), []);
        await new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(context);
        Assert.Equal(500, Assert.IsType<StatusCodeResult>(context.Result).StatusCode);
    }
    [Theory] [InlineData("false")] [InlineData("foreign")] [InlineData("cancelled")]
    public async Task Failed_or_cancelled_restoration_clears_ephemeral_context(string fault)
    {
        using var f = await Ready(); var owner = Profile(f); await Save(owner); using var boundary = new Boundary(f, owner);
        boundary.Orchestrator.Setup(x => x.SetActiveFieldAsync("A")).Returns(() => {
            boundary.Field = fault == "foreign" ? "B" : "A";
            return fault == "cancelled" ? Task.FromException<bool>(new OperationCanceledException()) : Task.FromResult(fault != "false");
        });
        if (fault == "cancelled") await Assert.ThrowsAsync<OperationCanceledException>(() => new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(boundary.Context));
        else await Assert.ThrowsAsync<RefusalException>(() => new RequireCurrentFieldAccessAttribute().OnAuthorizationAsync(boundary.Context));
        Assert.Null(boundary.Field);
        boundary.Observations.Verify(x => x.RecordPolicyEvaluationAsync(It.Is<AuthorizationObservation>(row => row.Decision == "Allowed")), Times.Never);
    }

    public static IEnumerable<object[]> LifecycleRoutes =>
        from route in new[] { "summary", "wells", "statistics", "timeline", "dashboard" }
        from authority in new[] { "anonymous", "missing-selection", "denied", "allowed" }
        select new object[] { route, authority };
    [Theory] [MemberData(nameof(LifecycleRoutes))]
    public async Task Actual_lifecycle_HTTP_routes_enforce_the_reviewed_profile_guard(string route, string authority)
    {
        using var f = await Ready(); var owner = Profile(f);
        if (authority != "missing-selection") await Save(owner);
        using var boundary = new Boundary(f, owner, authority != "denied");
        boundary.Orchestrator.Setup(x => x.GetFieldLifecycleSummaryAsync()).ReturnsAsync(new FieldLifecycleSummary());
        boundary.Orchestrator.Setup(x => x.GetFieldWellsAsync()).ReturnsAsync([]);
        boundary.Orchestrator.Setup(x => x.GetFieldStatisticsAsync()).ReturnsAsync(new FieldStatistics());
        boundary.Orchestrator.Setup(x => x.GetFieldTimelineAsync()).ReturnsAsync(new FieldTimeline());
        boundary.Orchestrator.Setup(x => x.GetFieldDashboardAsync()).ReturnsAsync(new FieldDashboard());
        await using var host = await HttpHost.Start(f, owner, boundary, authority != "anonymous");
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/" + route);
        Assert.Equal(authority switch { "anonymous" => HttpStatusCode.Unauthorized, "missing-selection" => HttpStatusCode.BadRequest, "denied" => HttpStatusCode.Forbidden, _ => HttpStatusCode.OK }, response.StatusCode);
        boundary.Orchestrator.Verify(x => x.SetActiveFieldAsync("A"), authority == "allowed" ? Times.Once() : Times.Never());
        Assert.Equal(authority == "allowed" ? 1 : 0, boundary.Orchestrator.Invocations.Count(x => x.Method.Name == route switch {
            "summary" => nameof(IFieldOrchestrator.GetFieldLifecycleSummaryAsync), "wells" => nameof(IFieldOrchestrator.GetFieldWellsAsync),
            "statistics" => nameof(IFieldOrchestrator.GetFieldStatisticsAsync), "timeline" => nameof(IFieldOrchestrator.GetFieldTimelineAsync), _ => nameof(IFieldOrchestrator.GetFieldDashboardAsync) }));
        Assert.Empty(f.Writes); Assert.All(f.Connections, connection => Assert.Equal("selected-tenant", connection));
    }
    [Theory]
    [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("read-grant", 403)] [InlineData("missing-manage", 403)]
    [InlineData("foreign-target", 409)] [InlineData("invalid-target", 400)] [InlineData("missing-body", 400)] [InlineData("allowed", 204)]
    public async Task Actual_HSE_assignment_HTTP_route_reuses_the_facade_command_and_refuses_unreviewed_authority(string authority, int expectedStatus)
    {
        using var f = await CompanyCorrectiveActionCommandTests.Ready(authority == "read-grant" ? "READ" : "WRITE", authority != "missing-manage");
        CompanyCorrectiveActionCommandTests.Target(f);
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy, commands);
        using var boundary = new Boundary(f, owner, authority != "denied-field");
        boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(facade);
        await using var host = await HttpHost.Start(f, owner, boundary, authority != "anonymous", typeof(HSEController));
        var target = authority == "foreign-target" ? "foreign-person" : authority == "invalid-target" ? " recorded-target" : "recorded-target";
        var json = authority == "missing-body" ? "" : System.Text.Json.JsonSerializer.Serialize(new { BusinessAssociateId = target, UserId = "external-actor" });
        var url = host.Url + "/api/field/current/hse/incidents/local/cas/1/responsible";
        using var response = await host.Http.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(expectedStatus, (int)response.StatusCode);
        if (authority == "allowed")
        {
            var assignment = Assert.Single(f.Rows.OfType<PROJECT_STEP_BA>(), row => row.PROJECT_ID == CompanyCorrectiveActionAuthorityTests.Plan && row.ACTIVE_IND == "Y");
            Assert.Equal("recorded-target", assignment.BUSINESS_ASSOCIATE_ID); Assert.Equal(23, assignment.ROLE_SEQ_NO); Assert.Equal("actor", assignment.ROW_CREATED_BY); Assert.Equal("actor", assignment.ROW_CHANGED_BY);
            Assert.Equal(2, f.Writes.Count); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
            using var retry = await host.Http.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.NoContent, retry.StatusCode); Assert.Equal(2, f.Writes.Count); Assert.Null(f.LeaseActiveTransaction);
        }
        else { Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions); }
    }
    [Theory]
    [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("read-grant", 403)] [InlineData("missing-manage", 403)]
    [InlineData("foreign-target", 409)] [InlineData("missing-id", 400)] [InlineData("missing-body", 400)] [InlineData("allowed", 200)]
    public async Task Actual_HSE_addition_HTTP_route_uses_the_shared_intent_and_reconciles_a_repeated_request(string authority, int expectedStatus)
    {
        using var f = await CompanyCorrectiveActionCommandTests.Ready(authority == "read-grant" ? "READ" : "WRITE", authority != "missing-manage");
        CompanyCorrectiveActionCommandTests.Target(f);
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy, commands);
        using var boundary = new Boundary(f, owner, authority != "denied-field");
        boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(facade);
        await using var host = await HttpHost.Start(f, owner, boundary, authority != "anonymous", typeof(HSEController));
        host.Http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        var request = CompanyCorrectiveActionCommandTests.Addition();
        var json = authority == "missing-body" ? "" : System.Text.Json.JsonSerializer.Serialize(new {
            ActionId = authority == "missing-id" ? Guid.Empty : request.ActionId, request.CADescription, request.CAType, request.DueDate,
            ResponsibleBaId = authority == "foreign-target" ? "foreign-person" : request.ResponsibleBaId, UserId = "external-actor" });
        var url = host.Url + "/api/field/current/hse/incidents/local/cas";
        using var response = await host.Http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(expectedStatus, (int)response.StatusCode);
        if (authority == "allowed")
        {
            Assert.Equal(request.ActionId.ToString("N"), System.Text.Json.JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync()));
            Assert.Equal(3, f.Writes.Count); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
            var step = Assert.Single(f.Rows.OfType<PROJECT_STEP>(), row => row.STEP_ID == request.ActionId.ToString("N")); Assert.Equal("actor", step.ROW_CREATED_BY);
            using var retry = await host.Http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.OK, retry.StatusCode); Assert.Equal(3, f.Writes.Count);
            using var collision = await host.Http.PostAsync(url, new StringContent(json.Replace(request.CADescription, "Different intent"), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Conflict, collision.StatusCode); Assert.Equal(3, f.Writes.Count); Assert.Null(f.LeaseActiveTransaction);
        }
        else { Assert.Empty(f.Writes); }
    }

    [Theory] [InlineData("allowed", 200)] [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("missing-review", 403)] [InlineData("missing-manage", 403)] [InlineData("foreign-incident", 404)]
    public async Task Actual_HSE_restriction_review_HTTP_route_requires_current_reviewer_and_returns_only_qualified_fingerprints(string authority, int expectedStatus)
    {
        using var f = await CompanyCorrectiveActionCommandTests.Ready(manage: authority != "missing-manage");
        CompanyHseRowAccessTests.Restrict(f, true);
        if (authority != "missing-review") { f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ViewRestricted" }); await f.Db.SaveChangesAsync(); }
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy, commands);
        using var boundary = new Boundary(f, owner, authority != "denied-field"); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(facade);
        await using var host = await HttpHost.Start(f, owner, boundary, authority != "anonymous", typeof(HSEController));
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/hse/incidents/" + (authority == "foreign-incident" ? "foreign" : "local") + "/restrictions/review");
        Assert.Equal(expectedStatus, (int)response.StatusCode);
        if (authority == "allowed") {
            var json = await response.Content.ReadAsStringAsync();
            var review = System.Text.Json.JsonSerializer.Deserialize<HseRestrictionReview>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            Assert.Equal("local", review.IncidentId); Assert.Equal("A", review.FieldId); Assert.Equal("company", review.Company.BusinessAssociateId);
            Assert.Equal("recorded-rest", Assert.Single(review.Restrictions).RestrictionId); Assert.Equal(7, review.Restrictions[0].RestrictionVersion); Assert.Equal(64, review.Restrictions[0].DefinitionFingerprint.Length);
            Assert.DoesNotContain("Human-reviewed", json); Assert.Empty(f.EligibilityConfiguration.Companies.Single().HseCaseReadRestrictions);
        }
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("allowed", 200)] [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("foreign-incident", 404)] [InlineData("missing-person", 409)] [InlineData("ambiguous-investigator", 409)]
    public async Task Actual_HSE_participant_HTTP_read_uses_recorded_identity_and_refuses_unreviewed_payload(string fault, int expectedStatus)
    {
        using var f = await CompanyCorrectiveActionCommandTests.Ready();
        f.Rows.AddRange([new R_INCIDENT_BA_ROLE { BA_ROLE = "INVESTIGATOR", ACTIVE_IND = "Y" },
            new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "external-contractor", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 1, PARTY_ROLE_OBS_NO = 1, INVOLVED_PARTY_BA_ID = "local-person", INVOLVED_BA_ROLE = "INVESTIGATOR", COMPANY_BA_ID = "external-contractor", ACTIVE_IND = "Y" }]);
        if (fault == "missing-person") f.Rows.RemoveAll(value => value is BUSINESS_ASSOCIATE ba && ba.BUSINESS_ASSOCIATE_ID == "local-person");
        if (fault == "ambiguous-investigator") f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 2, PARTY_ROLE_OBS_NO = 1, INVOLVED_PARTY_BA_ID = "local-person", INVOLVED_BA_ROLE = "INVESTIGATOR", ACTIVE_IND = "Y" });
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy, commands);
        using var boundary = new Boundary(f, owner, fault != "denied-field"); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(facade);
        await using var host = await HttpHost.Start(f, owner, boundary, fault != "anonymous", typeof(HSEController));
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/hse/incident?incidentId=" + (fault == "foreign-incident" ? "foreign" : "local"));
        Assert.Equal(expectedStatus, (int)response.StatusCode);
        if (fault == "allowed") { var json = await response.Content.ReadAsStringAsync(); Assert.Contains("local-person", json); Assert.DoesNotContain("foreign-person", json); }
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Fact]
    public async Task Incident_HTTP_read_binds_reserved_identifier_after_current_field_authorization()
    {
        const string id = "case/واحد?part#2";
        using var f = await CompanyCorrectiveActionCommandTests.Ready();
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        using var boundary = new Boundary(f, owner, true);
        var hse = new Mock<IFieldHSEService>();
        hse.Setup(service => service.GetIncidentAsync(id)).ReturnsAsync(new Beep.OilandGas.Models.Data.HSE.HSEIncidentRecord { IncidentId = id, FieldId = "A" });
        hse.Setup(service => service.AssignInvestigatorAsync(id, It.IsAny<AssignIncidentInvestigatorRequest>(), "actor")).Returns(Task.CompletedTask);
        boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(hse.Object);
        await using var host = await HttpHost.Start(f, owner, boundary, true, typeof(HSEController));
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/hse/incident?incidentId=" + Uri.EscapeDataString(id));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Beep.OilandGas.Models.Data.HSE.HSEIncidentRecord>();
        Assert.Equal(id, result!.IncidentId); hse.Verify(service => service.GetIncidentAsync(id), Times.Once);
        using var assignment = await host.Http.PutAsJsonAsync(host.Url + "/api/field/current/hse/incident/investigator?incidentId=" + Uri.EscapeDataString(id), new AssignIncidentInvestigatorRequest(new string('A', 64), 1, 1, "person"));
        Assert.Equal(HttpStatusCode.NoContent, assignment.StatusCode);
        hse.Verify(service => service.AssignInvestigatorAsync(id, It.IsAny<AssignIncidentInvestigatorRequest>(), "actor"), Times.Once);
        Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("allowed", 204)] [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("unknown-participant", 409)] [InlineData("foreign-incident", 404)] [InlineData("missing-manage", 403)]
    public async Task Actual_HSE_investigator_HTTP_command_preserves_recorded_participant_identity_and_enforces_authority(string fault, int expectedStatus)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        var request = await CompanyIncidentCommandTests.Request(f);
        if (fault == "unknown-participant") request = request with { BusinessAssociateId = "foreign-person" };
        if (fault == "missing-manage") { f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == "HSE.ManageIncidents").ToArrayAsync()); await f.Db.SaveChangesAsync(); await f.Select("company"); }
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var ca = new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy, ca, CompanyIncidentCommandTests.Commands(f));
        using var boundary = new Boundary(f, owner, fault != "denied-field"); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(facade);
        await using var host = await HttpHost.Start(f, owner, boundary, fault != "anonymous", typeof(HSEController));
        var url = host.Url + "/api/field/current/hse/incident/investigator?incidentId=" + (fault == "foreign-incident" ? "foreign" : "local");
        using var response = await host.Http.PutAsJsonAsync(url, request); Assert.Equal(expectedStatus, (int)response.StatusCode);
        if (fault == "allowed") {
            var record = Assert.Single(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.INVOLVED_BA_ROLE == "INVESTIGATOR" && row.ACTIVE_IND == "Y"); Assert.Equal(17, record.PARTY_OBS_NO); Assert.Equal("actor", record.ROW_CREATED_BY);
            using var stale = await host.Http.PutAsJsonAsync(url, request); Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode); Assert.Single(f.Writes);
            var review = await host.Http.GetFromJsonAsync<InvestigatorAssignmentReview>(host.Url + "/api/field/current/hse/incident/investigator-review?incidentId=local");
            Assert.NotNull(review!.CurrentInvestigator); request = request with { ExpectedRevision = review.Revision };
            using var retry = await host.Http.PutAsJsonAsync(url, request); Assert.Equal(HttpStatusCode.NoContent, retry.StatusCode); Assert.Single(f.Writes);
        } else Assert.Empty(f.Writes);
        Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData("allowed", 200)] [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("foreign-incident", 404)] [InlineData("missing-manage", 403)]
    public async Task Actual_HSE_investigator_review_HTTP_reads_complete_role_keys_without_writes(string fault, int expectedStatus)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        if (fault == "missing-manage") { f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == "HSE.ManageIncidents").ToArrayAsync()); await f.Db.SaveChangesAsync(); await f.Select("company"); }
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var ca = new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy, ca, CompanyIncidentCommandTests.Commands(f));
        using var boundary = new Boundary(f, owner, fault != "denied-field"); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(facade);
        await using var host = await HttpHost.Start(f, owner, boundary, fault != "anonymous", typeof(HSEController));
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/hse/incident/investigator-review?incidentId=" + (fault == "foreign-incident" ? "foreign" : "local"));
        Assert.Equal(expectedStatus, (int)response.StatusCode);
        if (fault == "allowed") {
            var review = await response.Content.ReadFromJsonAsync<InvestigatorAssignmentReview>();
            Assert.Equal("local", review!.IncidentId); Assert.Equal("actor", review.Context.ActorUserId);
            Assert.Contains(review.Participants, row => row.PartyObservation == 17 && row.RoleObservation == 4 && row.BusinessAssociateId == "local-person");
        }
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }

    [Theory] [InlineData("legacy-query")] [InlineData("null-body")] [InlineData("incomplete-body")]
    public async Task Actual_HSE_investigator_HTTP_requires_a_complete_reviewed_body(string fault)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var ca = new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy, ca, CompanyIncidentCommandTests.Commands(f));
        using var boundary = new Boundary(f, owner); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(facade);
        await using var host = await HttpHost.Start(f, owner, boundary, true, typeof(HSEController));
        var url = host.Url + "/api/field/current/hse/incident/investigator?incidentId=local";
        using var content = fault == "legacy-query" ? null : new StringContent(fault == "null-body" ? "null" : "{\"expectedRevision\":\"" + new string('A',64) + "\"}", Encoding.UTF8, "application/json");
        using var response = await host.Http.PutAsync(url + (fault == "legacy-query" ? "?baId=local-person" : ""), content);
        Assert.Equal(fault == "legacy-query" ? HttpStatusCode.UnsupportedMediaType : HttpStatusCode.BadRequest, response.StatusCode); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }

    private sealed class OnlyController(Type[] controllerTypes) : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        { foreach (var controller in feature.Controllers.Where(x => !controllerTypes.Contains(x.AsType())).ToArray()) feature.Controllers.Remove(controller); }
    }
    private sealed class HttpHost(WebApplication app, string url) : IAsyncDisposable
    {
        public string Url => url; public HttpClient Http { get; } = new();
        public IServiceProvider Services => app.Services;
        public static async Task<HttpHost> Start(Fixture f, ICompanyFieldSelectionService owner, Boundary boundary, bool authenticated, Type? controllerType = null, Action<IServiceCollection>? configure = null,Type[]? additionalControllers=null, IAccessControlService? accessOverride=null, bool companyMiddleware = false, Action<WebApplication>? beforeEndpoints = null)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
            builder.WebHost.UseUrls("http://127.0.0.1:0"); builder.Logging.ClearProviders();
            builder.Services.AddAuthentication("fixture").AddScheme<AuthenticationSchemeOptions, FixtureAuthentication>("fixture", _ => { }); builder.Services.AddAuthorization();
            builder.Services.AddSingleton(new FixturePrincipal(authenticated)); builder.Services.AddSingleton(f.Account); builder.Services.AddSingleton(f.Gate);
            builder.Services.AddSingleton(owner); builder.Services.AddSingleton(accessOverride ?? boundary.Access.Object); builder.Services.AddSingleton(boundary.Orchestrator.Object); builder.Services.AddSingleton(boundary.Observations.Object);
            configure?.Invoke(builder.Services);
            builder.Services.AddExceptionHandler<RefusalExceptionHandler>(); builder.Services.AddProblemDetails();
            builder.Services.AddControllers().AddApplicationPart(typeof(FieldOrchestratorController).Assembly).ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(new OnlyController(new[]{controllerType ?? typeof(FieldOrchestratorController)}.Concat(additionalControllers??[]).ToArray())));
            var app = builder.Build(); app.Use(async (context, next) => { f.Http.HttpContext = context; await next(); });
            app.UseExceptionHandler(); app.UseAuthentication();
            if (companyMiddleware) app.UseMiddleware<CompanyRequestMiddleware>((Func<ClaimsPrincipal, string?>)(principal => principal.FindFirst("party_id")?.Value));
            app.UseAuthorization(); beforeEndpoints?.Invoke(app); app.MapControllers(); await app.StartAsync();
            return new(app, app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
        }
        public async ValueTask DisposeAsync() { Http.Dispose(); await app.StopAsync(); await app.DisposeAsync(); }
    }
    public sealed record FixturePrincipal(bool Authenticated);
    public sealed class FixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, FixturePrincipal principal)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(!principal.Authenticated ? AuthenticateResult.NoResult() :
            AuthenticateResult.Success(new(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "actor")], Scheme.Name)), Scheme.Name)));
    }
}
