using System.Text.Json;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using ReviewFixture=Beep.OilandGas.Repository.Tests.CompanyMaintenanceQualifiedAssetTests.ReviewFixture;
namespace Beep.OilandGas.Repository.Tests;
public sealed class CompanyMaintenanceQualifiedOperatingTests
{
    [Fact]
    public async Task PartnerFacilityStatusUsesDirectAccessEvenWhenFieldGrantIsStronger()
    {
        using var f=await Ready();f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_TYPE=="gas").CURRENT_OPERATOR="partner";await f.Native.Grant("FIELD","A","WRITE",inherit:true);
        var identity=await f.Read();Assert.Equal("READ",Assert.Single(identity.Facilities).AccessLevel);var operating=await f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,identity,f.Guard);operating.Require(identity);Assert.Contains(operating.Statuses,row=>row.AssetType=="FACILITY");Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task CurrentStatusOwnerReadsOnlyNativeIdentityAndOperatingEvidenceWithoutPoliciesOrLicences()
    {
        using var f=await Ready();var current=await f.Owner.ReadMaintenanceQualifiedCurrentStatusAsync(f.Context,f.Order,f.Guard);current.RequireCurrentStatus(new(CompanyMaintenanceProcessStart.InstanceIdFor(f.Context.Company,Guid.NewGuid()),f.Order.FieldId,f.Order.InstanceId),f.Order.Components);
        Assert.NotNull(current.Operating);Assert.Null(current.Licensing);Assert.Null(current.ReturnPolicy);Assert.Equal("READ",Assert.Single(current.Equipment).AccessLevel);Assert.Empty(f.Native.Writes);
        Assert.DoesNotContain(f.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] is string table && (table.Contains("LICEN",StringComparison.Ordinal) || table=="WORK_ORDER_MAINTENANCE_RETURN_POLICY"));
    }
    [Theory][InlineData("source")][InlineData("component")]
    public async Task CurrentStatusOwnerRejectsSourceOrOrderChangeAtItsClosingAuthorityBoundary(string change)
    {
        using var f=await Ready();var total=0;
        await f.Owner.ReadMaintenanceQualifiedCurrentStatusAsync(f.Context,f.Order,()=>{total++;return f.Guard();});var observed=0;
        Task Guard()
        {
            if(++observed==total)
            {
                if(change=="source")f.Native.Editor.Setup(editor=>editor.GetDataSource(f.Context.Database.ConnectionName)).Returns(Mock.Of<TheTechIdea.Beep.IDataSource>());
                else f.Order.Components[0]=f.Order.Components[0] with{NativeRevision=new string('F',64)};
            }
            return f.Guard();
        }
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceQualifiedCurrentStatusAsync(f.Context,f.Order,Guard));Assert.Equal(total,observed);Assert.Empty(f.Native.Writes);
    }
    private static async Task<ReviewFixture> Ready()
    {
        var f=await ReviewFixture.Create();var at=DateTime.UtcNow.AddDays(-1);
        f.Native.Rows.Add(new FACILITY_STATUS{FACILITY_ID="plant",FACILITY_TYPE="gas",STATUS_ID="facility-condition",STATUS_TYPE="NativeFacilityType",STATUS="INACTIVE",SOURCE="native-facility",ACTIVE_IND="Y",EFFECTIVE_DATE=at,START_TIME=at,TIMEZONE="UTC",REMARK="Private native source remark"});
        f.Native.Rows.Add(new EQUIPMENT_STATUS{EQUIPMENT_ID="pump",EQUIP_STATUS_ID="equipment-condition",EQUIP_STATUS_TYPE="NativeEquipmentType",EQUIP_STATUS="IDLE",SOURCE="native-equipment",ACTIVE_IND="Y",EFFECTIVE_DATE=at,START_TIME=at,TIMEZONE="UTC"});
        f.Native.Rows.Add(new R_FAC_STATUS_TYPE{STATUS_TYPE="NativeFacilityType",ACTIVE_IND="Y",EFFECTIVE_DATE=at.AddDays(-10)});f.Native.Rows.Add(new R_EQUIP_STATUS_TYPE{STATUS_TYPE="NativeEquipmentType",ACTIVE_IND="Y",EFFECTIVE_DATE=at.AddDays(-10)});
        f.Native.Rows.Add(new R_FACILITY_STATUS{STATUS_TYPE="NativeFacilityType",STATUS="INACTIVE",ACTIVE_IND="Y",EFFECTIVE_DATE=at.AddDays(-10)});f.Native.Rows.Add(new R_EQUIP_STATUS{STATUS_TYPE="NativeEquipmentType",EQUIP_STATUS="IDLE",ACTIVE_IND="Y",EFFECTIVE_DATE=at.AddDays(-10)});f.Native.Rows.Add(new R_TIMEZONE{TIMEZONE="UTC",ACTIVE_IND="Y",EFFECTIVE_DATE=at.AddDays(-10)});return f;
    }
    private static int StatusReads(ReviewFixture f)=>f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string is "FACILITY_STATUS" or "EQUIPMENT_STATUS");
    [Fact]
    public async Task ActualNativeStatusReaderPreservesRecordedCodesReferencesAndUtcWithoutInferringOperationalMeaning()
    {
        using var f=await Ready();var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);Assert.Equal(2,result.Statuses.Count);Assert.Equal("INACTIVE",Assert.Single(result.Statuses.Where(row=>row.AssetType=="FACILITY")).Value);Assert.Equal("IDLE",Assert.Single(result.Statuses.Where(row=>row.AssetType=="EQUIPMENT")).Value);Assert.All(result.Statuses,row=>Assert.True(row.Covers(result.ComparedAtUtc)));Assert.Equal(2,result.References.Count);Assert.All(result.References,row=>Assert.True(row.Covers(result.ComparedAtUtc)));Assert.Contains("no assumed operational meaning",Assert.Single(result.Blockers));
        var combined=identity with{Operating=result,ComparedAtUtc=result.ComparedAtUtc};combined.Require(f.Order);Assert.NotEqual(identity.SourceRevision(),combined.SourceRevision());var body=JsonSerializer.Serialize(result);Assert.DoesNotContain("Private native source remark",body);Assert.DoesNotContain("PERCENT_CAPABILITY",body);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("reference")][InlineData("type")][InlineData("zone")][InlineData("missing-status")][InlineData("overlap")][InlineData("future")][InlineData("invalid-period")][InlineData("ambiguous-zone-date")][InlineData("inactive-record")]
    public async Task UnresolvedNativeStatusEvidenceRemainsExplicitWithoutSelectingALatestOrDefault(string fault)
    {
        using var f=await Ready();var row=f.Native.Rows.OfType<FACILITY_STATUS>().Single();
        if(fault=="reference")f.Native.Rows.RemoveAll(value=>value is R_FACILITY_STATUS);if(fault=="type")f.Native.Rows.RemoveAll(value=>value is R_FAC_STATUS_TYPE);if(fault=="zone")f.Native.Rows.RemoveAll(value=>value is R_TIMEZONE);if(fault=="missing-status")f.Native.Rows.Remove(row);
        if(fault=="overlap"){var other=CompanyInventorySourceReview.Copy(row);other.STATUS_ID="another-current";f.Native.Rows.Add(other);}if(fault=="future")row.EFFECTIVE_DATE=DateTime.UtcNow.AddDays(1);if(fault=="invalid-period")row.END_TIME=row.EFFECTIVE_DATE!.Value.AddDays(-1);if(fault=="inactive-record")row.ACTIVE_IND="N";
        if(fault=="ambiguous-zone-date"){row.TIMEZONE="America/New_York";row.EFFECTIVE_DATE=new DateTime(2026,11,1,1,30,0,DateTimeKind.Unspecified);f.Native.Rows.Add(new R_TIMEZONE{TIMEZONE=row.TIMEZONE,ACTIVE_IND="Y"});}
        var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);Assert.True(result.Blockers.Count>1);Assert.Contains(result.Blockers,value=>value.Contains("FACILITY plant gas",StringComparison.Ordinal));if(fault=="overlap")Assert.Equal(2,result.Statuses.Count(value=>value.AssetType=="FACILITY" && value.Covers(result.ComparedAtUtc)));if(fault=="ambiguous-zone-date")Assert.False(result.Statuses.Single(value=>value.AssetType=="FACILITY").TemporalEvidence);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("owner")][InlineData("grant")][InlineData("installation")]
    public async Task OriginalQualifiedParentsMustStillBeAuthorizedBeforeAnyOperatingStatusRead(string fault)
    {
        using var f=await Ready();var identity=await f.Read();if(fault=="owner")f.Native.Rows.OfType<EQUIPMENT>().Single().CURRENT_OWNER_BA_ID="other";if(fault=="installation")f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;if(fault=="grant"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}var before=StatusReads(f);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,identity,f.Guard));Assert.Equal(before,StatusReads(f));Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("foreign")][InlineData("duplicate")]
    public async Task NativeStatusPredicatesAndCompoundMembershipMustAgree(string fault)
    {
        using var f=await Ready();var identity=await f.Read();var row=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_STATUS>().Single());if(fault=="foreign"){row.FACILITY_TYPE="oil";f.Native.Source.Setup(source=>source.GetEntityAsync("FACILITY_STATUS",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{row});}else f.Native.Rows.Add(row);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,identity,f.Guard));Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task PairedOperatingReadRejectsAChangedRecordedValueWithoutAnotherBusinessWrite()
    {
        using var f=await Ready();var identity=await f.Read();var reads=0;f.Native.AfterRead=()=>{var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");if(call.Arguments[0] as string=="FACILITY_STATUS" && ++reads==2)f.Native.Rows.OfType<FACILITY_STATUS>().Single().STATUS="ACTIVE";return Task.CompletedTask;};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,identity,f.Guard));Assert.Equal(2,reads);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("owner")][InlineData("grant")][InlineData("installation")]
    public async Task ClosingOperatingParentCheckRejectsChangesAfterTheSecondStatusPopulation(string change)
    {
        using var f=await Ready();var identity=await f.Read();var reads=0;
        f.Native.AfterRead=async()=>
        {
            var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");
            if(call.Arguments[0] as string!="FACILITY_STATUS" || ++reads!=2)return;
            if(change=="owner")f.Native.Rows.OfType<EQUIPMENT>().Single().CURRENT_OWNER_BA_ID="other-company";
            if(change=="installation")f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;
            if(change=="grant"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}
        };
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,identity,f.Guard));
        Assert.Equal(2,reads);Assert.Empty(f.Native.Writes);
    }
}
