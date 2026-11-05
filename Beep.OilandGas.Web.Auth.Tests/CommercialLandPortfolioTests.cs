using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if COMMERCIAL_LAND_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class CommercialLandPortfolioTests
{
    private sealed class Auth(bool denied):AuthenticationStateProvider
    {private string actor="actor";public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,denied?"OtherRole":"CompanyCommercial")],"fixture"))));public void Change(){actor="other";NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}}
    private sealed class Js:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);}
    private sealed class Navigation:NavigationManager
    {public Navigation()=>Initialize("https://fixture/","https://fixture/accounting/revenue-workbench");protected override void NavigateToCore(string uri,bool forceLoad){}}
    private sealed class Components(IServiceProvider services):IComponentActivator
    {public CommercialLandPortfolio? Page;public IComponent CreateInstance(Type type){var result=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(result is CommercialLandPortfolio page)Page=page;return result;}}
    [Theory][InlineData("full")][InlineData("empty")][InlineData("error")][InlineData("missing-company")][InlineData("missing-field")][InlineData("denied")][InlineData("filter")][InlineData("review")][InlineData("late-company")][InlineData("late-field")][InlineData("late-auth")]
    public async Task RecordedLeaseQueueUsesCurrentScopeAndExactStatusDrillDown(string mode)
    {
        using var company=new CompanyClientContext();if(mode!="missing-company")Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["CompanyCommercial"],[])));
        var rows=new List<LeaseSummary>{new(){LeaseId="lease-a",FieldId="field",LandRightSubtype="subtype",DateZoneId="UTC",Status="RECORDED_REVIEW",LeaseName="Name<recorded>",CalculatedInterest=.25m},new(){LeaseId="lease-b",FieldId="field",LandRightSubtype="subtype",DateZoneId="UTC",Status="RECORDED_ACTIVE",CalculatedInterest=.5m}};
        var api=new Mock<ILeaseServiceClient>(MockBehavior.Strict);var pending=new TaskCompletionSource<List<LeaseSummary>>();
        var setup=api.Setup(value=>value.GetAvailableLeasesAsync(It.Is<Dictionary<string,string>>(filter=>filter.Count==1 && filter["FieldId"]=="field"),It.IsAny<CancellationToken>()));
        if(mode is "late-company" or "late-field" or "late-auth")setup.Returns(pending.Task);else if(mode=="error")setup.ThrowsAsync(new UnauthorizedAccessException());else setup.ReturnsAsync(mode=="empty"?[]:rows);
        api.Setup(value=>value.GetAvailableLeasesAsync(It.Is<Dictionary<string,string>>(filter=>filter.Count==1 && filter["FieldId"]=="field-b"),It.IsAny<CancellationToken>())).ReturnsAsync([new(){LeaseId="lease-c",FieldId="field-b",LandRightSubtype="subtype",DateZoneId="UTC",Status="OTHER_FIELD",CalculatedInterest=.75m}]);
        api.Setup(value=>value.EvaluateLeaseAsync("lease-a","field",It.IsAny<CancellationToken>())).ReturnsAsync(rows[0]);
        string? currentField=mode=="missing-field"?null:"field";var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).Returns(()=>Task.FromResult(currentField));var authentication=new Auth(mode=="denied");
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(api.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=renderer.BeginRenderingComponent<CommercialLandPortfolio>();
            if(mode=="late-company")
            {
                company.Clear();pending.SetResult(rows);await root.QuiescenceTask;Assert.DoesNotContain("lease-a",root.ToHtmlString());Assert.Contains("Select a company",root.ToHtmlString());return;
            }
            if(mode is "late-field" or "late-auth")
            {
                if(mode=="late-field"){currentField="field-b";fields.Raise(value=>value.CurrentFieldChanged+=null,"field-b");}else authentication.Change();
                pending.SetResult(rows);await root.QuiescenceTask;Assert.DoesNotContain("lease-a",root.ToHtmlString());
                if(mode=="late-field")Assert.Contains("lease-c",root.ToHtmlString());else Assert.Contains("app roles",root.ToHtmlString());return;
            }
            await root.QuiescenceTask;var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            if(mode=="filter")
            {
                typeof(CommercialLandPortfolio).GetMethod("SelectStatus",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["RECORDED_REVIEW"]);
                typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
                Assert.Contains("lease-a",root.ToHtmlString());Assert.DoesNotContain("lease-b",root.ToHtmlString());Assert.Contains("Showing 1 records",root.ToHtmlString());
            }
            if(mode=="review")
            {
                var state=(LeasePortfolioState)typeof(CommercialLandPortfolio).GetField("_portfolio",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(page)!;
                await (Task)typeof(CommercialLandPortfolio).GetMethod("Review",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,[state.Leases[0]])!;
                typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);Assert.Contains("Recorded lease review",root.ToHtmlString());
                api.Verify(value=>value.EvaluateLeaseAsync("lease-a","field",It.IsAny<CancellationToken>()),Times.Once);
            }
            var html=root.ToHtmlString();
            if(mode is "full" or "filter" or "review"){Assert.Contains("Retrieved UTC",html);Assert.Contains("Name&lt;recorded&gt;",html);Assert.Contains("Date zone UTC",html);}
            if(mode=="empty")Assert.Contains("No recorded leases were returned",html);
            if(mode=="error"){Assert.Contains("Lease records could not be confirmed",html);Assert.DoesNotContain("All recorded leases (0)",html);}
            if(mode=="missing-field")Assert.Contains("Choose an available FIELD",html);
            if(mode=="missing-company")Assert.Contains("Select a company",html);
            if(mode=="denied")Assert.Contains("app roles",html);
            if(mode is "denied" or "missing-company" or "missing-field")api.Verify(value=>value.GetAvailableLeasesAsync(It.IsAny<Dictionary<string,string>>(),It.IsAny<CancellationToken>()),Times.Never);
        });
    }
}
#endif
