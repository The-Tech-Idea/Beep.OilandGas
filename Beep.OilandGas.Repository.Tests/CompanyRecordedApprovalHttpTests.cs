using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Theory][InlineData(false,false)][InlineData(true,false)][InlineData(false,true)][InlineData(true,true)]
    public async Task ActualRecordedApprovalHttpUsesCurrentCompanyCapabilityAndPublicFactsWithoutWrites(bool phaseAware,bool requirements)
    {
        // Controlled already-recorded canonical approval; this does not certify the approval command endpoint.
        using var f=await Ready();await f.Grant("FIELD","A","WRITE");var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName="selected-tenant",ConnectionKey="SELECTED-TENANT",ConcurrencyStamp="lifecycle-version"});await f.Db.SaveChangesAsync();
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var core=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(core,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core));
        var owners=MaintenanceReadOwners(f,modules,profile);WorkOrderDetailModel? originalOrder=null;
        if(requirements)
        {
            const string order="order-الكويت";await f.Grant("WORK_ORDER",order,"WRITE");f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,WORK_ORDER_TYPE="CORRECTIVE",INSTRUCTIONS="Original work",ACTIVE_IND="Y"});
            foreach(var component in new[]{"component","isolation-component","risk-component"})f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,COMPONENT_ID=component,FIELD_ID="A",ACTIVE_IND="Y"});
            originalOrder=await owners.Order.GetByIdAsync("A",order);Assert.NotNull(originalOrder);
        }
        var complete=Beep.OilandGas.ApiService.Tests.WorkOrderCompleteApprovalFixture.Create(phaseAware,coreDatabase:database,fieldId:"A",captureInspectionRows:requirements?rows=>f.Rows.AddRange(rows):null,originalOrder:originalOrder);var decision=new CompanyWorkOrderExecutionPermitDecision(complete.Review,Guid.NewGuid(),"Approve","Private original reviewer rationale");var recorded=decision.Prepare(complete.Pending,DateTime.UtcNow);f.Rows.Add(recorded);if(!requirements)await f.Grant("WORK_ORDER",decision.Review.Proposal.Task.WorkOrderId,"WRITE");
        var creator=new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object);f.Editor.SetupGet(value=>value.classCreator).Returns(creator);f.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT")).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_EXECUTION_PERMIT"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_PERMIT)));
        var native=new WorkOrderExecutionPermitService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        if(requirements){f.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName="WORK_ORDER_INSPECTION",EntityTypeName=typeof(WORK_ORDER_INSPECTION).FullName!,PrimaryKeyColumn="INSPECTION_ID"});f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(["INSPECTION_ID"]);f.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_INSPECTION")).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_INSPECTION"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_INSPECTION)));}
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),_=>Mock.Of<IProcessService>()),production,boundary.Orchestrator.Object,workOrders:owners.Order,inspections:owners.Inspections,executionPermits:native);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceHandoffController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});var url=host.Url+"/api/field/current/maintenance-handoffs/execution-permit/"+(requirements?"execution-requirements":"recorded-approval");var query=decision.ProposalReference();
        int PrivateReads()=>f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="WORK_ORDER_EXECUTION_PERMIT");var reads=PrivateReads();using(var denied=await host.Http.PostAsJsonAsync(url,query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(reads,PrivateReads());
        if(requirements && phaseAware)
        {
            var startPrivateReads=f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && new[]{"WORK_ORDER_EXECUTION_START","WORK_ORDER_EXECUTION_PERMIT","WORK_ORDER_PERSONNEL_ASSIGNMENT","COMPANY_PERSONNEL_IDENTITY","RM_FILE_CONTENT"}.Contains(value.Arguments[0] as string,StringComparer.Ordinal));
            foreach(var (path,body) in new[]{("work-start/review",(object)query),("work-start/recovery/reference",(object)new CompanyWorkOrderExecutionStartReference(query,Guid.NewGuid())),("work-submission/inspection-review",(object)new CompanyWorkOrderExecutionStartReference(query,Guid.NewGuid())),("work-submission/review",(object)new CompanyWorkOrderExecutionStartReference(query,Guid.NewGuid())),("work-submission/recovery/reference",(object)new CompanyWorkOrderSubmissionReference(new(query,Guid.NewGuid()),Guid.NewGuid())),("material-usage/review",(object)new CompanyWorkOrderExecutionStartReference(query,Guid.NewGuid())),("material-usage/recovery/reference",(object)new CompanyWorkOrderMaterialUsageReference(new(query,Guid.NewGuid()),Guid.NewGuid()))})
            {
                using var startDenied=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/execution-permit/"+path,body);Assert.True(startDenied.StatusCode==HttpStatusCode.Forbidden,path+" returned "+startDenied.StatusCode);Assert.Equal("no-store",startDenied.Headers.CacheControl?.ToString());
            }
            Assert.Equal(startPrivateReads,f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && new[]{"WORK_ORDER_EXECUTION_START","WORK_ORDER_EXECUTION_PERMIT","WORK_ORDER_PERSONNEL_ASSIGNMENT","COMPANY_PERSONNEL_IDENTITY","RM_FILE_CONTENT"}.Contains(value.Arguments[0] as string,StringComparer.Ordinal)));
            var executionUrl=host.Url+"/api/field/current/maintenance-handoffs/execution-permit/execution-prerequisites";
            using(var denied=await host.Http.PostAsJsonAsync(executionUrl,query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);
            Assert.Equal(reads,PrivateReads());
            var executionPermission=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ExecuteWork"};f.Db.RoleClaims.Add(executionPermission);await f.Db.SaveChangesAsync();
            using(var missingInventory=await host.Http.PostAsJsonAsync(executionUrl,query))Assert.Equal(HttpStatusCode.Forbidden,missingInventory.StatusCode);
            Assert.Equal(reads,PrivateReads()); // Keep the newly captured grant for this fixture's remaining requests.
        }
        var permission=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ViewApprovedPermit"};f.Db.RoleClaims.Add(permission);await f.Db.SaveChangesAsync();
        using(var response=await host.Http.PostAsJsonAsync(url,query))
        {
            Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());var body=await response.Content.ReadAsStringAsync();Assert.DoesNotContain(decision.Rationale,body);Assert.DoesNotContain("DECISION_DATA_JSON",body);Assert.DoesNotContain("CurrentMaterialAcceptance",body);Assert.Equal("no-store",response.Headers.CacheControl?.ToString());
            CompanyWorkOrderRecordedApproval result;
            if(requirements){var comparison=(await response.Content.ReadFromJsonAsync<CompanyWorkOrderExecutionRequirements>())!;comparison.Require(f.Gate.Company.Scope,f.Gate.Company.UserId,query);Assert.Empty(comparison.Blockers);Assert.Equal(complete.Review.CurrentSource.Inspections.Count,comparison.Requirements.Count);if(phaseAware)Assert.Equal("UNRECORDED",comparison.Requirements.Single(row=>row.InspectionId=="future-operation").Status);result=comparison.Approval;}
            else result=(await response.Content.ReadFromJsonAsync<CompanyWorkOrderRecordedApproval>())!;result.Require(f.Gate.Company.Scope,query);Assert.Equal(decision.IntentId,result.DecisionId);Assert.NotEqual(f.Gate.Company.UserId,result.Approver);
        }
        if(requirements)
        {
            var header=f.Rows.OfType<WORK_ORDER>().Single(row=>row.WORK_ORDER_ID==query.Task.WorkOrderId);var instructions=header.INSTRUCTIONS;header.INSTRUCTIONS="Changed current native order scope";
            using(var changedOrder=await host.Http.PostAsJsonAsync(url,query)){Assert.True(changedOrder.StatusCode==HttpStatusCode.OK,await changedOrder.Content.ReadAsStringAsync());var compared=(await changedOrder.Content.ReadFromJsonAsync<CompanyWorkOrderExecutionRequirements>())!;compared.Require(f.Gate.Company.Scope,f.Gate.Company.UserId,query);Assert.Contains(compared.Blockers,value=>value.StartsWith("Original approved order scope",StringComparison.Ordinal));}header.INSTRUCTIONS=instructions;
            if(phaseAware)
            {
                var future=f.Rows.OfType<WORK_ORDER_INSPECTION>().Single(row=>row.INSPECTION_ID=="future-operation");var text=future.REQUIRED_TEXT;future.REQUIRED_TEXT="Changed future native requirement";
                using(var changedRequirement=await host.Http.PostAsJsonAsync(url,query)){Assert.True(changedRequirement.StatusCode==HttpStatusCode.OK,await changedRequirement.Content.ReadAsStringAsync());var compared=(await changedRequirement.Content.ReadFromJsonAsync<CompanyWorkOrderExecutionRequirements>())!;compared.Require(f.Gate.Company.Scope,f.Gate.Company.UserId,query);Assert.Contains(compared.Blockers,value=>value.StartsWith("Original approved native requirements",StringComparison.Ordinal));}future.REQUIRED_TEXT=text;
            }
        }
        recorded.BUSINESS_ASSOCIATE_ID="another-company";
        using(var foreign=await host.Http.PostAsJsonAsync(url,query)){Assert.Equal(HttpStatusCode.Forbidden,foreign.StatusCode);Assert.DoesNotContain(decision.Rationale,await foreign.Content.ReadAsStringAsync());}recorded.BUSINESS_ASSOCIATE_ID=decision.Review.Context.Company.BusinessAssociateId;
        var scope=recorded.SCOPE_DESCRIPTION;recorded.SCOPE_DESCRIPTION="Changed stored body";
        using(var changed=await host.Http.PostAsJsonAsync(url,query))Assert.Equal(HttpStatusCode.Conflict,changed.StatusCode);recorded.SCOPE_DESCRIPTION=scope;
        f.Db.RoleClaims.Remove(permission);await f.Db.SaveChangesAsync();reads=PrivateReads();using(var revoked=await host.Http.PostAsJsonAsync(url,query))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(reads,PrivateReads());Assert.Empty(f.Writes);
    }
}
