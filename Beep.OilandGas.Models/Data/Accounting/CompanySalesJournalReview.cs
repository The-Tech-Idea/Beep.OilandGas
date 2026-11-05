using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed record CompanySalesJournalReviewSource(string ActorUserId,CompanySalesJournalPreparationOutcome Original,CompanySalesJournalSourceReview Current,string Revision)
{
    public static CompanySalesJournalReviewSource Create(string actor,CompanySalesJournalPreparationOutcome original,CompanySalesJournalSourceReview current)
    {
        var source=new CompanySalesJournalReviewSource(actor,original,current,string.Empty);source=source with{Revision=Digest(source)};source.Require();return CompanyInventorySourceReview.Copy(source);
    }
    public void Require()
    {
        BillingPolicyValues.Text(ActorUserId,128);ArgumentNullException.ThrowIfNull(Original);ArgumentNullException.ThrowIfNull(Current);var preparation=CompanySalesJournalPreparationProof.Require(Original.Header).Command;Original.Require(preparation);preparation.RequireCurrent(Current);
        if(Original.Header.STATUS!="DRAFT" || Original.Header.REVIEW_WORKFLOW_JSON is not null || ActorUserId==preparation.Source.ActorUserId || Revision!=Digest(this))throw new InvalidOperationException("An independent reviewer, original complete DRAFT and exact current maker-view source are required.");
    }
    public void RequireCurrent(CompanySalesJournalReviewSource current){Require();current.Require();if(JsonSerializer.Serialize(this)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("The complete displayed journal review changed before disposition.");}
    private static string Digest(CompanySalesJournalReviewSource source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(source with{Revision=string.Empty}));
}
public sealed record CompanySalesJournalReviewCommand(string IntentId,CompanySalesJournalReviewSource Review,string Decision,string FinancialDisposition,string FinancialEvidence,string SourceDisposition,string SourceEvidence,string Reason)
{
    public void Require()
    {
        ArgumentNullException.ThrowIfNull(Review);Review.Require();BillingPolicyValues.Intent(IntentId);var preparation=CompanySalesJournalPreparationProof.Require(Review.Original.Header).Command;
        if(IntentId==preparation.IntentId || IntentId==preparation.JournalId || preparation.LineIds.Contains(IntentId) || Decision is not ("APPROVE" or "REJECT"))throw new InvalidOperationException("A distinct original review identity and explicit approve/reject decision are required.");
        foreach(var value in new[]{FinancialDisposition,SourceDisposition})if(value is not ("CONFIRMED" or "ISSUE"))throw new InvalidOperationException("Explicit journal financial and source dispositions are required.");
        BillingPolicyValues.Text(FinancialEvidence,2000);BillingPolicyValues.Text(SourceEvidence,2000);BillingPolicyValues.Text(Reason,1000);if(Decision=="APPROVE" && (FinancialDisposition!="CONFIRMED" || SourceDisposition!="CONFIRMED"))throw new InvalidOperationException("Approval requires confirmed financial and source evidence.");
        if(JsonSerializer.Serialize(this).Length>SALES_JOURNAL_REVIEW_INTENT.MaximumCommandLength)throw new InvalidOperationException("The complete original review request exceeds its bound.");
    }
}
public sealed record CompanySalesJournalReviewProof(SALES_JOURNAL_REVIEW_INTENT Intent,DateTime ReviewedAtUtc,string RowRevision)
{
    public const int MaximumLength=33554432;
    public static string Create(SALES_JOURNAL_REVIEW_INTENT intent,JOURNAL_ENTRY row)
    {
        var record=new CompanySalesJournalReviewProof(CompanyInventorySourceReview.Copy(intent),row.ROW_CHANGED_DATE??throw new InvalidOperationException("Native review audit is required."),Digest(row));record.RequireFacts(row);var json=JsonSerializer.Serialize(record);if(json.Length>MaximumLength)throw new InvalidOperationException("The native journal review proof exceeds its bound.");return json;
    }
    public static CompanySalesJournalReviewProof Require(JOURNAL_ENTRY row)
    {
        if(row.POSTING_WORKFLOW_JSON is not null)row=CompanySalesJournalPostingHeaderProof.OriginalReviewedHeader(row);
        if(string.IsNullOrWhiteSpace(row.REVIEW_WORKFLOW_JSON) || row.REVIEW_WORKFLOW_JSON.Length>MaximumLength)throw new InvalidOperationException("The original native independent review proof is unavailable.");var proof=JsonSerializer.Deserialize<CompanySalesJournalReviewProof>(row.REVIEW_WORKFLOW_JSON) ??throw new InvalidOperationException("The native review proof is malformed.");proof.RequireFacts(row);if(proof.RowRevision!=Digest(row))throw new InvalidOperationException("The complete reviewed header changed.");return proof;
    }
    private void RequireFacts(JOURNAL_ENTRY row)
    {
        ArgumentNullException.ThrowIfNull(Intent);var command=Intent.Restore();var original=command.Review.Original.Header;CompanyGLAccountReview.RequireAudit(row);
        if(ReviewedAtUtc==default || ReviewedAtUtc.Kind!=DateTimeKind.Utc || ReviewedAtUtc<Intent.ROW_CREATED_DATE || ReviewedAtUtc<original.ROW_CHANGED_DATE || row.STATUS!=(command.Decision=="APPROVE"?"APPROVED":"REJECTED") || row.ROW_CHANGED_BY!=command.Review.ActorUserId || row.ROW_CHANGED_DATE!=ReviewedAtUtc)throw new InvalidOperationException("Independent reviewer, original decision and ordered native UTC audit are unconfirmed.");
        var normalized=CompanyInventorySourceReview.Copy(row);normalized.REVIEW_WORKFLOW_JSON=null;normalized.STATUS=original.STATUS;normalized.ROW_CHANGED_BY=original.ROW_CHANGED_BY;normalized.ROW_CHANGED_DATE=original.ROW_CHANGED_DATE;if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Reviewed journal financial, source, maker or preparation facts changed.");
    }
    public static JOURNAL_ENTRY OriginalDraftHeader(JOURNAL_ENTRY row)=>CompanyInventorySourceReview.Copy(Require(row).Intent.Restore().Review.Original.Header);
    private static string Digest(JOURNAL_ENTRY row){var copy=CompanyInventorySourceReview.Copy(row);copy.REVIEW_WORKFLOW_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
public sealed record CompanySalesJournalReviewRecovery(CompanyScope Company,string ActorUserId,string JournalId,SALES_JOURNAL_REVIEW_INTENT? Intent,CompanySalesJournalPreparationOutcome? Outcome)
{
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Text(ActorUserId,128);BillingPolicyValues.Intent(JournalId);
        if(Intent is null){if(Outcome is not null)throw new InvalidOperationException("A reviewed outcome requires its original retained reviewer request.");return;}
        var command=Intent.Restore();if(command.Review.ActorUserId!=ActorUserId || command.Review.Current.Company!=Company || command.Review.Original.Header.JOURNAL_ENTRY_ID!=JournalId)throw new InvalidOperationException("Only the original journal reviewer can recover their retained request in its company.");
        if(Outcome is not null){var maker=CompanySalesJournalPreparationProof.Require(command.Review.Original.Header).Command;Outcome.Require(maker);if(JsonSerializer.Serialize(CompanySalesJournalReviewProof.Require(Outcome.Header).Intent)!=JsonSerializer.Serialize(Intent))throw new InvalidOperationException("The complete reviewed native outcome differs from its original reviewer request.");}
    }
}
