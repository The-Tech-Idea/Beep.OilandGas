using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderInspectionCommandTests
{
    [Fact]
    public async Task ExplicitApplicabilityIsRetainedBeforeTransportAndCannotBeChangedByReadback()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"technician",new(new("tenant","company"),["ConfiguredRole"],[])));using var state=new WorkOrderInspectionState(company);await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>()));await state.ReviewWriteAccessAsync((_,_,_)=>Task.FromResult(true));
        var request=new AddWorkOrderInspectionRequirementRequest{FieldId="field",CondSeq=1,CondType=ConditionType.Regulatory,RequiredText="Recorded regulatory condition",Jurisdiction="Reported authority",ApplicableStage="RETURN_TO_SERVICE"};var attempt=state.BeginRequirement(request);request.ApplicableStage="PRE_START";Assert.Equal("RETURN_TO_SERVICE",attempt.Requirement!.ApplicableStage);
        var saved=WorkOrderInspectionRules.Project(WorkOrderInspectionRules.PrepareRequirement(attempt.Requirement,"order","technician","inspection",DateTime.UtcNow));var altered=saved with{ApplicableStage="PRE_START"};Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,altered,altered));Assert.True(state.Blocked);state.Confirm(attempt,saved,saved);Assert.False(state.Blocked);
    }
    [Theory][InlineData("PRE_START","PRE_START",null,true)][InlineData("OPERATIONAL","WORK_SUBMISSION",null,true)][InlineData("CLOSE_OUT","COMPLETION_VERIFICATION",null,true)][InlineData("REGULATORY","RETURN_TO_SERVICE",null,true)][InlineData("SCE_VERIFY","COMPLETION_VERIFICATION",null,true)][InlineData("SCE_VERIFY","PRE_START","ISOLATION",true)][InlineData("OPERATIONAL","PRE_START",null,false)][InlineData("CLOSE_OUT","WORK_SUBMISSION",null,false)][InlineData("SCE_VERIFY","RETURN_TO_SERVICE","ISOLATION",false)][InlineData("REGULATORY","UNKNOWN",null,false)]
    public void ExplicitApplicabilityMustMatchConditionAndPrerequisiteAndSurvivesRecording(string type,string stage,string? prerequisite,bool valid)
    {
        var request=new AddWorkOrderInspectionRequirementRequest{FieldId="field",CondSeq=1,CondType=type,RequiredText="Recorded phase requirement",Jurisdiction="Reported authority",RegulatoryRef="reference",PermitPrerequisiteKind=prerequisite,ApplicableStage=stage};
        if(!valid){Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionRules.PrepareRequirement(request,"order","planner","inspection",DateTime.UtcNow));return;}
        var row=WorkOrderInspectionRules.PrepareRequirement(request,"order","planner","inspection",DateTime.UtcNow);Assert.Equal(stage,row.APPLICABLE_STAGE);Assert.Equal(stage,WorkOrderInspectionRules.Project(row).ApplicableStage);
        var immutable=WorkOrderInspectionRules.RequirementRevision(row);var source=WorkOrderInspectionRules.Revision(row);var legacy=TheTechIdea.Data.OilGas.CompanyInventorySourceReview.Copy(row);legacy.APPLICABLE_STAGE=null;
        Assert.NotEqual(immutable,WorkOrderInspectionRules.RequirementRevision(legacy));Assert.NotEqual(source,WorkOrderInspectionRules.Revision(legacy));Assert.DoesNotContain("APPLICABLE_STAGE",System.Text.Json.JsonSerializer.Serialize(legacy));Assert.DoesNotContain("ApplicableStage",System.Text.Json.JsonSerializer.Serialize(WorkOrderInspectionRules.Project(legacy)));
        var recorded=WorkOrderInspectionRules.PrepareResult(row,ResultRequest(row),"technician",DateTime.UtcNow);Assert.Equal(stage,recorded.APPLICABLE_STAGE);Assert.Equal(immutable,WorkOrderInspectionRules.RequirementRevision(recorded));WorkOrderInspectionRules.RequireReadback(recorded,recorded);
        var changed=TheTechIdea.Data.OilGas.CompanyInventorySourceReview.Copy(recorded);changed.APPLICABLE_STAGE=null;Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionRules.RequireReadback(recorded,changed));
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ExplicitPrerequisiteCommandsNeedExistingAppPermissionAndFreezeTheirKind(bool permitted)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"technician",new(new("tenant","company"),["ConfiguredRole"],permitted?["HSE.ApprovePermit"]:[])));using var state=new WorkOrderInspectionState(company);await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>()));await state.ReviewWriteAccessAsync((_,_,_)=>Task.FromResult(true));
        var request=new AddWorkOrderInspectionRequirementRequest{FieldId="field",CondSeq=1,CondType=ConditionType.PreStart,RequiredText="Explicit recorded prerequisite",Jurisdiction="Reported authority",RegulatoryRef="source-ref",PermitPrerequisiteKind="ISOLATION"};
        if(!permitted){Assert.Throws<UnauthorizedAccessException>(()=>state.BeginRequirement(request));Assert.False(state.Blocked);return;}
        var attempt=state.BeginRequirement(request);request.PermitPrerequisiteKind="RISK";Assert.Equal("ISOLATION",attempt.Requirement!.PermitPrerequisiteKind);var saved=WorkOrderInspectionRules.Project(WorkOrderInspectionRules.PrepareRequirement(attempt.Requirement,"order","technician","inspection",DateTime.UtcNow));Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,saved with{PermitPrerequisiteKind="RISK"},saved with{PermitPrerequisiteKind="RISK"}));state.Confirm(attempt,saved,saved);Assert.False(state.Blocked);
    }
    [Theory][InlineData(true,true)][InlineData(true,false)][InlineData(false,true)]
    public async Task Inspection_edits_need_both_original_field_and_order_write_access(bool field,bool order)
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>()));
        Assert.False(state.CanEdit);var calls=new List<string>();
        await state.ReviewWriteAccessAsync((actor,id,type)=>{Assert.Equal("technician",actor);calls.Add(type);Assert.Equal(type=="FIELD"?"field":"order",id);return Task.FromResult(type=="FIELD"?field:order);});
        Assert.Equal(field&&order,state.CanEdit);Assert.Equal(field?new[]{"FIELD","WORK_ORDER"}:new[]{"FIELD"},calls);
        if(!state.CanEdit)Assert.Throws<InvalidOperationException>(()=>state.BeginRequirement(new(){FieldId="field"}));
    }
    [Fact] public async Task Late_access_response_cannot_enable_edits_after_company_switch()
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>()));
        var response=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var review=state.ReviewWriteAccessAsync((_,_,_)=>response.Task);
        Select(company,"B");response.SetResult(true);await review;Assert.False(state.CanEdit);Assert.False(state.WriteAccessConfirmed);
    }
    [Fact] public async Task Older_positive_access_response_cannot_override_newer_denial()
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>()));
        var response=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var old=state.ReviewWriteAccessAsync((_,_,_)=>response.Task);
        await state.ReviewWriteAccessAsync((_,_,_)=>Task.FromResult(false));response.SetResult(true);await old;
        Assert.False(state.CanEdit);Assert.False(state.ReviewingWriteAccess);Assert.NotNull(state.WriteAccessError);
    }
    [Fact] public async Task Failed_access_review_preserves_read_evidence_and_refuses_edits()
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);var row=WorkOrderInspectionRules.Project(Row());
        await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>{row}));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewWriteAccessAsync((_,_,_)=>throw new InvalidOperationException("Unavailable")));
        Assert.True(state.Confirmed);Assert.Same(row,Assert.Single(state.Rows));Assert.False(state.CanEdit);Assert.NotNull(state.WriteAccessError);
    }
    [Fact] public async Task ProgressKeepsFailureAndNotApplicableSeparateFromPasses()
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);
        var rows=new List<InspectionCondition>();
        foreach(var pair in new[]{(ConditionType.PreStart,"UNRECORDED"),(ConditionType.PreStart,"FAIL"),(ConditionType.CloseOut,"PASS"),(ConditionType.CloseOut,"NA")})
        {
            var row=Row();row.COND_SEQ=rows.Count+1;row.INSPECTION_ID=$"inspection-{row.COND_SEQ}";row.COND_TYPE=pair.Item1;
            if(pair.Item2!="UNRECORDED"){var request=ResultRequest(row);request.Result=pair.Item2;row=WorkOrderInspectionRules.PrepareResult(row,request,"technician",DateTime.UtcNow);}
            rows.Add(WorkOrderInspectionRules.Project(row));
        }
        await state.LoadAsync("field","order",()=>Task.FromResult(rows));
        var pre=Assert.Single(state.Progress.Where(row=>row.Category==ConditionType.PreStart));
        Assert.Equal(new WorkOrderInspectionState.CategoryProgress(ConditionType.PreStart,2,1,0,1,0),pre);
        Assert.Equal(new WorkOrderInspectionState.CategoryProgress(ConditionType.CloseOut,2,0,1,0,1),Assert.Single(state.Progress.Where(row=>row.Category==ConditionType.CloseOut)));
        Assert.False(state.CanEdit);Select(company,"B");Assert.Empty(state.Progress);
    }
    [Fact] public async Task EmptyChecklistDoesNotInventRequiredCategoryCoverage()
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);
        await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>()));Assert.Empty(state.Progress);
    }
    [Fact] public async Task FailedReloadClearsEarlierProgress()
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);
        await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>{WorkOrderInspectionRules.Project(Row())}));Assert.Single(state.Progress);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync("field","order",()=>Task.FromException<List<InspectionCondition>>(new InvalidOperationException("Unavailable"))));
        Assert.Empty(state.Progress);Assert.False(state.Confirmed);
    }
    private static WORK_ORDER_INSPECTION Row()=>WorkOrderInspectionRules.PrepareRequirement(new(){FieldId="field",CondSeq=1,CondType=ConditionType.PreStart,RequiredText="Actual isolation requirement",Jurisdiction="Recorded authority"},"order","planner","inspection",DateTime.UtcNow);
    private static RecordInspectionResultRequest ResultRequest(WORK_ORDER_INSPECTION row)=>new(){FieldId=row.FIELD_ID,InspectionId=row.INSPECTION_ID,ExpectedSourceRevision=WorkOrderInspectionRules.Revision(row),Result="PASS",Notes="Actual isolation verified"};
    private static CompanyClientContext Company(){var c=new CompanyClientContext();Select(c,"A");return c;}
    private static void Select(CompanyClientContext c,string id)=>Assert.True(c.TrySelect(c.Clear(),"technician",new(new("tenant",id),[],[])));
    [Fact] public void RecordingKeepsRequirementAndOriginalRowUntouched(){var row=Row();var recorded=WorkOrderInspectionRules.PrepareResult(row,ResultRequest(row),"technician",DateTime.UtcNow);Assert.Equal("",row.RESULT_STATUS);Assert.Equal(row.REQUIRED_TEXT,recorded.REQUIRED_TEXT);Assert.Equal("planner",recorded.REQUIREMENT_ACTOR);Assert.Equal("technician",recorded.INSPECTOR);Assert.Equal("PASS",recorded.RESULT_STATUS);Assert.NotEqual(WorkOrderInspectionRules.Revision(row),WorkOrderInspectionRules.Revision(recorded));WorkOrderInspectionRules.RequireReadback(recorded,recorded);}
    [Theory][InlineData("revision")][InlineData("field")][InlineData("identity")][InlineData("result")][InlineData("notes")][InlineData("actor")][InlineData("zone")][InlineData("recorded")]
    public void UnreviewedOrRepeatedResultRefuses(string fault){var row=Row();var request=ResultRequest(row);var actor="technician";var utc=DateTime.UtcNow;switch(fault){case "revision":request.ExpectedSourceRevision=new string('0',64);break;case "field":request.FieldId="other";break;case "identity":request.InspectionId="other";break;case "result":request.Result="";break;case "notes":request.Notes="";break;case "actor":actor="";break;case "zone":utc=DateTime.SpecifyKind(utc,DateTimeKind.Unspecified);break;default:row=WorkOrderInspectionRules.PrepareResult(row,request,actor,utc);request=ResultRequest(row);break;}Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionRules.PrepareResult(row,request,actor,utc));}
    [Theory][InlineData("text")][InlineData("type")][InlineData("jurisdiction")][InlineData("maker")][InlineData("result")][InlineData("notes")][InlineData("inspector")][InlineData("date")][InlineData("null")]
    public void ChangedSavedEvidenceCannotConfirmResult(string fault){var source=Row();var intended=WorkOrderInspectionRules.PrepareResult(source,ResultRequest(source),"technician",DateTime.UtcNow);var saved=WorkOrderInspectionRules.PrepareResult(source,ResultRequest(source),"technician",intended.INSPECTED_AT_UTC!.Value);switch(fault){case "text":saved.REQUIRED_TEXT="other";break;case "type":saved.COND_TYPE=ConditionType.CloseOut;break;case "jurisdiction":saved.JURISDICTION="other";break;case "maker":saved.REQUIREMENT_ACTOR="other";break;case "result":saved.RESULT_STATUS="FAIL";break;case "notes":saved.RESULT_NOTES="other";break;case "inspector":saved.INSPECTOR="other";break;case "date":saved.INSPECTED_AT_UTC=intended.INSPECTED_AT_UTC.Value.AddMinutes(1);break;}Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionRules.RequireReadback(intended,fault=="null"?null:saved));}
    [Fact] public async Task UnknownResultStaysBlockedAcrossCompanyAwayAndBack(){using var c=Company();using var state=new WorkOrderInspectionState(c);var source=WorkOrderInspectionRules.Project(Row());await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>{source}));await state.ReviewWriteAccessAsync((_,_,_)=>Task.FromResult(true));var attempt=state.BeginResult(source,"PASS","Recorded evidence");Select(c,"B");Assert.False(state.IsCurrent(attempt));Select(c,"A");await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>{source}));await state.ReviewWriteAccessAsync((_,_,_)=>Task.FromResult(true));Assert.True(state.Blocked);Assert.Throws<InvalidOperationException>(()=>state.BeginResult(source,"PASS","Again"));}
    [Fact] public async Task LateInspectionReadCannotPublishAfterCompanyChange(){using var c=Company();using var state=new WorkOrderInspectionState(c);var read=new TaskCompletionSource<List<InspectionCondition>>();var load=state.LoadAsync("field","order",()=>read.Task);Select(c,"B");read.SetResult([WorkOrderInspectionRules.Project(Row())]);await load;Assert.False(state.Confirmed);Assert.Empty(state.Rows);}
    [Fact] public async Task ExactResponseAndActualReadbackConfirmFrozenResult(){using var c=Company();using var state=new WorkOrderInspectionState(c);var source=Row();var dto=WorkOrderInspectionRules.Project(source);await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>{dto}));await state.ReviewWriteAccessAsync((_,_,_)=>Task.FromResult(true));var attempt=state.BeginResult(dto,"PASS","Recorded evidence");var recorded=WorkOrderInspectionRules.Project(WorkOrderInspectionRules.PrepareResult(source,attempt.Result!,"technician",DateTime.UtcNow));Assert.True(state.IsCurrent(attempt));Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,recorded,recorded with{ResultText="other"}));Assert.True(state.Blocked);state.Confirm(attempt,recorded,recorded);Assert.False(state.Blocked);}
    [Theory][InlineData("field")][InlineData("order")][InlineData("identity")][InlineData("revision")][InlineData("duplicate")]
    public async Task WrongReviewCannotPublish(string fault){using var c=Company();using var state=new WorkOrderInspectionState(c);var row=WorkOrderInspectionRules.Project(Row());row=fault switch{"field"=>row with{FieldId="other"},"order"=>row with{WorkOrderId="other"},"identity"=>row with{InspectionId=""},"revision"=>row with{SourceRevision=""},_=>row};var rows=fault=="duplicate"?new List<InspectionCondition>{row,row}:new(){row};await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync("field","order",()=>Task.FromResult(rows)));Assert.False(state.Confirmed);Assert.Empty(state.Rows);}

    [Fact] public async Task RequirementIntentStaysFrozenAndNeedsMatchingRecordedReadback()
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>()));await state.ReviewWriteAccessAsync((_,_,_)=>Task.FromResult(true));
        var request=new AddWorkOrderInspectionRequirementRequest{FieldId="field",CondSeq=1,CondType=ConditionType.PreStart,RequiredText="Recorded requirement",Jurisdiction="Authority"};
        var attempt=state.BeginRequirement(request);request.RequiredText="Changed";request.Jurisdiction="Changed";Assert.Equal("Recorded requirement",attempt.Requirement!.RequiredText);Assert.Equal("Authority",attempt.Requirement.Jurisdiction);
        var saved=WorkOrderInspectionRules.Project(WorkOrderInspectionRules.PrepareRequirement(attempt.Requirement,"order","technician","inspection",DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,saved,saved with{RequiredText="Other"}));Assert.True(state.Blocked);state.Confirm(attempt,saved,saved);Assert.False(state.Blocked);
    }
    [Fact] public async Task MissingRationaleNeverStartsAnUncertainResult()
    {
        using var company=Company();using var state=new WorkOrderInspectionState(company);var row=WorkOrderInspectionRules.Project(Row());await state.LoadAsync("field","order",()=>Task.FromResult(new List<InspectionCondition>{row}));await state.ReviewWriteAccessAsync((_,_,_)=>Task.FromResult(true));
        Assert.Throws<InvalidOperationException>(()=>state.BeginResult(row,"PASS"," "));Assert.False(state.Blocked);
    }
}
