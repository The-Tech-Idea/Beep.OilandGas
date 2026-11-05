#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesPaymentScreenTests(CompanySalesPaymentDraftTests.PostedFixture shared):IClassFixture<CompanySalesPaymentDraftTests.PostedFixture>
{
    private sealed class Authentication:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"collector"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator{internal Beep.OilandGas.Web.Components.Workflow.SalesPaymentWorkbench? Workbench;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.SalesPaymentWorkbench workbench)Workbench=workbench;return component;}}
    [Fact] public async Task CollectorFormRequiresEvidenceAndRetainsNativeProposalUntilExactRecoveryAndPublicConfirmation()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var p=shared.Posting;var native=p.Reviewed.Draft.Native;native.Rows.Clear();native.Rows.AddRange(shared.Rows.Select(CompanyGLAccountReviewTests.Fixture.Clone));native.Writes=0;native.Transactions.Clear();native.Source.Setup(value=>value.InsertEntity("AR_PAYMENT",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((AR_PAYMENT)value));return new ErrorsInfo{Flag=Errors.Ok};});using var company=CompanyGLAccountHttpTests.Selected(actor:"collector",grants:["Accounting.View","Accounting.ReviewSalesPaymentSource","Accounting.PrepareSalesPayment"]);using var state=new CompanySalesReceivableWorkspaceState(company);state.AttachOwner(new object());var client=new Mock<IAccountingServiceClient>();var lost=true;var bodies=new List<string>();var original=shared.Source;
        client.Setup(value=>value.GetCompanySalesPaymentSourceAsync(original.Ledger.ReceivableId,original.Ledger.BookId,original.Ledger.CurrencyCode)).Returns(()=>p.AR.GetCompanySalesPaymentSourceAsync(original.Ledger.Company,"collector",original.Ledger.BindingRevision,original.Ledger.BookId,original.Ledger.CurrencyCode,original.Ledger.ReceivableId,p.Reviewed.Draft.Service,p.Guard,p.Field));
        client.Setup(value=>value.PrepareCompanySalesPaymentAsync(It.IsAny<CompanySalesPaymentCommand>())).Returns((CompanySalesPaymentCommand command)=>{bodies.Add(JsonSerializer.Serialize(command));return p.AR.PrepareCompanySalesPaymentAsync(command,p.Reviewed.Draft.Service,p.Guard,p.Field);});
        client.Setup(value=>value.GetCompanySalesPaymentDraftAsync(It.IsAny<string>())).Returns((string id)=>lost?Task.FromException<AR_PAYMENT?>(new InvalidOperationException("Lost followup")):p.AR.GetCompanySalesPaymentDraftAsync(original.Ledger.Company,"collector",id,p.Guard,p.Field));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"ReceivableId",original.Ledger.ReceivableId}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesPaymentWorkbench>(parameters);var components=(Components)provider.GetRequiredService<IComponentActivator>();void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);void Render()=>components.Workbench!.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null);async Task Action(string name){await(Task)components.Workbench!.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Render();}
            Field("_book",original.Ledger.BookId);Field("_currency",original.Ledger.CurrencyCode);Render();await Action("Load");await Action("Prepare");Assert.Equal(0,native.Writes);Assert.False(state.HasPendingPayment);Field("_number","screen-payment-001");Field("_date",original.Ledger.Journals.Single().PostingDate);Field("_amount",1m);Field("_method","BANK_TRANSFER");Field("_payer",original.CustomerId);Field("_reference","screen-bank-trace-001");Field("_evidence","Original receipt evidence");Field("_observed","2026-10-12T00:00:00+00:00");Field("_reason","Reviewed receipt proposal");Render();await Action("Prepare");Assert.True(state.HasPendingPayment);Assert.Equal(1,native.Writes);var retained=JsonSerializer.Serialize(state.PendingPayment);Assert.Contains("exact original payment request is retained",root.ToHtmlString());Assert.False(state.CanPrepareJournal);Assert.False(state.CanRecordJournalReview);
            components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesPaymentWorkbench>(parameters);Assert.Equal(retained,JsonSerializer.Serialize(state.PendingPayment));lost=false;await Action("Retry");Assert.False(state.HasPendingPayment);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(1,native.Writes);Assert.Contains("payment proposal is recorded and confirmed",root.ToHtmlString());var id=state.PaymentResult!.AR_PAYMENT_ID;Assert.True(company.TrySelect(company.Clear(),"collector",new(original.Ledger.Company,["ConfiguredCompanyRole"],["Accounting.View","Accounting.ReviewSalesPaymentSource"])));Field("_recoveryId",id);Render();await Action("Recover");Assert.NotNull(state.PaymentResult);Assert.Equal(1,native.Writes);Assert.DoesNotContain("Prepare reviewed payment proposal",root.ToHtmlString());
        });
    }
}
#endif
