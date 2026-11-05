#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Auth.Tests.Fixtures;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class InventoryLinkDecisionRenderingTests
{
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("corrupt")]
    public async Task ExistingOrderSourceShowsIndependentOutcomeAndKeepsOriginalPreparerAudit(string mode)
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create(mode=="reject"?PurchaseOrderInventoryLinkDecision.Reject:PurchaseOrderInventoryLinkDecision.Approve);
        var preparedAt=f.Stored.ROW_CHANGED_DATE;await f.Record();if(mode=="corrupt")f.Stored.INVENTORY_LINK_DECISION_JSON="{}";
        var source=(await f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,"order","reviewer"))!;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"reviewer",new(source.Company,["ConfiguredReviewer"],["Procurement.View","Inventory.View"])));
        using var state=new PurchaseOrderWorkspaceState(company);await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(source));
        var line=Assert.Single(state.Lines!);
        if(mode=="corrupt"){Assert.Null(line.InventoryDecision);Assert.Null(line.Preparation);}
        else{Assert.Equal("actor",line.Preparation!.Actor);Assert.Equal(preparedAt,line.Preparation.RecordedAt);Assert.Equal("reviewer",line.InventoryDecision!.Actor);Assert.Equal(mode=="reject"?"current-item":"target",line.InventoryItemId);}
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<PurchaseOrderRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return root.ToHtmlString();});
        if(mode=="corrupt"){Assert.Contains("Independent inventory-link decision evidence is unconfirmed",html);Assert.DoesNotContain("Verified inventory-link decision",html);}
        else{Assert.Contains("Verified inventory-link decision",html);Assert.Contains(mode=="reject"?"Reject":"Approve",html);Assert.Contains("Reviewer: reviewer",html);Assert.Contains("Prepared by actor",html);Assert.DoesNotContain("Independent approval and target assignment remain unconfirmed",html);Assert.Contains("Stock movement and financial posting remain unconfirmed",html);}
    }
}
#endif
