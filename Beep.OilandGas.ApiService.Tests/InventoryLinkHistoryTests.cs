#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InventoryLinkHistoryTests
{
    internal static PO_INVENTORY_LINK_HISTORY Entry(InventoryLinkDecisionTests.Fixture f)
    {
        var at=DateTime.UtcNow;var row=new PO_INVENTORY_LINK_HISTORY{PO_INVENTORY_LINK_HISTORY_ID=Guid.NewGuid().ToString("N"),OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",SEQUENCE_NUMBER=1,CYCLE_INTENT_ID=Guid.NewGuid().ToString("N"),SOURCE_REVISION=f.Review.Purchase.Revision,BINDING_REVISION="binding",REASON="Reopen independently resolved reference for a new reviewed target.",ARCHIVED_LINE_JSON=JsonSerializer.Serialize(f.Stored),ACTIVE_IND="Y",ROW_CREATED_BY="reviewer",ROW_CHANGED_BY="reviewer",ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};
        row.RECORDED_REVISION=PurchaseOrderInventoryLinkHistoryReview.EntryRevision(row);return row;
    }
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("grant")][InlineData("foreign")][InlineData("duplicate")][InlineData("missing")][InlineData("schema")][InlineData("cursor-schema")][InlineData("drift")][InlineData("head")]
    public async Task ExistingNativeOwnerReadsCompleteOwnedHistoryAndRequiresMatchingActiveCursor(string mode)
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create();await f.Record();var n=f.Existing.Existing.Native;var writes=n.Writes;var entry=Entry(f);
        if(mode!="empty")f.Stored.INVENTORY_LINK_HISTORY_REVISION=entry.RECORDED_REVISION;
        var schema=new ClassCreator(n.Editor.Object).ConvertToEntityStructure(typeof(PO_INVENTORY_LINK_HISTORY));if(mode=="schema")schema.Fields.RemoveAll(field=>field.FieldName=="OWNER_TENANT_ID");
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="PO_INVENTORY_LINK_HISTORY"),true)).Returns(schema);
        if(mode=="cursor-schema"){var columns=new ClassCreator(n.Editor.Object).ConvertToEntityStructure(typeof(PO_LINE_ITEM));columns.Fields.RemoveAll(field=>field.FieldName=="INVENTORY_LINK_HISTORY_REVISION");n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="PO_LINE_ITEM"),true)).Returns(columns);}
        var reads=0;n.Source.Setup(value=>value.GetEntityAsync("PO_INVENTORY_LINK_HISTORY",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>{
            ++reads;if(mode=="foreign")entry.OWNER_COMPANY_BA_ID="other";if(mode=="drift" && reads==2)f.Existing.Existing.Order.DESCRIPTION="changed";
            return mode is "empty" or "missing"?Array.Empty<PO_INVENTORY_LINK_HISTORY>():mode=="duplicate"?new[]{entry,entry}:new[]{entry};});
        if(mode=="head")f.Stored.INVENTORY_LINK_HISTORY_REVISION=new string('A',64);
        Task<PurchaseOrderInventoryLinkHistoryReview> Read()=>f.Existing.Existing.PO.GetInventoryLinkHistoryAsync(new("tenant","company"),"order","po-line","reviewer",()=>mode=="grant"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask);
        if(mode is "valid" or "empty")
        {
            var review=await Read();Assert.Equal(mode=="empty"?0:1,review.Entries.Count);Assert.Equal(2,reads);
            if(mode=="valid"){entry.REASON="tampered";Assert.NotEqual(entry.REASON,review.Entries[0].REASON);}
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>Read());Assert.Equal(writes,n.Writes);
    }
    [Theory][InlineData("sequence")][InlineData("predecessor")][InlineData("intent")][InlineData("audit")][InlineData("archive")][InlineData("revision")]
    public async Task CanonicalHistoryRejectsMissingOrForgedCycleEvidence(string mode)
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create();await f.Record();var entry=Entry(f);
        if(mode=="sequence")entry.SEQUENCE_NUMBER=2;if(mode=="predecessor")entry.PREVIOUS_REVISION=new string('A',64);if(mode=="intent")entry.CYCLE_INTENT_ID="invalid";
        if(mode=="audit")entry.ROW_CREATED_BY="SYSTEM";if(mode=="archive")entry.ARCHIVED_LINE_JSON="{}";
        if(mode!="revision")entry.RECORDED_REVISION=PurchaseOrderInventoryLinkHistoryReview.EntryRevision(entry);else entry.REASON="changed";
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderInventoryLinkHistoryReview.Create(new("tenant","company"),"reviewer","order","po-line",[entry],entry.RECORDED_REVISION));
    }
}
#endif
