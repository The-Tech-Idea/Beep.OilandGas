using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    // Explicit authentication seam; standard role claims come from the actual selected app company, not a token.
    private sealed class HandoffRoleBridge(CompanyModuleConnection company):IClaimsTransformation
    {
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal user)
        {
            if(user.Identity?.IsAuthenticated!=true)return user;
            var selected=await company.RefreshCompanyAsync();var identity=new ClaimsIdentity("handoff-role-fixture");
            foreach(var role in selected.Roles)identity.AddClaim(new(ClaimTypes.Role,role));user.AddIdentity(identity);return user;
        }
    }
    [Fact]
    public async Task ActualMaintenanceReviewRejectsMissingOriginalReferencesBeforeProductionReads()
    {
        using var f=await Ready();await f.Grant("FIELD","A","WRITE");var profile=Profile(f);await Save(profile);
        using var boundary=new Boundary(f,profile);var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var owner=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),Mock.Of<IProcessService>(),production,boundary.Orchestrator.Object);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceHandoffController),services=>{services.AddSingleton(owner);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});
        using var response=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/review",new CompanyMaintenanceProcessStartQuery("",Guid.Empty,"",""));
        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);Assert.Empty(f.Writes);
        Assert.DoesNotContain(f.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WELL_INTERVENTION_INTENT");
    }
    [Fact] public Task ActualOriginalMaintenanceRecoveryHttp()=>ActualMaintenanceHandoffReviewAndStartUseOriginalNativeReceiptAndScopedAppAuthority("allowed");
    [Theory][InlineData("order-read-only")][InlineData("revoke-during-start")]
    public async Task ActualMaintenanceHandoffReviewAndStartUseOriginalNativeReceiptAndScopedAppAuthority(string fault)
    {
        // Skipped dependency builds must still supply the current transaction owner at runtime.
        Assert.NotNull(typeof(Beep.OilandGas.PPDM39.Core.PPDMDataSourceTransaction).GetMethod("RequireIdle",[typeof(TheTechIdea.Beep.IDataSource)]));
        using var f=await Ready();await f.Grant("FIELD","A","WRITE");await f.Grant("WELL","well/الكويت%2F","WRITE");await f.Grant("WORK_ORDER","order/العالم%2F",fault=="order-read-only"?"READ":"WRITE");
        var profile=Profile(f);await Save(profile);CompanyProductionBoundaryTests.NativeDecisionSource(f);CompanyProductionBoundaryTests.NativeDecisionProvider(f);
        const string well="well/الكويت%2F",order="order/العالم%2F";
        f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,WORK_ORDER_TYPE=WorkOrderSubType.Corrective,INSTRUCTIONS="Recorded original scope",ACTIVE_IND="Y"});
        f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,COMPONENT_ID="component",FIELD_ID="A",UWI=well,ACTIVE_IND="Y"});
        var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var request=await CompanyProductionBoundaryTests.NativeDecisionRequest(f,profile);request.WorkOrderId=order;
        var outcome=await production.RecordNativeInterventionDecisionAsync(well,request,"actor");var priorWrites=f.Writes.Count;
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName="selected-tenant",ConnectionKey="SELECTED-TENANT",ConcurrencyStamp="lifecycle-version"});await f.Db.SaveChangesAsync();
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        var roles=new Mock<IRolePersonaReader>();roles.Setup(value=>value.GetActivePersonasAsync("Administrator")).ReturnsAsync(["MAINTENANCE"]);
        var process=new PPDMProcessService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,"selected-tenant",personaReader:roles.Object);
        var bound=new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),connection=>process);
        foreach(var pair in new[]{("PROCESS_DEFINITION","PROCESS_DEFINITION_ID"),("PROCESS_INSTANCE","PROCESS_INSTANCE_ID"),("PROCESS_STEP_INSTANCE","PROCESS_STEP_INSTANCE_ID"),("PROCESS_HISTORY","PROCESS_HISTORY_ID"),("PROCESS_APPROVAL","PROCESS_APPROVAL_ID"),("CROSS_PERSONA_TASK","CROSS_TASK_ID")})
        {
            f.Metadata.Setup(value=>value.GetTableMetadataAsync(pair.Item1)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{EntityTypeName=pair.Item1,PrimaryKeyColumn=pair.Item2});
            f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(pair.Item1)).ReturnsAsync([pair.Item2]);
        }
        f.Rows.Add(new PROCESS_DEFINITION{PROCESS_DEFINITION_ID="maintenance-review",PROCESS_NAME="Configured receiving review",PROCESS_TYPE="WORK_ORDER",ENTITY_TYPE="WORK_ORDER",ACTIVE_IND="Y",IS_ACTIVE="Y",VERSION="1",PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="receive",StepName="Receiving review",StepType="APPROVAL",SequenceNumber=1,RequiresApproval=true,RequiredRoles=["Administrator"]}}})});
        using var boundary=new Boundary(f,profile);
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),bound,production,boundary.Orchestrator.Object);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceHandoffController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});
        var durations=new System.Collections.Concurrent.ConcurrentDictionary<string,(int Count,double Milliseconds)>();
        using var listener=new System.Diagnostics.ActivityListener
        {
            ShouldListenTo=source=>source.Name is "Beep.OilandGas.WorkOrderPermitDecision" or "Beep.OilandGas.ProductionIntervention",
            Sample=(ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _)=>System.Diagnostics.ActivitySamplingResult.AllData,
            ActivityStopped=activity=>durations.AddOrUpdate(activity.OperationName,(1,activity.Duration.TotalMilliseconds),(_,before)=>(before.Count+1,before.Milliseconds+activity.Duration.TotalMilliseconds))
        };
        System.Diagnostics.ActivitySource.AddActivityListener(listener);
        Dictionary<string,int> Reads()=>f.Source.Invocations.Where(call=>call.Method.Name=="GetEntityAsync").GroupBy(call=>call.Arguments[0]?.ToString()??"unknown").ToDictionary(group=>group.Key,group=>group.Count());
        var beforeReads=Reads();
        async Task<HttpResponseMessage> ObserveReview()
        {
            try{return await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/review",new CompanyMaintenanceProcessStartQuery("maintenance-review",outcome.IntentId,well,order));}
            finally
            {
                var reads=Reads().ToDictionary(pair=>pair.Key,pair=>pair.Value-beforeReads.GetValueOrDefault(pair.Key));
                var observed=new{Scenario=fault,Stages=durations.OrderBy(pair=>pair.Key).Select(pair=>new{Stage=pair.Key,pair.Value.Count,pair.Value.Milliseconds}),Reads=reads.Where(pair=>pair.Value>0).ToDictionary(pair=>pair.Key,pair=>pair.Value)};
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"oilgas-335-handoff-profile-"+fault+".json"),JsonSerializer.Serialize(observed,new JsonSerializerOptions{WriteIndented=true}));
            }
        }
        using var reviewResponse=await ObserveReview();
        if(fault=="order-read-only"){Assert.Equal(HttpStatusCode.Forbidden,reviewResponse.StatusCode);Assert.Equal(priorWrites,f.Writes.Count);return;}
        Assert.Equal(HttpStatusCode.OK,reviewResponse.StatusCode);var review=(await reviewResponse.Content.ReadFromJsonAsync<CompanyMaintenanceProcessStartReview>())!;Assert.Equal(outcome.NativeReceiptRevision,review.Source.ReceiptRevision);Assert.Equal(order,review.Source.WorkOrderId);
        if(fault=="allowed")
        {
            f.Rows.Add(new PROCESS_DEFINITION{PROCESS_DEFINITION_ID="equipment-template",PROCESS_NAME="Equipment workflow",PROCESS_TYPE="WORK_ORDER",ENTITY_TYPE="EQUIPMENT",ACTIVE_IND="Y",IS_ACTIVE="Y",VERSION="1"});
            using var choicesResponse=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/choices",new CompanyMaintenanceSourceQuery(outcome.IntentId,well,order));
            Assert.Equal(HttpStatusCode.OK,choicesResponse.StatusCode);var choices=(await choicesResponse.Content.ReadFromJsonAsync<CompanyMaintenanceStartCatalog>())!;
            choices.Require(review.Context.Company,"actor",new(outcome.IntentId,well,order));Assert.Equal("maintenance-review",Assert.Single(choices.Choices).ProcessId);Assert.Equal(priorWrites,f.Writes.Count);
        }
        var intent=review.CreateIntent(Guid.NewGuid());var revoked=false;
        using var missingRecovery=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/recovery",intent);
        Assert.Equal(HttpStatusCode.Conflict,missingRecovery.StatusCode);Assert.Equal(priorWrites,f.Writes.Count);Assert.Empty(f.Rows.OfType<PROCESS_INSTANCE>());
        if(fault=="revoke-during-start")f.AfterLeaseWrite=row=>{if(row is PROCESS_INSTANCE){revoked=true;var grant=f.Db.Set<AppUserAssetAccess>().Single(value=>value.AssetType=="WORK_ORDER" && value.AssetId==order);grant.IsActive=false;f.Db.SaveChanges();}};
        using var response=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs",intent);
        if(fault=="revoke-during-start"){Assert.True(revoked);Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Empty(f.Rows.OfType<PROCESS_INSTANCE>());Assert.Empty(f.Rows.OfType<CROSS_PERSONA_TASK>());return;}
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);var receipt=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceProcessStartReceipt>())!;receipt.Require(intent);
        Assert.Equal(priorWrites+4,f.Writes.Count);Assert.Single(f.Rows.OfType<PROCESS_INSTANCE>());Assert.Single(f.Rows.OfType<CROSS_PERSONA_TASK>());Assert.Null(f.LeaseActiveTransaction);
        using var repeated=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs",intent);Assert.Equal(HttpStatusCode.OK,repeated.StatusCode);Assert.Equal(priorWrites+4,f.Writes.Count);
        using var recovered=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/recovery",intent);
        Assert.Equal(HttpStatusCode.OK,recovered.StatusCode);var recovery=(await recovered.Content.ReadFromJsonAsync<CompanyMaintenanceProcessStartReceipt>())!;
        recovery.Require(intent);Assert.Equal(receipt.RecordedRevision,recovery.RecordedRevision);Assert.Equal(priorWrites+4,f.Writes.Count);
        var reference=new CompanyMaintenanceRecoveryQuery(intent.IntentId,outcome.IntentId,"A",well,order);
        using var referenceResponse=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/recovery/reference",reference);
        Assert.Equal(HttpStatusCode.OK,referenceResponse.StatusCode);var restored=(await referenceResponse.Content.ReadFromJsonAsync<CompanyMaintenanceRecovery>())!;
        restored.Require(review.Context.Company,"actor",reference);Assert.Equal(intent,restored.Intent);Assert.Equal(receipt.RecordedRevision,restored.Receipt.RecordedRevision);Assert.Equal(priorWrites+4,f.Writes.Count);
        var task=f.Rows.OfType<CROSS_PERSONA_TASK>().Single();f.Rows.Remove(task);
        using var incomplete=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/recovery",intent);
        Assert.Equal(HttpStatusCode.Conflict,incomplete.StatusCode);Assert.Equal(priorWrites+4,f.Writes.Count);Assert.Empty(f.Rows.OfType<CROSS_PERSONA_TASK>());
        using var incompleteReference=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/recovery/reference",reference);
        Assert.Equal(HttpStatusCode.Conflict,incompleteReference.StatusCode);Assert.Equal(priorWrites+4,f.Writes.Count);Assert.Empty(f.Rows.OfType<CROSS_PERSONA_TASK>());
    }
}
