using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace TheTechIdea.Data.OilGas;

/// <summary>Frozen supporting rows reviewed with the header; null means the exact row was absent.</summary>
public sealed record PermitApplicationSupportingSnapshot(
    [property: JsonRequired] string? DrillingJson,
    [property: JsonRequired] string? EnvironmentalJson,
    [property: JsonRequired] string? InjectionJson)
{
    [JsonRequired] public string AttachmentsJson { get; init; } = "[]";
    [JsonIgnore] public IReadOnlyList<APPLICATION_ATTACHMENT> Attachments =>
        Read<APPLICATION_ATTACHMENT[]>(AttachmentsJson) ?? throw new InvalidOperationException("Attachment population evidence is absent.");
    [JsonRequired] public string DocumentsJson {get;init;}="[]";
    [JsonIgnore] public IReadOnlyList<PermitAttachmentDocumentEvidence> Documents=>Read<PermitAttachmentDocumentEvidence[]>(DocumentsJson)??throw new InvalidOperationException("Attachment content evidence is absent.");
    [JsonIgnore] public bool AllDocumentsVerified=>Documents.Count==Attachments.Count && Documents.All(row=>row.State=="VERIFIED");
    public static readonly PermitApplicationSupportingSnapshot Empty = new(null, null, null);
    private static readonly JsonSerializerOptions Json = new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    [JsonIgnore] public DRILLING_PERMIT_APPLICATION? Drilling => Read<DRILLING_PERMIT_APPLICATION>(DrillingJson);
    [JsonIgnore] public ENVIRONMENTAL_PERMIT_APPLICATION? Environmental => Read<ENVIRONMENTAL_PERMIT_APPLICATION>(EnvironmentalJson);
    [JsonIgnore] public INJECTION_PERMIT_APPLICATION? Injection => Read<INJECTION_PERMIT_APPLICATION>(InjectionJson);
    private static T? Read<T>(string? value) where T : class
    {
        if (value is null) return null;
        if (value.Length is 0 or > 131072) throw new InvalidOperationException("Supporting permit evidence exceeds its bounds.");
        return JsonSerializer.Deserialize<T>(value, Json) ?? throw new InvalidOperationException("Supporting permit evidence is absent.");
    }
    public static PermitApplicationSupportingSnapshot Capture(PERMIT_APPLICATION parent, DRILLING_PERMIT_APPLICATION? drilling,
        ENVIRONMENTAL_PERMIT_APPLICATION? environmental, INJECTION_PERMIT_APPLICATION? injection, IReadOnlyList<APPLICATION_ATTACHMENT>? attachments = null, IReadOnlyList<PermitAttachmentDocumentEvidence>? documents = null)
    {
        var result = new PermitApplicationSupportingSnapshot(drilling is null ? null : JsonSerializer.Serialize(drilling, Json),
            environmental is null ? null : JsonSerializer.Serialize(environmental, Json), injection is null ? null : JsonSerializer.Serialize(injection, Json)) { AttachmentsJson=JsonSerializer.Serialize((attachments??[]).OrderBy(row=>row?.APPLICATION_ATTACHMENT_ID,StringComparer.Ordinal).ToArray(),Json) };
        result=result with {DocumentsJson=JsonSerializer.Serialize((documents??(attachments??[]).Select(row=>new PermitAttachmentDocumentEvidence(row?.APPLICATION_ATTACHMENT_ID??"","UNLINKED",null)).ToArray()).OrderBy(row=>row.AttachmentId,StringComparer.Ordinal).ToArray(),Json)};
        result.Require(parent); return result;
    }
    public void Require(PERMIT_APPLICATION parent)
    {
        var d=Drilling; var e=Environmental; var i=Injection;
        PermitApplicationSupportingRecords.RequireParent(parent,d,e,i);
        if(AttachmentsJson is not { Length: > 0 and <=131072 })throw new InvalidOperationException("Attachment population evidence is absent or exceeds its bounds.");
        var attachments=Attachments;
        if(attachments.Count>128 || attachments.Any(row=>row is null || row.ACTIVE_IND!="Y" ||
            !LeaseSummaryValidator.Identifier(row.APPLICATION_ATTACHMENT_ID) || row.APPLICATION_ATTACHMENT_ID.Length>128 || row.PERMIT_APPLICATION_ID!=parent.PERMIT_APPLICATION_ID ||
            string.IsNullOrWhiteSpace(row.FILE_NAME) || row.FILE_NAME.Length>255 || row.FILE_NAME.Any(char.IsControl) || row.FILE_SIZE is <0 ||
            (row.DOCUMENT_STAGE_ID is not null && !Guid.TryParseExact(row.DOCUMENT_STAGE_ID,"N",out _)) || row.FILE_SIZE_UOM is {Length:>16} ||
            row.FILE_TYPE is {Length:>128} || row.DOCUMENT_TYPE is {Length:>128} || row.DESCRIPTION is {Length:>4096} || row.UPLOADED_BY is {Length:>128}) ||
            attachments.Select(row=>row.APPLICATION_ATTACHMENT_ID).Distinct(StringComparer.Ordinal).Count()!=attachments.Count)
            throw new InvalidOperationException("Reviewed attachments must be a bounded unique active metadata population for the exact application; metadata does not prove file content.");
        if(DocumentsJson is not {Length:>0 and <=131072})throw new InvalidOperationException("Attachment content population exceeds its bound.");
        var documents=Documents;
        if(documents.Count!=attachments.Count || documents.Select(row=>row.AttachmentId).Distinct(StringComparer.Ordinal).Count()!=documents.Count)
            throw new InvalidOperationException("Exact complete attachment content evidence is required.");
        foreach(var row in attachments)(documents.SingleOrDefault(d=>d.AttachmentId==row.APPLICATION_ATTACHMENT_ID)??throw new InvalidOperationException("Attachment content evidence is missing.")).Require(row,parent);
        if (d is not null && d.ACTIVE_IND != "Y" || e is not null && e.ACTIVE_IND != "Y" || i is not null && i.ACTIVE_IND != "Y")
            throw new InvalidOperationException("Reviewed supporting evidence must be active.");
    }
}
