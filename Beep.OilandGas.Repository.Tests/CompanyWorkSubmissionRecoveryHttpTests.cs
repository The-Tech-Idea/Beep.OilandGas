using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
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
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public async Task ActualOwnSubmissionRecoveryHttpUsesCurrentAppAuthorityAndNeverReplaysRecordedReport()
    {
        // Controlled already-recorded canonical report; this gate does not certify its write endpoint.
        using var f=await Ready();await f.Grant("FIELD","A","WRITE");var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName="selected-tenant",ConnectionKey="SELECTED-TENANT",ConcurrencyStamp="lifecycle-version"});await f.Db.SaveChangesAsync();
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var core=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(core,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core));
        var original=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(database,"A",f.Gate.Company.UserId,"Administrator");var command=original.Command;var row=command.Prepare(DateTime.UtcNow);f.Rows.Add(row);await f.Grant("WORK_ORDER",command.Review.Inspections.Start.Permit.Task.WorkOrderId,"WRITE");
        var type=typeof(WORK_ORDER_EXECUTION_SUBMISSION);var table=type.Name;var creator=new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object);f.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        f.Metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn="SUBMISSION_ID"});f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(["SUBMISSION_ID"]);f.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>creator.ConvertToEntityStructure(type));
        var native=new WorkOrderExecutionPermitService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),_=>Mock.Of<IProcessService>()),production,boundary.Orchestrator.Object,executionPermits:native);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceHandoffController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});var path=host.Url+"/api/field/current/maintenance-handoffs/execution-permit/work-submission/recovery/reference";var reference=new CompanyWorkOrderSubmissionReference(new(command.Review.Inspections.Start.Permit,original.Start.IntentId),command.IntentId);
        int Reads()=>f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string==table);
        var before=Reads();using(var denied=await host.Http.PostAsJsonAsync(path,reference))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(before,Reads());
        var permission=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ExecuteWork"};f.Db.RoleClaims.Add(permission);await f.Db.SaveChangesAsync();
        using(var recovered=await host.Http.PostAsJsonAsync(path,reference)){Assert.True(recovered.StatusCode==HttpStatusCode.OK,await recovered.Content.ReadAsStringAsync());Assert.Equal("no-store",recovered.Headers.CacheControl?.ToString());var result=(await recovered.Content.ReadFromJsonAsync<CompanyWorkOrderSubmissionRecovery>())!;result.Require(f.Gate.Company.Scope,f.Gate.Company.UserId,reference);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(result.Command));}
        using(var wrong=await host.Http.PostAsJsonAsync(path,reference with{IntentId=Guid.NewGuid()}))Assert.Equal(HttpStatusCode.Forbidden,wrong.StatusCode);
        row.EXECUTOR_USER_ID="foreign";row.COMMAND_JSON="malformed private report";using(var foreign=await host.Http.PostAsJsonAsync(path,reference)){Assert.Equal(HttpStatusCode.Forbidden,foreign.StatusCode);Assert.DoesNotContain(command.WorkPerformed,await foreign.Content.ReadAsStringAsync());}
        Assert.Empty(f.Writes);Assert.DoesNotContain(f.Source.Invocations,call=>call.Method.Name is "InsertEntity" or "UpdateEntity");
    }
}
