using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DrillingReportingReassignmentStateTests
{
    private static readonly Lazy<Task<DrillingReportingReassignment>> Baseline=new(async()=>
    {
        var f=new DrillingReportingReassignmentTests.Fixture();await f.Prepare();return new(await f.Review(),Guid.NewGuid(),DateTimeOffset.UtcNow,"Transfer reporting to the configured replacement team",true);
    });
    private static DrillingReportingReassignmentCatalog Catalog(DrillingReportingReassignment original)
    {
        var review=original.Review;var value=new DrillingReportingReassignmentCatalog(review.Responsibility,review.Execution,[review.Destination],[review.ManagerRole],review.ManagerPersona,review.AuthorityRevision,review.NativeRevision,"",DateTimeOffset.UtcNow);
        return value with{Revision=value.ContentRevision()};
    }
    private sealed class Flow : IDisposable
    {
        internal CompanyClientContext Company=new();internal Mock<IPersonaContextService> Personas=new();internal Mock<IDrillingServiceClient> Client=new();
        internal DevelopmentDraftWorkspaceTests.Journal Journal=new();internal DrillingReportingReassignmentState State=null!;internal DrillingReportingReassignment Original=null!;
        internal async Task Prepare()
        {
            Original=CompanyInventorySourceReview.Copy(await Baseline.Value);var scope=Original.Scope();Company.TrySelect(Company.Clear(),scope.ActorUserId,new(scope.Company,[Original.Review.ManagerRole],["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View","Drilling.Edit","Drilling.ManageWellProgram","Workflow.Start"]));
            Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=Original.Review.ManagerPersona});Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=Original.Review.ManagerPersona});Personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            Client.Setup(c=>c.GetReportingReassignmentCatalogAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(Catalog(Original));
            Client.Setup(c=>c.ReviewReportingReassignmentAsync(It.IsAny<DrillingReportingReassignmentSelection>(),It.IsAny<CancellationToken>())).ReturnsAsync(Original.Review);
            Client.Setup(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingReportingReassignment original,CancellationToken _)=>Receipt(original));
            Client.Setup(c=>c.RecoverReportingReassignmentAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingReportingReassignment original,CancellationToken _)=>Receipt(original));
            State=NewState();await State.LoadAsync(Original.Review.Responsibility.Root.InstanceId());State.Select(Original.Review.Destination.ProcessId,Original.Review.ManagerRole);await State.ReviewAsync();Assert.True(State.CanPrepare);
        }
        internal DrillingReportingReassignmentState NewState()=>new(Company,Personas.Object,Client.Object,Journal,(_,_,fallback)=>fallback);
        internal void Freeze()=>State.Prepare(DateTimeOffset.UtcNow.ToString("O"),"Transfer ongoing reporting to the reviewed team",true);
        internal static DrillingReportingReassignmentReceipt Receipt(DrillingReportingReassignment original)=>new(original.Revision(),original.Review.Responsibility.Root.InstanceId(),true,original.HistoryId(),original.ReassignedAtUtc,original.Review.Responsibility.Number+1,original.StepInstanceId(),original.TaskIds(),new('A',64),DateTimeOffset.UtcNow);
        public void Dispose(){State.Dispose();Company.Dispose();}
    }
    [Theory,InlineData("reason"),InlineData("confirmation"),InlineData("offset"),InlineData("future"),InlineData("before-review")]
    public async Task TransferRequiresActualExplicitUtcReasonAndAcknowledgedManagement(string fault)
    {
        using var f=new Flow();await f.Prepare();var utc=fault switch{"offset"=>DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).ToString("O"),"future"=>DateTimeOffset.UtcNow.AddDays(1).ToString("O"),"before-review"=>f.Original.Review.CheckedAtUtc.AddMinutes(-1).ToString("O"),_=>DateTimeOffset.UtcNow.ToString("O")};
        f.State.Prepare(utc,fault=="reason"?"":"Accountable transfer",fault!="confirmation");Assert.Null(f.State.Prepared);Assert.NotNull(f.State.Error);
        f.Client.Verify(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]
    public async Task UnchangedCrewAndManagerCannotPrepareAnotherGenerationFromTheScreen()
    {
        using var f=new Flow();await f.Prepare();var catalog=Catalog(f.Original) with{Options=[f.Original.Review.Responsibility.Workflow]};catalog=catalog with{Revision=catalog.ContentRevision()};
        f.Client.Setup(c=>c.GetReportingReassignmentCatalogAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(catalog);await f.State.LoadAsync(f.Original.Review.Responsibility.Root.InstanceId());f.State.Select(catalog.Options[0].ProcessId,f.Original.Review.ManagerRole);
        f.Client.Invocations.Clear();Assert.False(f.State.CanReview);await f.State.ReviewAsync();f.Client.Verify(c=>c.ReviewReportingReassignmentAsync(It.IsAny<DrillingReportingReassignmentSelection>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Theory,InlineData(false),InlineData(true)]
    public async Task FailedOrUnconfirmedRetentionNeverSendsATransfer(bool readback)
    {
        using var f=new Flow();await f.Prepare();f.Freeze();f.Journal.FailRetain=!readback;f.Journal.FailRetainReadback=readback;await f.State.SubmitAsync();
        Assert.NotNull(f.State.Pending);Assert.Contains("No submission",f.State.PendingExplanation);f.Client.Verify(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]
    public async Task LostAcknowledgementRecoversRetainedOriginalWithReadOnlyRightsBeforeCatalogOrProfileLoading()
    {
        using var f=new Flow();await f.Prepare();f.Freeze();f.Client.Setup(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>())).Returns((DrillingReportingReassignment original,CancellationToken _)=>{Assert.Equal(original.Revision(),f.Journal.ReportingReassignmentOriginal!.Revision());throw new HttpRequestException("Lost acknowledgement");});
        await f.State.SubmitAsync();f.State.Dispose();var scope=f.Original.Scope();f.Company.TrySelect(f.Company.Clear(),scope.ActorUserId,new(scope.Company,[f.Original.Review.ManagerRole],["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        f.Client.Invocations.Clear();f.Personas.Invocations.Clear();f.State=f.NewState();await f.State.LoadAsync(f.Original.Review.Responsibility.Root.InstanceId());Assert.True(f.State.CanRecover);Assert.Null(f.State.Catalog);
        await f.State.RecoverAsync();Assert.True(f.State.Receipt!.Recorded);Assert.Null(f.Journal.ReportingReassignmentOriginal);f.Client.Verify(c=>c.GetReportingReassignmentCatalogAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());f.Personas.Verify(p=>p.EnsureLoadedAsync(),Times.Never());
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LateTransferReceiptCannotPublishOrClearAnotherContext(string change)
    {
        using var f=new Flow();await f.Prepare();f.Freeze();var arrived=new TaskCompletionSource<DrillingReportingReassignment>(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<DrillingReportingReassignmentReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>())).Returns((DrillingReportingReassignment original,CancellationToken _)=>{arrived.SetResult(original);return release.Task;});var work=f.State.SubmitAsync();var sent=await arrived.Task;
        if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);if(change=="route")await f.State.LoadAsync("other");if(change=="dispose")f.State.Dispose();release.SetResult(Flow.Receipt(sent));await work;
        Assert.Null(f.State.Receipt);Assert.NotNull(f.Journal.ReportingReassignmentOriginal);
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LateCatalogCannotPublishIntoAnotherPrivateContext(string change)
    {
        using var f=new Flow();await f.Prepare();var arrived=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<DrillingReportingReassignmentCatalog>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c=>c.GetReportingReassignmentCatalogAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns((string _,CancellationToken __)=>{arrived.SetResult();return release.Task;});var work=f.State.LoadAsync(f.Original.Review.Responsibility.Root.InstanceId());await arrived.Task;
        if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);if(change=="route")await f.State.LoadAsync("other");if(change=="dispose")f.State.Dispose();release.SetResult(Catalog(f.Original));await work;Assert.Null(f.State.Catalog);Assert.Null(f.State.Review);
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LateReviewCannotPublishIntoAnotherPrivateContext(string change)
    {
        using var f=new Flow();await f.Prepare();var arrived=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource<DrillingReportingReassignmentReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c=>c.ReviewReportingReassignmentAsync(It.IsAny<DrillingReportingReassignmentSelection>(),It.IsAny<CancellationToken>())).Returns((DrillingReportingReassignmentSelection _,CancellationToken __)=>{arrived.SetResult();return release.Task;});var work=f.State.ReviewAsync();await arrived.Task;
        if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null!);if(change=="route")await f.State.LoadAsync("other");if(change=="dispose")f.State.Dispose();release.SetResult(f.Original.Review);await work;Assert.Null(f.State.Review);Assert.Null(f.State.Prepared);
    }
    [Fact]
    public async Task ChangedWorkspaceDuringProfileLoadingRefusesStaleManagementPreview()
    {
        using var f=new Flow();await f.Prepare();f.Personas.Setup(p=>p.EnsureLoadedAsync()).Callback(()=>
        {
            f.Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code="OTHER"});f.Personas.Raise(p=>p.Changed+=null!);
        }).Returns(Task.CompletedTask);
        await f.State.ReviewAsync();Assert.Null(f.State.Review);Assert.False(f.State.CanPrepare);Assert.NotNull(f.State.Error);
    }
    [Fact]
    public async Task ConfirmedTransferReceiptSurvivesCleanupFailureAndRecoveryClearsOnlyItsOriginal()
    {
        using var f=new Flow();await f.Prepare();f.Freeze();f.Journal.FailClear=true;await f.State.SubmitAsync();Assert.True(f.State.Receipt!.Recorded);Assert.NotNull(f.State.Pending);Assert.Contains("Clearing the tab input is not confirmed",f.State.PendingExplanation);
        f.Journal.FailClear=false;await f.State.RecoverAsync();Assert.Null(f.Journal.ReportingReassignmentOriginal);f.Client.Verify(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>()),Times.Once());
    }
    [Fact]
    public async Task ContextChangeDuringRetentionPreventsTransportAndPreservesOriginalScopeInput()
    {
        using var f=new Flow();await f.Prepare();f.Freeze();var arrived=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Journal.BeforeReviewRetain=async()=>{arrived.SetResult();await release.Task;};var work=f.State.SubmitAsync();await arrived.Task;f.Company.Clear();release.SetResult();await work;Assert.NotNull(f.Journal.ReportingReassignmentOriginal);Assert.Null(f.State.Pending);
        f.Client.Verify(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]
    public async Task ActualTransferPageUsesConfiguredSelectionsExplicitFieldsAndRetainedConfirmation()
    {
        using var f=new Flow();await f.Prepare();f.State.Dispose();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/reporting/transfer?assignmentId="+Uri.EscapeDataString(f.Original.Review.Responsibility.Root.InstanceId()));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingReportingTransfer>();page.WaitForAssertion(()=>Assert.Contains("Current reporting generation 1",page.Markup));
        foreach(var field in page.FindComponents<MudSelect<string>>())await page.InvokeAsync(()=>field.Instance.ValueChanged.InvokeAsync(field.Instance.Label=="Receiving reporting workflow"?f.Original.Review.Destination.ProcessId:f.Original.Review.ManagerRole));
        await page.FindAll("button").Single(button=>button.TextContent=="Check reporting transfer").ClickAsync(new MouseEventArgs());page.WaitForAssertion(()=>Assert.Contains("Reviewed receiving responsibility",page.Markup));
        await page.FindComponents<MudTextField<string>>().Single(field=>field.Instance.Label=="Actual reporting transfer UTC").Find("input").ChangeAsync(new ChangeEventArgs{Value=DateTimeOffset.UtcNow.ToString("O")});
        await page.FindComponents<MudTextField<string>>().Single(field=>field.Instance.Label=="Transfer reason").Find("textarea").ChangeAsync(new ChangeEventArgs{Value="Move ongoing reporting to the replacement crew"});await page.FindComponent<MudCheckBox<bool>>().Find("input").ChangeAsync(new ChangeEventArgs{Value=true});
        await page.FindAll("button").Single(button=>button.TextContent=="Review transfer confirmation").ClickAsync(new MouseEventArgs());page.WaitForAssertion(()=>Assert.Contains("Confirm reporting transfer",page.Markup));
        f.Client.Setup(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingReportingReassignment original,CancellationToken _)=>{Assert.Equal(original.Revision(),f.Journal.ReportingReassignmentOriginal!.Revision());return Flow.Receipt(original);});
        await page.FindAll("button").Single(button=>button.TextContent=="Confirm and retain reporting transfer").ClickAsync(new MouseEventArgs());page.WaitForAssertion(()=>Assert.Contains("receiving generation remains ongoing",page.Markup));Assert.Null(f.Journal.ReportingReassignmentOriginal);
    }
    [Theory,InlineData("current"),InlineData("company"),InlineData("actor"),InlineData("duplicate-workflow"),InlineData("manager-role"),InlineData("revision")]
    public async Task CatalogBindsCurrentCompanyAccountGenerationAndCompleteConfiguredOptions(string fault)
    {
        var original=CompanyInventorySourceReview.Copy(await Baseline.Value);var scope=original.Scope();var catalog=Catalog(original);
        var company=fault=="company"?new CompanyScope("tenant","foreign"):scope.Company;var actor=fault=="actor"?"foreign-account":scope.ActorUserId;
        if(fault=="duplicate-workflow")catalog=catalog with{Options=[catalog.Options[0],catalog.Options[0]]};
        if(fault=="manager-role")catalog=catalog with{ManagerRoles=[]};if(fault=="revision")catalog=catalog with{Revision=new('F',64)};
        if(fault=="current")catalog.Require(company,actor,original.Review.Responsibility.Root.InstanceId());else Assert.Throws<InvalidOperationException>(()=>catalog.Require(company,actor,original.Review.Responsibility.Root.InstanceId()));
    }
    [Fact]
    public async Task ActualProtectedStoreRetainsExactTransferSeparatelyAndRefusesReplacementOrForeignClear()
    {
        var original=CompanyInventorySourceReview.Copy(await Baseline.Value);var scope=original.Scope();var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();
        DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainReportingReassignmentAsync(original);Assert.Equal(original.Revision(),(await Store().ReadReportingReassignmentAsync(scope.Company,scope.ActorUserId))!.Revision());
        Assert.Null(await Store().ReadReportingReassignmentAsync(new("tenant","foreign"),scope.ActorUserId));Assert.Null(await Store().ReadReportingClosureAsync(scope.Company,scope.ActorUserId));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainReportingReassignmentAsync(original with{IntentId=Guid.NewGuid()}));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearReportingReassignmentAsync(original with{Reason="Different retained reason"}));
        await Store().ClearReportingReassignmentAsync(original);Assert.Empty(tab.Values);
    }
    private sealed class Transport(Func<HttpRequestMessage,Task<HttpResponseMessage>> send):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>send(request);}
    [Theory,InlineData("company"),InlineData("actor")]
    public async Task TypedClientRejectsAValidCatalogReturnedForAnotherCompanyOrAccount(string fault)
    {
        var original=CompanyInventorySourceReview.Copy(await Baseline.Value);using var company=new CompanyClientContext();company.TrySelect(company.Clear(),fault=="actor"?"foreign":original.Scope().ActorUserId,new(fault=="company"?new("tenant","foreign"):original.Scope().Company,[],[]));
        using var http=new HttpClient(new Transport(_=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(Catalog(original))}))){BaseAddress=new("http://localhost")};var client=new DrillingServiceClient(new(http,Mock.Of<IFailureReporter>(),company));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetReportingReassignmentCatalogAsync(original.Review.Responsibility.Root.InstanceId()));
    }
    [Fact]
    public async Task TypedClientUsesExactCatalogReviewTransferAndReadOnlyRecoveryRoutesAndOriginalPayload()
    {
        var original=CompanyInventorySourceReview.Copy(await Baseline.Value);var scope=original.Scope();var catalog=Catalog(original);using var company=new CompanyClientContext();company.TrySelect(company.Clear(),scope.ActorUserId,new(scope.Company,[original.Review.ManagerRole],[]));
        var calls=new List<string>();using var http=new HttpClient(new Transport(async request=>
        {
            var path=request.RequestUri!.AbsolutePath;calls.Add(path);
            if(path.EndsWith("/catalog")){Assert.Equal(HttpMethod.Get,request.Method);return new(HttpStatusCode.OK){Content=JsonContent.Create(catalog)};}
            Assert.Equal(HttpMethod.Post,request.Method);
            if(path.EndsWith("/review")){var selected=await request.Content!.ReadFromJsonAsync<DrillingReportingReassignmentSelection>();Assert.Equal(catalog.Revision,selected!.CatalogRevision);return new(HttpStatusCode.OK){Content=JsonContent.Create(original.Review)};}
            var sent=await request.Content!.ReadFromJsonAsync<DrillingReportingReassignment>();Assert.Equal(original.Revision(),sent!.Revision());
            return new(HttpStatusCode.OK){Content=JsonContent.Create(new DrillingReportingReassignmentReceipt(original.Revision(),original.Review.Responsibility.Root.InstanceId(),true,original.HistoryId(),original.ReassignedAtUtc,2,original.StepInstanceId(),original.TaskIds(),new('A',64),DateTimeOffset.UtcNow))};
        })){BaseAddress=new("http://localhost")};var client=new DrillingServiceClient(new(http,Mock.Of<IFailureReporter>(),company));
        var loaded=await client.GetReportingReassignmentCatalogAsync(original.Review.Responsibility.Root.InstanceId());
        Assert.Equal(original.Review.Revision,(await client.ReviewReportingReassignmentAsync(new(loaded.Responsibility.Root.InstanceId(),loaded.Revision,original.Review.Destination.ProcessId,original.Review.ManagerRole))).Revision);
        Assert.True((await client.ReassignReportingAsync(original)).Recorded);Assert.True((await client.RecoverReportingReassignmentAsync(original)).Recorded);
        Assert.Equal(new[]{"/api/drilling/programs/execution/reporting/transfer/catalog","/api/drilling/programs/execution/reporting/transfer/review","/api/drilling/programs/execution/reporting/transfer","/api/drilling/programs/execution/reporting/transfer/recover"},calls);
        company.TrySelect(company.Clear(),"foreign",new(scope.Company,[],[]));await Assert.ThrowsAsync<InvalidOperationException>(()=>client.ReassignReportingAsync(original));Assert.Equal(4,calls.Count);
    }
}
#endif
