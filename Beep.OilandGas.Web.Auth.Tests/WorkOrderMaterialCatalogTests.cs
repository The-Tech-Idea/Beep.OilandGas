#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaterialCatalogTests
{
    internal static CompanyWorkOrderMaterialCatalog Catalog()
    {
        var source=WorkOrderMaterialSourceTests.Source();return CompanyWorkOrderMaterialCatalog.Create(source.WorkOrder.Stage,[source.Inventory.Item],DateTime.UtcNow);
    }
    [Theory][InlineData("original")][InlineData("empty")][InlineData("duplicate")][InlineData("foreign")][InlineData("inactive")][InlineData("utc")][InlineData("revision")][InlineData("late-task")][InlineData("unknown")]
    public void CatalogKeepsCompleteOwnedMembershipAndUnknownRecordedStock(string mode)
    {
        var catalog=Catalog();var item=CompanyInventorySourceReview.Copy(catalog.Items[0]);var rows=new[]{item};var utc=catalog.ComparedAtUtc;
        if(mode=="empty")rows=[];if(mode=="duplicate")rows=[item,item];if(mode=="foreign")item.OWNER_COMPANY_BA_ID="other";if(mode=="inactive")item.ACTIVE_IND="N";if(mode=="utc")utc=DateTime.SpecifyKind(utc,DateTimeKind.Unspecified);if(mode=="unknown"){item.QUANTITY_ON_HAND=null;item.UNIT_OF_MEASURE=null;item.LOCATION_ID=null;}
        var error=Record.Exception(()=>{var result=CompanyWorkOrderMaterialCatalog.Create(catalog.Stage,rows,utc);if(mode=="revision")result=result with{Revision=new string('F',64)};var task=mode=="late-task"?result.Stage.Task with{WorkOrderId="other"}:result.Stage.Task;result.Require(catalog.Stage.Context.Company,"issuer",task);if(mode=="unknown")Assert.Null(Assert.Single(result.Items).QUANTITY_ON_HAND);});
        if(mode is "original" or "empty" or "unknown")Assert.Null(error);else Assert.NotNull(error);
    }
    private sealed class Transport(string mode,CompanyWorkOrderMaterialCatalog catalog,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/materials/catalog",request.RequestUri!.AbsolutePath);Assert.Equal(catalog.Stage.Task,await request.Content!.ReadFromJsonAsync<CompanyMaintenanceReceivingQuery>(token));if(mode=="late")change();var result=mode=="forged"?catalog with{Revision=new string('F',64)}:catalog;return new(HttpStatusCode.OK){Content=JsonContent.Create(result)};
        }
    }
    [Theory][InlineData("original")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("role")]
    public async Task FullTypedClientRejectsForeignMembershipRoleAndLateAccount(string mode)
    {
        var catalog=Catalog();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(catalog.Stage.Context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?["Inventory.View"]:["Inventory.View","Inventory.ReserveWorkOrderMaterial"])));var transport=new Transport(mode,catalog,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://catalog.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.ReviewWorkOrderMaterialCatalogAsync(catalog.Stage.Task);Assert.Single(result.Items);});if(mode=="original")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
}
#endif
