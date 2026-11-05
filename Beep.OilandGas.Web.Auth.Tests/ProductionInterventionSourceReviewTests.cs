using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProductionInterventionSourceReviewTests
{
    [Fact] public void RecordedReferenceReaderPreservesExplicitInternationalAndCompleteIdentities()
    {
        var links=ProductionInterventionMarkers.Read("Recorded rationale WORK_ORDER_ID:order/a?b AFE_ID:afe:42 AFE_NUMBER:شركة-42");
        Assert.Equal("order/a?b",links["WORK_ORDER_ID"]);Assert.Equal("afe:42",links["AFE_ID"]);Assert.Equal("شركة-42",links["AFE_NUMBER"]);
        Assert.Equal("Recorded rationale",ProductionInterventionSourceReviews.RemoveWorkflowMarkers("Recorded rationale WORK_ORDER_ID:order/a?b AFE_ID:afe:42 AFE_NUMBER:شركة-42"));
    }
    [Theory][InlineData("WORK_ORDER_ID:")][InlineData("WORK_ORDER_ID: ")][InlineData("WORK_ORDER_ID:one WORK_ORDER_ID:two")][InlineData("WORK_ORDER_ID:one WORK_ORDER_ID:one")][InlineData("AFE_ID:one AFE_ID:two")]
    public void EmptyOrDuplicateRecordedReferencesCannotSelectFirstHandoff(string remark)
    {
        Assert.Throws<InvalidOperationException>(()=>ProductionInterventionMarkers.Read(remark));
        Assert.Throws<InvalidOperationException>(()=>ProductionInterventionSourceReviews.RemoveWorkflowMarkers(remark));
    }
    [Theory][InlineData("MY_WORK_ORDER_ID:other")][InlineData("prefixAFE_ID:other")]
    public void MarkerLikeTextInsideWordRemainsRationale(string note)
    {
        Assert.Empty(ProductionInterventionMarkers.Read(note));Assert.Equal(note,ProductionInterventionSourceReviews.RemoveWorkflowMarkers(note));ProductionInterventionMarkers.RequireRationale(note);
    }
    [Theory][InlineData("WORK_ORDER_ID")][InlineData("AFE_ID")][InlineData("AFE_NUMBER")][InlineData("ABANDONMENT_ID")][InlineData("PROCESS_INSTANCE_ID")][InlineData("SOURCE_PROCESS")]
    public void RationaleCannotInjectRecordedWorkflowReference(string key)
    {
        var note=$"Recorded rationale {key}:foreign";Assert.Throws<ArgumentException>(()=>ProductionInterventionDecisionChecks.RecordedRemark(note,"order",null,null));
        var rows=new[]{Activity()};var request=Decision(rows);request.Note=note;Assert.Throws<ArgumentException>(()=>ProductionInterventionSourceReviews.RequireDecision(rows,request,new("tenant","company"),"actor","field","well"));
    }
    [Fact] public void ExplicitNoteAndReferencesUseOneRoundTripFormat()
    {
        var remark=ProductionInterventionDecisionChecks.RecordedRemark("  Recorded rationale  ","order/a?b","afe-42","AFE-42");
        var links=ProductionInterventionMarkers.Read(remark);Assert.Equal("order/a?b",links["WORK_ORDER_ID"]);Assert.Equal("Recorded rationale",ProductionInterventionSourceReviews.RemoveWorkflowMarkers(remark));
    }
    private static WELL_ACTIVITY Activity(decimal observation=1)=>new(){UWI="well",ACTIVITY_OBS_NO=observation,ACTIVITY_TYPE_ID="WORKOVER",REMARK="Recorded issue",SOURCE="Recorded source",ACTIVE_IND="Y",ROW_CREATED_BY="preparer",START_DATE=new(2026,10,6)};
    private static string Revision(params WELL_ACTIVITY[] rows)=>ProductionInterventionSourceReviews.Revision(rows,new("tenant","company"),"actor","field","well");
    [Fact] public void IndependentlyMaterializedSameRecordsHaveSameRevision()=>Assert.Equal(Revision(Activity()),Revision(Activity()));
    [Fact] public void ProviderOrderingDoesNotChangeReview(){var a=Activity();var b=Activity(2);Assert.Equal(Revision(a,b),Revision(b,a));}
    [Theory][InlineData("note")][InlineData("start")][InlineData("type")][InlineData("source")][InlineData("creator")][InlineData("active")][InlineData("observation")]
    public void EditedOriginalObservationInvalidatesReview(string field){var row=Activity();var original=Revision(row);switch(field){case "note":row.REMARK="Changed";break;case "start":row.START_DATE=new(2026,10,7);break;case "type":row.ACTIVITY_TYPE_ID="CHANGED";break;case "source":row.SOURCE="other";break;case "creator":row.ROW_CREATED_BY="other";break;case "active":row.ACTIVE_IND="N";break;default:row.ACTIVITY_OBS_NO=2;break;}Assert.Throws<InvalidOperationException>(()=>ProductionInterventionDecisionChecks.RequireRevision(original,Revision(row)));}
    [Fact] public void NewDecisionActivityInvalidatesOriginalReview(){var row=Activity();var original=Revision(row);var decision=Activity(2);decision.ACTIVITY_TYPE_ID="WORKOVER_APPROVED";Assert.Throws<InvalidOperationException>(()=>ProductionInterventionDecisionChecks.RequireRevision(original,Revision(row,decision)));}
    [Theory][InlineData("company")][InlineData("tenant")][InlineData("actor")][InlineData("field")]
    public void DigestIsBoundToCompanyAccountAndField(string kind){var row=Activity();var original=Revision(row);var changed=ProductionInterventionSourceReviews.Revision([row],new(kind=="tenant"?"other":"tenant",kind=="company"?"other":"company"),kind=="actor"?"other":"actor",kind=="field"?"other":"field","well");Assert.NotEqual(original,changed);}
    [Theory][InlineData("missing")][InlineData("empty")][InlineData("duplicate")][InlineData("well")][InlineData("nullrow")]
    public void UnavailableOrAmbiguousHistoryCannotProduceRevision(string fault){WELL_ACTIVITY[]? rows=[Activity()];switch(fault){case "missing":rows=null;break;case "empty":rows=[];break;case "duplicate":rows=[Activity(),Activity()];break;case "well":rows[0].UWI="other";break;default:rows=[null!];break;}Assert.Throws<InvalidOperationException>(()=>ProductionInterventionSourceReviews.Revision(rows,new("tenant","company"),"actor","field","well"));}
    [Theory][InlineData("")][InlineData("wrong")][InlineData("GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG")]
    public void MissingOrMalformedRequestReviewIsRefused(string expected){var request=new ProductionInterventionDecisionRequest{ExpectedSourceRevision=expected,FieldId="field",Decision="DEFERRED",Note="Reviewed"};Assert.Throws<ArgumentException>(()=>ProductionInterventionDecisionChecks.Require(request,"field"));}
    [Fact] public void ExactOriginalReviewMatches(){var revision=Revision(Activity());ProductionInterventionDecisionChecks.RequireRevision(revision,revision);}
    private static ProductionInterventionDecisionRequest Decision(WELL_ACTIVITY[] rows,string actor="actor",string decision="APPROVED")=>new(){FieldId="field",ExpectedSourceRevision=ProductionInterventionSourceReviews.Revision(rows,new("tenant","company"),actor,"field","well"),Decision=decision,Note="Reviewed",InterventionType="WORKOVER",Problem="Recorded issue"};
    [Fact] public void MatchingRecordedContentAllowsIndependentReviewer(){var rows=new[]{Activity()};ProductionInterventionSourceReviews.RequireDecision(rows,Decision(rows),new("tenant","company"),"actor","field","well");}
    [Theory][InlineData("type")][InlineData("problem")]
    public void TamperedSubmittedSourceContentIsRefusedEvenWithOriginalRevision(string field){var rows=new[]{Activity()};var request=Decision(rows);if(field=="type")request.InterventionType="Injected type";else request.Problem="Injected problem";Assert.Throws<InvalidOperationException>(()=>ProductionInterventionSourceReviews.RequireDecision(rows,request,new("tenant","company"),"actor","field","well"));}
    [Theory][InlineData("preparer")][InlineData("Administrator")]
    public void SourceMakerCannotApproveTheirOwnActivity(string actor){var row=Activity();row.ROW_CREATED_BY=actor;var rows=new[]{row};Assert.Throws<UnauthorizedAccessException>(()=>ProductionInterventionSourceReviews.RequireDecision(rows,Decision(rows,actor),new("tenant","company"),actor,"field","well"));}
    [Theory][InlineData(null)][InlineData("")][InlineData(" ")]
    public void UnknownSourceMakerCannotBeApproved(string? maker){var row=Activity();row.ROW_CREATED_BY=maker;var rows=new[]{row};Assert.Throws<UnauthorizedAccessException>(()=>ProductionInterventionSourceReviews.RequireDecision(rows,Decision(rows),new("tenant","company"),"actor","field","well"));}
    [Theory][InlineData("DEFERRED")][InlineData("REJECTED")]
    public void SourceMakerMayRecordNonApprovalDecision(string decision){var row=Activity();var rows=new[]{row};ProductionInterventionSourceReviews.RequireDecision(rows,Decision(rows,"preparer",decision),new("tenant","company"),"preparer","field","well");}
    [Fact] public void LatestDecisionCannotReplaceOriginalMakerAndContent(){var original=Activity();var latest=Activity(2);latest.ACTIVITY_TYPE_ID="WORKOVER_DEFERRED";latest.ROW_CREATED_BY="actor";latest.REMARK="Different decision note";var rows=new[]{original,latest};Assert.Same(original,ProductionInterventionSourceReviews.OriginalSource(rows,"well"));ProductionInterventionSourceReviews.RequireDecision(rows,Decision(rows),new("tenant","company"),"actor","field","well");}
    [Fact] public void DecisionOnlyHistoryHasNoInventedOriginalSource(){var row=Activity();row.ACTIVITY_TYPE_ID="WORKOVER_APPROVED";Assert.Throws<InvalidOperationException>(()=>ProductionInterventionSourceReviews.OriginalSource([row],"well"));}
    [Fact] public void WorkflowMarkersDoNotBecomeSubmittedEngineeringNotes(){var row=Activity();row.REMARK="Recorded issue WORK_ORDER_ID:order AFE_ID:afe";var rows=new[]{row};ProductionInterventionSourceReviews.RequireDecision(rows,Decision(rows),new("tenant","company"),"actor","field","well");Assert.Equal("Recorded issue",ProductionInterventionSourceReviews.RemoveWorkflowMarkers(row.REMARK));}

}
