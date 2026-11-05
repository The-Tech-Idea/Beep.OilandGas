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
public sealed class InvoiceCorrectionWorkspaceTests
{
    private static CompanyClientContext Selected()
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.CorrectInvoice"]));return company;}
    private static async Task Edit(APInvoiceWorkspaceState state,InvoiceCorrectionCommandTests.Fixture f)
    {
        var n=f.Existing.Existing.Existing.ReadOwner.Native;await state.LoadDetailAsync("invoice",id=>n.AP.GetSourceReviewAsync(f.Command.Company,id,"actor"));await state.LoadFinancialReviewAsync((_,_)=>Task.FromResult(f.Review.Current));
        state.BeginCorrection();state.SetCorrectionTotal(3m);state.SetCorrectionAmount("line","QUANTITY",3m);state.SetCorrectionAmount("line","UNIT_PRICE",1m);state.SetCorrectionAmount("line","LINE_TOTAL",3m);state.SetCorrectionDescription("line",f.Command.Draft.Lines[0].Description);state.SetCorrectionReason(f.Command.Draft.Reason);
        await state.LoadCorrectionReviewAsync((_,draft)=>Task.FromResult(APInvoiceCorrectionReview.Create(f.Review.Current,draft,f.Command.BindingRevision)));
    }
    [Theory][InlineData("valid")][InlineData("mutated-input")][InlineData("unknown")][InlineData("missing-source")][InlineData("changed-source")][InlineData("late-financial")][InlineData("late-company")][InlineData("late-account")][InlineData("late-dispose")]
    public async Task ScopedCorrectionIntentFreezesSurvivesUncertaintyAndRejectsLateOrIncompleteConfirmation(string mode)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native;using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Edit(state,f);
        async Task<APInvoiceCorrectionResult> Record(APInvoiceCorrectionSubmission input)
        {var result=await n.AP.RecordCorrectionAsync(input.Command,input.Review,n.Authority,()=>InvoiceVarianceDecisionReviewTests.Financial(f.Existing.Existing,"actor"));if(mode=="mutated-input")input.Review.Current.Invoice.Invoice.TOTAL_AMOUNT=999m;return result;}
        Task<APInvoiceSourceReview?> Source(string id)=>n.AP.GetSourceReviewAsync(f.Command.Company,id,"actor");
        if(mode.StartsWith("late-",StringComparison.Ordinal))
        {
            var pending=new TaskCompletionSource<APInvoiceCorrectionResult>(TaskCreationOptions.RunContinuationsAsynchronously);APInvoiceCorrectionSubmission? captured=null;
            var load=state.RecordCorrectionAsync(input=>{captured=input;return pending.Task;},Source);Assert.True(state.CorrectionCommandBusy);Assert.True(state.HasUnconfirmedCorrectionIntent);
            var recorded=await Record(captured!);
            if(mode=="late-financial")state.ClearFinancialReview();if(mode=="late-dispose")state.Dispose();
            if(mode is "late-company" or "late-account")company.TrySelect(company.Clear(),mode=="late-account"?"another":"actor",new(new("tenant",mode=="late-company"?"another":"company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.CorrectInvoice"]));
            pending.SetResult(recorded);await load;Assert.Null(state.CorrectionResult);Assert.True(state.HasUnconfirmedCorrectionIntent);Assert.False(state.CorrectionCommandBusy);
            if(mode is "late-company" or "late-account")Assert.False(state.CanViewCorrectionIntent);Assert.Equal(4,n.Native.Writes);return;
        }
        if(mode=="unknown")n.Native.FailCommit=true;
        async Task<APInvoiceSourceReview?> Read(string id)
        {
            if(mode=="missing-source")return null;var source=(await Source(id))!;if(mode=="changed-source")source.Lines[0].ITEM_DESCRIPTION="Unconfirmed changed source";return source;
        }
        if(mode is not ("valid" or "mutated-input"))await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordCorrectionAsync(Record,Read));else await state.RecordCorrectionAsync(Record,Read);
        if(mode is "valid" or "mutated-input")
        {Assert.False(state.HasUnconfirmedCorrectionIntent);Assert.NotNull(state.CorrectionResult);Assert.Equal(3m,state.Detail!.Amount);var copy=state.CorrectionResult!;copy.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(3m,state.CorrectionResult!.Invoice.TOTAL_AMOUNT);Assert.Null(state.FinancialReview);Assert.Null(state.CorrectionReview);}
        else
        {
            Assert.True(state.HasUnconfirmedCorrectionIntent);Assert.Null(state.CorrectionResult);var original=state.CorrectionIntent!;var intent=original.Command.IntentId;original.Review.Current.Invoice.Invoice.TOTAL_AMOUNT=999m;Assert.Equal(2.01m,state.CorrectionIntent!.Review.Current.Invoice.Invoice.TOTAL_AMOUNT);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceAsync(_=>throw new InvalidOperationException(),Source));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceDecisionAsync("REQUEST_CORRECTION","Another",_=>throw new InvalidOperationException(),Source));Assert.Throws<InvalidOperationException>(()=>state.BeginCorrection());
            state.Clear();Assert.True(state.HasUnconfirmedCorrectionIntent);await state.LoadDetailAsync("invoice",Source);Assert.True(state.CanReconcileCorrection);n.Native.FailCommit=false;
            await state.RecordCorrectionAsync(Record,Source);Assert.Equal(intent,state.CorrectionResult!.Command.IntentId);Assert.False(state.HasUnconfirmedCorrectionIntent);
        }
        Assert.Equal(4,n.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("unknown")][InlineData("grant")][InlineData("contributor")][InlineData("auth-account")]
    public async Task CompleteInvoiceScreenSavesOrReconcilesExactCorrectionWithActualApi(string mode)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.CorrectInvoice"];
        using var company=Selected();var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider,InvoiceCorrectionEditorTests.Authentication>();services.AddSingleton<IAccountingServiceClient>(client);services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,InvoiceCorrectionEditorTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<APInvoiceWorkspaceState>();
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));var components=(InvoiceCorrectionEditorTests.Components)provider.GetRequiredService<IComponentActivator>();
            async Task Invoke(IComponent component,string method,params object?[] args){var result=component.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,args);if(result is Task task)await task;}
            await Invoke(components.Page!,"ReviewFinancial");await Invoke(components.Page!,"StateHasChanged");var editor=components.Editor!;await Invoke(editor,"Begin");await Invoke(editor,"TotalChanged",3m);await Invoke(editor,"AmountChanged","line","QUANTITY",3m);await Invoke(editor,"AmountChanged","line","UNIT_PRICE",1m);await Invoke(editor,"AmountChanged","line","LINE_TOTAL",3m);await Invoke(editor,"ReasonChanged",f.Command.Draft.Reason);await Invoke(editor,"ReviewCorrection");Assert.NotNull(state.CorrectionReview);
            if(mode is "unknown" or "auth-account")n.Native.FailCommit=true;if(mode=="grant")host.Context.Permissions=["Accounting.View","Procurement.View"];if(mode=="contributor")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
            await Invoke(editor,"SaveCorrection");await Invoke(editor,"StateHasChanged");
            if(mode=="unknown")
            {
                Assert.True(state.HasUnconfirmedCorrectionIntent);var before=WebUtility.HtmlDecode(root.ToHtmlString());Assert.Contains("Correction outcome is unconfirmed",before);Assert.Contains("Reconcile original correction intent",before);Assert.DoesNotContain("Start company accounting workflow",before);
                var intent=state.CorrectionIntent!.Command.IntentId;n.Native.FailCommit=false;await Invoke(editor,"SaveCorrection");Assert.Equal(intent,state.CorrectionResult!.Command.IntentId);await Invoke(editor,"StateHasChanged");
            }
            if(mode=="auth-account")
            {
                Assert.True(state.HasUnconfirmedCorrectionIntent);var calls=host.Context.Calls;((InvoiceCorrectionEditorTests.Authentication)provider.GetRequiredService<AuthenticationStateProvider>()).Change("another");
                Assert.Null(state.Detail);Assert.Null(state.Lines);await Invoke(components.Page!,"StateHasChanged");await Invoke(editor,"SaveCorrection");Assert.Equal(calls,host.Context.Calls);await Invoke(editor,"StateHasChanged");
            }
            return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(mode is "valid" or "unknown"){Assert.NotNull(state.CorrectionResult);Assert.Contains("Exact corrected invoice source and history confirmed",html);Assert.Contains("Independent financial rereview and correction task completion remain pending",html);Assert.Equal(4,n.Native.Writes);}
        else if(mode=="auth-account"){Assert.Null(state.CorrectionResult);Assert.True(state.HasUnconfirmedCorrectionIntent);Assert.DoesNotContain("Original correction intent:",html);Assert.DoesNotContain("Original correction reason:",html);Assert.Equal(4,n.Native.Writes);}
        else{Assert.Null(state.CorrectionResult);Assert.True(state.HasUnconfirmedCorrectionIntent);Assert.Contains("Correction outcome is unconfirmed",html);Assert.Equal(2,n.Native.Writes);}
        Assert.Equal("DRAFT",f.Existing.Existing.Stored.STATUS);
    }
}
#endif
