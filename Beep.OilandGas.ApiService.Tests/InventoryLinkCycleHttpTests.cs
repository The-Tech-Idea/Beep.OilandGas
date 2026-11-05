#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InventoryLinkCycleHttpTests
{
    [Theory][InlineData("resolved")][InlineData("pending")][InlineData("escaped")][InlineData("unknown")][InlineData("post-commit")][InlineData("grant")][InlineData("inventory-grant")][InlineData("procurement-grant")][InlineData("role")][InlineData("actor")][InlineData("binding")][InlineData("header")][InlineData("revoke-during")][InlineData("roles-changed")][InlineData("grants-changed")][InlineData("route")][InlineData("company")][InlineData("changed-intent")][InlineData("changed-reason")][InlineData("archive-write")][InlineData("archive-corrupt")][InlineData("line-write")]
    [InlineData("history-only")][InlineData("read-grant")][InlineData("read-role")][InlineData("read-binding")][InlineData("read-anonymous")][InlineData("read-action")][InlineData("read-wrong-action")]
    [InlineData("history-inventory-grant")][InlineData("history-procurement-grant")][InlineData("history-role")]
    public async Task CurrentCompanyApiAndTypedClientPreserveOriginalCycleAndCompleteOwnedHistory(string mode)
    {
        var order=mode=="escaped"?"order/a?b":"order";var line=mode=="escaped"?"line/α%2F":"po-line";
        var f=await InventoryLinkCycleTests.Fixture.Create(pending:mode=="pending",orderId:order,lineId:line);var n=f.Native;var baseline=n.Writes;var original=JsonSerializer.Serialize(f.Stored);var reference=f.Stored.INVENTORY_ITEM_ID;
        var inventory=CompanyInventorySourceTests.CreateService(n.Editor,()=>Task.FromResult(n.Connection));
        string[] grants=["Procurement.View","Inventory.View","Procurement.ReopenInventoryLink"];
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="read-anonymous",permissions:grants,configureAccounting:(services,context)=>{services.AddSingleton(inventory);services.AddSingleton(f.Service);services.AddSingleton(f.Existing.Existing.Existing.AP);});
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var root="api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(order)+"/lines/"+Uri.EscapeDataString(line);
        if(mode is "history-inventory-grant" or "history-procurement-grant" or "history-role")
        {
            if(mode=="history-inventory-grant")host.Context.Permissions=["Procurement.View"];
            if(mode=="history-procurement-grant")host.Context.Permissions=["Inventory.View"];
            if(mode=="history-role")host.Context.Roles=["OtherRole"];
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetInventoryLinkHistoryAsync(order,line));Assert.Equal(HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(baseline,n.Writes);return;
        }
        if(mode=="history-only")
        {
            host.Context.Permissions=["Procurement.View","Inventory.View"];
            Assert.Empty((await client.GetInventoryLinkHistoryAsync(order,line)).History.Entries);
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetInventoryLinkCycleReviewAsync(order,line,f.Command.Action));Assert.Equal(HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(baseline,n.Writes);return;
        }
        if(mode.StartsWith("read-",StringComparison.Ordinal))
        {
            if(mode=="read-grant")host.Context.Permissions=["Procurement.View","Inventory.View"];
            if(mode=="read-role")host.Context.Roles=["OtherRole"];if(mode=="read-binding")host.Context.ChangeBinding=true;
            if(mode=="read-action"){var response=await host.Client.GetAsync(root+"/inventory-link-cycle-review/0");Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);}
            else
            {
                var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetInventoryLinkCycleReviewAsync(order,line,mode=="read-wrong-action"?PurchaseOrderInventoryLinkCycleAction.SupersedePending:f.Command.Action));
                Assert.Equal(mode=="read-anonymous"?HttpStatusCode.Unauthorized:mode=="read-wrong-action"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);
            }
            Assert.Equal(baseline,n.Writes);return;
        }
        var review=await client.GetInventoryLinkCycleReviewAsync(order,line,f.Command.Action);
        var command=new PurchaseOrderInventoryLinkCycleCommand(review.Purchase.Company,"actor",order,line,review.Action,review.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),"Reviewed original audit before reopening this cycle.");
        var submission=new PurchaseOrderInventoryLinkCycleSubmission(command,review);
        if(mode=="grant")host.Context.Permissions=["Procurement.View","Inventory.View","Procurement.PrepareInventoryLink","Procurement.RecordInventoryLinkDecision"];
        if(mode=="inventory-grant")host.Context.Permissions=["Procurement.View","Procurement.ReopenInventoryLink"];
        if(mode=="procurement-grant")host.Context.Permissions=["Inventory.View","Procurement.ReopenInventoryLink"];
        if(mode=="role")host.Context.Roles=["OtherRole"];if(mode=="actor")host.Context.Actor="other";
        if(mode=="binding")host.Context.PolicyBinding="changed";if(mode=="header")f.Existing.Existing.Existing.Order.DESCRIPTION="changed";
        if(mode is "roles-changed" or "grants-changed" or "revoke-during")
        {
            var start=host.Context.Calls;host.Context.OnCapture=call=>{if(call<=start+1)return;if(mode=="roles-changed")host.Context.Roles=["ConfiguredCompanyRole","AdditionalRole"];if(mode=="grants-changed")host.Context.Permissions=[..grants,"Unrelated.View"];if(mode=="revoke-during")host.Context.Permissions=["Procurement.View","Inventory.View"];};
        }
        if(mode is "archive-write" or "archive-corrupt")f.Fault=mode;if(mode=="line-write")f.Existing.Fault="write";
        if(mode=="unknown")n.FailCommit=true;if(mode=="post-commit")n.AfterCommit=()=>Assert.Single(n.Rows.OfType<PURCHASE_ORDER>()).DESCRIPTION="changed";
        if(mode is "route" or "company")
        {
            if(mode=="company")submission=submission with{Command=command with{Company=new("tenant","other")}};
            var response=await host.Client.PostAsJsonAsync((mode=="route"?root.Replace("/po-line","/other",StringComparison.Ordinal):root)+"/inventory-link-cycle",submission);
            Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(baseline,n.Writes);return;
        }
        if(mode is "resolved" or "pending" or "escaped" or "unknown" or "post-commit" or "changed-intent" or "changed-reason")
        {
            if(mode is "unknown" or "post-commit")
            {
                var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ReopenInventoryLinkAsync(submission));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);
                n.FailCommit=false;n.AfterCommit=null;if(mode=="post-commit")Assert.Single(n.Rows.OfType<PURCHASE_ORDER>()).DESCRIPTION=review.Purchase.Header.DESCRIPTION;
            }
            var result=await client.ReopenInventoryLinkAsync(submission);submission.RequireResult(result);await client.ReopenInventoryLinkAsync(submission);
            Assert.Equal(original,Assert.Single(result.History.Entries).ARCHIVED_LINE_JSON);Assert.Equal(reference,f.Stored.INVENTORY_ITEM_ID);Assert.Null(f.Stored.INVENTORY_LINK_REVIEW_JSON);Assert.Equal(baseline+2,n.Writes);
            host.Context.Permissions=["Procurement.View","Inventory.View"];var history=await client.GetInventoryLinkHistoryAsync(order,line);Assert.Equal(result.History.Revision,history.History.Revision);Assert.Equal(baseline+2,n.Writes);
            if(mode is "changed-intent" or "changed-reason")
            {
                host.Context.Permissions=grants;var changed=submission with{Command=mode=="changed-intent"?command with{IntentId=Guid.NewGuid().ToString("N")}:command with{Reason="Replacement reason"}};
                var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ReopenInventoryLinkAsync(changed));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(baseline+2,n.Writes);
            }
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ReopenInventoryLinkAsync(submission));
            Assert.Equal(mode is "grant" or "inventory-grant" or "procurement-grant" or "role" or "actor" or "roles-changed" or "grants-changed" or "revoke-during"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
            Assert.Equal(original,JsonSerializer.Serialize(f.Stored));Assert.Empty(n.Rows.OfType<PO_INVENTORY_LINK_HISTORY>());
            Assert.Equal(baseline+(mode is "archive-write" or "archive-corrupt"?1:mode=="line-write"?2:0),n.Writes);
        }
    }
    private sealed class Reply(Func<HttpRequestMessage,HttpResponseMessage> reply):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>Task.FromResult(reply(request));}
    [Theory][InlineData("valid")][InlineData("account")][InlineData("company")][InlineData("reference")][InlineData("cursor")][InlineData("archive")][InlineData("header")][InlineData("null")]
    public async Task TypedCycleClientValidatesCompleteOriginalResultAndDropsLateCompanyOrAccount(string mode)
    {
        var f=await InventoryLinkCycleTests.Fixture.Create();var result=await f.Record();var submission=new PurchaseOrderInventoryLinkCycleSubmission(f.Command,f.Review);
        if(mode=="reference")result.Purchase.Lines[0].INVENTORY_ITEM_ID="changed";if(mode=="cursor")result.Purchase.Lines[0].INVENTORY_LINK_HISTORY_REVISION=null;
        if(mode=="archive")result.History.Entries[0].REASON="changed";if(mode=="header")result.Purchase.Header.DESCRIPTION="changed";
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(f.Command.Company,["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Procurement.ReopenInventoryLink"])));
        using var http=new HttpClient(new Reply(request=>{
            Assert.EndsWith("/order/lines/po-line/inventory-link-cycle",request.RequestUri!.AbsoluteUri);
            if(mode is "company" or "account")company.TrySelect(company.Clear(),mode=="account"?"other":"actor",new(mode=="company"?new("tenant","other"):f.Command.Company,["ConfiguredCompanyRole"],[]));
            return new(HttpStatusCode.OK){Content=mode=="null"?JsonContent.Create<PurchaseOrderInventoryLinkCycleResult?>(null):JsonContent.Create(result)};
        })){BaseAddress=new Uri("https://cycle.test/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="valid")submission.RequireResult(await client.ReopenInventoryLinkAsync(submission));
        else if(mode is "account" or "company")await Assert.ThrowsAsync<OperationCanceledException>(()=>client.ReopenInventoryLinkAsync(submission));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>client.ReopenInventoryLinkAsync(submission));
    }
    [Theory][InlineData("history-valid")][InlineData("history-company")][InlineData("history-actor")][InlineData("history-cursor")][InlineData("history-null")][InlineData("review-valid")][InlineData("review-action")][InlineData("review-revision")][InlineData("review-null")][InlineData("review-account")]
    public async Task TypedHistoryAndCycleReviewRequireOriginalAccountResourcesAndCompleteCanonicalEvidence(string mode)
    {
        var f=await InventoryLinkCycleTests.Fixture.Create();var result=await f.Record();var review=f.Review;
        if(mode=="history-company")result=result with{Purchase=result.Purchase with{Company=new("tenant","other")}};
        if(mode=="history-actor")result=result with{Purchase=result.Purchase with{ActorUserId="other"}};
        if(mode=="history-cursor")result.Purchase.Lines[0].INVENTORY_LINK_HISTORY_REVISION=null;
        if(mode=="review-action")review=review with{Action=PurchaseOrderInventoryLinkCycleAction.SupersedePending};if(mode=="review-revision")review=review with{Revision=new string('A',64)};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(f.Command.Company,["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Procurement.ReopenInventoryLink"])));
        using var http=new HttpClient(new Reply(request=>{
            Assert.EndsWith(mode.StartsWith("history",StringComparison.Ordinal)?"/inventory-link-history":"/inventory-link-cycle-review/ReopenResolved",request.RequestUri!.AbsoluteUri);
            if(mode=="review-account")company.TrySelect(company.Clear(),"other",new(f.Command.Company,["ConfiguredCompanyRole"],[]));
            return new(HttpStatusCode.OK){Content=mode.StartsWith("history",StringComparison.Ordinal)?JsonContent.Create(mode=="history-null"?null:result):JsonContent.Create(mode=="review-null"?null:review)};
        })){BaseAddress=new Uri("https://cycle.test/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        async Task Read(){if(mode.StartsWith("history",StringComparison.Ordinal))await client.GetInventoryLinkHistoryAsync("order","po-line");else await client.GetInventoryLinkCycleReviewAsync("order","po-line",PurchaseOrderInventoryLinkCycleAction.ReopenResolved);}
        if(mode.EndsWith("valid",StringComparison.Ordinal))await Read();else if(mode=="review-account")await Assert.ThrowsAsync<OperationCanceledException>(Read);else await Assert.ThrowsAsync<InvalidOperationException>(Read);
    }
}
#endif
