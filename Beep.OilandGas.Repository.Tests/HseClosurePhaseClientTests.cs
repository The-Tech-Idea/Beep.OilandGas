using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class HseClosurePhaseClientTests
{
    [Theory]
    [InlineData("valid")][InlineData("nonterminal")][InlineData("approval-mismatch")]
    [InlineData("native-lost")][InlineData("native-receipt-mismatch")][InlineData("company-change")][InlineData("approval-lost")]
    public async Task TypedClosureClientConfirmsApprovalBeforeNativeAndNeverRetriesUncertainty(string mode)
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("database", new string('F', 64)), "actor", new string('A', 64), "A");
        var action = new CAVerificationAction(1, "action", "Valve tested", "COMPLETED", "operator", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "operator", "Valve inspected and tested");
        var actions = new CAVerificationReview(context, "local", "plan", new string('C', 64), [action]);
        var decision = new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent test confirmed the repair is effective.");
        var original = new CompanyHseClosureCommand(Guid.NewGuid(), new(new(actions, [decision]), new("lifecycle", new string('B', 64)), "instance", "step", mode == "nonterminal" ? "INC_VERIFICATION" : "INC_CLOSURE", "Administrator", new string('A', 64), new string('B', 64)), "Final independent incident closure approved.");
        var receipt = new CompanyHseClosureReceipt(original.IntentId, context.Company, "actor", "instance", "step", original.Review.Stage, "history", "approval", mode == "nonterminal" ? "next-step" : null, ["closed-task"], mode == "nonterminal" ? ["next-task"] : [], new string('D', 64));
        original.Require(); receipt.Require(original);
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Version, "actor", new(context.Company, ["Administrator"], ["Workflow.Approve"])));
        var calls = new List<string>();
        using var http = new HttpClient(new Handler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method); calls.Add(request.RequestUri!.AbsolutePath);
            if (calls.Count == 1)
            {
                Assert.EndsWith("/closure/complete-stage", request.RequestUri.AbsolutePath);
                var sent = await request.Content!.ReadFromJsonAsync<CompanyHseClosureCommand>();
                Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(sent));
                if (mode == "approval-lost") throw new HttpRequestException("Controlled lifecycle approval acknowledgement loss.");
                if (mode == "company-change") company.Clear();
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(mode == "approval-mismatch" ? receipt with { IntentId = Guid.NewGuid() } : receipt) };
            }
            Assert.Equal(2, calls.Count); Assert.EndsWith("/closure/record-native", request.RequestUri.AbsolutePath);
            var approved = (await request.Content!.ReadFromJsonAsync<CompanyHseClosureRecovery>())!; approved.Require();
            Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(approved.Original));
            Assert.Equal(JsonSerializer.Serialize(receipt), JsonSerializer.Serialize(approved.Receipt));
            if (mode == "native-lost") throw new HttpRequestException("Controlled native closure acknowledgement loss.");
            var returned = mode == "native-receipt-mismatch" ? receipt with { GraphRevision = new string('E', 64) } : receipt;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new CompanyHseClosureOutcome(original, returned, new(original, returned, 4))) };
        })) { BaseAddress = new Uri("https://controlled.example") };
        var client = new HSEServiceClient(new ApiClient(http, Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(), company), NullLogger<HSEServiceClient>.Instance, Mock.Of<Beep.OilandGas.Client.App.Services.Field.IFieldService>());
        if (mode is "valid" or "nonterminal")
        {
            var outcome = await client.CompleteClosureStageAsync(original); outcome.Require();
            Assert.Equal(mode == "valid", outcome.Native is not null);
        }
        else if (mode is "native-lost" or "approval-lost") await Assert.ThrowsAsync<HttpRequestException>(() => client.CompleteClosureStageAsync(original));
        else if (mode == "company-change") await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.CompleteClosureStageAsync(original));
        else await Assert.ThrowsAsync<InvalidOperationException>(() => client.CompleteClosureStageAsync(original));
        Assert.Equal(mode is "nonterminal" or "approval-mismatch" or "company-change" or "approval-lost" ? 1 : 2, calls.Count);
    }
    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }
}
