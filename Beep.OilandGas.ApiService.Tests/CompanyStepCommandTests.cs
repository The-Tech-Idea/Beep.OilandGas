#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyStepCommandTests
{
    private static object Clone(object value)=>JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!;
    internal static async Task<(CompanyProcessStartTests.Fixture Native,CompanyAccountingStepCommand Command)> Prepare(bool terminal=false,bool policy=false,bool sameRole=false,string type="AP_INVOICE")
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");
        native.Definition.ENTITY_TYPE=type;
        if(terminal)
        {
            var config=System.Text.Json.Nodes.JsonNode.Parse(native.Definition.PROCESS_CONFIG_JSON!)!;
            config["Steps"]!.AsArray().RemoveAt(1);native.Definition.PROCESS_CONFIG_JSON=config.ToJsonString();
        }
        if(policy)
        {
            var config=System.Text.Json.Nodes.JsonNode.Parse(native.Definition.PROCESS_CONFIG_JSON!)!;
            config["Steps"]![0]!["StepConfiguration"]=new System.Text.Json.Nodes.JsonObject{["RecordedPolicy"]="configured"};native.Definition.PROCESS_CONFIG_JSON=config.ToJsonString();
        }
        if(sameRole)
        {
            var config=System.Text.Json.Nodes.JsonNode.Parse(native.Definition.PROCESS_CONFIG_JSON!)!;
            config["Steps"]![1]!["RequiredRoles"]=new System.Text.Json.Nodes.JsonArray("ConfiguredCompanyRole");native.Definition.PROCESS_CONFIG_JSON=config.ToJsonString();
        }
        native.Native.Personas.Setup(value=>value.GetActivePersonasAsync("Controller")).ReturnsAsync(new List<string>{"CONTROLLER","FINANCE"});
        ConfigureUpdates(native);
        string[] permissions=["Accounting.View","Procurement.View","Workflow.Start","Workflow.Approve"];
        await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:permissions);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var start=await client.ReviewCompanyAccountingStartAsync(new("definition",type,type=="AP_INVOICE"?"invoice":"order"));
        var receipt=await native.Start(start.CreateIntent(Guid.NewGuid().ToString("N")) with{ActorUserId="starter"});
        var review=(await client.ReviewCompanyAccountingStepAsync(receipt.InstanceId))!;
        return(native,new(review,Guid.NewGuid().ToString("N"),"Reviewed recorded source and completed workflow review."));
    }
    internal static void ConfigureUpdates(CompanyProcessStartTests.Fixture native)
    {
        native.Native.Source.Setup(source=>source.UpdateEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>{
            native.Native.Writes++;if(native.Native.Writes==native.FailWrite)return new ErrorsInfo{Flag=Errors.Failed};
            var key=table switch{"PROCESS_INSTANCE"=>"PROCESS_INSTANCE_ID","PROCESS_STEP_INSTANCE"=>"PROCESS_STEP_INSTANCE_ID","CROSS_PERSONA_TASK"=>"CROSS_TASK_ID",_=>throw new InvalidOperationException(table)};
            var id=value.GetType().GetProperty(key)!.GetValue(value);var index=native.Native.Rows.FindIndex(row=>row.GetType().Name==table && Equals(row.GetType().GetProperty(key)!.GetValue(row),id));
            if(index<0)return new ErrorsInfo{Flag=Errors.Failed};var saved=Clone(value);
            if(native.Native.CorruptReadback && saved is PROCESS_INSTANCE process)process.ROW_CREATED_BY="changed";
            native.Native.Rows[index]=saved;return new ErrorsInfo{Flag=Errors.Ok};});
    }
    [Fact] public async Task CompleteGraphHistoryAndPersonaHandoffCommitAndExactRetryDoesNotWrite()
    {
        var (native,command)=await Prepare();var receipt=await native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask);receipt.Require(command);
        var process=await native.Service.GetProcessInstanceAsync(command.Review.InstanceId);Assert.Equal("post",process!.CurrentStepId);Assert.Equal(2,process.History.Count);
        Assert.Equal(StepStatus.COMPLETED,process.StepInstances.Single(row=>row.StepId=="review").Status);Assert.Equal(StepStatus.PENDING,process.StepInstances.Single(row=>row.StepId=="post").Status);
        Assert.Equal(2,receipt.CompletedTaskIds.Count);Assert.Equal(2,receipt.NextTaskIds.Count);Assert.Equal(16,native.Native.Writes);
        var approval=Assert.Single(native.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal("APPROVED",approval.APPROVAL_STATUS);Assert.Equal("actor",approval.APPROVED_BY);Assert.Equal(command.Notes,approval.APPROVAL_NOTES);
        Assert.All(native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>receipt.CompletedTaskIds.Contains(row.CROSS_TASK_ID)),row=>Assert.Equal("COMPLETED",row.TASK_STATUS));
        var retry=await native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask);Assert.Equal(JsonSerializer.Serialize(receipt),JsonSerializer.Serialize(retry));Assert.Equal(16,native.Native.Writes);
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)]
    public async Task EveryWriteFailureRollsBackCompleteOriginalHandoff(int offset)
    {
        var (native,command)=await Prepare();var before=JsonSerializer.Serialize(native.Native.Rows);native.FailWrite=native.Native.Writes+offset;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask));
        Assert.Equal(before,JsonSerializer.Serialize(native.Native.Rows));Assert.Equal("rollback",native.Native.Transactions.Last());
    }
    [Fact] public async Task FinalStepClosesProcessAndTasksWithoutInventingNextRecipient()
    {
        var (native,command)=await Prepare(true);var receipt=await native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask);
        Assert.Null(receipt.NextStepInstanceId);Assert.Empty(receipt.NextTaskIds);Assert.Equal(2,receipt.CompletedTaskIds.Count);
        var process=await native.Service.GetProcessInstanceAsync(command.Review.InstanceId);Assert.Equal(ProcessStatus.COMPLETED,process!.Status);Assert.Equal("actor",process.CompletedBy);Assert.Equal(2,process.History.Count);
        var writes=native.Native.Writes;await native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask);Assert.Equal(writes,native.Native.Writes);
    }
    [Fact] public async Task ConfiguredPolicyCannotBeSilentlyBypassedBySequentialCompletion()
    {
        var (native,command)=await Prepare(policy:true);var before=JsonSerializer.Serialize(native.Native.Rows);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask));
        Assert.Equal(before,JsonSerializer.Serialize(native.Native.Rows));Assert.Equal(6,native.Native.Writes);
    }
    [Fact] public async Task NextRecipientCanReviewAndCompleteTerminalStepAndOldIntentCannotAdoptProgress()
    {
        var (native,command)=await Prepare(sameRole:true);await native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask);
        string[] permissions=["Accounting.View","Workflow.Start","Workflow.Approve","Workflow.Execute"];
        await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:permissions);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var review=(await client.ReviewCompanyAccountingStepAsync(command.Review.InstanceId))!;Assert.True(review.Eligible);Assert.Equal("Workflow.Execute",review.RequiredPermission);
        var second=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Completed recorded next workflow step without financial posting.");
        var receipt=await native.Service.CompleteCompanyAccountingStepAsync(second,()=>Task.CompletedTask);Assert.Empty(receipt.NextTaskIds);
        var process=await native.Service.GetProcessInstanceAsync(review.InstanceId);Assert.Equal(ProcessStatus.COMPLETED,process!.Status);Assert.Equal(3,process.History.Count);
        Assert.Single(native.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.All(native.Native.Rows.OfType<CROSS_PERSONA_TASK>(),row=>Assert.Equal("COMPLETED",row.TASK_STATUS));
        var writes=native.Native.Writes;await native.Service.CompleteCompanyAccountingStepAsync(second,()=>Task.CompletedTask);Assert.Equal(writes,native.Native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask));Assert.Equal(writes,native.Native.Writes);
    }
    [Fact] public async Task ProviderAuditCorruptionCannotProduceConfirmedCommand()
    {
        var (native,command)=await Prepare();var before=JsonSerializer.Serialize(native.Native.Rows);native.Native.CorruptReadback=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask));Assert.Equal(before,JsonSerializer.Serialize(native.Native.Rows));
    }
    [Fact] public async Task LostCommittedTaskEvidenceRefusesSuccessAndRetainsRecordedCommand()
    {
        var (native,command)=await Prepare();native.Native.DropCommittedTasks=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask));
        Assert.Equal("commit",native.Native.Transactions.Last());Assert.Contains("CompanyAccountingStepCommand",native.Native.Rows.OfType<PROCESS_INSTANCE>().Single().INSTANCE_DATA_JSON);
        var writes=native.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask));Assert.Equal(writes,native.Native.Writes);
    }
    [Fact] public async Task InTransactionAuthorityLossRollsBackClosedTasksAndGraph()
    {
        var (native,command)=await Prepare();var before=JsonSerializer.Serialize(native.Native.Rows);var calls=0;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>native.Service.CompleteCompanyAccountingStepAsync(command,()=>++calls>=3?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask));
        Assert.Equal(before,JsonSerializer.Serialize(native.Native.Rows));Assert.Equal("rollback",native.Native.Transactions.Last());
    }
    [Theory][InlineData("notes")][InlineData("graph")][InlineData("tasks")][InlineData("definition")][InlineData("approval")]
    public async Task ChangedRetainedIntentOrRecordedOutcomeCannotBeAdoptedOrRepaired(string fault)
    {
        var (native,command)=await Prepare();var receipt=await native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask);var writes=native.Native.Writes;
        switch(fault)
        {
            case "notes":command=command with{Notes="changed"};break;
            case "graph":native.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="post").STATUS="COMPLETED";break;
            case "tasks":native.Native.Rows.RemoveAll(row=>row is CROSS_PERSONA_TASK task && receipt.NextTaskIds.Contains(task.CROSS_TASK_ID));break;
            case "definition":native.Definition.VERSION="2.0.0";break;
            case "approval":native.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVED_BY="different";break;
        }
        await Assert.ThrowsAsync<InvalidOperationException>(()=>native.Service.CompleteCompanyAccountingStepAsync(command,()=>Task.CompletedTask));Assert.Equal(writes,native.Native.Writes);
    }
}
#endif
