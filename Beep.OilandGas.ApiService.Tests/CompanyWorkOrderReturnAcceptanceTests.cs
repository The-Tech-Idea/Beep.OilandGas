#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyWorkOrderReturnAcceptanceTests
{
    internal sealed class Fixture
    {
        internal readonly CompanyMaintenanceReturnHandoffTests.Fixture Flow=new();
        internal CompanyMaintenanceReturnHandoffRecovery Handoff=null!;
        internal CompanyWorkOrderReturnReview Review=null!;
        internal bool Changed,OwnedChanged,Revoked;
        internal async Task Ready(bool realAudit=false,string mode="current",bool assetReturn=false,bool multipleWells=false)
        {
            await Flow.Ready(realAudit,returnAcceptance:mode!="missing-check",assetReturn:assetReturn,multipleWells:multipleWells);var service=Flow.Flow.Flow.Flow.Owner.Service;Handoff=await service.RecordReturnHandoffAsync(Flow.Command,Flow.Flow.Authority,Flow.Flow.Read);var source=Flow.Command.Review.Completion;var lifecycle=Flow.Command.Review.Stage.Context with{ActorUserId="return-approver",Revision="return-acceptance-authority"};var assignment=await service.ReadReturnAssignmentAsync(lifecycle,source.Report.Query,Authority);
            var utc=DateTime.UtcNow;var context=source.Context with{ActorUserId=lifecycle.ActorUserId,Revision=lifecycle.Revision};var personId=mode=="verifier-person"?assignment.Verification.ReviewerPersonBaId:"return-person";
            var person=new CompanyNativePersonFacts(context.Company,personId,"Recorded return approver","PERSON",utc.AddDays(-1),utc.AddDays(1),new(context.Company.BusinessAssociateId,personId,1,"Recorded return duty","ACTIVE",utc.AddDays(-1),utc.AddDays(1),new string('D',64)),new string('E',64));
            var link=new CompanyPersonnelIdentityVersion(context.Company,context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(context.Company,context.ActorUserId,1),context.ActorUserId,personId,"PERSON",1,utc.AddDays(-1),utc.AddDays(1),"Prior independently recorded identity","identity-administrator",source.Identities.Versions.Min(row=>row.RecordedAtUtc),new string('F',64));
            var links=mode=="verifier-person"?source.Identities.Versions.Where(row=>row.PersonBaId!=personId).Append(link).ToArray():source.Identities.Versions.Append(link).ToArray();var identities=CompanyPersonnelIdentityCatalog.Create(new(context.Company,context.Database,context.ActorUserId,new string('A',64)),links,DateTime.UtcNow);
            var rows=source.Requirements.Select(row=>row.ApplicableStage=="RETURN_TO_SERVICE"?row with{Status="PASS",ResultText="Actual accepted return observation",Inspector=context.ActorUserId,InspectDate=mode=="early"?Handoff.Receipt.RecordedAtUtc.AddSeconds(-1):utc,SourceRevision=new string('C',64)}:row).ToArray();
            if(mode=="unaccepted")rows=source.Requirements.ToArray();var comparison=DateTime.UtcNow;var completion=new CompanyWorkOrderCompletionReview(context,source.Report,source.Order,rows,identities,person,comparison,CompanyWorkOrderCompletionReview.GetBlockers(context,source.Report,source.Order,rows,identities,person,comparison));var inspectors=mode=="missing-person"?Array.Empty<CompanyNativePersonFacts>():[person];
            Review=new(assignment,completion,inspectors,CompanyWorkOrderReturnReview.GetBlockers(assignment,completion,inspectors));Review.Require();
        }
        internal Task Authority(string? role){if(Revoked)throw new UnauthorizedAccessException("Return authority revoked");if(role is not null)Assert.Equal("FacilityRole",role);return Task.CompletedTask;}
        internal Task<CompanyWorkOrderReturnReview?> Read(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(Flow.Flow.Flow.Flow.Owner.Native.Source.Object,System.Data.IsolationLevel.Serializable);if(Changed || owned && OwnedChanged)throw new InvalidOperationException("Changed current physical return evidence");var source=Review.Completion;var utc=DateTime.UtcNow;var completion=source with{ComparedAtUtc=utc,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(source.Context,source.Report,source.Order,source.Requirements,source.Identities,source.ReviewerPerson,utc)};var result=Review with{Completion=completion,Blockers=CompanyWorkOrderReturnReview.GetBlockers(Review.Assignment,completion,Review.Inspectors)};return Task.FromResult<CompanyWorkOrderReturnReview?>(result);
        }
        internal CompanyWorkOrderReturnCommand Command()=>new(Review,Guid.NewGuid(),DateTime.UtcNow,"Independent actual return accepted",true);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualReturnAcceptanceAppendsThreeRowsAndPreservesAllEarlierReceiptsAndPhysicalSources(bool realAudit)
    {
        var f=new Fixture();await f.Ready(realAudit);var service=f.Flow.Flow.Flow.Flow.Owner.Service;var native=f.Flow.Flow.Flow.Flow.Owner.Native;var command=f.Command();command.Require();var report=JsonSerializer.Serialize(native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single());var tasks=JsonSerializer.Serialize(native.Rows.OfType<CROSS_PERSONA_TASK>().ToArray());var steps=JsonSerializer.Serialize(native.Rows.OfType<PROCESS_STEP_INSTANCE>().ToArray());var writes=native.Writes;
        Assert.Null(await service.ReadReturnAcceptanceFactsAsync(f.Review.Assignment.Stage.Context,command.Reference().Query,()=>Task.CompletedTask));var result=await service.RecordReturnAcceptanceAsync(command,f.Authority,f.Read);result.Require(f.Review.Assignment.Stage.Context.Company,f.Review.Assignment.Stage.Context.ActorUserId,command.Reference());Assert.Equal(writes+3,native.Writes);writes=native.Writes;
        Assert.Equal(report,JsonSerializer.Serialize(native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single()));Assert.Equal(tasks,JsonSerializer.Serialize(native.Rows.OfType<CROSS_PERSONA_TASK>().ToArray()));Assert.Equal(steps,JsonSerializer.Serialize(native.Rows.OfType<PROCESS_STEP_INSTANCE>().ToArray()));Assert.Equal("return",native.Rows.OfType<PROCESS_INSTANCE>().Single().CURRENT_STEP_ID);
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await service.RecoverReturnAcceptanceAsync(f.Review.Assignment.Stage.Context,command.Reference(),f.Authority)));Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await service.RecordReturnAcceptanceAsync(command,f.Authority,f.Read)));
        Assert.Equal(JsonSerializer.Serialize(f.Handoff),JsonSerializer.Serialize(await service.RecoverReturnHandoffAsync(f.Handoff.Command.Review.Stage.Context,f.Handoff.Command.Reference(),f.Flow.Flow.Authority)));
        Assert.Equal(JsonSerializer.Serialize(f.Flow.Decision),JsonSerializer.Serialize(await service.RecoverCompletionDecisionAsync(f.Flow.Decision.Command.Review.Stage.Context,f.Flow.Decision.Command.Reference(),f.Flow.Flow.Authority)));
        Assert.Equal(JsonSerializer.Serialize(f.Flow.Flow.Handoff),JsonSerializer.Serialize(await service.RecoverExecutionHandoffAsync(f.Flow.Flow.Handoff.Command.Review.Stage.Context,f.Flow.Flow.Handoff.Command.Reference(),f.Flow.Flow.Flow.Authority)));
        var facts=await service.ReadReturnAcceptanceFactsAsync(f.Review.Assignment.Stage.Context with{ActorUserId="authorized-reader"},command.Reference().Query,()=>Task.CompletedTask);Assert.Equal(command.ReturnedAtUtc,facts!.ReturnedAtUtc);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(facts));Assert.DoesNotContain(command.IntentId.ToString(),JsonSerializer.Serialize(facts));Assert.Equal(writes,native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.RecordReturnAcceptanceAsync(command with{IntentId=Guid.NewGuid()},f.Authority,f.Read));Assert.Equal(writes,native.Writes);
    }
    [Theory][InlineData("missing-check")][InlineData("early")][InlineData("unaccepted")][InlineData("missing-person")][InlineData("verifier-person")][InlineData("source")][InlineData("owned-source")][InlineData("revoked")][InlineData("future")][InlineData("unconfirmed")]
    public async Task UnestablishedReturnInspectionIdentitySourceAuthorityOrTimeCannotRecordAcceptance(string fault)
    {
        var f=new Fixture();await f.Ready(mode:fault);if(fault=="source")f.Changed=true;if(fault=="owned-source")f.OwnedChanged=true;if(fault=="revoked")f.Revoked=true;var command=f.Command();if(fault=="future")command=command with{ReturnedAtUtc=DateTime.UtcNow.AddHours(1)};if(fault=="unconfirmed")command=command with{Confirmed=false};var native=f.Flow.Flow.Flow.Flow.Owner.Native;var writes=native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Flow.Flow.Flow.Owner.Service.RecordReturnAcceptanceAsync(command,f.Authority,f.Read));Assert.Equal(writes,native.Writes);
    }
    [Theory][InlineData("approval")][InlineData("history")][InlineData("task")][InlineData("delta")][InlineData("foreign-actor")][InlineData("foreign-intent")][InlineData("uncertain")]
    public async Task OriginalReturnRecoveryRejectsChangedLaterGraphAndNeverReplaysAnUncertainWrite(string fault)
    {
        var f=new Fixture();await f.Ready();var native=f.Flow.Flow.Flow.Flow.Owner.Native;var service=f.Flow.Flow.Flow.Flow.Owner.Service;var command=f.Command();native.FailCommit=fault=="uncertain";var error=await Record.ExceptionAsync(()=>service.RecordReturnAcceptanceAsync(command,f.Authority,f.Read));if(fault=="uncertain")Assert.NotNull(error);else Assert.Null(error);native.FailCommit=false;var writes=native.Writes;var context=f.Review.Assignment.Stage.Context;var reference=command.Reference();
        if(fault=="approval")native.Rows.OfType<PROCESS_APPROVAL>().Single(row=>row.APPROVAL_TYPE=="RETURN_TO_SERVICE").APPROVAL_NOTES="Changed return reason";if(fault=="history")native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="PHYSICAL_RETURN_ACCEPTED").USER_ID="unrecorded-actor";if(fault=="task")native.Rows.OfType<CROSS_PERSONA_TASK>().First(row=>row.ASSIGNED_ROLE=="FacilityRole").TASK_STATUS="CANCELLED";
        if(fault=="delta"){var row=native.Rows.OfType<PROCESS_INSTANCE>().Single();var payload=JsonNode.Parse(row.INSTANCE_DATA_JSON!)!;payload["Data"]!["CompanyWorkOrderReturnAcceptance"]!["Delta"]!["BeforeRevision"]=new string('F',64);row.INSTANCE_DATA_JSON=payload.ToJsonString();}if(fault=="foreign-actor")context=context with{ActorUserId="other-approver"};if(fault=="foreign-intent")reference=reference with{IntentId=Guid.NewGuid()};
        if(fault=="uncertain"){var result=await service.RecoverReturnAcceptanceAsync(context,reference,f.Authority);result.Receipt.Require(command);}else await Assert.ThrowsAnyAsync<Exception>(()=>service.RecoverReturnAcceptanceAsync(context,reference,f.Authority));Assert.Equal(writes,native.Writes);
    }
}
#endif
