using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InventoryLinkDecisionHttpTests
{
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("escaped")][InlineData("unknown")][InlineData("post-commit")][InlineData("grant")][InlineData("inventory-grant")][InlineData("procurement-grant")][InlineData("role")][InlineData("actor")][InlineData("binding")][InlineData("target")][InlineData("header")][InlineData("revoke-during")][InlineData("roles-changed")][InlineData("grants-changed")][InlineData("route")][InlineData("company")][InlineData("changed-intent")][InlineData("changed-reason")][InlineData("write")][InlineData("corrupt")]
    [InlineData("read-grant")][InlineData("read-role")][InlineData("read-binding")][InlineData("read-preparer")][InlineData("read-anonymous")]
    [InlineData("write-preparer")]
    [InlineData("resolution")][InlineData("resolution-binding")][InlineData("resolution-intent")][InlineData("resolution-grant")][InlineData("resolution-header")]
    public async Task ActualApiAndTypedClientRequireIndependentCurrentReviewerAndPreserveExactIntent(string mode)
    {
        var order=mode=="escaped"?"order/a?b":"order";var line=mode=="escaped"?"line/α%2F":"po-line";
        var f=await InventoryLinkDecisionTests.Fixture.Create(receiving:true,orderId:order,lineId:line,itemId:"target/α%2F");var n=f.Existing.Existing.Native;var baseline=n.Writes;
        var receipt=System.Text.Json.JsonSerializer.Serialize(f.Existing.Stored);var proposal=f.Stored.INVENTORY_LINK_REVIEW_JSON;
        n.Rows.Add(f.Item);var creator=new ClassCreator(n.Editor.Object);
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_ITEM"),true)).Returns(creator.ConvertToEntityStructure(typeof(INVENTORY_ITEM)));
        n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_TRANSACTION"),true)).Returns(creator.ConvertToEntityStructure(typeof(INVENTORY_TRANSACTION)));
        n.Source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>new[]{f.Item});
        n.Source.Setup(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(Array.Empty<INVENTORY_TRANSACTION>());
        var inventory=CompanyInventorySourceTests.CreateService(n.Editor,()=>Task.FromResult(n.Connection));
        string[] grants=["Procurement.View","Inventory.View","Procurement.RecordInventoryLinkDecision"];
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="read-anonymous",permissions:grants,configureAccounting:(services,context)=>{services.AddSingleton(inventory);services.AddSingleton(f.Existing.Existing.PO);services.AddSingleton(f.Existing.Existing.AP);context.Actor="reviewer";});
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"reviewer",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode.StartsWith("read-",StringComparison.Ordinal))
        {
            if(mode=="read-grant")host.Context.Permissions=["Procurement.View","Inventory.View","Procurement.PrepareInventoryLink"];
            if(mode=="read-role")host.Context.Roles=["OtherRole"];if(mode=="read-binding")host.Context.ChangeBinding=true;
            if(mode=="read-preparer"){host.Context.Actor="actor";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","actor");}
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetInventoryLinkDecisionReviewAsync(order,line));
            Assert.Equal(mode=="read-anonymous"?HttpStatusCode.Unauthorized:mode is "read-binding" or "read-preparer"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(baseline,n.Writes);return;
        }
        var review=await client.GetInventoryLinkDecisionReviewAsync(order,line);Assert.Equal(f.Item.INVENTORY_ITEM_ID,review.Target.Item.INVENTORY_ITEM_ID);Assert.Null(review.Target.Item.QUANTITY_ON_HAND);
        var command=new PurchaseOrderInventoryLinkDecisionCommand(review.Purchase.Company,"reviewer",order,line,f.Item.INVENTORY_ITEM_ID,mode=="reject"?PurchaseOrderInventoryLinkDecision.Reject:PurchaseOrderInventoryLinkDecision.Approve,review.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),"Independently reviewed source and recorded target.");
        var submission=new PurchaseOrderInventoryLinkDecisionSubmission(command,review);
        if(mode=="grant")host.Context.Permissions=["Procurement.View","Inventory.View","Procurement.PrepareInventoryLink"];
        if(mode=="inventory-grant")host.Context.Permissions=["Procurement.View","Procurement.RecordInventoryLinkDecision"];
        if(mode=="procurement-grant")host.Context.Permissions=["Inventory.View","Procurement.RecordInventoryLinkDecision"];
        if(mode=="role")host.Context.Roles=["OtherRole"];if(mode=="actor")host.Context.Actor="other";
        if(mode=="binding")host.Context.PolicyBinding="changed";if(mode=="target")f.Item.LOCATION_ID="changed";if(mode=="header")f.Existing.Existing.Order.DESCRIPTION="changed";
        if(mode=="company")submission=submission with{Command=command with{Company=new("tenant","other")}};
        if(mode is "revoke-during" or "roles-changed" or "grants-changed")
        {
            var start=host.Context.Calls;host.Context.OnCapture=call=>{if(call<=start+1)return;
                if(mode=="revoke-during")host.Context.Permissions=["Procurement.View","Inventory.View"];
                if(mode=="roles-changed")host.Context.Roles=["ConfiguredCompanyRole","AdditionalRole"];
                if(mode=="grants-changed")host.Context.Permissions=[..grants,"Unrelated.View"];};
        }
        if(mode is "write" or "corrupt")f.Fault=mode;
        if(mode=="unknown")n.FailCommit=true;if(mode=="post-commit")n.AfterCommit=()=>f.Item.LOCATION_ID="changed";
        var route="api/accounting/traditional/inventory/company/purchase-orders/"+Uri.EscapeDataString(order)+"/lines/"+Uri.EscapeDataString(line)+"/inventory-link-decision";
        if(mode is "route" or "company" or "write-preparer")
        {
            if(mode=="write-preparer")
            {
                host.Context.Actor="actor";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","actor");
                submission=submission with{Command=command with{ActorUserId="actor"},Review=review with{Purchase=review.Purchase with{ActorUserId="actor"},Target=review.Target with{ActorUserId="actor"}}};
            }
            var response=await host.Client.PostAsJsonAsync(mode=="route"?route.Replace("/po-line/","/other/",StringComparison.Ordinal):route,submission);
            Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(baseline,n.Writes);return;
        }
        if(mode is "approve" or "reject" or "escaped" or "unknown" or "post-commit" or "changed-intent" or "changed-reason" || mode.StartsWith("resolution",StringComparison.Ordinal))
        {
            if(mode is "unknown" or "post-commit")
            {var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordInventoryLinkDecisionAsync(submission));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);n.FailCommit=false;n.AfterCommit=null;f.Item.LOCATION_ID="bin";}
            var result=await client.RecordInventoryLinkDecisionAsync(submission);submission.RequireResult(result);Assert.Equal(mode=="reject"?"current-item":f.Item.INVENTORY_ITEM_ID,result.INVENTORY_ITEM_ID);
            await client.RecordInventoryLinkDecisionAsync(submission);Assert.Equal(baseline+1,n.Writes);
            var resolved=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetInventoryLinkDecisionReviewAsync(order,line));Assert.Equal(HttpStatusCode.Conflict,resolved.StatusCode);
            if(mode is "changed-intent" or "changed-reason")
            {
                var changed=submission with{Command=mode=="changed-intent"?command with{IntentId=Guid.NewGuid().ToString("N")}:command with{Reason="Changed reviewer reason"}};
                var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordInventoryLinkDecisionAsync(changed));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(baseline+1,n.Writes);
            }
            if(mode!="reject")
            {var linked=await client.GetPurchaseReceiptInventoryReviewAsync(order,"receipt");Assert.Equal(f.Item.INVENTORY_ITEM_ID,linked.Inventory.Item.INVENTORY_ITEM_ID);Assert.Null(linked.Inventory.Item.QUANTITY_ON_HAND);}
            if(mode.StartsWith("resolution",StringComparison.Ordinal))
            {
                host.Context.Actor="actor";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","actor");
                host.Context.Permissions=mode=="resolution-grant"?["Procurement.View"]:["Procurement.View","Inventory.View"];
                Assert.True(company.TrySelect(company.Clear(),"actor",new(command.Company,["ConfiguredCompanyRole"],host.Context.Permissions)));
                var original=new PurchaseOrderInventoryLinkProposalSubmission(f.Preparation,f.PreparationReview);
                if(mode=="resolution-binding")host.Context.PolicyBinding="changed";if(mode=="resolution-header")f.Existing.Existing.Order.DESCRIPTION="changed";
                if(mode=="resolution-intent")original=original with{Command=f.Preparation with{IntentId=Guid.NewGuid().ToString("N")}};
                if(mode=="resolution"){var resolution=await client.GetInventoryLinkResolutionAsync(original);resolution.Require(original);Assert.Equal("actor",resolution.Purchase.ActorUserId);}
                else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetInventoryLinkResolutionAsync(original));Assert.Equal(mode=="resolution-grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);}
                Assert.Equal(baseline+1,n.Writes);
            }
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordInventoryLinkDecisionAsync(submission));
            Assert.Equal(mode is "grant" or "inventory-grant" or "procurement-grant" or "role" or "actor" or "revoke-during" or "roles-changed" or "grants-changed"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
            Assert.Equal(mode is "write" or "corrupt"?baseline+1:baseline,n.Writes);Assert.Null(f.Stored.INVENTORY_LINK_DECISION_JSON);Assert.Equal("current-item",f.Stored.INVENTORY_ITEM_ID);
        }
        Assert.Equal(proposal,f.Stored.INVENTORY_LINK_REVIEW_JSON);Assert.Equal(receipt,System.Text.Json.JsonSerializer.Serialize(f.Existing.Stored));
    }
    private sealed class Reply(Func<HttpRequestMessage,HttpResponseMessage> reply):HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>Task.FromResult(reply(request));
    }
    [Theory][InlineData("valid")][InlineData("account")][InlineData("company")][InlineData("intent")][InlineData("decision")][InlineData("reference")][InlineData("review")][InlineData("missing-proof")][InlineData("null")]
    public async Task TypedDecisionClientRequiresOriginalResultProofAndDropsLateCompanyOrAccount(string mode)
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create();var row=await f.Record();var command=f.Command;var review=f.Review;
        var submission=new PurchaseOrderInventoryLinkDecisionSubmission(command,review);
        if(mode is "intent" or "decision" or "review")
        {
            var recorded=mode=="intent"?command with{IntentId=Guid.NewGuid().ToString("N")}:mode=="decision"?command with{Decision=PurchaseOrderInventoryLinkDecision.Reject}:command;
            var retained=mode=="review"?review with{Purchase=review.Purchase with{RetrievedAtUtc=review.Purchase.RetrievedAtUtc.AddMinutes(1)}}:review;
            if(mode=="decision")row.INVENTORY_ITEM_ID="current-item";
            row.INVENTORY_LINK_DECISION_JSON=PurchaseOrderInventoryLinkDecisionProof.Create(recorded,retained,row);
        }
        if(mode=="reference")row.INVENTORY_ITEM_ID="other";if(mode=="missing-proof")row.INVENTORY_LINK_DECISION_JSON=null;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"reviewer",new(command.Company,["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Procurement.RecordInventoryLinkDecision"])));
        using var http=new HttpClient(new Reply(request=>{
            Assert.EndsWith("/order/lines/po-line/inventory-link-decision",request.RequestUri!.AbsoluteUri);
            if(mode is "account" or "company")company.TrySelect(company.Clear(),mode=="account"?"other":"reviewer",new(mode=="company"?new("tenant","other"):command.Company,["ConfiguredCompanyRole"],[]));
            return new(HttpStatusCode.OK){Content=mode=="null"?JsonContent.Create<PO_LINE_ITEM?>(null):JsonContent.Create(row)};
        })){BaseAddress=new Uri("https://decision.test/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="valid")submission.RequireResult(await client.RecordInventoryLinkDecisionAsync(submission));
        else if(mode is "account" or "company")await Assert.ThrowsAsync<OperationCanceledException>(()=>client.RecordInventoryLinkDecisionAsync(submission));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>client.RecordInventoryLinkDecisionAsync(submission));
    }
}
#endif
