using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.ApiService.Controllers.Facility;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public async Task ProtectedFacilityEquipmentReadUsesActualNativeOwnerAndCurrentAuthority()
    {
        using var fixture=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();var f=fixture.Native;
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.View"});
        var core=await f.Gate.ResolveAsync();f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName=core.ConnectionName,ConnectionKey=core.ConnectionName.ToUpperInvariant(),ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();await f.Select("company");
        f.Rows.Add(new FIELD{FIELD_ID="A",ACTIVE_IND="Y"});
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);
        boundary.Access.Setup(value=>value.CheckAssetAccessAsync("actor","plant","FACILITY",It.IsAny<string?>(),"gas")).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        var failureMode="";var reader=new Mock<ICompanyFacilityEquipmentReader>();reader.Setup(value=>value.ReadDevelopmentFacilityEquipmentAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<Func<Task>>())).Returns((FieldSelectionReview context,string id,string type,Func<Task> guard)=>failureMode=="driver"?Task.FromException<IReadOnlyList<FACILITY_EQUIPMENT>>(new Beep.OilandGas.PPDM39.Core.DataSourceTransactionCapabilityException("Unsupported driver")):failureMode=="schema"?Task.FromException<IReadOnlyList<FACILITY_EQUIPMENT>>(new Beep.OilandGas.PPDM39.Core.NativeSchemaCompatibilityException("Physical schema mismatch")):fixture.Owner.ReadDevelopmentFacilityEquipmentAsync(context,id,type,guard));
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(DevelopmentFacilityEvidenceController),services=>{
            services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddSingleton<ICompanyFieldSelectionAuthority>(profile);services.AddSingleton<IUserAssetAccessStore>(new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()));services.AddSingleton(Mock.Of<IFieldDevelopmentService>());services.AddSingleton(modules);services.AddSingleton<DevelopmentFacilityReadAuthority>();services.AddSingleton<Func<FieldSelectionReview,ICompanyFacilityEquipmentReader>>(_=>_=>reader.Object);services.AddSingleton<CompanyDevelopmentFacilityEquipmentService>();
        });
        var url=host.Url+"/api/field/current/development/facilities/equipment?fieldId=A&facilityId=plant&facilityType=gas";
        using(var response=await host.Http.GetAsync(url)){Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.True(response.Headers.CacheControl?.NoStore);var item=Assert.Single((await response.Content.ReadFromJsonAsync<List<DevelopmentFacilityEquipmentItem>>())!);Assert.Equal("pump",item.EquipmentId);Assert.Equal(7,item.InstallationObservation);Assert.DoesNotContain("ROW_CREATED_BY",await response.Content.ReadAsStringAsync());}
        var grant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="EQUIPMENT");grant.IsActive=false;await f.Db.SaveChangesAsync();
        using(var denied=await host.Http.GetAsync(url))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Empty(f.Writes);
        failureMode="driver";
        using(var unavailable=await host.Http.GetAsync(url)){Assert.Equal(HttpStatusCode.Conflict,unavailable.StatusCode);Assert.Contains("update its Beep driver",await unavailable.Content.ReadAsStringAsync());}Assert.Empty(f.Writes);
        failureMode="schema";using(var unavailable=await host.Http.GetAsync(url)){Assert.Equal(HttpStatusCode.Conflict,unavailable.StatusCode);Assert.Contains("review the Beep migration and driver",await unavailable.Content.ReadAsStringAsync());}Assert.Empty(f.Writes);
        failureMode="";fixture.SchemaMissing=true;using(var missing=await host.Http.GetAsync(url)){Assert.Equal(HttpStatusCode.Conflict,missing.StatusCode);Assert.Contains("review the Beep migration and driver",await missing.Content.ReadAsStringAsync());}Assert.Empty(f.Writes);

    }

    [Fact]
    public async Task ProtectedInstallationSaveAndReadOnlyRecoveryUseActualNativeOwner()
    {
        using var fixture=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();var f=fixture.Native;
        foreach(var existingGrant in await f.Db.Set<AppUserAssetAccess>().ToListAsync())existingGrant.AccessLevel="WRITE";
        f.Db.RoleClaims.AddRange(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.View"},new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.Edit"});
        var core=await f.Gate.ResolveAsync();f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName=core.ConnectionName,ConnectionKey=core.ConnectionName.ToUpperInvariant(),ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();await f.Select("company");f.Rows.Add(new FIELD{FIELD_ID="A",ACTIVE_IND="Y"});
        var prior=f.Rows.OfType<FACILITY_EQUIPMENT>().Single();prior.ACTIVE_IND="N";prior.REMOVE_DATE=DateTime.UtcNow.AddDays(-1);
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("FACILITY_EQUIPMENT_INSTALL_INTENT")).ReturnsAsync(new PPDMTableMetadata{TableName="FACILITY_EQUIPMENT_INSTALL_INTENT",EntityTypeName=nameof(FACILITY_EQUIPMENT_INSTALL_INTENT),PrimaryKeyColumn="FACILITY_EQUIPMENT_INSTALL_INTENT_ID"});f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("FACILITY_EQUIPMENT_INSTALL_INTENT")).ReturnsAsync(["FACILITY_EQUIPMENT_INSTALL_INTENT_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>{var type=requested.EntityName=="FACILITY_EQUIPMENT_INSTALL_INTENT"?typeof(FACILITY_EQUIPMENT_INSTALL_INTENT):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;var shape=creator.ConvertToEntityStructure(type);var keys=f.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult();foreach(var column in shape.Fields){column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal);if(requested.EntityName=="FACILITY_EQUIPMENT" && column.Fieldtype==typeof(string).FullName){column.Size=4096;column.Size1=4096;}}return shape;});
        var properties=f.Source.Object.Dataconnection.ConnectionProp;var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(properties);connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.LeasePhysicalConnection.Object);f.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);f.EnableCorrectiveActionTransactions();
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);boundary.Access.Setup(value=>value.CheckAssetAccessAsync("actor","plant","FACILITY",It.IsAny<string?>(),"gas")).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        var selectedCompany=f.CompanySelection;var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var native=new FacilityManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),core.ConnectionName,resolveModuleConnection:module=>modules.ResolveAsync(module),maintenanceAssetAccess:f.Policy);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(DevelopmentFacilityEvidenceController),services=>{services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddSingleton<ICompanyFieldSelectionAuthority>(profile);services.AddSingleton<IUserAssetAccessStore>(new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()));services.AddSingleton(Mock.Of<IFieldDevelopmentService>());services.AddSingleton(modules);services.AddSingleton<DevelopmentFacilityReadAuthority>();services.AddSingleton<Func<FieldSelectionReview,ICompanyEquipmentInstallationReader>>(_=>_=>native);services.AddSingleton<Func<FieldSelectionReview,ICompanyEquipmentInstallationOwner>>(_=>_=>native);services.AddSingleton<CompanyEquipmentInstallationReviewService>();services.AddSingleton<CompanyEquipmentInstallationCommandService>();});
        var context=(await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;var proposal=new DevelopmentEquipmentInstallationProposal(context,"plant","gas","pump","Administrator","Reviewed installation");var root=host.Url+"/api/field/current/development/facilities/equipment/installation";
        using var response=await host.Http.PostAsJsonAsync(root+"-review?fieldId=A",proposal);Assert.True(response.IsSuccessStatusCode,await response.Content.ReadAsStringAsync());var review=(await response.Content.ReadFromJsonAsync<DevelopmentEquipmentInstallationReview>())!;
        var command=new DevelopmentEquipmentInstallation(review,Guid.NewGuid(),Guid.NewGuid(),DateTime.UtcNow.AddMinutes(-1),true);using(var saved=await host.Http.PostAsJsonAsync(root+"?fieldId=A",command)){Assert.True(saved.IsSuccessStatusCode,await saved.Content.ReadAsStringAsync());var receipt=(await saved.Content.ReadFromJsonAsync<DevelopmentEquipmentInstallationReceipt>())!;receipt.Require(command);Assert.Equal(8,receipt.Installation.INSTALL_OBS_NO);}Assert.Equal(2,f.Writes.Count);
        foreach(var grant in await f.Db.Set<AppUserAssetAccess>().ToListAsync())grant.AccessLevel="READ";var edit=await f.Db.RoleClaims.SingleAsync(value=>value.RoleId=="admin" && value.ClaimValue=="DevelopmentPlanning.Edit");f.Db.RoleClaims.Remove(edit);await f.Db.SaveChangesAsync();await f.Select("company");selectedCompany.SelectCompany(f.Gate.Company);tenant.SelectCompany(f.Gate.Company);
        using(var denied=await host.Http.PostAsJsonAsync(root+"?fieldId=A",command))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);
        using(var recovered=await host.Http.PostAsJsonAsync(root+"/recover?fieldId=A",command)){Assert.True(recovered.IsSuccessStatusCode,await recovered.Content.ReadAsStringAsync());(await recovered.Content.ReadFromJsonAsync<DevelopmentEquipmentInstallationReceipt>())!.Require(command);}Assert.Equal(2,f.Writes.Count);
        var equipment=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="EQUIPMENT");equipment.IsActive=false;await f.Db.SaveChangesAsync();using(var denied=await host.Http.PostAsJsonAsync(root+"/recover?fieldId=A",command))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(2,f.Writes.Count);
    }

    [Fact]
    public async Task ProtectedInstallationReviewUsesActualNativeAuthorityWithoutWrites()
    {
        using var fixture=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();var f=fixture.Native;
        foreach(var existingGrant in await f.Db.Set<AppUserAssetAccess>().ToListAsync())existingGrant.AccessLevel="WRITE";
        f.Db.RoleClaims.AddRange(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.View"},new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.Edit"});
        var core=await f.Gate.ResolveAsync();f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName=core.ConnectionName,ConnectionKey=core.ConnectionName.ToUpperInvariant(),ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();await f.Select("company");f.Rows.Add(new FIELD{FIELD_ID="A",ACTIVE_IND="Y"});
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);boundary.Access.Setup(value=>value.CheckAssetAccessAsync("actor","plant","FACILITY",It.IsAny<string?>(),"gas")).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(DevelopmentFacilityEvidenceController),services=>{
            services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddSingleton<ICompanyFieldSelectionAuthority>(profile);services.AddSingleton<IUserAssetAccessStore>(new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()));services.AddSingleton(Mock.Of<IFieldDevelopmentService>());services.AddSingleton(modules);services.AddSingleton<DevelopmentFacilityReadAuthority>();services.AddSingleton<Func<FieldSelectionReview,ICompanyEquipmentInstallationReader>>(_=>_=>fixture.Owner);services.AddSingleton<CompanyEquipmentInstallationReviewService>();
        });
        var context=(await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;var proposal=new DevelopmentEquipmentInstallationProposal(context,"plant","gas","pump","Administrator","Install for reviewed maintenance");var url=host.Url+"/api/field/current/development/facilities/equipment/installation-review?fieldId=A";
        using(var response=await host.Http.PostAsJsonAsync(url,proposal)){Assert.True(response.IsSuccessStatusCode,await response.Content.ReadAsStringAsync());var review=(await response.Content.ReadFromJsonAsync<DevelopmentEquipmentInstallationReview>())!;review.Require();Assert.Equal(proposal,review.Proposal);Assert.Equal(1,review.Source.RecordedRows);}
        var grant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="EQUIPMENT");grant.AccessLevel="READ";await f.Db.SaveChangesAsync();
        using(var denied=await host.Http.PostAsJsonAsync(url,proposal))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task ProtectedFacilityCreationUsesInheritedFieldAndActualNativeOwner()
    {
        using var fixture=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();var f=fixture.Native;await f.Grant("FIELD","A","WRITE",inherit:true);
        f.Db.RoleClaims.AddRange(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.View"},new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.Create"});
        var core=await f.Gate.ResolveAsync();f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName=core.ConnectionName,ConnectionKey=core.ConnectionName.ToUpperInvariant(),ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();await f.Select("company");
        f.Rows.AddRange([new FIELD{FIELD_ID="A",ACTIVE_IND="Y"},new R_FACILITY_TYPE{FACILITY_TYPE="PROCESSING",ACTIVE_IND="Y",LONG_NAME="Recorded type"},new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="partner",ACTIVE_IND="Y"}]);
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);var selectedCompany=f.CompanySelection;var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("FACILITY_CREATE_INTENT")).ReturnsAsync(new PPDMTableMetadata{TableName="FACILITY_CREATE_INTENT",EntityTypeName=nameof(FACILITY_CREATE_INTENT),PrimaryKeyColumn="FACILITY_CREATE_INTENT_ID"});f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("FACILITY_CREATE_INTENT")).ReturnsAsync(["FACILITY_CREATE_INTENT_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(value=>value.CheckEntityExist(It.IsAny<string>())).Returns(true);
        f.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>
        {
            var type=requested.EntityName=="FACILITY_CREATE_INTENT"?typeof(FACILITY_CREATE_INTENT):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;
            var shape=creator.ConvertToEntityStructure(type);var keys=f.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult();
            foreach(var column in shape.Fields){column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal);if(requested.EntityName is "FACILITY" or "FACILITY_FIELD" && column.Fieldtype==typeof(string).FullName){column.Size=4096;column.Size1=4096;if(requested.EntityName=="FACILITY" && column.FieldName=="DESCRIPTION")column.AllowDBNull=true;}}return shape;
        });
        var properties=f.Source.Object.Dataconnection.ConnectionProp;var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(properties);connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.LeasePhysicalConnection.Object);f.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);f.EnableCorrectiveActionTransactions();
        var native=new FacilityManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),core.ConnectionName,resolveModuleConnection:module=>modules.ResolveAsync(module),maintenanceAssetAccess:f.Policy);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(DevelopmentFacilityEvidenceController),services=>
        {
            services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddSingleton<ICompanyFieldSelectionAuthority>(profile);services.AddSingleton<IUserAssetAccessStore>(new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()));services.AddSingleton(Mock.Of<IFieldDevelopmentService>());services.AddSingleton(modules);
            services.AddSingleton<DevelopmentFacilityReadAuthority>();services.AddSingleton<Func<FieldSelectionReview,ICompanyFacilityCreationReader>>(_=>_=>native);services.AddSingleton<Func<FieldSelectionReview,ICompanyFacilityCreationOwner>>(_=>_=>native);services.AddSingleton<CompanyDevelopmentFacilityCreationService>();
        });
        var selected=(await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;var draft=new DevelopmentFacilityCreationDraft(selected,"Administrator","new-plant","PROCESSING","partner","New plant",null,"Expansion");var url=host.Url+"/api/field/current/development/facilities";
        using var response=await host.Http.PostAsJsonAsync(url+"/creation-review?fieldId=A",draft);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());var review=(await response.Content.ReadFromJsonAsync<DevelopmentFacilityCreationReview>())!;
        var command=new DevelopmentFacilityCreation(review,Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),true);using var saved=await host.Http.PostAsJsonAsync(url+"/creation?fieldId=A",command);Assert.True(saved.StatusCode==HttpStatusCode.OK,await saved.Content.ReadAsStringAsync());(await saved.Content.ReadFromJsonAsync<DevelopmentFacilityCreationReceipt>())!.Require(command);Assert.Equal(3,f.Writes.Count);
        var created=f.Rows.OfType<FACILITY>().Single(value=>value.FACILITY_ID=="new-plant");Assert.Equal("partner",created.CURRENT_OPERATOR);Assert.NotNull(await f.Policy.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",created));
        var createClaim=await f.Db.RoleClaims.SingleAsync(value=>value.RoleId=="admin" && value.ClaimValue=="DevelopmentPlanning.Create");f.Db.RoleClaims.Remove(createClaim);var fieldGrant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(value=>value.AssetType=="FIELD" && value.AssetId=="A");fieldGrant.AccessLevel="READ";await f.Db.SaveChangesAsync();await f.Select("company");selectedCompany.SelectCompany(f.Gate.Company);tenant.SelectCompany(f.Gate.Company);
        using var denied=await host.Http.PostAsJsonAsync(url+"/creation?fieldId=A",command);Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(3,f.Writes.Count);
        using var recovered=await host.Http.PostAsJsonAsync(url+"/creation/recover?fieldId=A",command);Assert.True(recovered.StatusCode==HttpStatusCode.OK,await recovered.Content.ReadAsStringAsync());(await recovered.Content.ReadFromJsonAsync<DevelopmentFacilityCreationReceipt>())!.Require(command);Assert.Equal(3,f.Writes.Count);
    }
    [Fact]
    public async Task ObsoleteRawFacilityMasterWritesAreNotExposed()
    {
        using var f=await Ready();var profile=Profile(f);using var boundary=new Boundary(f,profile);
        var facility=new Mock<IFacilityManagementService>();var production=new Mock<IProductionManagementService>();
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(FacilityController),services=>{services.AddSingleton(facility.Object);services.AddSingleton(production.Object);});
        var url=host.Url+"/api/facility";var row=new FACILITY{FACILITY_ID="plant",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A"};
        using var create=await host.Http.PostAsJsonAsync(url,row);using var update=await host.Http.PutAsJsonAsync(url+"/plant",row);
        Assert.Equal(HttpStatusCode.MethodNotAllowed,create.StatusCode);Assert.Equal(HttpStatusCode.MethodNotAllowed,update.StatusCode);
        Assert.Empty(facility.Invocations);Assert.Empty(f.Writes);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ProtectedFacilityEditAndRetryUseTheActualNativeOwner(bool partner)
    {
        using var fixture=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();var f=fixture.Native;
        var fieldGrant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FIELD" && row.AssetId=="A");fieldGrant.AccessLevel="WRITE";fieldGrant.Inherit=partner;
        var facilityGrant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");facilityGrant.AccessLevel="WRITE";
        f.Db.RoleClaims.AddRange(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.View"},new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="DevelopmentPlanning.Edit"});
        var core=await f.Gate.ResolveAsync();f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName=core.ConnectionName,ConnectionKey=core.ConnectionName.ToUpperInvariant(),ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();await f.Select("company");
        f.Rows.Add(new FIELD{FIELD_ID="A",ACTIVE_IND="Y"});var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);
        boundary.Access.Setup(value=>value.CheckAssetAccessAsync("actor","plant","FACILITY",It.IsAny<string?>(),"gas")).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        var row=f.Rows.OfType<FACILITY>().Single(value=>value.FACILITY_TYPE=="gas");
        if(partner)row.CURRENT_OPERATOR="partner";
        var reads=new Mock<IFieldDevelopmentService>();reads.Setup(value=>value.GetFacilitiesForFieldAsync("A",It.IsAny<List<AppFilter>?>())).ReturnsAsync(()=>[CompanyInventorySourceReview.Copy(row)]);
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("FACILITY_EDIT_INTENT")).ReturnsAsync(new PPDMTableMetadata{TableName="FACILITY_EDIT_INTENT",EntityTypeName=nameof(FACILITY_EDIT_INTENT),PrimaryKeyColumn="FACILITY_EDIT_INTENT_ID"});
        f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("FACILITY_EDIT_INTENT")).ReturnsAsync(["FACILITY_EDIT_INTENT_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(value=>value.CheckEntityExist(It.IsAny<string>())).Returns(true);
        f.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>
        {
            var type=requested.EntityName=="FACILITY_EDIT_INTENT"?typeof(FACILITY_EDIT_INTENT):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;
            var shape=creator.ConvertToEntityStructure(type);var keys=requested.EntityName=="FACILITY_EDIT_INTENT"?new[]{"FACILITY_EDIT_INTENT_ID"}:f.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult().ToArray();
            foreach(var column in shape.Fields)column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal);return shape;
        });
        var properties=f.Source.Object.Dataconnection.ConnectionProp;var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(properties);
        connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.LeasePhysicalConnection.Object);f.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);f.EnableCorrectiveActionTransactions();
        var selectedCompany=f.CompanySelection;var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        var owner=new FacilityManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),core.ConnectionName,resolveModuleConnection:module=>modules.ResolveAsync(module),maintenanceAssetAccess:f.Policy,maintenanceEvidenceZone:"Asia/Kuwait");
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(DevelopmentFacilityEvidenceController),services=>
        {
            services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddSingleton<ICompanyFieldSelectionAuthority>(profile);services.AddSingleton<IUserAssetAccessStore>(new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()));
            services.AddSingleton(reads.Object);services.AddSingleton(modules);services.AddSingleton<DevelopmentFacilityReadAuthority>();services.AddSingleton<Func<FieldSelectionReview,ICompanyFacilityEditOwner>>(_=>_=>owner);services.AddSingleton<CompanyDevelopmentFacilityEditService>();
        });
        var url=host.Url+"/api/field/current/development/facilities";
        using var reviewResponse=await host.Http.GetAsync(url+"/edit-review?facilityId=plant&facilityType=gas&editorRole=Administrator");Assert.True(reviewResponse.StatusCode==HttpStatusCode.OK,await reviewResponse.Content.ReadAsStringAsync());
        var review=(await reviewResponse.Content.ReadFromJsonAsync<DevelopmentFacilityEditReview>())!;var command=new DevelopmentFacilityEdit(review,Guid.NewGuid(),"Updated facility",null,"Correct the name",true);
        using var save=await host.Http.PostAsJsonAsync(url+"/edit",command);Assert.True(save.StatusCode==HttpStatusCode.OK,await save.Content.ReadAsStringAsync());(await save.Content.ReadFromJsonAsync<DevelopmentFacilityEditReceipt>())!.Require(command);Assert.Equal(2,f.Writes.Count);
        using var retry=await host.Http.PostAsJsonAsync(url+"/edit",command);Assert.True(retry.StatusCode==HttpStatusCode.OK,await retry.Content.ReadAsStringAsync());Assert.Equal(2,f.Writes.Count);
        var editClaim=await f.Db.RoleClaims.SingleAsync(value=>value.RoleId=="admin" && value.ClaimValue=="DevelopmentPlanning.Edit");f.Db.RoleClaims.Remove(editClaim);fieldGrant.AccessLevel="READ";facilityGrant.AccessLevel="READ";await f.Db.SaveChangesAsync();await f.Select("company");selectedCompany.SelectCompany(f.Gate.Company);tenant.SelectCompany(f.Gate.Company);
        using var denied=await host.Http.PostAsJsonAsync(url+"/edit",command);Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(2,f.Writes.Count);
        using var recover=await host.Http.PostAsJsonAsync(url+"/edit/recover",command);Assert.True(recover.StatusCode==HttpStatusCode.OK,await recover.Content.ReadAsStringAsync());(await recover.Content.ReadFromJsonAsync<DevelopmentFacilityEditReceipt>())!.Require(command);Assert.Equal(2,f.Writes.Count);
    }
}
