using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

public sealed record APInvoiceCorrectionCommand(CompanyScope Company,string ActorUserId,string InvoiceId,APInvoiceCorrectionDraft Draft,
    string ExpectedReviewRevision,string BindingRevision,int Version,string? PreviousHistoryRevision,string IntentId)
{
    public void RequireShape()
    {
        PurchaseOrderOwnership.RequireScope(Company);
        static bool Id(string? value)=>!string.IsNullOrWhiteSpace(value) && value.Length<=128 && !value.Any(char.IsControl);
        if(!Id(ActorUserId) || ActorUserId=="SYSTEM" || !Id(InvoiceId) || Draft is null ||
            !ProcessDefinitionRevision.IsValid(ExpectedReviewRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision) ||
            Version is <1 or >APInvoiceCorrectionHistory.MaximumVersions ||
            (Version==1?PreviousHistoryRevision is not null:!ProcessDefinitionRevision.IsValid(PreviousHistoryRevision)) ||
            !Guid.TryParseExact(IntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=IntentId)
            throw new InvalidOperationException("Exact accountant correction account, source, review, version, predecessor, binding and original intent are required.");
        Draft.RequireShape();
    }
    public void Require(APInvoiceCorrectionReview review)
    {
        RequireShape();ArgumentNullException.ThrowIfNull(review);review.Require(Company,ActorUserId,InvoiceId,Draft);
        var history=review.Current.Invoice.Invoice.CORRECTION_HISTORY_JSON;
        var count=history is null?0:APInvoiceCorrectionHistory.Require(Company,review.Current.Invoice.Invoice,review.Current.Invoice.Lines).Versions.Count;
        if(review.Revision!=ExpectedReviewRevision || review.BindingRevision!=BindingRevision || Version!=count+1 ||
            PreviousHistoryRevision!=(history is null?null:ProcessDefinitionRevision.FromJson(history)))
            throw new InvalidOperationException("The correction command differs from its exact current review and recorded history predecessor.");
    }
    public bool Same(APInvoiceCorrectionCommand other)=>JsonSerializer.Serialize(this)==JsonSerializer.Serialize(other);
}
public sealed record APInvoiceCorrectionVersion(APInvoiceCorrectionCommand Command,DateTime ChangedAtUtc,string RecordedRevision);
/// <summary>Bounded linear correction history. Original controller/proposal proofs stay byte-for-byte recorded; no financial approval.</summary>
public sealed record APInvoiceCorrectionHistory(AP_INVOICE OriginalInvoice,IReadOnlyList<AP_INVOICE_LINE> OriginalLines,IReadOnlyList<APInvoiceCorrectionVersion> Versions)
{
    public const int MaximumLength=262144,MaximumVersions=8;
    private static readonly JsonSerializerOptions Options=new(){UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};
    private static string Json<T>(T value)=>JsonSerializer.Serialize(value);
    private static AP_INVOICE Header(AP_INVOICE row){var copy=PurchaseReceiptDecisionProof.Copy(row);copy.CORRECTION_HISTORY_JSON=null;return copy;}
    private static string Digest(AP_INVOICE row,IReadOnlyList<AP_INVOICE_LINE> lines)=>ProcessDefinitionRevision.FromJson(Json(new{Invoice=Header(row),Lines=lines.OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal)}));
    private static string VersionDigest(APInvoiceCorrectionCommand command,DateTime changed,AP_INVOICE row,IReadOnlyList<AP_INVOICE_LINE> lines)=>ProcessDefinitionRevision.FromJson(Json(new{command,ChangedAtUtc=changed,Source=Digest(row,lines)}));
    private static string Encode(APInvoiceCorrectionHistory history)
    {var json=Json(history);if(json.Length>MaximumLength)throw new InvalidOperationException("Correction history exceeds the bounded Beep source contract.");return json;}
    private static void Apply(APInvoiceCorrectionCommand command,AP_INVOICE row,IReadOnlyList<AP_INVOICE_LINE> lines,DateTime changed,AP_INVOICE root)
    {
        command.RequireShape();APInvoiceOwnership.Require(command.Company,command.InvoiceId,row);APInvoiceOwnership.RequireLines(command.InvoiceId,lines);
        var request=APInvoiceVarianceDecisionProof.Require(command.Company,root);var proposal=APInvoiceVarianceProof.Require(command.Company,root);
        if(changed==default || changed.Kind!=DateTimeKind.Utc || row.ROW_CHANGED_DATE is {} previousAudit && changed<previousAudit || row.STATUS!="DRAFT" || row.ACTIVE_IND!="Y" || request.Command.Disposition!="REQUEST_CORRECTION" ||
            proposal.Command.ActorUserId!=command.ActorUserId || request.Command.BindingRevision!=command.BindingRevision || row.VARIANCE_PROPOSAL_JSON!=root.VARIANCE_PROPOSAL_JSON || row.VARIANCE_DECISION_JSON!=root.VARIANCE_DECISION_JSON ||
            request.Command.IntentId!=command.Draft.DecisionIntentId || APInvoiceCorrectionReview.DecisionRevision(row.VARIANCE_DECISION_JSON!)!=command.Draft.ExpectedDecisionRevision ||
            !lines.Select(line=>line.AP_INVOICE_LINE_ID).Order(StringComparer.Ordinal).SequenceEqual(command.Draft.Lines.Select(line=>line.LineId).Order(StringComparer.Ordinal)) || lines.Any(line=>line.ACTIVE_IND!="Y"))
            throw new InvalidOperationException("Correction history requires the original preparer, exact controller request, draft source, lines and UTC audit.");
        if(row.TOTAL_AMOUNT==command.Draft.TotalAmount && lines.All(line=>{var edit=command.Draft.Lines.Single(value=>value.LineId==line.AP_INVOICE_LINE_ID);return line.QUANTITY==edit.Quantity && line.UNIT_PRICE==edit.UnitPrice && line.LINE_TOTAL==edit.LineTotal && line.ITEM_DESCRIPTION==edit.Description;}))
            throw new InvalidOperationException("An unchanged invoice cannot add a correction history version.");
        row.TOTAL_AMOUNT=command.Draft.TotalAmount;row.ROW_CHANGED_BY=command.ActorUserId;row.ROW_CHANGED_DATE=changed;
        foreach(var line in lines)
        {
            var edit=command.Draft.Lines.Single(value=>value.LineId==line.AP_INVOICE_LINE_ID);
            line.QUANTITY=edit.Quantity;line.UNIT_PRICE=edit.UnitPrice;line.LINE_TOTAL=edit.LineTotal;line.ITEM_DESCRIPTION=edit.Description;
            line.ROW_CHANGED_BY=command.ActorUserId;line.ROW_CHANGED_DATE=changed;
        }
    }
    public static string Create(APInvoiceCorrectionCommand command,APInvoiceCorrectionReview reviewed,AP_INVOICE recorded,IReadOnlyList<AP_INVOICE_LINE> recordedLines)
    {
        command=PurchaseReceiptDecisionProof.Copy(command);reviewed=PurchaseReceiptDecisionProof.Copy(reviewed);command.Require(reviewed);
        var before=reviewed.Current.Invoice;var row=PurchaseReceiptDecisionProof.Copy(before.Invoice);var lines=PurchaseReceiptDecisionProof.Copy(before.Lines.ToArray());
        var changed=recorded.ROW_CHANGED_DATE??throw new InvalidOperationException("Recorded correction audit is required.");
        // Validate the existing prefix before removing it for deterministic reconstruction.
        var prior=row.CORRECTION_HISTORY_JSON is null?null:Require(command.Company,row,lines);
        row.CORRECTION_HISTORY_JSON=null;Apply(command,row,lines,changed,prior?.OriginalInvoice??before.Invoice);
        if(Json(Header(recorded))!=Json(row) || Json(recordedLines.OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal))!=Json(lines.OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("Recorded correction changes fields outside the exact reviewed header, lines and audit.");
        var version=new APInvoiceCorrectionVersion(command,changed,VersionDigest(command,changed,row,lines));
        return Encode(new(prior?.OriginalInvoice??PurchaseReceiptDecisionProof.Copy(before.Invoice),prior?.OriginalLines??PurchaseReceiptDecisionProof.Copy(before.Lines.ToArray()),
            (prior?.Versions??Array.Empty<APInvoiceCorrectionVersion>()).Append(version).ToArray()));
    }
    public static APInvoiceCorrectionHistory Require(CompanyScope company,AP_INVOICE row,IReadOnlyList<AP_INVOICE_LINE>? actualLines=null)
    {
        APInvoiceOwnership.Require(company,row?.AP_INVOICE_ID??"",row);
        var json=row!.CORRECTION_HISTORY_JSON;
        if(json is not {Length:>0} || json.Length>MaximumLength)throw new InvalidOperationException("Recorded correction history is unavailable.");
        APInvoiceCorrectionHistory history;
        try{history=JsonSerializer.Deserialize<APInvoiceCorrectionHistory>(json,Options)??throw new JsonException();}catch(JsonException){throw new InvalidOperationException("Recorded correction history is incompatible.");}
        if(history.OriginalInvoice is null || history.OriginalLines is null || history.OriginalLines.Count is <1 or >128 || history.OriginalLines.Any(line=>line is null) || history.Versions is null || history.Versions.Count is <1 or >MaximumVersions ||
            history.OriginalInvoice.CORRECTION_HISTORY_JSON is not null || history.Versions.Any(version=>version?.Command is null) ||
            history.Versions.Select(version=>version.Command.IntentId).Distinct(StringComparer.Ordinal).Count()!=history.Versions.Count)
            throw new InvalidOperationException("Complete bounded correction history with unique intents and an uncorrected root is required.");
        var expected=PurchaseReceiptDecisionProof.Copy(history.OriginalInvoice);var lines=PurchaseReceiptDecisionProof.Copy(history.OriginalLines.OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal).ToArray());
        APInvoiceOwnership.Require(company,row.AP_INVOICE_ID,expected);APInvoiceOwnership.RequireLines(row.AP_INVOICE_ID,lines);
        var prefix=new List<APInvoiceCorrectionVersion>();
        foreach(var version in history.Versions)
        {
            var command=version.Command;command.RequireShape();
            if(command.Company!=company || command.InvoiceId!=row.AP_INVOICE_ID || command.Version!=prefix.Count+1 ||
                command.PreviousHistoryRevision!=(expected.CORRECTION_HISTORY_JSON is null?null:ProcessDefinitionRevision.FromJson(expected.CORRECTION_HISTORY_JSON)) ||
                command.Draft.ExpectedInvoiceRevision!=APInvoiceSourceReview.CorrectionSourceRevision(company,expected,lines) ||
                prefix.Count>0 && version.ChangedAtUtc<prefix[^1].ChangedAtUtc)
                throw new InvalidOperationException("Correction history version, predecessor, source or audit order changed.");
            // Prefix was validated on the preceding iteration; proofs address the preserved original request.
            expected.CORRECTION_HISTORY_JSON=null;Apply(command,expected,lines,version.ChangedAtUtc,history.OriginalInvoice);
            if(version.RecordedRevision!=VersionDigest(command,version.ChangedAtUtc,expected,lines))throw new InvalidOperationException("Recorded correction values differ from their exact version command.");
            prefix.Add(version);expected.CORRECTION_HISTORY_JSON=Encode(new(history.OriginalInvoice,history.OriginalLines,prefix.ToArray()));
        }
        if(Encode(history)!=json || Json(expected)!=Json(row) || actualLines is not null && Json(lines)!=Json(actualLines.OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("The current corrected invoice or complete line set differs from its recorded history.");
        return PurchaseReceiptDecisionProof.Copy(history);
    }
    internal APInvoiceSourceReview SourceBeforeLast()
    {
        var row=PurchaseReceiptDecisionProof.Copy(OriginalInvoice);var lines=PurchaseReceiptDecisionProof.Copy(OriginalLines.OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal).ToArray());
        var prefix=new List<APInvoiceCorrectionVersion>();
        foreach(var version in Versions.Take(Versions.Count-1))
        {
            row.CORRECTION_HISTORY_JSON=null;Apply(version.Command,row,lines,version.ChangedAtUtc,OriginalInvoice);prefix.Add(version);
            row.CORRECTION_HISTORY_JSON=Encode(new(OriginalInvoice,OriginalLines,prefix.ToArray()));
        }
        var last=Versions[^1].Command;return APInvoiceSourceReview.Create(last.Company,last.ActorUserId,row,lines);
    }
}
public sealed record APInvoiceCorrectionSubmission(APInvoiceCorrectionCommand Command,APInvoiceCorrectionReview Review);
public sealed record APInvoiceCorrectionResult(APInvoiceCorrectionCommand Command,AP_INVOICE Invoice,IReadOnlyList<AP_INVOICE_LINE> Lines)
{
    public void Require(APInvoiceCorrectionSubmission submission)
    {
        submission.Command.Require(submission.Review);APInvoiceOwnership.RequireLines(submission.Command.InvoiceId,Lines);
        var history=APInvoiceCorrectionHistory.Require(submission.Command.Company,Invoice,Lines);
        if(Command is null || !Command.Same(submission.Command) || !history.Versions[^1].Command.Same(Command))
            throw new InvalidOperationException("Exact original correction outcome is unconfirmed.");
        var before=history.SourceBeforeLast();
        if(JsonSerializer.Serialize(before.Invoice)!=JsonSerializer.Serialize(submission.Review.Current.Invoice.Invoice) ||
            JsonSerializer.Serialize(before.Lines)!=JsonSerializer.Serialize(submission.Review.Current.Invoice.Lines.OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("Correction outcome does not preserve its exact reviewed original source and line set.");
    }
}
