using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.Accounting;
public enum SalesContractTermApplicability
{
    PreparationRequired,IndependentApprovalRequired,Rejected,DateInterpretationRequired,OutsideEffectiveDates,CurrencyRequired,CurrencyMismatch,ApprovedStoredDateAndCurrency
}
/// <summary>Applicability to one recorded revenue date/currency; delivery and financial execution require their own evidence.</summary>
public sealed record SalesContractTermApplicabilityReview(SalesContractTermApplicability Status,DateTime StoredTransactionDate,string? RecordedCurrency,string SourceRevision,string? TermsRevision,string Revision)
{
    public static SalesContractTermApplicabilityReview Create(SalesContractTermsReadReview original)
    {
        ArgumentNullException.ThrowIfNull(original);var source=original.Source.Source;
        original.Require(source.Company,source.ActorUserId,source.FieldId,source.From,source.Through,original.Source.TransactionId);
        var row=source.Rows.Single(row=>row.REVENUE_TRANSACTION_ID==original.Source.TransactionId);
        var date=row.TRANSACTION_DATE ??throw new InvalidOperationException("The original recorded transaction date is required.");
        var record=original.Recorded;var terms=record?.Preparation.Terms;
        var status=record is null?SalesContractTermApplicability.PreparationRequired:record.Decision is null?SalesContractTermApplicability.IndependentApprovalRequired:
            record.Decision.Decision==SalesContractTermsDecision.Reject?SalesContractTermApplicability.Rejected:
            date.Kind!=DateTimeKind.Unspecified?SalesContractTermApplicability.DateInterpretationRequired:
            date.Date<terms!.EffectiveFrom || terms.EffectiveThrough is {} through && date.Date>through?SalesContractTermApplicability.OutsideEffectiveDates:
            string.IsNullOrWhiteSpace(row.CURRENCY_CODE)?SalesContractTermApplicability.CurrencyRequired:
            row.CURRENCY_CODE!=terms.CurrencyCode?SalesContractTermApplicability.CurrencyMismatch:SalesContractTermApplicability.ApprovedStoredDateAndCurrency;
        var result=new SalesContractTermApplicabilityReview(status,date,row.CURRENCY_CODE,original.Source.Revision,record?.Revision,string.Empty);
        return result with{Revision=Digest(result)};
    }
    public void Require(SalesContractTermsReadReview original)
    {
        if(JsonSerializer.Serialize(this)!=JsonSerializer.Serialize(Create(original)))throw new InvalidOperationException("The approved term applicability differs from its original recorded source.");
    }
    private static string Digest(SalesContractTermApplicabilityReview result)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{result.Status,result.StoredTransactionDate,result.RecordedCurrency,result.SourceRevision,result.TermsRevision}));
}
