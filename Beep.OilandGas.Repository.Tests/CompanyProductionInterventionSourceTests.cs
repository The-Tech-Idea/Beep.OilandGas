using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;
using Fixture=Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CompanyProductionBoundaryTests
{
    [Fact]public async Task AnotherCompanyWithTheSameWellGrantCannotReadTheOriginalCompanyActivity()
    {
        var (data,owner,_)=await ActivitySource();using(data){data.Http.HttpContext=data.Request;Assert.Single(await owner.GetWellActivitiesForFieldAsync("A"));await data.Select("other-company");data.Http.HttpContext=data.Request;await data.Grant("FIELD","A");await data.Grant("WELL","well");var profile=Profile(data);await Save(profile);data.Source.Invocations.Clear();Assert.Empty(await Phase(data,profile).GetWellActivitiesForFieldAsync("A"));data.Source.Verify(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<AppFilter>>()),Times.Never);Assert.Empty(data.Writes);}
    }
    private static WELL_ACTIVITY Activity(string well="well")=>new(){UWI=well,SOURCE="engineer-capture",ACTIVITY_OBS_NO=1,ACTIVE_IND="Y",ACTIVITY_TYPE_ID="WORKOVER_CANDIDATE",REMARK="Original recorded intervention",ROW_CREATED_BY="engineer"};
    private static async Task<(Fixture Data,PPDMProductionService Owner,WELL Well)> ActivitySource()
    {
        var data=await Ready();var profile=Profile(data);await Save(profile);var well=new WELL{UWI="well",ASSIGNED_FIELD="A",OPERATOR="company",ACTIVE_IND="Y"};data.Rows.Add(well);data.Rows.Add(Activity());await data.Grant("WELL","well");return(data,Phase(data,profile),well);
    }
    [Fact]public async Task ActualRecordedActivityUsesCompleteNativeIdentityAndVerifiedCompanyWellParent()
    {
        var (data,owner,_)=await ActivitySource();using(data){data.Http.HttpContext=data.Request;data.Rows.Add(new WELL{UWI="foreign",ASSIGNED_FIELD="A",OPERATOR="other-company",ACTIVE_IND="Y"});data.Rows.Add(Activity("foreign"));var row=Assert.Single(await owner.GetWellActivitiesForFieldAsync("A"));Assert.Equal("well",row.UWI);Assert.Equal("engineer-capture",row.SOURCE);Assert.Equal(1,row.ACTIVITY_OBS_NO);Assert.Equal("engineer",row.ROW_CREATED_BY);Assert.Empty(data.Writes);data.Source.Verify(value=>value.GetEntityAsync("WELL_ACTIVITY",It.Is<List<AppFilter>>(filters=>filters.Any(filter=>filter.FieldName=="UWI" && filter.FilterValue=="well") && filters.Any(filter=>filter.FieldName=="ACTIVE_IND" && filter.FilterValue=="Y"))),Times.AtLeastOnce);}
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualRecordedActivityRequiresExplicitFieldInheritanceOrDirectWellGrant(bool inherit)
    {
        using var data=await Ready();var profile=Profile(data);await Save(profile);data.Rows.Add(new WELL{UWI="well",ASSIGNED_FIELD="A",OPERATOR="company",ACTIVE_IND="Y"});data.Rows.Add(Activity());await data.Grant("FIELD","A",inherit:inherit);var rows=await Phase(data,profile).GetWellActivitiesForFieldAsync("A");Assert.Equal(inherit?1:0,rows.Count);Assert.Empty(data.Writes);
    }
    [Theory][InlineData("field")][InlineData("company")][InlineData("grant")]
    public async Task ActivityReadCannotKeepChangedParentFieldCompanyOrWellGrant(string change)
    {
        var (data,owner,well)=await ActivitySource();using(data){data.Http.HttpContext=data.Request;var changed=false;data.Source.Setup(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<AppFilter>>())).Returns(async()=>{changed=true;if(change=="field")well.ASSIGNED_FIELD="B";if(change=="company")well.OPERATOR="other-company";if(change=="grant"){foreach(var grant in await data.Db.Set<AppUserAssetAccess>().Where(value=>value.AssetType=="WELL").ToListAsync())grant.IsActive=false;await data.Db.SaveChangesAsync();}return(IEnumerable<object>)new[]{Activity()};});await Assert.ThrowsAnyAsync<Exception>(()=>owner.GetWellActivitiesForFieldAsync("A"));Assert.True(changed);Assert.Empty(data.Writes);}
    }
    [Theory][InlineData("key")][InlineData("relationship")]
    public async Task NativeActivityMetadataDriftRefusesBeforeActivityProviderRead(string change)
    {
        var (data,owner,_)=await ActivitySource();using(data){data.Http.HttpContext=data.Request;var original=await data.Metadata.Object.GetTableMetadataAsync("WELL_ACTIVITY");var altered=System.Text.Json.JsonSerializer.Deserialize<PPDMTableMetadata>(System.Text.Json.JsonSerializer.Serialize(original))!;if(change=="key")altered.PrimaryKeyColumn="UWI";else altered.ForeignKeys.RemoveAll(value=>value.ForeignKeyColumn=="UWI" && value.ReferencedTable=="WELL");data.Metadata.Setup(value=>value.GetTableMetadataAsync("WELL_ACTIVITY")).ReturnsAsync(altered);await Assert.ThrowsAnyAsync<Exception>(()=>owner.GetWellActivitiesForFieldAsync("A"));data.Source.Verify(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<AppFilter>>()),Times.Never);Assert.Empty(data.Writes);}
    }
    [Fact]public async Task ActivityProviderOverReturnCannotPublishAnUnassignedWell()
    {
        var (data,owner,_)=await ActivitySource();using(data){data.Http.HttpContext=data.Request;data.Source.Setup(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<AppFilter>>())).ReturnsAsync((IEnumerable<object>)new[]{Activity(),Activity("unassigned")});await Assert.ThrowsAnyAsync<Exception>(()=>owner.GetWellActivitiesForFieldAsync("A"));data.Source.Verify(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<AppFilter>>()),Times.AtLeastOnce);Assert.Empty(data.Writes);}
    }
    [Theory][InlineData("source")][InlineData("negative")][InlineData("fraction")][InlineData("conflicting-key")]
    public async Task ActivityIdentityCannotBeIncompleteFractionalOrAmbiguous(string change)
    {
        var (data,owner,_)=await ActivitySource();using(data){data.Http.HttpContext=data.Request;var row=data.Rows.OfType<WELL_ACTIVITY>().Single();if(change=="source")row.SOURCE="";if(change=="negative")row.ACTIVITY_OBS_NO=-1;if(change=="fraction")row.ACTIVITY_OBS_NO=1.5m;if(change=="conflicting-key"){var conflict=Activity();conflict.REMARK="Another native body for the same complete key";data.Rows.Add(conflict);}await Assert.ThrowsAnyAsync<Exception>(()=>owner.GetWellActivitiesForFieldAsync("A"));data.Source.Verify(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<AppFilter>>()),Times.AtLeastOnce);Assert.Empty(data.Writes);}
    }
    [Fact]public async Task ReadableActivityDoesNotGrantGenericDecisionWriteAuthority()
    {
        var (data,owner,_)=await ActivitySource();using(data){data.Http.HttpContext=data.Request;await data.Grant("WELL","well","WRITE");Assert.Single(await owner.GetWellActivitiesForFieldAsync("A"));await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordInterventionDecisionAsync("well","WORKOVER_REJECTED","A typed original decision is required","actor"));Assert.Empty(data.Writes);}
    }
}
