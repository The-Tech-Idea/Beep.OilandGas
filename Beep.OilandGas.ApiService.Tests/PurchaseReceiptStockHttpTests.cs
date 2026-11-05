using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PurchaseReceiptStockHttpTests
{
    [Theory][InlineData("page")][InlineData("size")][InlineData("total")][InlineData("revision")][InlineData("null")][InlineData("account")][InlineData("company")]
    public async Task TypedStockPageClientRefusesMalformedMetadataAndLateContext(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();var result=await f.Inventory.GetStockRecoveryPageAsync(new("tenant","company"),"actor",1,25,f.Receiving.Existing.PO,f.Authority);
        if(mode=="page")result=result with{PageNumber=2};if(mode=="size")result=result with{PageSize=26};if(mode=="total")result=result with{TotalRecords=0};if(mode=="revision")result=result with{Revision=new string('a',64)};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View"])));
        using var http=new HttpClient(new Reply(request=>{
            Assert.EndsWith("/stock-intents/pages/1/25",request.RequestUri!.AbsoluteUri);
            if(mode is "account" or "company")Assert.True(company.TrySelect(company.Clear(),mode=="account"?"other":"actor",new(new("tenant",mode=="company"?"other":"company"),["ConfiguredCompanyRole"],["Inventory.View"])));
            return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};
        })){BaseAddress=new Uri("https://fixture/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        await Assert.ThrowsAnyAsync<Exception>(()=>client.GetCompanyStockRecoveryPageAsync(1,25));Assert.Equal(3,f.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("bad-page")]
    public async Task TypedClientAndCompanyApiReadOnlyTheRequestedOriginalStockPage(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();await f.Prepare(quantity:4);await f.Record();var writes=f.Native.Writes;
        string[] grants=["Procurement.View","Inventory.View"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configureAccounting:(services,context)=>{services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.PO);services.AddSingleton(f.Receiving.Existing.AP);});
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="grant"){host.Context.Permissions=["Inventory.View"];Assert.Equal(HttpStatusCode.Forbidden,(await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyStockRecoveryPageAsync(2,1))).StatusCode);}
        else if(mode=="bad-page")Assert.Equal(HttpStatusCode.BadRequest,(await host.Client.GetAsync("api/accounting/traditional/inventory/company/stock-intents/pages/0/51")).StatusCode);
        else
        {
            var first=await client.GetCompanyStockRecoveryPageAsync(1,1);var second=await client.GetCompanyStockRecoveryPageAsync(2,1);
            Assert.Equal(2,first.TotalRecords);Assert.Equal(2,second.TotalRecords);Assert.NotEqual(first.Index.Entries.Single().Command.IntentId,second.Index.Entries.Single().Command.IntentId);
            second.Require(new("tenant","company"),"actor",2,1);Assert.False(second.HasNextPage);
        }
        Assert.Equal(writes,f.Native.Writes);
    }
    [Theory][InlineData("exact")][InlineData("empty")][InlineData("later")][InlineData("foreign")][InlineData("grant")][InlineData("duplicate")][InlineData("altered")][InlineData("authority")][InlineData("limit")][InlineData("membership")][InlineData("source-drift")]
    public async Task ActualStockDiscoveryIndexIncludesOnlyValidatedOriginalAccountEvidence(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();var original=f.Submission;
        if(mode!="empty")await f.Record();if(mode=="later"){await f.Prepare(quantity:4);await f.Record();}
        if(mode=="foreign")f.Native.Rows.Add(new INVENTORY_STOCK_INTENT{INVENTORY_STOCK_INTENT_ID=Guid.NewGuid().ToString("N"),OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ROW_CREATED_BY="other"});
        if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Single()));
        if(mode=="altered")f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Single().COMMAND_JSON="{}";
        if(mode=="limit")for(var i=0;i<PurchaseReceiptStockIntentIndex.MaximumEntries;i++)f.Native.Rows.Add(new INVENTORY_STOCK_INTENT{INVENTORY_STOCK_INTENT_ID=Guid.NewGuid().ToString("N"),OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ROW_CREATED_BY="actor"});
        if(mode is "membership" or "source-drift")
        {
            var lists=0;f.Native.Source.Setup(value=>value.GetEntityAsync("INVENTORY_STOCK_INTENT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>{
                if(filters.Any(filter=>filter.FieldName=="ROW_CREATED_BY"))
                {
                    lists++;if(mode=="membership" && lists==2)f.Native.Rows.RemoveAll(row=>row is INVENTORY_STOCK_INTENT);
                    if(mode=="source-drift" && lists==3)f.Item.LOCATION_ID="changed during discovery";
                }
                return f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Where(row=>filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>(object)CompanyInventorySourceReview.Copy(row)).ToArray();
            });
        }
        var baseline=f.Native.Writes;string[] grants=["Procurement.View","Inventory.View"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configureAccounting:(services,context)=>{services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.PO);services.AddSingleton(f.Receiving.Existing.AP);});
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="grant")host.Context.Permissions=["Inventory.View"];
        if(mode=="authority"){var start=host.Context.Calls;host.Context.OnCapture=call=>{if(call>start+1)host.Context.PolicyBinding="changed";};}
        if(mode is "grant" or "authority" or "duplicate" or "altered" or "limit" or "membership" or "source-drift")
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyStockRecoveryIndexAsync());Assert.Equal(mode is "grant" or "authority"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
        }
        else
        {
            var result=await client.GetCompanyStockRecoveryIndexAsync();result.Require(new("tenant","company"),"actor");Assert.Equal(mode=="empty"?0:mode=="later"?2:1,result.Entries.Count);
            if(mode!="empty"){var entry=result.Entries.Single(row=>row.Command.IntentId==original.Command.IntentId);Assert.Equal(original.Command,entry.Command);Assert.Equal(mode!="later",entry.CurrentMatchesOriginalEffect);}
        }
        Assert.Equal(baseline,f.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("null")][InlineData("entry")][InlineData("revision")][InlineData("scope")][InlineData("actor")][InlineData("duplicate")][InlineData("account-change")][InlineData("company-change")]
    public async Task TypedStockDiscoveryClientRejectsMalformedOrLateAccountEvidence(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();
        var result=await f.Inventory.GetStockRecoveryIndexAsync(new("tenant","company"),"actor",f.Receiving.Existing.PO,f.Authority);
        if(mode=="entry")result=result with{Entries=[result.Entries[0] with{ItemId=""}]};if(mode=="revision")result=result with{Revision=new string('a',64)};
        if(mode=="scope")result=result with{Company=new("tenant","other")};if(mode=="actor")result=result with{ActorUserId="other"};if(mode=="duplicate")result=result with{Entries=[result.Entries[0],result.Entries[0]]};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View"])));
        using var http=new HttpClient(new Reply(request=>{
            Assert.Equal(HttpMethod.Get,request.Method);Assert.EndsWith("/company/stock-intents",request.RequestUri!.AbsoluteUri);
            if(mode is "account-change" or "company-change")Assert.True(company.TrySelect(company.Clear(),mode=="account-change"?"other":"actor",new(new("tenant",mode=="company-change"?"other":"company"),["ConfiguredCompanyRole"],["Inventory.View"])));
            return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};
        })){BaseAddress=new Uri("https://fixture/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="valid")Assert.Single((await client.GetCompanyStockRecoveryIndexAsync()).Entries);
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.GetCompanyStockRecoveryIndexAsync());Assert.Equal(3,f.Native.Writes);
    }
    [Theory][InlineData("exact")][InlineData("item-changed")][InlineData("purchase-changed")][InlineData("later-receipt")][InlineData("unknown")]
    [InlineData("role")][InlineData("inventory-grant")][InlineData("procurement-grant")][InlineData("actor")][InlineData("company")]
    [InlineData("roles-changed")][InlineData("grants-changed")][InlineData("binding-changed")][InlineData("anonymous")]
    [InlineData("missing")][InlineData("duplicate")][InlineData("altered")][InlineData("bad-intent")][InlineData("foreign-scope")][InlineData("recorded-actor")]
    [InlineData("purchase-unavailable")][InlineData("item-unavailable")][InlineData("both-unavailable")]
    public async Task ActualCompanyApiAndTypedClientRecoverSavedIntentWithoutMutation(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();var original=CompanyInventorySourceReview.Copy(f.Submission);
        if(mode=="unknown"){f.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());f.Native.FailCommit=false;}else await f.Record();
        if(mode=="item-changed")f.Item.LOCATION_ID="later bin";if(mode=="purchase-changed")f.Receiving.Existing.Order.DESCRIPTION="Later order";
        if(mode=="later-receipt"){await f.Prepare(quantity:4);await f.Record();}
        if(mode is "purchase-unavailable" or "both-unavailable")f.Native.Rows.RemoveAll(row=>row is PURCHASE_ORDER);
        if(mode is "item-unavailable" or "both-unavailable")f.Native.Rows.RemoveAll(row=>row is INVENTORY_ITEM);
        var entry=f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Single(row=>row.INVENTORY_STOCK_INTENT_ID==original.Command.IntentId);
        if(mode=="missing")f.Native.Rows.Remove(entry);if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(entry));if(mode=="altered")entry.ORIGINAL_ITEM_JSON="{}";
        var baseline=f.Native.Writes;
        // Recovery is a read: mutation grants are deliberately absent.
        string[] grants=["Procurement.View","Inventory.View"];
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="anonymous",permissions:grants,configureAccounting:(services,context)=>{
            services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.PO);services.AddSingleton(f.Receiving.Existing.AP);
        });
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant",mode=="foreign-scope"?"other":"company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="role")host.Context.Roles=["OtherRole"];if(mode=="inventory-grant")host.Context.Permissions=["Procurement.View"];if(mode=="procurement-grant")host.Context.Permissions=["Inventory.View"];
        if(mode=="actor")host.Context.Actor="other";if(mode=="company")entry.OWNER_COMPANY_BA_ID="other";if(mode=="recorded-actor")entry.ROW_CREATED_BY="other";
        if(mode is "roles-changed" or "grants-changed" or "binding-changed")
        {
            var start=host.Context.Calls;host.Context.OnCapture=call=>{if(call<=start+1)return;
                if(mode=="roles-changed")host.Context.Roles=["ConfiguredCompanyRole","AdditionalRole"];
                if(mode=="grants-changed")host.Context.Permissions=[..grants,"Unrelated.View"];if(mode=="binding-changed")host.Context.PolicyBinding="changed";
            };
        }
        if(mode=="bad-intent")Assert.Equal(HttpStatusCode.BadRequest,(await host.Client.GetAsync("api/accounting/traditional/inventory/company/stock-intents/invalid")).StatusCode);
        else if(mode is "exact" or "item-changed" or "purchase-changed" or "later-receipt" or "unknown" or "purchase-unavailable" or "item-unavailable" or "both-unavailable")
        {
            var result=await client.GetPurchaseReceiptStockRecoveryAsync(original.Command.IntentId);
            Assert.Equal(original.Command,result.OriginalSubmission().Command);Assert.Equal(original.Review.Revision,result.OriginalSubmission().Review.Revision);
            Assert.Equal(mode is "exact" or "unknown",result.CurrentMatchesOriginalEffect());
            if(mode is "item-unavailable" or "both-unavailable")Assert.Null(result.CurrentInventory);else Assert.Equal(mode=="later-receipt"?18m:14m,result.CurrentInventory!.Item.QUANTITY_ON_HAND);
            result.Entry.COMMAND_JSON="caller mutation";Assert.Equal(original.Command,(await client.GetPurchaseReceiptStockRecoveryAsync(original.Command.IntentId)).OriginalSubmission().Command);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetPurchaseReceiptStockRecoveryAsync(original.Command.IntentId));
            Assert.Equal(mode=="anonymous"?HttpStatusCode.Unauthorized:mode is "actor" or "role" or "inventory-grant" or "procurement-grant" or "roles-changed" or "grants-changed" or "binding-changed"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
        }
        Assert.Equal(baseline,f.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("null")][InlineData("entry")][InlineData("source")][InlineData("history")][InlineData("revision")][InlineData("intent")][InlineData("account")][InlineData("company")][InlineData("bad-intent")]
    public async Task TypedRecoveryClientValidatesCompleteSavedEvidenceAndOriginalContext(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();var intent=f.Submission.Command.IntentId;
        var result=await f.Inventory.GetStockRecoveryAsync(new("tenant","company"),intent,"actor",f.Receiving.Existing.PO,f.Authority);
        if(mode=="entry")result.Entry.ORIGINAL_ITEM_JSON="{}";if(mode=="source")result.CurrentInventory.Item.LOCATION_ID="replaced";
        if(mode=="history")result.CurrentInventory.Transactions.Single().QUANTITY=999;
        if(mode=="revision")result=result with{Revision=new string('a',64)};if(mode=="intent")result.Entry.INVENTORY_STOCK_INTENT_ID=Guid.NewGuid().ToString("N");
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View"])));
        var requests=0;using var http=new HttpClient(new Reply(request=>{
            requests++;Assert.Equal(HttpMethod.Get,request.Method);Assert.EndsWith("/stock-intents/"+intent,request.RequestUri!.AbsoluteUri);
            if(mode is "account" or "company")Assert.True(company.TrySelect(company.Clear(),mode=="account"?"other":"actor",new(new("tenant",mode=="company"?"other":"company"),["ConfiguredCompanyRole"],["Inventory.View"])));
            return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};
        })){BaseAddress=new Uri("https://fixture/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="valid")Assert.Equal(f.Submission.Command,(await client.GetPurchaseReceiptStockRecoveryAsync(intent)).OriginalSubmission().Command);
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.GetPurchaseReceiptStockRecoveryAsync(mode=="bad-intent"?"bad":intent));
        Assert.Equal(mode=="bad-intent"?0:1,requests);Assert.Equal(3,f.Native.Writes);
    }
    private sealed class Reply(Func<HttpRequestMessage,HttpResponseMessage> reply):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>Task.FromResult(reply(request));}
    [Theory][InlineData("valid")][InlineData("item")][InlineData("history")][InlineData("proof")][InlineData("missing-movement")][InlineData("account")][InlineData("company")][InlineData("null")]
    public async Task TypedStockClientRequiresCompleteOriginalPostimageAndDropsLateContext(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();var submission=CompanyInventorySourceReview.Copy(f.Submission);var result=await f.Record();
        var item=CompanyInventorySourceReview.Copy(result.Inventory.Item);var rows=result.Inventory.Transactions.Select(CompanyInventorySourceReview.Copy).ToList();
        if(mode=="item")item.QUANTITY_ON_HAND=15m;if(mode=="proof")rows[0].PURCHASE_RECEIPT_STOCK_JSON="{}";
        if(mode=="history")rows.Add(new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="extra",INVENTORY_ITEM_ID="item",TRANSACTION_TYPE="USAGE",QUANTITY=-1m,ACTIVE_IND="Y"});
        if(mode=="missing-movement")rows.Clear();
        result=PurchaseReceiptInventoryReview.Create(result.Purchase,CompanyInventorySourceReview.Create(result.Inventory.Company,result.Inventory.ActorUserId,item,rows),result.ReceiptId,result.BindingRevision);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Inventory.RecordReceiptStock"])));
        using var http=new HttpClient(new Reply(request=>{
            Assert.EndsWith("/order/receipts/receipt/stock-movement",request.RequestUri!.AbsoluteUri);Assert.Equal(HttpMethod.Post,request.Method);
            var payload=JsonSerializer.Deserialize<PurchaseReceiptStockSubmission>(request.Content!.ReadAsStringAsync().GetAwaiter().GetResult(),new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            payload.Require();Assert.Equal(submission.Command,payload.Command);
            // The client has already frozen the caller's mutable source objects.
            f.Submission.Review.Source.Inventory.Item.LOCATION_ID="changed caller";
            if(mode is "account" or "company")Assert.True(company.TrySelect(company.Clear(),mode=="account"?"other":"actor",new(new("tenant",mode=="company"?"other":"company"),["ConfiguredCompanyRole"],["Inventory.View"])));
            return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};
        })){BaseAddress=new Uri("https://fixture/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="valid")submission.RequireResult(await client.RecordPurchaseReceiptStockAsync(f.Submission));
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.RecordPurchaseReceiptStockAsync(f.Submission));
        Assert.Equal(3,f.Native.Writes);
    }
    [Theory][InlineData("receive")][InlineData("return")][InlineData("escaped")][InlineData("unknown")]
    [InlineData("grant")][InlineData("wrong-receive-grant")][InlineData("wrong-return-grant")][InlineData("inventory-grant")][InlineData("procurement-grant")]
    [InlineData("role")][InlineData("actor")][InlineData("binding")][InlineData("route")][InlineData("source")][InlineData("anonymous")]
    [InlineData("roles-changed")][InlineData("grants-changed")][InlineData("revoke-insert")][InlineData("revoke-commit")]
    public async Task ActualTypedClientCompanyApiAndNativeCommandRequireSpecificCurrentAuthority(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();if(mode=="escaped"){f.OrderId="order/a?b";f.ReceiptId="receipt/α%2F";}
        await f.Initialize();var n=f.Native;
        var returning=mode is "return" or "wrong-return-grant";
        if(returning){await f.Record();await f.Prepare(PurchaseReceiptStockMovementKind.Return,1m,f.Submission.Command.IntentId);}
        var baseline=n.Writes;
        string[] grants=["Procurement.View","Inventory.View","Inventory.RecordReceiptStock","Inventory.RecordSupplierReturn"];
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="anonymous",permissions:grants,configureAccounting:(services,context)=>{
            services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.PO);services.AddSingleton(f.Receiving.Existing.AP);
        });
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var submission=f.Submission;
        if(mode!="anonymous")
        {
            // Read grants alone may inspect the source; they do not authorize either command.
            host.Context.Permissions=["Procurement.View","Inventory.View"];
            var review=await client.GetPurchaseReceiptStockReviewAsync(f.OrderId,f.ReceiptId);
            submission=submission with{Review=review,Command=submission.Command with{ExpectedReviewRevision=review.Revision}};
            host.Context.Permissions=grants;
        }
        if(mode=="grant")host.Context.Permissions=["Procurement.View","Inventory.View"];
        if(mode=="wrong-receive-grant")host.Context.Permissions=["Procurement.View","Inventory.View","Inventory.RecordSupplierReturn"];
        if(mode=="wrong-return-grant")host.Context.Permissions=["Procurement.View","Inventory.View","Inventory.RecordReceiptStock"];
        if(mode=="inventory-grant")host.Context.Permissions=grants.Where(value=>value!="Inventory.View").ToArray();
        if(mode=="procurement-grant")host.Context.Permissions=grants.Where(value=>value!="Procurement.View").ToArray();
        if(mode=="role")host.Context.Roles=["OtherRole"];if(mode=="actor")host.Context.Actor="other";if(mode=="binding")host.Context.PolicyBinding="changed";
        if(mode=="source")f.Item.LOCATION_ID="changed";
        if(mode is "roles-changed" or "grants-changed")
        {
            var start=host.Context.Calls;host.Context.OnCapture=call=>{if(call<=start+1)return;if(mode=="roles-changed")host.Context.Roles=["ConfiguredCompanyRole","AdditionalRole"];else host.Context.Permissions=[..grants,"Unrelated.View"];};
        }
        if(mode=="revoke-insert")f.OnInsert=()=>host.Context.Permissions=["Procurement.View","Inventory.View"];
        if(mode=="revoke-commit")n.AfterCommit=()=>host.Context.Permissions=["Procurement.View","Inventory.View"];
        if(mode=="unknown")n.FailCommit=true;
        if(mode=="route")
        {
            var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/inventory/company/purchase-orders/order/receipts/other/stock-movement",submission);
            Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(baseline,n.Writes);return;
        }
        if(mode is "receive" or "return" or "escaped" or "unknown" or "revoke-commit")
        {
            if(mode is "unknown" or "revoke-commit")
            {
                var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordPurchaseReceiptStockAsync(submission));
                Assert.Equal(mode=="unknown"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);
                n.FailCommit=false;n.AfterCommit=null;host.Context.Permissions=grants;
            }
            var result=await client.RecordPurchaseReceiptStockAsync(submission);submission.RequireResult(result);
            await client.RecordPurchaseReceiptStockAsync(submission);Assert.Equal(baseline+3,n.Writes);Assert.Equal(returning?13m:14m,f.Item.QUANTITY_ON_HAND);
            Assert.Equal("DRAFT",f.Receiving.Existing.Invoice.STATUS);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordPurchaseReceiptStockAsync(submission));
            Assert.Equal(mode=="anonymous"?HttpStatusCode.Unauthorized:mode is "binding" or "source"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);
            Assert.Equal(baseline+(mode=="revoke-insert"?1:0),n.Writes);Assert.Equal(returning?14m:10m,f.Item.QUANTITY_ON_HAND);
            if(mode=="revoke-insert")Assert.Empty(n.Rows.OfType<INVENTORY_TRANSACTION>());
        }
    }
}
#endif
