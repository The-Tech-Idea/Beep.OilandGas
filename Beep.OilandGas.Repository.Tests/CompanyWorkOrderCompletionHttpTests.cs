using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact] public async Task ActualIndependentCompletionReviewHttpReadsRecordedReportCurrentInspectionsAndNativePersonWithoutWrites()
    {
        using var f=await Ready();const string order="order-الكويت";await f.Grant("FIELD","A","WRITE");await f.Grant("WORK_ORDER",order,"WRITE");
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName="selected-tenant",ConnectionKey="SELECTED-TENANT",ConcurrencyStamp="completion-lifecycle-version"});await f.Db.SaveChangesAsync();
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        var core=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(core,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core));
        var owners=MaintenanceReadOwners(f,modules,profile);f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,WORK_ORDER_TYPE="CORRECTIVE",INSTRUCTIONS="Original reported work scope",ACTIVE_IND="Y"});f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,COMPONENT_ID="component",FIELD_ID="A",ACTIVE_IND="Y"});
        var actualOrder=await owners.Order.GetByIdAsync("A",order);Assert.NotNull(actualOrder);
        // Already-recorded canonical work-report/inspection seam; original execution writes are a separate gate.
        var original=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(database,"A",originalOrder:actualOrder,captureInspectionRows:rows=>f.Rows.AddRange(rows));
        var report=original.Command.Prepare(DateTime.UtcNow);f.Rows.Add(report);var query=new CompanyWorkOrderCompletionQuery(original.Command.Review.Inspections.Start.Permit.Task,report.SUBMISSION_ID);
        var creator=new ClassCreator(f.Editor.Object);f.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        foreach(var (type,keys) in new[]{(typeof(WORK_ORDER_EXECUTION_SUBMISSION),new[]{"SUBMISSION_ID"}),(typeof(WORK_ORDER_INSPECTION),new[]{"INSPECTION_ID"}),(typeof(COMPANY_PERSONNEL_IDENTITY),new[]{"IDENTITY_RECORD_ID"}),(typeof(BA_EMPLOYEE),new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}),(typeof(BUSINESS_ASSOCIATE),new[]{"BUSINESS_ASSOCIATE_ID"})})
        {
            var table=type.Name;var shape=creator.ConvertToEntityStructure(type);foreach(var field in shape.Fields){field.IsKey=keys.Contains(field.FieldName);if(field.IsKey)field.AllowDBNull=false;}
            var metadata=new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys),Module="PPDM_CORE"};
            foreach(var column in table=="BA_EMPLOYEE"?new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID"}:table=="COMPANY_PERSONNEL_IDENTITY"?new[]{"PERSON_BA_ID","OWNER_COMPANY_BA_ID"}:Array.Empty<string>())metadata.ForeignKeys.Add(new(){ForeignKeyColumn=column,ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"});
            f.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName==table),true)).Returns(shape);
            f.Metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(metadata);f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);
        }
        var personnel=new ContractorManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,async()=>{var access=await f.Gate.RefreshCompanyAsync();return(access.Scope,access.UserId,database);},NullLogger<ContractorManagementService>.Instance);
        var native=new WorkOrderExecutionPermitService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),_=>Mock.Of<IProcessService>()),production,boundary.Orchestrator.Object,owners.Order,owners.Inspections,native,contractors:personnel);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceHandoffController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});var url=host.Url+"/api/field/current/maintenance-handoffs/execution-permit/completion-review";
        int ReportReads()=>f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="WORK_ORDER_EXECUTION_SUBMISSION");
        var reads=ReportReads();using(var denied=await host.Http.PostAsJsonAsync(url,query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(reads,ReportReads());Assert.Empty(f.Writes);
        var permission=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.VerifyCompletion"};f.Db.RoleClaims.Add(permission);await f.Db.SaveChangesAsync();
        async Task<CompanyWorkOrderCompletionReview> Read()
        {
            using var response=await host.Http.PostAsJsonAsync(url,query);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());
            var result=(await response.Content.ReadFromJsonAsync<CompanyWorkOrderCompletionReview>())!;result.Require();Assert.DoesNotContain(original.Command.Reason,JsonSerializer.Serialize(result));Assert.DoesNotContain(original.Command.IntentId.ToString(),JsonSerializer.Serialize(result));return result;
        }
        var missing=await Read();Assert.Null(missing.ReviewerPerson);Assert.Contains(missing.Blockers,value=>value.Contains("reviewer account/native person",StringComparison.Ordinal));Assert.Contains(CompanyWorkOrderCompletionReview.PendingVerification,missing.Blockers);
        var utc=DateTime.UtcNow;const string personId="completion-person";
        f.Rows.Add(new BA_EMPLOYEE{EMPLOYER_BA_ID="company",EMPLOYEE_BA_ID=personId,EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",STATUS="Reported active employment",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(30)});
        f.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID=personId,BA_LONG_NAME="Recorded independent reviewer",BA_TYPE="PERSON",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(30)});
        var readerContext=new CompanyPersonnelPolicyContext(f.Gate.Company.Scope,database,f.Gate.Company.UserId,new string('A',64));var person=await personnel.ReadIdentityPersonAsync(readerContext,personId,()=>Task.CompletedTask);
        var mapperContext=readerContext with{ActorUserId="identity-administrator"};var empty=CompanyPersonnelIdentityCatalog.Create(mapperContext,[],DateTime.UtcNow);
        var identityReview=CompanyPersonnelIdentityReview.Create(mapperContext,new(f.Gate.Company.UserId,personId),new(f.Gate.Company.Scope,f.Gate.Company.UserId,"Actual reviewer account",true,new string('B',64)),person,empty,DateTime.UtcNow);
        var mapping=new CompanyPersonnelIdentityCommand(identityReview,1,Guid.NewGuid(),utc,utc.AddDays(10),"Recorded independent identity evidence","Private mapper rationale",true);var identity=mapping.Prepare(DateTime.UtcNow);f.Rows.Add(identity);
        var current=await Read();Assert.Equal(personId,current.ReviewerPerson!.PersonBaId);Assert.Equal(report.RECORDED_REVISION,current.Report.Revision);Assert.Contains(current.Blockers,value=>value.Contains("No explicitly approved completion-verification",StringComparison.Ordinal));Assert.DoesNotContain(mapping.Reason,JsonSerializer.Serialize(current));Assert.Empty(f.Writes);
        var header=f.Rows.OfType<WORK_ORDER>().Single();var instructions=header.INSTRUCTIONS;header.INSTRUCTIONS="Changed report scope";Assert.Contains("Original submitted work scope or component population changed.",(await Read()).Blockers);header.INSTRUCTIONS=instructions;
        var own=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(database,"A",actor:f.Gate.Company.UserId,originalOrder:actualOrder);var ownRow=own.Command.Prepare(DateTime.UtcNow);f.Rows.Add(ownRow);
        using(var self=await host.Http.PostAsJsonAsync(url,new CompanyWorkOrderCompletionQuery(own.Command.Review.Inspections.Start.Permit.Task,ownRow.SUBMISSION_ID)))Assert.Equal(HttpStatusCode.Forbidden,self.StatusCode);
        report.WORK_ORDER_ID="foreign-order";report.COMMAND_JSON="malformed private report";using(var foreign=await host.Http.PostAsJsonAsync(url,query))Assert.Equal(HttpStatusCode.Conflict,foreign.StatusCode);report.WORK_ORDER_ID=order;report.COMMAND_JSON=JsonSerializer.Serialize(original.Command);
        f.Db.RoleClaims.Remove(permission);await f.Db.SaveChangesAsync();reads=ReportReads();using(var revoked=await host.Http.PostAsJsonAsync(url,query))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(reads,ReportReads());Assert.Empty(f.Writes);
    }
}
