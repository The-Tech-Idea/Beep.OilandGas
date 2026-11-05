using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Header transition receipt. Native posting confirmation additionally requires its actual intent, GL rows, AR correlation and outcome.</summary>
public sealed record CompanySalesJournalPostingHeaderProof(JOURNAL_ENTRY Reviewed,string IntentId,string ActorUserId,DateTime PostedAtUtc,string RowRevision)
{
    public const int MaximumLength=67108864;
    public static string Create(SALES_JOURNAL_POSTING_INTENT intent,JOURNAL_ENTRY header)
    {
        var command=intent.Restore();var value=new CompanySalesJournalPostingHeaderProof(CompanyInventorySourceReview.Copy(command.Source.Reviewed.Header),command.IntentId,command.Source.ActorUserId,header.ROW_CHANGED_DATE??throw new InvalidOperationException("Native posting UTC audit is required."),Digest(header));value.RequireFacts(header);var json=JsonSerializer.Serialize(value);if(json.Length>MaximumLength)throw new InvalidOperationException("The complete native posting header proof exceeds its bound.");return json;
    }
    public static CompanySalesJournalPostingHeaderProof Require(JOURNAL_ENTRY header)
    {
        if(string.IsNullOrWhiteSpace(header.POSTING_WORKFLOW_JSON) || header.POSTING_WORKFLOW_JSON.Length>MaximumLength)throw new InvalidOperationException("The exact native posting header transition is unavailable.");var value=JsonSerializer.Deserialize<CompanySalesJournalPostingHeaderProof>(header.POSTING_WORKFLOW_JSON)??throw new InvalidOperationException("The native posting header proof is malformed.");value.RequireFacts(header);if(value.RowRevision!=Digest(header))throw new InvalidOperationException("The complete native posted header changed.");return value;
    }
    private void RequireFacts(JOURNAL_ENTRY header)
    {
        ArgumentNullException.ThrowIfNull(Reviewed);BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Text(ActorUserId,128);CompanyGLAccountReview.RequireAudit(header);
        if(Reviewed.POSTING_WORKFLOW_JSON is not null || Reviewed.SALES_JOURNAL_POSTING_INTENT_ID is not null || Reviewed.STATUS!="APPROVED" || CompanySalesJournalReviewProof.Require(Reviewed).Intent.Restore().Decision!="APPROVE" || header.STATUS!="POSTED" || header.SALES_JOURNAL_POSTING_INTENT_ID!=IntentId || header.ROW_CHANGED_BY!=ActorUserId || header.ROW_CHANGED_DATE!=PostedAtUtc || PostedAtUtc==default || PostedAtUtc.Kind!=DateTimeKind.Utc || PostedAtUtc<Reviewed.ROW_CHANGED_DATE)throw new InvalidOperationException("Original independent approval and ordered native posting identity/audit are required.");
        var normalized=CompanyInventorySourceReview.Copy(header);normalized.POSTING_WORKFLOW_JSON=null;normalized.SALES_JOURNAL_POSTING_INTENT_ID=null;normalized.STATUS=Reviewed.STATUS;normalized.ROW_CHANGED_BY=Reviewed.ROW_CHANGED_BY;normalized.ROW_CHANGED_DATE=Reviewed.ROW_CHANGED_DATE;if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(Reviewed))throw new InvalidOperationException("Posting changed the frozen reviewed journal facts.");
    }
    public static JOURNAL_ENTRY OriginalReviewedHeader(JOURNAL_ENTRY header)=>CompanyInventorySourceReview.Copy(Require(header).Reviewed);
    private static string Digest(JOURNAL_ENTRY header){var value=CompanyInventorySourceReview.Copy(header);value.POSTING_WORKFLOW_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value));}
}

