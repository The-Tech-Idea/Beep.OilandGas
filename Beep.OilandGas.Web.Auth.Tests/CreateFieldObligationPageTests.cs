using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.PPDM39.BusinessProcess;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CreateFieldObligationPageTests
{
    private static readonly BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
    private sealed class Store : IFieldObligationIntentStore
    {
        public CreateFieldObligation? Pending; public bool Fail;
        public Task<CreateFieldObligation?> ReadAsync(FieldSelectionReview context) => Task.FromResult(Pending);
        public Task RetainAsync(CreateFieldObligation request) { if (Fail || Pending is not null && Pending != request) throw new InvalidOperationException(); Pending = request; return Task.CompletedTask; }
        public Task ClearAsync(CreateFieldObligation request) { if (Pending != request) throw new InvalidOperationException(); Pending = null; return Task.CompletedTask; }
    }
    private sealed class Handler(FieldSelectionReview review) : HttpMessageHandler
    {
        public bool LoseResponse; public List<CreateFieldObligation> Requests = [];
        public TaskCompletionSource? Started; public TaskCompletionSource? Release;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.Method == HttpMethod.Get) result = new FieldObligationCreationOptions(review, ["REPORT"], ["FIELD"]);
            else
            {
                var command = JsonSerializer.Deserialize<CreateFieldObligation>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                Requests.Add(command);
                if (Started is not null && Release is not null) { Started.TrySetResult(); await Release.Task; }
                if (LoseResponse) { LoseResponse = false; throw new HttpRequestException("Response lost"); }
                result = new CreatedFieldObligation(review, "FO-" + command.IntentId.ToString("N").ToUpperInvariant(), 1, 1);
            }
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }
    [Fact]
    public async Task ProtectedIntentSurvivesPageOwnerAndCannotBeReplacedOrReadByAnotherCompany()
    {
        var tab = new DevelopmentDraftIntentStoreTests.TabStorage(); var protection = new EphemeralDataProtectionProvider();
        IFieldObligationIntentStore Store() => new FieldObligationIntentStore(new ProtectedSessionStorage(tab, protection));
        var review = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var original = new CreateFieldObligation(review, Guid.NewGuid(), "REPORT", "FIELD", "Private obligation details", null);
        await Store().RetainAsync(original); Assert.Equal(original, await Store().ReadAsync(review));
        Assert.Equal(original, await Store().ReadAsync(review with { Revision = "refreshed" }));
        Assert.DoesNotContain("Private obligation details", string.Join(" ", tab.Values.Values));
        Assert.Null(await Store().ReadAsync(review with { Company = new("tenant", "other") }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainAsync(original with { IntentId = Guid.NewGuid() }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().ClearAsync(original with { Description = "Different" }));
        await Store().ClearAsync(original); Assert.Null(await Store().ReadAsync(review));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SubmitRetainsOriginalAndRetriesAnUnconfirmedOutcome(bool loseResponse)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Compliance"], [])));
        var context = new FieldSelectionReview(company.Selection!.Scope, new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Beep.OilandGas.Models.Data.FieldResponse { FieldId = "field", FieldName = "Field A", SelectionAvailable = true, SelectionContext = context });
        var store = new Store(); using var handler = new Handler(context) { LoseResponse = loseResponse };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = new TestContext();
        ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton<IFieldObligationIntentStore>(store);
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CreateFieldObligationPage>(); page.WaitForAssertion(() => Assert.Contains("Record obligation", page.Markup));
        await page.InvokeAsync(() => { typeof(CreateFieldObligationPage).GetField("_type", Flags)!.SetValue(page.Instance, "REPORT");
            typeof(CreateFieldObligationPage).GetField("_component", Flags)!.SetValue(page.Instance, "FIELD");
            typeof(CreateFieldObligationPage).GetField("_description", Flags)!.SetValue(page.Instance, "Recorded fact"); });
        async Task Submit() => await page.InvokeAsync(() => (Task)typeof(CreateFieldObligationPage).GetMethod("Submit", Flags)!.Invoke(page.Instance, null)!);
        await Submit(); await page.InvokeAsync(() => page.Render());
        if (loseResponse)
        {
            Assert.NotNull(store.Pending); Assert.Contains("outcome is unconfirmed", page.Markup); await Submit(); await page.InvokeAsync(() => page.Render());
            Assert.Equal(2, handler.Requests.Count); Assert.Equal(handler.Requests[0], handler.Requests[1]);
        }
        else Assert.Single(handler.Requests);
        Assert.Null(store.Pending); Assert.Contains("were recorded", page.Markup);
    }

    [Fact]
    public async Task ReopenedPageRecoversAndResendsTheOriginalIntent()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var context = new FieldSelectionReview(company.Selection!.Scope, new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Beep.OilandGas.Models.Data.FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var original = new CreateFieldObligation(context, Guid.NewGuid(), "REPORT", "FIELD", "Retained record", null); var store = new Store { Pending = original };
        using var handler = new Handler(context); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = new TestContext();
        ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton<IFieldObligationIntentStore>(store); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CreateFieldObligationPage>(); page.WaitForAssertion(() => Assert.Contains("Retry original submission", page.Markup));
        await page.InvokeAsync(() => (Task)typeof(CreateFieldObligationPage).GetMethod("Submit", Flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(original, Assert.Single(handler.Requests)); Assert.Null(store.Pending);
    }

    [Fact]
    public async Task ReopenedPageCanRetryOriginalIntentAfterTheSameFieldReviewRefreshes()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var originalContext = new FieldSelectionReview(company.Selection!.Scope, new("core", "binding"), "actor", "old-revision", "field");
        var current = originalContext with { Revision = "new-revision" };
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Beep.OilandGas.Models.Data.FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = current });
        var original = new CreateFieldObligation(originalContext, Guid.NewGuid(), "REPORT", "FIELD", "Retained record", null); var store = new Store { Pending = original };
        using var handler = new Handler(current); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = new TestContext();
        ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton<IFieldObligationIntentStore>(store); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CreateFieldObligationPage>(); page.WaitForAssertion(() => Assert.Contains("FIELD review was refreshed", page.Markup));
        await page.InvokeAsync(() => (Task)typeof(CreateFieldObligationPage).GetMethod("Submit", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render()); Assert.Equal(original, Assert.Single(handler.Requests)); Assert.Null(store.Pending); Assert.Contains("were recorded", page.Markup);
    }

    [Fact]
    public async Task CompanyChangeKeepsTheOriginalPendingRequestAndDiscardsTheLateResponse()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var context = new FieldSelectionReview(company.Selection!.Scope, new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Beep.OilandGas.Models.Data.FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var store = new Store(); using var handler = new Handler(context) { Started = new(TaskCreationOptions.RunContinuationsAsynchronously), Release = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = new TestContext();
        ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton<IFieldObligationIntentStore>(store); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CreateFieldObligationPage>(); page.WaitForAssertion(() => Assert.Contains("Record obligation", page.Markup));
        await page.InvokeAsync(() => { typeof(CreateFieldObligationPage).GetField("_type", Flags)!.SetValue(page.Instance, "REPORT"); typeof(CreateFieldObligationPage).GetField("_component", Flags)!.SetValue(page.Instance, "FIELD"); typeof(CreateFieldObligationPage).GetField("_description", Flags)!.SetValue(page.Instance, "Recorded fact"); });
        var submit = page.InvokeAsync(() => (Task)typeof(CreateFieldObligationPage).GetMethod("Submit", Flags)!.Invoke(page.Instance, null)!);
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5)); await page.InvokeAsync(company.Clear); handler.Release.SetResult(); await submit;
        await page.InvokeAsync(() => page.Render()); Assert.NotNull(store.Pending); Assert.DoesNotContain("were recorded", page.Markup);
    }

    [Fact]
    public async Task FailedRetentionDoesNotSendTheRequest()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var context = new FieldSelectionReview(company.Selection!.Scope, new("core", "binding"), "actor", "revision", "field");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Beep.OilandGas.Models.Data.FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        using var handler = new Handler(context); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = new TestContext();
        ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton<IFieldObligationIntentStore>(new Store { Fail = true }); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CreateFieldObligationPage>(); page.WaitForAssertion(() => Assert.Contains("Record obligation", page.Markup));
        await page.InvokeAsync(() => { typeof(CreateFieldObligationPage).GetField("_type", Flags)!.SetValue(page.Instance, "REPORT"); typeof(CreateFieldObligationPage).GetField("_component", Flags)!.SetValue(page.Instance, "FIELD"); typeof(CreateFieldObligationPage).GetField("_description", Flags)!.SetValue(page.Instance, "Recorded fact"); });
        await page.InvokeAsync(() => (Task)typeof(CreateFieldObligationPage).GetMethod("Submit", Flags)!.Invoke(page.Instance, null)!);
        Assert.Empty(handler.Requests);
    }
}
#endif
