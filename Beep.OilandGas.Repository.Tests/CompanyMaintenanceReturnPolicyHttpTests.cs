using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public async Task MaintenanceReturnPolicyHttpRequiresCurrentAdminPermissionAndConfiguredRolesAndRecoversOriginalAppend()
    {
        using var f=await Ready();f.EnableCorrectiveActionTransactions();f.Db.Roles.Add(new IdentityRole{Id="facility-return",Name="FacilityRole",NormalizedName="FACILITYROLE",ConcurrencyStamp="return-role-v1"});var returnPermission=new IdentityRoleClaim<string>{RoleId="facility-return",ClaimType="permission",ClaimValue="maintenance.returntoservice"};f.Db.RoleClaims.Add(returnPermission);await f.Db.SaveChangesAsync();await f.Select("company");
        var table=typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY).Name;var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase))[table];f.Metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(metadata);f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(["POLICY_RECORD_ID"]);var creator=new ClassCreator(f.Editor.Object);f.Editor.SetupGet(value=>value.classCreator).Returns(creator);f.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY)));
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var service=new CompanyMaintenanceReturnPolicyService(f.Gate,modules,f.Db,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceReturnPoliciesController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});var path=host.Url+"/api/company/maintenance-return-policies";
        int Reads()=>f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string==table);
        using(var denied=await host.Http.GetAsync(path+"/review"))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(0,Reads());Assert.Empty(f.Writes);
        var configure=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ConfigureReturnPolicy"};f.Db.RoleClaims.Add(configure);await f.Db.SaveChangesAsync();
        CompanyMaintenanceReturnPolicyCatalog catalog;using(var response=await host.Http.GetAsync(path+"/review")){Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.True(response.Headers.CacheControl?.NoStore);catalog=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceReturnPolicyCatalog>())!;catalog.Require();Assert.Empty(catalog.Versions);}
        var command=new CompanyMaintenanceReturnPolicyCommand(catalog.Context,catalog.Revision,CompanyMaintenanceReturnPolicyTests.Draft(DateTime.UtcNow),1,Guid.NewGuid(),"Private retained maintenance policy rationale");var before=Reads();
        using(var unknownRole=await host.Http.PostAsJsonAsync(path+"/record",command with{Policy=command.Policy with{AuthorizedReturnRoles=["UnknownRole"]}}))Assert.Equal(HttpStatusCode.BadRequest,unknownRole.StatusCode);Assert.Equal(before,Reads());Assert.Empty(f.Writes);
        using(var wrongPermissionCase=await host.Http.PostAsJsonAsync(path+"/record",command))Assert.Equal(HttpStatusCode.BadRequest,wrongPermissionCase.StatusCode);Assert.Equal(before,Reads());Assert.Empty(f.Writes);
        returnPermission.ClaimValue="Maintenance.ReturnToService";await f.Db.SaveChangesAsync();using(var refreshed=await host.Http.GetAsync(path+"/review")){Assert.Equal(HttpStatusCode.OK,refreshed.StatusCode);catalog=(await refreshed.Content.ReadFromJsonAsync<CompanyMaintenanceReturnPolicyCatalog>())!;}command=command with{Context=catalog.Context,ExpectedCatalogRevision=catalog.Revision};
        WORK_ORDER_MAINTENANCE_RETURN_POLICY receipt;using(var recorded=await host.Http.PostAsJsonAsync(path+"/record",command)){Assert.True(recorded.StatusCode==HttpStatusCode.OK,await recorded.Content.ReadAsStringAsync());Assert.True(recorded.Headers.CacheControl?.NoStore);receipt=(await recorded.Content.ReadFromJsonAsync<WORK_ORDER_MAINTENANCE_RETURN_POLICY>())!;command.RequireRecorded(receipt);}Assert.Single(f.Writes);Assert.Equal(new[]{"begin","commit"},f.LeaseTransactions);
        var reference=new CompanyMaintenanceReturnPolicyReference(command.RecordId(),command.IntentId);using(var recovered=await host.Http.PostAsJsonAsync(path+"/recovery/reference",reference)){Assert.True(recovered.StatusCode==HttpStatusCode.OK,await recovered.Content.ReadAsStringAsync());var original=(await recovered.Content.ReadFromJsonAsync<CompanyMaintenanceReturnPolicyRecovery>())!;original.Require(command.Context.Company,command.Context.ActorUserId,reference);Assert.Equal(JsonSerializer.Serialize(receipt),JsonSerializer.Serialize(original.Recorded));}Assert.Single(f.Writes);
        using(var substituted=await host.Http.PostAsJsonAsync(path+"/record",command with{IntentId=Guid.NewGuid()}))Assert.Equal(HttpStatusCode.Conflict,substituted.StatusCode);Assert.Single(f.Writes);
        using(var history=await host.Http.GetAsync(path+"/review")){Assert.Equal(HttpStatusCode.OK,history.StatusCode);var body=await history.Content.ReadAsStringAsync();Assert.DoesNotContain("COMMAND_JSON",body);Assert.DoesNotContain(command.IntentId.ToString("D"),body);Assert.DoesNotContain(command.Reason,body);var result=JsonSerializer.Deserialize<CompanyMaintenanceReturnPolicyCatalog>(body,new JsonSerializerOptions(JsonSerializerDefaults.Web))!;result.Require();Assert.Single(result.Versions);}
        // Configured return-role changes invalidate new appends, while current administrators may still recover immutable older receipts.
        f.Db.RoleClaims.Remove(returnPermission);await f.Db.SaveChangesAsync();before=Reads();using(var stale=await host.Http.PostAsJsonAsync(path+"/record",command))Assert.Equal(HttpStatusCode.Forbidden,stale.StatusCode);Assert.Equal(before,Reads());
        using(var historical=await host.Http.PostAsJsonAsync(path+"/recovery/reference",reference))Assert.True(historical.StatusCode==HttpStatusCode.OK,await historical.Content.ReadAsStringAsync());Assert.Single(f.Writes);
        f.Db.RoleClaims.Remove(configure);await f.Db.SaveChangesAsync();before=Reads();using(var denied=await host.Http.PostAsJsonAsync(path+"/recovery/reference",reference))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(before,Reads());Assert.Single(f.Writes);
        Assert.All(f.Writes,row=>Assert.IsType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>(row));Assert.DoesNotContain(f.Source.Invocations,call=>call.Method.Name is "CreateEntityAs" or "ExecuteSql" or "RunScript");
    }
}
