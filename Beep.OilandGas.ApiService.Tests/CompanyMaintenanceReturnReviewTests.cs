#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMaintenanceReturnReviewTests
{
    [Fact] public async Task ConfiguredReturnReviewRequiresRecordedVerificationAndDoesNotAdvanceTasksOrPhysicalState()
    {
        var f=new CompanyWorkOrderCompletionDecisionTests.Fixture();await f.Ready(returnRole:"FacilityRole");var native=f.Flow.Flow.Owner.Native;var service=f.Flow.Flow.Owner.Service;
        var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),"VERIFIED","Independent current work accepted");await service.RecordCompletionDecisionAsync(command,f.Authority,f.Read);
        var completion=Current(f.Review.Completion);var before=JsonSerializer.Serialize(native.Rows);var writes=native.Writes;var result=await service.ReviewReturnHandoffAsync(f.Review.Stage.Context,command.Reference().Query,completion,f.Authority);result.Require();
        Assert.Equal("FacilityRole",result.NextRole);Assert.Equal("return",result.NextStepId);Assert.Equal(f.Review.Completion.WorkSubjectRevision(),result.Decision.WorkSubjectRevision);Assert.Equal("VERIFIED",result.Decision.Decision);
        Assert.Equal("verify",native.Rows.OfType<PROCESS_INSTANCE>().Single().CURRENT_STEP_ID);Assert.Equal("BLOCKED",native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="return").STATUS);Assert.Equal(before,JsonSerializer.Serialize(native.Rows));Assert.Equal(writes,native.Writes);
    }
    [Theory][InlineData("missing")][InlineData("rejected")][InlineData("no-return")][InlineData("executor-role")][InlineData("reviewer-role")][InlineData("changed-evidence")][InlineData("foreign-company")][InlineData("revoked")][InlineData("changed-step")]
    public async Task UnverifiedChangedOrUnconfiguredReturnCannotBePresentedAsReady(string fault)
    {
        var f=new CompanyWorkOrderCompletionDecisionTests.Fixture();var role=fault switch{"no-return"=>null,"executor-role"=>"SupervisorRole","reviewer-role"=>"VerifierRole",_=>"FacilityRole"};await f.Ready(returnRole:role);var service=f.Flow.Flow.Owner.Service;
        var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),fault=="rejected"?"REJECTED":"VERIFIED","Original independent decision");if(fault!="missing")await service.RecordCompletionDecisionAsync(command,f.Authority,f.Read);
        var completion=Current(f.Review.Completion);var context=f.Review.Stage.Context;
        if(fault=="changed-evidence"){var rows=completion.Requirements.Select(row=>row.ApplicableStage=="COMPLETION_VERIFICATION"?row with{ResultText="Changed accepted observation"}:row).ToArray();completion=completion with{Requirements=rows,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(completion.Context,completion.Report,completion.Order,rows,completion.Identities,completion.ReviewerPerson,completion.ComparedAtUtc)};}
        if(fault=="foreign-company")context=context with{Company=new("tenant","foreign")};if(fault=="revoked")f.Revoked=true;if(fault=="changed-step")f.Flow.Flow.Owner.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="return").STATUS="PENDING";
        var before=JsonSerializer.Serialize(f.Flow.Flow.Owner.Native.Rows);var writes=f.Flow.Flow.Owner.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(()=>service.ReviewReturnHandoffAsync(context,command.Reference().Query,completion,f.Authority));Assert.Equal(before,JsonSerializer.Serialize(f.Flow.Flow.Owner.Native.Rows));Assert.Equal(writes,f.Flow.Flow.Owner.Native.Writes);
    }
    private static CompanyWorkOrderCompletionReview Current(CompanyWorkOrderCompletionReview source)
    {var utc=DateTime.UtcNow;return source with{ComparedAtUtc=utc,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(source.Context,source.Report,source.Order,source.Requirements,source.Identities,source.ReviewerPerson,utc)};}
    [Fact] public async Task FutureReturnResultsDoNotReplaceTheSubjectOfTheEarlierVerification()
    {
        var f=new CompanyWorkOrderCompletionDecisionTests.Fixture();await f.Ready();var original=f.Review.Completion;var task=original.Report.Query.Task;
        var requirement=WorkOrderInspectionRules.ProjectWithRequirementProof(WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=1000,CondType="REGULATORY",ApplicableStage="RETURN_TO_SERVICE",RequiredText="Actual facility return observation",Jurisdiction="Recorded jurisdiction"},task.WorkOrderId,"requirement-maker","return-check",original.Report.CompletedAtUtc.AddDays(-1)));
        var requirements=original.Requirements.Append(requirement).ToArray();var report=original.Report with{OriginalRequirementPopulation=CompanyWorkOrderExecutionRequirements.Population(task.FieldId,task.WorkOrderId,requirements)};
        var baseline=original with{Report=report,Requirements=requirements,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(original.Context,report,original.Order,requirements,original.Identities,original.ReviewerPerson,original.ComparedAtUtc)};var revision=baseline.WorkSubjectRevision();
        var recorded=requirements.Select(row=>row.ApplicableStage=="RETURN_TO_SERVICE"?row with{Status="PASS",ResultText="Actual return observation",Inspector="facility-reviewer",InspectDate=original.ComparedAtUtc,SourceRevision=new string('D',64)}:row).ToArray();
        var later=baseline with{Requirements=recorded,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(original.Context,report,original.Order,recorded,original.Identities,original.ReviewerPerson,original.ComparedAtUtc)};Assert.Equal(revision,later.WorkSubjectRevision());Assert.NotEqual(baseline.SourceRevision(),later.SourceRevision());
    }
    [Fact] public async Task WorkSubjectBindsActualResultsButExcludesCurrentViewerAndComparisonClock()
    {
        var f=new CompanyWorkOrderCompletionDecisionTests.Fixture();await f.Ready();var original=f.Review.Completion;var revision=original.WorkSubjectRevision();
        var at=original.ComparedAtUtc.AddSeconds(1);var later=original with{ComparedAtUtc=at,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(original.Context,original.Report,original.Order,original.Requirements,original.Identities,original.ReviewerPerson,at)};Assert.Equal(revision,later.WorkSubjectRevision());
        var context=original.Context with{ActorUserId="second-reviewer",Revision="current-authority"};var first=original.Identities.Versions.Single();var second=first with{RecordId=CompanyPersonnelIdentityCommand.RecordIdFor(context.Company,context.ActorUserId,1),UserId=context.ActorUserId,PersonBaId="second-person"};
        var catalog=CompanyPersonnelIdentityCatalog.Create(new(context.Company,context.Database,context.ActorUserId,new string('B',64)),[first,second],at);var person=original.ReviewerPerson! with{PersonBaId="second-person",Employment=original.ReviewerPerson!.Employment with{EmployeeBaId="second-person"}};
        var viewer=original with{Context=context,Identities=catalog,ReviewerPerson=person,ComparedAtUtc=at,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(context,original.Report,original.Order,original.Requirements,catalog,person,at)};Assert.Equal(revision,viewer.WorkSubjectRevision());Assert.NotEqual(original.SourceRevision(),viewer.SourceRevision());
        var results=original.Requirements.Select(row=>row.ApplicableStage=="COMPLETION_VERIFICATION"?row with{ResultText="Changed actual result"}:row).ToArray();var changed=original with{Requirements=results,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(original.Context,original.Report,original.Order,results,original.Identities,original.ReviewerPerson,original.ComparedAtUtc)};Assert.NotEqual(revision,changed.WorkSubjectRevision());
    }
}
#endif
