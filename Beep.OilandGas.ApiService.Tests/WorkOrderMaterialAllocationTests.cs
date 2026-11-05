#if WORKFLOW_INBOX_FOCUSED
extern alias actualOwners;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using System.Text.Json;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.ConfigUtil;
using Xunit;
using Detail=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderMaterialAllocationTests
{
    [Fact]
    public async Task CurrentAcceptedStockProofRequiresExplicitOwnershipAndRetainsOriginalAuditWithoutWriting()
    {
        var f=Fixture();var accepted=new WorkOrderMaterialAcceptanceFixture(requirementProof:true,inventoryDatabase:new(f.Native.Connection,"production-fingerprint"));
        f.Native.Rows.Clear();f.Native.Rows.Add(CompanyInventorySourceReview.Copy(accepted.Inventory));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(accepted.Allocation));
        var saved=accepted.Command.Prepare(accepted.Pending,DateTime.UtcNow);var facts=accepted.Command.Facts(saved);var readiness=accepted.Current(saved);var utc=DateTime.UtcNow;
        var current=new CompanyWorkOrderAcceptedMaterialReview(facts,readiness.Reservation,readiness.CurrentMaterial,utc,CompanyWorkOrderAcceptedMaterialReview.GetBlockers(accepted.Permit,facts,readiness.Reservation,readiness.CurrentMaterial,utc));
        var original=JsonSerializer.Serialize(accepted.Allocation);
        var retained=JsonSerializer.Serialize(current);var executor=current.Material.Source.Context with{ActorUserId="executor",Revision="current-execution-authority"};
        await f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask);
        await f.Inventory.RequireExecutionMaterialCurrentProofAsync(executor,accepted.Permit,current,()=>Task.CompletedTask);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Inventory.RequireExecutionMaterialCurrentProofAsync(executor,accepted.Permit,current,()=>Task.FromException(new UnauthorizedAccessException("Current executor revoked"))));
        var privateReads=f.Native.Source.Invocations.Count;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Inventory.RequireExecutionMaterialCurrentProofAsync(executor with{Company=new("tenant","foreign")},accepted.Permit,current,()=>Task.CompletedTask));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Inventory.RequireExecutionMaterialCurrentProofAsync(executor with{Database=new("foreign","binding")},accepted.Permit,current,()=>Task.CompletedTask));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Inventory.RequireExecutionMaterialCurrentProofAsync(executor with{FieldId="foreign"},accepted.Permit,current,()=>Task.CompletedTask));
        Assert.Equal(privateReads,f.Native.Source.Invocations.Count);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask,true));
        var separateCore=new PersonaTaskRoutingTests.Fixture();f.Native.Editor.Setup(value=>value.GetDataSource("separate-approval-core")).Returns(separateCore.Source.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask,true,"separate-approval-core"));
        await Beep.OilandGas.PPDM39.Core.PPDMDataSourceTransaction.ExecuteAsync(separateCore.Source.Object,async()=>
        {
            var reads=f.Native.Source.Invocations.Count;
            Assert.False(await f.Inventory.RequireAcceptedMaterialOwnedCoreProofAsync(accepted.Permit,current,()=>Task.CompletedTask,"separate-approval-core"));
            Assert.Equal(reads,f.Native.Source.Invocations.Count);
            Assert.False(await f.Inventory.RequireExecutionMaterialOwnedCoreProofAsync(executor,accepted.Permit,current,()=>Task.CompletedTask,"separate-approval-core"));
            Assert.Equal(reads,f.Native.Source.Invocations.Count);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask,true,"separate-approval-core"));
            Assert.Equal(0,separateCore.Writes);Assert.Equal(0,f.Native.Writes);
        });
        await Beep.OilandGas.PPDM39.Core.PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            await f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask,true);
            await f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask,true,"same-provider-core-alias");
            Assert.True(await f.Inventory.RequireAcceptedMaterialOwnedCoreProofAsync(accepted.Permit,current,()=>Task.CompletedTask,"same-provider-core-alias"));
            Assert.True(await f.Inventory.RequireExecutionMaterialOwnedCoreProofAsync(executor,accepted.Permit,current,()=>Task.CompletedTask,"same-provider-core-alias"));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask));
            f.Item.QUANTITY_ON_HAND=1m;
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask,true));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Inventory.RequireExecutionMaterialOwnedCoreProofAsync(executor,accepted.Permit,current,()=>Task.CompletedTask,"same-provider-core-alias"));
            var allocation=Assert.Single(f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>());var body=allocation.COMMAND_JSON;
            allocation.OWNER_TENANT_ID="foreign";allocation.COMMAND_JSON="{";
            await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Inventory.RequireAcceptedMaterialCurrentProofAsync(accepted.Permit,current,()=>Task.CompletedTask,true));
            allocation.OWNER_TENANT_ID=accepted.Permit.Context.Company.TenantId;allocation.COMMAND_JSON=body;
        });
        Assert.Equal(0,f.Native.Writes);Assert.Equal(original,JsonSerializer.Serialize(Assert.Single(f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>())));
        Assert.Equal(retained,JsonSerializer.Serialize(current));
    }
    private static CompanyWorkOrderMaterialSourceReview Source(PurchaseReceiptStockCommandTests.Fixture f)
    {
        var company=new CompanyScope("tenant","company");var context=new FieldSelectionReview(company,new("lifecycle","lifecycle-fingerprint"),"actor","authority","field");var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(company,Guid.NewGuid()),"field","order-الكويت");
        var stage=new CompanyMaintenanceAssignedStageReview(context,task,"step","execute","Configured duty","ConfiguredRole","receiver","history","approval",new string('A',64),new string('B',64));
        var order=new Detail{InstanceId=task.WorkOrderId,FieldId="field",Source="WORK_ORDER",State="",EvidenceUnavailableReason="No execution status",Components=[new(task.WorkOrderId,"component","field",null,null)]};var core=new AssetDatabaseScope("core","core-fingerprint");var work=new CompanyMaintenanceInspectionReview(stage,core,order,[],CompanyMaintenanceInspectionReview.Revision(stage,core,order,[]),DateTimeOffset.UtcNow);
        var query=new CompanyWorkOrderMaterialSourceQuery(task,"item",2m,"bbl");var inventory=CompanyInventorySourceReview.Create(company,"actor",f.Item,[]);return new(context with{Database=new(f.Native.Connection,"production-fingerprint")},query,work,inventory,CompanyWorkOrderMaterialSourceReview.GetBlockers(query,inventory));
    }
    private static PurchaseReceiptStockCommandTests.Fixture Fixture()
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();var metadata=(Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository)typeof(Beep.OilandGas.Accounting.Services.InventoryService).GetField("_metadata",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.GetValue(f.Inventory)!;Mock.Get(metadata).Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_MATERIAL_ALLOCATION")).ReturnsAsync(["ALLOCATION_ID"]);var creator=new ClassCreator(f.Native.Editor.Object);f.Native.Source.Setup(s=>s.GetEntityStructure(It.Is<EntityStructure>(q=>q.EntityName=="WORK_ORDER_MATERIAL_ALLOCATION"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_MATERIAL_ALLOCATION)));
        f.Native.Source.Setup(s=>s.InsertEntity("WORK_ORDER_MATERIAL_ALLOCATION",It.IsAny<object>())).Returns((string _,object row)=>{f.Native.Writes++;f.Native.Rows.Add(CompanyInventorySourceReview.Copy((WORK_ORDER_MATERIAL_ALLOCATION)row));return new ErrorsInfo{Flag=Errors.Ok};});Mock.Get(metadata).Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_MATERIAL_RELEASE")).ReturnsAsync(["RELEASE_ID"]);f.Native.Source.Setup(s=>s.GetEntityStructure(It.Is<EntityStructure>(q=>q.EntityName=="WORK_ORDER_MATERIAL_RELEASE"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_MATERIAL_RELEASE)));f.Native.Source.Setup(s=>s.InsertEntity("WORK_ORDER_MATERIAL_RELEASE",It.IsAny<object>())).Returns((string _,object row)=>{f.Native.Writes++;f.Native.Rows.Add(CompanyInventorySourceReview.Copy((WORK_ORDER_MATERIAL_RELEASE)row));return new ErrorsInfo{Flag=Errors.Ok};});return f;
    }
    [Theory][InlineData("record")][InlineData("recovery")][InlineData("competing")][InlineData("inventory")][InlineData("actor")][InlineData("forged")][InlineData("read-only-missing")]
    public async Task NativeReservationUsesCompletePopulationOwnedTransactionAndOriginalRecovery(string mode)
    {
        var f=Fixture();var source=Source(f);var review=await f.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,()=>Task.CompletedTask);review.Require();Assert.Equal(10m,review.AvailableQuantity);
        var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),review.ComparedAtUtc,review.ComparedAtUtc.AddHours(4),"Original reviewed work requirement");
        if(mode=="competing")
        {
            var competing=command with{IntentId=Guid.NewGuid()};f.Native.Rows.Add(competing.Prepare(review.ComparedAtUtc));
        }
        if(mode=="inventory")f.Item.QUANTITY_ON_HAND=9m;
        if(mode=="forged")command=command with{Reason="changed",Review=review with{AvailableQuantity=20m}};
        Task Guard()=>mode=="actor"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        var error=await Record.ExceptionAsync(async()=>await f.Inventory.RecordWorkOrderMaterialAllocationAsync(command,Guard,()=>Task.CompletedTask,mode=="read-only-missing"));
        if(mode is not ("record" or "recovery")){Assert.NotNull(error);Assert.Equal(0,f.Native.Writes);Assert.Equal(10m,source.Inventory.Item.QUANTITY_ON_HAND);return;}
        Assert.Null(error);Assert.Equal(1,f.Native.Writes);var row=Assert.Single(f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>());command.RequireRecorded(row);Assert.Equal(10m,f.Item.QUANTITY_ON_HAND);Assert.Empty(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());
        var before=JsonSerializer.Serialize(row);var recovered=await f.Inventory.RecordWorkOrderMaterialAllocationAsync(command,Guard,()=>Task.CompletedTask,true);Assert.Equal(before,JsonSerializer.Serialize(recovered));Assert.Equal(1,f.Native.Writes);
        var retry=await f.Inventory.RecordWorkOrderMaterialAllocationAsync(command,Guard,()=>Task.CompletedTask);Assert.Equal(before,JsonSerializer.Serialize(retry));Assert.Equal(1,f.Native.Writes);
        var fresh=await f.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,Guard);Assert.Equal(2m,fresh.ReservedQuantity);Assert.Equal(8m,fresh.AvailableQuantity);Assert.Single(fresh.Allocations);Assert.DoesNotContain("COMMAND_JSON",JsonSerializer.Serialize(fresh.Allocations));
    }
    [Theory][InlineData("quantity")][InlineData("audit")][InlineData("owner")][InlineData("body")]
    public void OriginalAllocationEvidenceCannotBeAdoptedAfterMutation(string mode)
    {
        var f=Fixture();var source=Source(f);var utc=DateTime.UtcNow;var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(4),"Original requirement");var row=command.Prepare(utc);
        if(mode=="quantity")row.QUANTITY=3m;if(mode=="audit")row.ROW_CHANGED_BY="other";if(mode=="owner")row.OWNER_COMPANY_BA_ID="other";if(mode=="body")row.COMMAND_JSON="{}";Assert.ThrowsAny<Exception>(()=>command.RequireRecorded(row));
    }
    [Theory][InlineData("write")][InlineData("readback")][InlineData("authority")][InlineData("unknown-commit")]
    public async Task FailedAllocationRollsBackAndUnknownCommitRecoversWithoutAnotherWrite(string mode)
    {
        var f=Fixture();var source=Source(f);var review=await f.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,()=>Task.CompletedTask);var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),review.ComparedAtUtc,review.ComparedAtUtc.AddHours(1),"Original requirement");var revoked=false;
        f.Native.Source.Setup(s=>s.InsertEntity("WORK_ORDER_MATERIAL_ALLOCATION",It.IsAny<object>())).Returns((string _,object value)=>
        {
            f.Native.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((WORK_ORDER_MATERIAL_ALLOCATION)value);if(mode=="readback")row.QUANTITY=8m;f.Native.Rows.Add(row);if(mode=="authority")revoked=true;return new ErrorsInfo{Flag=Errors.Ok};
        });
        Task Guard()=>revoked?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        if(mode=="unknown-commit")f.Native.FailCommit=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Inventory.RecordWorkOrderMaterialAllocationAsync(command,Guard,()=>Task.CompletedTask));Assert.Equal(10m,f.Item.QUANTITY_ON_HAND);Assert.Empty(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());
        if(mode!="unknown-commit"){Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>());Assert.Contains("rollback",f.Native.Transactions);return;}
        Assert.Single(f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>());f.Native.FailCommit=false;var before=f.Native.Writes;var rowRecovered=await f.Inventory.RecordWorkOrderMaterialAllocationAsync(command,Guard,()=>Task.CompletedTask,true);command.RequireRecorded(rowRecovered);Assert.Equal(before,f.Native.Writes);
    }
    [Fact] public void AllocationUsesCanonicalExplicitNonnullableBeepMetadata()
    {
        Assert.Equal(typeof(CompanyScope).Assembly,typeof(WORK_ORDER_MATERIAL_ALLOCATION).Assembly);var f=Fixture();var shape=new ClassCreator(f.Native.Editor.Object).ConvertToEntityStructure(typeof(WORK_ORDER_MATERIAL_ALLOCATION));var key=Assert.Single(shape.Fields,p=>p.IsKey);Assert.Equal("ALLOCATION_ID",key.FieldName);Assert.False(key.AllowDBNull);
        foreach(var name in new[]{"COMMAND_JSON","OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","INVENTORY_ITEM_ID","QUANTITY","VALID_FROM_UTC","VALID_TO_UTC"})Assert.False(Assert.Single(shape.Fields,p=>p.FieldName==name).AllowDBNull);
        Assert.Equal(2097152,Assert.Single(shape.Fields,p=>p.FieldName=="COMMAND_JSON").Size1);
    }
    [Theory][InlineData("original")][InlineData("foreign-owner")][InlineData("foreign-actor")][InlineData("foreign-task")][InlineData("body")][InlineData("late-authority")]
    public async Task PrivateReferenceRecoveryChecksPublicOwnerBeforeBodyAndNeverWrites(string mode)
    {
        var f=Fixture();var source=Source(f);var review=await f.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,()=>Task.CompletedTask);var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),review.ComparedAtUtc,review.ComparedAtUtc.AddHours(1),"Original requirement");await f.Inventory.RecordWorkOrderMaterialAllocationAsync(command,()=>Task.CompletedTask,()=>Task.CompletedTask);var row=f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>().Single();
        if(mode=="foreign-owner"){row.OWNER_COMPANY_BA_ID="other";row.COMMAND_JSON="malformed private body";}if(mode=="foreign-actor"){row.ROW_CREATED_BY="other";row.COMMAND_JSON="malformed private body";}if(mode=="foreign-task"){row.WORK_ORDER_ID="other";row.COMMAND_JSON="malformed private body";}if(mode=="body")row.COMMAND_JSON="{}";
        var guards=0;Task Guard()=>mode=="late-authority" && ++guards>2?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        var query=new CompanyWorkOrderMaterialAllocationReferenceQuery(source.Query.Task,command.IntentId);var error=await Record.ExceptionAsync(async()=>{var result=await f.Inventory.RecoverWorkOrderMaterialAllocationReferenceAsync(source.Context.Company,"actor",query,Guard);result.Require(source.Context.Company,"actor",query);Assert.Equal(command.IntentId,result.Command.IntentId);});
        if(mode=="original")Assert.Null(error);else if(mode is "foreign-owner" or "foreign-actor" or "foreign-task" or "late-authority")Assert.IsType<UnauthorizedAccessException>(error);else Assert.NotNull(error);Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData("original")][InlineData("write")][InlineData("readback")][InlineData("authority")][InlineData("original-drift")][InlineData("unknown-commit")]
    public async Task FullReleasePreservesOriginalStockAndRestoresCapacityWithExactRecovery(string mode)
    {
        var f=Fixture();var source=Source(f);var review=await f.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,()=>Task.CompletedTask);var allocation=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),review.ComparedAtUtc,review.ComparedAtUtc.AddHours(2),"Original requirement");var saved=await f.Inventory.RecordWorkOrderMaterialAllocationAsync(allocation,()=>Task.CompletedTask,()=>Task.CompletedTask);var originalJson=JsonSerializer.Serialize(saved);var release=new CompanyWorkOrderMaterialReleaseCommand(new(allocation,saved),Guid.NewGuid(),"Cancelled work requirement");var revoked=false;
        if(mode=="original-drift")f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>().Single().QUANTITY=3m;
        f.Native.Source.Setup(s=>s.InsertEntity("WORK_ORDER_MATERIAL_RELEASE",It.IsAny<object>())).Returns((string _,object value)=>{f.Native.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((WORK_ORDER_MATERIAL_RELEASE)value);if(mode=="readback")row.RELEASED_QUANTITY=999;f.Native.Rows.Add(row);if(mode=="authority")revoked=true;return new ErrorsInfo{Flag=Errors.Ok};});
        Task Guard()=>revoked?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;if(mode=="unknown-commit")f.Native.FailCommit=true;
        var error=await Record.ExceptionAsync(()=>f.Inventory.ReleaseWorkOrderMaterialAsync(release,Guard,()=>Task.CompletedTask));
        Assert.Equal(10m,f.Item.QUANTITY_ON_HAND);Assert.Empty(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());
        if(mode is not ("original" or "unknown-commit")){Assert.NotNull(error);Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_MATERIAL_RELEASE>());return;}
        if(mode=="original")Assert.Null(error);else{Assert.NotNull(error);f.Native.FailCommit=false;}
        var rowRelease=Assert.Single(f.Native.Rows.OfType<WORK_ORDER_MATERIAL_RELEASE>());release.RequireRecorded(rowRelease);Assert.Equal(originalJson,JsonSerializer.Serialize(f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>().Single()));var writes=f.Native.Writes;
        var recovery=await f.Inventory.ReleaseWorkOrderMaterialAsync(release,Guard,()=>Task.CompletedTask,true);release.RequireRecorded(recovery);var recoveredReference=await f.Inventory.RecoverWorkOrderMaterialReleaseReferenceAsync(source.Context.Company,"actor",new(new(source.Query.Task,allocation.IntentId),release.IntentId),Guard);recoveredReference.Require(source.Context.Company,"actor",new(new(source.Query.Task,allocation.IntentId),release.IntentId));await f.Inventory.ReleaseWorkOrderMaterialAsync(release,Guard,()=>Task.CompletedTask);Assert.Equal(writes,f.Native.Writes);
        var fresh=await f.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,Guard);fresh.Require();Assert.Equal(0m,fresh.ReservedQuantity);Assert.Equal(10m,fresh.AvailableQuantity);Assert.Single(fresh.Releases!);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Inventory.ReleaseWorkOrderMaterialAsync(release with{IntentId=Guid.NewGuid()},Guard,()=>Task.CompletedTask));Assert.Equal(writes,f.Native.Writes);
    }
    [Fact] public void ReleaseReferencesCannotInventCapacityAndOldReservationBodiesRemainExact()
    {
        var f=Fixture();var source=Source(f);var utc=DateTime.UtcNow;var old=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);Assert.DoesNotContain("Releases",JsonSerializer.Serialize(old));old.Require();var command=new CompanyWorkOrderMaterialAllocationCommand(old,Guid.NewGuid(),utc,utc.AddHours(2),"Original requirement");var row=command.Prepare(utc);var release=new CompanyWorkOrderMaterialReleaseCommand(new(command,row),Guid.NewGuid(),"Cancelled original");var releaseRow=release.Prepare(utc.AddSeconds(1));
        var current=CompanyWorkOrderMaterialAllocationReview.Create(source,[row],[releaseRow],utc.AddSeconds(2));Assert.Equal(10m,current.AvailableQuantity);var damaged=current with{Releases=[new(row.ALLOCATION_ID,releaseRow.RECORDED_REVISION,utc,1m)]};Assert.Throws<InvalidOperationException>(()=>damaged.Require());
        releaseRow.ALLOCATION_ID="other";Assert.Throws<InvalidOperationException>(()=>CompanyWorkOrderMaterialAllocationReview.Create(source,[row],[releaseRow],utc.AddSeconds(2)));
    }
    [Theory][InlineData("original")][InlineData("foreign")][InlineData("late-authority")]
    public async Task AuthorizedPublicReservationFactsNeverReturnPrivateOriginalBody(string mode)
    {
        var f=Fixture();var source=Source(f);var review=await f.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,()=>Task.CompletedTask);var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),review.ComparedAtUtc,review.ComparedAtUtc.AddHours(1),"Private reservation rationale");await f.Inventory.RecordWorkOrderMaterialAllocationAsync(command,()=>Task.CompletedTask,()=>Task.CompletedTask);
        if(mode=="foreign"){var row=f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>().Single();row.OWNER_COMPANY_BA_ID="foreign";row.COMMAND_JSON="malformed private body";}var guards=0;Task Guard()=>mode=="late-authority" && ++guards>2?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        var error=await Record.ExceptionAsync(async()=>{var result=await f.Inventory.ReadWorkOrderMaterialReservationFactsAsync(source.Context.Company,source.Query.Task,command.IntentId,Guard);result.Require(source.Context.Company,source.Query.Task,command.IntentId);Assert.DoesNotContain("COMMAND_JSON",JsonSerializer.Serialize(result));Assert.DoesNotContain("Private reservation rationale",JsonSerializer.Serialize(result));});if(mode=="original")Assert.Null(error);else Assert.IsType<UnauthorizedAccessException>(error);Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData("current")][InlineData("released")][InlineData("hidden")][InlineData("late-authority")][InlineData("duplicate")]
    public async Task NativeFieldReservationQueueFiltersRightsBeforePrivateBodiesAndNeverInfersStock(string mode)
    {
        var f=Fixture();var source=Source(f);var review=await f.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,()=>Task.CompletedTask);var command=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),review.ComparedAtUtc,review.ComparedAtUtc.AddHours(1),"Private rationale");var row=await f.Inventory.RecordWorkOrderMaterialAllocationAsync(command,()=>Task.CompletedTask,()=>Task.CompletedTask);
        if(mode=="released")await f.Inventory.ReleaseWorkOrderMaterialAsync(new(new(command,row),Guid.NewGuid(),"Private cancellation"),()=>Task.CompletedTask,()=>Task.CompletedTask);
        if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(row));
        if(mode=="hidden")f.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>().Single().COMMAND_JSON="malformed private body";
        var calls=0;Task Guard()=>mode=="late-authority" && ++calls>3?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        var error=await Record.ExceptionAsync(async()=>
        {
            var result=await f.Inventory.ReadMaterialReservationQueueAsync(source.Context,order=>mode!="hidden" && order==source.Query.Task.WorkOrderId,Guard);result.Require(source.Context.Company,"actor","field");
            Assert.Equal(mode=="hidden"?0:1,result.Count());if(mode!="hidden")Assert.Equal(1,result.Count(mode=="released"?CompanyMaterialReservationStatus.Released:CompanyMaterialReservationStatus.Active));
            var json=JsonSerializer.Serialize(result);Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain("Private rationale",json);Assert.DoesNotContain("Private cancellation",json);Assert.DoesNotContain("AvailableQuantity",json);
        });
        if(mode=="late-authority")Assert.IsType<UnauthorizedAccessException>(error);else if(mode=="duplicate")Assert.NotNull(error);else Assert.Null(error);Assert.Equal(mode=="released"?2:1,f.Native.Writes);Assert.Equal(10m,f.Item.QUANTITY_ON_HAND);
    }
}
#endif
