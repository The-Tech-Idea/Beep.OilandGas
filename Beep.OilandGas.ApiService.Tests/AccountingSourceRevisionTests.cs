using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class AccountingSourceRevisionTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public void SameOwnedContentHasOneRevisionAcrossReadersWhileAccountValidationRemainsExplicit(bool purchase)
    {
        var fixture=APInvoiceFinancialReviewTests.Review();var company=fixture.Invoice.Company;
        if(purchase)
        {
            var source=fixture.PurchaseLines[0].Source.Order;
            var first=PurchaseOrderSourceReview.Create(company,"receiver",source.Header,source.Lines,source.Receipts);
            var second=PurchaseOrderSourceReview.Create(company,"accountant",source.Header,source.Lines,source.Receipts);
            Assert.Equal(first.Revision,second.Revision);first.Require(company,"receiver","order");second.Require(company,"accountant","order");
            Assert.Throws<InvalidOperationException>(()=>first.Require(company,"accountant","order"));
            var header=PurchaseReceiptDecisionProof.Copy(source.Header);header.DESCRIPTION="Changed physical source";
            Assert.NotEqual(first.Revision,PurchaseOrderSourceReview.Create(company,"accountant",header,source.Lines,source.Receipts).Revision);
        }
        else
        {
            var source=fixture.Invoice;
            var first=APInvoiceSourceReview.Create(company,"preparer",source.Invoice,source.Lines);
            var second=APInvoiceSourceReview.Create(company,"controller",source.Invoice,source.Lines);
            Assert.Equal(first.Revision,second.Revision);first.Require(company,"preparer","invoice");second.Require(company,"controller","invoice");
            Assert.Throws<InvalidOperationException>(()=>first.Require(company,"controller","invoice"));
            var header=PurchaseReceiptDecisionProof.Copy(source.Invoice);header.INVOICE_NUMBER="Changed financial source";
            Assert.NotEqual(first.Revision,APInvoiceSourceReview.Create(company,"controller",header,source.Lines).Revision);
        }
    }
}
