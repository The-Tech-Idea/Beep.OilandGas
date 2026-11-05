#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceCorrectionEditorTests
{
    internal sealed class Authentication:AuthenticationStateProvider
    {
        private string actor="actor";
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new(System.Security.Claims.ClaimTypes.NameIdentifier,actor),new(System.Security.Claims.ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));
        public void Change(string user){actor=user;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private static CompanyClientContext Selected(string actor="actor")
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.CorrectInvoice"]));return company;}
    private static void Edit(APInvoiceWorkspaceState state)
    {state.BeginCorrection();state.SetCorrectionTotal(3m);state.SetCorrectionAmount("line","QUANTITY",3m);state.SetCorrectionAmount("line","UNIT_PRICE",1m);state.SetCorrectionAmount("line","LINE_TOTAL",3m);state.SetCorrectionReason("Explicit accountant correction for the recorded controller request.");}
    [Theory][InlineData("valid")][InlineData("null")][InlineData("failure")][InlineData("late-edit")][InlineData("late-financial")][InlineData("late-company")][InlineData("late-account")][InlineData("late-dispose")]
    public async Task ScopedCorrectionEditsClearReviewsAndRejectLateOrIncompletePublication(string mode)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare(within:false);await f.Record();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadDetailAsync("invoice",id=>f.Existing.Existing.ReadOwner.Native.AP.GetSourceReviewAsync(f.Command.Company,id,"actor"));var financial=await InvoiceVarianceDecisionReviewTests.Financial(f.Existing,"actor");
        await state.LoadFinancialReviewAsync((_,_)=>Task.FromResult(financial));Edit(state);
        if(mode=="null")
        {state.SetCorrectionAmount("line","QUANTITY",null);Assert.Null(Assert.Single(state.CorrectionLines).Quantity);var calls=0;await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadCorrectionReviewAsync((_,_)=>{calls++;throw new InvalidOperationException();}));Assert.Equal(0,calls);Assert.Null(state.CorrectionReview);Assert.False(state.CorrectionReviewBusy);return;}
        if(mode=="failure")
        {await state.LoadCorrectionReviewAsync((_,draft)=>Task.FromResult(APInvoiceCorrectionReview.Create(financial,draft,f.Command.BindingRevision)));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadCorrectionReviewAsync((_,_)=>throw new InvalidOperationException("Changed evidence")));Assert.Null(state.CorrectionReview);Assert.False(state.CorrectionReviewBusy);Assert.True(state.CorrectionEditing);return;}
        if(mode=="valid")
        {
            await state.LoadCorrectionReviewAsync((_,draft)=>Task.FromResult(APInvoiceCorrectionReview.Create(financial,draft,f.Command.BindingRevision)));Assert.NotNull(state.CorrectionReview);var copy=state.CorrectionReview!;copy.Current.Invoice.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,state.CorrectionReview!.Current.Invoice.Invoice.TOTAL_AMOUNT);
            state.SetCorrectionDescription("line","Updated explicit description");Assert.Null(state.CorrectionReview);Assert.True(state.CorrectionEditing);state.ClearFinancialReview();Assert.False(state.CorrectionEditing);Assert.Empty(state.CorrectionLines);return;
        }
        var pending=new TaskCompletionSource<APInvoiceCorrectionReview>(TaskCreationOptions.RunContinuationsAsynchronously);APInvoiceCorrectionDraft? captured=null;
        var load=state.LoadCorrectionReviewAsync((_,draft)=>{captured=draft;return pending.Task;});Assert.True(state.CorrectionReviewBusy);
        if(mode=="late-edit")state.SetCorrectionTotal(4m);if(mode=="late-financial")state.ClearFinancialReview();if(mode=="late-dispose")state.Dispose();
        if(mode is "late-company" or "late-account"){company.TrySelect(company.Clear(),mode=="late-account"?"other-account":"actor",new(new("tenant",mode=="late-company"?"second":"company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.CorrectInvoice"]));}
        pending.SetResult(APInvoiceCorrectionReview.Create(financial,captured!,f.Command.BindingRevision));await load;Assert.Null(state.CorrectionReview);Assert.False(state.CorrectionReviewBusy);
        Assert.Equal(mode=="late-edit",state.CorrectionEditing);Assert.Equal(2,f.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    internal sealed class Components(IServiceProvider services):IComponentActivator
    {
        public IComponent? Page,Editor;
        public IComponent CreateInstance(Type type){var result=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(type==typeof(Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench))Page=result;if(type==typeof(Beep.OilandGas.Web.Components.BusinessProcess.APInvoiceCorrectionEditor))Editor=result;return result;}
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("changed")][InlineData("null")][InlineData("edit")][InlineData("other-preparer")]
    public async Task CompleteInvoicePageReviewsExplicitCorrectionWithActualApiAndClearsUnconfirmedOrEditedEvidence(string mode)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare(within:false);await f.Record();var n=f.Existing.Existing.ReadOwner.Native;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.CorrectInvoice"];
        var actor=mode=="other-preparer"?mode:"actor";using var company=Selected(actor);if(actor!="actor"){host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);}
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider,InvoiceCorrectionEditorTests.Authentication>();services.AddSingleton<IAccountingServiceClient>(client);services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<APInvoiceWorkspaceState>();
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));var components=(Components)provider.GetRequiredService<IComponentActivator>();
            async Task Invoke(IComponent component,string method,params object?[] args){var result=component.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,args);if(result is Task task)await task;}
            await Invoke(components.Page!,"ReviewFinancial");await Invoke(components.Page!,"StateHasChanged");var editor=components.Editor!;
            if(mode=="other-preparer"){Assert.False(state.CanViewCorrection);return WebUtility.HtmlDecode(root.ToHtmlString());}
            await Invoke(editor,"Begin");Assert.True(state.CorrectionEditing);await Invoke(editor,"TotalChanged",3m);await Invoke(editor,"AmountChanged","line","QUANTITY",3m);await Invoke(editor,"AmountChanged","line","UNIT_PRICE",1m);await Invoke(editor,"AmountChanged","line","LINE_TOTAL",3m);await Invoke(editor,"ReasonChanged","Explicit accountant correction for the recorded controller request.");
            if(mode=="grant")host.Context.Permissions=["Accounting.View","Procurement.View"];if(mode=="changed")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;if(mode=="null")await Invoke(editor,"AmountChanged","line","QUANTITY",null);
            await Invoke(editor,"ReviewCorrection");if(mode=="edit"){Assert.NotNull(state.CorrectionReview);await Invoke(editor,"ReasonChanged","A changed correction explanation.");}
            await Invoke(editor,"StateHasChanged");return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(mode=="valid"){Assert.NotNull(state.CorrectionReview);Assert.Contains("Current source and correction request confirmed",html);Assert.Contains("Recorded invoice total: 2.01",html);}
        else{Assert.Null(state.CorrectionReview);Assert.DoesNotContain("Current source and correction request confirmed",html);}
        if(mode=="other-preparer")Assert.DoesNotContain("Prepare explicit correction",html);else if(mode=="changed")Assert.Contains("Correction review is unavailable or changed",html);else if(mode is "grant" or "null")Assert.Contains("Correction review could not be confirmed",html);
        Assert.Equal(2,n.Native.Writes);Assert.Equal("DRAFT",f.Existing.Stored.STATUS);Assert.Equal(2.01m,f.Existing.Stored.TOTAL_AMOUNT);
    }
}
#endif
