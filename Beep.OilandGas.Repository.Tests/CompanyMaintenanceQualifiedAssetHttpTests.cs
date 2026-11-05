using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    private static CompanyMaintenanceQualifiedAssetEvidenceReader QualifiedHttpReader(CompanyPPDMRowAccessTests.Fixture f,ModuleConnectionResolver modules)=>new(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,f.Policy,Options.Create(f.EligibilityConfiguration),Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),new CompanyMaintenanceReturnPolicyService(f.Gate,modules,f.Db,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
    private static async Task PrepareQualifiedReturnHttpSources(CompanyPPDMRowAccessTests.Fixture f,string orderId)
    {
        // Reuse the reviewed native source fixture; no return, work report or lifecycle outcome is copied.
        var prepared=await CompanyMaintenanceQualifiedLicensingTests.QualifiedCommand();using var source=prepared.Fixture;
        f.Editor.SetupGet(editor=>editor.classCreator).Returns(new ClassCreator(f.Editor.Object));
        var names=new HashSet<string>(StringComparer.Ordinal){"AREA","R_AREA_TYPE","R_LIC_STATUS_TYPE","R_LICENSE_STATUS","SUBSTANCE","BUSINESS_ASSOCIATE","PPDM_UNIT_OF_MEASURE","PPDM_QUANTITY","PPDM_MEASUREMENT_SYSTEM","R_PPDM_UOM_USAGE","RESTRICTION","REST_CLASS","R_REST_TYPE","R_REST_DURATION","R_SOURCE","R_TIMEZONE","WORK_ORDER_MAINTENANCE_RETURN_POLICY"};
        bool Included(string table)=>names.Contains(table) || table.StartsWith("FACILITY",StringComparison.Ordinal) || table.StartsWith("EQUIPMENT",StringComparison.Ordinal) || table.StartsWith("R_FAC",StringComparison.Ordinal) || table.StartsWith("R_EQUIP",StringComparison.Ordinal);
        var rows=source.Native.Rows.Where(row=>Included(row.GetType().Name)).ToArray();
        // A declared empty violation/reference table is distinct from a missing schema.
        var tables=rows.Select(row=>row.GetType().Name).Concat(source.Native.Source.Invocations.Where(call=>call.Method.Name=="CheckEntityExist").Select(call=>(string)call.Arguments[0]).Where(Included)).Distinct(StringComparer.Ordinal).ToArray();
        foreach(var table in tables)
        {
            var type=rows.FirstOrDefault(row=>row.GetType().Name==table)?.GetType()??typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+table)!;Assert.NotNull(type);
            var metadata=await source.Native.Metadata.Object.GetTableMetadataAsync(table);Assert.NotNull(metadata);
            var keys=(await source.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(table)).ToArray();Assert.NotEmpty(keys);
            var shape=source.Native.Source.Object.GetEntityStructure(new EntityStructure{EntityName=table},true);Assert.NotNull(shape);
            f.Metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(metadata);f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);
            f.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName==table),true)).Returns(()=>CompanyInventorySourceReview.Copy(shape));
            foreach(var row in rows.Where(row=>row.GetType()==type && row is not WORK_ORDER_MAINTENANCE_RETURN_POLICY))
            {
                if(f.Rows.Any(existing=>existing.GetType()==type && keys.All(key=>Equals(type.GetProperty(key)!.GetValue(existing),type.GetProperty(key)!.GetValue(row)))))continue;
                f.Rows.Add(System.Text.Json.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(row,type),type)!);
            }
        }
        var core=await f.Gate.ResolveAsync();var policyContext=new CompanyMaintenanceReturnPolicyContext(f.Gate.Company.Scope,core,"policy-maker",new string('A',64));
        var orderType=f.Rows.OfType<WORK_ORDER>().Single(row=>row.WORK_ORDER_ID==orderId).WORK_ORDER_TYPE;
        if(!LeaseSummaryValidator.Identifier(orderType))throw new InvalidOperationException("Original native work-order type is required before preparing the explicit fixture policy.");
        var policy=CompanyMaintenanceReturnPolicyCommand.Read(source.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>().Single()).Policy with{WorkOrderType=orderType};
        var utc=DateTime.UtcNow;var empty=CompanyMaintenanceReturnPolicyCatalog.Create(policyContext,[],utc);
        f.Rows.Add(new CompanyMaintenanceReturnPolicyCommand(policyContext,empty.Revision,policy,1,Guid.NewGuid(),"Explicit recorded policy for protected mixed return").Prepare(utc));
        var component=f.Rows.OfType<WORK_ORDER_COMPONENT>().Single(row=>row.WORK_ORDER_ID==orderId);component.EQUIPMENT_ID="pump";component.FACILITY_ID="plant";component.FACILITY_TYPE="gas";
        await f.Grant("FACILITY","plant","READ",facilityType:"gas");await f.Grant("EQUIPMENT","pump","READ");
    }
    [Fact]
    public async Task QualifiedHttpSourceFixtureReadsCompleteIdentityStatusAndLicensingIncludingEmptyTables()
    {
        using var f=await Ready();await f.Grant("FIELD","A","READ");
        f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID="source-check-order",WORK_ORDER_TYPE=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSubType.Corrective,ACTIVE_IND="Y"});f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID="source-check-order",COMPONENT_ID="component",FIELD_ID="A",ACTIVE_IND="Y"});
        await PrepareQualifiedReturnHttpSources(f,"source-check-order");DeclareCompletionSourceSchema(f);var database=await f.Gate.ResolveAsync();
        var order=new Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel{InstanceId="source-check-order",FieldId="A",Source="WORK_ORDER",WoSubType=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSubType.Corrective,Components=[new("source-check-order","component","A","","pump"){FacilityId="plant",FacilityType="gas",NativeRevision=new string('A',64)}]};
        var context=new FieldSelectionReview(f.Gate.Company.Scope,database,f.Gate.Company.UserId,"source-schema-check","A");
        var owner=new Beep.OilandGas.ProductionOperations.Services.FacilityManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),database.ConnectionName,maintenanceAssetAccess:f.Policy,maintenanceEvidenceZone:f.EligibilityConfiguration.Companies.Single().TimeZoneId);
        var identity=await owner.ReadMaintenanceQualifiedAssetsAsync(context,order,()=>Task.CompletedTask);identity.Require(order);
        var originalScope=await owner.ReadMaintenanceQualifiedWorkAssetsAsync(context,order,()=>Task.CompletedTask,false);Assert.Equal(originalScope.IdentityRevision(),identity.IdentityRevision());
        var operating=await owner.ReadMaintenanceQualifiedOperatingAsync(context,order,identity,()=>Task.CompletedTask);Assert.NotEmpty(operating.Statuses);
        var licensing=await owner.ReadMaintenanceQualifiedLicensingAsync(context,order,identity,()=>Task.CompletedTask);Assert.NotEmpty(licensing.Licenses);Assert.Empty(licensing.Violations);
        var sources=identity with{Operating=operating,Licensing=licensing};var policyContext=CompanyMaintenanceQualifiedReturnPolicy.ContextFor(sources,["FacilityRole"]);
        var catalog=CompanyMaintenanceQualifiedReturnPolicy.ScopeCatalog(sources,order,await owner.ReadMaintenanceReturnPoliciesAsync(policyContext,()=>Task.CompletedTask));
        var references=await owner.ReadMaintenanceReturnNativeSourcesAsync(context,order,sources,catalog,()=>Task.CompletedTask);
        var policy=CompanyMaintenanceQualifiedReturnPolicy.Create(sources,order,catalog,["FacilityRole"],references,DateTime.UtcNow);Assert.Empty(policy.Blockers);Assert.All(catalog.Versions,row=>Assert.Equal(order.WoSubType,row.Policy.WorkOrderType));
        var utc=DateTime.UtcNow;f.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="source-check-person",BA_TYPE="PERSON",BA_LONG_NAME="Recorded source check person",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(10)});
        f.Rows.Add(new BA_EMPLOYEE{EMPLOYER_BA_ID=context.Company.BusinessAssociateId,EMPLOYEE_BA_ID="source-check-person",EMPLOYEE_OBS_NO=1,STATUS="Recorded active employment",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(10)});
        var people=new ContractorManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,database)),Microsoft.Extensions.Logging.Abstractions.NullLogger<ContractorManagementService>.Instance);
        var person=await people.ReadIdentityPersonAsync(new(context.Company,database,context.ActorUserId,new string('B',64)),"source-check-person",()=>Task.CompletedTask);Assert.Equal("source-check-person",person.PersonBaId);
        Assert.Empty(f.Writes);
    }
    [Theory][InlineData("equipment-only",false)][InlineData("current",true)]
    public async Task ProtectedRecordedQualifiedStatusUsesActualRetainedReturnWithCurrentReadOnlyAuthority(string mode,bool includeWell)
    {
        WORK_ORDER_EXECUTION_SUBMISSION? report=null;
        var ready=await CompanyMaintenanceQualifiedLicensingTests.QualifiedCommand(mode,includeWell,row=>report=row);using var fixture=ready.Fixture;var f=fixture.Native;var command=ready.Command;
        await CompanyMaintenanceQualifiedLicensingTests.ConfigureQualifiedReturnApi(fixture,command);
        var nativeApi=CompanyMaintenanceQualifiedLicensingTests.QualifiedReturnApi(fixture);
        // Canonical report/physical/closure setup remains a seam; this creates the actual complete native return and intent.
        var saved=await nativeApi.RecordMaintenanceAssetReturnAsync(command,role=>CompanyMaintenanceQualifiedLicensingTests.QualifiedReturnApiAuthority(fixture,command,role),_=>Task.FromResult(command.Review.Physical),_=>Task.FromResult<CompanyMaintenanceClosureFacts?>(command.Review.Assets.Closure));
        saved.Require(fixture.Context.Company,fixture.Context.ActorUserId,command.Reference());Assert.NotNull(report);f.Rows.Add(report);var writes=f.Writes.Count;
        await f.Grant("FIELD",command.Reference().Query.Task.FieldId,"READ");await f.Grant("WORK_ORDER",command.Reference().Query.Task.WorkOrderId,"READ");await f.Grant("FACILITY","plant","READ",facilityType:"gas");await f.Grant("EQUIPMENT","pump","READ");if(includeWell)await f.Grant("WELL","well","READ");
        f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row=>row.RoleId=="qualified-return-role").ToArrayAsync());
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"});
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName=fixture.Context.Database.ConnectionName,ConnectionKey=fixture.Context.Database.ConnectionName.ToUpperInvariant(),ConcurrencyStamp="qualified-public-lifecycle"});
        await f.Db.SaveChangesAsync();await f.Select(fixture.Context.Company.BusinessAssociateId,fixture.Context.ActorUserId);
        Assert.DoesNotContain("Workflow.Execute",f.Gate.Company.Permissions);Assert.DoesNotContain("Facilities.ManageEquipment",f.Gate.Company.Permissions);
        var reportMetadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase))["WORK_ORDER_EXECUTION_SUBMISSION"];
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(reportMetadata);f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
        f.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_EXECUTION_SUBMISSION"),true)).Returns(()=>{var shape=f.Editor.Object.classCreator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_SUBMISSION));foreach(var field in shape.Fields)field.IsKey=field.FieldName=="SUBMISSION_ID";return shape;});
        f.Rows.Add(new FIELD{FIELD_ID=command.Reference().Query.Task.FieldId,ACTIVE_IND="Y"});
        var profile=Profile(f);await Save(profile,command.Reference().Query.Task.FieldId);using var boundary=new Boundary(f,profile);boundary.Field=command.Reference().Query.Task.FieldId;
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var process=new Mock<IProcessService>();
        // A canonical original order read model and already-closed lifecycle facts are explicit independent boundaries here.
        var orders=new Mock<IWorkOrderService>();orders.Setup(owner=>owner.GetByIdAsync(command.Reference().Query.Task.FieldId,command.Reference().Query.Task.WorkOrderId)).ReturnsAsync(()=>CompanyInventorySourceReview.Copy(command.Review.Assets.Order));
        process.As<ICompanyMaintenanceClosureOwner>().Setup(owner=>owner.ReadMaintenanceClosureFactsAsync(It.IsAny<FieldSelectionReview>(),command.Reference().Query,It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview _,CompanyWorkOrderCompletionQuery _,Func<Task> authority)=>{await authority();return command.Review.Assets.Closure;});
        var qualified=new CompanyMaintenanceQualifiedAssetEvidenceReader(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,f.Policy,Options.Create(f.EligibilityConfiguration),Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),new CompanyMaintenanceReturnPolicyService(f.Gate,modules,f.Db,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        var wells=new CompanyMaintenanceWellEvidenceReader(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,f.Policy,Options.Create(f.EligibilityConfiguration),qualified);
        var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),_=>process.Object),production,boundary.Orchestrator.Object,orders.Object,executionPermits:new(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object),maintenanceWells:wells,qualifiedAssets:qualified);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceAssetReturnStatusController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});
        var url=host.Url+"/api/field/current/maintenance-handoffs/execution-permit/asset-return/status";
        using(var response=await host.Http.PostAsJsonAsync(url,command.Reference().Query))
        {
            Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());
            var status=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceAssetReturnStatus>())!;status.Require();Assert.NotNull(status.Recorded);Assert.Equal(saved.Receipt.ReturnId,status.Recorded.ReturnId);Assert.Equal(command.Review.Assets.Closure.Revision,status.Recorded.ClosureRevision);Assert.Equal(includeWell?1:0,status.CurrentWells.Count);
            Assert.Equal(mode=="equipment-only"?1:2,status.Recorded.QualifiedAssets!.Select(row=>(row.AssetType,row.AssetId,row.FacilityType)).Distinct().Count());Assert.All(status.CurrentQualifiedAssets!.Facilities,row=>Assert.Equal("READ",row.AccessLevel));Assert.All(status.CurrentQualifiedAssets.Equipment,row=>Assert.Equal("READ",row.AccessLevel));
            var json=await response.Content.ReadAsStringAsync();Assert.DoesNotContain(command.IntentId.ToString("D"),json);Assert.DoesNotContain(command.Reason,json);Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain("BEFORE_JSON",json);Assert.DoesNotContain("AFTER_JSON",json);Assert.Equal(writes,f.Writes.Count);
        }
        var original=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="EQUIPMENT" && row.AssetId=="pump");original.IsActive=false;await f.Db.SaveChangesAsync();
        using(var revoked=await host.Http.PostAsJsonAsync(url,command.Reference().Query))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(writes,f.Writes.Count);
    }
    private sealed class TargetReturnGateReached:Exception{}
    [Fact]
    public async Task ReturnRoleCatalogDeduplicatesExactClaimsAndRechecksTheConfiguredRoleDespiteAnotherRolesCapability()
    {
        using var f=await Ready();var scope=f.Gate.Company.Scope;
        f.Db.Roles.AddRange(new IdentityRole{Id="catalog-return-role",Name="FacilityRole",NormalizedName="FACILITYROLE"},new IdentityRole{Id="catalog-case-role",Name="CaseRole",NormalizedName="CASEROLE"});
        foreach(var id in new[]{"catalog-return-role","catalog-case-role"})
        {
            f.Db.UserRoles.Add(new IdentityUserRole<string>{UserId="actor",RoleId=id});
            f.Db.Add(new AppUserRoleExtension{UserRoleId="scope-"+id,TenantId=scope.TenantId,BusinessAssociateId=scope.BusinessAssociateId,UserId="actor",RoleId=id,GrantedByUserId="actor"});
        }
        var first=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"};
        var duplicate=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"};
        var configured=new IdentityRoleClaim<string>{RoleId="catalog-return-role",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"};
        f.Db.RoleClaims.AddRange(first,duplicate,configured,new IdentityRoleClaim<string>{RoleId="catalog-case-role",ClaimType="permission",ClaimValue="maintenance.returntoservice"});
        await f.Db.SaveChangesAsync();await f.Select("company");
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        var core=await modules.ResolveAsync("PPDM_CORE");var context=new FieldSelectionReview(scope,new(core,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core)),"actor",new string('A',64),"A");
        var service=new CompanyMaintenanceReturnPolicyService(f.Gate,modules,f.Db,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
        Assert.Equal(new[]{"Administrator","FacilityRole"},await service.ReadEvidenceReturnRolesAsync(context));
        // The account keeps the capability through Administrator; FacilityRole must still lose its own qualification.
        f.Db.RoleClaims.Remove(configured);await f.Db.SaveChangesAsync();
        Assert.Equal(new[]{"Administrator"},await service.ReadEvidenceReturnRolesAsync(context));
        f.Db.RoleClaims.RemoveRange(first,duplicate);await f.Db.SaveChangesAsync();
        var refusal=await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>service.ReadEvidenceReturnRolesAsync(context));
        Assert.Equal(Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden,refusal.Kind);Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("W","allowed")][InlineData("E","allowed")][InlineData("F","allowed")][InlineData("WEF","allowed")]
    [InlineData("W","missing")][InlineData("E","missing")][InlineData("F","missing")][InlineData("WEF","missing")]
    [InlineData("W","revoked")][InlineData("E","revoked")][InlineData("F","revoked")][InlineData("WEF","revoked")][InlineData("F","case")]
    public async Task ActualReturnAuthorityUsesCurrentOriginalTargetsAndAppCapabilities(string targets,string mode)
    {
        using var f=await Ready();
        var required=new List<string>{"Maintenance.ReturnToService","Workflow.Execute"};
        if(targets.Contains('W'))required.Add("WellManagement.UpdateWellStatus");if(targets.Contains('F'))required.Add("Facilities.Edit");if(targets.Contains('E'))required.Add("Facilities.ManageEquipment");
        var lastPermission=required[required.Count-1];
        foreach(var capability in required.Where(value=>mode!="missing" || value!=lastPermission))f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=mode=="case" && capability==lastPermission?capability.ToLowerInvariant():capability});
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName="selected-tenant",ConnectionKey="SELECTED-TENANT",ConcurrencyStamp="lifecycle-version"});await f.Db.SaveChangesAsync();await f.Select("company");await f.Grant("FIELD","A","WRITE");await f.Grant("WORK_ORDER","target-order","WRITE");
        f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID="target-order",WORK_ORDER_TYPE="CORRECTIVE",ACTIVE_IND="Y"});f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID="target-order",COMPONENT_ID="component",FIELD_ID="A",UWI=targets.Contains('W')?"well":null,EQUIPMENT_ID=targets.Contains('E')?"equipment":null,FACILITY_ID=targets.Contains('F')?"facility":null,FACILITY_TYPE=targets.Contains('F')?"gas":null,ACTIVE_IND="Y"});
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);boundary.Field="A";var user=await new HandoffRoleBridge(f.Gate).TransformAsync(f.Request.User);f.Request.User=user;
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var owners=MaintenanceReadOwners(f,modules,profile);
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(f.Gate.Company.Scope,Guid.NewGuid()),"A","target-order");var query=new CompanyWorkOrderCompletionQuery(task,"original-submission");
        var process=new Mock<IProcessService>();process.As<ICompanyWorkOrderReturnOwner>();process.As<ICompanyMaintenanceClosureOwner>();var reached=0;
        process.As<ICompanyMaintenanceClosedReturnOwner>().Setup(value=>value.ReadClosedReturnSourceAsync(It.IsAny<FieldSelectionReview>(),query,It.IsAny<string>(),false,It.IsAny<Func<string?,Task>>())).Returns(async(FieldSelectionReview _,CompanyWorkOrderCompletionQuery _,string _,bool _,Func<string?,Task> authority)=>
        {
            reached++;if(mode=="revoked"){f.Db.RoleClaims.Remove(await f.Db.RoleClaims.SingleAsync(row=>row.ClaimValue==lastPermission));await f.Db.SaveChangesAsync();}
            await authority("Administrator");throw new TargetReturnGateReached();
        });
        var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),_=>process.Object),production,boundary.Orchestrator.Object,owners.Order,executionPermits:new(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object),maintenanceWells:Mock.Of<ICompanyMaintenanceAssetReturnOwner>());
        var error=await Record.ExceptionAsync(()=>service.ReviewMaintenanceAssetReturnAsync(user,query));
        if(mode=="allowed"){Assert.IsType<TargetReturnGateReached>(error);Assert.Equal(1,reached);}
        else{Assert.True(error is UnauthorizedAccessException or Beep.OilandGas.Models.Core.Refusals.RefusalException,error?.ToString());Assert.Equal(mode=="revoked"?1:0,reached);}
        Assert.Empty(f.Writes);Assert.Empty(f.LeaseTransactions);
    }
    [Theory]
    [InlineData("unchanged")][InlineData("permission")][InlineData("app-role")][InlineData("standard-role")]
    [InlineData("field")][InlineData("stored-account")][InlineData("order-grant")]
    [InlineData("lifecycle-binding")][InlineData("core-binding")][InlineData("physical-target")]
    public async Task ReturnEvidenceGuardRechecksBothBindingsAndCurrentAuthorityAfterNativeRead(string change)
    {
        using var f=await Ready();
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"});
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName="selected-tenant",ConnectionKey="SELECTED-TENANT",ConcurrencyStamp="lifecycle-version"});
        await f.Db.SaveChangesAsync();await f.Select("company");await f.Grant("FIELD","A","WRITE");await f.Grant("WORK_ORDER","guard-order","WRITE");
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);boundary.Field="A";
        var user=await new HandoffRoleBridge(f.Gate).TransformAsync(f.Request.User);f.Request.User=user;
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        var connection=await modules.ResolveAsync("PPDM_CORE");var core=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection));
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(f.Gate.Company.Scope,Guid.NewGuid()),"A","guard-order");
        var report=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(core,"A",taskOverride:task).Command.Prepare(DateTime.UtcNow);f.Rows.Add(report);var facts=CompanyWorkOrderSubmissionFacts.From(report);
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_SUBMISSION",EntityTypeName=typeof(WORK_ORDER_EXECUTION_SUBMISSION).FullName!,PrimaryKeyColumn="SUBMISSION_ID"});
        f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        f.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_SUBMISSION")).Returns(true);
        f.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_EXECUTION_SUBMISSION"),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_SUBMISSION));foreach(var column in shape.Fields)column.IsKey=column.FieldName=="SUBMISSION_ID";return shape;});
        var process=new Mock<IProcessService>();process.As<ICompanyWorkOrderReturnOwner>();var reached=0;
        process.As<ICompanyMaintenanceClosureOwner>().Setup(value=>value.ReadMaintenanceClosureFactsAsync(It.IsAny<FieldSelectionReview>(),facts.Query,It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview _,CompanyWorkOrderCompletionQuery _,Func<Task> authority)=>
        {
            if(reached++==0)
            {
                switch(change)
                {
                    case "permission":f.Db.RoleClaims.Remove(await f.Db.RoleClaims.SingleAsync(row=>row.ClaimValue=="Maintenance.ReturnToService"));break;
                    case "app-role":f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync(row=>row.UserId=="actor" && row.RoleId=="admin"));break;
                    case "standard-role":foreach(var identity in user.Identities)foreach(var claim in identity.FindAll(identity.RoleClaimType).ToArray())identity.RemoveClaim(claim);break;
                    case "field":boundary.Field="B";break;
                    case "stored-account":(await f.Db.Users.SingleAsync(row=>row.Id=="actor")).ConcurrencyStamp=Guid.NewGuid().ToString();break;
                    case "order-grant":(await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="WORK_ORDER" && row.AssetId=="guard-order")).IsActive=false;break;
                    case "lifecycle-binding":(await f.Db.ModuleDatabases.SingleAsync(row=>row.ModuleId=="LIFECYCLE")).ConcurrencyStamp="changed-lifecycle";break;
                    case "core-binding":(await f.Db.ModuleDatabases.SingleAsync(row=>row.ModuleId=="PPDM_CORE")).ConcurrencyStamp="changed-core";break;
                    case "physical-target":f.Editor.Object.ConfigEditor.DataConnections.Single(row=>row.ConnectionName=="selected-tenant").Database="changed-native-target";break;
                }
                await f.Db.SaveChangesAsync();
            }
            await authority();return (CompanyMaintenanceClosureFacts?)null;
        });
        var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),_=>process.Object),production,boundary.Orchestrator.Object,executionPermits:new(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object));
        var error=await Record.ExceptionAsync(()=>service.ReadMaintenanceClosureStatusAsync(user,facts.Query));
        if(change=="unchanged"){Assert.Null(error);Assert.Equal(2,reached);}
        else{Assert.Equal(1,reached);Assert.NotNull(error);Assert.True(error is UnauthorizedAccessException or Beep.OilandGas.Models.Core.Refusals.RefusalException,error.ToString());}
        Assert.Empty(f.Writes);Assert.Empty(f.LeaseTransactions);
    }
    [Fact]
    public Task PartnerQualifiedMaintenanceEvidenceHttpUsesActualNativeSource()=>QualifiedMaintenanceEvidenceHttpUsesCurrentAppGrantsNativeParentsAndOriginalReportWithoutWrites(false,true);
    [Theory][InlineData(false,false)][InlineData(true,false)]
    public async Task QualifiedMaintenanceEvidenceHttpUsesCurrentAppGrantsNativeParentsAndOriginalReportWithoutWrites(bool recordedPolicy,bool partner)
    {
        var durations=new System.Collections.Concurrent.ConcurrentDictionary<string,(int Count,double Milliseconds)>();
        using var activityListener=new System.Diagnostics.ActivityListener
        {
            ShouldListenTo=source=>source.Name=="Beep.OilandGas.WorkOrderPermitDecision",
            Sample=(ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _)=>System.Diagnostics.ActivitySamplingResult.AllData,
            ActivityStopped=activity=>durations.AddOrUpdate(activity.OperationName,(1,activity.Duration.TotalMilliseconds),(_,before)=>(before.Count+1,before.Milliseconds+activity.Duration.TotalMilliseconds))
        };
        System.Diagnostics.ActivitySource.AddActivityListener(activityListener);
        using var f=await Ready();var permission=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"};f.Db.RoleClaims.Add(permission);f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName="selected-tenant",ConnectionKey="SELECTED-TENANT",ConcurrencyStamp="lifecycle-version"});await f.Db.SaveChangesAsync();await f.Select("company");await f.Grant("FIELD","A","WRITE",inherit:partner);
        const string order="qualified-order";await f.Grant("WORK_ORDER",order,"WRITE");await f.Grant("EQUIPMENT","pump");await f.Grant("FACILITY","plant",facilityType:"gas");var utc=DateTime.UtcNow;
        f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,WORK_ORDER_TYPE="CORRECTIVE",ACTIVE_IND="Y"});f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,COMPONENT_ID="original-component",FIELD_ID="A",EQUIPMENT_ID="pump",FACILITY_ID="plant",FACILITY_TYPE="gas",ACTIVE_IND="Y"});
        f.Rows.Add(new EQUIPMENT{EQUIPMENT_ID="pump",CURRENT_OWNER_BA_ID="company",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),COMMISSION_DATE=utc.AddDays(-20)});f.Rows.Add(new FACILITY{FACILITY_ID="plant",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR=partner?"partner":"company",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});f.Rows.Add(new FACILITY{FACILITY_ID="plant",FACILITY_TYPE="oil",PRIMARY_FIELD_ID="B",CURRENT_OPERATOR="other-company",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});f.Rows.Add(new FACILITY_EQUIPMENT{FACILITY_ID="plant",FACILITY_TYPE="gas",EQUIPMENT_ID="pump",INSTALL_OBS_NO=7,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-5),INSTALL_DATE=utc.AddDays(-5),SOURCE="recorded-install"});
        f.Rows.Add(new FACILITY_STATUS{FACILITY_ID="plant",FACILITY_TYPE="gas",STATUS_ID="facility-native-status",STATUS_TYPE="NativeCondition",STATUS="INACTIVE",SOURCE="recorded-native",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),TIMEZONE="UTC"});f.Rows.Add(new EQUIPMENT_STATUS{EQUIPMENT_ID="pump",EQUIP_STATUS_ID="equipment-native-status",EQUIP_STATUS_TYPE="NativeCondition",EQUIP_STATUS="IDLE",SOURCE="recorded-equipment",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),TIMEZONE="UTC"});f.Rows.Add(new R_FAC_STATUS_TYPE{STATUS_TYPE="NativeCondition",ACTIVE_IND="Y"});f.Rows.Add(new R_EQUIP_STATUS_TYPE{STATUS_TYPE="NativeCondition",ACTIVE_IND="Y"});f.Rows.Add(new R_FACILITY_STATUS{STATUS_TYPE="NativeCondition",STATUS="INACTIVE",ACTIVE_IND="Y"});f.Rows.Add(new R_EQUIP_STATUS{STATUS_TYPE="NativeCondition",EQUIP_STATUS="IDLE",ACTIVE_IND="Y"});f.Rows.Add(new R_TIMEZONE{TIMEZONE="UTC",ACTIVE_IND="Y"});
        f.Rows.Add(new FACILITY_LICENSE{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="qualified-licence",LICENSE_NUM="LIC-HTTP-001",FACILITY_LICENSE_TYPE="RecordedType",GRANTED_BY_BA_ID="licence-issuer",GRANTED_TO_BA_ID="company",ASSIGNED_FIELD_ID="A",ACTIVE_IND="Y",GRANTED_DATE=utc.AddDays(-20),EFFECTIVE_DATE=utc.AddDays(-10),EXPIRY_DATE=utc.AddDays(100),FEES_PAID_IND="Y",VIOLATION_IND="N",SOURCE="native-licence-source",REMARK="Private licensing remark"});
        if(!f.Rows.OfType<BUSINESS_ASSOCIATE>().Any(row=>row.BUSINESS_ASSOCIATE_ID=="company"))f.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="company",ACTIVE_IND="Y"});f.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="licence-issuer",ACTIVE_IND="Y",FIRST_NAME="Private issuer contact"});f.Rows.Add(new FACILITY_LIC_TYPE{GRANTED_BY_BA_ID="licence-issuer",FACILITY_LICENSE_TYPE="RecordedType",ACTIVE_IND="Y"});
        f.Rows.Add(new FACILITY_LIC_STATUS{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="qualified-licence",STATUS_ID="qualified-licence-status",LICENSE_STATUS_TYPE="RecordedDecision",LICENSE_STATUS="RECORDED_APPROVAL",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-10),SOURCE="native-licence-decision"});f.Rows.Add(new R_LIC_STATUS_TYPE{LICENSE_STATUS_TYPE="RecordedDecision",ACTIVE_IND="Y"});f.Rows.Add(new R_LICENSE_STATUS{LICENSE_STATUS_TYPE="RecordedDecision",LICENSE_STATUS="RECORDED_APPROVAL",ACTIVE_IND="Y"});
        f.Rows.Add(new FACILITY_LIC_AREA{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="qualified-licence",AREA_ID="recorded-area",AREA_TYPE="RecordedRegion",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),SOURCE="native-area-assignment",REMARK="Private licence area remark"});f.Rows.Add(new AREA{AREA_ID="recorded-area",AREA_TYPE="RecordedRegion",PREFERRED_NAME="Recorded <region> name",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),SOURCE="native-area-catalog",SOURCE_DOCUMENT_ID="recorded-area-document",REMARK="Private referenced area remark"});f.Rows.Add(new R_AREA_TYPE{AREA_TYPE="RecordedRegion",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});f.Rows.Add(new AREA{AREA_ID="recorded-area",AREA_TYPE="Country",PREFERRED_NAME="Wrong qualified country",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
        f.Rows.Add(new FACILITY_LIC_COND{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="qualified-licence",CONDITION_ID="recorded-condition",CONDITION_TYPE="RecordedCondition",CONDITION_CODE="RecordedCode",DUE_CONDITION="RecordedDue",CONDITION_VALUE=0m,CONDITION_VALUE_UOM="recorded-unit",DUE_TERM=2.5m,DUE_TERM_UOM="recorded-unit",RESTRICTION_ID="recorded-restriction",RESTRICTION_VERSION=2m,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),SOURCE="native-condition",CONTACT_BA_ID="Private condition contact"});
        f.Rows.Add(new R_FAC_LIC_COND{CONDITION_TYPE="RecordedCondition",ACTIVE_IND="Y"});f.Rows.Add(new R_FAC_LIC_COND_CODE{CONDITION_TYPE="RecordedCondition",CONDITION_CODE="RecordedCode",ACTIVE_IND="Y"});f.Rows.Add(new R_FAC_LIC_DUE_CONDITION{DUE_CONDITION="RecordedDue",ACTIVE_IND="Y"});
        f.Rows.Add(new PPDM_UNIT_OF_MEASURE{UOM_ID="recorded-unit",UOM_NAME="Recorded unit",UOM_QUANTITY_TYPE="RecordedQuantity",UOM_SYSTEM_ID="RecordedSystem",UOM_USAGE_TYPE="RecordedUsage",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),SOURCE="native-unit",SOURCE_DOCUMENT_ID="unit-document",REMARK="Private unit remark"});f.Rows.Add(new PPDM_QUANTITY{QUANTITY_TYPE="RecordedQuantity",ACTIVE_IND="Y"});f.Rows.Add(new PPDM_MEASUREMENT_SYSTEM{UOM_SYSTEM_ID="RecordedSystem",ACTIVE_IND="Y"});f.Rows.Add(new R_PPDM_UOM_USAGE{UOM_USAGE_TYPE="RecordedUsage",ACTIVE_IND="Y"});
        f.Rows.Add(new RESTRICTION{RESTRICTION_ID="recorded-restriction",RESTRICTION_VERSION=2.00m,RESTRICTION_NAME="Recorded restriction two",DESCRIPTION="Recorded <restriction> text",REGULATORY_ACT="Recorded act",RESTRICTION_CLASS="RecordedClass",RESTRICTION_TYPE="RecordedType",REST_DURATION_TYPE="RecordedDuration",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),SOURCE="native-restriction",SOURCE_DOCUMENT_ID="restriction-document",REMARK="Private restriction remark"});f.Rows.Add(new RESTRICTION{RESTRICTION_ID="recorded-restriction",RESTRICTION_VERSION=3m,RESTRICTION_NAME="Wrong later restriction",ACTIVE_IND="Y"});f.Rows.Add(new REST_CLASS{RESTRICTION_CLASS_ID="RecordedClass",ACTIVE_IND="Y"});f.Rows.Add(new R_REST_TYPE{RESTRICTION_TYPE="RecordedType",ACTIVE_IND="Y"});f.Rows.Add(new R_REST_DURATION{REST_DURATION_TYPE="RecordedDuration",ACTIVE_IND="Y"});
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var connection=await modules.ResolveAsync("PPDM_CORE");var core=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection));var owners=MaintenanceReadOwners(f,modules,profile);
        var originalOrder=(await owners.Order.GetByIdAsync("A",order))!;var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(f.Gate.Company.Scope,Guid.NewGuid()),"A",order);var original=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(core,"A",originalOrder:originalOrder,taskOverride:task);var report=original.Command.Prepare(DateTime.UtcNow);f.Rows.Add(report);var facts=CompanyWorkOrderSubmissionFacts.From(report);
        f.Metadata.Setup(metadata=>metadata.GetTableMetadataAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_SUBMISSION",EntityTypeName=typeof(WORK_ORDER_EXECUTION_SUBMISSION).FullName!,PrimaryKeyColumn="SUBMISSION_ID"});f.Metadata.Setup(metadata=>metadata.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Editor.SetupGet(editor=>editor.classCreator).Returns(creator);f.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns(true);f.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>{var type=requested.EntityName=="WORK_ORDER_ASSET_RETURN"?typeof(WORK_ORDER_ASSET_RETURN):requested.EntityName=="WORK_ORDER_MAINTENANCE_RETURN_POLICY"?typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY):requested.EntityName=="WORK_ORDER_EXECUTION_SUBMISSION"?typeof(WORK_ORDER_EXECUTION_SUBMISSION):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;var shape=creator.ConvertToEntityStructure(type);var keys=f.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult().ToArray();foreach(var column in shape.Fields)column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal);return shape;});
        var policyMetadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase))["WORK_ORDER_MAINTENANCE_RETURN_POLICY"];f.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_MAINTENANCE_RETURN_POLICY")).ReturnsAsync(policyMetadata);f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_MAINTENANCE_RETURN_POLICY")).ReturnsAsync(["POLICY_RECORD_ID"]);

        var returnMetadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase))["WORK_ORDER_ASSET_RETURN"];f.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_ASSET_RETURN")).ReturnsAsync(returnMetadata);f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_ASSET_RETURN")).ReturnsAsync(["RETURN_ID"]);
        if(recordedPolicy)
        {
            f.Rows.Add(new R_FACILITY_STATUS{STATUS_TYPE="NativeCondition",STATUS="RECORDED_TARGET",ACTIVE_IND="Y"});f.Rows.Add(new R_EQUIP_STATUS{STATUS_TYPE="NativeCondition",EQUIP_STATUS="RECORDED_TARGET",ACTIVE_IND="Y"});f.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="fulfilment-authority",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
            var condition=f.Rows.OfType<FACILITY_LIC_COND>().Single();condition.FULFILLED_IND="Y";condition.EXEMPT_IND="N";condition.FULFILLED_DATE=utc.AddDays(-1);condition.FULFILLED_BY_BA_ID="fulfilment-authority";
            var policy=new CompanyMaintenanceReturnPolicyDraft("CORRECTIVE","gas","Explicit company jurisdiction","recorded-area","RecordedRegion",["Administrator"],[new("FACILITY","NativeCondition",["INACTIVE"],"RECORDED_TARGET","recorded-policy-source","Reviewed transition"),new("EQUIPMENT","NativeCondition",["IDLE"],"RECORDED_TARGET","recorded-policy-source","Reviewed transition")],[new("licence-issuer","RecordedType","REQUIRED","REQUIRED_PAID",[new("RecordedDecision",["RECORDED_APPROVAL"],"Reviewed licence status")],[new("RecordedCondition","RecordedCode","FULFILLED_ONLY","RecordedDue",null,null,new("EXACT",0,"recorded-unit","Reviewed numeric term"),new("EXACT",2.5m,"recorded-unit","Reviewed due term"),"EXACT_VERSION","recorded-restriction",2,"restriction-document","Reviewed restriction applicability","Reviewed condition"){AcceptedFulfilledByBaIds=["fulfilment-authority"]}],[],"Reviewed licence applicability")],utc.AddDays(-1),utc.AddDays(30),"Reviewed policy source");
            var policyContext=new CompanyMaintenanceReturnPolicyContext(f.Gate.Company.Scope,core,"policy-maker",new string('D',64));var empty=CompanyMaintenanceReturnPolicyCatalog.Create(policyContext,[],utc);f.Rows.Add(new CompanyMaintenanceReturnPolicyCommand(policyContext,empty.Revision,policy,1,Guid.NewGuid(),"Private policy intent rationale").Prepare(utc));
        }
        foreach(var key in f.Rows.Select(row=>row.GetType().GetProperty("SOURCE")?.GetValue(row) as string).Append("recorded-policy-source").Where(value=>LeaseSummaryValidator.Identifier(value)).Distinct(StringComparer.Ordinal).ToArray())f.Rows.Add(new R_SOURCE{SOURCE=key!,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),REMARK="Private native source remark"});
        // Explicit already-closed lifecycle facts boundary; this test does not certify the original closure write/graph.
        var closure=new CompanyMaintenanceClosureFacts(f.Gate.Company.Scope,core,facts.Query,"independent-closer","FacilityRole","closed-history",["closed-task"],DateTime.UtcNow,new string('A',64),new string('B',64),facts.Revision,new string('C',64));closure.Require();var process=new Mock<IProcessService>();process.As<ICompanyWorkOrderReturnOwner>();process.As<ICompanyMaintenanceClosureOwner>().Setup(owner=>owner.ReadMaintenanceClosureFactsAsync(It.IsAny<FieldSelectionReview>(),facts.Query,It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview _,CompanyWorkOrderCompletionQuery _,Func<Task> authority)=>{await authority();return closure;});
        var qualified=new CompanyMaintenanceQualifiedAssetEvidenceReader(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,f.Policy,Options.Create(f.EligibilityConfiguration),Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),new CompanyMaintenanceReturnPolicyService(f.Gate,modules,f.Db,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));var reports=new WorkOrderExecutionPermitService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),_=>process.Object),production,boundary.Orchestrator.Object,owners.Order,executionPermits:reports,maintenanceWells:new CompanyMaintenanceWellEvidenceReader(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,f.Policy,Options.Create(f.EligibilityConfiguration),qualified),qualifiedAssets:qualified);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceHandoffController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();},additionalControllers:[typeof(CompanyMaintenanceAssetReturnStatusController)]);var url=host.Url+"/api/field/current/maintenance-handoffs/execution-permit/asset-return/evidence";var statusUrl=url.Replace("/evidence","/status",StringComparison.Ordinal);
        try
        {
        using(var response=await host.Http.PostAsJsonAsync(url,facts.Query)){Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());var result=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceAssetEvidence>())!;result.Require();Assert.Equal(facts.Query,result.Report.Query);Assert.Null(result.Report.QualifiedWorkAssets);Assert.Contains(CompanyMaintenanceQualifiedWorkScope.MissingOriginal,result.Blockers);Assert.NotNull(result.QualifiedAssets);Assert.Equal("gas",Assert.Single(result.QualifiedAssets.Facilities).FacilityType);Assert.Equal(partner?"partner":"company",Assert.Single(result.QualifiedAssets.Facilities).OperatorBaId);Assert.Equal(7,Assert.Single(result.QualifiedAssets.Equipment).Installation.InstallationObservation);Assert.NotNull(result.QualifiedAssets.Operating);Assert.Contains(result.QualifiedAssets.Operating.Statuses,row=>row.AssetType=="FACILITY" && row.Value=="INACTIVE");Assert.Contains(result.QualifiedAssets.Operating.Statuses,row=>row.AssetType=="EQUIPMENT" && row.Value=="IDLE");Assert.Contains(result.QualifiedAssets.Operating.Blockers,value=>value.Contains("no assumed operational meaning",StringComparison.Ordinal));Assert.NotNull(result.QualifiedAssets.Licensing);Assert.Equal("LIC-HTTP-001",Assert.Single(result.QualifiedAssets.Licensing.Licenses).LicenseNumber);var area=Assert.Single(result.QualifiedAssets.Licensing.Areas!);Assert.Equal("RecordedRegion",area.AreaType);Assert.Equal("Recorded <region> name",area.Area!.PreferredName);Assert.Equal("recorded-area-document",area.Area.SourceDocumentId);var terms=Assert.Single(result.QualifiedAssets.Licensing.Conditions).Terms!;Assert.Equal(0m,terms.Value);Assert.Equal(2.5m,terms.DueTerm);Assert.Equal("RecordedQuantity",terms.ValueUnit!.QuantityType);Assert.Equal(2m,terms.Restriction!.Version);Assert.Equal("restriction-document",terms.Restriction.SourceDocumentId);Assert.Contains(result.QualifiedAssets.Licensing.Blockers,value=>value.Contains("do not establish legal or operating approval",StringComparison.Ordinal));Assert.NotNull(result.QualifiedAssets.ReturnPolicy);if(recordedPolicy){Assert.Empty(result.QualifiedAssets.ReturnPolicy.Blockers);Assert.Single(result.QualifiedAssets.ReturnPolicy.Applications[0].PolicyRecordIds);Assert.Contains("Administrator",result.QualifiedAssets.ReturnPolicy.ActorReturnRoles);Assert.Contains(result.Blockers,value=>value.Contains("complete controlled mixed-asset return",StringComparison.Ordinal));}else{Assert.NotEmpty(result.QualifiedAssets.ReturnPolicy.Blockers);Assert.Contains(result.Blockers,value=>value.Contains("explicit return-policy",StringComparison.Ordinal));}Assert.DoesNotContain(f.Db.RoleClaims,row=>row.ClaimValue=="Maintenance.ConfigureReturnPolicy");var body=await response.Content.ReadAsStringAsync();Assert.DoesNotContain("COMMAND_JSON",body);Assert.DoesNotContain("Private licensing remark",body);Assert.DoesNotContain("Private issuer contact",body);Assert.DoesNotContain("Private licence area remark",body);Assert.DoesNotContain("Private referenced area remark",body);Assert.DoesNotContain("Wrong qualified country",body);Assert.DoesNotContain("Private condition contact",body);Assert.DoesNotContain("Private unit remark",body);Assert.DoesNotContain("Private restriction remark",body);Assert.DoesNotContain("Wrong later restriction",body);Assert.DoesNotContain("Private policy intent rationale",body);Assert.DoesNotContain("Private native source remark",body);Assert.DoesNotContain(original.Command.IntentId.ToString("D"),body);Assert.Empty(f.Writes);}
        var grants=new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>());
        foreach(var resource in new[]{("FIELD","A"),("WORK_ORDER",order)})Assert.True(await grants.GrantAsync(f.Gate.Company.UserId,core.Fingerprint,resource.Item1,resource.Item2,"READ",resource.Item1=="FIELD" && partner));
        Assert.DoesNotContain("Workflow.Execute",f.Gate.Company.Permissions);Assert.DoesNotContain("Facilities.Edit",f.Gate.Company.Permissions);Assert.DoesNotContain("Facilities.ManageEquipment",f.Gate.Company.Permissions);
        int PolicyAndLicenceReads()=>f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] is string table && (table=="WORK_ORDER_MAINTENANCE_RETURN_POLICY" || table.StartsWith("FACILITY_LIC",StringComparison.Ordinal)));
        var unrelatedReads=PolicyAndLicenceReads();
        using(var publicStatus=await host.Http.PostAsJsonAsync(statusUrl,facts.Query))
        {
            Assert.True(publicStatus.StatusCode==HttpStatusCode.OK,await publicStatus.Content.ReadAsStringAsync());Assert.Equal("no-store",publicStatus.Headers.CacheControl?.ToString());
            var result=(await publicStatus.Content.ReadFromJsonAsync<CompanyMaintenanceAssetReturnStatus>())!;result.Require();Assert.Null(result.Recorded);Assert.Empty(result.CurrentWells);Assert.NotNull(result.OriginalComponents);
            var current=result.CurrentQualifiedAssets!;Assert.Equal("READ",Assert.Single(current.Equipment).AccessLevel);Assert.Equal("READ",Assert.Single(current.Facilities).AccessLevel);Assert.Null(current.Licensing);Assert.Null(current.ReturnPolicy);
            Assert.Contains(current.Operating!.Statuses,row=>row.AssetType=="FACILITY" && row.Value=="INACTIVE");Assert.Contains(current.Operating.Statuses,row=>row.AssetType=="EQUIPMENT" && row.Value=="IDLE");
            var json=await publicStatus.Content.ReadAsStringAsync();Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain("Private policy intent rationale",json);Assert.DoesNotContain("Private native source remark",json);Assert.DoesNotContain("\"budgetAmt\"",json);Assert.DoesNotContain("\"contractors\"",json);Assert.DoesNotContain("\"costLines\"",json);Assert.Empty(f.Writes);
        }
        Assert.Equal(unrelatedReads,PolicyAndLicenceReads());
        f.Source.Setup(source=>source.CheckEntityExist("WORK_ORDER_ASSET_RETURN")).Returns(false);
        using(var missingStorage=await host.Http.PostAsJsonAsync(statusUrl,facts.Query)){Assert.Equal(HttpStatusCode.Conflict,missingStorage.StatusCode);Assert.DoesNotContain("\"recorded\":null",await missingStorage.Content.ReadAsStringAsync());}
        f.Source.Setup(source=>source.CheckEntityExist("WORK_ORDER_ASSET_RETURN")).Returns(true);
        using(var foreignStatus=await host.Http.PostAsJsonAsync(statusUrl,facts.Query with{Task=facts.Query.Task with{FieldId="B"}}))Assert.Equal(HttpStatusCode.Forbidden,foreignStatus.StatusCode);
        foreach(var resource in new[]{("FIELD","A"),("WORK_ORDER",order)})Assert.True(await grants.GrantAsync(f.Gate.Company.UserId,core.Fingerprint,resource.Item1,resource.Item2,"WRITE",resource.Item1=="FIELD" && partner));
        }
        finally
        {
            var reads=f.Source.Invocations.Where(call=>call.Method.Name=="GetEntityAsync").GroupBy(call=>call.Arguments[0]?.ToString()??"unknown").ToDictionary(group=>group.Key,group=>group.Count());
            var diagnostic=new{RecordedPolicy=recordedPolicy,Stages=durations.OrderBy(pair=>pair.Key).Select(pair=>new{Stage=pair.Key,pair.Value.Count,pair.Value.Milliseconds}),Reads=reads};
            var path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"oilgas-335-http-profile-"+recordedPolicy.ToString().ToLowerInvariant()+".json");
            System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(diagnostic,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
        }
        int InstallationReads()=>f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="FACILITY_EQUIPMENT");
        using(var foreignField=await host.Http.PostAsJsonAsync(url,facts.Query with{Task=facts.Query.Task with{FieldId="B"}}))Assert.Equal(HttpStatusCode.Forbidden,foreignField.StatusCode);
        var equipmentGrant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="EQUIPMENT");equipmentGrant.IsActive=false;await f.Db.SaveChangesAsync();var before=InstallationReads();using(var denied=await host.Http.PostAsJsonAsync(url,facts.Query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(before,InstallationReads());using(var deniedStatus=await host.Http.PostAsJsonAsync(statusUrl,facts.Query))Assert.Equal(HttpStatusCode.Forbidden,deniedStatus.StatusCode);Assert.Equal(before,InstallationReads());Assert.Empty(f.Writes);
    }
}
