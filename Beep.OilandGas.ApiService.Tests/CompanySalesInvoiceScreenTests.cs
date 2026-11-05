#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanySalesInvoiceScreenTests
{
    private sealed class Authentication:AuthenticationStateProvider
    {
        public string Actor="preparer";public bool Role=true;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(User(Actor,Role)));
        public void Change(string actor){Actor=actor;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private static ClaimsPrincipal User(string actor,bool role=true)=>new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,actor)}.Concat(role?[new Claim(ClaimTypes.Role,"ConfiguredCompanyRole")]:[]),"test"));
    private sealed class Components:IComponentActivator
    {
        public Beep.OilandGas.Web.Components.Workflow.SalesInvoiceWorkbench? Workbench;
        public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.SalesInvoiceWorkbench workbench)Workbench=workbench;return component;}
    }
    [Theory][InlineData("success")][InlineData("unknown")][InlineData("recreated")][InlineData("failed-write")]
    [InlineData("missing-tax")][InlineData("precision")][InlineData("date-time")][InlineData("due-before")][InlineData("number")][InlineData("notes")][InlineData("permission")]
    public async Task BillingFormUsesActualClientNativeEvidenceExplicitFactsAndExactOriginalRecovery(string mode)
    {
        var f=new CompanySalesInvoiceHttpTests.Fixture();await f.Prepare("partial");await using var host=await f.Start();using var company=f.Selected("Accounting.PrepareSalesInvoice");
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);var auth=new Authentication{Actor=f.Actor};
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();
        services.AddSingleton(company);services.AddScoped<CompanySalesInvoiceWorkspaceState>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddSingleton<IFieldSelectionContext>(Mock.Of<IFieldSelectionContext>());
        services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company),Microsoft.Extensions.Logging.Abstractions.NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>()));
        services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var state=provider.GetRequiredService<CompanySalesInvoiceWorkspaceState>();string? original=null;
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"Ticket",f.Ticket}});
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesInvoices>(parameters);var workbench=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;
            void Field(string name,object? value)=>workbench.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(workbench,value);
            void Refresh()=>workbench.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench,null);
            async Task Invoke(string method){await (Task)workbench.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench,null)!;Refresh();}
            Assert.NotNull(state.Workspace);Assert.Contains("Sales invoice preparation",root.ToHtmlString());Assert.Contains("49.38",root.ToHtmlString());Assert.Contains("Record invoice draft proposal",root.ToHtmlString());
            Assert.Null(workbench.GetType().GetField("_tax",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(workbench));
            Assert.Null(workbench.GetType().GetField("_date",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(workbench));
            var facts=CompanySalesInvoiceDraftTests.Facts();Field("_number",mode=="number"?"":facts.InvoiceNumber);Field("_date",mode=="date-time"?facts.InvoiceDate.AddHours(12):facts.InvoiceDate);Field("_due",mode=="due-before"?facts.InvoiceDate.AddDays(-1):facts.DueDate);
            Field("_tax",mode=="missing-tax"?null:mode=="precision"?(decimal?)0.0001m:facts.TaxAmount);Field("_charges",facts.ChargesAmount);Field("_taxNotes",mode=="notes"?"":facts.TaxReviewNotes);
            Field("_chargeNotes",facts.ChargesReviewNotes);Field("_rightsNotes",facts.EntitlementReviewNotes);Field("_reason",facts.Reason);Refresh();
            if(mode=="permission")host.Context.Permissions=["Accounting.View"];
            if(mode is "unknown" or "recreated")f.UncertainInvoiceCommit=true;if(mode=="failed-write")f.FailInvoiceWrite=true;
            await Invoke("Record");
            if(mode is "unknown" or "recreated" or "failed-write")
            {
                Assert.True(state.HasPendingIntent);Assert.Null(state.Result);original=JsonSerializer.Serialize(state.Pending);Assert.Contains("original invoice request remains unconfirmed",root.ToHtmlString());
                f.Data.Native.FailCommit=false;f.UncertainInvoiceCommit=false;f.FailInvoiceWrite=false;
                if(mode=="recreated")
                {
                    workbench.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesInvoices>(parameters);workbench=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;
                    Assert.False(state.HasPendingIntent);Assert.NotNull(state.Result);Assert.Equal(3,f.Data.Native.Writes);
                }
                else{Field("_reason","Changed input must not replace the retained original reason.");await Invoke("Retry");}
            }
            return root.ToHtmlString();
        });
        if(mode is "success" or "unknown" or "recreated" or "failed-write")
        {
            Assert.NotNull(state.Result);Assert.False(state.HasPendingIntent);Assert.Contains("Invoice draft and original outcome receipt confirmed",html);
            var command=state.Result!.Intent.Restore(f.Data.Current.Company,f.Actor);Assert.Equal(CompanySalesInvoiceDraftTests.Facts(),command.Facts);state.Result.Require(command);
            if(original is not null)Assert.Equal(original,JsonSerializer.Serialize(command));Assert.Equal(mode=="failed-write"?4:3,f.Data.Native.Writes);
            Assert.Equal(49.38m,state.Result.Invoice.TOTAL_AMOUNT);
        }
        else{Assert.Null(state.Result);Assert.Empty(f.Data.Native.Rows.OfType<INVOICE>());Assert.Empty(f.Data.Native.Rows.OfType<SALES_INVOICE_INTENT>());Assert.Equal(0,f.Data.Native.Writes);}
    }
    [Theory][InlineData("view-only")][InlineData("policy-grant")][InlineData("role")][InlineData("account")][InlineData("policy-pending")][InlineData("policy-missing")]
    public async Task ReadOnlyAndWrongAccountFormsCannotPrepareInvoices(string mode)
    {
        var f=new CompanySalesInvoiceHttpTests.Fixture();await f.Prepare(approvePolicy:mode!="policy-pending");if(mode=="policy-missing")f.Data.Native.Rows.RemoveAll(row=>row is ACCOUNTING_METHOD);
        await using var host=await f.Start();using var company=f.Selected(mode=="policy-grant"?"Accounting.PrepareBillingPolicy":mode is "policy-pending" or "policy-missing"?"Accounting.PrepareSalesInvoice":"Accounting.View");
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);var auth=new Authentication{Actor=mode=="account"?"another":f.Actor,Role=mode!="role"};
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanySalesInvoiceWorkspaceState>();
        services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>(await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesInvoiceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"TicketId",f.Ticket}}))).ToHtmlString());
        Assert.DoesNotContain("Record invoice draft proposal",html);if(mode is "account" or "role")Assert.DoesNotContain("Price extension before",html);else Assert.Contains("Price extension before",html);
        if(mode is "policy-pending" or "policy-missing")Assert.Contains("An independently approved company billing policy for this recorded currency is unavailable",html);Assert.Equal(0,f.Data.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("auth")][InlineData("detached")]
    public async Task LateOutcomeIsRedactedAndOriginalNativeRequestSurvivesPresentationInvalidation(string mode)
    {
        var f=new CompanySalesInvoiceHttpTests.Fixture();await f.Prepare();await using var host=await f.Start();using var company=f.Selected("Accounting.PrepareSalesInvoice");
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);using var state=new CompanySalesInvoiceWorkspaceState(company);var owner=new object();state.AttachOwner(owner);state.Admit(User(f.Actor));await state.LoadAsync(f.Ticket,client);
        var committed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);f.Data.Native.AfterCommit=()=>{if(f.Data.Native.Rows.OfType<SALES_INVOICE_OUTCOME>().Any())committed.TrySetResult();};
        f.OnInvoiceWrite=()=>{if(mode=="company")company.Clear();else if(mode=="auth")state.Invalidate();else state.DetachOwner(owner);};
        if(mode=="company"){var refused=await Record.ExceptionAsync(()=>state.PrepareAsync(CompanySalesInvoiceDraftTests.Facts(),client));Assert.True(refused is OperationCanceledException or InvalidOperationException);}
        else await state.PrepareAsync(CompanySalesInvoiceDraftTests.Facts(),client);
        await committed.Task.WaitAsync(TimeSpan.FromSeconds(30));
        Assert.True(state.HasPendingIntent);Assert.Null(state.Result);Assert.Null(state.Workspace);Assert.Null(state.Pending);Assert.Equal(3,f.Data.Native.Writes);
        f.OnInvoiceWrite=null;company.TrySelect(company.Clear(),f.Actor,new(f.Data.Current.Company,["ConfiguredCompanyRole"],["Accounting.View"]));state.AttachOwner(new object());state.Admit(User(f.Actor));
        await state.LoadAsync(f.Ticket,client);Assert.NotNull(state.Result);Assert.False(state.HasPendingIntent);Assert.Equal(3,f.Data.Native.Writes);
    }
    [Fact] public async Task NativeCommandReopensInNewCircuitAfterFailedInvoiceAndKeepsExactOriginalFacts()
    {
        var f=new CompanySalesInvoiceHttpTests.Fixture();await f.Prepare();await using var host=await f.Start();using var company=f.Selected("Accounting.PrepareSalesInvoice");var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        CompanySalesInvoiceDraftCommand original;
        using(var first=new CompanySalesInvoiceWorkspaceState(company))
        {first.AttachOwner(new object());first.Admit(User(f.Actor));await first.LoadAsync(f.Ticket,client);f.FailInvoiceWrite=true;await Assert.ThrowsAsync<OilGasApiException>(()=>first.PrepareAsync(CompanySalesInvoiceDraftTests.Facts(),client));original=first.Pending!;}
        using var recovered=new CompanySalesInvoiceWorkspaceState(company);recovered.AttachOwner(new object());recovered.Admit(User(f.Actor));await recovered.LoadAsync(f.Ticket,client);
        Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(recovered.Pending));Assert.False(recovered.CanPrepare);f.FailInvoiceWrite=false;await recovered.SubmitPendingAsync(client);recovered.Result!.Require(original);
        Assert.Single(f.Data.Native.Rows.OfType<SALES_INVOICE_INTENT>());Assert.Single(f.Data.Native.Rows.OfType<INVOICE>());Assert.Single(f.Data.Native.Rows.OfType<SALES_INVOICE_OUTCOME>());
    }
    [Fact] public async Task MissingReceiptInConfirmationCannotClearOriginalRequestOrPresentSuccess()
    {
        var f=new CompanySalesInvoiceHttpTests.Fixture();await f.Prepare();await using var host=await f.Start();using var company=f.Selected("Accounting.PrepareSalesInvoice");var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        using var state=new CompanySalesInvoiceWorkspaceState(company);state.AttachOwner(new object());state.Admit(User(f.Actor));await state.LoadAsync(f.Ticket,client);
        var damaged=new Mock<IAccountingServiceClient>();damaged.Setup(value=>value.PrepareCompanySalesInvoiceAsync(It.IsAny<CompanySalesInvoiceDraftCommand>())).Returns((CompanySalesInvoiceDraftCommand command)=>client.PrepareCompanySalesInvoiceAsync(command));
        damaged.Setup(value=>value.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket)).Returns(async()=>{var loaded=await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket);return loaded with{Outcome=loaded.Outcome! with{Receipt=null!}};});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.PrepareAsync(CompanySalesInvoiceDraftTests.Facts(),damaged.Object));Assert.True(state.HasPendingIntent);Assert.Null(state.Result);Assert.Equal(3,f.Data.Native.Writes);
        var original=state.Pending!;await state.LoadAsync(f.Ticket,client);state.Result!.Require(original);Assert.False(state.HasPendingIntent);Assert.Equal(3,f.Data.Native.Writes);
    }
}
#endif
