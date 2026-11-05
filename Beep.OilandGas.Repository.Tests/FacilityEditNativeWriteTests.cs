using System.Text.Json;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed class FacilityEditNativeWriteTests
{
    [Theory][InlineData("valid")][InlineData("partner")][InlineData("lost-ack")][InlineData("first-write")][InlineData("second-write")][InlineData("rollback-fail")][InlineData("authority")][InlineData("stale")][InlineData("begin")]
    public async Task ActualOwnerUsesOneTransactionAndDoesNotLeavePartialFacilityOrIntent(string mode)
    {
        using var f=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();
        var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");grant.AccessLevel="WRITE";await f.Native.Db.SaveChangesAsync();
        var row=f.Native.Rows.OfType<FACILITY>().Single(value=>value.FACILITY_TYPE=="gas");
        if(mode=="partner"){row.CURRENT_OPERATOR="partner";await f.Native.Grant("FIELD","A","WRITE",inherit:true);}
        var review=new DevelopmentFacilityEditReview(f.Context,CompanyInventorySourceReview.Copy(row),"Editor",DevelopmentFacilityEditReview.SourceDigest(row),new string('A',64),"",DateTimeOffset.UtcNow);
        review=review with{Revision=review.ContentRevision()};var command=new DevelopmentFacilityEdit(review,Guid.NewGuid(),"Updated facility","Updated description","Correct the recorded description",true);
        f.Native.Metadata.Setup(m=>m.GetTableMetadataAsync("FACILITY_EDIT_INTENT")).ReturnsAsync(new PPDMTableMetadata{TableName="FACILITY_EDIT_INTENT",EntityTypeName=nameof(FACILITY_EDIT_INTENT),PrimaryKeyColumn="FACILITY_EDIT_INTENT_ID"});
        f.Native.Metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("FACILITY_EDIT_INTENT")).ReturnsAsync(["FACILITY_EDIT_INTENT_ID"]);
        var creator=new ClassCreator(f.Native.Editor.Object);f.Native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns(true);
        f.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>
        {
            var type=requested.EntityName=="FACILITY_EDIT_INTENT"?typeof(FACILITY_EDIT_INTENT):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;
            var shape=creator.ConvertToEntityStructure(type);var keys=requested.EntityName=="FACILITY_EDIT_INTENT"?new[]{"FACILITY_EDIT_INTENT_ID"}:f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult().ToArray();
            foreach(var column in shape.Fields)column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal);return shape;
        });
        var properties=f.Native.Source.Object.Dataconnection.ConnectionProp;var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(properties);
        connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.Native.LeasePhysicalConnection.Object);f.Native.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);
        f.Native.EnableCorrectiveActionTransactions();if(mode=="first-write")f.Native.LeaseFailWriteNumber=1;if(mode is "second-write" or "rollback-fail")f.Native.LeaseFailWriteNumber=2;
        if(mode=="rollback-fail")f.Native.LeaseRollbackFailed=true;
        if(mode=="authority")f.Native.AfterLeaseWrite=_=>f.Revoked=true;if(mode=="stale")row.REMARK="changed before command";if(mode=="begin")f.Native.LeaseBeginNoOp=true;
        if(mode=="lost-ack"){f.Native.LeaseCommitFailed=true;f.Native.LeaseCommitAfterFailure=true;}
        var owner=new FacilityManagementService(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),f.Context.Database.ConnectionName,
            resolveModuleConnection:_=>Task.FromResult(f.Context.Database.ConnectionName),maintenanceAssetAccess:f.Native.Policy,maintenanceEvidenceZone:"Asia/Kuwait");
        var initial=JsonSerializer.Serialize(row);var untouched=JsonSerializer.Serialize(f.Native.Rows.Where(value=>value is not FACILITY_EDIT_INTENT && (value is not FACILITY facility || facility.FACILITY_TYPE!="gas")));
        if(mode is "valid" or "partner")
        {
            var saved=await owner.RecordFacilityEditAsync(command,f.Guard);var restored=saved.Restore(f.Context.Company,f.Context.ActorUserId);
            Assert.Equal(command.Revision(),restored.Command.Revision());Assert.Equal("Updated facility",restored.Result.FACILITY_LONG_NAME);Assert.Single(f.Native.Rows.OfType<FACILITY_EDIT_INTENT>());
            Assert.Equal("Updated facility",f.Native.Rows.OfType<FACILITY>().Single(value=>value.FACILITY_TYPE=="gas").FACILITY_LONG_NAME);Assert.Contains("commit",f.Native.LeaseTransactions);
            Assert.Equal(2,f.Native.Writes.Count);Assert.Equal(untouched,JsonSerializer.Serialize(f.Native.Rows.Where(value=>value is not FACILITY_EDIT_INTENT && (value is not FACILITY facility || facility.FACILITY_TYPE!="gas"))));
            var retry=await owner.RecordFacilityEditAsync(command,f.Guard);Assert.Equal(saved.RECORDED_REVISION,retry.RECORDED_REVISION);Assert.Equal(2,f.Native.Writes.Count);
            await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordFacilityEditAsync(command with{Reason="A different reason"},f.Guard));Assert.Equal(2,f.Native.Writes.Count);
            grant.AccessLevel="READ";await f.Native.Db.SaveChangesAsync();await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordFacilityEditAsync(command,f.Guard));Assert.Equal(2,f.Native.Writes.Count);
        }
        else if(mode=="lost-ack")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordFacilityEditAsync(command,f.Guard));Assert.Single(f.Native.Rows.OfType<FACILITY_EDIT_INTENT>());
            var writes=f.Native.Writes.Count;f.Native.LeaseCommitFailed=false;grant.AccessLevel="READ";await f.Native.Db.SaveChangesAsync();
            var recovered=await owner.RecoverFacilityEditAsync(command,f.Guard);Assert.Equal(command.Revision(),recovered.Restore(f.Context.Company,f.Context.ActorUserId).Command.Revision());Assert.Equal(writes,f.Native.Writes.Count);
            await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecoverFacilityEditAsync(command with{IntentId=Guid.NewGuid()},f.Guard));Assert.Equal(writes,f.Native.Writes.Count);
            await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecoverFacilityEditAsync(command with{Reason="Different retained reason"},f.Guard));Assert.Equal(writes,f.Native.Writes.Count);
            f.Native.Rows.OfType<FACILITY>().Single(value=>value.FACILITY_TYPE=="gas").REMARK="Subsequent source change";
            var historical=await owner.RecoverFacilityEditAsync(command,f.Guard);Assert.Equal("Updated facility",historical.Restore(f.Context.Company,f.Context.ActorUserId).Result.FACILITY_LONG_NAME);Assert.Equal(writes,f.Native.Writes.Count);
        }
        else if(mode=="rollback-fail")
        {
            await Assert.ThrowsAsync<AggregateException>(()=>owner.RecordFacilityEditAsync(command,f.Guard));Assert.Contains("rollback",f.Native.LeaseTransactions);Assert.NotNull(f.Native.LeaseActiveTransaction);
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordFacilityEditAsync(command,f.Guard));Assert.Empty(f.Native.Rows.OfType<FACILITY_EDIT_INTENT>());
            Assert.Equal(initial,JsonSerializer.Serialize(f.Native.Rows.OfType<FACILITY>().Single(value=>value.FACILITY_TYPE=="gas")));
            if(mode is "first-write" or "second-write" or "authority")Assert.Contains("rollback",f.Native.LeaseTransactions);
        }
    }
}
