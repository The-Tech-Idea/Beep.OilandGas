using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Xunit;
using ReviewFixture=Beep.OilandGas.Repository.Tests.CompanyMaintenanceQualifiedAssetTests.ReviewFixture;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CompanyMaintenanceQualifiedLicensingTests
{
    private static CompanyMaintenanceQualifiedReturnObservation[] QualifiedObservations()=>[new("FACILITY","plant","gas",0m),new("EQUIPMENT","pump",null,72.5m)];
    private static async Task<(ReviewFixture Fixture,CompanyMaintenanceQualifiedAssets Sources)> ReadyQualifiedReturn(string mode="current",bool completeCommand=false,bool includeWell=false)
    {
        var f=await ReadyPolicy();if(mode=="facility-only")f.Order.Components=[f.Order.Components[0] with{EquipmentId=""}];if(mode=="equipment-only")f.Order.Components=[f.Order.Components[0] with{FacilityId=null,FacilityType=null}];await f.Native.Grant("EQUIPMENT","pump","WRITE");await f.Native.Grant("FACILITY","plant","WRITE",facilityType:"gas");
        if(mode=="equipment-only")await f.Native.Grant("FACILITY","plant","READ",facilityType:"gas");
        if(mode=="preserve")
        {
            f.Native.Rows.Add(new FACILITY_STATUS{FACILITY_ID="plant",FACILITY_TYPE="gas",STATUS_ID="preserved-safety",STATUS_TYPE="NativeSafety",STATUS="KNOWN",ACTIVE_IND="Y",EFFECTIVE_DATE=DateTime.UtcNow.AddDays(-1),SOURCE="native-status",TIMEZONE="UTC",PERCENT_CAPABILITY=43m});
            f.Native.Rows.Add(new R_FAC_STATUS_TYPE{STATUS_TYPE="NativeSafety",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FACILITY_STATUS{STATUS_TYPE="NativeSafety",STATUS="KNOWN",ACTIVE_IND="Y"});
        }
        if(mode=="zero")foreach(var row in f.Native.Rows.Where(row=>row is FACILITY_STATUS or EQUIPMENT_STATUS))row.GetType().GetProperty("PERCENT_CAPABILITY")!.SetValue(row,0m);
        if(mode=="unspecified")foreach(var row in f.Native.Rows.Where(row=>row is FACILITY_STATUS or EQUIPMENT_STATUS))
        {row.GetType().GetProperty("TIMEZONE")!.SetValue(row,"Asia/Kuwait");row.GetType().GetProperty("EFFECTIVE_DATE")!.SetValue(row,DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1).AddHours(3),DateTimeKind.Unspecified));}
        if(mode=="unspecified")f.Native.Rows.Add(new R_TIMEZONE{TIMEZONE="Asia/Kuwait",ACTIVE_IND="Y"});
        if(completeCommand){f.Order.State="";f.Order.EvidenceUnavailableReason="No inferred header execution state";}
        if(completeCommand)f.Order.Components.AddRange([
            new(f.Order.InstanceId,"isolation-component",f.Order.FieldId,"",""){InformationItemId="ISOLATION-information",InformationItemSubtype="DOCUMENT",NativeRevision=new string('B',64)},
            new(f.Order.InstanceId,"risk-component",f.Order.FieldId,"",""){InformationItemId="RISK-information",InformationItemSubtype="DOCUMENT",NativeRevision=new string('C',64)}]);
        if(includeWell)f.Order.Components.Add(new(f.Order.InstanceId,"well-component",f.Order.FieldId,"well",""){NativeRevision=new string('D',64)});
        SourceReferences(f);f.Native.EnableCorrectiveActionTransactions();var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(f.Native.Source.Object.Dataconnection.ConnectionProp);
        connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.Native.LeasePhysicalConnection.Object);f.Native.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase))["WORK_ORDER_MAINTENANCE_RETURN_POLICY"];
        f.Native.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_MAINTENANCE_RETURN_POLICY")).ReturnsAsync(metadata);f.Native.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_MAINTENANCE_RETURN_POLICY")).ReturnsAsync(["POLICY_RECORD_ID"]);
        f.Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool _)=>
        {
            var type=request.EntityName=="WORK_ORDER_ASSET_RETURN"?typeof(WORK_ORDER_ASSET_RETURN):request.EntityName=="WORK_ORDER_MAINTENANCE_RETURN_POLICY"?typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+request.EntityName)!;
            var shape=f.Native.Editor.Object.classCreator.ConvertToEntityStructure(type);var keys=f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(request.EntityName).GetAwaiter().GetResult();foreach(var field in shape.Fields)field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal);return shape;
        });
        var compared=await Compare(f,[ApplicablePolicy()]);var context=CompanyMaintenanceQualifiedReturnPolicy.ContextFor(compared.Sources,["FacilityRole"]);
        var empty=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard);var draft=ApplicablePolicy();
        if(mode=="equipment-only")draft=draft with{Transitions=[draft.Transitions[0] with{TargetValue="UNUSED_PARENT_TARGET",NativeSource="unused-parent-source"},draft.Transitions[1]]};
        if(mode=="preserve")draft=draft with{Transitions=[..draft.Transitions,new("FACILITY","NativeSafety",["KNOWN"],"","","Explicit preservation"){Disposition="PRESERVE"}]};
        await f.Owner.RecordMaintenanceReturnPolicyAsync(new(context,empty.Revision,draft,1,Guid.NewGuid(),"Reviewed native return policy"),f.Guard);
        var catalog=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard);var nativeSources=await f.Owner.ReadMaintenanceReturnNativeSourcesAsync(f.Context,f.Order,compared.Sources,catalog,f.Guard);
        var policy=CompanyMaintenanceQualifiedReturnPolicy.Create(compared.Sources,f.Order,catalog,["FacilityRole"],nativeSources,DateTime.UtcNow);
        Assert.Empty(policy.Blockers);return(f,compared.Sources with{ReturnPolicy=policy});
    }

    [Theory][InlineData("current")][InlineData("zero")][InlineData("preserve")][InlineData("unspecified")]
    public async Task QualifiedReturnPreparationRetainsExactNativeNullZeroHistoryAndPreservedFacetsWithoutWrites(string mode)
    {
        var ready=await ReadyQualifiedReturn(mode);using var f=ready.Fixture;var source=ready.Sources;var writes=f.Native.Writes.Count;
        var original=JsonSerializer.Serialize(f.Native.Rows);var observations=QualifiedObservations();var at=DateTime.UtcNow;
        var changes=CompanyMaintenanceQualifiedAssetReturn.Resolve(source,f.Order,observations,"FacilityRole",at);Assert.Equal(2,changes.Count);
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var rows=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(source,f.Order,observations,"FacilityRole","original-return",at,"Explicit qualified observations",f.Guard);
            Assert.Equal(DateTimeKind.Utc,rows.RecordedAtUtc.Kind);
            Assert.True(rows.RecordedAtUtc>=at);Assert.Equal(rows.BeforeFacilities.Length+1,rows.AfterFacilities.Length);Assert.Equal(rows.BeforeEquipment.Length+1,rows.AfterEquipment.Length);
            var facility=rows.AfterFacilities.Single(row=>row.ACTIVE_IND=="Y" && row.STATUS_TYPE=="NativeCondition");var equipment=rows.AfterEquipment.Single(row=>row.ACTIVE_IND=="Y");
            Assert.Equal("RECORDED_TARGET",facility.STATUS);Assert.Equal("RECORDED_TARGET",equipment.EQUIP_STATUS);Assert.Equal("recorded-policy-source",facility.SOURCE);Assert.Equal("UTC",equipment.TIMEZONE);Assert.Equal(0m,facility.PERCENT_CAPABILITY);Assert.Equal(72.5m,equipment.PERCENT_CAPABILITY);
            Assert.Equal(at,facility.EFFECTIVE_DATE);Assert.Equal(at,equipment.START_TIME);Assert.Null(facility.END_TIME);Assert.Null(equipment.EXPIRY_DATE);Assert.Equal("actor",facility.ROW_CREATED_BY);Assert.Equal(rows.RecordedAtUtc,equipment.ROW_CREATED_DATE);
            foreach(var change in changes)Assert.Contains(change.StatusId("original-return"),new[]{facility.STATUS_ID,equipment.EQUIP_STATUS_ID});
            var expired=rows.AfterFacilities.Single(row=>row.STATUS_ID=="facility-status");Assert.Equal("N",expired.ACTIVE_IND);
            if(mode=="zero")Assert.Equal(0m,expired.PERCENT_CAPABILITY);else Assert.Null(expired.PERCENT_CAPABILITY);
            if(mode=="unspecified"){Assert.Equal(DateTimeKind.Unspecified,expired.EXPIRY_DATE!.Value.Kind);Assert.Equal(at.AddHours(3).Ticks,expired.EXPIRY_DATE.Value.Ticks);}else Assert.Equal(at,expired.EXPIRY_DATE);
            if(mode=="preserve")Assert.Equal(JsonSerializer.Serialize(rows.BeforeFacilities.Single(row=>row.STATUS_ID=="preserved-safety")),JsonSerializer.Serialize(rows.AfterFacilities.Single(row=>row.STATUS_ID=="preserved-safety")));
        });
        Assert.Equal(original,JsonSerializer.Serialize(f.Native.Rows));Assert.Equal(writes,f.Native.Writes.Count);Assert.Single(f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>());
    }

    [Fact]
    public async Task QualifiedReturnFacilityOnlyPreparationDoesNotRequireAnEquipmentStatusSchemaOrReadItsRows()
    {
        var ready=await ReadyQualifiedReturn("facility-only");using var f=ready.Fixture;
        f.Native.Source.Setup(value=>value.CheckEntityExist(It.IsAny<string>())).Returns((string table)=>table!="EQUIPMENT_STATUS");f.Native.Source.Invocations.Clear();
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var rows=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,[QualifiedObservations()[0]],"FacilityRole","facility-only-return",DateTime.UtcNow,"Explicit facility observation",f.Guard);
            Assert.Single(rows.BeforeFacilities);Assert.Equal(2,rows.AfterFacilities.Length);Assert.Empty(rows.BeforeEquipment);Assert.Empty(rows.AfterEquipment);
        });
        f.Native.Source.Verify(value=>value.CheckEntityExist("EQUIPMENT_STATUS"),Times.Never);
        f.Native.Source.Verify(value=>value.GetEntityAsync("EQUIPMENT_STATUS",It.IsAny<List<AppFilter>>()),Times.Never);
        Assert.Single(f.Native.Writes);Assert.DoesNotContain(f.Native.Writes,row=>row is FACILITY_STATUS or EQUIPMENT_STATUS);
    }

    [Fact]
    public async Task QualifiedReturnEquipmentOnlyPreparationKeepsItsReadOnlyInstalledFacilityOutsideTheMutationTargets()
    {
        var ready=await ReadyQualifiedReturn("equipment-only");using var f=ready.Fixture;
        Assert.Equal("READ",Assert.Single(ready.Sources.Facilities).AccessLevel);
        var observations=new[]{QualifiedObservations()[1]};var transitions=CompanyMaintenanceQualifiedAssetReturn.Resolve(ready.Sources,f.Order,observations,"FacilityRole",DateTime.UtcNow);
        Assert.Equal("EQUIPMENT",Assert.Single(transitions).AssetType);
        Assert.Throws<InvalidOperationException>(()=>CompanyMaintenanceQualifiedAssetReturn.Resolve(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole",DateTime.UtcNow));
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var rows=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,observations,"FacilityRole","equipment-only-return",DateTime.UtcNow,"Explicit equipment observation",f.Guard);
            Assert.Equal(JsonSerializer.Serialize(rows.BeforeFacilities),JsonSerializer.Serialize(rows.AfterFacilities));Assert.Single(rows.BeforeEquipment);Assert.Equal(2,rows.AfterEquipment.Length);
        });
        Assert.Single(f.Native.Writes);Assert.DoesNotContain(f.Native.Writes,row=>row is FACILITY_STATUS or EQUIPMENT_STATUS);
    }

    [Theory][InlineData("omit")][InlineData("duplicate")][InlineData("foreign")][InlineData("qualifier")][InlineData("missing-percent")][InlineData("over-percent")][InlineData("role")][InlineData("before-review")]
    public async Task QualifiedReturnRequiresEveryOriginalObservationExactQualifierRoleAndActualTime(string mode)
    {
        var ready=await ReadyQualifiedReturn();using var f=ready.Fixture;var observations=QualifiedObservations();var at=DateTime.UtcNow;
        if(mode=="omit")observations=[observations[0]];if(mode=="duplicate")observations=[observations[0],observations[0]];
        if(mode=="foreign")observations=[observations[0],observations[1] with{AssetId="other"}];if(mode=="qualifier")observations=[observations[0] with{FacilityType="oil"},observations[1]];
        if(mode=="missing-percent")observations=[observations[0] with{ObservedCapabilityPercent=null},observations[1]];if(mode=="over-percent")observations=[observations[0],observations[1] with{ObservedCapabilityPercent=100.1m}];
        if(mode=="before-review")at=ready.Sources.ComparedAtUtc.AddSeconds(-1);
        Assert.ThrowsAny<Exception>(()=>CompanyMaintenanceQualifiedAssetReturn.Resolve(ready.Sources,f.Order,observations,mode=="role"?"OtherRole":"FacilityRole",at));
        Assert.Single(f.Native.Writes); // Only the earlier explicit policy append.
    }

    [Theory][InlineData("idle")][InlineData("isolation")][InlineData("equipment-read")][InlineData("facility-read")][InlineData("status")][InlineData("licence")][InlineData("source")][InlineData("policy")][InlineData("utc")][InlineData("utc-clock")]
    public async Task QualifiedReturnPreparationRefusesChangedSourcesRightsOrUnownedTransactionsBeforeAnyStatusWrite(string mode)
    {
        var ready=await ReadyQualifiedReturn(mode=="utc-clock"?"unspecified":"current");using var f=ready.Fixture;var writes=f.Native.Writes.Count;var at=DateTime.UtcNow;
        if(mode=="isolation")f.Native.TransactionIsolation=IsolationLevel.ReadCommitted;
        if(mode=="equipment-read")await f.Native.Grant("EQUIPMENT","pump","READ");if(mode=="facility-read")await f.Native.Grant("FACILITY","plant","READ",facilityType:"gas");
        if(mode=="status")f.Native.Rows.OfType<EQUIPMENT_STATUS>().Single().PERCENT_CAPABILITY=99m;
        if(mode=="licence")f.Native.Rows.OfType<FACILITY_LICENSE>().Single().FEES_PAID_IND="N";
        if(mode=="source")f.Native.Rows.OfType<R_SOURCE>().Single(row=>row.SOURCE=="recorded-policy-source").ACTIVE_IND="N";
        if(mode=="policy")f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>().Single().POLICY_JSON="{}";
        if(mode=="utc")f.Native.Rows.OfType<R_TIMEZONE>().Single(row=>row.TIMEZONE=="UTC").ACTIVE_IND="N";
        if(mode=="utc-clock")f.Native.Rows.OfType<R_TIMEZONE>().Single(row=>row.TIMEZONE=="UTC").EFFECTIVE_DATE=DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1),DateTimeKind.Unspecified);
        Task Prepare()=>f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole","original-return",at,"Explicit observed return",f.Guard);
        if(mode=="idle")await Assert.ThrowsAnyAsync<Exception>(Prepare);else await Assert.ThrowsAnyAsync<Exception>(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,Prepare));
        Assert.Equal(writes,f.Native.Writes.Count);Assert.DoesNotContain(f.Native.Writes,row=>row is FACILITY_STATUS or EQUIPMENT_STATUS);
    }

    [Theory][InlineData("FACILITY_STATUS")][InlineData("EQUIPMENT_STATUS")]
    public async Task QualifiedReturnNativeCapabilityDbNullRemainsUnknownThroughCanonicalRepository(string table)
    {
        using var f=await ReadyPolicy();var row=f.Native.Rows.Single(row=>row.GetType().Name==table);var type=row.GetType();
        Assert.Equal(typeof(decimal?),type.GetProperty("PERCENT_CAPABILITY")!.PropertyType);
        var values=type.GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(property=>property.CanRead && property.CanWrite).ToDictionary(property=>property.Name,property=>property.GetValue(row)??DBNull.Value);values["PERCENT_CAPABILITY"]=DBNull.Value;
        f.Native.Source.Setup(value=>value.GetEntityAsync(table,It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{values});
        var repo=new PPDMGenericRepository(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,type,f.Context.Database.ConnectionName,table,null,null,null,["PERCENT_CAPABILITY"]);
        var current=Assert.Single(await repo.GetAsync([]));Assert.Null(type.GetProperty("PERCENT_CAPABILITY")!.GetValue(current));
        values.Remove("PERCENT_CAPABILITY");await Assert.ThrowsAsync<InvalidOperationException>(()=>repo.GetAsync([]));Assert.Empty(f.Native.Writes);
    }
}
