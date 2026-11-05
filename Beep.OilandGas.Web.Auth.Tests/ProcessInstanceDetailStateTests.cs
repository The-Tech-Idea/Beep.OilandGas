using Beep.OilandGas.ProductionOperations.Models;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProcessInstanceDetailStateTests
{
    [Theory][InlineData("missing")][InlineData("order")][InlineData("field")][InlineData("component")][InlineData("time")][InlineData("inspection")]
    public async Task MaintenanceActionReviewRequiresOriginalRecordedSource(string fault)
    {
        using var c=Company();var source=Source();source.EntityType="WORK_ORDER";source.EntityId="order";
        using var state=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(source),_=>Task.FromResult(History()));
        await state.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        var order=new Beep.OilandGas.ProductionOperations.Models.WorkOrderSourceReview("order","field","WO-42","type",null,null,"Recorded instructions",[new("order","component","field","well","")],[],DateTimeOffset.UtcNow);
        order=fault switch{"order"=>order with{WorkOrderId="other"},"field"=>order with{FieldId="other"},"component"=>order with{Components=[]},"time"=>order with{RetrievedAtUtc=default},"inspection"=>order with{Inspections=null!},_=>order};
        var review=Review() with{WorkOrder=fault=="missing"?null:order};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadActionsAsync(_=>Task.FromResult(review)));Assert.Null(state.ActionReview);
    }
    [Fact] public async Task ConfirmedMaintenanceReviewPublishesAndClearsWithCompany()
    {
        using var c=Company();var source=Source();source.EntityType="WORK_ORDER";source.EntityId="order";
        using var state=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(source),_=>Task.FromResult(History()));await state.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        var order=new Beep.OilandGas.ProductionOperations.Models.WorkOrderSourceReview("order","field","WO-42","type",null,null,"Recorded instructions",[new("order","component","field","well","")],[],DateTimeOffset.UtcNow);
        await state.LoadActionsAsync(_=>Task.FromResult(Review() with{WorkOrder=order}));Assert.Equal("WO-42",state.ActionReview!.WorkOrder!.RecordedNumber);c.Clear();Assert.Null(state.ActionReview);
    }
    [Fact] public async Task EmptyResponseClearsEarlierActionReviewWithoutNullReferenceFailure()
    {
        using var company=Company();using var state=new ProcessInstanceDetailState(company,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));
        await state.LoadAsync("source",()=>Task.FromResult<string?>("field"));await state.LoadActionsAsync(_=>Task.FromResult(Review()));Assert.NotNull(state.ActionReview);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadActionsAsync(_=>Task.FromResult<Beep.OilandGas.LifeCycle.Models.ProcessTransitionReview>(null!)));Assert.Null(state.ActionReview);Assert.NotNull(state.Instance);
    }
    private static CompanyClientContext Company(){var c=new CompanyClientContext();Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","A"),[],[])));return c;}
    private static ProcessInstance Source()=>new(){InstanceId="source",FieldId="field",EntityId="invoice",EntityType="INVOICE",ProcessId="P2P",CurrentState="draft",CurrentStepId="review",Status=ProcessStatus.COMPLETED};
    private static List<ProcessHistoryEntry> History()=>[new(){HistoryId="history",InstanceId="source",Action="APPROVED",PerformedBy="reviewer"}];
    [Fact] public async Task RecordedStatusAndHistoryPublishTogether(){using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));Assert.Equal(ProcessStatus.COMPLETED,s.Instance!.Status);Assert.Equal("reviewer",Assert.Single(s.History).PerformedBy);}
    [Theory][InlineData("identity")][InlineData("field")][InlineData("history")][InlineData("duplicate")]
    public async Task UnconfirmedSourceOrHistoryCannotPublish(string fault){using var c=Company();var source=Source();var rows=History();if(fault=="identity")source.InstanceId="other";if(fault=="field")source.FieldId="other";if(fault=="history")rows[0].InstanceId="other";if(fault=="duplicate")rows.Add(rows[0]);using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(source),_=>Task.FromResult(rows));await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadAsync("source",()=>Task.FromResult<string?>("field")));Assert.Null(s.Instance);Assert.Empty(s.History);}
    [Fact] public async Task CompanySwitchDiscardsLateSource(){using var c=Company();var pending=new TaskCompletionSource<ProcessInstance?>(TaskCreationOptions.RunContinuationsAsynchronously);var historyCalls=0;using var s=new ProcessInstanceDetailState(c,_=>pending.Task,_=>{historyCalls++;return Task.FromResult(History());});var load=s.LoadAsync("source",()=>Task.FromResult<string?>("field"));c.Clear();pending.SetResult(Source());await load;Assert.Null(s.Instance);Assert.Equal(0,historyCalls);}
    [Fact] public async Task FieldInvalidationDiscardsLateHistory(){using var c=Company();var pending=new TaskCompletionSource<List<ProcessHistoryEntry>>(TaskCreationOptions.RunContinuationsAsynchronously);using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>pending.Task);var load=s.LoadAsync("source",()=>Task.FromResult<string?>("field"));s.Clear();pending.SetResult(History());await load;Assert.Null(s.Instance);Assert.Empty(s.History);}
    [Fact] public async Task FailedReloadClearsPreviouslyConfirmedRecord(){using var c=Company();var fail=false;using var s=new ProcessInstanceDetailState(c,_=>fail?throw new HttpRequestException():Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));fail=true;await Assert.ThrowsAsync<HttpRequestException>(()=>s.LoadAsync("source",()=>Task.FromResult<string?>("field")));Assert.Null(s.Instance);Assert.Empty(s.History);}
    [Fact] public async Task MissingFieldDoesNotRequestData(){using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>throw new Exception("no request"),_=>throw new Exception("no request"));await s.LoadAsync("source",()=>Task.FromResult<string?>(null));Assert.Null(s.Instance);}
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ContextChangeDuringFieldLookupCannotAdoptNewScope(bool companyChange)
    {
        using var c=Company();var pending=new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);var reads=0;
        using var s=new ProcessInstanceDetailState(c,_=>{reads++;return Task.FromResult<ProcessInstance?>(Source());},_=>Task.FromResult(History()));
        var load=s.LoadAsync("source",()=>pending.Task);
        if(companyChange)c.Clear();else s.Clear();
        pending.SetResult("field");await load;Assert.Equal(0,reads);Assert.Null(s.Instance);Assert.Empty(s.History);
    }
    private static Beep.OilandGas.LifeCycle.Models.ProcessTransitionReview Review()=>new(new("tenant","A"),"actor","field","source","draft","review",new string('A',64),[new("t","draft","approved","approve",true)]);
    [Theory][InlineData("company")][InlineData("state")][InlineData("actor")]
    public async Task ActionReviewMustMatchDisplayedSource(string fault)
    {
        using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        var review=Review();review=fault switch { "company"=>review with { Company=new("tenant","B") },"state"=>review with { CurrentState="changed" },_=>review with { ActorUserId="other" } };
        await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadActionsAsync(_=>Task.FromResult(review)));Assert.Null(s.ActionReview);
    }
    [Fact] public async Task LateActionReviewCannotPublishAfterInvalidation()
    {
        using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        var pending=new TaskCompletionSource<Beep.OilandGas.LifeCycle.Models.ProcessTransitionReview>(TaskCreationOptions.RunContinuationsAsynchronously);var load=s.LoadActionsAsync(_=>pending.Task);s.Clear();pending.SetResult(Review());await load;Assert.Null(s.ActionReview);
    }
    [Fact] public async Task ConfirmedActionReviewPublishesExactRevisionAndChoices()
    {
        using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review()));Assert.Equal(new string('A',64),s.ActionReview!.Revision);Assert.Equal("approved",Assert.Single(s.ActionReview.Choices).ToStateId);
    }
    [Fact] public async Task UncertainSubmissionBlocksRepeatAfterReloadAndChangedRevision()
    {
        using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review()));
        var original=s.ActionReview!;Assert.True(s.TryBeginSubmission(original,original.Choices[0]));
        Assert.False(s.TryBeginSubmission(original,original.Choices[0]));Assert.True(s.HasUnconfirmedAttempt);
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review() with {Revision=new string('B',64)}));
        Assert.True(s.HasUnconfirmedAttempt);Assert.False(s.CanSubmit(s.ActionReview!));
    }
    [Fact] public async Task ReturnedResponseBlocksOldRevisionButAllowsFreshReviewedAction()
    {
        using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review()));
        var original=s.ActionReview!;Assert.True(s.TryBeginSubmission(original,original.Choices[0]));s.MarkResponseReturned(original);
        Assert.False(s.HasUnconfirmedAttempt);Assert.False(s.CanSubmit(original));
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review()));Assert.False(s.CanSubmit(s.ActionReview!));
        await s.LoadActionsAsync(_=>Task.FromResult(Review() with {Revision=new string('B',64)}));Assert.True(s.CanSubmit(s.ActionReview!));
    }
    [Fact] public async Task UncertainAttemptSurvivesCompanyReturnWithoutLeakingToOtherCompany()
    {
        using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review()));
        var original=s.ActionReview!;Assert.True(s.TryBeginSubmission(original,original.Choices[0]));
        Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","B"),[],[])));
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review() with {Company=new("tenant","B")}));
        Assert.False(s.HasUnconfirmedAttempt);Assert.True(s.CanSubmit(s.ActionReview!));
        Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","A"),[],[])));
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review()));
        Assert.True(s.HasUnconfirmedAttempt);Assert.False(s.CanSubmit(s.ActionReview!));
    }
    [Fact] public async Task UnreviewedChoiceAndInvalidatedReviewCannotStartSubmission()
    {
        using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));await s.LoadActionsAsync(_=>Task.FromResult(Review()));
        var original=s.ActionReview!;Assert.False(s.TryBeginSubmission(original,original.Choices[0] with {ToStateId="unreviewed"}));
        Assert.False(s.HasUnconfirmedAttempt);s.Clear();Assert.False(s.TryBeginSubmission(original,original.Choices[0]));
    }
    private static Beep.OilandGas.LifeCycle.Models.JournalSourceReview JournalReview()=>new("invoice","JE-1",null,"DRAFT",null,"preparer",null,null,null,null,null,0m,DateTimeOffset.UtcNow,[]);
    [Theory][InlineData("missing")][InlineData("identity")][InlineData("timestamp")][InlineData("offset")]
    public async Task JournalSourceProjectionMustMatchDisplayedEntity(string fault)
    {
        using var c=Company();var source=Source();source.EntityType="JOURNAL_ENTRY";
        using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(source),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        var journal=JournalReview();var review=Review() with {Journal=fault switch {"missing"=>null,"identity"=>journal with {JournalEntryId="other"},"timestamp"=>journal with {RetrievedAtUtc=default},_=>journal with {RetrievedAtUtc=DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3))}}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadActionsAsync(_=>Task.FromResult(review)));Assert.Null(s.ActionReview);
    }
    [Fact] public async Task RecordedJournalProjectionClearsWithSourceInvalidation()
    {
        using var c=Company();var source=Source();source.EntityType="JOURNAL_ENTRY";
        using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(source),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        await s.LoadActionsAsync(_=>Task.FromResult(Review() with {Journal=JournalReview()}));
        Assert.Null(s.ActionReview!.Journal!.TotalDebit);Assert.Equal(0m,s.ActionReview.Journal.TotalCredit);
        c.Clear();Assert.Null(s.ActionReview);Assert.Null(s.Instance);
    }
    [Fact] public async Task NonJournalSourceCannotPublishJournalDetails()
    {
        using var c=Company();using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(Source()),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadActionsAsync(_=>Task.FromResult(Review() with {Journal=JournalReview()})));Assert.Null(s.ActionReview);
    }
    [Theory][InlineData("missing")][InlineData("foreign")][InlineData("duplicate")][InlineData("account")]
    public async Task InvalidJournalLineProjectionCannotPublish(string fault)
    {
        using var c=Company();var source=Source();source.EntityType="JOURNAL_ENTRY";
        using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(source),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        var line=new Beep.OilandGas.LifeCycle.Models.JournalSourceLineReview("line","invoice",1,"account",10m,null,"KWD",null,null,null);
        var lines=fault switch {"missing"=>null,"foreign"=>new[]{line with {JournalEntryId="other"}},"duplicate"=>new[]{line,line},_=>new[]{line with {AccountId=""}}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadActionsAsync(_=>Task.FromResult(Review() with {Journal=JournalReview() with {Lines=lines}})));Assert.Null(s.ActionReview);
    }
    [Fact] public async Task JournalLinesPublishWithRecordedNullableValuesAndClearOnReload()
    {
        using var c=Company();var source=Source();source.EntityType="JOURNAL_ENTRY";
        using var s=new ProcessInstanceDetailState(c,_=>Task.FromResult<ProcessInstance?>(source),_=>Task.FromResult(History()));await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));
        var line=new Beep.OilandGas.LifeCycle.Models.JournalSourceLineReview("line","invoice",1,"account",10m,null,"KWD",null,null,null);
        await s.LoadActionsAsync(_=>Task.FromResult(Review() with {Journal=JournalReview() with {Lines=[line]}}));
        var recorded=Assert.Single(s.ActionReview!.Journal!.Lines!);Assert.Equal("KWD",recorded.CurrencyCode);Assert.Null(recorded.Credit);Assert.Null(recorded.ExchangeRate);
        await s.LoadAsync("source",()=>Task.FromResult<string?>("field"));Assert.Null(s.ActionReview);
    }
}