public sealed record CompanySalesJournalPostingOutcome(SALES_JOURNAL_POSTING_INTENT Intent,CompanySalesJournalPreparationOutcome Journal,IReadOnlyList<GL_ENTRY> Entries,AR_INVOICE Receivable)
{
    public static void RequireEntryInsertion(CompanySalesJournalPostingCommand command,SALES_JOURNAL_POSTING_INTENT retained,GL_ENTRY row,int index)
    {
        command.Require();if(JsonSerializer.Serialize(retained.Restore())!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The exact retained original posting request is required before a GL insertion.");CompanyGLAccountReview.RequireAudit(row);var expected=command.CreateEntry(index);
        if(row.ROW_CREATED_BY!=command.Source.ActorUserId || row.ROW_CREATED_DATE<retained.ROW_CREATED_DATE || row.ROW_CHANGED_DATE is not null && row.ROW_CHANGED_BY!=command.Source.ActorUserId)throw new InvalidOperationException("The original GL insertion actor/audit is unconfirmed.");expected.ROW_CREATED_BY=row.ROW_CREATED_BY;expected.ROW_CREATED_DATE=row.ROW_CREATED_DATE;expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Only the exact original reviewed GL entry can be inserted.");
    }
    public void Require(CompanySalesJournalPostingCommand command)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("journal-posting-outcome",this,command);if(CompanySalesInvoiceValidation.Contains(key))return;
        command.Require();ArgumentNullException.ThrowIfNull(Intent);ArgumentNullException.ThrowIfNull(Journal);ArgumentNullException.ThrowIfNull(Receivable);if(JsonSerializer.Serialize(Intent.Restore())!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The posted outcome belongs to another original posting request.");
        var proof=CompanySalesJournalPostingHeaderProof.Require(Journal.Header);Journal.Require(CompanySalesJournalPreparationProof.Require(command.Source.Reviewed.Header).Command);
        if(proof.IntentId!=command.IntentId || proof.ActorUserId!=command.Source.ActorUserId || proof.PostedAtUtc<Intent.ROW_CREATED_DATE || JsonSerializer.Serialize(proof.Reviewed)!=JsonSerializer.Serialize(command.Source.Reviewed.Header) || JsonSerializer.Serialize(Journal.Lines)!=JsonSerializer.Serialize(command.Source.Reviewed.Lines) || JsonSerializer.Serialize(Receivable)!=JsonSerializer.Serialize(command.Source.Receivable) || Entries is null || Entries.Count!=command.EntryIds.Count)throw new InvalidOperationException("The complete posted journal/GL/AR correlation differs from its frozen original request.");
        for(var index=0;index<Entries.Count;index++)
        {
            var row=Entries[index];ArgumentNullException.ThrowIfNull(row);CompanyGLAccountReview.RequireAudit(row);var expected=command.CreateEntry(index);
            if(row.ROW_CREATED_BY!=command.Source.ActorUserId || row.ROW_CREATED_DATE<Intent.ROW_CREATED_DATE || row.ROW_CREATED_DATE>proof.PostedAtUtc || row.ROW_CHANGED_DATE>proof.PostedAtUtc || row.ROW_CHANGED_DATE is not null && row.ROW_CHANGED_BY!=command.Source.ActorUserId)throw new InvalidOperationException("The native GL insertion audit does not belong to the original posting.");
            expected.ROW_CREATED_BY=row.ROW_CREATED_BY;expected.ROW_CREATED_DATE=row.ROW_CREATED_DATE;expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("A native GL entry differs from its exact original reviewed line.");
        }
        CompanySalesInvoiceValidation.Remember(key);
    }
}
public sealed record CompanySalesJournalPostingRecovery(CompanyScope Company,string ActorUserId,string JournalId,SALES_JOURNAL_POSTING_INTENT? Intent,CompanySalesJournalPostingOutcome? Outcome)
{
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Text(ActorUserId,128);BillingPolicyValues.Intent(JournalId);
        if(Intent is null){if(Outcome is not null)throw new InvalidOperationException("A native posting outcome requires its original retained request.");return;}
        var command=Intent.Restore();if(command.Source.Current.Company!=Company || command.Source.ActorUserId!=ActorUserId || command.Source.Reviewed.Header.JOURNAL_ENTRY_ID!=JournalId)throw new InvalidOperationException("Only the original posting producer can recover their retained request in its company.");Outcome?.Require(command);
    }
}
