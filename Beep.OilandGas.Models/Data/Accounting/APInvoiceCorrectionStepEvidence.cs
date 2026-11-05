using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.Accounting;
/// <summary>Version-bound workflow evidence; acknowledgement is not financial approval.</summary>
public sealed record APInvoiceCorrectionStepEvidence(CompanyScope Company,string ActorUserId,string InvoiceId,string Action,
    int CorrectionVersion,string CorrectionIntentId,string CorrectionPreparerUserId,string SourceMakerUserId,string SourceRevision,
    string HistoryRevision,string FinancialRevision,string ProductionBindingRevision,string? CorrectionWorkIntentId,string Revision)
{
    public static APInvoiceCorrectionStepEvidence Create(APInvoiceFinancialReview current,string binding,string action,string? correctionWorkIntentId=null)
    {
        ArgumentNullException.ThrowIfNull(current);current.Require(current.Invoice.Company,current.Invoice.ActorUserId,current.Invoice.Invoice.AP_INVOICE_ID,current.Invoice.Revision);
        var history=APInvoiceCorrectionHistory.Require(current.Invoice.Company,current.Invoice.Invoice,current.Invoice.Lines);var latest=history.Versions[^1].Command;
        var proposal=APInvoiceVarianceProof.Require(current.Invoice.Company,history.OriginalInvoice);var request=APInvoiceVarianceDecisionProof.Require(current.Invoice.Company,history.OriginalInvoice);
        if(current.MatchingPolicies is null || current.ReceiptConsumption is null || current.Invoice.Invoice.STATUS!="DRAFT" || current.Invoice.Invoice.ACTIVE_IND!="Y" ||
            binding!=latest.BindingRevision || binding!=request.Command.BindingRevision || request.Command.Disposition!="REQUEST_CORRECTION")
            throw new InvalidOperationException("Complete current corrected source, policy, receipt and original production binding evidence are required.");
        if(action is CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement or CompanyAccountingStepEvidence.InvoiceCorrectionReturn)_=APInvoiceCorrectionDecisionReview.Create(current,binding);
        var evidence=new APInvoiceCorrectionStepEvidence(current.Invoice.Company,current.Invoice.ActorUserId,current.Invoice.Invoice.AP_INVOICE_ID,action,
            latest.Version,latest.IntentId,latest.ActorUserId,proposal.OriginalInvoice.ROW_CREATED_BY!,current.Invoice.Revision,
            ProcessDefinitionRevision.FromJson(current.Invoice.Invoice.CORRECTION_HISTORY_JSON!),current.Revision,binding,correctionWorkIntentId,"");
        evidence=evidence with{Revision=Digest(evidence)};evidence.Require(evidence.Company,evidence.ActorUserId,evidence.InvoiceId);return evidence;
    }
    public void Require(CompanyScope company,string actor,string invoiceId)
    {
        PurchaseOrderOwnership.RequireScope(company);
        var ids=new[]{ActorUserId,InvoiceId,CorrectionPreparerUserId,SourceMakerUserId};
        if(Company!=company || ActorUserId!=actor || InvoiceId!=invoiceId || ids.Any(id=>string.IsNullOrWhiteSpace(id) || id=="SYSTEM" || id.Length>128 || id.Any(char.IsControl)) ||
            CorrectionVersion is <1 or >APInvoiceCorrectionHistory.MaximumVersions || !Guid.TryParseExact(CorrectionIntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=CorrectionIntentId ||
            new[]{SourceRevision,HistoryRevision,FinancialRevision,ProductionBindingRevision,Revision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)) ||
            Action is not (CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork or CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement or CompanyAccountingStepEvidence.InvoiceCorrectionReturn) ||
            Action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork && (actor!=CorrectionPreparerUserId || CorrectionWorkIntentId is not null) ||
            Action is CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement or CompanyAccountingStepEvidence.InvoiceCorrectionReturn && (actor==CorrectionPreparerUserId || actor==SourceMakerUserId || !Guid.TryParseExact(CorrectionWorkIntentId,"N",out var work) || work==Guid.Empty || work.ToString("N")!=CorrectionWorkIntentId) || Revision!=Digest(this))
            throw new InvalidOperationException("Exact latest correction version, current financial binding and independent acknowledgement identity are required.");
    }
    private static string Digest(APInvoiceCorrectionStepEvidence evidence)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(evidence with{Revision=""}));
}
