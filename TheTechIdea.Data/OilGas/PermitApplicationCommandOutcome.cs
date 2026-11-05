using System.Reflection;
using Beep.OilandGas.PermitsAndApplications.Constants;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded native result tied to one immutable reviewed command; not legal regulator approval.</summary>
public sealed record PermitApplicationCommandOutcome(
    [property: JsonRequired] PermitApplicationCommand Original,
    [property: JsonRequired] DateTimeOffset RecordedAtUtc,
    [property: JsonRequired] string ApplicationJson,
    [property: JsonRequired] string HistoryJson)
{
    [JsonRequired] public PermitApplicationSupportingSnapshot Supporting { get; init; } = PermitApplicationSupportingSnapshot.Empty;
    public static PermitApplicationCommandOutcome Prepare(PermitApplicationCommand original, DateTimeOffset recorded)
    {
        original.Require();
        if (recorded == default || recorded.Offset != TimeSpan.Zero) throw new InvalidOperationException("A recorded UTC command time is required.");
        var row = original.Review.ReadApplication(); var actor = original.Review.Context.Field.ActorUserId;
        if(original.Action=="CREATE_DRAFT")
        {
            row.CREATED_DATE=recorded.UtcDateTime;row.ROW_CREATED_BY=actor;row.ROW_CREATED_DATE=recorded.UtcDateTime;
            row.PPDM_GUID=original.IntentId.ToString();row.ROW_EFFECTIVE_DATE=recorded.UtcDateTime;
        }
        else if(original.Action=="EDIT_DRAFT")original.DraftChanges!.Apply(row);
        else if(original.Action=="ATTACH_DOCUMENT") { /* Parent filing state remains unchanged. */ }
        else if(PermitSupportingAmendments.EntityType(original.Action) is not null) { /* Supporting amendments preserve parent filing state. */ }
        else row.STATUS = original.Action switch
        {
            "SUBMIT"=>PermitApplicationStatus.Submitted,
            "APPROVE"=>PermitApplicationStatus.Approved,
            "REJECT"=>PermitApplicationStatus.Rejected,
            "RETURN_FOR_CORRECTION"=>PermitApplicationStatus.AdditionalInformationRequired,
            _=>throw new InvalidOperationException("The reviewed lifecycle action is unsupported.")
        };
        if (original.Action == "SUBMIT")
        {
            row.SUBMITTED_BY_USER_ID = actor; row.SUBMITTED_DATE = recorded.UtcDateTime;
            row.SUBMISSION_COMPLETE = true; row.SUBMISSION_COMPLETE_IND = "Y"; row.DECISION = null; row.DECISION_DATE = null;
        }
        else if(original.Action is "APPROVE" or "REJECT" or "RETURN_FOR_CORRECTION") { row.DECISION = original.Action; row.DECISION_DATE = recorded.UtcDateTime; }
        if(original.Action=="RETURN_FOR_CORRECTION"){row.SUBMISSION_COMPLETE=false;row.SUBMISSION_COMPLETE_IND="N";}
        row.ROW_CHANGED_BY = actor; row.ROW_CHANGED_DATE = recorded.UtcDateTime;
        var history = new PERMIT_STATUS_HISTORY
        {
            PERMIT_STATUS_HISTORY_ID = "PC-" + original.IntentId.ToString("N"), PERMIT_APPLICATION_ID = row.PERMIT_APPLICATION_ID,
            STATUS = PermitApplicationStatusCodes.ToStorageKey(row.STATUS), STATUS_DATE = recorded.UtcDateTime, STATUS_REMARKS = original.Reason, UPDATED_BY = actor,
            ACTIVE_IND = "Y", PPDM_GUID = original.IntentId.ToString(), ROW_EFFECTIVE_DATE = recorded.UtcDateTime,
            ROW_CREATED_BY = actor, ROW_CHANGED_BY = actor, ROW_CREATED_DATE = recorded.UtcDateTime, ROW_CHANGED_DATE = recorded.UtcDateTime
        };
        var supporting=original.Review.Supporting;
        if(PermitSupportingAmendments.EntityType(original.Action) is not null)
            supporting=PermitSupportingAmendments.Prepare(original,recorded);
        if(original.Action=="ATTACH_DOCUMENT")
        {
            var attachment=original.Attachment!.Build(original,recorded);
            supporting=PermitApplicationSupportingSnapshot.Capture(row,supporting.Drilling,supporting.Environmental,supporting.Injection,
                supporting.Attachments.Append(attachment).ToArray(),supporting.Documents.Append(new(attachment.APPLICATION_ATTACHMENT_ID,"VERIFIED",original.Attachment.Document)).ToArray());
        }
        return new(original, recorded, JsonSerializer.Serialize(row), JsonSerializer.Serialize(history)) { Supporting=supporting };
    }
    public PERMIT_APPLICATION ReadApplication() { Require(); return JsonSerializer.Deserialize<PERMIT_APPLICATION>(ApplicationJson)!; }
    public PERMIT_STATUS_HISTORY ReadHistory() { Require(); return JsonSerializer.Deserialize<PERMIT_STATUS_HISTORY>(HistoryJson)!; }
    // Compare native values, including inherited business/audit fields. Provider DateTime kind is not UTC provenance.
    public static string Payload(object row, params string[] excluded) => JsonSerializer.Serialize(row.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0 && !excluded.Contains(p.Name, StringComparer.Ordinal))
        .OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => new object?[] {p.Name, p.GetValue(row) is DateTime date ? date.Ticks : p.GetValue(row)}));
    public void Require()
    {
        if (Original is null || ApplicationJson is not {Length: > 0 and <= 131072} || HistoryJson is not {Length: > 0 and <= 131072})
            throw new InvalidOperationException("The recorded permit outcome is absent or exceeds its bounds.");
        var expected = Prepare(Original, RecordedAtUtc);
        var application = JsonSerializer.Deserialize<PERMIT_APPLICATION>(ApplicationJson);
        var history = JsonSerializer.Deserialize<PERMIT_STATUS_HISTORY>(HistoryJson);
        var actor = Original.Review.Context.Field.ActorUserId;
        if(application is null || Supporting is null)throw new InvalidOperationException("The recorded permit supporting outcome is absent.");
        Supporting.Require(application);
        if(PermitSupportingAmendments.EntityType(Original.Action) is not null)
        {
            var child=PermitSupportingAmendments.Read(Supporting,Original.Action); var target=PermitSupportingAmendments.Read(expected.Supporting,Original.Action)!;
            if(child is null || PermitSupportingAmendments.Replace(Supporting,target)!=expected.Supporting ||
                Payload(child,"ROW_CREATED_DATE","ROW_CHANGED_DATE")!=Payload(target,"ROW_CREATED_DATE","ROW_CHANGED_DATE") ||
                child.ROW_CHANGED_DATE is not {} childChanged || childChanged<RecordedAtUtc.UtcDateTime ||
                PermitSupportingAmendments.Read(Original.Review.Supporting,Original.Action) is {} before && child.ROW_CREATED_DATE!=before.ROW_CREATED_DATE ||
                PermitSupportingAmendments.Read(Original.Review.Supporting,Original.Action) is null && (child.ROW_CREATED_DATE is not {} childCreated || childCreated<RecordedAtUtc.UtcDateTime || childChanged<childCreated))
                throw new InvalidOperationException("The saved supporting details differ from the reviewed amendment or audit.");
        }
        else if(Original.Action=="ATTACH_DOCUMENT")
        {
            var id="PAF-"+Original.IntentId.ToString("N");var actual=Supporting.Attachments.SingleOrDefault(row=>row.APPLICATION_ATTACHMENT_ID==id);
            var target=expected.Supporting.Attachments.Single(row=>row.APPLICATION_ATTACHMENT_ID==id);
            if(actual is null || Payload(actual,"ROW_CREATED_DATE","ROW_CHANGED_DATE")!=Payload(target,"ROW_CREATED_DATE","ROW_CHANGED_DATE") ||
                actual.ROW_CREATED_DATE is not {} createdAttachment || createdAttachment<RecordedAtUtc.UtcDateTime || actual.ROW_CHANGED_DATE is not {} changedAttachment || changedAttachment<createdAttachment)
                throw new InvalidOperationException("The recorded attachment differs from its reviewed link/audit.");
            var normalized=expected.Supporting with {AttachmentsJson=JsonSerializer.Serialize(expected.Supporting.Attachments.Select(row=>row.APPLICATION_ATTACHMENT_ID==id?actual:row).ToArray())};
            if(Supporting!=normalized)throw new InvalidOperationException("The link command changed other supporting evidence.");
        }
        else if(Supporting!=expected.Supporting)throw new InvalidOperationException("A permit command changed supporting evidence without an amendment.");
        var excludedApplicationDates=Original.Action=="CREATE_DRAFT"?new[]{"ROW_CREATED_DATE","ROW_CHANGED_DATE"}:new[]{"ROW_CHANGED_DATE"};
        if (application is null || history is null ||
            Payload(application, excludedApplicationDates) != Payload(JsonSerializer.Deserialize<PERMIT_APPLICATION>(expected.ApplicationJson)!, excludedApplicationDates) ||
            Payload(history, "ROW_CREATED_DATE", "ROW_CHANGED_DATE") != Payload(JsonSerializer.Deserialize<PERMIT_STATUS_HISTORY>(expected.HistoryJson)!, "ROW_CREATED_DATE", "ROW_CHANGED_DATE") ||
            application.ROW_CHANGED_DATE is not { } changed || changed < RecordedAtUtc.UtcDateTime || application.ROW_CHANGED_BY != actor ||
            Original.Action=="CREATE_DRAFT" && (application.ROW_CREATED_BY!=actor || application.ROW_CREATED_DATE is not {} applicationCreated ||
                applicationCreated<RecordedAtUtc.UtcDateTime || changed<applicationCreated) ||
            history.ROW_CREATED_DATE is not { } created || created < RecordedAtUtc.UtcDateTime || history.ROW_CHANGED_DATE is not { } historyChanged || historyChanged < created ||
            history.ROW_CREATED_BY != actor || history.ROW_CHANGED_BY != actor)
            throw new InvalidOperationException("The native permit outcome differs from its reviewed command or recorded audit.");
    }
}
