#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMaintenanceWellEvidenceTests
{
    [Fact]
    public void ExistingComponentProjectionPreservesQualifiedFacilityDocumentAndUnresolvedRelationships()
    {
        var row=new WORK_ORDER_COMPONENT{WORK_ORDER_ID="order",COMPONENT_ID="component",FIELD_ID="field",ACTIVE_IND="Y",UWI="well",FACILITY_ID="facility",FACILITY_TYPE="PROCESSING",INFORMATION_ITEM_ID="document",INFO_ITEM_SUBTYPE="DOCUMENT",INSTRUMENT_ID="instrument"};var header=new WORK_ORDER{WORK_ORDER_ID="order",ACTIVE_IND="Y"};
        var first=RecordedWorkOrderRows.Project("field",[row],[header]).Single().Components.Single();Assert.Equal("facility",first.FacilityId);Assert.Equal("PROCESSING",first.FacilityType);Assert.Equal("document",first.InformationItemId);Assert.Equal("DOCUMENT",first.InformationItemSubtype);Assert.Contains("INSTRUMENT_ID",first.UnresolvedReferenceKinds!);Assert.True(ProcessDefinitionRevision.IsValid(first.NativeRevision));
        row.FACILITY_TYPE="STORAGE";var closing=RecordedWorkOrderRows.Project("field",[row],[header]).Single().Components.Single();Assert.NotEqual(first.NativeRevision,closing.NativeRevision);Assert.Equal(first.WellId,closing.WellId);
    }
    [Fact]
    public void OlderRetainedComponentJsonKeepsItsOriginalShapeWithoutInventingNativeEvidence()
    {
        const string original="{\"WorkOrderId\":\"order\",\"ComponentId\":\"component\",\"FieldId\":\"field\",\"WellId\":\"well\",\"EquipmentId\":\"\"}";var row=JsonSerializer.Deserialize<WorkOrderComponentReference>(original)!;Assert.Equal(original,JsonSerializer.Serialize(row));Assert.Null(row.NativeRevision);Assert.Null(row.FacilityId);
    }
    internal static object ProjectNativeRow(object row,string shape,string? omitted=null,bool duplicate=false)
    {
        var values=row.GetType().GetProperties(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.DeclaredOnly)
            .Where(property=>property.CanRead && property.CanWrite).ToDictionary(property=>property.Name,property=>property.GetValue(row)??DBNull.Value);
        if(omitted is not null)values.Remove(omitted);
        if(duplicate)values.Add("end_time",values["END_TIME"]);
        if(shape=="dictionary")return values;
        var table=new System.Data.DataTable();foreach(var name in values.Keys)table.Columns.Add(name,typeof(object));
        var current=table.NewRow();foreach(var pair in values)current[pair.Key]=pair.Value;table.Rows.Add(current);return current;
    }
    [Theory]
    [InlineData("WELL","ROW_QUALITY","dictionary",false)][InlineData("WELL_STATUS","END_TIME","dictionary",false)]
    [InlineData("R_WELL_STATUS","EXPIRY_DATE","dictionary",false)][InlineData("R_TIMEZONE","REMARK","dictionary",false)]
    [InlineData("WELL","ROW_QUALITY","table",false)][InlineData("WELL_STATUS","END_TIME","table",false)]
    [InlineData("R_WELL_STATUS","EXPIRY_DATE","table",false)][InlineData("R_TIMEZONE","REMARK","table",false)]
    [InlineData("WELL","ROW_QUALITY","dictionary",true)][InlineData("WELL_STATUS","END_TIME","dictionary",true)]
    [InlineData("R_WELL_STATUS","EXPIRY_DATE","dictionary",true)][InlineData("R_TIMEZONE","REMARK","dictionary",true)]
    [InlineData("WELL","ROW_QUALITY","table",true)][InlineData("WELL_STATUS","END_TIME","table",true)]
    [InlineData("R_WELL_STATUS","EXPIRY_DATE","table",true)][InlineData("R_TIMEZONE","REMARK","table",true)]
    public async Task WellProofRequiresEveryReturnedColumnWhileRetainingDeclaredNullAndZero(string table,string nullableColumn,string shape,bool missing)
    {
        var f=new Fixture();var before=JsonSerializer.Serialize(f.Native.Rows);
        f.Native.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string name,List<AppFilter> filters)=>
        {
            var rows=f.Native.Rows.Where(row=>row.GetType().Name==name && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).ToArray();
            return rows.Select(row=>name==table?ProjectNativeRow(row,shape,missing?nullableColumn:null):JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
        });
        if(missing)await Assert.ThrowsAsync<InvalidOperationException>(f.Read);
        else{var review=await f.Read();review.Require();Assert.Empty(review.Blockers);Assert.Equal(3,review.Facets.Count);}
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("dictionary")][InlineData("table")]
    public async Task CaseAmbiguousProviderColumnsCannotProduceConfirmedWellEvidence(string shape)
    {
        var f=new Fixture();f.Native.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>
            f.Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue))
                .Select(row=>table=="WELL_STATUS"?ProjectNativeRow(row,shape,duplicate:true):JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());
        await Assert.ThrowsAsync<InvalidOperationException>(f.Read);Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    internal sealed class Fixture
    {
        internal readonly PersonaTaskRoutingTests.Fixture Native;
        internal readonly Mock<IPPDMRowAccessPolicy> Access=new();
        internal readonly Mock<IPPDMMetadataRepository> Metadata=new();
        internal readonly FieldSelectionReview Context;
        internal string Level="WRITE";
        internal bool Revoked,Changed,ForeignStatus,ChangedZone;
        internal WellServices Owner;
        internal Fixture(string mode="current",FieldSelectionReview? context=null,PersonaTaskRoutingTests.Fixture? native=null)
        {
            Native=native??new();Context=context??new(new("tenant","company"),new("lifecycle","fingerprint"),"return-actor","current-source-authority","field");if(native is null)Native.Rows.Clear();var utc=DateTime.UtcNow;var definitions=PPDM39Metadata.GetMetadata();Metadata.Setup(value=>value.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync((string table)=>definitions[table]);Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>definitions[table].PrimaryKeyColumn.Split(',').ToList());
            var creator=new ClassCreator(Native.Editor.Object);Native.Editor.SetupGet(value=>value.classCreator).Returns(creator);Native.Source.Setup(value=>value.CheckEntityExist(It.IsAny<string>())).Returns(true);
            Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),It.IsAny<bool>())).Returns((EntityStructure entity,bool _)=>creator.ConvertToEntityStructure(typeof(WELL).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+entity.EntityName)!));
            Native.Rows.Add(new WELL{UWI="well",ASSIGNED_FIELD=mode=="foreign-field"?"other":"field",OPERATOR=mode=="foreign-company"?"other":"company",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-30),EXPIRY_DATE=utc.AddYears(1)});
            foreach(var (facet,value) in new[]{("Life Cycle","Operating"),("Role","Produce"),("Condition",mode=="abandoned"?"Abandoned":"Shut In")})
            {
                Native.Rows.Add(new WELL_STATUS{UWI="well",SOURCE="recorded-source",STATUS_ID=facet+"-original",STATUS_TYPE=facet,STATUS=value,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),TIMEZONE="UTC"});Native.Rows.Add(new R_WELL_STATUS{STATUS_TYPE=facet,STATUS=value,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
            }
            Native.Rows.Add(new R_WELL_STATUS{STATUS_TYPE="Condition",STATUS="Active",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});Native.Rows.Add(new R_TIMEZONE{TIMEZONE="UTC",ACTIVE_IND="Y"});
            var condition=Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_TYPE=="Condition");if(mode=="missing-condition")Native.Rows.Remove(condition);if(mode=="missing-reference")Native.Rows.RemoveAll(row=>row is R_WELL_STATUS reference && reference.STATUS=="Active");if(mode=="overlap")Native.Rows.Add(new WELL_STATUS{UWI="well",SOURCE="second-source",STATUS_ID="condition-overlap",STATUS_TYPE="Condition",STATUS="Shut In",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddHours(-1),TIMEZONE="UTC"});if(mode=="future")condition.EFFECTIVE_DATE=utc.AddDays(1);if(mode=="expired")condition.EXPIRY_DATE=utc.AddHours(-1);if(mode=="read-only")Level="READ";if(mode=="qualified-role")Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_TYPE=="Role").STATUS_QUALIFIER="Unreviewed role qualifier";
            string? zone="UTC";if(mode=="unspecified-no-zone"){zone=null;condition.TIMEZONE=null;condition.EFFECTIVE_DATE=DateTime.SpecifyKind(condition.EFFECTIVE_DATE!.Value.AddTicks(1),DateTimeKind.Unspecified);}if(mode=="unspecified-kuwait"){zone="Asia/Kuwait";condition.TIMEZONE="Asia/Kuwait";condition.EFFECTIVE_DATE=DateTime.SpecifyKind(utc.AddDays(-1).AddHours(3),DateTimeKind.Unspecified);Native.Rows.Add(new R_TIMEZONE{TIMEZONE="Asia/Kuwait",ACTIVE_IND="Y"});}if(mode=="unknown-zone"){condition.TIMEZONE="missing-zone";condition.EFFECTIVE_DATE=DateTime.SpecifyKind(utc.AddDays(-1),DateTimeKind.Unspecified);}if(mode=="local-time")condition.EFFECTIVE_DATE=DateTime.SpecifyKind(utc.AddDays(-1).AddTicks(1),DateTimeKind.Local);if(mode=="ambiguous-time"){condition.TIMEZONE="America/New_York";condition.EFFECTIVE_DATE=new DateTime(2026,11,1,1,30,0,DateTimeKind.Unspecified);Native.Rows.Add(new R_TIMEZONE{TIMEZONE="America/New_York",ACTIVE_IND="Y"});}
            Access.Setup(value=>value.GetReadConjunctionsAsync(typeof(WELL),"WELL")).ReturnsAsync(new List<IReadOnlyList<AppFilter>>{new[]{new AppFilter{FieldName="UWI",Operator="=",FilterValue="well"}}});Access.Setup(value=>value.GetReadConstraintsAsync(typeof(WELL),"WELL")).ReturnsAsync(new[]{new AppFilter{FieldName="OPERATOR",Operator="=",FilterValue="company"}});Access.Setup(value=>value.ValidateRowsAsync(typeof(WELL),"WELL",It.IsAny<IReadOnlyList<object>>())).Returns(()=>Revoked?Task.FromException(new UnauthorizedAccessException("Well grants revoked")):Task.CompletedTask);Access.Setup(value=>value.GetEffectiveAccessLevelAsync(typeof(WELL),"WELL",It.IsAny<object>(),true)).ReturnsAsync(()=>Revoked?null:Level);
            var statusReads=0;Native.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>
            {
                if(table=="WELL_STATUS" && ++statusReads==4 && Changed)condition.STATUS="Idle";if(table=="WELL_STATUS" && statusReads==4 && ChangedZone)Native.Rows.OfType<R_TIMEZONE>().Single(row=>row.TIMEZONE=="UTC").ROW_QUALITY="changed";
                if(table=="WELL_STATUS" && ForeignStatus)return new object[]{new WELL_STATUS{UWI="foreign",SOURCE="source",STATUS_ID="foreign-status",STATUS_TYPE=filters.Single(value=>value.FieldName=="STATUS_TYPE").FilterValue,STATUS="Shut In",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1)}};
                return Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
            });
            if(mode=="metadata-key")Metadata.Setup(value=>value.GetTableMetadataAsync("WELL_STATUS")).ReturnsAsync(new PPDMTableMetadata{EntityTypeName="WELL_STATUS",TableName="WELL_STATUS",PrimaryKeyColumn="STATUS_ID"});
            Owner=mode=="missing-policy"?new(Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Metadata.Object,Context.Database.ConnectionName):new(Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Metadata.Object,Context.Database.ConnectionName,Access.Object,zone);
        }
        internal Task Guard()=>Revoked?Task.FromException(new UnauthorizedAccessException("Well return authority revoked")):Task.CompletedTask;
        internal Task<CompanyMaintenanceWellEvidence> Read()=>Owner.ReadMaintenanceWellEvidenceAsync(Context,"well",Guard);
    }
    [Theory][InlineData("current")][InlineData("unspecified-kuwait")]
    public async Task ActualCanonicalOwnerReadsIndependentRecordedFacetsReferencesAndNativeTimeZoneWithoutWrites(string mode)
    {
        var f=new Fixture(mode);var result=await f.Read();result.Require();Assert.Empty(result.Blockers);Assert.Equal(3,result.Facets.Count);Assert.Equal("Operating",result.Facets.Single(row=>row.Facet=="Life Cycle").Value);Assert.Equal("Produce",result.Facets.Single(row=>row.Facet=="Role").Value);Assert.Equal("Shut In",result.Facets.Single(row=>row.Facet=="Condition").Value);Assert.Contains(result.References,row=>row.Facet=="Condition" && row.Value=="Active");Assert.All(result.Facets,row=>Assert.Equal(DateTimeKind.Utc,row.EffectiveAtUtc!.Value.Kind));Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("missing-condition")][InlineData("missing-reference")][InlineData("overlap")][InlineData("future")][InlineData("expired")][InlineData("abandoned")][InlineData("qualified-role")][InlineData("read-only")][InlineData("unspecified-no-zone")][InlineData("unknown-zone")][InlineData("local-time")][InlineData("ambiguous-time")]
    public async Task MissingAmbiguousExpiredUnsupportedOrUninterpretableEvidenceRemainsAnExplicitBlocker(string mode)
    {
        var f=new Fixture(mode);if(mode=="unspecified-no-zone")Assert.Equal(DateTimeKind.Unspecified,f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_TYPE=="Condition").EFFECTIVE_DATE!.Value.Kind);if(mode=="local-time")Assert.Equal(DateTimeKind.Local,f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_TYPE=="Condition").EFFECTIVE_DATE!.Value.Kind);var result=await f.Read();result.Require();Assert.NotEmpty(result.Blockers);Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("missing-policy")][InlineData("metadata-key")][InlineData("foreign-field")][InlineData("foreign-company")][InlineData("foreign-status")][InlineData("revoked")][InlineData("changed")][InlineData("changed-zone")]
    public async Task CanonicalMetadataCompanyParentSourceAndReferenceChangesRefuseWithoutFallbackOrWrites(string fault)
    {
        var f=new Fixture(fault){ForeignStatus=fault=="foreign-status",Revoked=fault=="revoked",Changed=fault=="changed",ChangedZone=fault=="changed-zone"};await Assert.ThrowsAnyAsync<Exception>(f.Read);Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);if(fault is "missing-policy" or "revoked")Assert.DoesNotContain(f.Native.Source.Invocations,value=>value.Method.Name=="GetEntityAsync");
    }
}
#endif
