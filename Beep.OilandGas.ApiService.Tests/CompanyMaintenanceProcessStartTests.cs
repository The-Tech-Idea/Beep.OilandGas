using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyMaintenanceProcessStartTests
{
    private sealed class Fixture
    {
        internal CompanyProcessStartTests.Fixture Owner=new("MaintenanceRole");
        internal ProductionInterventionMaintenanceLink Source=new(new("tenant","company"),"actor","field","order/العالم%2F","well/الكويت%2F",Guid.NewGuid(),"original-reviewer","Original approved intervention",DateTime.UtcNow,new string('A',64),new string('B',64),DateTimeOffset.UtcNow);
        internal int Reads;
        internal Fixture()
        {
            Owner.Definition.PROCESS_TYPE="WORK_ORDER";Owner.Definition.ENTITY_TYPE="WORK_ORDER";
            Owner.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{
                new Beep.OilandGas.Models.Processes.ProcessStepDefinition{StepId="receive",StepName="Receiving review",StepType="APPROVAL",SequenceNumber=1,RequiredRoles=["MaintenanceRole"],RequiresApproval=true},
                new Beep.OilandGas.Models.Processes.ProcessStepDefinition{StepId="execute",StepName="Authorized work",StepType="ACTION",SequenceNumber=2,RequiredRoles=["SupervisorRole"]}}});
        }
        internal async Task<CompanyMaintenanceProcessStart> Intent()
        {
            var definition=await Owner.Service.GetProcessDefinitionAsync("definition");
            return new(new(new("tenant","company"),new("lifecycle","fingerprint"),"actor","binding-review","field"),Guid.NewGuid(),"definition",ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),Source);
        }
        internal Task<ProductionInterventionMaintenanceLink> Read()
        {
            PPDMDataSourceTransaction.RequireIdle(Owner.Native.Source.Object);Reads++;
            return Task.FromResult(Source with{RetrievedAtUtc=DateTimeOffset.UtcNow});
        }
        internal Task<CompanyMaintenanceProcessStartReceipt> Start(CompanyMaintenanceProcessStart intent)=>Owner.Service.StartMaintenanceProcessAsync(intent,Owner.Guard,Read);
    }
    [Theory][InlineData("blank-name")][InlineData("wrong-key")]
    public async Task NativeCommandOwnerRejectsAnInvalidDefinitionEvenWithAMatchingClientRevision(string fault)
    {
        var f=new Fixture();var intent=await f.Intent();
        if(fault=="blank-name")f.Owner.Definition.PROCESS_NAME=" ";
        else f.Owner.Definition.PROCESS_DEFINITION_ID="foreign-definition";
        var definition=await f.Owner.Service.GetProcessDefinitionAsync(f.Owner.Definition.PROCESS_DEFINITION_ID!);
        intent=intent with{DefinitionRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))};
        f.Owner.Native.Source.Setup(value=>value.GetEntityAsync("PROCESS_DEFINITION",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{f.Owner.Definition});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Start(intent));
        Assert.Equal(0,f.Owner.Native.Writes);Assert.Empty(f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>());Assert.Empty(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>());
        Assert.Equal(new[]{"begin","rollback"},f.Owner.Native.Transactions);
    }
    [Fact] public async Task OriginalMaintenanceAssignmentCommitsWholeGraphAndTasksAndRetryDoesNotWrite()
    {
        var f=new Fixture();var intent=await f.Intent();var receipt=await f.Start(intent);receipt.Require(intent);
        Assert.Equal(6,f.Owner.Native.Writes);Assert.Equal(new[]{"begin","commit"},f.Owner.Native.Transactions);Assert.Equal(2,f.Reads);
        var process=Assert.Single(f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>());Assert.Equal("field",process.FIELD_ID);Assert.Equal(intent.Source.WorkOrderId,process.ENTITY_ID);
        var stored=CompanyMaintenanceProcessStart.FromPayload(process.INSTANCE_DATA_JSON!,process.PROCESS_INSTANCE_ID!,"field",intent.Source.WorkOrderId);Assert.Equal(intent,stored);
        Assert.Equal(2,f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().Count());Assert.All(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>(),task=>{Assert.Equal("MaintenanceRole",task.ASSIGNED_ROLE);Assert.Equal("PENDING",task.TASK_STATUS);});
        var retry=await f.Start(intent);Assert.Equal(receipt.RecordedRevision,retry.RecordedRevision);Assert.Equal(6,f.Owner.Native.Writes);Assert.Equal(4,f.Reads);
    }
    [Theory][InlineData("allowed")][InlineData("missing")][InlineData("actor")][InlineData("company")][InlineData("field")][InlineData("order")][InlineData("decision")][InlineData("binding")][InlineData("borrowed")][InlineData("revoked")]
    public async Task ReferenceRecoveryRestoresOnlyThePrivateOriginalIntentWithoutWrites(string fault)
    {
        var f=new Fixture();var intent=await f.Intent();CompanyMaintenanceProcessStartReceipt? receipt=null;
        if(fault!="missing")receipt=await f.Start(intent);
        var current=intent.Context;var query=new CompanyMaintenanceRecoveryQuery(intent.IntentId,intent.Source.DecisionId,intent.Context.FieldId!,intent.Source.WellId,intent.Source.WorkOrderId);
        if(fault=="actor")current=current with{ActorUserId="foreign-actor"};
        if(fault=="company")current=current with{Company=new("tenant","foreign-company")};
        if(fault=="field")query=query with{FieldId="foreign-field"};
        if(fault=="order")query=query with{WorkOrderId="foreign-order"};
        if(fault=="decision")query=query with{DecisionId=Guid.NewGuid()};
        if(fault=="binding")current=current with{Database=new("lifecycle","different-fingerprint")};
        if(fault=="borrowed")f.Owner.Native.Borrowed=true;
        if(fault=="revoked")f.Owner.RevokeAt=f.Owner.Guards+1;
        var writes=f.Owner.Native.Writes;var before=JsonSerializer.Serialize(f.Owner.Native.Rows);
        var privateReads=f.Owner.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="PROCESS_INSTANCE");
        Task<CompanyMaintenanceProcessStart> Load()=>f.Owner.Service.ReadOriginalMaintenanceIntentAsync(current,query,f.Owner.Guard);
        if(fault=="allowed")
        {
            var restored=await Load();Assert.Equal(intent,restored);
            var confirmation=await f.Owner.Service.RecoverMaintenanceProcessAsync(restored,f.Owner.Guard,f.Read);
            var recovered=new CompanyMaintenanceRecovery(restored,confirmation);recovered.Require(current.Company,current.ActorUserId,query);
            Assert.Equal(receipt!.RecordedRevision,recovered.Receipt.RecordedRevision);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>Load());
        if(fault is "field" or "borrowed" or "revoked")Assert.Equal(privateReads,f.Owner.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="PROCESS_INSTANCE"));
        Assert.Equal(writes,f.Owner.Native.Writes);Assert.Equal(before,JsonSerializer.Serialize(f.Owner.Native.Rows));
    }
    [Theory][InlineData("allowed")][InlineData("lost-commit")][InlineData("missing-process")][InlineData("missing-task")][InlineData("changed-task")][InlineData("progressed")][InlineData("foreign-intent")][InlineData("changed-source")][InlineData("borrowed")][InlineData("changed-destinations")][InlineData("revoked")]
    public async Task OriginalRecoveryConfirmsOnlyCompleteUnchangedAssignmentWithoutWrites(string fault)
    {
        var f=new Fixture();var intent=await f.Intent();CompanyMaintenanceProcessStartReceipt? original=null;
        if(fault=="lost-commit") { f.Owner.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent));f.Owner.Native.FailCommit=false; }
        else if(fault!="missing-process")original=await f.Start(intent);
        if(fault=="missing-task")f.Owner.Native.Rows.Remove(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().First());
        if(fault=="changed-task")f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().First().ASSIGNED_ROLE="foreign-role";
        if(fault=="progressed")f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single().CURRENT_STATE="RECEIVED";
        if(fault=="foreign-intent")intent=intent with{Context=intent.Context with{ActorUserId="other"},Source=intent.Source with{Actor="other"}};
        if(fault=="changed-source")f.Source=f.Source with{WorkOrderRevision=new string('D',64)};
        if(fault=="borrowed")f.Owner.Native.Borrowed=true;
        if(fault=="changed-destinations")f.Owner.Native.ChangedDestinations=true;
        if(fault=="revoked")f.Owner.RevokeAt=f.Owner.Guards+3;
        var writes=f.Owner.Native.Writes;var before=JsonSerializer.Serialize(f.Owner.Native.Rows);
        Task<CompanyMaintenanceProcessStartReceipt> Recover()=>f.Owner.Service.RecoverMaintenanceProcessAsync(intent,f.Owner.Guard,f.Read);
        if(fault is "allowed" or "lost-commit") { var receipt=await Recover();receipt.Require(intent);if(original is not null)Assert.Equal(original.RecordedRevision,receipt.RecordedRevision); }
        else await Assert.ThrowsAnyAsync<Exception>(()=>Recover());
        Assert.Equal(writes,f.Owner.Native.Writes);
        Assert.Equal(before,JsonSerializer.Serialize(f.Owner.Native.Rows));
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)]
    public async Task EveryInsertFailureRollsBackAssignmentWithoutClaimingReceipt(int write)
    {
        var f=new Fixture();f.Owner.FailWrite=write;var intent=await f.Intent();await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent));
        Assert.Single(f.Owner.Native.Rows);Assert.Empty(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>());Assert.Equal(new[]{"begin","rollback"},f.Owner.Native.Transactions);
    }
    [Fact] public async Task LostCommitResponseReusesOnlyTheSameOriginalIntent()
    {
        var f=new Fixture();var intent=await f.Intent();f.Owner.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent));Assert.Equal(6,f.Owner.Native.Writes);
        f.Owner.Native.FailCommit=false;var result=await f.Start(intent);result.Require(intent);Assert.Equal(6,f.Owner.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent with{IntentId=Guid.NewGuid()}));Assert.Equal(6,f.Owner.Native.Writes);
    }
    [Theory][InlineData("borrowed")][InlineData("revision")][InlineData("source")][InlineData("authority")][InlineData("destination")]
    public async Task UnconfirmedOriginalAuthoritySourceDefinitionOrAssignmentNeverPublishes(string fault)
    {
        var f=new Fixture();var intent=await f.Intent();
        if(fault=="borrowed")f.Owner.Native.Borrowed=true;
        if(fault=="revision")f.Owner.Definition.VERSION="changed";
        if(fault=="source")f.Source=f.Source with{WorkOrderRevision=new string('C',64)};
        if(fault=="authority")f.Owner.RevokeAt=4;
        if(fault=="destination")f.Owner.Native.ChangedDestinations=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent));Assert.Empty(f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>());Assert.Empty(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>());
        if(fault=="borrowed"){Assert.Equal(0,f.Reads);Assert.Empty(f.Owner.Native.Transactions);}
    }
    [Fact] public async Task SourceChangeAfterCommitRetainsOriginalGraphButReturnsNoConfirmation()
    {
        var f=new Fixture();var intent=await f.Intent();f.Owner.Native.AfterCommit=()=>f.Source=f.Source with{WorkOrderRevision=new string('C',64)};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent));Assert.Equal(6,f.Owner.Native.Writes);Assert.Single(f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>());
        Assert.Equal(intent,CompanyMaintenanceProcessStart.FromPayload(f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single().INSTANCE_DATA_JSON!,intent.InstanceId(),"field",intent.Source.WorkOrderId));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent));Assert.Equal(6,f.Owner.Native.Writes);
    }
    [Fact] public async Task MissingRecordedTaskSetIsNotRepairedAndDifferentIntentContentCannotBeAdopted()
    {
        var f=new Fixture();var intent=await f.Intent();await f.Start(intent);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent with{Context=intent.Context with{Revision="other-binding"}}));Assert.Equal(6,f.Owner.Native.Writes);
        f.Owner.Native.Rows.RemoveAll(row=>row is CROSS_PERSONA_TASK);await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent));Assert.Equal(6,f.Owner.Native.Writes);
    }
    [Fact] public async Task RemovedMaintenanceOwnershipCannotRouteOrConfirmTheRecordedAssignment()
    {
        var f=new Fixture();var intent=await f.Intent();var receipt=await f.Start(intent);var process=f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single();process.INSTANCE_DATA_JSON="{}";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(intent));Assert.Equal(6,f.Owner.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.Native.Router.RouteTaskAsync(intent.InstanceId(),receipt.StepInstanceId,"MaintenanceRole","WORK_ORDER",intent.Source.WorkOrderId,null,"actor"));Assert.Equal(6,f.Owner.Native.Writes);
    }
    [Fact] public async Task FieldAndOrderGrantsAloneCannotPublishAnotherCompanyMaintenancePayload()
    {
        var f=new Fixture();var intent=await f.Intent();await f.Start(intent);
        var resources=new Moq.Mock<Beep.OilandGas.Models.Core.Interfaces.IAccessControlService>();
        resources.Setup(value=>value.CheckAssetAccessAsync("actor",Moq.It.IsAny<string>(),Moq.It.IsAny<string>(),"READ",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var reads=new Beep.OilandGas.ApiService.Services.CompanyProcessReadService(resources.Object);
        Assert.NotNull(await reads.ReadAsync(f.Owner.Service,"actor","field",intent.InstanceId(),intent.Context.Company));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>reads.ReadAsync(f.Owner.Service,"actor","field",intent.InstanceId(),new("tenant","other-company")));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>reads.ReadAsync(f.Owner.Service,"actor","field",intent.InstanceId()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>reads.HistoryAsync(f.Owner.Service,"actor","field",intent.InstanceId(),new("tenant","other-company")));
        Assert.Equal(6,f.Owner.Native.Writes);
    }
}
#endif
