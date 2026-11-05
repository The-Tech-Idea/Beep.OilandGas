using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyPersonnelIdentityTests
{
    [Theory][InlineData("record")][InlineData("app-before")][InlineData("app-after")][InlineData("person-after")][InlineData("write")][InlineData("readback")][InlineData("unknown")][InlineData("schema")][InlineData("borrowed")][InlineData("isolation")][InlineData("authority")]
    public async Task NativeIdentityAppendClosesActualPersonAndAccountProofAndRecoversOriginalAuditWithoutAnotherWrite(string mode)
    {
        var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var company=new CompanyScope("tenant","company");var context=new CompanyPersonnelPolicyContext(company,new("core","binding"),"administrator",new string('A',64));var utc=DateTime.UtcNow;
        var employee=new BA_EMPLOYEE{EMPLOYER_BA_ID="company",EMPLOYEE_BA_ID="person",EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",STATUS="Reported employment status",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(30)};var person=new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="person",BA_LONG_NAME="Reported native person",BA_TYPE="Reported person type",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(30)};f.Rows.Add(employee);f.Rows.Add(person);
        var metadata=new Mock<IPPDMMetadataRepository>();var creator=new ClassCreator(f.Editor.Object);
        foreach(var (type,keys) in new[]{(typeof(BA_EMPLOYEE),new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}),(typeof(BUSINESS_ASSOCIATE),new[]{"BUSINESS_ASSOCIATE_ID"}),(typeof(COMPANY_PERSONNEL_IDENTITY),new[]{"IDENTITY_RECORD_ID"})})
        {
            var table=type.Name;var definition=new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys)};foreach(var column in table=="BA_EMPLOYEE"?new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID"}:table=="COMPANY_PERSONNEL_IDENTITY"?new[]{"PERSON_BA_ID","OWNER_COMPANY_BA_ID"}:Array.Empty<string>())definition.ForeignKeys.Add(new(){ForeignKeyColumn=column,ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"});metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(definition);metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);f.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName==table),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(type);foreach(var field in shape.Fields){field.IsKey=keys.Contains(field.FieldName);if(field.IsKey)field.AllowDBNull=false;}if(mode=="schema" && table=="COMPANY_PERSONNEL_IDENTITY")shape.Fields.RemoveAll(field=>field.FieldName=="COMMAND_JSON");return shape;});
        }
        var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);var native=await owner.ReadIdentityPersonAsync(context,"person",()=>Task.CompletedTask);var empty=CompanyPersonnelIdentityCatalog.Create(context,[],DateTime.UtcNow);var review=CompanyPersonnelIdentityReview.Create(context,new("reviewer","person"),new(company,"reviewer","Actual account",true,new string('B',64)),native,empty,DateTime.UtcNow);var command=new CompanyPersonnelIdentityCommand(review,1,Guid.NewGuid(),utc,utc.AddDays(10),"Actual identity evidence reference","Private administrator verification rationale",true);command.RequireCatalog(empty);
        f.Source.Setup(value=>value.InsertEntity("COMPANY_PERSONNEL_IDENTITY",It.IsAny<object>())).Returns((string _,object value)=>{f.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((COMPANY_PERSONNEL_IDENTITY)value);if(mode=="readback")row.ROW_CREATED_BY="changed audit";f.Rows.Add(row);if(mode=="person-after")person.BA_LONG_NAME="Changed native person";return new ErrorsInfo{Flag=Errors.Ok};});
        if(mode=="unknown")f.FailCommit=true;if(mode=="borrowed")f.Borrowed=true;if(mode=="isolation")f.Isolation=System.Data.IsolationLevel.ReadCommitted;Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;var proofs=0;Task AppProof(){proofs++;if(mode=="app-before" || mode=="app-after" && proofs>=3)throw new InvalidOperationException("Actual account changed");return Task.CompletedTask;}
        var error=await Record.ExceptionAsync(async()=>command.RequireRecorded(await owner.RecordPersonnelIdentityAsync(command,Guard,AppProof)));if(mode=="record")Assert.Null(error);else Assert.NotNull(error);
        if(mode is "record" or "unknown")
        {
            f.FailCommit=false;Assert.Equal(1,f.Writes);var original=Assert.Single(f.Rows.OfType<COMPANY_PERSONNEL_IDENTITY>());var before=JsonSerializer.Serialize(original);command.RequireRecorded(await owner.RecordPersonnelIdentityAsync(command,()=>Task.CompletedTask,()=>throw new Exception("Recovery cannot refresh/create source proof"),true));var reference=new CompanyPersonnelIdentityReference(command.RecordId(),command.IntentId);var recovered=await owner.RecoverPersonnelIdentityReferenceAsync(context,reference,()=>Task.CompletedTask);recovered.Require(company,context.ActorUserId,reference);Assert.Equal(before,JsonSerializer.Serialize(recovered.Recorded));Assert.Equal(1,f.Writes);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(await owner.ReadPersonnelIdentitiesAsync(context,()=>Task.CompletedTask)));await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordPersonnelIdentityAsync(command with{IntentId=Guid.NewGuid()},()=>Task.CompletedTask,()=>Task.CompletedTask));Assert.Equal(1,f.Writes);
            var catalog=await owner.ReadPersonnelIdentitiesAsync(context,()=>Task.CompletedTask);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RequireRecordedReviewerIdentityCoreAsync(catalog,native,"reviewer",utc,utc.AddDays(1),()=>Task.CompletedTask));
            await PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object,async()=>
            {
                await ((ICompanyRecordedReviewerIdentityCore)owner).RequireRecordedReviewerIdentityCoreAsync(catalog,native,"reviewer",utc,utc.AddDays(1),()=>Task.CompletedTask);
                await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RequireRecordedReviewerIdentityCoreAsync(catalog,native,context.ActorUserId,utc,utc.AddDays(1),()=>Task.CompletedTask));
                await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RequireRecordedReviewerIdentityCoreAsync(catalog,native,"reviewer",utc,utc.AddDays(11),()=>Task.CompletedTask));
                var name=person.BA_LONG_NAME;person.BA_LONG_NAME="Changed current native person";
                await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RequireRecordedReviewerIdentityCoreAsync(catalog,native,"reviewer",utc,utc.AddDays(1),()=>Task.CompletedTask));
                person.BA_LONG_NAME=name;
            });
            var executor=new FieldSelectionReview(company,context.Database,"executor","execution-authority","field");
            var executionOwner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((company,executor.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);
            var historical=JsonSerializer.Serialize(catalog);
            Task Current()=>executionOwner.RequireExecutionReviewerIdentityCoreAsync(executor,catalog,native,"reviewer",utc,utc.AddDays(1),()=>Task.CompletedTask);
            await Assert.ThrowsAsync<InvalidOperationException>(Current);
            await PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object,async()=>
            {
                await Current();
                await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>executionOwner.RequireExecutionReviewerIdentityCoreAsync(executor with{Company=new("tenant","foreign")},catalog,native,"reviewer",utc,utc.AddDays(1),()=>Task.CompletedTask));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>executionOwner.RequireExecutionReviewerIdentityCoreAsync(executor with{Database=new("foreign","binding")},catalog,native,"reviewer",utc,utc.AddDays(1),()=>Task.CompletedTask));
                await Assert.ThrowsAsync<InvalidOperationException>(()=>executionOwner.RequireExecutionReviewerIdentityCoreAsync(executor,catalog,native,"executor",utc,utc.AddDays(1),()=>Task.CompletedTask));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>executionOwner.RequireExecutionReviewerIdentityCoreAsync(executor,catalog,native,"reviewer",utc,utc.AddDays(1),()=>Task.FromException(new UnauthorizedAccessException("Executor authority revoked"))));
                var name=person.BA_LONG_NAME;person.BA_LONG_NAME="Changed current native person";await Assert.ThrowsAsync<InvalidOperationException>(Current);person.BA_LONG_NAME=name;
                await Current();
            });
            Assert.Equal(historical,JsonSerializer.Serialize(catalog));
            Assert.Equal(1,f.Writes);Assert.Equal(before,JsonSerializer.Serialize(original));
        }
        else{Assert.Empty(f.Rows.OfType<COMPANY_PERSONNEL_IDENTITY>());if(mode is "write" or "readback" or "app-after" or "person-after" or "isolation")Assert.Contains("rollback",f.Transactions);}
    }
    [Theory][InlineData("missing")][InlineData("expired")][InlineData("foreign")]
    public async Task CurrentCompanyEmploymentClosesBeforePrivateNativePersonMetadata(string mode)
    {
        var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var company=new CompanyScope("tenant","company");var context=new CompanyPersonnelPolicyContext(company,new("core","binding"),"administrator",new string('A',64));var utc=DateTime.UtcNow;var employee=new BA_EMPLOYEE{EMPLOYER_BA_ID=mode=="foreign"?"other":"company",EMPLOYEE_BA_ID="person",EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",EFFECTIVE_DATE=mode=="missing"?null:utc.AddDays(-1),EXPIRY_DATE=mode=="expired"?utc.AddDays(-1):utc.AddDays(1)};f.Rows.Add(employee);var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetTableMetadataAsync("BA_EMPLOYEE")).ReturnsAsync(new PPDMTableMetadata{TableName="BA_EMPLOYEE",EntityTypeName=typeof(BA_EMPLOYEE).FullName!,PrimaryKeyColumn="EMPLOYER_BA_ID,EMPLOYEE_BA_ID,EMPLOYEE_OBS_NO",ForeignKeys=[new(){ForeignKeyColumn="EMPLOYER_BA_ID",ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"},new(){ForeignKeyColumn="EMPLOYEE_BA_ID",ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"}]});metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("BA_EMPLOYEE")).ReturnsAsync(["EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"]);var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(value=>value.CheckEntityExist("BA_EMPLOYEE")).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(BA_EMPLOYEE));foreach(var field in shape.Fields){field.IsKey=new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}.Contains(field.FieldName);if(field.IsKey)field.AllowDBNull=false;}return shape;});var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.ReadIdentityPersonAsync(context,"person",()=>Task.CompletedTask));f.Source.Verify(value=>value.GetEntityAsync("BUSINESS_ASSOCIATE",It.IsAny<List<AppFilter>>()),Times.Never);Assert.Equal(0,f.Writes);
    }
    [Theory][InlineData("unverified")][InlineData("self")][InlineData("time-zone")][InlineData("window")][InlineData("overlap-account")][InlineData("overlap-person")][InlineData("audit")]
    public void CanonicalIdentityProofRefusesSelfAssertionsAmbiguousMappingsAndChangedNativeAudit(string mode)
    {
        var utc=DateTime.UtcNow;var company=new CompanyScope("tenant","company");var context=new CompanyPersonnelPolicyContext(company,new("core","binding"),mode=="self"?"reviewer":"administrator",new string('A',64));var catalog=CompanyPersonnelIdentityCatalog.Create(context,[],utc);var native=new CompanyNativePersonFacts(company,"person","Same reported name","Reported person type",utc.AddDays(-1),utc.AddDays(30),new("company","person",1,null,null,utc.AddDays(-1),utc.AddDays(30),new string('C',64)),new string('D',64));var review=CompanyPersonnelIdentityReview.Create(context,new("reviewer","person"),new(company,"reviewer","Same reported name",true,new string('B',64)),native,catalog,utc);var command=new CompanyPersonnelIdentityCommand(review,1,Guid.NewGuid(),mode=="time-zone"?DateTime.SpecifyKind(utc,DateTimeKind.Unspecified):utc,mode=="window"?utc.AddDays(31):utc.AddDays(10),"Actual source","Original rationale",mode!="unverified");
        var error=Record.Exception(()=>{command.Require();var row=command.Prepare(utc);if(mode=="audit"){row.ROW_CREATED_BY="another";row.RECORDED_REVISION=CompanyPersonnelIdentityCommand.Digest(row);command.RequireRecorded(row);}if(mode is "overlap-account" or "overlap-person"){var first=CompanyPersonnelIdentityVersion.From(row);var second=first with{RecordId=CompanyPersonnelIdentityCommand.RecordIdFor(company,mode=="overlap-person"?"another":"reviewer",mode=="overlap-person"?1:2),UserId=mode=="overlap-person"?"another":"reviewer",PersonBaId=mode=="overlap-person"?"person":"another-person",Version=mode=="overlap-person"?1:2};CompanyPersonnelIdentityCatalog.Create(context,[first,second],utc);}});Assert.NotNull(error);
    }
}
#endif
