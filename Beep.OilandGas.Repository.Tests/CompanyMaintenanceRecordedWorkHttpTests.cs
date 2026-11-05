using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public async Task RecordedWorkLandingHttpDiscoversExactReportsWithReadRightsAndNeverMutates()
    {
        using var f=await Ready();var permission=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"};f.Db.RoleClaims.Add(permission);await f.Db.SaveChangesAsync();await f.Select("company");
        const string order="order-الكويت";await f.Grant("WORK_ORDER",order,"READ");f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,ACTIVE_IND="Y"});f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,FIELD_ID="A",COMPONENT_ID="component",ACTIVE_IND="Y"});
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var name=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(name,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],name));var owners=MaintenanceReadOwners(f,modules,profile);
        var sourceRows=new List<WORK_ORDER_EXECUTION_SUBMISSION>();
        for(var index=0;index<2;index++){var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(f.Gate.Company.Scope,Guid.NewGuid()),"A",order);var original=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(database,"A",taskOverride:task);var row=original.Command.Prepare(DateTime.UtcNow);sourceRows.Add(row);f.Rows.Add(row);}
        // A colliding original report ID owned by another company must never be decoded or counted.
        var foreign=CompanyInventorySourceReview.Copy(sourceRows[0]);foreign.OWNER_COMPANY_BA_ID="other-company";foreign.COMMAND_JSON="{";f.Rows.Add(foreign);
        var creator=new ClassCreator(f.Editor.Object);f.Editor.SetupGet(editor=>editor.classCreator).Returns(creator);f.Source.Setup(source=>source.CheckEntityExist("WORK_ORDER_EXECUTION_SUBMISSION")).Returns(true);f.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_EXECUTION_SUBMISSION"),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_SUBMISSION));foreach(var column in shape.Fields)column.IsKey=column.FieldName=="SUBMISSION_ID";return shape;});
        f.Metadata.Setup(metadata=>metadata.GetTableMetadataAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_SUBMISSION",EntityTypeName=typeof(WORK_ORDER_EXECUTION_SUBMISSION).FullName!,PrimaryKeyColumn="SUBMISSION_ID"});f.Metadata.Setup(metadata=>metadata.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
        f.Source.Setup(source=>source.GetEntityAsync("WORK_ORDER_EXECUTION_SUBMISSION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string _,List<AppFilter> filters)=>f.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Where(row=>filters.All(filter=>{var value=typeof(WORK_ORDER_EXECUTION_SUBMISSION).GetProperty(filter.FieldName)!.GetValue(row);if(filter.FieldName=="ROW_CREATED_DATE"){var utc=DateTime.Parse(filter.FilterValue,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);return filter.Operator==">="?(DateTime)value!>=utc:(DateTime)value!<utc;}return value?.ToString()==filter.FilterValue;})).Select(CompanyInventorySourceReview.Copy).ToArray());
        var reports=new WorkOrderExecutionPermitService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),Mock.Of<IProcessService>(),production,boundary.Orchestrator.Object,owners.Order,executionPermits:reports);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceRecordedWorkController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});var url=host.Url+"/api/field/current/maintenance-handoffs/recorded-work/pages/";var window=new CompanyMaintenanceRecordedWorkWindow("A",DateTime.UtcNow.AddMinutes(-5),DateTime.UtcNow.AddMinutes(5));
        foreach(var page in new[]{1,2})
        {
            using var response=await host.Http.PostAsJsonAsync(url+page+"/1",window);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());var result=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceRecordedWorkPage>())!;result.Require(f.Gate.Company.Scope,"actor",window,page,1);Assert.Equal(2,result.TotalRecords);Assert.Single(result.Rows);Assert.Contains("closureSubmission=",result.Rows[0].DetailRoute(result.Context.Company));Assert.DoesNotContain("COMMAND_JSON",await response.Content.ReadAsStringAsync());Assert.Empty(f.Writes);
        }
        using(var wrongField=await host.Http.PostAsJsonAsync(url+"1/1",window with{FieldId="B"}))Assert.Equal(HttpStatusCode.Forbidden,wrongField.StatusCode);
        using(var foreignOrder=await host.Http.PostAsJsonAsync(url+"1/1",window with{WorkOrderId="ungranted-order"}))Assert.Equal(HttpStatusCode.Forbidden,foreignOrder.StatusCode);
        using(var badPage=await host.Http.PostAsJsonAsync(url+"0/1",window))Assert.Equal(HttpStatusCode.BadRequest,badPage.StatusCode);
        using(var exact=await host.Http.PostAsJsonAsync(url+"1/8",window with{WorkOrderId=order})){Assert.Equal(HttpStatusCode.OK,exact.StatusCode);Assert.Equal(2,(await exact.Content.ReadFromJsonAsync<CompanyMaintenanceRecordedWorkPage>())!.TotalRecords);}
        using(var outside=await host.Http.PostAsJsonAsync(url+"1/8",window with{FromUtc=DateTime.UtcNow.AddDays(1),ToUtc=DateTime.UtcNow.AddDays(2)})){Assert.Equal(HttpStatusCode.OK,outside.StatusCode);Assert.Equal(0,(await outside.Content.ReadFromJsonAsync<CompanyMaintenanceRecordedWorkPage>())!.TotalRecords);}
        var grant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="WORK_ORDER" && row.AssetId==order);grant.IsActive=false;await f.Db.SaveChangesAsync();
        var beforeRevokedOrder=f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WORK_ORDER_EXECUTION_SUBMISSION");
        using(var revokedOrder=await host.Http.PostAsJsonAsync(url+"1/1",window with{WorkOrderId=order}))Assert.Equal(HttpStatusCode.Forbidden,revokedOrder.StatusCode);
        Assert.Equal(beforeRevokedOrder,f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WORK_ORDER_EXECUTION_SUBMISSION"));grant.IsActive=true;await f.Db.SaveChangesAsync();
        f.Db.RoleClaims.Remove(permission);await f.Db.SaveChangesAsync();var reads=f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WORK_ORDER_EXECUTION_SUBMISSION");
        using(var revoked=await host.Http.PostAsJsonAsync(url+"1/1",window))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(reads,f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WORK_ORDER_EXECUTION_SUBMISSION"));Assert.Empty(f.Writes);
    }
}
