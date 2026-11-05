using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture=Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
namespace Beep.OilandGas.Repository.Tests;

public sealed class EquipmentAssetAccessTests
{
    private static RepositoryAssetAccessStore Store(Fixture f)=>new(f.Db,f.Gate,new PPDMAssetGrantTargetVerifier(f.Gate,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,f.Eligibility));
    private static async Task<Fixture> Ready()
    {
        var f=await Fixture.Create();_=CompanyAssetFacadeTests.Service(f);
        f.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="company",ACTIVE_IND="Y"});
        f.Rows.Add(new EQUIPMENT{EQUIPMENT_ID="pump-الكويت",CURRENT_OWNER_BA_ID="company",ACTIVE_IND="Y"});return f;
    }
    [Theory][InlineData("READ")][InlineData("WRITE")][InlineData("DELETE")]
    public async Task EquipmentGrantChecksItsNativeOwnerAndUsesTheExistingAppStoreAndFacade(string level)
    {
        using var f=await Ready();var scope=await f.Gate.ResolveAsync();
        Assert.True(await Store(f).GrantAsync("actor",scope.Fingerprint,"EQUIPMENT","pump-الكويت",level,false));
        var saved=await f.Db.Set<AppUserAssetAccess>().SingleAsync();Assert.Equal("company",saved.OrganizationId);Assert.Equal("EQUIPMENT",saved.AssetType);Assert.Null(saved.FacilityType);Assert.False(saved.Inherit);
        f.Rows.Add(new EQUIPMENT{EQUIPMENT_ID=saved.AssetId,CURRENT_OWNER_BA_ID="other-company",ACTIVE_IND="Y"});
        Assert.Contains(f.Queries,query=>query.Any(filter=>filter.FieldName=="EQUIPMENT_ID" && filter.FilterValue==saved.AssetId) && query.Any(filter=>filter.FieldName=="CURRENT_OWNER_BA_ID" && filter.FilterValue=="company") && query.Any(filter=>filter.FieldName=="ACTIVE_IND" && filter.FilterValue=="Y"));
        var facade=CompanyAssetFacadeTests.Service(f);var result=Assert.Single(await facade.GetUserAccessibleAssetsAsync("actor","EQUIPMENT"));Assert.Equal(level,result.AccessLevel);Assert.Equal(saved.AssetId,result.AssetId);Assert.Null(result.FacilityType);Assert.False(result.Inherit);
        Assert.True((await facade.CheckAssetAccessAsync("actor",saved.AssetId,"EQUIPMENT",level)).HasAccess);Assert.Empty(f.Writes);
        var owned=Assert.Single((await f.Service.GetEntitiesAsync("EQUIPMENT",[])).Entities);Assert.Equal(saved.AssetId,owned["EQUIPMENT_ID"]);Assert.Equal("company",owned["CURRENT_OWNER_BA_ID"]);
        Assert.True(await Store(f).RevokeAsync("actor",scope.Fingerprint,"EQUIPMENT",saved.AssetId));Assert.False((await facade.CheckAssetAccessAsync("actor",saved.AssetId,"EQUIPMENT")).HasAccess);
    }
    [Theory][InlineData("foreign")][InlineData("inactive")][InlineData("missing")][InlineData("duplicate")][InlineData("wrong-id")][InlineData("relationship")][InlineData("key")]
    public async Task AdmissionCannotSaveUnownedAmbiguousOrUnprovenEquipment(string fault)
    {
        using var f=await Ready();var row=f.Rows.OfType<EQUIPMENT>().Single();
        if(fault=="foreign")row.CURRENT_OWNER_BA_ID="other-company";if(fault=="inactive")row.ACTIVE_IND="N";if(fault=="missing")f.Rows.Remove(row);
        if(fault=="duplicate")f.Rows.Add(CompanyInventorySourceReview.Copy(row));if(fault=="wrong-id"){row.EQUIPMENT_ID="other";f.IgnoreFilters=true;}
        var metadata=await f.Metadata.Object.GetTableMetadataAsync("EQUIPMENT");if(fault=="relationship")metadata.ForeignKeys.RemoveAll(key=>key.ForeignKeyColumn=="CURRENT_OWNER_BA_ID");if(fault=="key")metadata.PrimaryKeyColumn="EQUIPMENT_ID,CURRENT_OWNER_BA_ID";
        var scope=await f.Gate.ResolveAsync();await Assert.ThrowsAsync<RefusalException>(()=>Store(f).GrantAsync("actor",scope.Fingerprint,"EQUIPMENT","pump-الكويت","READ",false));Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());Assert.Empty(f.Writes);
        if(fault is "relationship" or "key")Assert.Empty(f.Queries);
    }
    [Theory][InlineData("inherit")][InlineData("qualifier")]
    public async Task EquipmentGrantNeverSuppliesAFieldOrFacilityRelationship(string fault)
    {
        using var f=await Ready();var scope=await f.Gate.ResolveAsync();
        var error=await Assert.ThrowsAsync<RefusalException>(()=>Store(f).GrantAsync("actor",scope.Fingerprint,"EQUIPMENT","pump-الكويت","READ",fault=="inherit",facilityType:fault=="qualifier"?"plant":null));
        Assert.Equal(RefusalKind.Invalid,error.Kind);Assert.Empty(f.Queries);Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }
    [Theory][InlineData("inherit")][InlineData("qualifier")]
    public async Task MalformedHistoricalEquipmentGrantCannotOpenAResourceQuery(string fault)
    {
        using var f=await Ready();await f.Grant("EQUIPMENT","pump-الكويت");var legacy=await f.Db.Set<AppUserAssetAccess>().SingleAsync();legacy.Inherit=fault=="inherit";legacy.FacilityType=fault=="qualifier"?"gas":null;await f.Db.SaveChangesAsync();
        Assert.False((await CompanyAssetFacadeTests.Service(f).CheckAssetAccessAsync("actor","pump-الكويت","EQUIPMENT")).HasAccess);Assert.Empty(f.Queries);Assert.Empty(f.Writes);
    }
    [Theory][InlineData("company")][InlineData("owner")][InlineData("revoke")][InlineData("field-only")]
    public async Task EffectiveEquipmentReadRequiresCurrentOwnedDirectAuthority(string fault)
    {
        using var f=await Ready();var row=f.Rows.OfType<EQUIPMENT>().Single();
        if(fault=="field-only")await f.Grant("FIELD","A",inherit:true);else await f.Grant("EQUIPMENT",row.EQUIPMENT_ID);
        var facade=CompanyAssetFacadeTests.Service(f);if(fault!="field-only")Assert.True((await facade.CheckAssetAccessAsync("actor",row.EQUIPMENT_ID,"EQUIPMENT")).HasAccess);
        if(fault=="company")await f.Select("other-company");if(fault=="owner")row.CURRENT_OWNER_BA_ID="other-company";
        if(fault=="revoke"){var grant=await f.Db.Set<AppUserAssetAccess>().SingleAsync();grant.IsActive=false;await f.Db.SaveChangesAsync();}
        Assert.False((await facade.CheckAssetAccessAsync("actor",row.EQUIPMENT_ID,"EQUIPMENT")).HasAccess);Assert.Empty(f.Writes);
    }
    [Theory][InlineData(PPDMRowMutation.Insert)][InlineData(PPDMRowMutation.Update)][InlineData(PPDMRowMutation.Delete)]
    public async Task DirectWriteGrantDoesNotOpenGenericEquipmentMutations(PPDMRowMutation operation)
    {
        using var f=await Ready();var row=f.Rows.OfType<EQUIPMENT>().Single();await f.Grant("EQUIPMENT",row.EQUIPMENT_ID,"DELETE");var before=f.Queries.Count;
        var error=await Assert.ThrowsAsync<RefusalException>(()=>f.Policy.ValidateMutationAsync(typeof(EQUIPMENT),"EQUIPMENT",row,operation==PPDMRowMutation.Insert?null:row,operation,"actor"));
        Assert.Equal(RefusalKind.Forbidden,error.Kind);Assert.Equal(before,f.Queries.Count);Assert.Empty(f.Writes);
    }
    [Theory][InlineData("owner")][InlineData("grant")]
    public async Task DetachedEquipmentCannotKeepAuthorityChangedDuringItsFinalNativeRead(string fault)
    {
        using var f=await Ready();var row=f.Rows.OfType<EQUIPMENT>().Single();await f.Grant("EQUIPMENT",row.EQUIPMENT_ID);var captured=CompanyInventorySourceReview.Copy(row);var changed=false;
        f.AfterRead=async()=>{if(changed)return;changed=true;if(fault=="owner")row.CURRENT_OWNER_BA_ID="other-company";else{var grant=await f.Db.Set<AppUserAssetAccess>().SingleAsync();grant.IsActive=false;await f.Db.SaveChangesAsync();}};
        if(fault=="owner")await Assert.ThrowsAsync<RefusalException>(()=>f.Policy.GetEffectiveAccessLevelAsync(typeof(EQUIPMENT),"EQUIPMENT",captured));else Assert.Null(await f.Policy.GetEffectiveAccessLevelAsync(typeof(EQUIPMENT),"EQUIPMENT",captured));
        Assert.True(changed);Assert.Empty(f.Writes);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ExistingAdminFormStateKeepsEquipmentDirectAndRequiresConfirmedReadback(bool invalidInheritance)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"admin",new(new("tenant","company"),["Administrator"],[])));var saved=new List<AssetAccess>();var calls=0;
        using var state=new AssetGrantAdministrationState(company,(_,_)=>Task.FromResult<List<AssetAccess>?>(saved.ToList()),(_,request,_)=>{calls++;Assert.Equal("EQUIPMENT",request.AssetType);Assert.Null(request.FacilityType);Assert.False(request.Inherit);saved.Add(new(){UserId=request.UserId,OrganizationId=request.OrganizationId,AssetType=request.AssetType,AssetId=request.AssetId,AccessLevel=request.AccessLevel,Inherit=false,Active=true});return Task.FromResult(true);},(_,_,_)=>throw new InvalidOperationException("Unexpected revoke"),(_,_,message)=>message,Mock.Of<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>());
        state.SelectUser("reader");await state.LoadAsync();state.SelectAssetType("EQUIPMENT");Assert.False(state.Inherit);state.AssetId="pump-الكويت";state.Inherit=invalidInheritance;await state.GrantAsync();
        Assert.Equal(invalidInheritance?0:1,calls);if(invalidInheritance){Assert.NotNull(state.Error);Assert.Empty(state.Grants);}else{Assert.Null(state.Error);Assert.NotNull(state.Confirmation);Assert.Equal("EQUIPMENT",Assert.Single(state.Grants).AssetType);}
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task HierarchyStateAcceptsTheDirectEquipmentButRejectsAnInventedField(bool fakeField)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"reader",new(new("tenant","company"),["Reader"],[])));
        var equipment=new AssetHierarchyNode{AssetId="pump-الكويت",AssetType="EQUIPMENT",UserHasAccess=true,ParentAssetId=fakeField?"A":null,ParentAssetType=fakeField?"FIELD":null};
        using var state=new AssetHierarchyState(company,(_,_)=>Task.FromResult<AssetHierarchyNode?>(new(){AssetId="company",AssetType="ORGANIZATION",Children=[equipment]}),(_,_,message)=>message);
        await state.LoadAsync();Assert.Equal(!fakeField,state.CanSelect(equipment));if(fakeField){Assert.NotNull(state.Error);Assert.Null(state.Hierarchy);}else{Assert.Null(state.Error);Assert.NotNull(state.Hierarchy);}
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task EquipmentVisibilityUsesTheExistingCompanyRevisionAndDoesNotDependOnFieldVisibility(bool withOrders)
    {
        using var f=await Ready();var store=new RepositoryHierarchyConfigurationStore(f.Db,f.Gate);
        var rows=new[]{"FIELD","WELL","POOL","FACILITY"}.Select(type=>new HierarchyConfig{OrganizationId="company",AssetType=type,HierarchyLevel=type=="FIELD"?0:1,ParentLevel=type=="FIELD"?null:0,LevelName=type,Active=true}).ToList();
        if(withOrders)rows.Add(new(){OrganizationId="company",AssetType="WORK_ORDER",HierarchyLevel=0,ParentLevel=null,LevelName="Orders",Active=true});Assert.True(await store.UpdateAsync("company",rows));
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Administrator"],[])));
        using var state=new HierarchyConfigurationState(company,(_,_)=>Read(),(_,_,message)=>message,(_,values,_)=>store.UpdateAsync("company",values));async Task<List<HierarchyConfig>?> Read()=>await store.ReadAsync("company",true);
        await state.LoadAsync();var previous=state.Configuration[0].Revision;Assert.True(state.CanAddEquipmentCategory);state.AddEquipmentCategory();state.AddEquipmentCategory();Assert.False(state.CanAddEquipmentCategory);var equipment=Assert.Single(state.Configuration.Where(row=>row.AssetType=="EQUIPMENT"));Assert.Equal(previous,equipment.Revision);Assert.Null(equipment.ParentLevel);Assert.Equal(0,equipment.HierarchyLevel);
        state.SetVisible(state.Configuration.Single(row=>row.AssetType=="FIELD"),false);Assert.True(equipment.Active);state.SetDisplayName(equipment,"معدات الشركة");await state.SaveAsync();
        Assert.Null(state.Error);Assert.NotNull(state.Confirmation);Assert.NotEqual(previous,state.Configuration[0].Revision);var saved=await store.ReadAsync("company",true);Assert.Equal(withOrders?6:5,saved.Count);Assert.True(saved.Single(row=>row.AssetType=="EQUIPMENT").Active);Assert.False(saved.Single(row=>row.AssetType=="FIELD").Active);Assert.Equal("معدات الشركة",saved.Single(row=>row.AssetType=="EQUIPMENT").LevelName);
    }
    [Theory][InlineData("parent")][InlineData("duplicate")]
    public async Task EquipmentConfigurationCannotInventAFieldRelationshipOrDuplicateCategory(string fault)
    {
        using var f=await Ready();var rows=new[]{"FIELD","WELL","POOL","FACILITY"}.Select(type=>new HierarchyConfig{OrganizationId="company",AssetType=type,HierarchyLevel=type=="FIELD"?0:1,ParentLevel=type=="FIELD"?null:0,LevelName=type,Active=true}).ToList();
        rows.Add(new(){OrganizationId="company",AssetType="EQUIPMENT",HierarchyLevel=fault=="parent"?1:0,ParentLevel=fault=="parent"?0:null,LevelName="Equipment",Active=true});if(fault=="duplicate")rows.Add(rows[^1]);
        Assert.Equal(RefusalKind.Invalid,(await Assert.ThrowsAsync<RefusalException>(()=>new RepositoryHierarchyConfigurationStore(f.Db,f.Gate).UpdateAsync("company",rows))).Kind);Assert.Empty(await new RepositoryHierarchyConfigurationStore(f.Db,f.Gate).ReadAsync("company",true));
    }
}
