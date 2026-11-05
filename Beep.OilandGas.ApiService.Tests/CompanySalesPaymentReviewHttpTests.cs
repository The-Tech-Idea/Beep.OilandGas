using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed partial class CompanySalesPaymentReviewTests
{
    [Fact]public async Task ActualControllerPaymentApiSeparatesReviewWriteRecoveryAndProducerPrivacy()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var p=shared.Base.Posting;var native=p.Reviewed.Draft.Native;var grants=new[]{"Accounting.View","Accounting.ReviewSalesPaymentSource","Accounting.ReviewSalesPayment","Accounting.RecordSalesPaymentDecision"};
        await using var host=await shared.Base.Baseline.NativeFixture.Receivable.Data.Invoice.Data.Start(permissions:grants,configure:(services,context)=>{context.Actor="controller";context.PolicyBinding=shared.Review.Current.Ledger.BindingRevision;services.AddSingleton<IARService>(p.AR);services.AddScoped<CompanySalesReceivableReads>();services.AddSingleton(p.Reviewed.Draft.Service);});host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","controller");using var company=CompanyGLAccountHttpTests.Selected(actor:"controller",grants:grants);var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        var source=await client.GetCompanySalesPaymentReviewAsync(shared.Draft.AR_PAYMENT_ID);source.Require();native.Source.Invocations.Clear();native.Editor.Invocations.Clear();var command=new CompanySalesPaymentReviewCommand(Guid.NewGuid().ToString("N"),source,"REJECT","ISSUE","Original amount evidence issue","CONFIRMED","Original receipt evidence","Return proposal to its maker");host.Context.Permissions=["Accounting.View","Accounting.ReviewSalesPaymentSource","Accounting.ReviewSalesPayment"];var denied=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordCompanySalesPaymentReviewAsync(command));Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(0,native.Writes);
        host.Context.Permissions=grants;native.Source.Invocations.Clear();commitLost=true;await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordCompanySalesPaymentReviewAsync(command));commitLost=false;native.FailCommit=false;native.Source.Invocations.Clear();var row=await client.RecordCompanySalesPaymentReviewAsync(command);Assert.Equal("REJECTED",row.STATUS);Assert.Equal(2,native.Writes);host.Context.Permissions=["Accounting.View","Accounting.ReviewSalesPaymentSource"];var recovery=await client.GetCompanySalesPaymentReviewRecoveryAsync(shared.Draft.AR_PAYMENT_ID);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(recovery.Intent!.Restore()));Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(recovery.Outcome));var freshDenied=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanySalesPaymentReviewAsync(shared.Draft.AR_PAYMENT_ID));Assert.Equal(HttpStatusCode.Forbidden,freshDenied.StatusCode);
        host.Context.Actor="collector";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","collector");Assert.True(company.TrySelect(company.Clear(),"collector",new(shared.Review.Current.Ledger.Company,["ConfiguredCompanyRole"],host.Context.Permissions)));var privateDenied=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanySalesPaymentReviewRecoveryAsync(shared.Draft.AR_PAYMENT_ID));Assert.Equal(HttpStatusCode.Forbidden,privateDenied.StatusCode);var producer=await client.GetCompanySalesPaymentDraftAsync(shared.Draft.AR_PAYMENT_ID);Assert.Equal(JsonSerializer.Serialize(shared.Draft),JsonSerializer.Serialize(producer));Assert.DoesNotContain("REVIEW_WORKFLOW_JSON",JsonSerializer.Serialize(producer));native.Source.Invocations.Clear();var facts=await client.GetCompanySalesPaymentSourceAsync(shared.Review.Current.Ledger.ReceivableId,shared.Review.Current.Ledger.BookId,shared.Review.Current.Ledger.CurrencyCode);Assert.Equal("REJECTED",Assert.Single(facts.Payments).Status);Assert.Equal(0,facts.UnreleasedProposedAmount);Assert.DoesNotContain("COMMAND_JSON",JsonSerializer.Serialize(facts));Assert.Equal(2,native.Writes);native.Source.Invocations.Clear();await RenderRejectedMaker(company,client);
    }
}
#endif
