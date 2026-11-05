#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DevelopmentInvestmentHandoffStateTests
{
    private sealed class Fixture:IDisposable
    {
        internal DevelopmentInvestmentHandoffNativeTests.Fixture Native=new();internal DevelopmentInvestmentHandoffState State=null!;
        internal DevelopmentDraftWorkspaceTests.Journal Journal=>Native.Source.Base.Journal;internal Mock<IAfeServiceClient> Client=>Native.Source.Base.Client;
        internal string Process=>Native.Original.Review.Source.Current.Source.Review.InstanceId;internal string Afe=>Native.Original.Review.Source.AfeId;
        internal async Task Initialize()
        {
            await Native.Prepare();Select();Client.Setup(client=>client.PrepareInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<DevelopmentInvestmentHandoffSelection>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Native.Original);
            Client.Setup(client=>client.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Status(false));
            State=NewState();await State.LoadAsync(Process,Afe);State.UseReview(Native.Original.Review);
        }
        internal void Select(bool write=true)=>Native.Source.Base.Company.TrySelect(Native.Source.Base.Company.Clear(),"investor",new(Native.Current.Current.Source.Review.Scope.Company,["InvestmentReviewer"],write?
            ["Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Start","Accounting.PrepareDevelopmentBudget"]:["Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        internal DevelopmentInvestmentHandoffState NewState()=>new(Native.Source.Base.Company,Native.Source.Base.Baseline.Reader.Personas.Object,Client.Object,Journal,(_,_,text)=>text);
        internal static DevelopmentInvestmentHandoffReceipt Receipt(DevelopmentInvestmentHandoff original,bool exists=true)=>new(original.Revision(),original.InstanceId(),exists,exists?"original-step":null,exists?["original-task"]:[],new string('D',64),DateTimeOffset.UtcNow);
        internal DevelopmentInvestmentHandoffStatus Status(bool exists)
        {
            var stage=Native.Original.Workflow.Steps.Single();var progress=new DevelopmentInvestmentProgress(Native.Original.InstanceId(),Native.Original.Workflow.Version,Native.Original.Workflow.DefinitionRevision,"IN_REVIEW",1,
                [new(1,stage.StepId,stage.StepName,stage.AssignedRole,stage.PersonaCodes,"PENDING",null,null,null,null,null,null)],null,null,null,new('C',64),"");progress=progress with{Revision=progress.ContentRevision()};
            var result=new DevelopmentInvestmentHandoffStatus(Native.Current,exists?Native.Original.InstanceId():null,exists?Native.Original.Workflow.ProcessName:null,exists?"investor":null,exists?DateTimeOffset.UtcNow:null,exists?progress:null,"",DateTimeOffset.UtcNow);return result with{Revision=result.ContentRevision()};
        }
        internal async Task Prepare(){await State.PrepareAsync(Native.Original.Workflow.ProcessId);Assert.NotNull(State.Prepared);}
        public void Dispose(){State?.Dispose();Native.Dispose();}
    }
    [Fact]public async Task ExplicitWorkflowSelectionFreezesAnOriginalAndRetentionPrecedesTheOnlyTransportWrite()
    {
        using var f=new Fixture();await f.Initialize();await f.State.PrepareAsync(null);Assert.Null(f.State.Prepared);await f.Prepare();var original=f.State.Prepared!;Assert.Null(f.Journal.HandoffOriginal);
        f.Client.Setup(client=>client.StartInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>())).Returns((DevelopmentInvestmentHandoff value,CancellationToken _)=>{
            Assert.Equal(value.Revision(),f.Journal.HandoffOriginal!.Revision());return Task.FromResult(Fixture.Receipt(value));});
        await f.State.SubmitAsync();Assert.True(f.State.Recorded!.Recorded);Assert.Null(f.State.Pending);Assert.Null(f.Journal.HandoffOriginal);Assert.False(f.State.CanPrepare);Assert.Contains("Budget approval remains separate",f.State.Confirmation);
        f.Client.Verify(client=>client.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("retain")][InlineData("transport")][InlineData("cleanup")]
    public async Task FailureCannotBecomeSuccessAndKeepsTheExactOriginalAvailableForRecovery(string fault)
    {
        using var f=new Fixture();await f.Initialize();await f.Prepare();var original=f.State.Prepared!;f.Journal.FailRetain=fault=="retain";f.Journal.FailClear=fault=="cleanup";
        f.Client.Setup(client=>client.StartInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>())).Returns((DevelopmentInvestmentHandoff value,CancellationToken _)=>fault=="transport"?Task.FromException<DevelopmentInvestmentHandoffReceipt>(new HttpRequestException("Unknown response")):Task.FromResult(Fixture.Receipt(value)));
        await f.State.SubmitAsync();Assert.NotNull(f.State.Pending);Assert.Null(f.State.Recorded);Assert.NotNull(f.State.Error);
        if(fault=="retain")f.Client.Verify(client=>client.StartInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>()),Times.Never);
        f.Journal.FailRetain=false;f.Journal.FailClear=false;f.Client.Setup(client=>client.RecoverInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>())).ReturnsAsync(Fixture.Receipt(original,fault!="retain"));
        await f.State.RecoverAsync();Assert.Null(f.State.Pending);Assert.Null(f.Journal.HandoffOriginal);Assert.NotNull(f.State.Confirmation);
    }
    [Fact]public async Task PageRecreationCanRecoverWithViewOnlyAuthorityWithoutRepeatingTheCommand()
    {
        using var f=new Fixture();await f.Initialize();f.Journal.HandoffOriginal=f.Native.Original;f.State.Dispose();f.Select(false);f.State=f.NewState();await f.State.LoadAsync(f.Process,f.Afe);Assert.True(f.State.CanRecover);Assert.False(f.State.CanPrepare);
        f.Client.Setup(client=>client.RecoverInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>())).ReturnsAsync(Fixture.Receipt(f.Native.Original));
        await f.State.RecoverAsync();Assert.True(f.State.Recorded!.Recorded);f.Client.Verify(client=>client.StartInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("prepare")][InlineData("write")][InlineData("status")]
    public async Task LateResponsesCannotPublishAcrossCompanyChangeAndOriginalWriteInputIsPreserved(string operation)
    {
        using var f=new Fixture();await f.Initialize();Task task;
        var prepared=new TaskCompletionSource<DevelopmentInvestmentHandoff>(TaskCreationOptions.RunContinuationsAsynchronously);var written=new TaskCompletionSource<DevelopmentInvestmentHandoffReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);var status=new TaskCompletionSource<DevelopmentInvestmentHandoffStatus>(TaskCreationOptions.RunContinuationsAsynchronously);
        if(operation=="prepare"){f.Client.Setup(client=>client.PrepareInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<DevelopmentInvestmentHandoffSelection>(),It.IsAny<CancellationToken>())).Returns(prepared.Task);task=f.State.PrepareAsync(f.Native.Original.Workflow.ProcessId);}
        else if(operation=="write"){await f.Prepare();f.Client.Setup(client=>client.StartInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>())).Returns(written.Task);task=f.State.SubmitAsync();}
        else{f.Client.Setup(client=>client.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(status.Task);task=f.State.ReadStatusAsync();}
        f.Native.Source.Base.Company.Clear();prepared.TrySetResult(f.Native.Original);written.TrySetResult(Fixture.Receipt(f.Native.Original));status.TrySetResult(f.Status(true));await task;
        Assert.Null(f.State.Prepared);Assert.Null(f.State.Recorded);Assert.Null(f.State.Status);if(operation=="write")Assert.NotNull(f.Journal.HandoffOriginal);
    }
    [Fact]public async Task ExactStatusReadbackSurvivesARecreatedOwnerAndNeverInfersAbsenceOnFailure()
    {
        using var f=new Fixture();await f.Initialize();f.Client.Setup(client=>client.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>f.Status(true));await f.State.ReadStatusAsync();Assert.Equal(f.Native.Original.InstanceId(),f.State.Status!.HandoffId);Assert.False(f.State.CanPrepare);
        f.State.Dispose();f.State=f.NewState();await f.State.LoadAsync(f.Process,f.Afe);await f.State.ReadStatusAsync();Assert.NotNull(f.State.Status!.HandoffId);
        f.Client.Setup(client=>client.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Unavailable"));await f.State.ReadStatusAsync();Assert.Null(f.State.Status);Assert.NotNull(f.State.Error);Assert.False(f.State.CanPrepare);
    }
    [Fact]public async Task ActualSourcePanelPreparesConfirmsAndReloadsTheRecordedHandoffWithoutFinancialApproval()
    {
        using var f=new Fixture();await f.Initialize();f.Client.Setup(client=>client.StartInvestmentHandoffAsync(It.IsAny<DevelopmentInvestmentHandoff>(),It.IsAny<CancellationToken>())).Returns((DevelopmentInvestmentHandoff original,CancellationToken _)=>Task.FromResult(Fixture.Receipt(original)));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Native.Source.Base.Company);ui.Services.AddSingleton(f.Native.Source.Base.Baseline.Reader.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var panel=ui.RenderComponent<DevelopmentInvestmentHandoffPanel>(parameters=>parameters.Add(value=>value.ProcessId,f.Process).Add(value=>value.AfeId,f.Afe).Add(value=>value.Review,f.Native.Original.Review));
        await panel.FindComponent<MudBlazor.MudSelect<string>>().InvokeAsync(()=>panel.FindComponent<MudBlazor.MudSelect<string>>().Instance.ValueChanged.InvokeAsync(f.Native.Original.Workflow.ProcessId));
        panel.FindAll("button").Single(button=>button.TextContent=="Review investment handoff").Click();panel.WaitForAssertion(()=>Assert.Contains("Confirm independent investment review handoff",panel.Markup));Assert.Contains("Configured investment review 1",panel.Markup);Assert.Contains("independent-reviewer",panel.Markup);
        panel.FindAll("button").Single(button=>button.TextContent=="Confirm investment handoff").Click();panel.WaitForAssertion(()=>Assert.Contains("Budget approval remains separate",panel.Markup));Assert.Null(f.Journal.HandoffOriginal);
        f.Client.Setup(client=>client.ReadInvestmentHandoffAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>f.Status(true));panel.FindAll("button").Single(button=>button.TextContent=="Read recorded investment handoff").Click();panel.WaitForAssertion(()=>Assert.Contains("Sender investor",panel.Markup));
        f.Client.Verify(client=>client.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
