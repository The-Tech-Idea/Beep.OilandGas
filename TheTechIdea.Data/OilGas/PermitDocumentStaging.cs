using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM.Models;
namespace TheTechIdea.Data.OilGas;

public sealed record PermitDocumentUploadProposal(
    [property:JsonRequired] string FileName,[property:JsonRequired] string Title,[property:JsonRequired] string DocumentType,
    [property:JsonRequired] string Language,[property:JsonRequired] string Version,[property:JsonRequired] string DigitalFormat,
    [property:JsonRequired] string InformationSubtype,[property:JsonRequired] string ContentHash,[property:JsonRequired] long ContentBytes,
    [property:JsonRequired] DateTimeOffset EffectiveFromUtc,[property:JsonRequired] DateTimeOffset EffectiveToUtc)
{
    public void Require()
    {
        if(new[]{FileName,Title,DocumentType,Language,Version,DigitalFormat,InformationSubtype}.Any(x=>string.IsNullOrWhiteSpace(x) || x.Length>128 || x!=x.Trim() || x.Any(char.IsControl)) ||
            FileName.Contains('/') || FileName.Contains((char)92) || !ProcessDefinitionRevision.IsValid(ContentHash) || ContentBytes is <=0 or >CompanyWorkOrderPermitDocumentFacts.MaximumContentBytes ||
            EffectiveFromUtc==default || EffectiveToUtc==default || EffectiveFromUtc.Offset!=TimeSpan.Zero || EffectiveToUtc.Offset!=TimeSpan.Zero || EffectiveToUtc<=EffectiveFromUtc)
            throw new InvalidOperationException("Review bounded explicit document metadata, SHA256/byte count and a UTC validity window; paths are not file names.");
    }
    public void RequireContent(byte[] content)
    {
        Require();ArgumentNullException.ThrowIfNull(content);
        if(content.LongLength!=ContentBytes || Convert.ToHexString(SHA256.HashData(content))!=ContentHash)
            throw new InvalidOperationException("The selected bytes differ from the retained upload original.");
    }
}
public sealed record PermitDocumentStageOriginal([property:JsonRequired] Guid IntentId,[property:JsonRequired] PermitApplicationSourceReview Review,
    [property:JsonRequired] PermitDocumentUploadProposal Document,[property:JsonRequired] string Reason)
{
    public void Require()
    {
        if(IntentId==Guid.Empty || Review is null || Document is null || string.IsNullOrWhiteSpace(Reason) || Reason!=Reason.Trim() || Reason.Length>4096)
            throw new InvalidOperationException("Retain one exact reviewed document upload original with a bounded reason.");
        Review.Require();Document.Require();
        if(Review.ReadApplication().STATUS is not (PermitApplicationStatus.Draft or PermitApplicationStatus.AdditionalInformationRequired))
            throw new InvalidOperationException("Only an author draft/correction application can stage a new document.");
        if(JsonSerializer.Serialize(this).Length>2097152)throw new InvalidOperationException("The retained document original exceeds its bound.");
    }
}
public sealed record PermitDocumentStageSeed([property:JsonRequired] PermitDocumentStageOriginal Original,[property:JsonRequired] Guid Nonce,
    [property:JsonRequired] DateTimeOffset SeededAtUtc,[property:JsonRequired] string CoreTarget,[property:JsonRequired] string PermitTarget)
{
    [JsonIgnore] public string DocumentId=>"PDA-"+Nonce.ToString("N");
    [JsonIgnore] public string InformationId=>"PDI-"+Nonce.ToString("N");
    [JsonIgnore] public string FileId=>"PDF-"+Nonce.ToString("N");
    public void Require()
    {
        if(Original is null || Nonce==Guid.Empty || SeededAtUtc==default || SeededAtUtc.Offset!=TimeSpan.Zero || !ProcessDefinitionRevision.IsValid(CoreTarget) || !ProcessDefinitionRevision.IsValid(PermitTarget))throw new InvalidOperationException("Native document staging ownership is absent.");
        Original.Require();
        if(JsonSerializer.Serialize(this).Length>2097152)throw new InvalidOperationException("The staging ownership seed exceeds its journal bound.");
    }
    public object[] BuildRows(byte[]? content)
    {
        Require();if(content is not null)Original.Document.RequireContent(content);
        var d=Original.Document;
        object[] rows=[new SOURCE_DOCUMENT { SOURCE_DOCUMENT_ID=DocumentId,DOCUMENT_TITLE=d.Title,DOCUMENT_TYPE=d.DocumentType,LANGUAGE=d.Language,
                EFFECTIVE_DATE=d.EffectiveFromUtc.UtcDateTime,EXPIRY_DATE=d.EffectiveToUtc.UtcDateTime },
            new RM_INFORMATION_ITEM { INFORMATION_ITEM_ID=InformationId,INFO_ITEM_SUBTYPE=d.InformationSubtype,SOURCE_DOCUMENT_ID=DocumentId,TITLE=d.Title,VERSION_NUM=d.Version,
                EFFECTIVE_DATE=d.EffectiveFromUtc.UtcDateTime,EXPIRY_DATE=d.EffectiveToUtc.UtcDateTime },
            new RM_FILE_CONTENT { FILE_ID=FileId,INFORMATION_ITEM_ID=InformationId,INFO_ITEM_SUBTYPE=d.InformationSubtype,DIGITAL_FORMAT=d.DigitalFormat,
                FILE_CONTENT=content!,FILE_SIZE=d.ContentBytes,FILE_SIZE_UOM="BYTE",EFFECTIVE_DATE=d.EffectiveFromUtc.UtcDateTime,EXPIRY_DATE=d.EffectiveToUtc.UtcDateTime }];
        foreach(var row in rows.Cast<IPPDMEntity>())
        {
            row.ACTIVE_IND="Y";row.PPDM_GUID=Nonce.ToString();row.ROW_EFFECTIVE_DATE=SeededAtUtc.UtcDateTime;
            row.ROW_CREATED_BY=Original.Review.Context.Field.ActorUserId;row.ROW_CHANGED_BY=row.ROW_CREATED_BY;
            row.ROW_CREATED_DATE=SeededAtUtc.UtcDateTime;row.ROW_CHANGED_DATE=SeededAtUtc.UtcDateTime;
        }
        return rows;
    }
    public void RequireRow(object row)
    {
        Require();var expected=BuildRows(null).Single(value=>value.GetType()==row.GetType());var audit=(IPPDMEntity)row;
        if(PermitApplicationCommandOutcome.Payload(row,"ROW_CREATED_DATE","ROW_CHANGED_DATE","FILE_CONTENT")!=PermitApplicationCommandOutcome.Payload(expected,"ROW_CREATED_DATE","ROW_CHANGED_DATE","FILE_CONTENT") ||
            audit.ROW_CREATED_DATE is not {} created || created<SeededAtUtc.UtcDateTime || audit.ROW_CHANGED_DATE is not {} changed || changed<created)
            throw new PermitDocumentEvidenceChangedException("Native document row differs from its exact staging nonce, parent request, header, restriction or audit.");
        if(row is RM_FILE_CONTENT file)
        {
            try { Original.Document.RequireContent(file.FILE_CONTENT); }
            catch(ArgumentNullException){throw new PermitDocumentEvidenceChangedException("The native staged bytes are absent.");}
            catch(InvalidOperationException){throw new PermitDocumentEvidenceChangedException("The native staged bytes differ from the retained original.");}
        }
    }
}
public sealed record PermitDocumentStagingProof([property:JsonRequired] PermitDocumentStageSeed Seed,[property:JsonRequired] string DocumentRevision,
    [property:JsonRequired] string InformationRevision,[property:JsonRequired] string FileRevision,[property:JsonRequired] DateTimeOffset StagedAtUtc)
{
    public static string Revision(object row)=>ProcessDefinitionRevision.FromJson(PermitApplicationCommandOutcome.Payload(row,"FILE_CONTENT"));
    public void Require(PermitDocumentStageOriginal original)
    {
        if(Seed is null || Seed.Original!=original || !ProcessDefinitionRevision.IsValid(DocumentRevision) || !ProcessDefinitionRevision.IsValid(InformationRevision) ||
            !ProcessDefinitionRevision.IsValid(FileRevision) || StagedAtUtc.Offset!=TimeSpan.Zero || StagedAtUtc<Seed.SeededAtUtc)
            throw new InvalidOperationException("Recorded staging proof differs from its exact original or native revisions.");
        Seed.Require();
    }
}
