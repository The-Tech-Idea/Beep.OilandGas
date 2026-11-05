using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed record CompanySalesJournalPreparation(string IntentId,string JournalId,CompanySalesJournalSourceReview Source,string Number,string Reason,IReadOnlyList<string> LineIds)
{
    public void Require()
    {
        ArgumentNullException.ThrowIfNull(Source);Source.Require(Source.Company,Source.ActorUserId,Source.ReceivableId,Source.BookId,Source.CurrencyCode,Source.CalendarId,Source.PostingDate,Source.Selection);
        BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Intent(JournalId);BillingPolicyValues.Text(Number,128);BillingPolicyValues.Text(Reason,1000);
        if(LineIds is null || LineIds.Count!=Source.Lines.Count || LineIds.Distinct(StringComparer.Ordinal).Count()!=LineIds.Count || IntentId==JournalId || LineIds.Any(id=>id==IntentId || id==JournalId))throw new InvalidOperationException("Distinct original journal/intent/line identities and exact line cardinality are required.");
        foreach(var id in LineIds)BillingPolicyValues.Intent(id);if(JsonSerializer.Serialize(this).Length>CompanySalesJournalPreparationProof.MaximumLength-8192)throw new InvalidOperationException("The exact preparation request exceeds its native bound.");
    }
    public void RequireCurrent(CompanySalesJournalSourceReview current){Require();current.Require(Source.Company,Source.ActorUserId,Source.ReceivableId,Source.BookId,Source.CurrencyCode,Source.CalendarId,Source.PostingDate,Source.Selection);if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(Source))throw new InvalidOperationException("The exact approved financial posting source changed. Retain the original journal request; do not substitute another source.");}
    public JOURNAL_ENTRY CreateHeader()=>new(){JOURNAL_ENTRY_ID=JournalId,ENTRY_NUMBER=Number,ENTRY_DATE=Source.PostingDate,ENTRY_TYPE="SALES_RECEIVABLE",STATUS="REQUESTED",DESCRIPTION=Reason,REFERENCE_NUMBER=Source.ReceivableId,SOURCE_MODULE="COMPANY_SALES_AR",TOTAL_DEBIT=Source.FunctionalTotal,TOTAL_CREDIT=Source.FunctionalTotal,
        OWNER_TENANT_ID=Source.Company.TenantId,OWNER_COMPANY_BA_ID=Source.Company.BusinessAssociateId,FIELD_ID=Source.FieldId,BOOK_ID=Source.BookId,CURRENCY_CODE=Source.CurrencyCode,SOURCE_AR_INVOICE_ID=Source.ReceivableId,COMPANY_LEDGER_PERIOD_ID=Source.Periods.Periods.Single(row=>row.From<=Source.PostingDate && row.Through>=Source.PostingDate).PeriodId,
        ACTIVE_IND="Y",SOURCE=CompanySalesJournalPreparationProof.Source,REMARK=Reason,PPDM_GUID=Guid.ParseExact(IntentId,"N").ToString()};
    public JOURNAL_ENTRY_LINE CreateLine(int index)
    {
        var value=Source.Lines[index];return new(){JOURNAL_ENTRY_LINE_ID=LineIds[index],JOURNAL_ENTRY_ID=JournalId,LINE_NUMBER=value.Number,GL_ACCOUNT_ID=value.AccountId,DEBIT_AMOUNT=value.Debit,CREDIT_AMOUNT=value.Credit,DESCRIPTION=value.Purpose,CURRENCY_CODE=Source.CurrencyCode,
            OWNER_TENANT_ID=Source.Company.TenantId,OWNER_COMPANY_BA_ID=Source.Company.BusinessAssociateId,FIELD_ID=Source.FieldId,BOOK_ID=Source.BookId,SOURCE_AR_INVOICE_ID=Source.ReceivableId,COMPANY_LEDGER_PERIOD_ID=CreateHeader().COMPANY_LEDGER_PERIOD_ID,MAPPING_KEY=value.MappingKey,
            ACTIVE_IND="Y",SOURCE=CompanySalesJournalPreparationProof.Source,PPDM_GUID=Guid.ParseExact(LineIds[index],"N").ToString()};
    }
}
public sealed record CompanySalesJournalLineRevision(string LineId,string Revision);
public sealed record CompanySalesJournalPreparationProof(CompanySalesJournalPreparation Command,DateTime RetainedAtUtc,DateTime? RequestedChangedAtUtc,DateTime? DraftedAtUtc,IReadOnlyList<CompanySalesJournalLineRevision> Lines,string RowRevision)
{
    public const int MaximumLength=8388608;
    public const string Source="COMPANY_SALES_JOURNAL_PREPARATION";
    public static string Retain(CompanySalesJournalPreparation command,JOURNAL_ENTRY header)
    {
        command.Require();CompanyGLAccountReview.RequireAudit(header);var record=new CompanySalesJournalPreparationProof(CompanyInventorySourceReview.Copy(command),header.ROW_CREATED_DATE!.Value,header.ROW_CHANGED_DATE,null,[],Digest(header));record.RequireFacts(header);return Encode(record);
    }
    public static string Draft(CompanySalesJournalPreparationProof retained,JOURNAL_ENTRY header,IReadOnlyList<JOURNAL_ENTRY_LINE> lines)
    {
        var record=retained with{DraftedAtUtc=header.ROW_CHANGED_DATE,Lines=lines.Select(row=>new CompanySalesJournalLineRevision(row.JOURNAL_ENTRY_LINE_ID,Digest(row))).ToArray(),RowRevision=Digest(header)};
        record.RequireFacts(header);record.RequireLines(header,lines);return Encode(record);
    }
    private static string Encode(CompanySalesJournalPreparationProof value){var json=JsonSerializer.Serialize(value);if(json.Length>MaximumLength)throw new InvalidOperationException("The complete native preparation proof exceeds its bound.");return json;}
    public static CompanySalesJournalPreparationProof Require(JOURNAL_ENTRY header)
    {
        if(header.POSTING_WORKFLOW_JSON is not null)header=CompanySalesJournalPostingHeaderProof.OriginalReviewedHeader(header);
        if(header.REVIEW_WORKFLOW_JSON is not null)header=CompanySalesJournalReviewProof.OriginalDraftHeader(header);
        if(string.IsNullOrWhiteSpace(header.PREPARATION_WORKFLOW_JSON) || header.PREPARATION_WORKFLOW_JSON.Length>MaximumLength)throw new InvalidOperationException("The original native journal preparation proof is unavailable.");
        var record=JsonSerializer.Deserialize<CompanySalesJournalPreparationProof>(header.PREPARATION_WORKFLOW_JSON) ??throw new InvalidOperationException("The original journal proof is malformed.");record.RequireFacts(header);if(record.RowRevision!=Digest(header))throw new InvalidOperationException("The complete original journal header changed.");return record;
    }
    private void RequireFacts(JOURNAL_ENTRY header)
    {
        if(header.POSTING_WORKFLOW_JSON is not null)header=CompanySalesJournalPostingHeaderProof.OriginalReviewedHeader(header);
        if(header.REVIEW_WORKFLOW_JSON is not null)header=CompanySalesJournalReviewProof.OriginalDraftHeader(header);
        ArgumentNullException.ThrowIfNull(Command);Command.Require();CompanyGLAccountReview.RequireAudit(header);
        if(RetainedAtUtc==default || RetainedAtUtc.Kind!=DateTimeKind.Utc || RequestedChangedAtUtc is {} changed && (changed.Kind!=DateTimeKind.Utc || changed<RetainedAtUtc) || Lines is null ||
            DraftedAtUtc is null && (header.STATUS!="REQUESTED" || Lines.Count!=0) || DraftedAtUtc is {} drafted && (header.STATUS!="DRAFT" || drafted.Kind!=DateTimeKind.Utc || drafted<(RequestedChangedAtUtc??RetainedAtUtc) || Lines.Count!=Command.LineIds.Count))throw new InvalidOperationException("The original retained/drafted state and ordered UTC audit are unconfirmed.");
        var expected=Command.CreateHeader();expected.STATUS=DraftedAtUtc is null?"REQUESTED":"DRAFT";expected.ROW_CREATED_BY=Command.Source.ActorUserId;expected.ROW_CREATED_DATE=RetainedAtUtc;
        expected.ROW_CHANGED_DATE=DraftedAtUtc??RequestedChangedAtUtc;expected.ROW_CHANGED_BY=expected.ROW_CHANGED_DATE is null?null!:Command.Source.ActorUserId;
        var actual=CompanyInventorySourceReview.Copy(header);actual.PREPARATION_WORKFLOW_JSON=null;if(JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Only the exact original company/source journal and native audit are confirmed.");
        foreach(var value in Lines){BillingPolicyValues.Intent(value.LineId);BillingPolicyValues.Revision(value.Revision);}if(!Lines.Select(value=>value.LineId).SequenceEqual(DraftedAtUtc is null?[]:Command.LineIds))throw new InvalidOperationException("The exact original native journal line identities are unconfirmed.");
    }
    public void RequireLines(JOURNAL_ENTRY header,IReadOnlyList<JOURNAL_ENTRY_LINE> lines)
    {
        RequireFacts(header);if(lines is null || lines.Count!=Lines.Count || lines.Any(row=>row is null))throw new InvalidOperationException("Complete exact original journal lines are required.");
        for(var index=0;index<lines.Count;index++)
        {
            var row=lines[index];CompanyGLAccountReview.RequireAudit(row);var expected=Command.CreateLine(index);
            if(row.ROW_CREATED_BY!=Command.Source.ActorUserId || row.ROW_CREATED_DATE<RetainedAtUtc || row.ROW_CREATED_DATE>DraftedAtUtc || row.ROW_CHANGED_DATE is not null && row.ROW_CHANGED_BY!=Command.Source.ActorUserId || row.ROW_CHANGED_DATE>DraftedAtUtc)throw new InvalidOperationException("The exact journal line maker and ordered draft audit are unconfirmed.");
            expected.ROW_CREATED_BY=row.ROW_CREATED_BY;expected.ROW_CREATED_DATE=row.ROW_CREATED_DATE;expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
            if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(row) || Lines[index].LineId!=row.JOURNAL_ENTRY_LINE_ID || Lines[index].Revision!=Digest(row))throw new InvalidOperationException("The complete native journal line differs from its original source purpose and currency amounts.");
        }
    }
    public static string Digest(JOURNAL_ENTRY header){var copy=CompanyInventorySourceReview.Copy(header);copy.PREPARATION_WORKFLOW_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Digest(JOURNAL_ENTRY_LINE line)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(line));
    public static void RequireLineInsertion(CompanySalesJournalPreparation command,JOURNAL_ENTRY retained,JOURNAL_ENTRY_LINE row,int index)
    {
        var original=Require(retained);if(JsonSerializer.Serialize(original.Command)!=JsonSerializer.Serialize(command) || retained.STATUS!="REQUESTED")throw new InvalidOperationException("The exact original retained request is required before line insertion.");
        CompanyGLAccountReview.RequireAudit(row);var expected=command.CreateLine(index);expected.ROW_CREATED_BY=row.ROW_CREATED_BY;expected.ROW_CREATED_DATE=row.ROW_CREATED_DATE;expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
        if(row.ROW_CREATED_BY!=command.Source.ActorUserId || row.ROW_CREATED_DATE<original.RetainedAtUtc || row.ROW_CHANGED_DATE is not null && row.ROW_CHANGED_BY!=command.Source.ActorUserId || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Only the exact original source-purpose line and maker audit are authorized.");
    }
}
public sealed record CompanySalesJournalPreparationOutcome(JOURNAL_ENTRY Header,IReadOnlyList<JOURNAL_ENTRY_LINE> Lines)
{
    public void Require(CompanySalesJournalPreparation command){var proof=CompanySalesJournalPreparationProof.Require(Header);if(JsonSerializer.Serialize(proof.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The outcome belongs to another original preparation request.");proof.RequireLines(Header,Lines);}
}
