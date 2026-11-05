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
public sealed class WorkOrderBudgetDecisionPanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"FinanceRole")],"fixture"))));}
    private sealed class Nav:NavigationManager{public void Start(string book)=>Initialize("http://fixture/","http://fixture/workorder?budgetDecisionBook="+System.Uri.EscapeDataString(book));protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Components:IComponentActivator{internal WorkOrderBudgetDecisionPanel Panel=null!;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is WorkOrderBudgetDecisionPanel panel)Panel=panel;return component;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    private static Task Invoke(object panel,string method)=>(Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("late-company")][InlineData("decision")][InlineData("reject")][InlineData("permission")][InlineData("role")][InlineData("field")]
    public async Task ActualIndependentDecisionFormRequiresExplicitOutcomeAndRetainsOriginalAfeBookAndIntent(string mode)
    {
        var review=WorkOrderBudgetDecisionStateTests.Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Current.Context.ActorUserId,new(review.Current.Context.Company,
            mode=="role"?["AnotherRole"]:["FinanceRole"],mode=="permission"?["Accounting.View"]:["Accounting.View","Accounting.ReviewWorkOrderBudget"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":review.Query.Budget.FieldId);
        var nav=new Nav();nav.Start(review.Query.Budget.BookId);nav.NavigateTo(nav.Uri+"&budgetDecisionAfe="+Uri.EscapeDataString(review.Query.AfeId));var client=new Mock<IWorkOrderServiceClient>();var reads=0;var sends=0;CompanyWorkOrderBudgetDecisionRecovery? recorded=null;
        client.Setup(value=>value.ReviewBudgetDecisionAsync(review.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(review);});
        client.Setup(value=>value.RecordBudgetDecisionAsync(It.IsAny<CompanyWorkOrderBudgetDecisionCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderBudgetDecisionCommand command,CancellationToken _)=>{
            Assert.Contains("budgetDecisionIntent="+command.IntentId.ToString("D"),nav.Uri);Assert.Contains("budgetDecisionBook="+Uri.EscapeDataString(review.Query.Budget.BookId),nav.Uri);sends++;recorded=new(command,command.Prepare(DateTime.UtcNow));
            if(mode=="unknown")throw new HttpRequestException("Lost AFE acknowledgement");if(mode=="late-company")company.Clear();return Task.FromResult(recorded);});
        client.Setup(value=>value.RecoverBudgetDecisionAsync(It.IsAny<CompanyWorkOrderBudgetDecisionReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderBudgetDecisionReference reference,CancellationToken _)=>{recorded!.Require(review.Current.Context.Company,review.Current.Context.ActorUserId,reference);return Task.FromResult(recorded);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderBudgetDecisionState>();services.AddSingleton<IWorkOrderServiceClient>(client.Object);
        services.AddSingleton<ICompanyLedgerBookClient>(Mock.Of<ICompanyLedgerBookClient>());services.AddSingleton(fields.Object);services.AddSingleton<NavigationManager>(nav);services.AddSingleton<AuthenticationStateProvider>(new Auth(review.Current.Context.ActorUserId));
        services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkOrderId",review.Query.Budget.WorkOrderId},{"FieldId",review.Query.Budget.FieldId}});
            var root=await renderer.RenderComponentAsync<WorkOrderBudgetDecisionPanel>(parameters);var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            await Invoke(panel,"ReviewAsync");if(mode is "role" or "permission" or "field"){Assert.Equal(0,reads);Assert.Equal(0,sends);return;}
            Assert.Equal(1,reads);Assert.Contains("KWD",root.ToHtmlString());Assert.Contains("Original author",root.ToHtmlString());
            void Set(string key,object value)=>panel.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
            Set("_decision",mode=="decision"?WorkOrderBudgetDecision.Unspecified:mode=="reject"?WorkOrderBudgetDecision.Reject:WorkOrderBudgetDecision.Approve);
            Set("_reason","Independent original scope/book/currency/amount review");Set("_confirm",true);
            await Invoke(panel,"RecordAsync");await Task.Yield();if(mode=="decision"){Assert.Equal(0,sends);return;}Assert.Equal(1,sends);
            if(mode=="late-company")
            {
                Assert.DoesNotContain(recorded!.Recorded.APPROVAL_WORKFLOW_ID,root.ToHtmlString());Assert.DoesNotContain(recorded.Command.IntentId.ToString("D"),root.ToHtmlString());
                Assert.True(company.TrySelect(company.Clear(),review.Current.Context.ActorUserId,new(review.Current.Context.Company,["FinanceRole"],["Accounting.View","Accounting.ReviewWorkOrderBudget"])));
                root=await renderer.RenderComponentAsync<WorkOrderBudgetDecisionPanel>(parameters);panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            }
            Assert.Contains(recorded!.Command.IntentId.ToString("D"),root.ToHtmlString());await Invoke(panel,"RecoverAsync");await Task.Yield();
            Assert.Contains(recorded.Recorded.APPROVAL_WORKFLOW_ID,root.ToHtmlString());Assert.Contains("Recorded independent",root.ToHtmlString());Assert.Contains(mode=="reject"?"REJECTED":"APPROVED",root.ToHtmlString());
            await Invoke(panel,"RecordAsync");Assert.Equal(1,sends);
        });
    }
}
#endif
