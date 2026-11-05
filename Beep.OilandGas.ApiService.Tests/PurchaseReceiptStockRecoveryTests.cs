using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PurchaseReceiptStockRecoveryTests
{
    [Theory][InlineData("purchase")][InlineData("item")][InlineData("both")][InlineData("foreign-item")][InlineData("missing-movement")][InlineData("altered-movement")][InlineData("prior")][InlineData("duplicate-movement")]
    public async Task HistoricalRecoveryUsesOriginalNativeEvidenceWithoutSubstitutingCurrentSource(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();
        if(mode=="prior"){var received=f.Submission.Command.IntentId;await f.Prepare(PurchaseReceiptStockMovementKind.Return,1,received);await f.Record();}
        var original=CompanyInventorySourceReview.Copy(f.Submission);var writes=f.Native.Writes;
        if(mode is "purchase" or "both")f.Native.Rows.RemoveAll(row=>row is PURCHASE_ORDER);
        if(mode is not "purchase"){if(mode=="foreign-item")f.Item.OWNER_COMPANY_BA_ID="other";else f.Native.Rows.RemoveAll(row=>row is INVENTORY_ITEM);}
        if(mode=="missing-movement")f.Native.Rows.RemoveAll(row=>row is INVENTORY_TRANSACTION);
        if(mode=="altered-movement")f.Native.Rows.OfType<INVENTORY_TRANSACTION>().Single().NOTES="changed";
        if(mode=="duplicate-movement"){var duplicate=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<INVENTORY_TRANSACTION>().Single());duplicate.INVENTORY_ITEM_ID="foreign item";f.Native.Rows.Add(duplicate);}
        f.Native.Rows.Add(new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="later unrelated",INVENTORY_ITEM_ID="item",NOTES="Foreign later data must not be returned"});
        Task<PurchaseReceiptStockRecovery> Read()=>f.Inventory.GetStockRecoveryAsync(new("tenant","company"),original.Command.IntentId,"actor",f.Receiving.Existing.PO,f.Authority);
        if(mode is "missing-movement" or "altered-movement" or "duplicate-movement")await Assert.ThrowsAsync<InvalidOperationException>(()=>Read());
        else
        {
            var result=await Read();result.Require(new("tenant","company"),"actor",original.Command.IntentId);Assert.Equal(original.Command,result.OriginalSubmission().Command);Assert.False(result.CurrentMatchesOriginalEffect());
            Assert.Equal(mode is not "purchase" and not "both",result.CurrentPurchaseAvailable);Assert.Equal(mode=="purchase",result.CurrentInventoryAvailable);
            if(mode!="purchase"){Assert.Null(result.CurrentInventory);Assert.DoesNotContain(result.NativeHistory!,row=>row.INVENTORY_TRANSACTION_ID=="later unrelated");Assert.Equal(mode=="prior"?2:1,result.NativeHistory!.Count);}
        }
        Assert.Equal(writes,f.Native.Writes);
    }
    [Fact] public async Task ActualWriterHistoryAboveTwoHundredCanReadItsFinalBeepPage()
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();
        for(var i=0;i<201;i++){await f.Prepare(quantity:0.01m);await f.Record();}
        Assert.Equal(201,f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Count());var writes=f.Native.Writes;
        var result=await f.Inventory.GetStockRecoveryPageAsync(new("tenant","company"),"actor",5,50,f.Receiving.Existing.PO,f.Authority);
        result.Require(new("tenant","company"),"actor",5,50);Assert.Single(result.Index.Entries);Assert.Equal(201,result.TotalRecords);Assert.False(result.HasNextPage);Assert.True(result.HasPreviousPage);Assert.Equal(writes,f.Native.Writes);
    }
    [Theory][InlineData("first")][InlineData("last")][InlineData("count")][InlineData("foreign")][InlineData("provider-error")][InlineData("drift")][InlineData("invalid-request")]
    public async Task NativeBeepPagesValidateMetadataAndCompleteOwnedMembership(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();await f.Prepare(quantity:4);await f.Record();var baseline=f.Native.Writes;
        var page=mode=="last"?2:1;var captures=0;
        if(mode=="provider-error")f.Native.Source.SetupGet(value=>value.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Failed});
        if(mode is "count" or "foreign" or "drift")f.Native.Source.Setup(value=>value.GetEntity("INVENTORY_STOCK_INTENT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>(),It.IsAny<int>(),It.IsAny<int>())).Returns((string table,List<TheTechIdea.Beep.Report.AppFilter> filters,int number,int size)=>{
            var row=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().OrderBy(value=>value.INVENTORY_STOCK_INTENT_ID,StringComparer.Ordinal).First());
            if(mode=="foreign")row.OWNER_COMPANY_BA_ID="other";var result=new PagedResult(new[]{row},number,size,mode=="drift" && ++captures>1?3:2);
            if(mode=="count")result.TotalPages=0;return result;
        });
        Task<PurchaseReceiptStockIntentPage> Read()=>f.Inventory.GetStockRecoveryPageAsync(new("tenant","company"),"actor",mode=="invalid-request"?0:page,1,f.Receiving.Existing.PO,f.Authority);
        if(mode is "first" or "last")
        {
            var result=await Read();result.Require(new("tenant","company"),"actor",page,1);Assert.Equal(2,result.TotalRecords);Assert.Single(result.Index.Entries);Assert.Equal(page==1,result.HasNextPage);Assert.Equal(page==2,result.HasPreviousPage);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>Read());Assert.Equal(baseline,f.Native.Writes);
    }
    [Theory][InlineData("exact")][InlineData("item-changed")][InlineData("purchase-changed")][InlineData("later-receipt")]
    [InlineData("owner")][InlineData("actor")][InlineData("command")][InlineData("snapshot")][InlineData("audit")][InlineData("entry-digest")]
    [InlineData("movement")][InlineData("missing-movement")][InlineData("duplicate-entry")][InlineData("missing-entry")]
    [InlineData("metadata-missing")][InlineData("metadata-size")][InlineData("metadata-null")][InlineData("binding")][InlineData("authority")]
    [InlineData("closing-entry")][InlineData("closing-purchase")]
    public async Task ActualNativeRecoveryRestoresOriginalPreimageAndSeparatesItFromCurrentOwnedState(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();var original=CompanyInventorySourceReview.Copy(f.Submission);
        var first=await f.Record();var movement=Assert.Single(first.Inventory.Transactions);
        var entry=Assert.Single(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>());Assert.Equal(JsonSerializer.Serialize(INVENTORY_STOCK_INTENT.Create(original,movement)),JsonSerializer.Serialize(entry));var originalBytes=JsonSerializer.Serialize(entry);
        if(mode=="item-changed")f.Item.LOCATION_ID="new bin";if(mode=="purchase-changed")f.Receiving.Existing.Order.DESCRIPTION="Later source";
        if(mode=="later-receipt"){await f.Prepare(quantity:4);await f.Record();Assert.Equal(originalBytes,JsonSerializer.Serialize(entry));}
        var writes=f.Native.Writes;
        if(mode=="owner")entry.OWNER_COMPANY_BA_ID="other";if(mode=="actor")entry.ROW_CREATED_BY="other";
        if(mode=="command")entry.COMMAND_JSON=JsonSerializer.Serialize(original.Command with{Reason="Replacement reason"});if(mode=="snapshot")entry.ORIGINAL_ITEM_JSON="{}";
        if(mode=="audit")entry.ROW_CHANGED_BY="other";if(mode=="entry-digest")entry.RECORDED_REVISION=new string('a',64);
        if(mode is "command" or "snapshot" or "audit")entry.RECORDED_REVISION=INVENTORY_STOCK_INTENT.EntryRevision(entry);
        if(mode=="movement")f.Native.Rows.OfType<INVENTORY_TRANSACTION>().Single().QUANTITY=999;
        if(mode=="missing-movement")f.Native.Rows.RemoveAll(value=>value is INVENTORY_TRANSACTION);
        if(mode=="duplicate-entry")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(entry));if(mode=="missing-entry")f.Native.Rows.Remove(entry);
        var schema=new ClassCreator(f.Native.Editor.Object).ConvertToEntityStructure(typeof(INVENTORY_STOCK_INTENT));
        var field=Assert.Single(schema.Fields,value=>value.FieldName=="ORIGINAL_PURCHASE_JSON");Assert.Equal(4194304,field.Size1);Assert.False(field.AllowDBNull);
        if(mode=="metadata-missing")schema.Fields.Remove(field);if(mode=="metadata-size")field.Size1=64;if(mode=="metadata-null")field.AllowDBNull=true;
        f.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_STOCK_INTENT"),true)).Returns(schema);
        var schemaReads=0;
        if(mode is "closing-entry" or "closing-purchase")f.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_STOCK_INTENT"),true)).Returns(()=>{
            if(++schemaReads==2){if(mode=="closing-entry")entry.COMMAND_JSON="{}";else f.Receiving.Existing.Order.DESCRIPTION="Changed during closing capture";}return schema;
        });
        var authorityCalls=0;Task Authority(){if(++authorityCalls==2 && mode=="binding")f.Native.Connection="changed";if(authorityCalls==2 && mode=="authority")throw new UnauthorizedAccessException();return Task.CompletedTask;}
        Task<PurchaseReceiptStockRecovery> Read()=>f.Inventory.GetStockRecoveryAsync(new("tenant","company"),original.Command.IntentId,"actor",f.Receiving.Existing.PO,Authority);
        if(mode is "exact" or "item-changed" or "purchase-changed" or "later-receipt")
        {
            var recovered=await Read();recovered.Require(original.Command.Company,"actor",original.Command.IntentId);
            var restored=recovered.OriginalSubmission();Assert.Equal(original.Command,restored.Command);Assert.Equal(original.Review.Revision,restored.Review.Revision);
            Assert.Equal(JsonSerializer.Serialize(original.Review.Source.Inventory.Item),JsonSerializer.Serialize(restored.Review.Source.Inventory.Item));
            Assert.Equal(mode=="exact",recovered.CurrentMatchesOriginalEffect());Assert.Equal(mode=="later-receipt"?18m:14m,recovered.CurrentInventory.Item.QUANTITY_ON_HAND);
            entry.COMMAND_JSON="changed caller";Assert.Equal(original.Command,recovered.OriginalSubmission().Command);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>Read());
        Assert.Equal(writes,f.Native.Writes);
    }
    [Fact] public async Task PriorMovementManifestRestoresLaterReturnWithoutEmbeddingEarlierRecoveryEntries()
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();var beforeFirst=f.Submission;var first=await f.Record();
        var firstEntry=INVENTORY_STOCK_INTENT.Create(beforeFirst,Assert.Single(first.Inventory.Transactions));var bytes=JsonSerializer.Serialize(firstEntry);
        await f.Prepare(PurchaseReceiptStockMovementKind.Return,1,beforeFirst.Command.IntentId);var beforeReturn=CompanyInventorySourceReview.Copy(f.Submission);var returned=await f.Record();
        var second=INVENTORY_STOCK_INTENT.Create(beforeReturn,returned.Inventory.Transactions.Single(row=>row.INVENTORY_TRANSACTION_ID==beforeReturn.Command.IntentId));
        var restored=second.Restore(new("tenant","company"),"actor",returned.Inventory.Transactions);Assert.Equal(beforeReturn.Command,restored.Command);Assert.Equal(beforeReturn.Review.Revision,restored.Review.Revision);
        Assert.DoesNotContain("COMMAND_JSON",second.PRIOR_MOVEMENT_MANIFEST_JSON);Assert.DoesNotContain("ORIGINAL_PURCHASE_JSON",second.PRIOR_MOVEMENT_MANIFEST_JSON);
        Assert.Equal(bytes,JsonSerializer.Serialize(firstEntry));
        var prior=returned.Inventory.Transactions.Single(row=>row.INVENTORY_TRANSACTION_ID==beforeFirst.Command.IntentId);prior.NOTES="Altered prior movement";
        Assert.Throws<InvalidOperationException>(()=>second.Restore(new("tenant","company"),"actor",returned.Inventory.Transactions));
    }
}
#endif
