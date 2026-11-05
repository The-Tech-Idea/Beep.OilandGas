using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyPersonnelPolicyTests
{
    [Theory][InlineData("record")][InlineData("write")][InlineData("readback")][InlineData("unknown")][InlineData("authority")][InlineData("schema")][InlineData("borrowed")][InlineData("isolation")][InlineData("catalog-drift")][InlineData("historical")]
    public async Task NativePolicyAppendPreservesEffectiveHistoryAndExactIntentRecoveryWithRollback(string mode)
    {
        var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var context=new CompanyPersonnelPolicyContext(new("tenant","company"),new("lifecycle","fingerprint"),"administrator",new string('A',64));var utc=DateTime.UtcNow;
        var policy=new CompanyPersonnelPolicyDraft("RecordedDuty","CORRECTIVE","Explicit jurisdiction","Recorded licence","authority","CERTIFICATION",["VALID"],[new("TRAINING_HOURS","TRAINING","MINIMUM",8m,"HOUR","policy-source")],utc.AddDays(-1),utc.AddDays(30),"Original policy document"){AcceptedEmploymentStatuses=["ACTIVE"],AcceptedEmploymentPositions=[]};
        if(mode=="historical")policy=policy with{AcceptedEmploymentStatuses=null,AcceptedEmploymentPositions=null};
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_PERSONNEL_POLICY")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_PERSONNEL_POLICY",EntityTypeName=typeof(WORK_ORDER_PERSONNEL_POLICY).FullName!,PrimaryKeyColumn="POLICY_RECORD_ID"});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_PERSONNEL_POLICY")).ReturnsAsync(["POLICY_RECORD_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(source=>source.CheckEntityExist("WORK_ORDER_PERSONNEL_POLICY")).Returns(true);f.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(WORK_ORDER_PERSONNEL_POLICY));if(mode=="schema")shape.Fields.RemoveAll(field=>field.FieldName=="COMMAND_JSON");return shape;});
        var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);
        var catalog=CompanyPersonnelPolicyCatalog.Create(context,[],utc);var command=new CompanyPersonnelPolicyCommand(context,catalog.Revision,policy,1,Guid.NewGuid(),"Explicit original policy rationale");command.RequireCatalog(catalog);
        f.Source.Setup(source=>source.InsertEntity("WORK_ORDER_PERSONNEL_POLICY",It.IsAny<object>())).Returns((string _,object value)=>
        {
            f.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((WORK_ORDER_PERSONNEL_POLICY)value);if(mode=="readback")row.POLICY_JSON="changed";f.Rows.Add(row);
            if(mode=="catalog-drift"){var other=command with{Policy=policy with{DutyCode="AnotherDuty"},IntentId=Guid.NewGuid()};f.Rows.Add(other.Prepare(DateTime.UtcNow));}return new ErrorsInfo{Flag=Errors.Ok};
        });
        if(mode=="unknown")f.FailCommit=true;if(mode=="borrowed")f.Borrowed=true;if(mode=="isolation")f.Isolation=System.Data.IsolationLevel.ReadCommitted;
        Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        if(mode=="historical")
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordPersonnelPolicyAsync(command,Guard));Assert.Equal(0,f.Writes);Assert.Empty(f.Transactions);
            var original=command.Prepare(DateTime.UtcNow);f.Rows.Add(original);var before=JsonSerializer.Serialize(original);Assert.DoesNotContain("AcceptedEmployment",original.COMMAND_JSON);
            var recovered=await owner.RecordPersonnelPolicyAsync(command,Guard,true);Assert.Equal(before,JsonSerializer.Serialize(recovered));var reference=new CompanyPersonnelPolicyReference(command.RecordId(),command.IntentId);var envelope=await owner.RecoverPersonnelPolicyReferenceAsync(context,reference,Guard);Assert.Equal(before,JsonSerializer.Serialize(envelope.Recorded));Assert.Equal(0,f.Writes);Assert.Empty(f.Transactions);return;
        }
        var error=await Record.ExceptionAsync(async()=>{var row=await owner.RecordPersonnelPolicyAsync(command,Guard);command.RequireRecorded(row);});
        if(mode=="record")Assert.Null(error);else Assert.NotNull(error);
        if(mode is "record" or "unknown")
        {
            Assert.Equal(1,f.Writes);var stored=f.Rows.OfType<WORK_ORDER_PERSONNEL_POLICY>().Single();var before=JsonSerializer.Serialize(stored);
            var recovered=await owner.RecordPersonnelPolicyAsync(command,()=>Task.CompletedTask,true);Assert.Equal(before,JsonSerializer.Serialize(recovered));var reference=new CompanyPersonnelPolicyReference(command.RecordId(),command.IntentId);
            var envelope=await owner.RecoverPersonnelPolicyReferenceAsync(context,reference,()=>Task.CompletedTask);envelope.Require(context.Company,context.ActorUserId,reference);Assert.Equal(before,JsonSerializer.Serialize(envelope.Recorded));Assert.Equal(1,f.Writes);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordPersonnelPolicyAsync(command with{IntentId=Guid.NewGuid()},()=>Task.CompletedTask));Assert.Equal(1,f.Writes);
            var history=await owner.ReadPersonnelPoliciesAsync(context,()=>Task.CompletedTask);Assert.Single(history.Versions);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(history));Assert.DoesNotContain(command.IntentId.ToString("D"),JsonSerializer.Serialize(history));
            if(mode=="record")
            {
                var next=new CompanyPersonnelPolicyCommand(context,history.Revision,policy with{EffectiveFromUtc=policy.EffectiveToUtc,EffectiveToUtc=policy.EffectiveToUtc.AddDays(30)},2,Guid.NewGuid(),"Explicit next policy version");
                await owner.RecordPersonnelPolicyAsync(next,()=>Task.CompletedTask);Assert.Equal(before,JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_PERSONNEL_POLICY>().Single(row=>row.VERSION_NO==1)));Assert.Equal(2,(await owner.ReadPersonnelPoliciesAsync(context,()=>Task.CompletedTask)).Versions.Count);
                var latest=await owner.ReadPersonnelPoliciesAsync(context,()=>Task.CompletedTask);var overlapping=next with{ExpectedCatalogRevision=latest.Revision,Version=3,IntentId=Guid.NewGuid()};await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordPersonnelPolicyAsync(overlapping,()=>Task.CompletedTask));Assert.Equal(2,f.Writes);
            }
        }
        else{Assert.Empty(f.Rows.OfType<WORK_ORDER_PERSONNEL_POLICY>());if(mode is "write" or "readback" or "catalog-drift" or "isolation")Assert.Contains("rollback",f.Transactions);else Assert.Empty(f.Transactions);}
    }
}
#endif
