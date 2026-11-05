using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMaintenanceReturnHandoffTests
{
    internal sealed class Fixture
    {
        internal readonly CompanyWorkOrderCompletionDecisionTests.Fixture Flow=new();
        internal CompanyWorkOrderCompletionDecisionRecovery Decision=null!;
        internal CompanyMaintenanceReturnHandoffCommand Command=null!;
        internal async Task Ready(bool realAudit=false,bool returnAcceptance=false,bool assetReturn=false,bool multipleWells=false)
        {
            await Flow.Ready(realAudit:realAudit,returnRole:"FacilityRole",returnAcceptance:returnAcceptance,assetReturn:assetReturn,multipleWells:multipleWells);Flow.Flow.Flow.Owner.Native.Personas.Setup(value=>value.GetActivePersonasAsync("FacilityRole")).ReturnsAsync(["FACILITY","PRODUCTION"]);
            Decision=await Flow.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(new(Flow.Review,Guid.NewGuid(),"VERIFIED","Original verified physical evidence"),Flow.Authority,Flow.Read);
            var source=Flow.Review.Completion;var utc=DateTime.UtcNow;Flow.Review=Flow.Review with{Completion=source with{ComparedAtUtc=utc,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(source.Context,source.Report,source.Order,source.Requirements,source.Identities,source.ReviewerPerson,utc)}};
            Command=new(await Flow.Flow.Flow.Owner.Service.ReviewReturnHandoffAsync(Flow.Review.Stage.Context,source.Report.Query,Flow.Review.Completion,Flow.Authority),Guid.NewGuid(),"Verified work routed to responsible return approver");Command.Require();
        }
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualHandoffRoutesCompleteReturnTasksAndPreservesOriginalDecisionAndExecutorRecovery(bool realAudit)
    {
        var f=new Fixture();await f.Ready(realAudit);var service=f.Flow.Flow.Flow.Owner.Service;var native=f.Flow.Flow.Flow.Owner.Native;var physical=JsonSerializer.Serialize(native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single());var approvals=JsonSerializer.Serialize(native.Rows.OfType<PROCESS_APPROVAL>().ToArray());var writes=native.Writes;
        var saved=await service.RecordReturnHandoffAsync(f.Command,f.Flow.Authority,f.Flow.Read);saved.Require(f.Command.Review.Stage.Context.Company,f.Command.Review.Stage.Context.ActorUserId,f.Command.Reference());Assert.Equal(2,saved.Receipt.ClosedTaskIds.Count);Assert.Equal(2,saved.Receipt.ReturnTaskIds.Count);Assert.Equal(writes+9,native.Writes);writes=native.Writes;
        Assert.Equal("return",native.Rows.OfType<PROCESS_INSTANCE>().Single().CURRENT_STEP_ID);Assert.All(native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>saved.Receipt.ClosedTaskIds.Contains(row.CROSS_TASK_ID)),row=>Assert.Equal("COMPLETED",row.TASK_STATUS));Assert.All(native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>saved.Receipt.ReturnTaskIds.Contains(row.CROSS_TASK_ID)),row=>Assert.Equal("PENDING",row.TASK_STATUS));
        Assert.Equal(physical,JsonSerializer.Serialize(native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single()));Assert.Equal(approvals,JsonSerializer.Serialize(native.Rows.OfType<PROCESS_APPROVAL>().ToArray()));
        Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(await service.RecoverReturnHandoffAsync(f.Command.Review.Stage.Context,f.Command.Reference(),f.Flow.Authority)));Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(await service.RecordReturnHandoffAsync(f.Command,f.Flow.Authority,f.Flow.Read)));
        Assert.Equal(JsonSerializer.Serialize(f.Decision),JsonSerializer.Serialize(await service.RecoverCompletionDecisionAsync(f.Decision.Command.Review.Stage.Context,f.Decision.Command.Reference(),f.Flow.Authority)));
        Assert.Equal(JsonSerializer.Serialize(f.Flow.Handoff),JsonSerializer.Serialize(await service.RecoverExecutionHandoffAsync(f.Flow.Handoff.Command.Review.Stage.Context,f.Flow.Handoff.Command.Reference(),f.Flow.Flow.Authority)));
        var publicFacts=await service.ReadCompletionDecisionFactsAsync(f.Command.Review.Stage.Context,f.Command.Reference().Query,()=>Task.CompletedTask);Assert.Equal("VERIFIED",publicFacts!.Decision);Assert.DoesNotContain(f.Command.Reason,JsonSerializer.Serialize(publicFacts));
        var actor=f.Command.Review.Stage.Context with{ActorUserId="return-approver",Revision="current-return-authority"};Task Authority(string? role){if(role is not null)Assert.Equal("FacilityRole",role);return Task.CompletedTask;}
        var stage=await service.ReviewMaintenanceAssignedStageAsync(actor,f.Command.Reference().Query.Task,Authority);Assert.Equal("return",stage.StepId);Assert.Equal("FacilityRole",stage.AssignedRole);Assert.Equal(writes,native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.RecordReturnHandoffAsync(f.Command with{IntentId=Guid.NewGuid()},f.Flow.Authority,f.Flow.Read));Assert.Equal(writes,native.Writes);
    }
    [Theory][InlineData("source")][InlineData("owned-source")][InlineData("authority")][InlineData("uncertain")][InlineData("no-personas")]
    public async Task ChangedSourceAuthorityOrUncertainAcknowledgementCannotCreateReplacement(string fault)
    {
        var f=new Fixture();await f.Ready();var native=f.Flow.Flow.Flow.Owner.Native;var service=f.Flow.Flow.Flow.Owner.Service;var writes=native.Writes;
        if(fault=="source")f.Flow.Changed=true;if(fault=="owned-source")f.Flow.ChangedOwned=true;if(fault=="authority")f.Flow.Revoked=true;if(fault=="uncertain")native.FailCommit=true;if(fault=="no-personas")native.Personas.Setup(value=>value.GetActivePersonasAsync("FacilityRole")).ReturnsAsync([]);
        await Assert.ThrowsAnyAsync<Exception>(()=>service.RecordReturnHandoffAsync(f.Command,f.Flow.Authority,f.Flow.Read));
        if(fault=="uncertain"){native.FailCommit=false;var saved=await service.RecoverReturnHandoffAsync(f.Command.Review.Stage.Context,f.Command.Reference(),f.Flow.Authority);saved.Receipt.Require(f.Command);}else Assert.Equal(writes,native.Writes);
    }
    [Theory][InlineData("task")][InlineData("step")][InlineData("history")][InlineData("extra-task")][InlineData("before")][InlineData("actor")][InlineData("intent")][InlineData("old-decision")]
    public async Task RecoveryAndPublicDecisionRejectUnrecordedLaterGraphChanges(string fault)
    {
        var f=new Fixture();await f.Ready();var native=f.Flow.Flow.Flow.Owner.Native;var service=f.Flow.Flow.Flow.Owner.Service;await service.RecordReturnHandoffAsync(f.Command,f.Flow.Authority,f.Flow.Read);var reference=f.Command.Reference();var context=f.Command.Review.Stage.Context;var writes=native.Writes;
        if(fault=="task")native.Rows.OfType<CROSS_PERSONA_TASK>().First(row=>row.ASSIGNED_ROLE=="FacilityRole").TASK_STATUS="CANCELLED";
        if(fault=="step")native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="return").COMPLETED_BY="unrecorded";
        if(fault=="history")native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="VERIFIED_WORK_ROUTED").USER_ID="other-actor";
        if(fault=="old-decision")native.Rows.OfType<PROCESS_APPROVAL>().Single(row=>row.APPROVAL_TYPE=="WORK_COMPLETION").APPROVAL_NOTES="Changed original decision";
        if(fault=="extra-task"){var copy=CompanyInventorySourceReview.Copy(native.Rows.OfType<CROSS_PERSONA_TASK>().First(row=>row.ASSIGNED_ROLE=="FacilityRole"));copy.CROSS_TASK_ID="unrecorded-return-task";native.Rows.Add(copy);}
        if(fault=="before"){var row=native.Rows.OfType<PROCESS_INSTANCE>().Single();var payload=JsonNode.Parse(row.INSTANCE_DATA_JSON!)!;payload["Data"]!["CompanyMaintenanceReturnHandoff"]!["Delta"]!["BeforeRevision"]=new string('F',64);row.INSTANCE_DATA_JSON=payload.ToJsonString();}
        if(fault=="actor")context=context with{ActorUserId="foreign-reviewer"};if(fault=="intent")reference=reference with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>service.RecoverReturnHandoffAsync(context,reference,f.Flow.Authority));
        if(fault is not ("actor" or "intent"))await Assert.ThrowsAnyAsync<Exception>(()=>service.ReadCompletionDecisionFactsAsync(f.Command.Review.Stage.Context,f.Command.Reference().Query,()=>Task.CompletedTask));Assert.Equal(writes,native.Writes);
    }
}
#endif
