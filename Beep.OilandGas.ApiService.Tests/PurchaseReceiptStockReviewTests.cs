using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PurchaseReceiptStockReviewTests
{
    [Theory]
    [InlineData("empty")][InlineData("received")][InlineData("returned")][InlineData("escaped")][InlineData("usage")][InlineData("all-rejected")][InlineData("fully-stocked")]
    [InlineData("unknown")][InlineData("negative")][InlineData("over-accepted")][InlineData("over-returned")]
    [InlineData("no-original")][InlineData("foreign")][InlineData("unit")][InlineData("location")][InlineData("decision")]
    [InlineData("legacy")][InlineData("unclassified")][InlineData("malformed")][InlineData("row")][InlineData("audit")][InlineData("invalid-kind")]
    [InlineData("metadata-missing")][InlineData("metadata-size")][InlineData("metadata-null")]
    [InlineData("inventory-grant")][InlineData("procurement-grant")][InlineData("closing-po")][InlineData("binding")]
    public async Task ActualClientApiNativeReviewUsesGrossAcceptanceAndOriginalReturnProvenance(string mode)
    {
        if(mode is "empty" or "received" or "returned" or "escaped" or "usage" or "all-rejected" or "fully-stocked"){await RecordedStockSource(mode);return;}
        var f=new PurchaseReceiptDecisionTests.Fixture();var n=f.Existing.Native;
        var orderId=mode=="escaped"?"order/a?b":"order";var receiptId=mode=="escaped"?"receipt/α%2F":"receipt";
        f.Existing.Order.PURCHASE_ORDER_ID=orderId;f.Existing.OrderLine.PURCHASE_ORDER_ID=orderId;f.Existing.Receipt.PURCHASE_ORDER_ID=orderId;f.Existing.Receipt.PO_RECEIPT_ID=receiptId;
        var item=new INVENTORY_ITEM{INVENTORY_ITEM_ID="item/α%2F",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="bbl",QUANTITY_ON_HAND=13m};
        f.Existing.OrderLine.INVENTORY_ITEM_ID=item.INVENTORY_ITEM_ID;
        await f.Prepare(accepted:mode=="all-rejected"?0m:8m,rejected:mode=="all-rejected"?10m:2m,orderId:orderId,receiptId:receiptId);await f.Record();var writes=n.Writes;
        var decisionRevision=ProcessDefinitionRevision.FromJson(f.Stored.RECEIVER_DECISION_JSON!);
        var rows=new List<INVENTORY_TRANSACTION>();
        INVENTORY_TRANSACTION Movement(PurchaseReceiptStockMovementKind kind,decimal quantity,string? original=null)
        {
            var id=Guid.NewGuid().ToString("N");var utc=new DateTime(2026,10,7,10,kind==PurchaseReceiptStockMovementKind.Receive?0:1,0,DateTimeKind.Utc);
            var row=new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID=id,INVENTORY_ITEM_ID=item.INVENTORY_ITEM_ID,ACTIVE_IND="Y",TRANSACTION_TYPE=kind==PurchaseReceiptStockMovementKind.Receive?"PURCHASE_RECEIPT":"SUPPLIER_RETURN",QUANTITY=kind==PurchaseReceiptStockMovementKind.Receive?quantity:-quantity,TRANSACTION_DATE=utc,ROW_CREATED_BY="warehouse-user",ROW_CHANGED_BY="warehouse-user",ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};
            var proof=new PurchaseReceiptStockMovement(new("tenant","company"),orderId,receiptId,item.INVENTORY_ITEM_ID,"bbl","warehouse","bin",decisionRevision,new string('a',64),"binding",id,"warehouse-user",kind,quantity,kind==PurchaseReceiptStockMovementKind.Receive?10m:14m,kind==PurchaseReceiptStockMovementKind.Receive?10m+quantity:14m-quantity,original,"physical movement",PurchaseReceiptStockMovement.DigestRow(row));
            if(mode=="foreign")proof=proof with{Company=new("tenant","other")};if(mode=="unit")proof=proof with{QuantityUnit="m³"};if(mode=="location")proof=proof with{LocationId="other"};if(mode=="decision")proof=proof with{ReceiptDecisionRevision=new string('b',64)};if(mode=="invalid-kind")proof=proof with{Kind=(PurchaseReceiptStockMovementKind)0};
            row.PURCHASE_RECEIPT_STOCK_JSON=JsonSerializer.Serialize(proof);return row;
        }
        if(mode is not ("empty" or "all-rejected"))rows.Add(Movement(PurchaseReceiptStockMovementKind.Receive,mode=="over-accepted"?9m:mode=="fully-stocked"?8m:4m));
        if(mode is "returned" or "over-returned" or "no-original")rows.Add(Movement(PurchaseReceiptStockMovementKind.Return,mode=="over-returned"?5m:1m,mode=="no-original"?Guid.NewGuid().ToString("N"):rows[0].INVENTORY_TRANSACTION_ID));
        if(mode=="unknown")item.QUANTITY_ON_HAND=null;if(mode=="negative")item.QUANTITY_ON_HAND=-1;
        if(mode=="legacy"){rows[0].PURCHASE_RECEIPT_STOCK_JSON=null;rows[0].TRANSACTION_TYPE="RECEIPT";}
        if(mode=="unclassified"){rows[0].PURCHASE_RECEIPT_STOCK_JSON=null;rows[0].TRANSACTION_TYPE=null;}
        if(mode=="usage")rows.Add(new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="usage",INVENTORY_ITEM_ID=item.INVENTORY_ITEM_ID,ACTIVE_IND="Y",TRANSACTION_TYPE="USAGE",QUANTITY=-1m});
        if(mode=="malformed")rows[0].PURCHASE_RECEIPT_STOCK_JSON="{}";if(mode=="row")rows[0].QUANTITY=2m;if(mode=="audit")rows[0].ROW_CHANGED_BY="other";
        var creator=new ClassCreator(n.Editor.Object);var schema=creator.ConvertToEntityStructure(typeof(INVENTORY_TRANSACTION));
        var field=Assert.Single(schema.Fields,value=>value.FieldName=="PURCHASE_RECEIPT_STOCK_JSON");Assert.True(field.AllowDBNull);Assert.Equal(16384,field.Size1);
        if(mode=="metadata-missing")schema.Fields.Remove(field);if(mode=="metadata-size")field.Size1=128;if(mode=="metadata-null")field.AllowDBNull=false;
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_ITEM"),true)).Returns(creator.ConvertToEntityStructure(typeof(INVENTORY_ITEM)));
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_TRANSACTION"),true)).Returns(schema);
        n.Source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new[]{item});
        var historyReads=0;
        n.Source.Setup(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>{if(++historyReads==2 && mode=="closing-po")f.Existing.OrderLine.QUANTITY=99m;return rows.ToArray();});
        var inventory=CompanyInventorySourceTests.CreateService(n.Editor,()=>Task.FromResult(n.Connection));
        string[] grants=mode=="inventory-grant"?["Procurement.View"]:mode=="procurement-grant"?["Inventory.View"]:["Procurement.View","Inventory.View"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configureAccounting:(services,context)=>{services.AddSingleton(inventory);services.AddSingleton(f.Existing.PO);services.AddSingleton(f.Existing.AP);context.ChangeBinding=mode=="binding";});
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode is "empty" or "received" or "returned" or "escaped" or "usage" or "all-rejected" or "fully-stocked")
        {
            var review=await client.GetPurchaseReceiptStockReviewAsync(orderId,receiptId);review.Require(new("tenant","company"),"actor",orderId,receiptId);
            Assert.Equal(mode=="all-rejected"?0m:8m,review.AcceptedQuantity);Assert.Equal(mode is "empty" or "all-rejected"?0m:mode=="fully-stocked"?8m:4m,review.ReceivedQuantity);Assert.Equal(mode=="returned"?1m:0m,review.ReturnedQuantity);Assert.Equal(mode=="empty"?8m:mode is "all-rejected" or "fully-stocked"?0m:4m,review.RemainingReceiptQuantity);
            Assert.Equal(2,historyReads);Assert.Equal(13m,review.Source.Inventory.Item.QUANTITY_ON_HAND);item.LOCATION_ID="changed";Assert.Equal("bin",review.Source.Inventory.Item.LOCATION_ID);
            var altered=review with{RemainingReceiptQuantity=8m+review.ReturnedQuantity};if(mode!="empty")Assert.Throws<InvalidOperationException>(()=>altered.Require(new("tenant","company"),"actor",orderId,receiptId));
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetPurchaseReceiptStockReviewAsync(orderId,receiptId));
            Assert.Equal(mode is "inventory-grant" or "procurement-grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
        }
        Assert.Equal(writes,n.Writes);
    }
    private static async Task RecordedStockSource(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();var n=f.Native;
        if(mode=="escaped"){f.OrderId="order/a?b";f.ReceiptId="receipt/α%2F";f.Item.INVENTORY_ITEM_ID="item/α%2F";f.Receiving.Existing.OrderLine.INVENTORY_ITEM_ID=f.Item.INVENTORY_ITEM_ID;}
        if(mode=="all-rejected"){await f.Receiving.Prepare(accepted:0m,rejected:10m);await f.Receiving.Record();n.Writes=0;}
        else await f.Initialize();
        if(mode is not ("empty" or "all-rejected"))
        {
            await f.Prepare(quantity:mode=="fully-stocked"?8m:4m);await f.Record();
            if(mode=="returned"){var received=f.Submission.Command.IntentId;await f.Prepare(PurchaseReceiptStockMovementKind.Return,1m,received);await f.Record();}
        }
        if(mode=="usage"){n.Rows.Add(new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="recorded-unrelated-usage",INVENTORY_ITEM_ID=f.Item.INVENTORY_ITEM_ID,ACTIVE_IND="Y",TRANSACTION_TYPE="USAGE",QUANTITY=-1m});f.Item.QUANTITY_ON_HAND-=1m;}
        var before=n.Writes;var onHand=f.Item.QUANTITY_ON_HAND;
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Inventory.View","Procurement.View"],configureAccounting:(services,context)=>{services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.PO);services.AddSingleton(f.Receiving.Existing.AP);});
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Inventory.View","Procurement.View"])));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var review=await client.GetPurchaseReceiptStockReviewAsync(f.OrderId,f.ReceiptId);review.Require(new("tenant","company"),"actor",f.OrderId,f.ReceiptId);
        Assert.Equal(mode=="all-rejected"?0m:8m,review.AcceptedQuantity);Assert.Equal(mode is "empty" or "all-rejected"?0m:mode=="fully-stocked"?8m:4m,review.ReceivedQuantity);Assert.Equal(mode=="returned"?1m:0m,review.ReturnedQuantity);Assert.Equal(mode=="empty"?8m:mode is "all-rejected" or "fully-stocked"?0m:4m,review.RemainingReceiptQuantity);
        Assert.Equal(onHand,review.Source.Inventory.Item.QUANTITY_ON_HAND);Assert.Equal("bin",review.Source.Inventory.Item.LOCATION_ID);Assert.Equal(mode is "empty" or "all-rejected"?0:mode=="returned"?2:1,n.Rows.OfType<INVENTORY_STOCK_INTENT>().Count());Assert.Equal(before,n.Writes);
        f.Item.LOCATION_ID="changed";Assert.Equal("bin",review.Source.Inventory.Item.LOCATION_ID);
        var altered=review with{RemainingReceiptQuantity=8m+review.ReturnedQuantity};if(mode!="empty")Assert.Throws<InvalidOperationException>(()=>altered.Require(new("tenant","company"),"actor",f.OrderId,f.ReceiptId));
    }
}
#endif
