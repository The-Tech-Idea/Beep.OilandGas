#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalPreparationScreenTests
{
    private sealed class Authentication:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"controller"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator
    {public Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench? Workbench;public IComponent CreateInstance(Type type){var value=(IComponent)Activator.CreateInstance(type)!;if(value is Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench workbench)Workbench=workbench;return value;}}
    [Fact] public async Task RenderedFormRequiresExplicitNumberAndReasonRetainsUnknownRequestAndRecoversOriginalDraft()
    {
        using var validation=TheTechIdea.Data.OilGas.CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare();CompanySalesJournalPreparationHttpTests.Writes(fixture);await using var host=await CompanySalesJournalSourceHttpTests.Start(fixture);host.Context.Permissions=CompanySalesJournalPreparationHttpTests.Grants;
        using var company=CompanyGLAccountHttpTests.Selected(actor:fixture.Current.ActorUserId,grants:CompanySalesJournalPreparationHttpTests.Grants);var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();
        services.AddSingleton(company);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();var state=provider.GetRequiredService<CompanySalesReceivableWorkspaceState>();state.AttachOwner(new object());await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"ReceivableId",fixture.Current.ReceivableId}}));var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Invoke(string method,params object?[] values)=>components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,values);
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            async Task Action(string name){await (Task)components.Workbench!.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Invoke("StateHasChanged");}
            Invoke("JournalSelectionChanged","book","statutory");Invoke("JournalSelectionChanged","currency","KWD");Invoke("JournalSelectionChanged","calendar","company-calendar");Invoke("JournalDateChanged",fixture.Date);Invoke("JournalSelectionChanged","receivable",fixture.Selection.ReceivableKey);Invoke("JournalSelectionChanged","revenue",fixture.Selection.RevenueKey);Invoke("StateHasChanged");await Action("LoadJournalSource");Assert.True(state.CanPrepareJournal);
            await Action("PrepareJournal");Assert.False(state.HasPendingJournal);Assert.Equal(0,fixture.Native.Writes);Field("_journalNumber","explicit-screen-journal");Field("_journalReason","Reviewed exact native source");Invoke("StateHasChanged");fixture.Native.FailCommit=true;await Action("PrepareJournal");Assert.True(state.HasPendingJournal);Assert.Contains("exact original journal preparation request is retained",root.ToHtmlString());var id=state.PendingJournal!.JournalId;
            components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"ReceivableId",fixture.Current.ReceivableId}}));Assert.Equal(id,state.PendingJournal!.JournalId);fixture.Native.FailCommit=false;await Action("ReconcileJournal");Assert.False(state.HasPendingJournal);Assert.Equal("DRAFT",state.JournalPreparation!.Header.STATUS);Assert.Equal(4,fixture.Native.Writes);Assert.Contains("does not establish ledger posting",root.ToHtmlString());
            Field("_journalRecoveryBook","statutory");Field("_journalRecoveryCurrency","KWD");Field("_journalRecoveryId",id);Invoke("StateHasChanged");await Action("RecoverJournal");Assert.True(state.HasPendingJournal);await Action("ReconcileJournal");Assert.False(state.HasPendingJournal);Assert.Equal(4,fixture.Native.Writes);
        });
    }
}
#endif
