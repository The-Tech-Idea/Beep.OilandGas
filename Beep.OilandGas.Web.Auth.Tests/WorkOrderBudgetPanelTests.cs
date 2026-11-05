using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderBudgetPanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"FinanceRole")],"fixture"))));}
    private sealed class Nav:NavigationManager{public void Start(string book)=>Initialize("http://fixture/","http://fixture/workorder?budgetBook="+System.Uri.EscapeDataString(book));protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Components:IComponentActivator{internal WorkOrderBudgetPanel Panel=null!;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is WorkOrderBudgetPanel panel)Panel=panel;return component;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    private static Task Invoke(object panel,string method)=>(Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("late-company")][InlineData("precision")][InlineData("utc")][InlineData("permission")][InlineData("role")][InlineData("field")]
    public async Task ActualBudgetFormUsesRecordedCurrencyAndRetainsBookAndIntentBeforeTransport(string mode)
    {
        var review=WorkOrderBudgetStateTests.Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,
            mode=="role"?["AnotherRole"]:["FinanceRole"],mode=="permission"?["Accounting.View"]:["Accounting.View","Accounting.PrepareWorkOrderBudget"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":review.Query.FieldId);
        var nav=new Nav();nav.Start(review.Query.BookId);var client=new Mock<IWorkOrderServiceClient>();var reads=0;var sends=0;CompanyWorkOrderBudgetRecovery? recorded=null;
        client.Setup(value=>value.ReviewBudgetAsync(review.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(review);});
        client.Setup(value=>value.CreateBudgetAsync(It.IsAny<CompanyWorkOrderBudgetCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderBudgetCommand command,CancellationToken _)=>{
            Assert.Contains("budgetIntent="+command.IntentId.ToString("D"),nav.Uri);Assert.Contains("budgetBook="+Uri.EscapeDataString(review.Query.BookId),nav.Uri);sends++;recorded=new(command,command.Prepare(DateTime.UtcNow));
            if(mode=="unknown")throw new HttpRequestException("Lost AFE acknowledgement");if(mode=="late-company")company.Clear();return Task.FromResult(recorded);});
        client.Setup(value=>value.RecoverBudgetAsync(It.IsAny<CompanyWorkOrderBudgetReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderBudgetReference reference,CancellationToken _)=>{recorded!.Require(review.Context.Company,review.Context.ActorUserId,reference);return Task.FromResult(recorded);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderBudgetState>();services.AddSingleton<IWorkOrderServiceClient>(client.Object);
        services.AddSingleton<ICompanyLedgerBookClient>(Mock.Of<ICompanyLedgerBookClient>());services.AddSingleton(fields.Object);services.AddSingleton<NavigationManager>(nav);services.AddSingleton<AuthenticationStateProvider>(new Auth(review.Context.ActorUserId));
        services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkOrderId",review.Query.WorkOrderId},{"FieldId",review.Query.FieldId}});
            var root=await renderer.RenderComponentAsync<WorkOrderBudgetPanel>(parameters);var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            await Invoke(panel,"ReviewAsync");if(mode is "role" or "permission" or "field"){Assert.Equal(0,reads);Assert.Equal(0,sends);return;}
            Assert.Equal(1,reads);Assert.Contains("KWD",root.ToHtmlString());Assert.Contains("decimal places 3",root.ToHtmlString());
            void Set(string key,object value)=>panel.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
            Set("_number","ExplicitAFE");Set("_name","Explicit work budget");Set("_amount",mode=="precision"?12.8751m:12.875m);
            Set("_effective",mode=="utc"?"2026-10-14T05:00:00":DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",System.Globalization.CultureInfo.InvariantCulture));Set("_reason","Reviewed native work budget");Set("_confirm",true);
            await Invoke(panel,"CreateAsync");await Task.Yield();if(mode is "utc" or "precision"){Assert.Equal(0,sends);return;}Assert.Equal(1,sends);
            if(mode=="late-company")
            {
                Assert.DoesNotContain(recorded!.Recorded.AFE_ID,root.ToHtmlString());Assert.DoesNotContain(recorded.Command.IntentId.ToString("D"),root.ToHtmlString());
                Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["FinanceRole"],["Accounting.View","Accounting.PrepareWorkOrderBudget"])));
                root=await renderer.RenderComponentAsync<WorkOrderBudgetPanel>(parameters);panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            }
            Assert.Contains(recorded!.Command.IntentId.ToString("D"),root.ToHtmlString());await Invoke(panel,"RecoverAsync");await Task.Yield();
            Assert.Contains(recorded.Recorded.AFE_ID,root.ToHtmlString());Assert.Contains("Independent approval and actual costs remain separate",root.ToHtmlString());
            await Invoke(panel,"CreateAsync");Assert.Equal(1,sends);
        });
    }
}
#endif
