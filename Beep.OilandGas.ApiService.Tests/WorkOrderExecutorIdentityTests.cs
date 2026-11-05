using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class WorkOrderExecutorIdentityTests
{
    [Theory][InlineData("current")][InlineData("qualified")][InlineData("unmapped")][InlineData("other-person")][InlineData("authority")][InlineData("person-drift")][InlineData("employment-drift")][InlineData("catalog-drift")]
    public async Task NativeExecutorIdentityClosesExactAssignedPersonAndOwnAccountUnderIdleAndOwnedReads(string mode)
    {
        // Controlled prerequisite projection; actual identity/person/native binding owner, not an approval/start API journey.
        var company=new CompanyScope("tenant","company");var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(company,Guid.NewGuid()),"field","order");
        var qualifiedOrder=mode=="qualified"?new WorkOrderDetailModel{InstanceId=task.WorkOrderId,FieldId=task.FieldId,Source="WORK_ORDER",State="",EvidenceUnavailableReason="No inferred header execution state",WoSubType="CORRECTIVE",Components=[
            new(task.WorkOrderId,"equipment-component",task.FieldId,"","pump"){FacilityId="plant",FacilityType="gas",NativeRevision=new string('A',64)},
            new(task.WorkOrderId,"isolation-component",task.FieldId,"",""){InformationItemId="ISOLATION-information",InformationItemSubtype="DOCUMENT"},
            new(task.WorkOrderId,"risk-component",task.FieldId,"",""){InformationItemId="RISK-information",InformationItemSubtype="DOCUMENT"}]}:null;
        var prototype=WorkOrderExecutionStartFixture.Original(originalOrder:qualifiedOrder,taskOverride:qualifiedOrder is null?null:task);var prerequisites=prototype.Command.Review.Prerequisites;var stage=prototype.Command.Review.Stage;var context=prerequisites.Requirements.Context;
        var native=new PersonaTaskRoutingTests.Fixture();native.Rows.Clear();var from=prerequisites.Requirements.Approval.ValidFromUtc.AddDays(-1);var to=prerequisites.Requirements.Approval.ValidToUtc.AddDays(1);var employeeId=prerequisites.Personnel.EmployeeBaId;
        var employee=new BA_EMPLOYEE{EMPLOYER_BA_ID=context.Company.BusinessAssociateId,EMPLOYEE_BA_ID=employeeId,EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",STATUS="ACTIVE",EMPLOYEE_POSITION="Recorded operator",EFFECTIVE_DATE=from,EXPIRY_DATE=to};
        var person=new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID=employeeId,BA_LONG_NAME="Actual native assigned employee",BA_TYPE="PERSON",ACTIVE_IND="Y",EFFECTIVE_DATE=from,EXPIRY_DATE=to};native.Rows.AddRange([employee,person]);
        var metadata=new Mock<IPPDMMetadataRepository>();var creator=new ClassCreator(native.Editor.Object);
        foreach(var (type,keys) in new[]{(typeof(BA_EMPLOYEE),new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}),(typeof(BUSINESS_ASSOCIATE),new[]{"BUSINESS_ASSOCIATE_ID"}),(typeof(COMPANY_PERSONNEL_IDENTITY),new[]{"IDENTITY_RECORD_ID"})})
        {
            var table=type.Name;var entry=new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys)};
            foreach(var column in table=="BA_EMPLOYEE"?new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID"}:table=="COMPANY_PERSONNEL_IDENTITY"?new[]{"PERSON_BA_ID","OWNER_COMPANY_BA_ID"}:Array.Empty<string>())entry.ForeignKeys.Add(new(){ForeignKeyColumn=column,ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"});
            metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(entry);metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);native.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);
            native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(type);foreach(var field in shape.Fields){field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal);if(field.IsKey)field.AllowDBNull=false;}return shape;});
        }
        var owner=new ContractorManagementService(native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);
        var identityContext=new CompanyPersonnelPolicyContext(context.Company,context.Database,context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(context)));
        var actual=await owner.ReadIdentityPersonAsync(identityContext,employeeId,()=>Task.CompletedTask);
        // Material/order/approval proof is a separate seam. Capture the actual employment revision before this identity review.
        prerequisites=prerequisites with{Personnel=prerequisites.Personnel with{EmploymentRevision=actual.Employment.Revision}};prerequisites=prerequisites with{Revision=CompanyWorkOrderExecutionPrerequisites.Digest(prerequisites)};
        var admin=identityContext with{ActorUserId="identity-administrator"};var empty=CompanyPersonnelIdentityCatalog.Create(admin,[],DateTime.UtcNow);
        var review=CompanyPersonnelIdentityReview.Create(admin,new(context.ActorUserId,employeeId),new(context.Company,context.ActorUserId,"Actual account",true,new string('A',64)),actual,empty,DateTime.UtcNow);
        var mapping=new CompanyPersonnelIdentityCommand(review,1,Guid.NewGuid(),from,to,"Independently verified employee identity","Private mapping rationale",true);var row=mapping.Prepare(DateTime.UtcNow);if(mode!="unmapped")native.Rows.Add(row);
        if(mode=="other-person"){prerequisites=prerequisites with{Personnel=prerequisites.Personnel with{EmployeeBaId="other-assigned-person"}};prerequisites=prerequisites with{Revision=CompanyWorkOrderExecutionPrerequisites.Digest(prerequisites)};}
        var personReads=0;var catalogReads=0;
        native.Source.Setup(value=>value.GetEntityAsync("BUSINESS_ASSOCIATE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>
        {
            if(++personReads==2){if(mode=="person-drift")person.BA_LONG_NAME="Changed actual person";if(mode=="employment-drift")employee.STATUS="INACTIVE";}
            return native.Rows.OfType<BUSINESS_ASSOCIATE>().Select(CompanyInventorySourceReview.Copy).ToArray();
        });
        native.Source.Setup(value=>value.GetEntityAsync("COMPANY_PERSONNEL_IDENTITY",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>
        {if(++catalogReads==2 && mode=="catalog-drift")row.SOURCE_REFERENCE="Changed retained identity source";return native.Rows.OfType<COMPANY_PERSONNEL_IDENTITY>().Select(CompanyInventorySourceReview.Copy).ToArray();});
        Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException("Actual executing authority revoked")):Task.CompletedTask;
        var beforeReads=native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="BUSINESS_ASSOCIATE");
        CompanyWorkOrderExecutorIdentity? identity=null;var error=await Record.ExceptionAsync(async()=>identity=await owner.ReadExecutorIdentityAsync(prerequisites,stage,Guard));
        if(mode is "current" or "qualified")
        {
            Assert.Null(error);
            if(mode=="qualified")
            {
                // Controlled asset snapshot; this native owner must still validate its actual person facts independently.
                var utc=DateTime.UtcNow;var scope=new CompanyMaintenanceQualifiedAssets(context,CompanyWorkOrderExecutionRequirements.OrderRevision(prerequisites.Requirements.Order),"UTC",
                    [new("plant","gas",context.FieldId!,context.Company.BusinessAssociateId,"READ",utc.AddDays(-1),null,new string('A',64))],
                    [new("pump",context.Company.BusinessAssociateId,"READ",utc.AddDays(-1),null,null,null,new string('B',64),new("pump","plant","gas",7,utc.AddDays(-1),utc.AddDays(-1),null,null,"native-install",new string('C',64),1,new string('D',64)))],utc);
                identity=identity! with{QualifiedWorkAssets=scope,ComparedAtUtc=DateTime.UtcNow,Revision=""};identity=identity with{Revision=CompanyWorkOrderExecutorIdentity.Digest(identity)};
            }
            identity!.Require();Assert.Equal(context.ActorUserId,identity.Identity.UserId);Assert.Equal(employeeId,identity.Person.PersonBaId);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RequireExecutorIdentityCoreAsync(identity,()=>Task.CompletedTask));
            await PPDMDataSourceTransaction.ExecuteAsync(native.Source.Object,async()=>
            {
                await owner.RequireExecutorIdentityCoreAsync(identity,()=>Task.CompletedTask);
                var name=person.BA_LONG_NAME;person.BA_LONG_NAME="Changed core person";await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RequireExecutorIdentityCoreAsync(identity,()=>Task.CompletedTask));person.BA_LONG_NAME=name;
                await owner.RequireExecutorIdentityCoreAsync(identity,()=>Task.CompletedTask);
            });
            Assert.DoesNotContain(mapping.Reason,JsonSerializer.Serialize(identity));Assert.DoesNotContain("Versions",JsonSerializer.Serialize(identity));
        }
        else Assert.NotNull(error);
        if(mode is "unmapped" or "other-person" or "authority")Assert.Equal(beforeReads,native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="BUSINESS_ASSOCIATE"));
        Assert.Equal(0,native.Writes);PPDMDataSourceTransaction.RequireIdle(native.Source.Object);
    }
}
#endif
