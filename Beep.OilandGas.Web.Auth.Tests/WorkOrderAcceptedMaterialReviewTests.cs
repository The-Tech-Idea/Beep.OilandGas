using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderAcceptedMaterialReviewTests
{
    [Fact]
    public void CompleteOriginalRequirementsAcceptAllFourIndependentCommandsAndRetainApprovalBody()
    {
        var f=WorkOrderCompleteApprovalFixture.Create();Assert.Empty(f.Review.Blockers);
        Assert.Equal(f.Review.OriginalSource.RequirementFactsRevision(),f.Review.CurrentSource.RequirementFactsRevision());
        var decision=new CompanyWorkOrderExecutionPermitDecision(f.Review,Guid.NewGuid(),"Approve","Independent complete prerequisite approval");
        var saved=decision.Prepare(f.Pending,DateTime.UtcNow);decision.RequireRecorded(saved);Assert.Equal("APPROVED",saved.STATUS);Assert.Equal(f.Pending.ISSUE_DATA_JSON,saved.ISSUE_DATA_JSON);
        Assert.Equal(JsonSerializer.Serialize(decision),JsonSerializer.Serialize(CompanyWorkOrderExecutionPermitDecision.FromRecorded(saved)));
        Assert.Equal(JsonSerializer.Serialize(f.Pending),JsonSerializer.Serialize(decision.ReadOriginalPending(saved)));
        Assert.Throws<InvalidOperationException>(()=>decision.Prepare(f.Pending,decision.Review.Proposal.ValidToUtc));
        saved.ROW_CREATED_BY="changed original issuer audit";Assert.Throws<InvalidOperationException>(()=>decision.RequireRecorded(saved));
    }
    [Theory][InlineData("material")][InlineData("personnel")][InlineData("risk")][InlineData("identity-population")]
    public void CompleteApprovalCannotAdoptMissingProofOrDifferentGlobalIdentityPopulations(string mode)
    {
        var f=WorkOrderCompleteApprovalFixture.Create();var review=f.Review;
        if(mode=="material")review=review with{CurrentMaterialAcceptance=null};
        if(mode=="personnel")review=review with{CurrentPersonnelAcceptance=null};
        if(mode=="risk")review=review with{CurrentDocumentAcceptance=review.CurrentDocumentAcceptance!.Where(row=>row.Acceptance.Kind!="RISK").ToArray()};
        if(mode=="identity-population")
        {
            var documents=review.CurrentDocumentAcceptance!.ToArray();var catalog=documents[1].CurrentIdentities!;
            var changed=CompanyPersonnelIdentityCatalog.Create(catalog.Context with{AuthorityRevision=new string('F',64)},catalog.Versions,catalog.ComparedAtUtc);
            documents[1]=documents[1] with{CurrentIdentities=changed};review=review with{CurrentDocumentAcceptance=documents};
        }
        review=review with{Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(review.Proposal,review.OriginalSource,review.CurrentSource,review.ComparedAtUtc,6,review.CurrentMaterialAcceptance,review.CurrentPersonnelAcceptance,review.CurrentDocumentAcceptance)};
        var decision=new CompanyWorkOrderExecutionPermitDecision(review,Guid.NewGuid(),"Approve","Original complete review is required");
        Assert.Throws<InvalidOperationException>(decision.Require);Assert.Throws<InvalidOperationException>(()=>decision.Prepare(f.Pending,DateTime.UtcNow));
        Assert.Equal("ISSUED_PENDING_APPROVAL",f.Pending.STATUS);Assert.Null(f.Pending.DECISION_DATA_JSON);
    }
    [Fact]
    public void CurrentPrerequisiteProofKeepsObservationTimeSeparateFromRetainedAcceptedSource()
    {
        var original=Review();var digest=original.PrerequisiteProofRevision();
        var observed=original with{ComparedAtUtc=original.ComparedAtUtc.AddTicks(1)};
        Assert.Equal(digest,observed.PrerequisiteProofRevision());
        var missing=observed with{CurrentMaterialAcceptance=null};
        missing=missing with{Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(missing.Proposal,missing.OriginalSource,missing.CurrentSource,missing.ComparedAtUtc,missing.PrerequisiteEvidenceVersion)};
        Assert.NotEqual(digest,missing.PrerequisiteProofRevision());
        Assert.Throws<InvalidOperationException>(()=>(missing with{Blockers=[]}).PrerequisiteProofRevision());
        Assert.Throws<InvalidOperationException>(()=>(observed with{CurrentMaterialAcceptance=observed.CurrentMaterialAcceptance! with{Acceptance=observed.CurrentMaterialAcceptance.Acceptance with{PermitIntentId=Guid.NewGuid()}}}).PrerequisiteProofRevision());
    }
    internal static CompanyWorkOrderExecutionPermitApprovalReview Review(string mode="current")
    {
        var f=new WorkOrderMaterialAcceptanceFixture();var row=f.Command.Prepare(f.Pending,DateTime.UtcNow);var acceptance=f.Command.Facts(row);if(mode=="stock")f.Inventory.QUANTITY_ON_HAND=4m;var ready=f.Current(row);var material=ready.CurrentMaterial;var utc=DateTime.UtcNow;
        if(mode=="released")
        {
            var original=CompanyWorkOrderMaterialAllocationCommand.Read(f.Allocation);var release=new CompanyWorkOrderMaterialReleaseCommand(new(original,f.Allocation),Guid.NewGuid(),"Cancelled accepted requirement").Prepare(utc);utc=DateTime.UtcNow;material=CompanyWorkOrderMaterialAllocationReview.Create(material.Source,[f.Allocation],[release],utc);
        }
        var current=new CompanyWorkOrderAcceptedMaterialReview(acceptance,ready.Reservation,material,utc,CompanyWorkOrderAcceptedMaterialReview.GetBlockers(ready.PermitReview.Proposal,acceptance,ready.Reservation,material,utc));return ready.PermitReview with{ComparedAtUtc=DateTime.UtcNow,PrerequisiteEvidenceVersion=2,CurrentMaterialAcceptance=current,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(ready.PermitReview.Proposal,ready.PermitReview.OriginalSource,ready.PermitReview.CurrentSource,DateTime.UtcNow,2,current)};
    }
    [Theory][InlineData("current")][InlineData("stock")][InlineData("released")][InlineData("forged")][InlineData("foreign")]
    public void CurrentAcceptanceDistinguishesRecordedProofFromChangedStockAndRelease(string mode)
    {
        var review=Review(mode);var query=new CompanyWorkOrderExecutionPermitReference(review.Proposal.Task,review.Proposal.IntentId);
        if(mode=="forged")review=review with{CurrentMaterialAcceptance=review.CurrentMaterialAcceptance! with{Blockers=[]},Blockers=[]};if(mode=="foreign")review=review with{CurrentMaterialAcceptance=review.CurrentMaterialAcceptance! with{Acceptance=review.CurrentMaterialAcceptance.Acceptance with{PermitIntentId=Guid.NewGuid()}}};
        var error=Record.Exception(()=>review.Require(review.Context.Company,review.Context.ActorUserId,query));if(mode is "forged" or "foreign"){Assert.NotNull(error);return;}Assert.Null(error);var material=review.CurrentMaterialAcceptance!;
        if(mode=="current"){Assert.Empty(material.Blockers);Assert.DoesNotContain(review.Blockers,row=>row.Contains("material inventory quantity"));Assert.Contains(review.Blockers,row=>row.Contains("personnel qualifications"));}
        if(mode=="stock")Assert.Contains(material.Blockers,row=>row.Contains("changed since"));if(mode=="released")Assert.Contains(material.Blockers,row=>row.Contains("released"));Assert.DoesNotContain("MATERIAL_ACCEPTANCE_DATA_JSON",JsonSerializer.Serialize(review));Assert.DoesNotContain("Private original logistics rationale",JsonSerializer.Serialize(review));
    }
    [Theory][InlineData(0)][InlineData(1)]
    public void RetainedLegacyPolicyBodiesAndBlockersRemainExact(int version)
    {
        var legacy=WorkOrderExecutionPermitStateTests.ApprovalReview();legacy=legacy with{PrerequisiteEvidenceVersion=version,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(legacy.Proposal,legacy.OriginalSource,legacy.CurrentSource,legacy.ComparedAtUtc,version)};var json=JsonSerializer.Serialize(legacy);Assert.DoesNotContain("CurrentMaterialAcceptance",json);var copy=JsonSerializer.Deserialize<CompanyWorkOrderExecutionPermitApprovalReview>(json)!;Assert.Equal(json,JsonSerializer.Serialize(copy));copy.Require(legacy.Context.Company,legacy.Context.ActorUserId,new(legacy.Proposal.Task,legacy.Proposal.IntentId));new CompanyWorkOrderExecutionPermitDecision(copy,Guid.NewGuid(),"Reject","Original rejection rationale").Require();
    }
    [Fact] public void CurrentPolicyRejectionStillRetainsExactEvidenceWithoutApprovingWork()
    {
        var review=Review();var decision=new CompanyWorkOrderExecutionPermitDecision(review,Guid.NewGuid(),"Reject","Independent rejection");decision.Require();var json=JsonSerializer.Serialize(decision);Assert.Equal(json,JsonSerializer.Serialize(JsonSerializer.Deserialize<CompanyWorkOrderExecutionPermitDecision>(json)));Assert.Throws<InvalidOperationException>(()=>(decision with{Outcome="Approve"}).Require());
    }
}
#endif
