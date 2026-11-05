#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMaintenanceAcceptanceTests
{
    internal sealed class Fixture
    {
        internal CompanyProcessStartTests.Fixture Owner=new("MaintenanceRole");
        internal ProductionInterventionMaintenanceLink Link=new(new("tenant","company"),"actor","field","order/العالم%2F","well/الكويت%2F",Guid.NewGuid(),"production-reviewer","Recorded approved scope",DateTime.UtcNow,new string('A',64),new string('B',64),DateTimeOffset.UtcNow);
        internal CompanyMaintenanceProcessStart Start=null!;
        internal FieldSelectionReview Receiver=new(new("tenant","company"),new("lifecycle","fingerprint"),"receiver","receiving-authority","field");
        internal int Guards,RevokeAt;
        internal bool WrongRole;
        internal Fixture()
        {
            Owner.Native.Destinations=["MAINTENANCE","FACILITY"];
            Owner.Native.Personas.Setup(value=>value.GetActivePersonasAsync("SupervisorRole")).ReturnsAsync(["SUPERVISOR"]);
            Owner.Definition.PROCESS_TYPE="WORK_ORDER";Owner.Definition.ENTITY_TYPE="WORK_ORDER";
            Owner.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="receive",StepName="Independent receiving",StepType="APPROVAL",SequenceNumber=1,RequiredRoles=["MaintenanceRole"],RequiresApproval=true},new ProcessStepDefinition{StepId="execute",StepName="Authorized execution",StepType="ACTION",SequenceNumber=2,RequiredRoles=["SupervisorRole"]}}});
            Owner.Native.Source.Setup(value=>value.UpdateEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object row)=>
            {
                Owner.Native.Writes++;if(Owner.Native.Writes==Owner.FailWrite)return new ErrorsInfo{Flag=Errors.Failed};
                var key=table switch{"PROCESS_INSTANCE"=>"PROCESS_INSTANCE_ID","PROCESS_STEP_INSTANCE"=>"PROCESS_STEP_INSTANCE_ID","CROSS_PERSONA_TASK"=>"CROSS_TASK_ID",_=>throw new InvalidOperationException("Unexpected fixture update")};
                var id=row.GetType().GetProperty(key)!.GetValue(row);var matches=Owner.Native.Rows.Where(value=>value.GetType()==row.GetType() && Equals(value.GetType().GetProperty(key)!.GetValue(value),id)).ToArray();
                if(matches.Length!=1)return new ErrorsInfo{Flag=Errors.Failed};
                var index=Owner.Native.Rows.IndexOf(matches[0]);Owner.Native.Rows[index]=JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!;return new ErrorsInfo{Flag=Errors.Ok};
            });
        }
        internal Task Authority(string? role)
        {
            Guards++;if(Guards==RevokeAt || role is not null && (WrongRole || role!="MaintenanceRole"))throw new UnauthorizedAccessException("Current independent receiving role/permission is required.");return Task.CompletedTask;
        }
        internal async Task Ready()
        {
            var definition=await Owner.Service.GetProcessDefinitionAsync("definition");
            Start=new(new(new("tenant","company"),new("lifecycle","fingerprint"),"actor","start-authority","field"),Guid.NewGuid(),"definition",ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),Link);
            await Owner.Service.StartMaintenanceProcessAsync(Start,Owner.Guard,()=>Task.FromResult(Link));
            Link=Link with{Actor="receiver"};
        }
        internal CompanyMaintenanceRecoveryQuery Query()=>new(Start.IntentId,Link.DecisionId,"field",Link.WellId,Link.WorkOrderId);
        internal Task<CompanyMaintenanceAcceptanceReview> Review()=>Owner.Service.ReviewMaintenanceAcceptanceAsync(Receiver,Query(),Link,Authority);
        internal Task<ProductionInterventionMaintenanceLink> Read(){PPDMDataSourceTransaction.RequireIdle(Owner.Native.Source.Object);return Task.FromResult(Link with{RetrievedAtUtc=DateTimeOffset.UtcNow});}
        internal Task<CompanyMaintenanceAcceptanceReceipt> Accept(CompanyMaintenanceAcceptanceCommand command)=>Owner.Service.AcceptMaintenanceAsync(command,Authority,Read);
        internal Task<CompanyMaintenanceAcceptanceReceipt> Recover(CompanyMaintenanceAcceptanceCommand command)=>Owner.Service.RecoverMaintenanceAcceptanceAsync(command,Authority,Read);
    }
    [Fact] public async Task IndependentReceivingAcceptanceCompletesOriginalTasksAuditsApprovalAndRoutesNextRoleOnce()
    {
        var f=new Fixture();await f.Ready();var review=await f.Review();review.Require();Assert.Equal("MaintenanceRole",review.AssignedRole);var command=new CompanyMaintenanceAcceptanceCommand(review,Guid.NewGuid(),"Independent receiving review accepted");
        var initial=f.Owner.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Recover(command));Assert.Equal(initial,f.Owner.Native.Writes);
        var receipt=await f.Accept(command);receipt.Require(command);Assert.Equal(initial+9,f.Owner.Native.Writes);Assert.Equal(2,receipt.CompletedTaskIds.Count);Assert.Single(receipt.NextTaskIds);
        var process=f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single();Assert.Equal("execute",process.CURRENT_STEP_ID);Assert.Equal("IN_PROGRESS",process.STATUS);Assert.Equal("actor",process.ROW_CREATED_BY);Assert.Equal("receiver",process.ROW_CHANGED_BY);
        var approval=f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Single();Assert.Equal("ACCEPTED",approval.APPROVAL_STATUS);Assert.Equal("receiver",approval.APPROVED_BY);Assert.Equal("actor",approval.REQUESTED_BY);
        Assert.Equal(2,f.Owner.Native.Rows.OfType<PROCESS_HISTORY>().Count());Assert.All(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(task=>receipt.CompletedTaskIds.Contains(task.CROSS_TASK_ID)),task=>{Assert.Equal("COMPLETED",task.TASK_STATUS);Assert.Equal("actor",task.ROW_CREATED_BY);Assert.Equal("receiver",task.ROW_CHANGED_BY);});
        Assert.Equal("SupervisorRole",f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(task=>receipt.NextTaskIds.Contains(task.CROSS_TASK_ID)).ASSIGNED_ROLE);
        var recovered=await f.Recover(command);Assert.Equal(receipt.RecordedRevision,recovered.RecordedRevision);var retry=await f.Accept(command);Assert.Equal(receipt.RecordedRevision,retry.RecordedRevision);Assert.Equal(initial+9,f.Owner.Native.Writes);
    }
    [Theory][InlineData("allowed")][InlineData("missing-command")][InlineData("foreign-actor")][InlineData("company")][InlineData("role")][InlineData("reference")][InlineData("field")][InlineData("order")]
    public async Task IncomingTaskDiscoveryAndPrivateAcceptanceReferenceRestoreNoRowsOrForeignCommands(string fault)
    {
        var f=new Fixture();await f.Ready();var command=new CompanyMaintenanceAcceptanceCommand(await f.Review(),Guid.NewGuid(),"Original receiving rationale");
        if(fault!="missing-command")await f.Accept(command);
        var task=new CompanyMaintenanceReceivingQuery(f.Start.InstanceId(),"field",f.Link.WorkOrderId);
        var query=new CompanyMaintenanceAcceptanceReferenceQuery(task,fault=="reference"?Guid.NewGuid():command.IntentId);
        var context=f.Receiver;
        if(fault=="foreign-actor")context=context with{ActorUserId="other-receiver"};
        if(fault=="company")context=context with{Company=new("tenant","foreign-company")};
        if(fault=="role")f.WrongRole=true;if(fault=="field")query=query with{Task=task with{FieldId="other-field"}};if(fault=="order")query=query with{Task=task with{WorkOrderId="other-order"}};
        var before=JsonSerializer.Serialize(f.Owner.Native.Rows);var writes=f.Owner.Native.Writes;
        if(fault=="allowed")
        {
            var assignment=await f.Owner.Service.DiscoverMaintenanceReceivingAssignmentAsync(context,task,f.Authority);Assert.Equal(f.Query(),assignment);
            var restored=await f.Owner.Service.ReadOriginalMaintenanceAcceptanceAsync(context,query,f.Authority);Assert.Equal(command,restored);
            var recovery=new CompanyMaintenanceAcceptanceRecovery(restored,await f.Recover(restored));recovery.Require(context.Company,context.ActorUserId,query);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.Service.ReadOriginalMaintenanceAcceptanceAsync(context,query,f.Authority));
        Assert.Equal(writes,f.Owner.Native.Writes);Assert.Equal(before,JsonSerializer.Serialize(f.Owner.Native.Rows));
    }
    [Theory][InlineData("starter")][InlineData("production-reviewer")][InlineData("role")][InlineData("source")][InlineData("missing-task")][InlineData("definition")][InlineData("borrowed")]
    public async Task InvalidReceivingActorRoleOriginalSourceAssignmentOrDefinitionCannotWrite(string fault)
    {
        var f=new Fixture();await f.Ready();var writes=f.Owner.Native.Writes;
        if(fault=="starter"){f.Receiver=f.Receiver with{ActorUserId="actor"};f.Link=f.Link with{Actor="actor"};}
        if(fault=="production-reviewer"){f.Receiver=f.Receiver with{ActorUserId="production-reviewer"};f.Link=f.Link with{Actor="production-reviewer"};}
        if(fault=="role")f.WrongRole=true;if(fault=="source")f.Link=f.Link with{WorkOrderRevision=new string('C',64)};
        if(fault=="missing-task")f.Owner.Native.Rows.Remove(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().First());
        if(fault=="definition")f.Owner.Definition.VERSION="changed";if(fault=="borrowed")f.Owner.Native.Borrowed=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Review());Assert.Equal(writes,f.Owner.Native.Writes);Assert.Empty(f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>());
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)]
    public async Task EveryAcceptanceWriteFailureRollsBackTasksGraphApprovalAndHistory(int offset)
    {
        var f=new Fixture();await f.Ready();var command=new CompanyMaintenanceAcceptanceCommand(await f.Review(),Guid.NewGuid(),"Recorded independent receiving rationale");
        var before=JsonSerializer.Serialize(f.Owner.Native.Rows);f.Owner.FailWrite=f.Owner.Native.Writes+offset;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Accept(command));Assert.Equal(before,JsonSerializer.Serialize(f.Owner.Native.Rows));Assert.Empty(f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>());
    }
    [Fact] public async Task LostAcceptanceCommitResponseCanBeConfirmedReadOnlyOnlyWithTheSameOriginalIntent()
    {
        var f=new Fixture();await f.Ready();var command=new CompanyMaintenanceAcceptanceCommand(await f.Review(),Guid.NewGuid(),"Original receiving acceptance");
        f.Owner.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Accept(command));var writes=f.Owner.Native.Writes;f.Owner.Native.FailCommit=false;
        var receipt=await f.Recover(command);receipt.Require(command);Assert.Equal(writes,f.Owner.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Recover(command with{Notes="Different rationale"}));await Assert.ThrowsAnyAsync<Exception>(()=>f.Accept(command with{IntentId=Guid.NewGuid()}));Assert.Equal(writes,f.Owner.Native.Writes);
    }
    [Theory][InlineData("revoke-during-write")][InlineData("source-after-commit")][InlineData("next-destinations")][InlineData("missing-task")][InlineData("missing-approval")][InlineData("history")][InlineData("forged-approval-audit")][InlineData("inactive-approval")]
    public async Task ChangedAuthoritySourceOrRecordedAcceptanceEvidenceNeverPublishesOrRepairs(string fault)
    {
        var f=new Fixture();await f.Ready();var command=new CompanyMaintenanceAcceptanceCommand(await f.Review(),Guid.NewGuid(),"Original independent receiving rationale");
        if(fault=="revoke-during-write")
        {
            var before=JsonSerializer.Serialize(f.Owner.Native.Rows);f.RevokeAt=f.Guards+5;
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Accept(command));Assert.Equal(before,JsonSerializer.Serialize(f.Owner.Native.Rows));return;
        }
        if(fault is "source-after-commit" or "next-destinations")
        {
            f.Owner.Native.AfterCommit=()=>
            {
                if(fault=="source-after-commit")f.Link=f.Link with{WorkOrderRevision=new string('C',64)};
                else f.Owner.Native.Personas.Setup(value=>value.GetActivePersonasAsync("SupervisorRole")).ReturnsAsync(["OTHER_SUPERVISOR"]);
            };
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Accept(command));
        }
        else await f.Accept(command);
        var writes=f.Owner.Native.Writes;
        if(fault=="missing-task")f.Owner.Native.Rows.Remove(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().First());
        if(fault=="missing-approval")f.Owner.Native.Rows.Remove(f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Single());
        if(fault=="history")f.Owner.Native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="RECEIVING_ACCEPTED").USER_ID="intruder";
        if(fault=="forged-approval-audit")
        {
            f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVED_BY="intruder";
            var process=f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single();var payload=System.Text.Json.Nodes.JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;
            payload["Data"]!["CompanyMaintenanceAcceptance"]!["Approval"]!["APPROVED_BY"]="intruder";process.INSTANCE_DATA_JSON=payload.ToJsonString();
        }
        if(fault=="inactive-approval")
        {
            f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Single().ACTIVE_IND="N";
            var process=f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single();var payload=System.Text.Json.Nodes.JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;
            payload["Data"]!["CompanyMaintenanceAcceptance"]!["Approval"]!["ACTIVE_IND"]="N";process.INSTANCE_DATA_JSON=payload.ToJsonString();
        }
        var unchanged=JsonSerializer.Serialize(f.Owner.Native.Rows);await Assert.ThrowsAnyAsync<Exception>(()=>f.Recover(command));Assert.Equal(writes,f.Owner.Native.Writes);Assert.Equal(unchanged,JsonSerializer.Serialize(f.Owner.Native.Rows));
    }
}
#endif
