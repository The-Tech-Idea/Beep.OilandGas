#if WORKFLOW_INBOX_FOCUSED
extern alias actualOwners;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using Xunit;
using Detail=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkOrderExecutionPermitStateTests
{
    internal static CompanyWorkOrderExecutionPermitApprovalReview PhaseAwareReview()
    {
        var initial=RequirementApprovalReview();var task=initial.Proposal.Task;
        var future=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=2,CondType="OPERATIONAL",RequiredText="Inspect actual work before submission",Jurisdiction=initial.Proposal.Jurisdiction,ApplicableStage="WORK_SUBMISSION"},task.WorkOrderId,"planner","future",DateTime.UtcNow);
        var originalRows=initial.OriginalSource.Inspections.Select(row=>row with{ApplicableStage="PRE_START"}).Concat([actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(future)]).ToArray();
        var currentRows=initial.CurrentSource.Inspections.Select(row=>row with{ApplicableStage="PRE_START"}).Concat([originalRows.Last()]).ToArray();var original=initial.OriginalSource with{Inspections=originalRows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(initial.OriginalSource.Stage,initial.OriginalSource.InspectionDatabase,initial.OriginalSource.Order,originalRows)};var current=initial.CurrentSource with{Inspections=currentRows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(initial.CurrentSource.Stage,initial.CurrentSource.InspectionDatabase,initial.CurrentSource.Order,currentRows)};
        var proposal=initial.Proposal with{RequirementEvidenceVersion=2,InspectionRevision=original.SnapshotRevision};return initial with{Proposal=proposal,OriginalSource=original,CurrentSource=current,PrerequisiteEvidenceVersion=7,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original,current,initial.ComparedAtUtc,7)};
    }
    [Theory][InlineData("complete")][InlineData("missing-stage")][InlineData("wrong-stage")][InlineData("changed-stage")]
    public void PhaseAwareIssueCapturesFutureConditionsWithoutCompletingThemAndPreservesLegacyPolicy(string mode)
    {
        var initial=Review();var task=initial.Source.Stage.Task;var utc=DateTime.UtcNow;
        var pre=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=1,CondType="PRE_START",RequiredText="Before starting",Jurisdiction="Explicit jurisdiction",ApplicableStage="PRE_START"},task.WorkOrderId,"planner","pre",utc);
        pre=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareResult(pre,new(){FieldId=task.FieldId,InspectionId=pre.INSPECTION_ID,ExpectedSourceRevision=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Revision(pre),Result="PASS",Notes="Performed before start"},"independent",utc);
        var future=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=2,CondType="REGULATORY",RequiredText="Verify before return",Jurisdiction="Explicit jurisdiction",ApplicableStage="RETURN_TO_SERVICE"},task.WorkOrderId,"planner","future",utc);
        if(mode=="missing-stage")future.APPLICABLE_STAGE=null;if(mode=="wrong-stage"){future.COND_TYPE="OPERATIONAL";future.APPLICABLE_STAGE="PRE_START";}
        if(mode=="wrong-stage"){Assert.Throws<InvalidOperationException>(()=>actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(future));return;}
        var rows=new[]{actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(pre),actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(future)};
        var source=initial.Source with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(initial.Source.Stage,initial.Source.InspectionDatabase,initial.Source.Order,rows)};var proposal=Proposal(initial with{Source=source,RequirementEvidenceVersion=2}) with{IntentId=Guid.NewGuid(),RequirementEvidenceVersion=2};
        if(mode=="missing-stage"){Assert.Throws<InvalidOperationException>(()=>proposal.RequireSource(source));return;}
        proposal.RequireSource(source);proposal.RequireRecorded(proposal.Prepare(utc,source));Assert.Equal("UNRECORDED",source.Inspections[1].Status);
        var current=source;if(mode=="changed-stage"){var changed=rows.Select(row=>row.InspectionId=="future"?row with{ApplicableStage="COMPLETION_VERIFICATION"}:row).ToArray();current=source with{Inspections=changed,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(source.Stage,source.InspectionDatabase,source.Order,changed)};}
        var blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,source,current,proposal.ValidFromUtc.AddMinutes(1),7);
        Assert.DoesNotContain(blockers,blocker=>blocker.StartsWith("Recorded inspections include unaccepted",StringComparison.Ordinal));
        Assert.Equal(mode=="changed-stage",blockers.Any(blocker=>blocker.StartsWith("Original order",StringComparison.Ordinal)));Assert.Contains(blockers,blocker=>blocker.Contains("material acceptance",StringComparison.Ordinal));
        Assert.Throws<InvalidOperationException>(()=>(proposal with{RequirementEvidenceVersion=1}).RequireSource(source));
    }
    [Theory][InlineData("missing")][InlineData("forged")][InlineData("historical")][InlineData("backdated")][InlineData("local-time")][InlineData("expired")]
    public void ApprovalBoundaryRefusesPartialForgedHistoricalAndUnavailableTimeProof(string mode)
    {
        var review=mode=="historical"?ApprovalReview():RequirementApprovalReview();
        var json=System.Text.Json.JsonSerializer.Serialize(review);
        var utc=review.ComparedAtUtc.AddSeconds(1);
        if(mode=="forged")review=review with{Blockers=[]};
        if(mode=="backdated")utc=review.ComparedAtUtc.AddTicks(-1);
        if(mode=="local-time")utc=DateTime.SpecifyKind(utc,DateTimeKind.Unspecified);
        if(mode=="expired")utc=review.Proposal.ValidToUtc.AddSeconds(1);
        Assert.Throws<InvalidOperationException>(()=>review.RequireCurrentPrerequisiteAcceptance(utc));
        if(mode!="forged")Assert.Equal(json,System.Text.Json.JsonSerializer.Serialize(review));
    }
    [Theory][InlineData("current")][InlineData("missing")][InlineData("unsupported")][InlineData("legacy")]
    public void VersionedPermitIssueRequiresImmutableNativeRequirementProofAndPreservesLegacyJson(string mode)
    {
        var initial=Review();var pending=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=initial.Source.Stage.Task.FieldId,CondSeq=1,CondType="PRE_START",RequiredText="Recorded original requirement",Jurisdiction="Explicit jurisdiction",RegulatoryRef="isolation",PermitPrerequisiteKind="ISOLATION"},initial.Source.Stage.Task.WorkOrderId,"requirement-author","requirement",DateTime.UtcNow);var rows=new[]{actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(pending)};if(mode=="missing")rows[0]=rows[0] with{RequirementRevision=null};var source=initial.Source with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(initial.Source.Stage,initial.Source.InspectionDatabase,initial.Source.Order,rows)};var version=mode=="legacy"?0:mode=="unsupported"?99:1;var review=initial with{Source=source,RequirementEvidenceVersion=version};var proposal=Proposal(review) with{IntentId=Guid.NewGuid(),RequirementEvidenceVersion=version};
        var error=Record.Exception(()=>{review.Require(review.Context.Company,review.Context.ActorUserId,review.Source.Stage.Task);proposal.RequireSource(source);var row=proposal.Prepare(DateTime.UtcNow,source);proposal.RequireRecorded(row);Assert.Equal(version,System.Text.Json.JsonSerializer.Deserialize<CompanyWorkOrderExecutionPermitIntent>(row.ISSUE_DATA_JSON)!.RequirementEvidenceVersion);if(version==0){Assert.DoesNotContain("RequirementEvidenceVersion",row.ISSUE_DATA_JSON);Assert.DoesNotContain("RequirementEvidenceVersion",System.Text.Json.JsonSerializer.Serialize(review));}});if(mode is "current" or "legacy")Assert.Null(error);else Assert.NotNull(error);
    }
    internal static CompanyWorkOrderExecutionPermitIssueReview Review()
    {
        var company=new CompanyScope("tenant","company");var context=new FieldSelectionReview(company,new("core","fingerprint"),"issuer","authority","field");
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(company,Guid.NewGuid()),"field","order/العالم%2F");
        var stage=new CompanyMaintenanceAssignedStageReview(context,task,"step","execute","Configured duty","ConfiguredRole","receiver","history","approval",new string('A',64),new string('B',64));
        var order=new Detail{InstanceId=task.WorkOrderId,FieldId="field",Source="WORK_ORDER",State="",EvidenceUnavailableReason="No execution status",Components=[new(task.WorkOrderId,"component","field",null,null)]};
        var source=new CompanyMaintenanceInspectionReview(stage,context.Database,order,[],CompanyMaintenanceInspectionReview.Revision(stage,context.Database,order,[]),DateTimeOffset.UtcNow);
        return new(context,source);
    }
    [Theory][InlineData("accepted")][InlineData("requirement")][InlineData("removed")][InlineData("added")][InlineData("stage")][InlineData("legacy")]
    public void PolicySixAllowsOnlyExactImmutableRequirementEvolutionAndKeepsOtherPrerequisitesBlocked(string mode)
    {
        var issue=RequirementReview();var proposal=Proposal(issue) with{IntentId=Guid.NewGuid(),RequirementEvidenceVersion=mode=="legacy"?0:1};var original=issue.Source;var context=issue.Context with{ActorUserId="independent",Revision="review-authority"};var stage=original.Stage with{Context=context};var rows=original.Inspections.Select(row=>row with{Status="PASS",Inspector="independent",ResultText="Recorded result",InspectDate=DateTime.UtcNow,SourceRevision=new string('C',64)}).ToArray();
        if(mode=="requirement")rows[0]=rows[0] with{RequiredText="Changed immutable requirement"};if(mode=="removed")rows=[];if(mode=="added")rows=rows.Concat([rows[0] with{InspectionId="added",CondSeq=2}]).ToArray();if(mode=="stage")stage=stage with{StepName="Changed current stage"};var current=original with{Stage=stage,Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,original.InspectionDatabase,original.Order,rows)};var utc=DateTime.UtcNow;
        if(mode=="removed"){Assert.Throws<InvalidOperationException>(()=>CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original,current,utc,6));return;}
        var blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original,current,utc,6);if(mode=="accepted")Assert.DoesNotContain(blockers,value=>value.Contains("facts changed"));else Assert.Contains(blockers,value=>value.Contains("facts changed"));Assert.Contains(blockers,value=>value.Contains("material acceptance"));Assert.Contains(blockers,value=>value.Contains("personnel acceptance"));Assert.Contains(blockers,value=>value.Contains("ISOLATION document acceptance"));
        var old=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original,current,utc,5);Assert.Contains(old,value=>value.Contains("facts changed"));
    }
    [Fact]
    public async Task VersionedIssueStateRefusesLegacyReplacementBeforeRetainingIntentOrTransport()
    {
        var review=RequirementReview();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["ConfiguredRole"],["HSE.IssuePermit"])));using var state=new WorkOrderExecutionPermitState(company);state.Attach(review.Source.Stage.Task,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewWorkOrderExecutionPermitIssueAsync(review.Source.Stage.Task,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);var retained=false;await Assert.ThrowsAsync<InvalidOperationException>(()=>state.IssueAsync(client.Object,Proposal(review),_=>{retained=true;return Task.CompletedTask;}));Assert.False(retained);Assert.Null(state.VisibleReference);client.Verify(value=>value.IssueWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitIntent>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    internal static CompanyWorkOrderExecutionPermitIssueReview RequirementReview()
    {
        var review=Review();var pending=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=review.Context.FieldId!,CondSeq=1,CondType="PRE_START",RequiredText="Original native requirement",Jurisdiction="Explicit jurisdiction",RegulatoryRef="isolation",PermitPrerequisiteKind="ISOLATION"},review.Source.Stage.Task.WorkOrderId,"requirement-author","requirement",DateTime.UtcNow);var rows=new[]{actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(pending)};var source=review.Source with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(review.Source.Stage,review.Source.InspectionDatabase,review.Source.Order,rows)};return review with{Source=source,RequirementEvidenceVersion=1};
    }
    internal static CompanyWorkOrderExecutionPermitApprovalReview RequirementApprovalReview(bool changed=false)
    {
        var issue=RequirementReview();var proposal=Proposal(issue) with{IntentId=Guid.NewGuid(),RequirementEvidenceVersion=1};var context=issue.Context with{ActorUserId="independent",Revision="review-authority"};var stage=issue.Source.Stage with{Context=context};var rows=issue.Source.Inspections.Select(row=>row with{RequiredText=changed?"Changed original requirement":row.RequiredText,Status="PASS",ResultText="Recorded independent result",Inspector="independent",InspectDate=DateTime.UtcNow,SourceRevision=new string('C',64)}).ToArray();var current=issue.Source with{Stage=stage,Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,issue.Source.InspectionDatabase,issue.Source.Order,rows)};var utc=DateTime.UtcNow;var result=new CompanyWorkOrderExecutionPermitApprovalReview(context,proposal,issue.Source,current,new string('D',64),utc,CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,issue.Source,current,utc,6)){PrerequisiteEvidenceVersion=6};result.Require(context.Company,context.ActorUserId,new(proposal.Task,proposal.IntentId));return result;
    }
    internal static CompanyWorkOrderExecutionPermitIntent Proposal(CompanyWorkOrderExecutionPermitIssueReview review)=>new(review.Context,review.Source.Stage.Task,Guid.Empty,review.Source.SnapshotRevision,"ElectricalIsolation","Reported jurisdiction","Original scope","isolation","risk","personnel","materials",new DateTime(2026,10,13,10,0,0,DateTimeKind.Utc),new DateTime(2026,10,13,18,0,0,DateTimeKind.Utc));
    internal static CompanyWorkOrderExecutionPermitReceipt Receipt(CompanyWorkOrderExecutionPermitIntent intent)=>new(intent.Context.Company,intent.Context.ActorUserId,intent.IntentId,intent.PermitId(),new string('C',64));
    internal static CompanyWorkOrderExecutionPermitApprovalReview ApprovalReview()
    {
        var original=Review();var proposal=Proposal(original) with{IntentId=Guid.NewGuid()};var context=original.Context with{ActorUserId="independent",Revision="review-authority"};var stage=original.Source.Stage with{Context=original.Source.Stage.Context with{ActorUserId=context.ActorUserId,Revision="review-stage-authority"}};
        var current=original.Source with{Stage=stage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,original.Source.InspectionDatabase,original.Source.Order,original.Source.Inspections)};var utc=proposal.ValidFromUtc.AddHours(1);
        return new(context,proposal,original.Source,current,new string('D',64),utc,CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original.Source,current,utc));
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")][InlineData("late-field")]
    public async Task RetainsOriginalBeforeTransportAndFencesLateConfirmation(string fault)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(review.Context.Company,["ConfiguredRole"],["HSE.IssuePermit"])));using var state=new WorkOrderExecutionPermitState(company);state.Attach(review.Source.Stage.Task,"source");
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewWorkOrderExecutionPermitIssueAsync(review.Source.Stage.Task,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);
        Guid retained=Guid.Empty;CompanyWorkOrderExecutionPermitIntent? original=null;var writes=0;
        client.Setup(c=>c.IssueWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitIntent>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitIntent body,CancellationToken _)=>
        {
            Assert.Equal(retained,body.IntentId);Assert.Equal(body,state.VisibleIntent);Assert.Equal(retained,state.VisibleReference);original=body;writes++;
            if(fault=="late-company")company.Clear();if(fault=="late-field")state.Attach(null,null);
            return fault=="unknown"?Task.FromException<CompanyWorkOrderExecutionPermitReceipt>(new HttpRequestException("Unknown commit")):Task.FromResult(Receipt(body));
        });
        var error=await Record.ExceptionAsync(()=>state.IssueAsync(client.Object,Proposal(review),id=>{retained=id;Assert.NotEqual(Guid.Empty,id);Assert.Equal(id,state.VisibleIntent!.IntentId);if(fault=="before-transport")state.Attach(null,null);return Task.CompletedTask;}));
        Assert.Equal(fault=="before-transport"?0:1,writes);Assert.False(state.CanIssue);
        if(fault=="confirmed"){Assert.Null(error);Assert.NotNull(state.Receipt);}
        if(fault=="unknown")
        {
            Assert.IsType<HttpRequestException>(error);Assert.Null(state.Receipt);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));
            client.Setup(c=>c.RecoverWorkOrderExecutionPermitReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitReference query,CancellationToken _)=>{Assert.Equal(retained,query.IntentId);return Task.FromResult(new CompanyWorkOrderExecutionPermitRecovery(original!,Receipt(original!)));});
            await state.RecoverAsync(client.Object,retained);Assert.Equal(original,state.VisibleIntent);Assert.NotNull(state.Receipt);Assert.Equal(1,writes);
        }
        if(fault is "before-transport" or "late-company" or "late-field"){Assert.Null(state.VisibleIntent);Assert.Null(state.VisibleReference);Assert.Null(state.Receipt);}
    }
    [Theory][InlineData("original")][InlineData("changed-body")][InlineData("foreign-actor")]
    public async Task FreshCircuitRestoresOnlyOriginalPrivateReferenceReadOnly(string fault)
    {
        var review=Review();var original=Proposal(review) with{IntentId=Guid.NewGuid()};using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(review.Context.Company,["ConfiguredRole"],["HSE.IssuePermit"])));using var state=new WorkOrderExecutionPermitState(company);state.Attach(original.Task,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();
        var returned=fault=="foreign-actor"?original with{Context=original.Context with{ActorUserId="foreign"}}:original;
        client.Setup(c=>c.RecoverWorkOrderExecutionPermitReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyWorkOrderExecutionPermitRecovery(returned,Receipt(returned)));
        if(fault=="foreign-actor")await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,original.IntentId));else await state.RecoverAsync(client.Object,original.IntentId);
        if(fault=="changed-body")
        {
            returned=original with{ScopeDescription="Changed scope"};client.Setup(c=>c.RecoverWorkOrderExecutionPermitReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyWorkOrderExecutionPermitRecovery(returned,Receipt(returned)));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,original.IntentId));Assert.Null(state.Receipt);Assert.Equal(original,state.VisibleIntent);
        }
        client.Verify(c=>c.IssueWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitIntent>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
