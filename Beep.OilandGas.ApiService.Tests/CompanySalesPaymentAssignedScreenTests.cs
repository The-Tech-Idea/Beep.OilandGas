using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
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
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed partial class CompanySalesPaymentReviewTests
{
    [Fact]public async Task AssignedPaymentFormUsesOriginalPublicSourceAndRejectsChangedNativeIdentityWithoutGlobalFieldSelection()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var source=CompanySalesPaymentWorkflowSource.Create(shared.Review.Current,shared.Draft.AR_PAYMENT_ID);var assigned=new CompanySalesPaymentProcessSourceReview(source.Company,"controller","opaque/assigned-payment%2F",source.PaymentId,source.SourceRevision,new string('B',64),source);assigned.Require(source.Company,"controller",assigned.InstanceId);
        using var company=CompanyGLAccountHttpTests.Selected(actor:"controller",grants:["Accounting.View","Accounting.ReviewSalesPaymentSource","Accounting.ReviewSalesPayment","Accounting.RecordSalesPaymentDecision"]);using var state=new CompanySalesReceivableWorkspaceState(company);var client=new Mock<IAccountingServiceClient>();var altered=false;client.Setup(value=>value.GetCompanySalesPaymentReviewAsync(source.PaymentId)).Returns(()=>{var value=CompanyInventorySourceReview.Copy(shared.Review);if(altered)value.Original.PAYMENT_NUMBER="Changed native draft";return Task.FromResult(value);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"AssignedSource",assigned}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.AssignedSalesPaymentWorkbench>(parameters);var component=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;var flags=BindingFlags.NonPublic|BindingFlags.Instance;
            Assert.Equal(source.PaymentId,component.GetType().GetField("_id",flags)!.GetValue(component));Assert.Contains(shared.Draft.PAYMENT_NUMBER,root.ToHtmlString());Assert.Contains(source.Current.Ledger.FieldId,root.ToHtmlString());await(Task)component.GetType().GetMethod("Load",flags)!.Invoke(component,null)!;component.GetType().GetMethod("StateHasChanged",flags)!.Invoke(component,null);Assert.NotNull(state.PaymentReviewSource);Assert.Contains("Record independent payment decision",root.ToHtmlString());
            altered=true;await(Task)component.GetType().GetMethod("Load",flags)!.Invoke(component,null)!;component.GetType().GetMethod("StateHasChanged",flags)!.Invoke(component,null);Assert.Null(state.PaymentReviewSource);Assert.False(state.CanRecordPaymentReview);Assert.Equal(0,shared.Base.Posting.Reviewed.Draft.Native.Writes);client.Verify(value=>value.RecordCompanySalesPaymentReviewAsync(It.IsAny<CompanySalesPaymentReviewCommand>()),Times.Never);
            company.Clear();Assert.Null(state.PaymentReviewSource);Assert.Null(state.PendingPaymentReview);Assert.False(state.CanRecordPaymentReview);
        });
    }
}
#endif
