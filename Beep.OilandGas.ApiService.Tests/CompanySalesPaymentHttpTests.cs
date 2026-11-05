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
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesPaymentHttpTests(CompanySalesPaymentDraftTests.PostedFixture shared):IClassFixture<CompanySalesPaymentDraftTests.PostedFixture>
{
    [Fact] public async Task ActualPaymentApiUsesCurrentRolesFieldPurposeAndReadOnlyOriginalRecovery()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var p=shared.Posting;var native=p.Reviewed.Draft.Native;native.Rows.Clear();native.Rows.AddRange(shared.Rows.Select(CompanyGLAccountReviewTests.Fixture.Clone));native.Writes=0;native.Transactions.Clear();native.Source.Setup(value=>value.InsertEntity("AR_PAYMENT",Moq.It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((AR_PAYMENT)value));return new TheTechIdea.Beep.ConfigUtil.ErrorsInfo{Flag=TheTechIdea.Beep.ConfigUtil.Errors.Ok};});
        var grants=new[]{"Accounting.View","Accounting.ReviewSalesPaymentSource","Accounting.PrepareSalesPayment"};await using var host=await shared.Baseline.NativeFixture.Receivable.Data.Invoice.Data.Start(permissions:grants,configure:(services,context)=>{context.Actor="collector";context.PolicyBinding=shared.Source.Ledger.BindingRevision;services.AddSingleton<IARService>(p.AR);services.AddScoped<CompanySalesReceivableReads>();services.AddSingleton(p.Reviewed.Draft.Service);});host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","collector");using var company=CompanyGLAccountHttpTests.Selected(actor:"collector",grants:grants);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var source=await client.GetCompanySalesPaymentSourceAsync(shared.Source.Ledger.ReceivableId,shared.Source.Ledger.BookId,shared.Source.Ledger.CurrencyCode);source.Require(source.Ledger.Company,"collector",source.Ledger.ReceivableId,source.Ledger.BookId,source.Ledger.CurrencyCode);var command=new CompanySalesPaymentCommand(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),source,"api-payment-001",source.Ledger.Journals.Single().PostingDate,1,"BANK_TRANSFER",source.CustomerId,"api-bank-trace-001","Actual original receipt reference",new DateTime(2026,10,12,0,0,0,DateTimeKind.Utc),"Original API receipt proposal");
        host.Context.Permissions=["Accounting.View","Accounting.ReviewSalesPaymentSource"];var denied=await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareCompanySalesPaymentAsync(command));Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(0,native.Writes);host.Context.Permissions=grants;native.FailCommit=true;await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareCompanySalesPaymentAsync(command));native.FailCommit=false;var row=await client.PrepareCompanySalesPaymentAsync(command);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(CompanySalesPaymentProof.Require(row).Command));Assert.Equal(1,native.Writes);var updated=await client.GetCompanySalesPaymentSourceAsync(source.Ledger.ReceivableId,source.Ledger.BookId,source.Ledger.CurrencyCode);Assert.Single(updated.Payments);Assert.Equal(1,updated.UnreleasedProposedAmount);Assert.DoesNotContain("PREPARATION_WORKFLOW_JSON",JsonSerializer.Serialize(updated));host.Context.Permissions=["Accounting.View","Accounting.ReviewSalesPaymentSource"];Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(await client.GetCompanySalesPaymentDraftAsync(command.Id)));
        host.Context.Actor="another";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","another");Assert.True(company.TrySelect(company.Clear(),"another",new(source.Ledger.Company,["ConfiguredCompanyRole"],host.Context.Permissions)));var privateDenied=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanySalesPaymentDraftAsync(command.Id));Assert.Equal(HttpStatusCode.Forbidden,privateDenied.StatusCode);Assert.Single((await client.GetCompanySalesPaymentSourceAsync(source.Ledger.ReceivableId,source.Ledger.BookId,source.Ledger.CurrencyCode)).Payments);Assert.Equal(1,native.Writes);
    }
}
#endif
