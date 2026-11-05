using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProcessDefinitionDetailStateTests
{
    private static CompanyClientContext Company(){var company=new CompanyClientContext();Select(company,"company");return company;}
    private static void Select(CompanyClientContext company,string id)=>Assert.True(company.TrySelect(company.Clear(),"accountant",new(new("tenant",id),[],[])));
    private static ProcessDefinition Definition()=>new(){ProcessId="definition",ProcessName="Actual financial definition",ProcessType="FINANCIAL",EntityType="JOURNAL_ENTRY",IsActive=true,CreatedDate=default,Steps=[new(){StepId="prepare",StepName="Recorded preparation",SequenceNumber=1,StepType="DATA_ENTRY",RequiredRoles=["Accountant"]}]};
    private static Task<string?> Field()=>Task.FromResult<string?>("field");
    private static ProcessInstance Instance()=>new(){InstanceId="instance/a?b",ProcessId="definition",FieldId="field",EntityId="journal",EntityType="JOURNAL_ENTRY",StartedBy="accountant"};
    private static Task Review(ProcessDefinitionDetailState state,CompanyClientContext company)=>state.ReviewSourceAsync("journal",request=>Task.FromResult(new ProcessStartSourceReview(company.Selection!.Scope,company.Selection.UserId,request.ExpectedFieldId,request.ProcessId,request.ExpectedDefinitionRevision,request.EntityId,request.EntityType!,"JE-1","Recorded journal","DRAFT","preparer",new string('A',64),DateTimeOffset.UtcNow)));
    [Fact] public async Task ReviewedDefinitionIsFrozenAndStartUsesItsExactFieldTypeAndRevision()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);var definition=Definition();await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(definition));
        definition.EntityType="WELL";definition.Steps[0].RequiredRoles.Clear();await Review(state,company);var attempt=state.Begin("journal");Assert.Equal("JOURNAL_ENTRY",attempt.Request.EntityType);Assert.Equal("field",attempt.Request.ExpectedFieldId);Assert.Equal(64,attempt.Request.ExpectedDefinitionRevision.Length);Assert.Equal("Accountant",Assert.Single(state.Current!.Steps[0].RequiredRoles));
        Assert.True(state.IsBlocked("journal"));state.Confirm(attempt,Instance(),Instance());Assert.False(state.IsBlocked("journal"));
    }
    [Theory][InlineData("missing")][InlineData("identity")][InlineData("step")][InlineData("sequence")][InlineData("role")]
    public async Task MalformedDefinitionCannotRemainVisibleOrStart(string fault)
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);var definition=Definition();
        if(fault=="identity")definition.ProcessId="other";if(fault=="step")definition.Steps.Add(definition.Steps[0]);if(fault=="sequence")definition.Steps[0].SequenceNumber=0;if(fault=="role")definition.Steps[0].RequiredRoles=null!;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(fault=="missing"?null:definition)));Assert.Null(state.Current);Assert.Throws<InvalidOperationException>(()=>state.Begin("journal"));
    }
    [Theory][InlineData("response")][InlineData("readback")][InlineData("field")][InlineData("entity")][InlineData("type")][InlineData("actor")][InlineData("instance")]
    public async Task UnconfirmedStartCannotReleaseRetainedIntent(string fault)
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Review(state,company);var attempt=state.Begin("journal");var response=Instance();var recorded=Instance();
        if(fault=="field")recorded.FieldId="other";if(fault=="entity")recorded.EntityId="other";if(fault=="type")recorded.EntityType="WELL";if(fault=="actor")recorded.StartedBy="other";if(fault=="instance")recorded.InstanceId="other";
        Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,fault=="response"?null:response,fault=="readback"?null:recorded));Assert.True(state.IsBlocked("journal"));Assert.Throws<InvalidOperationException>(()=>state.Begin("journal"));
    }
    [Fact] public async Task PendingStartSurvivesCompanyAwayAndBackWithoutOldDisplay()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Review(state,company);var attempt=state.Begin("journal");Select(company,"other");Assert.Null(state.Current);Assert.False(state.IsCurrent(attempt));Select(company,"company");await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));Assert.True(state.IsBlocked("journal"));
    }
    [Fact] public async Task LateDefinitionCannotPublishAfterFieldInvalidation()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);var pending=new TaskCompletionSource<ProcessDefinition?>();var load=state.LoadAsync("definition",Field,_=>pending.Task);state.Clear();pending.SetResult(Definition());await load;Assert.Null(state.Current);
    }
    [Fact] public async Task FailedReloadClearsPreviousDefinition()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadAsync("definition",Field,_=>Task.FromException<ProcessDefinition?>(new HttpRequestException())));Assert.Null(state.Current);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task InactiveOrEmptyDefinitionCannotStart(bool empty)
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);var definition=Definition();if(empty)definition.Steps=[];else definition.IsActive=false;await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(definition));Assert.Throws<InvalidOperationException>(()=>state.Begin("journal"));
    }
    [Fact] public async Task StartRequiresRecordedReviewAndExactReviewedEntity()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);
        await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));
        Assert.Throws<InvalidOperationException>(()=>state.Begin("journal"));await Review(state,company);
        Assert.Throws<InvalidOperationException>(()=>state.Begin("another"));var attempt=state.Begin("journal");Assert.Equal(new string('A',64),attempt.Request.ExpectedSourceRevision);
    }
    [Fact] public async Task EditingSourceClearsEarlierReviewAndRefusesStart()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);
        await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Review(state,company);state.ClearSource();
        Assert.Null(state.SourceReview);Assert.Throws<InvalidOperationException>(()=>state.Begin("journal"));
    }
    [Theory][InlineData("company")][InlineData("field")][InlineData("input")]
    public async Task LateSourceReviewCannotPopulateChangedContext(string change)
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);
        await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Review(state,company);var review=state.SourceReview!;
        var pending=new TaskCompletionSource<ProcessStartSourceReview>();var load=state.ReviewSourceAsync("journal",_=>pending.Task);
        if(change=="company")Select(company,"another");else if(change=="field")state.Clear();else state.ClearSource();
        pending.SetResult(review);await load;Assert.Null(state.SourceReview);
    }
    [Fact] public async Task FailedReviewClearsEarlierSource()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);
        await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Review(state,company);
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.ReviewSourceAsync("journal",_=>Task.FromException<ProcessStartSourceReview>(new HttpRequestException())));
        Assert.Null(state.SourceReview);Assert.Throws<InvalidOperationException>(()=>state.Begin("journal"));
    }
    [Fact] public async Task OlderOverlappingReviewCannotReplaceCurrentSourceEvidence()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);
        await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Review(state,company);var original=state.SourceReview!;
        var pending=new TaskCompletionSource<ProcessStartSourceReview>();var older=state.ReviewSourceAsync("journal",_=>pending.Task);
        await state.ReviewSourceAsync("journal",_=>Task.FromResult(original with{RecordedReference="newer"}));pending.SetResult(original);await older;
        Assert.Equal("newer",state.SourceReview!.RecordedReference);
    }
    [Fact] public async Task ChangedContextReadbackCannotReleaseOriginalPendingIntent()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);
        await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Review(state,company);var attempt=state.Begin("journal");Select(company,"another");
        Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,Instance(),Instance()));Select(company,"company");
        await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));Assert.True(state.IsBlocked("journal"));
    }
    [Fact] public async Task ClearingOriginalSourceReviewInvalidatesPendingConfirmation()
    {
        using var company=Company();using var state=new ProcessDefinitionDetailState(company);
        await state.LoadAsync("definition",Field,_=>Task.FromResult<ProcessDefinition?>(Definition()));await Review(state,company);var attempt=state.Begin("journal");state.ClearSource();
        Assert.False(state.IsCurrent(attempt));Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,Instance(),Instance()));Assert.True(state.IsBlocked("journal"));
    }
}
