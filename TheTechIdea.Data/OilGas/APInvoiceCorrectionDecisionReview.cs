using System.Text.Json;
namespace TheTechIdea.Data.OilGas;
/// <summary>Independent current diagnostics of the latest recorded source correction; no recorded acknowledgement or financial approval.</summary>
public sealed record APInvoiceCorrectionDecisionReview(APInvoiceFinancialReview Current,APInvoiceCorrectionHistory RecordedHistory,
    string BindingRevision,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public static APInvoiceCorrectionDecisionReview Create(APInvoiceFinancialReview current,string binding)
    {
        ArgumentNullException.ThrowIfNull(current);current=PurchaseReceiptDecisionProof.Copy(current);
        var history=APInvoiceCorrectionHistory.Require(current.Invoice.Company,current.Invoice.Invoice,current.Invoice.Lines);
        var result=new APInvoiceCorrectionDecisionReview(current,history,binding,Digest(current,history,binding),DateTimeOffset.UtcNow);
        result.Require(current.Invoice.Company,current.Invoice.ActorUserId,current.Invoice.Invoice.AP_INVOICE_ID,current.Invoice.Revision);return result;
    }
    public void Require(CompanyScope company,string actor,string id,string expectedInvoiceRevision)
    {
        if(Current?.Invoice is null || RecordedHistory?.Versions is null || string.IsNullOrWhiteSpace(actor) || actor=="SYSTEM" || actor.Length>128 || actor.Any(char.IsControl) ||
            !ProcessDefinitionRevision.IsValid(BindingRevision) || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("Complete addressed current correction rereview and production binding are required.");
        Current.Require(company,actor,id,expectedInvoiceRevision);
        var history=APInvoiceCorrectionHistory.Require(company,Current.Invoice.Invoice,Current.Invoice.Lines);var latest=history.Versions[^1];
        var proposal=APInvoiceVarianceProof.Require(company,history.OriginalInvoice);var request=APInvoiceVarianceDecisionProof.Require(company,history.OriginalInvoice);
        if(actor==latest.Command.ActorUserId || actor==proposal.Command.ActorUserId || actor==proposal.OriginalInvoice.ROW_CREATED_BY)
            throw new UnauthorizedAccessException("An independent source-maker and correction-preparer reviewer is required.");
        if(BindingRevision!=latest.Command.BindingRevision || BindingRevision!=request.Command.BindingRevision || request.Command.Disposition!="REQUEST_CORRECTION" ||
            Current.Invoice.Invoice.STATUS!="DRAFT" || Current.Invoice.Invoice.ACTIVE_IND!="Y" || Current.MatchingPolicies is null || Current.ReceiptConsumption is null ||
            JsonSerializer.Serialize(history)!=JsonSerializer.Serialize(RecordedHistory) || Revision!=Digest(Current,RecordedHistory,BindingRevision))
            throw new InvalidOperationException("The independent correction review differs from its latest complete recorded source, request, contributors or binding.");
    }
    public int CorrectionVersion=>RecordedHistory.Versions[^1].Command.Version;
    public string CorrectionIntentId=>RecordedHistory.Versions[^1].Command.IntentId;
    public string HistoryRevision=>ProcessDefinitionRevision.FromJson(Current.Invoice.Invoice.CORRECTION_HISTORY_JSON!);
    private static string Digest(APInvoiceFinancialReview current,APInvoiceCorrectionHistory history,string binding)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{
        current.Invoice.Company,current.Invoice.ActorUserId,current.Invoice.Invoice.AP_INVOICE_ID,current.Revision,history,binding}));
}
