#if WORKFLOW_INBOX_FOCUSED
extern alias actualData;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Item=actualData::Beep.OilandGas.Models.Data.ProductionAccounting.INVENTORY_ITEM;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaterialSourceTests
{
    internal static CompanyWorkOrderMaterialSourceReview Source()
    {
        var order=WorkOrderExecutionPermitStateTests.Review();var query=new CompanyWorkOrderMaterialSourceQuery(order.Source.Stage.Task,"part-الكويت",2m,"EA");var item=new Item{INVENTORY_ITEM_ID=query.ItemId,ITEM_NAME="Recorded pump spare",OWNER_TENANT_ID=order.Context.Company.TenantId,OWNER_COMPANY_BA_ID=order.Context.Company.BusinessAssociateId,ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="EA",QUANTITY_ON_HAND=5m};var inventory=CompanyInventorySourceReview.Create(order.Context.Company,"issuer",item,[]);return new(order.Context with{Database=new("production","inventory-binding")},query,order.Source,inventory,CompanyWorkOrderMaterialSourceReview.GetBlockers(query,inventory));
    }
    [Theory][InlineData("original")][InlineData("unit")][InlineData("quantity")][InlineData("unknown-stock")][InlineData("inactive")][InlineData("foreign")][InlineData("blockers")]
    public void MaterialSourcePreservesExplicitUnitsMissingStockAndOwnershipWithoutAssumingAllocation(string fault)
    {
        var source=Source();var query=source.Query;var item=CompanyInventorySourceReview.Copy(source.Inventory.Item);
        if(fault=="unit")query=query with{QuantityUnit="kg"};if(fault=="quantity")query=query with{RequiredQuantity=6};if(fault=="unknown-stock")item.QUANTITY_ON_HAND=null;if(fault=="inactive")item.ACTIVE_IND="N";if(fault=="foreign")item.OWNER_COMPANY_BA_ID="other";
        if(fault=="foreign"){Assert.Throws<InvalidOperationException>(()=>CompanyInventorySourceReview.Create(source.Context.Company,"issuer",item,[]));return;}
        var inventory=CompanyInventorySourceReview.Create(source.Context.Company,"issuer",item,[]);source=source with{Query=query,Inventory=inventory,Blockers=CompanyWorkOrderMaterialSourceReview.GetBlockers(query,inventory)};
        if(fault=="blockers"){source=source with{Blockers=[]};Assert.Throws<InvalidOperationException>(()=>source.Require(source.Context.Company,"issuer",query));return;}
        source.Require(source.Context.Company,"issuer",query);Assert.Contains(source.Blockers,b=>b.Contains("allocation",StringComparison.Ordinal));Assert.Equal(fault=="original"?1:2,source.Blockers.Count);
    }
    private sealed class Response(CompanyWorkOrderMaterialSourceReview source,string fault,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {Calls++;Assert.Equal("/api/field/current/maintenance-handoffs/materials/source-review",request.RequestUri!.AbsolutePath);Assert.Equal(source.Query,(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderMaterialSourceQuery>(token))!);if(fault=="late-company")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(fault=="blockers"?source with{Blockers=[]}:source)};}
    }
    [Theory][InlineData("original")][InlineData("permission")][InlineData("blockers")][InlineData("late-company")]
    public async Task FullWorkflowClientFencesOriginalMaterialQueryAndCompleteSource(string fault)
    {
        var source=Source();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],fault=="permission"?[]:["Inventory.View"])));var transport=new Response(source,fault,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(async()=>{var result=await client.ReviewWorkOrderMaterialSourceAsync(source.Query);result.Require(source.Context.Company,"issuer",source.Query);});if(fault=="original")Assert.Null(error);else Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString()??"Unexpected material source publication");Assert.Equal(fault=="permission"?0:1,transport.Calls);
    }
}
#endif
