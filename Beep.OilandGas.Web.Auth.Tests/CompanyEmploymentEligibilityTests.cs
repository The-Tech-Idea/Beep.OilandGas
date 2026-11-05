#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using TheTechIdea.Data.OilGas;
using Xunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class CompanyEmploymentEligibilityTests
{
    [Fact] public async Task FreshReviewerCannotRetainOrSendHistoricalEligibilityButCanRecoverItsExactOriginalReference()
    {
        var f=new WorkOrderPersonnelAcceptanceFixture(true);var context=f.Evidence.Assignment.Roster.Context;using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["HSE.ApprovePermit"])));
        using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(f.Evidence.Assignment.Query.Permit,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();var retained=false;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.AcceptPersonnelAsync(client.Object,f.Evidence,f.Pending.INSPECTION_ID,"New historical attempt",_=>{retained=true;return Task.CompletedTask;}));Assert.False(retained);Assert.Null(state.VisiblePersonnelAcceptanceReference);Assert.True(state.CanAcceptPersonnel);
        var reference=new CompanyWorkOrderPersonnelAcceptanceReference(f.Evidence.Assignment.Query.Permit,f.Command.InspectionId,f.Command.IntentId);client.Setup(c=>c.RecoverWorkOrderPersonnelAcceptanceReferenceAsync(reference,It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyWorkOrderPersonnelAcceptanceRecovery(f.Command,f.Command.Prepare(f.Pending,DateTime.UtcNow)));
        await state.RecoverPersonnelAcceptanceReferenceAsync(client.Object,reference);f.Command.RequireRecorded(state.PersonnelAcceptanceReceipt!);Assert.Equal(0,state.VisiblePersonnelAcceptanceCommand!.Review.EligibilityEvidenceVersion);
        client.Verify(c=>c.RecordWorkOrderPersonnelAcceptanceAsync(It.IsAny<CompanyWorkOrderPersonnelAcceptanceCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    internal static CompanyPersonnelPolicyEvidenceReview Review(string mode)
    {
        var baseline=CompanyPersonnelPolicyClientTests.Evidence();var oldPolicy=baseline.ApplicablePolicy!;var draft=oldPolicy.Policy;
        var member=baseline.Credentials.Personnel.Employment!;
        if(mode=="rejected")member=member with{Status="SUSPENDED"};
        if(mode=="unknown")member=member with{Status=null};
        if(mode=="case")member=member with{Status="active"};
        if(mode=="company-choice") {member=member with{Status="Company-specific eligible status"};draft=draft with{AcceptedEmploymentStatuses=["Company-specific eligible status"]};}
        if(mode=="positions")draft=draft with{AcceptedEmploymentPositions=[member.Position!]};
        if(mode=="wrong-position")draft=draft with{AcceptedEmploymentPositions=["Another recorded position"]};
        if(mode is "unconfigured" or "historical")draft=draft with{AcceptedEmploymentStatuses=null,AcceptedEmploymentPositions=null};
        var maker=baseline.Policies.Context with{ActorUserId=oldPolicy.RecordedBy};var catalog=CompanyPersonnelPolicyCatalog.Create(maker,[],DateTime.UtcNow);
        var policy=new CompanyPersonnelPolicyCommand(maker,catalog.Revision,draft,1,Guid.NewGuid(),"Explicit policy source");var version=CompanyPersonnelPolicyVersion.From(policy.Prepare(DateTime.UtcNow));
        var policies=CompanyPersonnelPolicyCatalog.Create(baseline.Policies.Context,[version],DateTime.UtcNow);
        var source=CompanyPersonnelSourceFacts.Create(baseline.Credentials.Personnel.Context,baseline.Credentials.Personnel.Query,member,baseline.Credentials.Personnel.License,DateTime.UtcNow);
        var personnel=baseline.Assignment.CurrentPersonnel with{Personnel=source,Blockers=CompanyWorkOrderPersonnelSourceReview.GetBlockers(baseline.Assignment.CurrentPersonnel.PermitReview.Proposal,source)};
        var assignment=baseline.Assignment with{CurrentPersonnel=personnel,Blockers=CompanyWorkOrderAssignedPersonnelReview.GetBlockers(baseline.Assignment.Assignment,personnel,baseline.Assignment.Roster.Releases)};
        var credentials=CompanyPersonnelCredentialFacts.Create(source,baseline.Credentials.Statuses,baseline.Credentials.Conditions,DateTime.UtcNow);var rule=mode=="historical"?0:1;
        return new(assignment,policies,credentials,CompanyPersonnelPolicyEvidenceReview.GetBlockers(assignment,policies,credentials,rule)){EligibilityEvidenceVersion=rule};
    }
    [Theory][InlineData("current")][InlineData("rejected")][InlineData("unknown")][InlineData("case")][InlineData("company-choice")][InlineData("positions")][InlineData("wrong-position")][InlineData("unconfigured")][InlineData("historical")]
    public void EligibilityUsesExplicitCompanyChoicesAndRetainsHistoricalReviewRules(string mode)
    {
        var review=Review(mode);review.Require(review.Assignment.Roster.Context.Company,review.Assignment.Roster.Context.ActorUserId,review.Assignment.Query);
        if(mode is "current" or "company-choice" or "positions" or "historical")Assert.Single(review.Blockers);else Assert.True(review.Blockers.Count>1);
        if(mode=="historical")
        {
            Assert.DoesNotContain("AcceptedEmployment",JsonSerializer.Serialize(review.Policies));Assert.DoesNotContain("EligibilityEvidenceVersion",JsonSerializer.Serialize(review));
            Assert.Throws<InvalidOperationException>(()=>review.ApplicablePolicy!.Policy.RequireCurrentEligibility());
            var current=review with{EligibilityEvidenceVersion=1,Blockers=CompanyPersonnelPolicyEvidenceReview.GetBlockers(review.Assignment,review.Policies,review.Credentials,1)};Assert.Contains(current.Blockers,value=>value.Contains("historical policy"));
        }
        else Assert.Throws<InvalidOperationException>(()=>(review with{EligibilityEvidenceVersion=99}).Require(review.Assignment.Roster.Context.Company,review.Assignment.Roster.Context.ActorUserId,review.Assignment.Query));
    }
    [Theory][InlineData("empty")][InlineData("duplicate")][InlineData("whitespace")][InlineData("positions")]
    public void InvalidEmploymentPolicyChoicesCannotBecomeSilentDefaults(string mode)
    {
        var policy=CompanyPersonnelPolicyClientTests.Draft();policy=mode switch{"empty"=>policy with{AcceptedEmploymentStatuses=[]},"duplicate"=>policy with{AcceptedEmploymentStatuses=["ACTIVE","ACTIVE"]},"whitespace"=>policy with{AcceptedEmploymentStatuses=[" "]},_=>policy with{AcceptedEmploymentPositions=["Operator","Operator"]}};
        Assert.Throws<ArgumentException>(()=>policy.RequireCurrentEligibility());
    }
    [Fact] public void ChangedEmploymentStatusBlocksCurrentAcceptanceWithoutChangingRecordedHistory()
    {
        var f=new WorkOrderPersonnelAcceptanceFixture();var saved=f.Command.Prepare(f.Pending,DateTime.UtcNow);var original=JsonSerializer.Serialize(saved);var current=f.Current(saved);var member=current.Credentials.Personnel.Employment! with{Status="SUSPENDED",Revision=new string('E',64)};
        var source=CompanyPersonnelSourceFacts.Create(current.Credentials.Personnel.Context,current.Credentials.Personnel.Query,member,current.Credentials.Personnel.License,DateTime.UtcNow);
        var personnel=current.Assignment.CurrentPersonnel with{Personnel=source,Blockers=CompanyWorkOrderPersonnelSourceReview.GetBlockers(current.Assignment.CurrentPersonnel.PermitReview.Proposal,source)};
        var assignment=current.Assignment with{CurrentPersonnel=personnel,Blockers=CompanyWorkOrderAssignedPersonnelReview.GetBlockers(current.Assignment.Assignment,personnel,current.Assignment.Roster.Releases)};
        var credentials=CompanyPersonnelCredentialFacts.Create(source,current.Credentials.Statuses,current.Credentials.Conditions,DateTime.UtcNow);current=current with{Assignment=assignment,Credentials=credentials,Blockers=CompanyPersonnelPolicyEvidenceReview.GetBlockers(assignment,current.Policies,credentials,1)};
        Assert.Throws<InvalidOperationException>(()=>f.Command.RequireCurrent(current,saved));Assert.Contains(CompanyWorkOrderAcceptedPersonnelReview.GetBlockers(current.Assignment.CurrentPersonnel.PermitReview.Proposal,f.Command.Facts(saved),current,DateTime.UtcNow),value=>value.Contains("employment status"));f.Command.RequireRecorded(saved);Assert.Equal(original,JsonSerializer.Serialize(saved));
    }
}
#endif
