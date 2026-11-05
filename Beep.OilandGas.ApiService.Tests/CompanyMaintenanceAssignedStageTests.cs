#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyMaintenanceAssignedStageTests
{
    [Fact] public async Task CurrentNextRoleCanReadRecordedReceivingAcceptanceWithoutPrivateCommandOrAnotherWrite()
    {
        var f=new CompanyMaintenanceAcceptanceTests.Fixture();await f.Ready();var command=new CompanyMaintenanceAcceptanceCommand(await f.Review(),Guid.NewGuid(),"Private original receiving notes");var receipt=await f.Accept(command);var writes=f.Owner.Native.Writes;var calls=0;
        var current=f.Receiver with{ActorUserId="supervisor",Revision="supervisor-authority"};var task=new CompanyMaintenanceReceivingQuery(command.Review.InstanceId,"field",f.Link.WorkOrderId);
        Task Authority(string? role){calls++;if(role is not null)Assert.Equal("SupervisorRole",role);return Task.CompletedTask;}
        f.Link=f.Link with{Reason="Later production review"};
        var result=await f.Owner.Service.ReviewMaintenanceAssignedStageAsync(current,task,Authority);result.Require(current.Company,current.ActorUserId,task);
        Assert.Equal("execute",result.StepId);Assert.Equal("Authorized execution",result.StepName);Assert.Equal("SupervisorRole",result.AssignedRole);Assert.Equal("receiver",result.ReceivingActor);Assert.Equal(receipt.NextStepInstanceId,result.StepInstanceId);Assert.Equal(receipt.HistoryId,result.ReceivingHistoryId);Assert.Equal(receipt.ApprovalId,result.ReceivingApprovalId);Assert.Equal(receipt.RecordedRevision,result.ProcessRevision);Assert.True(calls>2);Assert.Equal(writes,f.Owner.Native.Writes);PPDMDataSourceTransaction.RequireIdle(f.Owner.Native.Source.Object);
        var json=System.Text.Json.JsonSerializer.Serialize(result);Assert.DoesNotContain(command.Notes,json);Assert.DoesNotContain(command.IntentId.ToString(),json);
    }
    [Theory][InlineData("company")][InlineData("database")][InlineData("fingerprint")][InlineData("field")][InlineData("order")][InlineData("role")][InlineData("revoke")][InlineData("missing-task")][InlineData("approval")][InlineData("assignee")][InlineData("definition")][InlineData("personas")][InlineData("snapshot")][InlineData("malformed")][InlineData("missing-command")]
    public async Task ChangedNextRoleOwnershipGraphOrAuthorityCannotPublishStageEvidence(string fault)
    {
        var f=new CompanyMaintenanceAcceptanceTests.Fixture();await f.Ready();var command=new CompanyMaintenanceAcceptanceCommand(await f.Review(),Guid.NewGuid(),"Original receiving scope");await f.Accept(command);var writes=f.Owner.Native.Writes;
        var current=f.Receiver with{ActorUserId="supervisor",Revision="supervisor-authority"};var task=new CompanyMaintenanceReceivingQuery(command.Review.InstanceId,"field",f.Link.WorkOrderId);
        if(fault=="company")current=current with{Company=new("tenant","foreign")};if(fault=="database")current=current with{Database=new("foreign","fingerprint")};if(fault=="field")task=task with{FieldId="foreign"};if(fault=="order")task=task with{WorkOrderId="foreign"};
        if(fault=="fingerprint")current=current with{Database=current.Database with{Fingerprint="foreign"}};
        if(fault=="snapshot")f.Owner.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="execute").STEP_NAME="Changed transient stage";
        if(fault is "malformed" or "missing-command")
        {
            var row=f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single();var payload=System.Text.Json.Nodes.JsonNode.Parse(row.INSTANCE_DATA_JSON!)!;
            if(fault=="malformed")payload["Data"]!["CompanyMaintenanceAcceptance"]="malformed";else payload["Data"]!["CompanyMaintenanceAcceptance"]!["Command"]=null;
            row.INSTANCE_DATA_JSON=payload.ToJsonString();
        }
        if(fault=="missing-task")f.Owner.Native.Rows.Remove(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.ASSIGNED_ROLE=="SupervisorRole"));
        if(fault=="approval")f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVED_BY="foreign";
        if(fault=="assignee")f.Owner.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="execute").ASSIGNED_TO="foreign";
        if(fault=="definition")f.Owner.Definition.PROCESS_NAME="Changed definition";
        if(fault=="personas")f.Owner.Native.Personas.Setup(value=>value.GetActivePersonasAsync("SupervisorRole")).ReturnsAsync(["FOREIGN_PERSONA"]);
        var calls=0;Task Authority(string? role){calls++;if(fault=="role" && role is not null || fault=="revoke" && calls>=3)throw new UnauthorizedAccessException("Current assigned role refused");return Task.CompletedTask;}
        var error=await Record.ExceptionAsync(()=>f.Owner.Service.ReviewMaintenanceAssignedStageAsync(current,task,Authority));Assert.True(error is InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected stage publication");Assert.Equal(writes,f.Owner.Native.Writes);PPDMDataSourceTransaction.RequireIdle(f.Owner.Native.Source.Object);
    }
    [Fact] public async Task InitialAssignmentCannotBecomeAnAcceptedNextRoleReview()
    {
        var f=new CompanyMaintenanceAcceptanceTests.Fixture();await f.Ready();var writes=f.Owner.Native.Writes;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.Service.ReviewMaintenanceAssignedStageAsync(f.Receiver,new(f.Start.InstanceId(),"field",f.Link.WorkOrderId),_=>Task.CompletedTask));Assert.Equal(writes,f.Owner.Native.Writes);
    }
}
#endif
