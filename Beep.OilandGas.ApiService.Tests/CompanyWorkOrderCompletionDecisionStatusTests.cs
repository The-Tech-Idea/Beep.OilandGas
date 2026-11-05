#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyWorkOrderCompletionDecisionStatusTests
{
    [Theory][InlineData("VERIFIED")][InlineData("REJECTED")][InlineData("unrecorded")]
    public async Task AnotherAuthorizedCompanyActorReadsPublicRecordedOutcomeWithoutPrivateCommandOrRecoveryIntent(string decision)
    {
        var f=new CompanyWorkOrderCompletionDecisionTests.Fixture();await f.Ready();var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),decision=="REJECTED"?"REJECTED":"VERIFIED","Private original reviewer decision reason");
        if(decision!="unrecorded")await f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read);var native=f.Flow.Flow.Owner.Native;var writes=native.Writes;var transactions=native.Transactions;
        var viewer=f.Review.Stage.Context with{ActorUserId="another-company-reader",Revision="current-read-authority"};var result=await f.Flow.Flow.Owner.Service.ReadCompletionDecisionFactsAsync(viewer,command.Reference().Query,()=>Task.CompletedTask);
        if(decision=="unrecorded")Assert.Null(result);else{Assert.NotNull(result);result.Require();Assert.Equal(decision,result.Decision);Assert.Equal(f.Review.Stage.Context.ActorUserId,result.ReviewerUserId);Assert.Equal(f.Review.Completion.ReviewerPerson!.PersonBaId,result.ReviewerPersonBaId);Assert.Equal(f.Review.Completion.Report.Revision,result.ReportRevision);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(result));Assert.DoesNotContain(command.IntentId.ToString(),JsonSerializer.Serialize(result));Assert.DoesNotContain("\"Command\"",JsonSerializer.Serialize(result));}
        Assert.Equal(writes,native.Writes);Assert.Equal(transactions,native.Transactions);
        if(decision!="unrecorded")await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Flow.Flow.Owner.Service.RecoverCompletionDecisionAsync(viewer,command.Reference(),_=>Task.CompletedTask));Assert.Equal(writes,native.Writes);
    }
    [Theory][InlineData("company")][InlineData("field")][InlineData("report")][InlineData("authority")][InlineData("changed-history")][InlineData("late-approval")]
    public async Task PublicDecisionStatusRefusesForeignChangingOrUnauthorizedProofInsteadOfPublishingAbsence(string fault)
    {
        var f=new CompanyWorkOrderCompletionDecisionTests.Fixture();await f.Ready();var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),"VERIFIED","Private reviewer rationale");await f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read);var native=f.Flow.Flow.Owner.Native;var writes=native.Writes;var context=f.Review.Stage.Context with{ActorUserId="reader"};var query=command.Reference().Query;
        if(fault=="company")context=context with{Company=new("tenant","foreign-company")};if(fault=="field")context=context with{FieldId="other-field"};if(fault=="report")query=query with{SubmissionId="other-report"};if(fault=="changed-history")native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="WORK_VERIFIED").USER_ID="different-reviewer";var guards=0;
        Task Guard(){guards++;if(fault=="authority")throw new UnauthorizedAccessException("Current read capability revoked");if(fault=="late-approval" && guards==2)native.Rows.OfType<PROCESS_APPROVAL>().Single(row=>row.APPROVAL_TYPE=="WORK_COMPLETION").APPROVAL_STATUS="REJECTED";return Task.CompletedTask;}
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Flow.Owner.Service.ReadCompletionDecisionFactsAsync(context,query,Guard));Assert.Equal(writes,native.Writes);
    }
}
#endif
