#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Components.BusinessProcess;
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
public sealed class InvoiceCorrectionDecisionWorkspaceTests
{
    private static readonly string[] Grants=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"];
    private static CompanyClientContext Selected(string actor="reviewer")
    {var c=new CompanyClientContext();c.TrySelect(c.Clear(),actor,new(new("tenant","company"),["ConfiguredCompanyRole"],Grants));return c;}
    [Theory][InlineData("valid")][InlineData("source")][InlineData("company")][InlineData("account")][InlineData("dispose")][InlineData("clear")][InlineData("policy")][InlineData("allocations")][InlineData("failure")][InlineData("maker")][InlineData("preparer")]
    public async Task ScopedLatestCorrectionReviewIsCopiedAndRejectsLatePublication(string mode)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();await f.Record();var n=f.Existing.Existing.Existing.ReadOwner.Native;
        var actor=mode=="maker"?"source-maker":mode=="preparer"?"actor":"reviewer";
        using var company=Selected(actor);using var state=new APInvoiceWorkspaceState(company);await state.LoadDetailAsync("invoice",id=>n.AP.GetSourceReviewAsync(f.Command.Company,id,actor));
        if(mode is "maker" or "preparer"){Assert.False(state.CanViewCorrectionDecision);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadCorrectionDecisionReviewAsync((_,_)=>throw new Exception("Must not read")));return;}
        var current=await InvoiceVarianceDecisionReviewTests.Financial(f.Existing.Existing,actor);var review=APInvoiceCorrectionDecisionReview.Create(current,f.Command.BindingRevision);
        var pending=new TaskCompletionSource<APInvoiceCorrectionDecisionReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var load=state.LoadCorrectionDecisionReviewAsync((id,revision)=>{Assert.Equal("invoice",id);Assert.Equal(current.Invoice.Revision,revision);return pending.Task;});Assert.True(state.CorrectionDecisionReviewBusy);
        if(mode=="source")await state.LoadDetailAsync("invoice",id=>n.AP.GetSourceReviewAsync(f.Command.Company,id,actor));
        if(mode=="clear")state.ClearFinancialReview();if(mode=="policy")state.ClearMatchingPolicies();if(mode=="allocations")state.ClearReceiptAllocations();if(mode=="dispose")state.Dispose();
        if(mode is "company" or "account")company.TrySelect(company.Clear(),mode=="account"?"another":actor,new(new("tenant",mode=="company"?"another":"company"),["ConfiguredCompanyRole"],Grants));
        if(mode=="failure"){pending.SetException(new InvalidOperationException("Missing current evidence"));await Assert.ThrowsAsync<InvalidOperationException>(()=>load);}else{pending.SetResult(review);await load;}
        Assert.False(state.CorrectionDecisionReviewBusy);
        if(mode=="valid")
        {Assert.NotNull(state.CorrectionDecisionReview);review.Current.Invoice.Invoice.TOTAL_AMOUNT=99m;var exposed=state.CorrectionDecisionReview!;exposed.Current.Invoice.Invoice.TOTAL_AMOUNT=88m;Assert.Equal(3m,state.CorrectionDecisionReview!.Current.Invoice.Invoice.TOTAL_AMOUNT);}
        else Assert.Null(state.CorrectionDecisionReview);
        Assert.Equal(4,n.Native.Writes);
    }
    private sealed class Components(IServiceProvider services):IComponentActivator
    {public APInvoiceCorrectionDecisionPanel? Panel;public IComponent CreateInstance(Type type){var c=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(c is APInvoiceCorrectionDecisionPanel p)Panel=p;return c;}}
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("auth-account")][InlineData("contributor")][InlineData("maker")]
    public async Task ActualPanelReviewsThroughApiAndClearsAuthenticatedAccountEvidence(string mode)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();await f.Record();var n=f.Existing.Existing.Existing.ReadOwner.Native;var actor=mode=="maker"?"source-maker":"reviewer";
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(s,_)=>{s.AddSingleton(f.Existing.Existing.Existing.Owner);s.AddSingleton(n.AP);s.AddSingleton(n.PO);});host.Context.Actor=actor;host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=Grants;host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);
        using var company=Selected(actor);var auth=new InvoiceCorrectionEditorTests.Authentication();auth.Change(actor);var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());using var state=new APInvoiceWorkspaceState(company);await state.LoadDetailAsync("invoice",client.GetAPInvoiceAsync);
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<APInvoiceCorrectionDecisionPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel!;
            async Task Invoke(string method){var result=panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);if(result is Task task)await task;}
            if(mode=="grant")host.Context.Permissions=["Accounting.View","Procurement.View"];
            if(mode=="contributor")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
            if(mode!="maker")await Invoke("ReviewCorrection");
            if(mode=="auth-account"){Assert.NotNull(state.CorrectionDecisionReview);auth.Change("another");await Invoke("ReviewCorrection");Assert.Null(state.Detail);Assert.Null(state.Lines);}
            await Invoke("StateHasChanged");return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(mode is "valid" or "contributor"){Assert.NotNull(state.CorrectionDecisionReview);Assert.Contains("Correction version: 1",html);Assert.Contains("Current corrected total: 3",html);Assert.Contains("This diagnostic review does not confirm acknowledgement or configured task completion",html);}
        else{Assert.Null(state.CorrectionDecisionReview);Assert.DoesNotContain("Correction intent:",html);}
        Assert.Equal(4,n.Native.Writes);Assert.Equal("DRAFT",f.Existing.Existing.Stored.STATUS);
    }
}
#endif
