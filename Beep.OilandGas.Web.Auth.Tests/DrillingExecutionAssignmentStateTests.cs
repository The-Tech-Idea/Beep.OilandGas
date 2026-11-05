using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DrillingExecutionAssignmentStateTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DrillingExecutionAssignmentNativeTests.Fixture Native = new();
        internal CompanyClientContext Company = new();
        internal Mock<IPersonaContextService> Personas = new();
        internal Mock<IDrillingServiceClient> Client = new();
        internal Mock<IDevelopmentDraftIntentStore> Journal = new();
        internal DrillingExecutionAssignmentReview Review = null!;
        internal DrillingExecutionAssignmentState State = null!;
        internal DrillingExecutionAssignment? Retained;
        internal string? Fault;
        internal int Writes;
        internal Func<Task>? BeforeRetain, BeforeClear;
        internal async Task Prepare()
        {
            await Native.Prepare();var original=Native.Original;var scope=original.Scope();
            Company.TrySelect(Company.Clear(),scope.ActorUserId,new(scope.Company,[original.SenderRole],
                ["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View","Drilling.Edit","Drilling.ManageWellProgram","Workflow.Start"]));
            Personas.SetupGet(p=>p.CurrentProfile).Returns(new AppUserPersona{UserId=scope.ActorUserId,PersonaCode=original.SenderPersonaCode});
            Personas.SetupGet(p=>p.CurrentPersona).Returns(new AppPersona{Code=original.SenderPersonaCode,Name="Execution coordination"});
            Personas.Setup(p=>p.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            Review=new(original.Source,[original.Workflow],[original.SenderRole],original.SenderPersonaCode,original.SenderAuthorityRevision,null,"",DateTimeOffset.UtcNow);
            Review=Review with{Revision=Review.ContentRevision()};Review.Require(scope.Company,scope.ActorUserId,original.Source.Basis.ReviewInstanceId);
            Client.Setup(c=>c.GetExecutionAssignmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>CompanyInventorySourceReview.Copy(Review));
            Journal.Setup(j=>j.ReadExecutionAssignmentAsync(It.IsAny<CompanyScope>(),It.IsAny<string>())).ReturnsAsync(()=>Retained is null?null:CompanyInventorySourceReview.Copy(Retained));
            Journal.Setup(j=>j.RetainExecutionAssignmentAsync(It.IsAny<DrillingExecutionAssignment>())).Returns(async(DrillingExecutionAssignment input)=>{
                if(BeforeRetain is not null)await BeforeRetain();if(Fault=="retain")throw new IOException("Storage failed");Retained=CompanyInventorySourceReview.Copy(input);});
            Journal.Setup(j=>j.ClearExecutionAssignmentAsync(It.IsAny<DrillingExecutionAssignment>())).Returns(async(DrillingExecutionAssignment input)=>{
                if(BeforeClear is not null)await BeforeClear();if(Fault=="clear")throw new IOException("Removal failed");if(Retained is not null)Assert.Equal(input.Revision(),Retained.Revision());Retained=null;});
            Client.Setup(c=>c.AssignExecutionAsync(It.IsAny<DrillingExecutionAssignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionAssignment input,CancellationToken _)=>{
                Writes++;if(Fault=="response")throw new HttpRequestException("Lost acknowledgement");return Fault=="receipt"?Receipt(input) with{IntentRevision=new('F',64)}:Receipt(input);});
            Client.Setup(c=>c.RecoverExecutionAssignmentAsync(It.IsAny<DrillingExecutionAssignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionAssignment input,CancellationToken _)=>Receipt(input));
            State=NewState();await State.LoadAsync(original.Source.Basis.ReviewInstanceId);
        }
        internal DrillingExecutionAssignmentState NewState()=>new(Company,Personas.Object,Client.Object,Journal.Object,(_,_,message)=>message);
        internal void Freeze()=>State.Prepare(Native.Original.Workflow.ProcessId,Native.Original.SenderRole,"Original site reporting responsibility confirmed");
        public void Dispose(){State?.Dispose();Company.Dispose();}
    }
    internal static DrillingExecutionAssignmentReceipt Receipt(DrillingExecutionAssignment original,bool recorded=true)
    {
        var step=DrillingExecutionAssignmentReceipt.StepId(original.InstanceId(),original.Workflow.Step.StepId);
        return new(original.Revision(),original.InstanceId(),recorded,recorded?step:null,recorded?original.Workflow.Step.PersonaCodes.Select(persona=>
            "task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=original.InstanceId(),Step=step,Persona=persona}))).ToArray():[],new('D',64),DateTimeOffset.UtcNow);
    }
    [Theory,InlineData("workflow"),InlineData("role"),InlineData("reason")]
    public async Task ExplicitConfiguredWorkflowSenderRoleAndReasonAreRequired(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.State.Prepare(fault=="workflow"?"unconfigured":f.Native.Original.Workflow.ProcessId,
            fault=="role"?"ForeignRole":f.Native.Original.SenderRole,fault=="reason"?" ":"Explicit original responsibility");
        Assert.Null(f.State.Prepared);Assert.NotNull(f.State.Error);Assert.Equal(0,f.Writes);Assert.Null(f.Retained);
    }
    [Fact]public async Task FrozenSourceRetainsBeforeTransportAndReceiptClearsOnlyTheOriginal()
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();var input=f.State.Prepared!;f.Review.Source.Recorded!.STATUS="Consumer mutation";
        Assert.Equal("STARTED",f.State.Prepared!.Source.Recorded!.STATUS);await f.State.SubmitAsync();Assert.Equal(1,f.Writes);
        Assert.True(f.State.Receipt!.Recorded);Assert.Equal(input.Revision(),f.State.Receipt.IntentRevision);Assert.Null(f.Retained);Assert.Null(f.State.Pending);
    }
    [Theory,InlineData("retain"),InlineData("response"),InlineData("receipt"),InlineData("clear")]
    public async Task RetentionTransportReceiptOrCleanupFailurePreservesOriginalWithoutBlindRetry(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.Fault=fault;f.Freeze();var input=f.State.Prepared!;await f.State.SubmitAsync();
        Assert.Equal(input.Revision(),f.State.Pending!.Revision());Assert.NotNull(f.State.Error);Assert.False(f.State.CanPrepare);
        Assert.Equal(fault=="retain"?0:1,f.Writes);
        if(fault=="retain")Assert.Contains("No submission was sent",f.State.PendingExplanation);
        if(fault=="clear"){Assert.True(f.State.Receipt!.Recorded);Assert.Contains("awaiting input cleanup",f.State.PendingTitle);}
        else Assert.Null(f.State.Receipt);
        f.Fault=null;await f.State.RecoverAsync();Assert.Null(f.State.Pending);Assert.True(f.State.Receipt!.Recorded);Assert.Equal(fault=="retain"?0:1,f.Writes);
    }
    [Fact]public async Task NewOwnerViewOnlyUnmappedRecoveryLoadsOriginalBeforeSourceOrPersonaLookup()
    {
        using var f=new Fixture();await f.Prepare();f.Fault="response";f.Freeze();await f.State.SubmitAsync();var input=f.State.Pending!;
        var selection=f.Company.Selection!;f.Company.TrySelect(f.Company.Clear(),selection.UserId,new(selection.Scope,["ViewOnlyRole"],
            ["Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        f.Personas.Setup(p=>p.EnsureLoadedAsync()).Throws(new InvalidOperationException("An unmapped recovery must not load workspace"));
        f.Client.Invocations.Clear();using var next=f.NewState();await next.LoadAsync(input.Source.Basis.ReviewInstanceId);
        Assert.Equal(input.Revision(),next.Pending!.Revision());Assert.True(next.CanRecover);await next.RecoverAsync();Assert.True(next.Receipt!.Recorded);
        f.Client.Verify(c=>c.GetExecutionAssignmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());
        Assert.Equal(1,f.Writes);
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LateAcknowledgementCannotPublishReceiptOrClearOriginalAfterContextChange(string change)
    {
        using var f=new Fixture();await f.Prepare();f.Freeze();var answer=new TaskCompletionSource<DrillingExecutionAssignmentReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c=>c.AssignExecutionAsync(It.IsAny<DrillingExecutionAssignment>(),It.IsAny<CancellationToken>())).Returns(answer.Task);
        var input=f.State.Prepared!;var save=f.State.SubmitAsync();Assert.NotNull(f.Retained);
        if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);
        if(change=="route")await f.State.LoadAsync("another");if(change=="dispose")f.State.Dispose();answer.SetResult(Receipt(input));await save;
        Assert.Null(f.State.Receipt);Assert.Null(f.State.Pending);Assert.Equal(input.Revision(),f.Retained!.Revision());
        f.Journal.Verify(j=>j.ClearExecutionAssignmentAsync(It.IsAny<DrillingExecutionAssignment>()),Times.Never());
    }
    [Fact]public async Task ProtectedAssignmentOriginalSeparatesCommandKindsAccountsCompaniesAndConflicts()
    {
        using var f=new Fixture();await f.Prepare();var input=f.Native.Original;var scope=input.Scope();var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();
        DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));await Store().RetainExecutionAssignmentAsync(input);
        Assert.DoesNotContain(input.Reason,Assert.Single(tab.Values).Value);Assert.Equal(input.Revision(),(await Store().ReadExecutionAssignmentAsync(scope.Company,scope.ActorUserId))!.Revision());
        Assert.Null(await Store().ReadExecutionAsync(scope.Company,scope.ActorUserId));Assert.Null(await Store().ReadDailyReportAsync(scope.Company,scope.ActorUserId));
        Assert.Null(await Store().ReadExecutionAssignmentAsync(new("tenant","other"),scope.ActorUserId));Assert.Null(await Store().ReadExecutionAssignmentAsync(scope.Company,"other"));
        await Assert.ThrowsAnyAsync<Exception>(()=>Store().RetainExecutionAssignmentAsync(input with{IntentId=Guid.NewGuid()}));
        await Assert.ThrowsAnyAsync<Exception>(()=>Store().ClearExecutionAssignmentAsync(input with{Reason="Changed"}));
        await Store().ClearExecutionAssignmentAsync(input);Assert.Empty(tab.Values);
        await Store().RetainExecutionAssignmentAsync(input);tab.Values[Assert.Single(tab.Values).Key]="corrupt";
        await Assert.ThrowsAnyAsync<Exception>(()=>Store().ReadExecutionAssignmentAsync(scope.Company,scope.ActorUserId));
    }
    [Theory,InlineData(false),InlineData(true)]public async Task ActualSenderScreenUsesEmptyExplicitFieldsThenFrozenConfirmationAndOutcome(bool lostAcknowledgement)
    {
        using var f=new Fixture();await f.Prepare();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(f.Journal.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/assignment?reviewId="+Uri.EscapeDataString(f.Native.Original.Source.Basis.ReviewInstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingExecutionAssignment>();
        page.WaitForAssertion(()=>Assert.Contains("Configured reporting destinations",page.Markup));Assert.Contains("Assignment reason",page.Markup);
        var selects=page.FindComponents<MudSelect<string>>();await page.InvokeAsync(()=>selects[0].Instance.ValueChanged.InvokeAsync(f.Native.Original.Workflow.ProcessId));
        await page.InvokeAsync(()=>selects[1].Instance.ValueChanged.InvokeAsync(f.Native.Original.SenderRole));
        await page.InvokeAsync(()=>page.FindComponent<MudTextField<string>>().Instance.ValueChanged.InvokeAsync("Actual reporting assignment and destination confirmed"));
        await page.FindAll("button").Single(x=>x.TextContent=="Review reporting assignment").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        Assert.True(page.Markup.Contains("Confirm original reporting assignment",StringComparison.Ordinal),string.Join(" | ",page.FindAll(".mud-alert-message").Select(x=>x.TextContent)));Assert.Equal(0,f.Writes);
        if(lostAcknowledgement)f.Fault="response";
        await page.FindAll("button").Single(x=>x.TextContent=="Confirm and retain reporting assignment").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        if(lostAcknowledgement)
        {
            page.WaitForAssertion(()=>Assert.Contains("Retained original reporting assignment",page.Markup));Assert.NotNull(f.Retained);
            Assert.DoesNotContain("Read-only source.",page.Markup);f.Fault=null;
            await page.FindAll("button").Single(x=>x.TextContent=="Recover original reporting assignment").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        }
        page.WaitForAssertion(()=>Assert.Contains("Original reporting assignment outcome",page.Markup));Assert.Equal(1,f.Writes);Assert.Null(f.Retained);
    }
}
#endif
