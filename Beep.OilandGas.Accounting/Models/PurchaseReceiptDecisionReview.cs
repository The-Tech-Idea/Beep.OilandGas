using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Accounting.Models;

/// <summary>Original complete purchase source bound to current company production configuration.</summary>
public sealed record PurchaseReceiptDecisionReview(PurchaseOrderSourceReview Source,string ReceiptId,string BindingRevision)
{
    public void Require(CompanyScope company,string actor,string order,string receipt)
    {
        if(Source is null || ReceiptId!=receipt || string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl))
            throw new InvalidOperationException("The original bound receiver review is unavailable.");
        Source.Require(company,actor,order);
        if(Source.Receipts.Count(row=>row.PO_RECEIPT_ID==receipt)!=1)throw new InvalidOperationException("Review one explicit owned receipt.");
    }
    public void Require(PurchaseReceiptDecisionCommand command)
    {
        Require(command.Company,command.ActorUserId,command.OrderId,command.ReceiptId);command.Require(Source);
        if(command.BindingRevision!=BindingRevision)throw new InvalidOperationException("The original receiving binding changed.");
    }
}
public sealed record PurchaseReceiptDecisionSubmission(PurchaseReceiptDecisionCommand Command,PurchaseReceiptDecisionReview Review);
public sealed record PurchaseReceiptDecisionResult(PurchaseReceiptDecisionCommand Command,PO_RECEIPT Receipt,DateTimeOffset ConfirmedAtUtc)
{
    public static PurchaseReceiptDecisionResult Create(PurchaseReceiptDecisionSubmission original,PO_RECEIPT row)
    {
        var result=new PurchaseReceiptDecisionResult(original.Command,PurchaseReceiptDecisionProof.Copy(row),DateTimeOffset.UtcNow);result.Require(original);return result;
    }
    public void Require(PurchaseReceiptDecisionSubmission original)
    {
        original.Review.Require(original.Command);
        if(Command!=original.Command || Receipt is null || ConfirmedAtUtc==default || ConfirmedAtUtc.Offset!=TimeSpan.Zero ||
            PurchaseReceiptDecisionProof.Require(Command.Company,Receipt).Command!=Command ||
            System.Text.Json.JsonSerializer.Serialize(PurchaseReceiptDecisionProof.Require(Command.Company,Receipt).OriginalReceipt)!=
            System.Text.Json.JsonSerializer.Serialize(original.Review.Source.Receipts.Single(row=>row.PO_RECEIPT_ID==Command.ReceiptId)))
            throw new InvalidOperationException("The receiver result does not match the original reviewed intent and receipt.");
    }
}
