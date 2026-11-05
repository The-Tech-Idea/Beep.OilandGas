using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProductionInterventionDecisionStateTests
{
    [Fact]public async Task OriginalAcknowledgementSurvivesConsumerMutationAndExplicitOriginalContextReturn()
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var attempt=state.Begin("well",Request());var result=Result();state.RetainAcknowledgement(attempt,result);result.WorkOrderId="Consumer changed response";Select(company,"B");Assert.False(state.HasAcknowledgement("field","well"));Select(company,"A");var reads=0;var recovered=await state.ReviewAcknowledgedOutcomeAsync("field","well",()=>{reads++;return Task.FromResult(new List<ProductionInterventionCandidateDto>{Readback(attempt)});});Assert.Equal(1,reads);Assert.Equal("order",recovered.WorkOrderId);Assert.False(state.IsBlocked("field","well"));Assert.False(state.HasAcknowledgement("field","well"));
    }
    [Fact]public async Task MissingAcknowledgementCannotInventRecoveryOrReadAnotherSource()
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);state.Begin("well",Request());var reads=0;await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAcknowledgedOutcomeAsync("field","well",()=>{reads++;return Task.FromResult(new List<ProductionInterventionCandidateDto>());}));Assert.Equal(0,reads);Assert.True(state.IsBlocked("field","well"));
    }
    [Theory][InlineData("changed")][InlineData("failure")]
    public async Task FailedFreshReadKeepsOriginalAcknowledgementAndBlock(string fault)
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var attempt=state.Begin("well",Request());state.RetainAcknowledgement(attempt,Result());await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAcknowledgedOutcomeAsync("field","well",()=>{if(fault=="failure")throw new InvalidOperationException("Native source unavailable");var record=Readback(attempt);record.LatestActivityRemark="Another recorded decision";return Task.FromResult(new List<ProductionInterventionCandidateDto>{record});}));Assert.True(state.HasAcknowledgement("field","well"));Assert.True(state.IsBlocked("field","well"));
    }
    [Fact]public async Task LateAwayAndBackReadCannotConfirmOriginalAcknowledgedDecision()
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var attempt=state.Begin("well",Request());state.RetainAcknowledgement(attempt,Result());var source=new TaskCompletionSource<List<ProductionInterventionCandidateDto>>(TaskCreationOptions.RunContinuationsAsynchronously);var reading=state.ReviewAcknowledgedOutcomeAsync("field","well",()=>source.Task);Select(company,"B");Select(company,"A");source.SetResult([Readback(attempt)]);await Assert.ThrowsAsync<OperationCanceledException>(()=>reading);Assert.True(state.HasAcknowledgement("field","well"));Assert.True(state.IsBlocked("field","well"));
    }
    [Fact]public async Task AnotherAccountCannotReviewRetainedAcknowledgement()
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var attempt=state.Begin("well",Request());state.RetainAcknowledgement(attempt,Result());Assert.True(company.TrySelect(company.Clear(),"other",new(new("tenant","A"),[],[])));var reads=0;await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAcknowledgedOutcomeAsync("field","well",()=>{reads++;return Task.FromResult(new List<ProductionInterventionCandidateDto>{Readback(attempt)});}));Assert.Equal(0,reads);Select(company,"A");Assert.True(state.IsBlocked("field","well"));
    }
    [Fact]public void ChangedAcknowledgementCannotReplaceOriginalEvidence()
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var attempt=state.Begin("well",Request());var wrong=Result();wrong.ReviewedSourceRevision=new string('B',64);Assert.Throws<InvalidOperationException>(()=>state.RetainAcknowledgement(attempt,wrong));Assert.False(state.HasAcknowledgement("field","well"));state.RetainAcknowledgement(attempt,Result());var replacement=Result();replacement.Message="Another response";Assert.Throws<InvalidOperationException>(()=>state.RetainAcknowledgement(attempt,replacement));Assert.True(state.HasAcknowledgement("field","well"));
    }
    [Fact]public void RetainedOriginalCannotBeChangedByRequestAttemptOrRecoveryConsumer()
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var request=Request();var attempt=state.Begin("well",request);request.Note="Changed input";attempt.Request.Note="Changed consumer";var recovery=state.Original("field","well")!;Assert.Equal("Reviewed evidence",recovery.Request.Note);recovery.Request.Note="Changed recovery";Assert.Equal("Reviewed evidence",state.Original("field","well")!.Request.Note);Assert.True(state.IsBlocked("field","well"));Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,Result(),Readback(attempt)));
    }
    [Theory][InlineData("company")][InlineData("account")]
    public void AnotherContextCannotReadOrConfirmOriginalDecision(string change)
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var attempt=state.Begin("well",Request());if(change=="company")Select(company,"B");else Assert.True(company.TrySelect(company.Clear(),"other",new(new("tenant","A"),[],[])));Assert.Null(state.Original("field","well"));Assert.False(state.IsBlocked("field","well"));Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,Result(),Readback(attempt)));Select(company,"A");Assert.Equal("Reviewed evidence",state.Original("field","well")!.Request.Note);Assert.True(state.IsBlocked("field","well"));Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,Result(),Readback(attempt)));
    }
    [Fact]public void ForgedAttemptCannotClearRetainedOriginalEvenWithMatchingFabricatedReadback()
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var attempt=state.Begin("well",Request());var forged=state.Original("field","well")!;forged.Request.Note="Another decision body";Assert.Throws<InvalidOperationException>(()=>state.Confirm(forged,Result(),Readback(forged)));Assert.True(state.IsBlocked("field","well"));state.Confirm(attempt,Result(),Readback(attempt));Assert.Null(state.Original("field","well"));Assert.False(state.IsBlocked("field","well"));Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,Result(),Readback(attempt)));
    }
    private static CompanyClientContext Company(){var c=new CompanyClientContext();Select(c,"A");return c;}
    private static void Select(CompanyClientContext c,string id)=>Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant",id),[],[])));
    private static ProductionInterventionDecisionRequest Request()=>new(){ExpectedSourceRevision=new string('A',64),FieldId="field",Decision=" approved ",Note="Reviewed evidence",CreateWorkOrder=true,WorkOrderNumber="WO-001",Jurisdiction="Kuwait authority"};
    private static ProductionInterventionDecisionResult Result()=>new(){ReviewedSourceRevision=new string('A',64),Success=true,FieldId="field",WellId="well",Decision="APPROVED",WorkOrderId="order"};
    private static ProductionInterventionCandidateDto Readback(ProductionInterventionDecisionState.Attempt a)=>new(){Company=a.Company.Scope,Actor=a.Company.UserId,FieldId=a.Request.FieldId,WellId=a.Well,SourceRevision=new string('B',64),SourceObservationNumber=1,LatestObservationNumber=2,RetrievedAtUtc=DateTimeOffset.UtcNow,Status=a.Request.Decision!,LatestActivityType=$"WORKOVER_{a.Request.Decision}",LatestRecordedBy=a.Company.UserId,LatestActivityRemark=ProductionInterventionDecisionChecks.RecordedRemark(a.Request.Note,"order",null,null),WorkOrderId="order"};
    [Theory][InlineData("null")][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("well")][InlineData("status")][InlineData("activity")][InlineData("maker")][InlineData("note")][InlineData("order")][InlineData("revision")][InlineData("observation")]
    public void UnconfirmedRecordedActivityCannotReleasePendingDecision(string fault)
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var request=Request();request.CreateWorkOrder=false;request.WorkOrderId="order";var attempt=state.Begin("well",request);var recorded=Readback(attempt);
        switch(fault)
        {
            case "null":recorded=null!;break;case "company":recorded.Company=new("tenant","foreign");break;case "actor":recorded.Actor="foreign";break;
            case "field":recorded.FieldId="foreign";break;case "well":recorded.WellId="foreign";break;case "status":recorded.Status="DEFERRED";break;
            case "activity":recorded.LatestActivityType="WORKOVER_REVIEWED";break;case "maker":recorded.LatestRecordedBy="other";break;
            case "note":recorded.LatestActivityRemark="Changed rationale WORK_ORDER_ID:order";break;case "order":recorded.WorkOrderId="other";break;
            case "revision":recorded.SourceRevision=attempt.Request.ExpectedSourceRevision;break;default:recorded.LatestObservationNumber=recorded.SourceObservationNumber;break;
        }
        Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,Result(),recorded));Assert.True(state.IsBlocked("field","well"));
    }
    [Theory][InlineData("order")][InlineData("afe")][InlineData("afe-number")]
    public void UnrequestedResponseHandoffCannotNavigateOrReleaseIntent(string handoff)
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var request=Request();request.CreateWorkOrder=false;var attempt=state.Begin("well",request);var result=Result();result.WorkOrderId=null;
        if(handoff=="order")result.WorkOrderId="unexpected";if(handoff=="afe")result.AfeId="unexpected";if(handoff=="afe-number")result.AfeNumber="unexpected";
        Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,result,Readback(attempt)));Assert.True(state.IsBlocked("field","well"));
    }
    [Theory][InlineData("APPROVED")][InlineData("DEFERRED")][InlineData("REJECTED")]
    public void ExactRecordedDecisionWithoutHandoffConfirmsOnlyItsActivity(string decision)
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var request=new ProductionInterventionDecisionRequest{FieldId="field",ExpectedSourceRevision=new string('A',64),Decision=decision,Note="Recorded rationale"};var attempt=state.Begin("well",request);
        var result=Result();result.Decision=decision;result.WorkOrderId=null;
        var recorded=Readback(attempt);recorded.WorkOrderId=null;recorded.LatestActivityRemark="Recorded rationale";
        state.Confirm(attempt,result,recorded);Assert.False(state.IsBlocked("field","well"));Assert.Null(result.WorkOrderId);
    }
    [Fact] public void ExactExplicitExistingOrderReadbackConfirms()
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var request=Request();request.CreateWorkOrder=false;request.WorkOrderId="order";var attempt=state.Begin("well",request);
        state.Confirm(attempt,Result(),Readback(attempt));Assert.False(state.IsBlocked("field","well"));
    }
    [Theory][InlineData("note")][InlineData("order")][InlineData("afe")]
    public void AmbiguousNewMarkerIntentIsRefusedBeforeItBecomesPending(string fault)
    {
        using var company=Company();var state=new ProductionInterventionDecisionState(company);var request=Request();
        if(fault=="note")request.Note="Review WORK_ORDER_ID:unexpected";if(fault=="order")request.WorkOrderId="order with spaces";if(fault=="afe")request.AfeId="afe with spaces";
        Assert.Throws<ArgumentException>(()=>state.Begin("well",request));Assert.False(state.IsBlocked("field","well"));
    }
    [Fact] public void FrozenDecisionKeepsOriginalValues(){using var c=Company();var s=new ProductionInterventionDecisionState(c);var request=Request();var a=s.Begin("well",request);request.WorkOrderNumber="other";request.FieldId="other";request.Note="Other";request.ExpectedSourceRevision=new string('B',64);Assert.Equal(new string('A',64),a.Request.ExpectedSourceRevision);Assert.Equal("WO-001",a.Request.WorkOrderNumber);Assert.Equal("field",a.Request.FieldId);Assert.Equal("Reviewed evidence",a.Request.Note);Assert.Equal("APPROVED",a.Request.Decision);}
    [Fact] public void UnknownOutcomeBlocksAcrossCompanyAwayAndBack(){using var c=Company();var s=new ProductionInterventionDecisionState(c);var a=s.Begin("well",Request());Select(c,"B");Assert.False(s.IsCurrent(a,"field"));Select(c,"A");Assert.True(s.IsBlocked("field","well"));Assert.Throws<InvalidOperationException>(()=>s.Begin("well",Request()));}
    [Fact] public void ChangedFieldCannotFollowUp(){using var c=Company();var s=new ProductionInterventionDecisionState(c);var a=s.Begin("well",Request());Assert.False(s.IsCurrent(a,"other"));Assert.True(s.IsBlocked("field","well"));Assert.False(s.IsBlocked("other","well"));}
    [Theory][InlineData("null")][InlineData("failure")][InlineData("field")][InlineData("well")][InlineData("decision")][InlineData("order")][InlineData("revision")]
    public void WrongResultCannotConfirm(string fault){using var c=Company();var s=new ProductionInterventionDecisionState(c);var a=s.Begin("well",Request());var result=Result();switch(fault){case "null":result=null!;break;case "revision":result.ReviewedSourceRevision=new string('B',64);break;case "failure":result.Success=false;break;case "field":result.FieldId="other";break;case "well":result.WellId="other";break;case "decision":result.Decision="REJECTED";break;default:result.WorkOrderId=null;break;}Assert.Throws<InvalidOperationException>(()=>s.Confirm(a,result,Readback(a)));Assert.True(s.IsBlocked("field","well"));}
    [Fact] public void ExactResponseConfirms(){using var c=Company();var s=new ProductionInterventionDecisionState(c);var a=s.Begin("well",Request());s.Confirm(a,Result(),Readback(a));Assert.False(s.IsBlocked("field","well"));}
    [Fact] public void ExistingOrderMustMatchReturnedIdentity(){using var c=Company();var s=new ProductionInterventionDecisionState(c);var request=Request();request.WorkOrderId="existing";var a=s.Begin("well",request);Assert.Throws<InvalidOperationException>(()=>s.Confirm(a,Result(),Readback(a)));Assert.True(s.IsBlocked("field","well"));}
    [Theory][InlineData("REVIEWED")][InlineData("")][InlineData("APPROVE")]
    public void UnknownDecisionIsRefused(string decision){var request=Request();request.Decision=decision;Assert.Throws<ArgumentException>(()=>ProductionInterventionDecisionChecks.Require(request,"field"));}
    [Theory][InlineData("field")][InlineData("note")][InlineData("jurisdiction")][InlineData("negative")][InlineData("nonfinite")][InlineData("deferralhandoff")][InlineData("number")]
    public void MissingOrInvalidIntentIsRefused(string fault){using var c=Company();var s=new ProductionInterventionDecisionState(c);var request=Request();switch(fault){case "number":request.WorkOrderNumber="";break;case "field":request.FieldId="";break;case "note":request.Note=" ";break;case "jurisdiction":request.Jurisdiction="";break;case "negative":request.EstimatedCostUsd=-1;break;case "nonfinite":request.EstimatedCostUsd=double.NaN;break;default:request.Decision="DEFERRED";break;}Assert.ThrowsAny<Exception>(()=>s.Begin("well",request));Assert.False(s.IsBlocked("field","well"));}
    [Fact] public void CurrentFieldCannotReplaceOriginalIntent()=>Assert.Throws<InvalidOperationException>(()=>ProductionInterventionDecisionChecks.Require(Request(),"other"));
    [Theory][InlineData("DEFERRED")][InlineData("REJECTED")]
    public void NonApprovalDoesNotCreateFinancialOrMaintenanceActions(string decision){var request=new ProductionInterventionDecisionRequest{ExpectedSourceRevision=new string('A',64),FieldId="field",Decision=decision,Note="Reviewed"};Assert.Equal(decision,ProductionInterventionDecisionChecks.Require(request,"field"));Assert.False(request.CreateWorkOrder);Assert.False(request.LinkAfe);}
    [Fact] public void ApprovalAloneDoesNotRequireOrCreateMaintenanceHeader(){var request=new ProductionInterventionDecisionRequest{FieldId="field",ExpectedSourceRevision=new string('A',64),Decision="APPROVED",Note="Reviewed"};Assert.Equal("APPROVED",ProductionInterventionDecisionChecks.Require(request,"field"));Assert.False(request.CreateWorkOrder);Assert.Null(request.WorkOrderNumber);}
    [Fact] public void NoCompanyCannotBegin(){using var c=new CompanyClientContext();var s=new ProductionInterventionDecisionState(c);Assert.Throws<InvalidOperationException>(()=>s.Begin("well",Request()));}
}
