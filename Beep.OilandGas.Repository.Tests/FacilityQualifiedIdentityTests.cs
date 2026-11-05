using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Moq;
using Xunit;
using Fixture=Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
namespace Beep.OilandGas.Repository.Tests;

public sealed class FacilityQualifiedIdentityTests
{
    private static FacilityManagementService Owner(Fixture f,Func<string,Task<string>>? resolver=null)=>new(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),"unselected-global",resolveModuleConnection:resolver??(_=>Task.FromResult("selected-tenant")));
    private static async Task<Fixture> Ready()
    {
        var f=await Fixture.Create();f.Rows.Add(new FACILITY{FACILITY_ID="plant-الكويت",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR="company",ACTIVE_IND="Y"});
        f.Rows.Add(new FACILITY{FACILITY_ID="plant-الكويت",FACILITY_TYPE="oil",PRIMARY_FIELD_ID="B",CURRENT_OPERATOR="other-company",ACTIVE_IND="Y"});return f;
    }
    [Fact]
    public async Task ActualOwnerRequiresBothNativeKeysAndNeverSelectsAnotherTypeOrLatestRow()
    {
        using var f=await Ready();var row=await Owner(f).GetFacilityAsync("plant-الكويت","gas");Assert.NotNull(row);Assert.Equal("gas",row.FACILITY_TYPE);Assert.Equal("A",row.PRIMARY_FIELD_ID);
        var query=Assert.Single(f.Queries);Assert.Equal(3,query.Count);Assert.Contains(query,filter=>filter.FieldName=="FACILITY_ID" && filter.FilterValue==row.FACILITY_ID);Assert.Contains(query,filter=>filter.FieldName=="FACILITY_TYPE" && filter.FilterValue=="gas");
        Assert.Null(await Owner(f).GetFacilityAsync("plant-الكويت","unrecorded"));Assert.Empty(f.Writes);
    }
    [Theory][InlineData(null)][InlineData("")][InlineData(" ")][InlineData(" gas")][InlineData("gas\n")]
    public async Task MissingOrNonCanonicalQualificationRefusesBeforeAnyParentOrChildRead(string? type)
    {
        using var f=await Ready();var error=await Assert.ThrowsAsync<RefusalException>(()=>Owner(f).ListFacilityEquipmentAsync("plant-الكويت",type));
        Assert.Equal(RefusalKind.Invalid,error.Kind);Assert.Empty(f.Queries);Assert.Empty(f.Writes);
    }
    [Theory][InlineData("duplicate")][InlineData("foreign-type")][InlineData("foreign-id")][InlineData("inactive")][InlineData("model")]
    public async Task PredicateViolationsAndAmbiguousQualifiedMembershipCannotBecomeAChosenFacility(string fault)
    {
        using var f=await Ready();f.Rows.RemoveAll(row=>row is FACILITY facility && facility.FACILITY_TYPE=="oil");var row=f.Rows.OfType<FACILITY>().Single();
        if(fault=="duplicate")f.Rows.Add(new FACILITY{FACILITY_ID=row.FACILITY_ID,FACILITY_TYPE=row.FACILITY_TYPE,ACTIVE_IND="Y",ROW_CHANGED_DATE=DateTime.UtcNow.AddDays(1)});
        if(fault=="foreign-type"){row.FACILITY_TYPE="other";f.IgnoreFilters=true;}if(fault=="foreign-id"){row.FACILITY_ID="other";f.IgnoreFilters=true;}if(fault=="inactive"){row.ACTIVE_IND="N";f.IgnoreFilters=true;}
        if(fault=="model")f.Source.Setup(source=>source.GetEntityAsync("FACILITY",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{new EQUIPMENT{EQUIPMENT_ID="pump"}});
        await Assert.ThrowsAsync<RefusalException>(()=>Owner(f).GetFacilityAsync("plant-الكويت","gas"));Assert.Empty(f.Writes);
    }
    [Theory][InlineData("missing-type")][InlineData("wrong-model")]
    public async Task QualifiedReadsRequireTheCanonicalCompoundMetadata(string fault)
    {
        using var f=await Ready();var metadata=await f.Metadata.Object.GetTableMetadataAsync("FACILITY");if(fault=="missing-type")metadata.PrimaryKeyColumn="FACILITY_ID";else metadata.EntityTypeName="EQUIPMENT";
        await Assert.ThrowsAsync<RefusalException>(()=>Owner(f).GetFacilityAsync("plant-الكويت","gas"));Assert.Empty(f.Queries);Assert.Empty(f.Writes);
    }
    [Theory][InlineData("binding")][InlineData("source")][InlineData("cancelled")][InlineData("metadata")]
    public async Task ChangedSourceOrCancellationCannotPublishTheEarlierQualifiedRow(string fault)
    {
        using var f=await Ready();var name="selected-tenant";using var cancellation=new CancellationTokenSource();
        f.AfterRead=async()=>{if(fault=="binding")name="other-core";if(fault=="source")f.Editor.Setup(editor=>editor.GetDataSource("selected-tenant")).Returns(Mock.Of<TheTechIdea.Beep.IDataSource>());if(fault=="cancelled")cancellation.Cancel();if(fault=="metadata")(await f.Metadata.Object.GetTableMetadataAsync("FACILITY")).PrimaryKeyColumn="FACILITY_ID";};
        var owner=Owner(f,_=>Task.FromResult(name));if(fault=="cancelled")await Assert.ThrowsAsync<OperationCanceledException>(()=>owner.GetFacilityAsync("plant-الكويت","gas",cancellation.Token));else await Assert.ThrowsAsync<RefusalException>(()=>owner.GetFacilityAsync("plant-الكويت","gas"));Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task EquipmentChildrenUseTheExactOriginalQualifiedFacilityWithoutCrossTypeRows()
    {
        using var f=await Ready();f.Rows.Add(new FACILITY_EQUIPMENT{FACILITY_ID="plant-الكويت",FACILITY_TYPE="gas",EQUIPMENT_ID="gas-pump",INSTALL_OBS_NO=1,ACTIVE_IND="Y"});f.Rows.Add(new FACILITY_EQUIPMENT{FACILITY_ID="plant-الكويت",FACILITY_TYPE="oil",EQUIPMENT_ID="foreign-pump",INSTALL_OBS_NO=2,ACTIVE_IND="Y"});
        Assert.Equal("gas-pump",Assert.Single(await Owner(f).ListFacilityEquipmentAsync("plant-الكويت","gas")).EQUIPMENT_ID);Assert.Equal(2,f.Queries.Count);Assert.All(f.Queries,query=>Assert.Contains(query,filter=>filter.FieldName=="FACILITY_TYPE" && filter.FilterValue=="gas"));Assert.Empty(f.Writes);
    }
}
