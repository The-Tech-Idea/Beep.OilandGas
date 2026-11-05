using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanyHseWorkflowClientTests
{
    private sealed class RcaRecoveryTransport(CompanyHseRcaRecoveredHandoff recovered, string mode, Action change) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("/api/field/current/hse/workflow/rca/handoff-recovery", request.RequestUri!.AbsolutePath);
            Assert.Contains("%2F", request.RequestUri.Query);
            if (mode == "late" || mode == "late-missing") change();
            if (mode is "missing" or "late-missing") return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            var value = mode == "foreign" ? recovered with { Receipt = recovered.Receipt with { Actor = "other" } } : recovered;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(value) });
        }
    }
    [Theory]
    [InlineData("valid")][InlineData("missing")][InlineData("foreign")][InlineData("late")][InlineData("late-missing")]
    public async Task RcaRecoveryClientChecksOriginalReceiptAndCurrentCompany(string mode)
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var cause = new IncidentRcaRecordedCause(context.Company, context.Database, context.FieldId!, "incident/الكويت", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('A', 64));
        var command = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), new(context, new("lifecycle", "binding"), cause, "instance", "step", "HSEOfficer", new string('B', 64)));
        var receipt = new CompanyHseRcaHandoffReceipt(context.Company, "actor", command.IntentId, "instance", "step", "history", "next", ["closed"], ["next-task"], new string('C', 64));
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["HSEOfficer"], ["HSE.ManageIncidents"])));
        using var transport = new RcaRecoveryTransport(new(command, receipt), mode, () => company.Clear()); using var http = new HttpClient(transport) { BaseAddress = new("https://api.test/") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        CompanyHseRcaRecoveredHandoff? actual = null;
        var error = await Record.ExceptionAsync(async () => actual = await client.RecoverHseRcaHandoffAsync(cause.IncidentId));
        if (mode == "valid") { Assert.Null(error); Assert.NotNull(actual); }
        else if (mode == "missing") { Assert.Null(error); Assert.Null(actual); }
        else Assert.NotNull(error);
    }
    private sealed class RcaHandoffTransport(CompanyHseRcaHandoffReview review, string mode, Action change) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Method == HttpMethod.Post)
            {
                Assert.Equal("/api/field/current/hse/workflow/rca/handoff", request.RequestUri!.AbsolutePath);
                var original = (await request.Content!.ReadFromJsonAsync<CompanyHseRcaHandoffCommand>(cancellationToken: token))!;
                if (mode == "late") change();
                var receipt = new CompanyHseRcaHandoffReceipt(review.Context.Company, mode == "foreign" ? "other" : review.Context.ActorUserId,
                    original.IntentId, review.InstanceId, review.StepInstanceId, "history", "next", ["closed"], ["next-task"], new string('C', 64));
                return new(HttpStatusCode.OK) { Content = JsonContent.Create(receipt) };
            }
            Assert.Equal("/api/field/current/hse/workflow/rca/handoff-review", request.RequestUri!.AbsolutePath);
            Assert.Contains("%2F", request.RequestUri.Query); Assert.Contains(review.Cause.IntentId.ToString("D"), request.RequestUri.Query);
            if (mode == "late") change();
            var value = mode == "foreign" ? review with { Cause = review.Cause with { Summary = "A different recorded cause observation." } } : review;
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };
        }
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign")]
    [InlineData("late")]
    public async Task RcaHandoffClientRejectsChangedReceiptOrCompany(string mode)
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var cause = new IncidentRcaRecordedCause(context.Company, context.Database, context.FieldId!, "incident/الكويت", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('A', 64));
        var review = new CompanyHseRcaHandoffReview(context, new("lifecycle", "binding"), cause, "instance", "step", "HSEOfficer", new string('B', 64));
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["HSEOfficer"], ["HSE.ManageIncidents"])));
        using var transport = new RcaHandoffTransport(review, mode, () => company.Clear()); using var http = new HttpClient(transport) { BaseAddress = new("https://api.test/") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        var error = await Record.ExceptionAsync(() => client.CompleteHseRcaHandoffAsync(new(Guid.NewGuid(), review)));
        if (mode == "valid") Assert.Null(error); else Assert.NotNull(error);
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign")]
    [InlineData("late")]
    public async Task RcaHandoffClientRejectsChangedCauseOrCompany(string mode)
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var original = new IncidentRcaRecordedCause(context.Company, context.Database, context.FieldId!, "incident/الكويت", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('A', 64));
        var review = new CompanyHseRcaHandoffReview(context, new("lifecycle", "binding"), original, "instance", "step", "HSEOfficer", new string('B', 64));
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["HSEOfficer"], ["HSE.ManageIncidents"])));
        using var transport = new RcaHandoffTransport(review, mode, () => company.Clear()); using var http = new HttpClient(transport) { BaseAddress = new("https://api.test/") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        var error = await Record.ExceptionAsync(() => client.ReviewHseRcaHandoffAsync(original));
        if (mode == "valid") Assert.Null(error); else Assert.NotNull(error);
    }

    private sealed class SharedRcaTransport(IncidentRcaRecordedCause cause, string mode, Action change) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("/api/field/current/hse/workflow/rca/shared", request.RequestUri!.AbsolutePath);
            if (mode == "late") change();
            var item = mode == "foreign" ? cause with { Company = new("tenant", "other") } : cause;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(mode == "duplicate" ? new[] { item, item } : new[] { item }) });
        }
    }
    [Theory]
    [InlineData("valid")][InlineData("foreign")][InlineData("duplicate")][InlineData("late")]
    public async Task SharedCauseClientChecksCompanyAndRejectsStaleOrDuplicateEvidence(string mode)
    {
        var scope = new CompanyScope("tenant", "company");
        var cause = new IncidentRcaRecordedCause(scope, new("core", "binding"), "field", "incident", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('A', 64));
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(scope, ["HSEOfficer"], ["HSE.ManageIncidents"])));
        using var transport = new SharedRcaTransport(cause, mode, () => company.Clear()); using var http = new HttpClient(transport) { BaseAddress = new("https://api.test/") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        var error = await Record.ExceptionAsync(() => client.ReadSharedHseCausesAsync("incident"));
        if (mode == "valid") Assert.Null(error); else Assert.NotNull(error);
    }

    private sealed class RecordedRcaTransport(IncidentRcaIntent original, string mode, Action change) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("/api/field/current/hse/workflow/rca/recorded", request.RequestUri!.AbsolutePath);
            Assert.Contains("%2F", request.RequestUri.Query);
            if (mode == "late") change();
            var item = mode == "foreign" ? original with { Source = original.Source with { IncidentId = "other" } } : original;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(mode == "duplicate" ? new[] { item, item } : new[] { item }) });
        }
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign")]
    [InlineData("duplicate")]
    [InlineData("late")]
    public async Task RecordedCauseRecoveryRejectsForeignDuplicateOrStaleResults(string mode)
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var original = new IncidentRcaIntent(Guid.NewGuid(), new(context, "incident/الكويت", new string('A', 64), 1, [], [option]), option, "Recorded cause for this incident.");
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["HSEOfficer"], ["HSE.ManageIncidents"])));
        using var transport = new RecordedRcaTransport(original, mode, () => company.Clear());
        using var http = new HttpClient(transport) { BaseAddress = new("https://api.test/") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        var error = await Record.ExceptionAsync(() => client.ReadRecordedHseCausesAsync(original.Source.IncidentId));
        if (mode == "valid") Assert.Null(error); else Assert.NotNull(error);
    }
    private sealed class RcaTransport(IncidentRcaSourceReview review, string mode, Action change) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Method == HttpMethod.Post)
            {
                Assert.Equal("/api/field/current/hse/workflow/rca/record-cause", request.RequestUri!.AbsolutePath);
                var original = (await request.Content!.ReadFromJsonAsync<IncidentRcaIntent>(cancellationToken: token))!;
                if (mode == "late") change();
                var receipt = new IncidentRcaReceipt(original.Source.Context.Company, mode == "foreign" ? "other" : original.Source.Context.ActorUserId,
                    original.Source.Context.FieldId!, original.IntentId, original.Source.IncidentId, original.Source.NextObservation, HSE_INCIDENT_RCA_INTENT.Create(original).COMMAND_HASH);
                return new(HttpStatusCode.OK) { Content = JsonContent.Create(receipt) };
            }
            Assert.Equal("/api/field/current/hse/workflow/rca/review", request.RequestUri!.AbsolutePath);
            Assert.Contains("%2F", request.RequestUri.Query);
            if (mode == "late") change();
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(mode == "foreign" ? review with { IncidentId = "other" } : review) };
        }
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign")]
    [InlineData("late")]
    public async Task RcaRecordRejectsForeignOrStaleReceipts(string mode)
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var review = new IncidentRcaSourceReview(context, "incident/الكويت", new string('A', 64), 1, [], [option]);
        var original = new IncidentRcaIntent(Guid.NewGuid(), review, option, "Reviewed cause for this incident.");
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["HSEOfficer"], ["HSE.ManageIncidents"])));
        using var transport = new RcaTransport(review, mode, () => company.Clear());
        using var http = new HttpClient(transport) { BaseAddress = new("https://api.test/") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        var error = await Record.ExceptionAsync(() => client.RecordHseIncidentCauseAsync(original));
        if (mode == "valid") Assert.Null(error); else Assert.NotNull(error);
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign")]
    [InlineData("late")]
    public async Task RcaReviewRejectsForeignOrStaleResults(string mode)
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var review = new IncidentRcaSourceReview(context, "incident/الكويت", new string('A', 64), 1, [], [new("ROOT", "PROCESS", "Process")]);
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["HSEOfficer"], ["HSE.ManageIncidents"])));
        using var transport = new RcaTransport(review, mode, () => company.Clear());
        using var http = new HttpClient(transport) { BaseAddress = new("https://api.test/") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        var error = await Record.ExceptionAsync(() => client.ReviewHseIncidentRcaAsync(review.IncidentId));
        if (mode == "valid") Assert.Null(error); else Assert.NotNull(error);
    }

    private sealed class Transport(CompanyHseWorkflowReview review, string mode, Action change) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Method == HttpMethod.Get)
            {
                Assert.Equal("/api/field/current/hse/workflow/review", request.RequestUri!.AbsolutePath);
                Assert.Contains("%2F", request.RequestUri.Query);
                if (mode == "late-review") change();
                return new(HttpStatusCode.OK) { Content = JsonContent.Create(mode == "foreign-review" ? review with { IncidentId = "other" } : review) };
            }
            Assert.Equal("/api/field/current/hse/workflow/start", request.RequestUri!.AbsolutePath);
            var original = (await request.Content!.ReadFromJsonAsync<CompanyHseWorkflowStart>(cancellationToken: token))!;
            if (mode == "late-start") change();
            var receipt = new CompanyHseWorkflowReceipt(original.Context.Company, mode == "foreign-receipt" ? "other" : original.Context.ActorUserId,
                original.Context.FieldId!, original.IntentId, original.IncidentId, original.InstanceId(), "step", ["task"], new string('C', 64));
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(receipt) };
        }
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("foreign-review")]
    [InlineData("late-review")]
    [InlineData("foreign-receipt")]
    [InlineData("late-start")]
    public async Task ReviewAndStartRejectForeignOrStaleResponses(string mode)
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var review = new CompanyHseWorkflowReview(context, new("lifecycle", "binding"), "incident/الكويت", new string('A', 64), new string('B', 64), "Incident reporting", "HSEOfficer");
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(context.Company, ["HSEOfficer"], ["HSE.ManageIncidents"])));
        using var transport = new Transport(review, mode, () => company.Clear());
        using var http = new HttpClient(transport) { BaseAddress = new("https://api.test/") };
        var client = new BusinessProcessServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company), NullLogger<BusinessProcessServiceClient>.Instance);
        var error = await Record.ExceptionAsync(async () =>
        {
            var result = await client.ReviewHseWorkflowAsync(review.IncidentId);
            var original = result.CreateIntent(Guid.NewGuid());
            (await client.StartHseWorkflowAsync(original)).Require(original);
        });
        if (mode == "valid") Assert.Null(error); else Assert.NotNull(error);
    }
}
#endif
