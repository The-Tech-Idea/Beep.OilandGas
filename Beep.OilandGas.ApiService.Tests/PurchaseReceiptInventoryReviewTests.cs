using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Accounting.Services;
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
public sealed class PurchaseReceiptInventoryReviewTests
{
    [Theory][InlineData("valid")][InlineData("escaped")][InlineData("missing-link")][InlineData("foreign-item")][InlineData("unit")][InlineData("warehouse")][InlineData("location")][InlineData("inactive")][InlineData("decision")][InlineData("inventory-grant")][InlineData("procurement-grant")][InlineData("binding")][InlineData("link-drift")]
    public async Task ActualReceiptGraphRequiresRecordedLinkProofOwnershipUnitsAndBothGrants(string mode)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();var n=f.Existing.Native;
        var orderId=mode=="escaped"?"order/a?b":"order";var receiptId=mode=="escaped"?"receipt/α%2F":"receipt";
        f.Existing.Order.PURCHASE_ORDER_ID=orderId;f.Existing.OrderLine.PURCHASE_ORDER_ID=orderId;f.Existing.Receipt.PURCHASE_ORDER_ID=orderId;f.Existing.Receipt.PO_RECEIPT_ID=receiptId;
        var item=new INVENTORY_ITEM{INVENTORY_ITEM_ID="item/α%2F",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="bbl",CURRENCY_CODE="KWD",QUANTITY_ON_HAND=null};
        f.Existing.OrderLine.INVENTORY_ITEM_ID=item.INVENTORY_ITEM_ID;
        await f.Prepare(orderId:orderId,receiptId:receiptId);await f.Record();var writes=n.Writes;
        if(mode=="missing-link")f.Existing.OrderLine.INVENTORY_ITEM_ID=null;if(mode=="foreign-item")item.OWNER_COMPANY_BA_ID="other";
        if(mode=="unit")item.UNIT_OF_MEASURE="m³";if(mode=="warehouse")item.WAREHOUSE_ID=null;if(mode=="location")item.LOCATION_ID=null;if(mode=="inactive")item.ACTIVE_IND="N";
        if(mode=="decision")f.Stored.RECEIVER_DECISION_JSON="{}";
        n.Rows.Add(item);
        var creator=new ClassCreator(n.Editor.Object);
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_ITEM"),true)).Returns(creator.ConvertToEntityStructure(typeof(INVENTORY_ITEM)));
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_TRANSACTION"),true)).Returns(creator.ConvertToEntityStructure(typeof(INVENTORY_TRANSACTION)));
        var itemReads=0;
        n.Source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>{++itemReads;if(mode=="link-drift" && itemReads==1)f.Existing.OrderLine.INVENTORY_ITEM_ID="different";return new[]{item};});
        n.Source.Setup(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(Array.Empty<INVENTORY_TRANSACTION>());
        var inventory=CompanyInventorySourceTests.CreateService(n.Editor,()=>Task.FromResult(n.Connection));
        string[] grants=mode=="inventory-grant"?["Procurement.View"]:mode=="procurement-grant"?["Inventory.View"]:["Procurement.View","Inventory.View"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configureAccounting:(services,context)=>{services.AddSingleton(inventory);services.AddSingleton(f.Existing.PO);services.AddSingleton(f.Existing.AP);context.ChangeBinding=mode=="binding";});
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode is "valid" or "escaped")
        {
            var review=await client.GetPurchaseReceiptInventoryReviewAsync(orderId,receiptId);review.Require(new("tenant","company"),"actor",orderId,receiptId);
            Assert.Equal(item.INVENTORY_ITEM_ID,review.Inventory.Item.INVENTORY_ITEM_ID);Assert.Null(review.Inventory.Item.QUANTITY_ON_HAND);Assert.Equal(8m,PurchaseReceiptDecisionProof.Require(review.Purchase.Company,Assert.Single(review.Purchase.Receipts)).Command.AcceptedQuantity);Assert.Equal("bin",review.Inventory.Item.LOCATION_ID);
            item.LOCATION_ID="changed";Assert.Equal("bin",review.Inventory.Item.LOCATION_ID);Assert.Equal(2,itemReads);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetPurchaseReceiptInventoryReviewAsync(orderId,receiptId));
            Assert.Equal(mode is "inventory-grant" or "procurement-grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
        }
        Assert.Equal(writes,n.Writes);
    }
    [Fact] public void ExplicitLineLinkUsesBoundedNullableMetadataAndChangesSourceRevision()
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();var source=PurchaseOrderSourceReview.Create(new("tenant","company"),"actor",f.Existing.Order,[f.Existing.OrderLine],[f.Existing.Receipt]);
        var field=Assert.Single(new ClassCreator(f.Existing.Native.Editor.Object).ConvertToEntityStructure(typeof(PO_LINE_ITEM)).Fields,row=>row.FieldName=="INVENTORY_ITEM_ID");Assert.Equal(128,field.Size1);Assert.True(field.AllowDBNull);
        Assert.DoesNotContain("INVENTORY_ITEM_ID",System.Text.Json.JsonSerializer.Serialize(f.Existing.OrderLine));
        f.Existing.OrderLine.INVENTORY_ITEM_ID="item";
        var linked=PurchaseOrderSourceReview.Create(source.Company,"actor",f.Existing.Order,[f.Existing.OrderLine],[f.Existing.Receipt]);Assert.NotEqual(source.Revision,linked.Revision);
        linked.Lines[0].INVENTORY_ITEM_ID="other";Assert.Throws<InvalidOperationException>(()=>linked.Require(source.Company,"actor","order"));
    }
}
#endif
