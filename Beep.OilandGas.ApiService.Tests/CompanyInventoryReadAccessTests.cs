#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyInventoryReadAccessTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    private static INVENTORY_ITEM Item()=>new(){INVENTORY_ITEM_ID="item/α%2F",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",UNIT_OF_MEASURE="ea",QUANTITY_ON_HAND=null};
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("role")][InlineData("binding")][InlineData("account")][InlineData("company")][InlineData("roles-changed")][InlineData("grants-changed")][InlineData("item-changed")][InlineData("history-changed")]
    public async Task NativeSourceReadRequiresCurrentAuthorityAndStableFullEvidence(string mode)
    {
        var item=Item();var source=new Mock<IDataSource>();var creator=new ClassCreator(Mock.Of<IDMEEditor>());var reads=0;
        source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>creator.ConvertToEntityStructure(request.EntityName=="INVENTORY_ITEM"?typeof(INVENTORY_ITEM):typeof(INVENTORY_TRANSACTION)));
        source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>{++reads;if(mode=="item-changed" && reads>1)item.LOCATION_ID="other";return new[]{item};});
        var historyReads=0;
        source.Setup(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>{++historyReads;return new[]{new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="movement",INVENTORY_ITEM_ID=item.INVENTORY_ITEM_ID,QUANTITY=mode=="history-changed" && historyReads>1?9:3}};});
        var editor=new Mock<IDMEEditor>();editor.Setup(value=>value.GetDataSource("bound")).Returns(source.Object);
        var context=new Mock<ICompanyAccountingReadContext>();var calls=0;
        context.Setup(value=>value.CaptureAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(()=>{
            ++calls;string[] roles=mode=="role"?["Other"]:mode=="roles-changed" && calls>1?["Warehouse","Extra"]:["Warehouse"];
            string[] grants=mode=="grant"?[]:mode=="grants-changed" && calls>1?["Inventory.View","Extra"]:["Inventory.View"];
            return (new RepositoryCompanyAccess(mode=="company" && calls>1?new("tenant","other"):Company,roles,grants),mode=="binding" && calls>1?"other":"binding",mode=="account" && calls>1?"other":"warehouse-user");});
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"warehouse-user"),new(ClaimTypes.Role,"Warehouse")],"app"));
        var owner=new CompanyInventoryReads(CompanyInventorySourceTests.CreateService(editor,()=>Task.FromResult("bound")),context.Object);
        if(mode=="valid"){var review=await owner.GetSourceAsync(user,item.INVENTORY_ITEM_ID);Assert.NotNull(review);Assert.Null(review.Item.QUANTITY_ON_HAND);Assert.Equal(2,reads);Assert.Equal(2,historyReads);}
        else if(mode is "grant" or "role"){await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.GetSourceAsync(user,item.INVENTORY_ITEM_ID));Assert.Equal(0,reads);}
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.GetSourceAsync(user,item.INVENTORY_ITEM_ID));
    }
    private sealed class Reply(Func<HttpRequestMessage,HttpResponseMessage> reply):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)=>Task.FromResult(reply(request));}
    [Theory][InlineData("valid")][InlineData("population")][InlineData("measurement")][InlineData("grant")]
    public async Task QueueComparesTheFullAuthorizedPopulationAndRecordedMeasurements(string mode)
    {
        var item=Item();var source=new Mock<IDataSource>();var reads=0;
        source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(INVENTORY_ITEM)));
        source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>{
            ++reads;if(mode=="measurement" && reads>1)item.UNIT_OF_MEASURE="bbl";
            var extra=CompanyInventorySourceReview.Copy(item);extra.INVENTORY_ITEM_ID="extra";
            return mode=="population" && reads>1?new[]{item,extra}:new[]{item};});
        var editor=new Mock<IDMEEditor>();editor.Setup(value=>value.GetDataSource("bound")).Returns(source.Object);
        var context=new Mock<ICompanyAccountingReadContext>();var calls=0;
        context.Setup(value=>value.CaptureAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(()=>{
            ++calls;return(new RepositoryCompanyAccess(Company,["Warehouse"],mode=="grant" && calls>1?[]:["Inventory.View"]),"binding","warehouse-user");});
        var owner=new CompanyInventoryReads(CompanyInventorySourceTests.CreateService(editor,()=>Task.FromResult("bound")),context.Object);
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"Warehouse")],"app"));
        if(mode=="valid")Assert.Equal(item.INVENTORY_ITEM_ID,Assert.Single(await owner.GetQueueAsync(user)).INVENTORY_ITEM_ID);
        else if(mode=="grant")await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.GetQueueAsync(user));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.GetQueueAsync(user));
    }
    [Theory][InlineData("valid")][InlineData("account")][InlineData("company")][InlineData("actor")][InlineData("source")]
    public async Task TypedClientRetainsExactIdAndRejectsLateOrChangedReview(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"warehouse-user",new(Company,["Warehouse"],["Inventory.View"])));
        var item=Item();var review=CompanyInventorySourceReview.Create(Company,"warehouse-user",item,[]);
        using var http=new HttpClient(new Reply(request=>{
            Assert.EndsWith("/item%2F%CE%B1%252F",request.RequestUri!.AbsoluteUri);
            if(mode is "account" or "company")company.TrySelect(company.Clear(),mode=="account"?"other":"warehouse-user",new(mode=="company"?new("tenant","other"):Company,["Warehouse"],["Inventory.View"]));
            if(mode=="actor")review=review with{ActorUserId="other"};if(mode=="source")review.Item.LOCATION_ID="changed";
            return new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(review)};
        })){BaseAddress=new Uri("https://inventory.test/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="valid"){var returned=await client.GetCompanyInventorySourceAsync(item.INVENTORY_ITEM_ID);Assert.NotNull(returned);returned.Require(Company,"warehouse-user",item.INVENTORY_ITEM_ID);}
        else if(mode is "account" or "company")await Assert.ThrowsAsync<OperationCanceledException>(()=>client.GetCompanyInventorySourceAsync(item.INVENTORY_ITEM_ID));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetCompanyInventorySourceAsync(item.INVENTORY_ITEM_ID));
    }
    [Theory][InlineData("valid")][InlineData("anonymous")][InlineData("grant")][InlineData("binding")][InlineData("source")]
    public async Task ActualInventoryEndpointUsesNativeOwnerAndIndependentAuthority(string mode)
    {
        var item=Item();var source=new Mock<IDataSource>();var creator=new ClassCreator(Mock.Of<IDMEEditor>());var reads=0;
        source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>creator.ConvertToEntityStructure(request.EntityName=="INVENTORY_ITEM"?typeof(INVENTORY_ITEM):typeof(INVENTORY_TRANSACTION)));
        source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>{++reads;if(mode=="source" && reads>1)item.LOCATION_ID="other";return new[]{item};});
        source.Setup(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<AppFilter>>())).ReturnsAsync(Array.Empty<INVENTORY_TRANSACTION>());
        var editor=new Mock<IDMEEditor>();editor.Setup(value=>value.GetDataSource("bound")).Returns(source.Object);
        var inventory=CompanyInventorySourceTests.CreateService(editor,()=>Task.FromResult("bound"));
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="anonymous",permissions:mode=="grant"?[]:["Inventory.View"],configureAccounting:(services,context)=>{services.AddSingleton(inventory);context.ChangeBinding=mode=="binding";});
        var response=await host.Client.GetAsync("api/accounting/traditional/inventory/company/items/"+Uri.EscapeDataString(item.INVENTORY_ITEM_ID));
        if(mode=="valid")
        {
            Assert.Equal(HttpStatusCode.OK,response.StatusCode);var review=await response.Content.ReadFromJsonAsync<CompanyInventorySourceReview>();Assert.NotNull(review);review.Require(Company,"actor",item.INVENTORY_ITEM_ID);Assert.Empty(review.Transactions);Assert.Null(review.Item.QUANTITY_ON_HAND);
        }
        else Assert.Equal(mode=="anonymous"?HttpStatusCode.Unauthorized:mode=="grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,response.StatusCode);
        if(mode is "anonymous" or "grant")Assert.Equal(0,reads);
    }
}

#endif
