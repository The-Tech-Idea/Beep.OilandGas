#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InventoryLinkPreparationHttpTests
{
    [Theory][InlineData("valid")][InlineData("escaped")][InlineData("grant")][InlineData("inventory-grant")][InlineData("procurement-grant")][InlineData("role")][InlineData("actor")][InlineData("binding")][InlineData("target")][InlineData("revoke-during")][InlineData("unknown")][InlineData("changed-intent")][InlineData("route")][InlineData("company")]
    [InlineData("read-grant")][InlineData("read-role")][InlineData("read-binding")][InlineData("read-actor")]
    [InlineData("po-read-grant")][InlineData("receiver-read-grant")][InlineData("po-read-valid")]
    public async Task ActualApiAndTypedClientRetainIntentAndEnforceCurrentCompanyAuthority(string mode)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();var n=f.Existing.Native;
        var order=mode=="escaped"?"order/a?b":"order";var line=mode=="escaped"?"line/α%2F":"po-line";
        f.Existing.Order.PURCHASE_ORDER_ID=order;f.Existing.OrderLine.PURCHASE_ORDER_ID=order;f.Existing.OrderLine.PO_LINE_ITEM_ID=line;
        f.Existing.Receipt.PURCHASE_ORDER_ID=order;f.Existing.Receipt.PO_LINE_ITEM_ID=line;
        var metadata=(IPPDMMetadataRepository)typeof(PurchaseOrderService).GetField("_metadata",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f.Existing.PO)!;
        Mock.Get(metadata).Setup(value=>value.GetPrimaryKeyColumnsAsync("PO_LINE_ITEM")).ReturnsAsync(new List<string>{"PO_LINE_ITEM_ID"});
        var item=new INVENTORY_ITEM{INVENTORY_ITEM_ID="target/α%2F",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="bbl",QUANTITY_ON_HAND=null};n.Rows.Add(item);
        var creator=new ClassCreator(n.Editor.Object);
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_ITEM"),true)).Returns(creator.ConvertToEntityStructure(typeof(INVENTORY_ITEM)));
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_TRANSACTION"),true)).Returns(creator.ConvertToEntityStructure(typeof(INVENTORY_TRANSACTION)));
        n.Source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>new[]{item});
        n.Source.Setup(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(Array.Empty<INVENTORY_TRANSACTION>());
        n.Source.Setup(value=>value.UpdateEntity("PO_LINE_ITEM",It.IsAny<object>())).Returns((string table,object value)=>{
            ++n.Writes;var row=CompanyInventorySourceReview.Copy((PO_LINE_ITEM)value);n.Rows.RemoveAll(value=>value is PO_LINE_ITEM existing && existing.PO_LINE_ITEM_ID==line);n.Rows.Add(row);return new ErrorsInfo{Flag=Errors.Ok};});
        var inventory=CompanyInventorySourceTests.CreateService(n.Editor,()=>Task.FromResult(n.Connection));
        string[] grants=["Procurement.View","Inventory.View","Procurement.PrepareInventoryLink"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configureAccounting:(services,context)=>{services.AddSingleton(inventory);services.AddSingleton(f.Existing.PO);services.AddSingleton(f.Existing.AP);});
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode.StartsWith("read-",StringComparison.Ordinal))
        {
            if(mode=="read-grant")host.Context.Permissions=["Procurement.View","Inventory.View"];
            if(mode=="read-role")host.Context.Roles=["OtherRole"];
            if(mode=="read-binding")host.Context.ChangeBinding=true;
            if(mode=="read-actor")host.Context.Actor="other";
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetInventoryLinkPreparationAsync(order,line,item.INVENTORY_ITEM_ID));
            Assert.Equal(mode=="read-binding"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(0,n.Writes);return;
        }
        var review=await client.GetInventoryLinkPreparationAsync(order,line,item.INVENTORY_ITEM_ID);Assert.Null(review.Target.Item.QUANTITY_ON_HAND);Assert.Equal(0,n.Writes);
        var command=new PurchaseOrderInventoryLinkProposalCommand(review.Purchase.Company,"actor",order,line,item.INVENTORY_ITEM_ID,review.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),"Reviewed warehouse, location and unit.");
        var submission=new PurchaseOrderInventoryLinkProposalSubmission(command,review);
        if(mode=="grant")host.Context.Permissions=["Procurement.View","Inventory.View"];
        if(mode=="inventory-grant")host.Context.Permissions=["Procurement.View","Procurement.PrepareInventoryLink"];
        if(mode=="procurement-grant")host.Context.Permissions=["Inventory.View","Procurement.PrepareInventoryLink"];
        if(mode=="role")host.Context.Roles=["OtherRole"];
        if(mode=="actor")host.Context.Actor="other";
        if(mode=="binding")host.Context.PolicyBinding="changed";
        if(mode=="target")item.LOCATION_ID="changed";
        if(mode=="company")submission=submission with{Command=command with{Company=new("tenant","other")}};
        if(mode=="revoke-during"){var start=host.Context.Calls;host.Context.OnCapture=call=>{if(call>start+1)host.Context.Permissions=["Procurement.View","Inventory.View"];};}
        if(mode=="unknown")n.FailCommit=true;
        if(mode=="route")
        {
            var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/inventory/company/purchase-orders/order/lines/other/inventory-link-preparation",submission);
            Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(0,n.Writes);return;
        }
        if(mode is "valid" or "escaped" or "unknown" or "changed-intent" or "po-read-grant" or "receiver-read-grant" or "po-read-valid")
        {
            if(mode=="unknown"){await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareInventoryLinkAsync(submission));n.FailCommit=false;}
            var result=await client.PrepareInventoryLinkAsync(submission);submission.RequireResult(result);Assert.Null(result.INVENTORY_ITEM_ID);
            await client.PrepareInventoryLinkAsync(submission);Assert.Equal(1,n.Writes);
            if(mode is "po-read-grant" or "receiver-read-grant" or "po-read-valid")
            {
                if(mode!="po-read-valid")host.Context.Permissions=["Procurement.View"];
                var route="api/accounting/traditional/purchase-order/order"+(mode=="receiver-read-grant"?"/receipts/receipt/decision-review":"");
                var response=await host.Client.GetAsync(route);Assert.Equal(mode=="po-read-valid"?HttpStatusCode.OK:HttpStatusCode.Forbidden,response.StatusCode);
                if(mode!="po-read-valid")Assert.DoesNotContain("target/",await response.Content.ReadAsStringAsync());
            }
            if(mode=="changed-intent")
            {
                var changed=submission with{Command=command with{IntentId=Guid.NewGuid().ToString("N")}};
                var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareInventoryLinkAsync(changed));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(1,n.Writes);
            }
        }
        else if(mode=="company")
        {
            var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/inventory/company/purchase-orders/order/lines/po-line/inventory-link-preparation",submission);
            Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>client.PrepareInventoryLinkAsync(submission));Assert.Equal(0,n.Writes);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareInventoryLinkAsync(submission));
            Assert.Equal(mode is "grant" or "inventory-grant" or "procurement-grant" or "role" or "actor" or "revoke-during"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(0,n.Writes);
        }
        Assert.Null(Assert.Single(n.Rows.OfType<PO_RECEIPT>()).RECEIVER_DECISION_JSON);
    }
    private sealed class Reply(Func<HttpRequestMessage,HttpResponseMessage> reply):HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>Task.FromResult(reply(request));
    }
    [Theory][InlineData("valid")][InlineData("account")][InlineData("company")][InlineData("intent")][InlineData("line")][InlineData("target")][InlineData("missing-proof")]
    public async Task TypedClientConfirmsOnlyTheOriginalPreparedLineAndIntent(string mode)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();var scope=new CompanyScope("tenant","company");
        var purchase=(await f.Existing.PO.GetSourceReviewAsync(scope,"order","actor"))!;
        var item=new INVENTORY_ITEM{INVENTORY_ITEM_ID="target",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="bbl"};
        var review=PurchaseOrderInventoryLinkProposalReview.Create(purchase,CompanyInventorySourceReview.Create(scope,"actor",item,[]),"po-line","binding");
        var command=new PurchaseOrderInventoryLinkProposalCommand(scope,"actor","order","po-line","target",review.Revision,"binding",Guid.NewGuid().ToString("N"),"Reviewed warehouse.");
        var submission=new PurchaseOrderInventoryLinkProposalSubmission(command,review);
        var row=CompanyInventorySourceReview.Copy(purchase.Lines[0]);row.ROW_CHANGED_BY="actor";row.ROW_CHANGED_DATE=DateTime.UtcNow;
        var recorded=mode=="intent"?command with{IntentId=Guid.NewGuid().ToString("N")}:command;
        row.INVENTORY_LINK_REVIEW_JSON=PurchaseOrderInventoryLinkProposalProof.Create(recorded,review,row);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(scope,["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Procurement.PrepareInventoryLink"])));
        using var http=new HttpClient(new Reply(request=>{
            if(mode is "account" or "company")company.TrySelect(company.Clear(),mode=="account"?"other":"actor",new(mode=="company"?new("tenant","other"):scope,["ConfiguredCompanyRole"],[]));
            if(mode=="line")row.PO_LINE_ITEM_ID="other";if(mode=="target")row.INVENTORY_ITEM_ID="target";if(mode=="missing-proof")row.INVENTORY_LINK_REVIEW_JSON=null;
            return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        })){BaseAddress=new Uri("https://preparation.test/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="valid")submission.RequireResult(await client.PrepareInventoryLinkAsync(submission));
        else if(mode is "account" or "company")await Assert.ThrowsAsync<OperationCanceledException>(()=>client.PrepareInventoryLinkAsync(submission));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>client.PrepareInventoryLinkAsync(submission));
    }
}
#endif
