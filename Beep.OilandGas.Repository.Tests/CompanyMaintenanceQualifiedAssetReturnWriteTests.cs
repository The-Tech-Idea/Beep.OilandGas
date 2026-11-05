using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.DataBase;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CompanyMaintenanceQualifiedLicensingTests
{
    [Theory][InlineData("raw-audit")][InlineData("extra-status")][InlineData("licensing")]
    [InlineData("zone-audit")][InlineData("capacity")][InlineData("policy")]
    public async Task PreparedQualifiedApplyRechecksCompleteCurrentInputsAndSourcesBeforeAnyMutation(string change)
    {
        var ready=await ReadyQualifiedReturn();using var f=ready.Fixture;var writes=f.Native.Writes.Count;var at=DateTime.UtcNow;var injected=false;
        await Assert.ThrowsAnyAsync<Exception>(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var rows=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole","current-input-return",at,"Original observed return",f.Guard);
            if(change=="raw-audit")f.Native.Rows.OfType<FACILITY_STATUS>().Single().ROW_CHANGED_BY="external-change";
            if(change=="extra-status"){var status=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_STATUS>().Single());status.STATUS_ID="external-inactive-status";status.ACTIVE_IND="N";f.Native.Rows.Add(status);}
            if(change=="licensing")f.Native.Rows.OfType<FACILITY_LICENSE>().Single().FEES_PAID_IND="N";
            if(change=="zone-audit")f.Native.Rows.OfType<R_TIMEZONE>().Single(row=>row.TIMEZONE=="UTC").ROW_CHANGED_BY="external-change";
            if(change=="policy")f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>().Single().POLICY_JSON="{}";
            if(change=="capacity")
            {
                var shape=f.Native.Source.Object.GetEntityStructure(new EntityStructure{EntityName="FACILITY_STATUS",DatasourceEntityName="FACILITY_STATUS"},true);
                shape.Fields.Single(field=>field.FieldName=="STATUS_ID").Size1=1;
                f.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName=="FACILITY_STATUS"),true)).Returns(shape);
            }
            injected=true;
            await f.Owner.ApplyQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole","current-input-return",at,"Original observed return",rows,f.Guard);
        }));
        Assert.True(injected);Assert.Equal(writes,f.Native.Writes.Count);Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
    }
    // This is the native participant/owned-provider fixture, not proof of the final mixed ledger/closure API.
    [Theory][InlineData("current")][InlineData("facility-only")][InlineData("equipment-only")][InlineData("preserve")][InlineData("zero")][InlineData("unspecified")]
    public async Task QualifiedReturnWriteJoinsOneOwnedTransactionAndPersistsOnlyItsExactPreparedNativeGraph(string mode)
    {
        var ready=await ReadyQualifiedReturn(mode);using var f=ready.Fixture;var writes=f.Native.Writes.Count;
        var observations=mode=="facility-only"?new[]{QualifiedObservations()[0]}:mode=="equipment-only"?new[]{QualifiedObservations()[1]}:QualifiedObservations();
        var at=DateTime.UtcNow;var preserved=JsonSerializer.Serialize(f.Native.Rows.Where(row=>row is not FACILITY_STATUS and not EQUIPMENT_STATUS));
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var rows=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,observations,"FacilityRole","owned-qualified-return",at,"Explicit native observations",f.Guard);
            await f.Owner.ApplyQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,observations,"FacilityRole","owned-qualified-return",at,"Explicit native observations",rows,f.Guard);
            Assert.Equal(rows.AfterFacilities.Length,f.Native.Rows.OfType<FACILITY_STATUS>().Count(row=>row.FACILITY_ID=="plant" && row.FACILITY_TYPE=="gas"));
            Assert.Equal(rows.AfterEquipment.Length,f.Native.Rows.OfType<EQUIPMENT_STATUS>().Count(row=>row.EQUIPMENT_ID=="pump" && mode!="facility-only"));
            if(mode=="equipment-only")Assert.Equal("INACTIVE",f.Native.Rows.OfType<FACILITY_STATUS>().Single(row=>row.STATUS_ID=="facility-status").STATUS);
            foreach(var change in CompanyMaintenanceQualifiedAssetReturn.Resolve(ready.Sources,f.Order,observations,"FacilityRole",at))
            {
                object stored=change.AssetType=="FACILITY"?f.Native.Rows.OfType<FACILITY_STATUS>().Single(row=>row.STATUS_ID==change.StatusId("owned-qualified-return")):
                    f.Native.Rows.OfType<EQUIPMENT_STATUS>().Single(row=>row.EQUIP_STATUS_ID==change.StatusId("owned-qualified-return"));
                Assert.Equal(change.ObservedCapabilityPercent,stored.GetType().GetProperty("PERCENT_CAPABILITY")!.GetValue(stored));
                Assert.Equal(rows.RecordedAtUtc,stored.GetType().GetProperty("ROW_CREATED_DATE")!.GetValue(stored));
                Assert.Equal("actor",stored.GetType().GetProperty("ROW_CHANGED_BY")!.GetValue(stored));
            }
        });
        var targets=CompanyMaintenanceQualifiedAssetReturn.Resolve(ready.Sources,f.Order,observations,"FacilityRole",at);
        Assert.Equal(writes+targets.Count*2,f.Native.Writes.Count);Assert.Equal(preserved,JsonSerializer.Serialize(f.Native.Rows.Where(row=>row is not FACILITY_STATUS and not EQUIPMENT_STATUS)));
        Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>()); // The participant never creates another ledger/outcome.
    }

    [Theory][InlineData("first-update")][InlineData("last-insert")][InlineData("readback")][InlineData("revoked-after-write")][InlineData("final-command-failure")]
    public async Task QualifiedReturnWriteFailureRollsBackAllEarlierNativeTargetsWithTheOwningCommand(string mode)
    {
        var ready=await ReadyQualifiedReturn();using var f=ready.Fixture;var before=JsonSerializer.Serialize(f.Native.Rows);var writes=f.Native.Writes.Count;
        if(mode=="first-update")f.Native.LeaseFailWriteNumber=writes+1;if(mode=="last-insert")f.Native.LeaseFailWriteNumber=writes+4;
        f.Native.AfterLeaseWrite=row=>
        {
            if(mode=="revoked-after-write")f.Revoked=true;
            if(mode=="readback" && row is EQUIPMENT_STATUS equipment && equipment.ACTIVE_IND=="Y")equipment.PERCENT_CAPABILITY=99m;
        };
        var at=DateTime.UtcNow;
        await Assert.ThrowsAnyAsync<Exception>(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var rows=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole","rollback-return",at,"Observed native return",f.Guard);
            await f.Owner.ApplyQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole","rollback-return",at,"Observed native return",rows,f.Guard);
            if(mode=="final-command-failure")throw new InvalidOperationException("The complete command's final intent/verification failed.");
        }));
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
    }
    [Theory][InlineData("FACILITY","grant")][InlineData("EQUIPMENT","grant")]
    [InlineData("FACILITY","owner")][InlineData("EQUIPMENT","owner")][InlineData("EQUIPMENT","installation")]
    public async Task QualifiedMutationRechecksDirectWriteAfterItsPinnedPreimageWasRead(string asset,string change)
    {
        var ready=await ReadyQualifiedReturn(asset=="FACILITY"?"facility-only":"equipment-only");using var f=ready.Fixture;
        var observations=asset=="FACILITY"?new[]{QualifiedObservations()[0]}:new[]{QualifiedObservations()[1]};
        bool ExternalChange(object row)=>change=="owner" && (asset=="FACILITY" && row is FACILITY{FACILITY_ID:"plant",FACILITY_TYPE:"gas"} ||
            asset=="EQUIPMENT" && row is EQUIPMENT{EQUIPMENT_ID:"pump"}) || change=="installation" && row is FACILITY_EQUIPMENT{EQUIPMENT_ID:"pump"};
        IEnumerable<object> UnchangedRows()=>f.Native.Rows.Where(row=>!ExternalChange(row));
        var before=JsonSerializer.Serialize(UnchangedRows());var writes=f.Native.Writes.Count;var changed=false;var at=DateTime.UtcNow;
        f.Native.AfterRead=async()=>
        {
            var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");
            if(changed || call.Arguments[0] as string!=(asset=="FACILITY"?"FACILITY_STATUS":"EQUIPMENT_STATUS") ||
                call.Arguments[1] is not List<TheTechIdea.Beep.Report.AppFilter> filters ||
                !filters.Any(filter=>filter.FieldName==(asset=="FACILITY"?"STATUS_ID":"EQUIP_STATUS_ID")))return;
            changed=true;
            if(change=="grant"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType==asset);grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}
            if(change=="owner" && asset=="FACILITY")f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_ID=="plant" && row.FACILITY_TYPE=="gas").CURRENT_OPERATOR="other-company";
            if(change=="owner" && asset=="EQUIPMENT")f.Native.Rows.OfType<EQUIPMENT>().Single(row=>row.EQUIPMENT_ID=="pump").CURRENT_OWNER_BA_ID="other-company";
            if(change=="installation")f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;
        };
        await Assert.ThrowsAnyAsync<Exception>(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var rows=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,observations,"FacilityRole","preimage-return",at,"Original native observations",f.Guard);
            await f.Owner.ApplyQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,observations,"FacilityRole","preimage-return",at,"Original native observations",rows,f.Guard);
        }));
        Assert.True(changed);Assert.Equal(writes,f.Native.Writes.Count);Assert.Equal(before,JsonSerializer.Serialize(UnchangedRows()));
        // An independently injected source/authorization change is not a mutation to undo for this refused command.
        if(change=="grant")Assert.False((await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType==asset)).IsActive);
        if(change=="owner" && asset=="FACILITY")Assert.Equal("other-company",f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_ID=="plant" && row.FACILITY_TYPE=="gas").CURRENT_OPERATOR);
        if(change=="owner" && asset=="EQUIPMENT")Assert.Equal("other-company",f.Native.Rows.OfType<EQUIPMENT>().Single(row=>row.EQUIPMENT_ID=="pump").CURRENT_OWNER_BA_ID);
        if(change=="installation")Assert.Equal(8,f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO);
        Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
    }

    [Theory][InlineData("grant")][InlineData("owner")][InlineData("installation")]
    public async Task QualifiedPostimageReadClosesTargetAuthorityBeforeTheOwningTransactionCanCommit(string change)
    {
        var ready=await ReadyQualifiedReturn();using var f=ready.Fixture;var at=DateTime.UtcNow;var changed=false;
        bool External(object row)=>change=="owner" && row is EQUIPMENT{EQUIPMENT_ID:"pump"} || change=="installation" && row is FACILITY_EQUIPMENT;
        var before=JsonSerializer.Serialize(f.Native.Rows.Where(row=>!External(row)));var writes=f.Native.Writes.Count;string? externalAfter=null;
        f.Native.AfterRead=async()=>
        {
            var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");
            if(changed || f.Native.Writes.Count<writes+4 || call.Arguments[0] as string!="FACILITY_STATUS")return;
            if(change=="grant"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(value=>value.AssetType=="EQUIPMENT");grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}
            if(change=="owner")f.Native.Rows.OfType<EQUIPMENT>().Single(value=>value.EQUIPMENT_ID=="pump").CURRENT_OWNER_BA_ID="other-company";
            if(change=="installation")f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;
            externalAfter=JsonSerializer.Serialize(f.Native.Rows.Where(External));
            changed=true;
        };
        await Assert.ThrowsAnyAsync<Exception>(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var prepared=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole","postimage-return",at,"Original observations",f.Guard);
            await f.Owner.ApplyQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole","postimage-return",at,"Original observations",prepared,f.Guard);
        }));
        Assert.True(changed);Assert.Equal(writes+4,f.Native.Writes.Count);
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows.Where(row=>!External(row))));Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
        Assert.Equal(externalAfter,JsonSerializer.Serialize(f.Native.Rows.Where(External)));
        if(change=="grant")Assert.False((await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(value=>value.AssetType=="EQUIPMENT")).IsActive);
    }

    [Theory][InlineData("prepared-target")][InlineData("prepared-preimage")][InlineData("source-rights")][InlineData("reason")][InlineData("return-id")]
    public async Task QualifiedReturnWriteCannotAcceptForgedPreparedRowsOrAChangedOriginalCommand(string mode)
    {
        var ready=await ReadyQualifiedReturn();using var f=ready.Fixture;var before=JsonSerializer.Serialize(f.Native.Rows);var writes=f.Native.Writes.Count;var at=DateTime.UtcNow;
        await Assert.ThrowsAnyAsync<Exception>(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var rows=await f.Owner.PrepareQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole","original-return",at,"Original observed reason",f.Guard);
            if(mode=="prepared-target")rows.AfterEquipment.Single(row=>row.ACTIVE_IND=="Y").PERCENT_CAPABILITY=99m;
            if(mode=="prepared-preimage")rows.BeforeFacilities[0].PERCENT_CAPABILITY=0m;
            if(mode=="source-rights")await f.Native.Grant("EQUIPMENT","pump","READ");
            await f.Owner.ApplyQualifiedAssetReturnRowsAsync(ready.Sources,f.Order,QualifiedObservations(),"FacilityRole",mode=="return-id"?"replacement-return":"original-return",at,
                mode=="reason"?"Changed reason":"Original observed reason",rows,f.Guard);
        }));
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));Assert.Equal(writes,f.Native.Writes.Count);
    }
}
