using Beep.OilandGas.Models.Data.WorkOrder;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderWorkflowTests
{
    private static InspectionCondition Passed(int sequence=1,string type="CLOSE_OUT")=>new(sequence,type,"Recorded inspection","PASS","recorded result",new DateTime(2026,10,6),null);
    [Theory][InlineData("DRAFT","WO-CORRECTIVE","PLANNED")][InlineData("SCOPED","WO-CORRECTIVE","PLANNED")][InlineData("PLANNED","WO-CORRECTIVE","IN_PROGRESS")][InlineData("IN_PROGRESS","WO-CORRECTIVE","COMPLETED")][InlineData("IN_PROGRESS","WO-SAFETY","UNDER_REVIEW")][InlineData("UNDER_REVIEW","WO-SAFETY","COMPLETED")]
    public void RecordedGraphOffersItsExistingDestinations(string from,string subtype,string to)=>Assert.Contains(to,WorkOrderWorkflow.Available(from,subtype));
    [Theory][InlineData("DRAFT","WO-SAFETY","COMPLETED")][InlineData("PLANNED","WO-CORRECTIVE","COMPLETED")][InlineData("IN_PROGRESS","WO-SAFETY","COMPLETED")][InlineData("UNDER_REVIEW","WO-CORRECTIVE","COMPLETED")][InlineData("COMPLETED","WO-CORRECTIVE","PLANNED")][InlineData("CANCELLED","WO-CORRECTIVE","PLANNED")][InlineData("UNKNOWN","WO-CORRECTIVE","PLANNED")][InlineData("DRAFT","UNKNOWN","PLANNED")]
    public void SkippedUnknownOrTerminalTransitionsAreNotOffered(string from,string subtype,string to)=>Assert.DoesNotContain(to,WorkOrderWorkflow.Available(from,subtype));
    [Fact] public void RecordedDatedPassConfirmsCondition(){Assert.True(WorkOrderWorkflow.RecordedConditionsPassed([Passed()],[ConditionType.CloseOut]));Assert.True(WorkOrderWorkflow.CompletionEvidence([Passed()]));}
    [Theory][InlineData("empty")][InlineData("missingtype")][InlineData("pending")][InlineData("fail")][InlineData("na")][InlineData("undated")][InlineData("defaultdate")][InlineData("duplicate")][InlineData("sequence")][InlineData("text")][InlineData("type")][InlineData("nullrow")][InlineData("null")]
    public void AbsentMalformedOrUnpassedEvidenceCannotConfirmCompletion(string fault)
    {
        var row=Passed();IReadOnlyList<InspectionCondition>? rows=fault switch{
            "empty"=>[],"missingtype"=>[row with{CondType=ConditionType.Operational}],"pending"=>[row with{Status="PENDING"}],"fail"=>[row with{Status="FAIL"}],"na"=>[row with{Status="NA"}],
            "undated"=>[row with{InspectDate=null}],"defaultdate"=>[row with{InspectDate=default(DateTime)}],"duplicate"=>[row,row],"sequence"=>[row with{CondSeq=0}],"text"=>[row with{RequiredText=""}],"type"=>[row with{CondType=""}],"nullrow"=>[null!],_=>null};
        Assert.False(WorkOrderWorkflow.RecordedConditionsPassed(rows,[ConditionType.CloseOut]));Assert.False(WorkOrderWorkflow.CompletionEvidence(rows));
    }
    [Fact] public void EachRequestedTypeNeedsItsOwnRecordedEvidence(){Assert.False(WorkOrderWorkflow.RecordedConditionsPassed([Passed()],[ConditionType.CloseOut,ConditionType.SceVerify]));Assert.True(WorkOrderWorkflow.RecordedConditionsPassed([Passed(),Passed(2,ConditionType.SceVerify)],[ConditionType.CloseOut,ConditionType.SceVerify]));}
    [Fact] public void CompletionChecksAllRecordedTypes(){Assert.False(WorkOrderWorkflow.CompletionEvidence([Passed(),Passed(2,ConditionType.SceVerify) with{Status="FAIL"}]));}
    [Fact] public void NoCloseOutCannotConfirmCompletion()=>Assert.False(WorkOrderWorkflow.CompletionEvidence([Passed(1,ConditionType.Operational)]));
    [Fact] public void EmptyOrDuplicateRequiredTypesCannotMeanAllPassed(){Assert.False(WorkOrderWorkflow.RecordedConditionsPassed([Passed()],[]));Assert.False(WorkOrderWorkflow.RecordedConditionsPassed([Passed()],["CLOSE_OUT","CLOSE_OUT"]));Assert.False(WorkOrderWorkflow.RecordedConditionsPassed([Passed()],null));}
}
