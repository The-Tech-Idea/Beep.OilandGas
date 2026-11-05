using System.Text.Json;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using System.Text.Json.Serialization;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
namespace TheTechIdea.Data.OilGas;

/// <summary>Client-safe staged document projection; server nonce/target fingerprints are never transport values.</summary>
public sealed record PermitDocumentStageReceipt([property:JsonRequired] Guid StageIntentId,[property:JsonRequired] CompanyScope Company,
    [property:JsonRequired] string FieldId,[property:JsonRequired] AssetDatabaseScope Core,[property:JsonRequired] AssetDatabaseScope Permits,
    [property:JsonRequired] string ApplicationId,[property:JsonRequired] string UploadedBy,[property:JsonRequired] PermitDocumentUploadProposal Document,
    [property:JsonRequired] string DocumentRevision,[property:JsonRequired] string InformationRevision,[property:JsonRequired] string FileRevision,
    [property:JsonRequired] DateTimeOffset StagedAtUtc)
{
    public static PermitDocumentStageReceipt From(PermitDocumentStagingProof proof)
    {
        var original=proof.Seed.Original;proof.Require(original);var c=original.Review.Context;
        return new(original.IntentId,c.Field.Company,c.Field.FieldId!,c.Field.Database,c.Database,original.Review.ReadApplication().PERMIT_APPLICATION_ID,
            c.Field.ActorUserId,original.Document,proof.DocumentRevision,proof.InformationRevision,proof.FileRevision,proof.StagedAtUtc);
    }
    public void Require()
    {
        if(StageIntentId==Guid.Empty || Company is null || Core is null || Permits is null || Document is null ||
            !LeaseSummaryValidator.Identifier(Company.TenantId) || !LeaseSummaryValidator.Identifier(Company.BusinessAssociateId) ||
            !LeaseSummaryValidator.Identifier(FieldId) || !LeaseSummaryValidator.Identifier(ApplicationId) || !LeaseSummaryValidator.Identifier(UploadedBy) ||
            !LeaseSummaryValidator.Identifier(Core.ConnectionName) || !LeaseSummaryValidator.Identifier(Core.Fingerprint) ||
            !LeaseSummaryValidator.Identifier(Permits.ConnectionName) || !LeaseSummaryValidator.Identifier(Permits.Fingerprint) ||
            !ProcessDefinitionRevision.IsValid(DocumentRevision) || !ProcessDefinitionRevision.IsValid(InformationRevision) || !ProcessDefinitionRevision.IsValid(FileRevision) ||
            StagedAtUtc==default || StagedAtUtc.Offset!=TimeSpan.Zero)throw new InvalidOperationException("The staged document receipt is absent, unbound or malformed.");
        Document.Require();
    }
    public void RequireFor(string applicationId,PermitApplicationDataScope context)
    {
        Require();context.Require();
        if(ApplicationId!=applicationId || Company!=context.Field.Company || FieldId!=context.Field.FieldId || Core!=context.Field.Database || Permits!=context.Database)
            throw new InvalidOperationException("The staged document belongs to another application, company, FIELD or current database binding.");
    }
    public void RequireOriginal(PermitDocumentStageOriginal original)
    {
        original.Require();RequireFor(original.Review.ReadApplication().PERMIT_APPLICATION_ID,original.Review.Context);
        if(StageIntentId!=original.IntentId || UploadedBy!=original.Review.Context.Field.ActorUserId || Document!=original.Document)
            throw new InvalidOperationException("The staging result differs from the retained original.");
    }
    public bool Matches(APPLICATION_ATTACHMENT row)=>row.DOCUMENT_STAGE_ID==StageIntentId.ToString("N") && row.FILE_NAME==Document.FileName &&
        row.FILE_TYPE==Document.DigitalFormat && row.DOCUMENT_TYPE==Document.DocumentType && row.DESCRIPTION==Document.Title &&
        row.FILE_SIZE==Document.ContentBytes && row.FILE_SIZE_UOM=="BYTE" && row.UPLOADED_BY==UploadedBy && row.UPLOAD_DATE.Ticks==StagedAtUtc.UtcDateTime.Ticks;
}
public sealed record PermitAttachmentProposal([property:JsonRequired] PermitDocumentStageReceipt Document)
{
    public void Require(PermitApplicationSourceReview review)
    {
        var parent=review.ReadApplication();Document.RequireFor(parent.PERMIT_APPLICATION_ID,review.Context);
        if(parent.STATUS is not (PermitApplicationStatus.Draft or PermitApplicationStatus.AdditionalInformationRequired) ||
            review.Supporting.Attachments.Any(row=>row.DOCUMENT_STAGE_ID==Document.StageIntentId.ToString("N")))
            throw new InvalidOperationException("Link an unlinked staged document on a draft/correction application.");
    }
    public APPLICATION_ATTACHMENT Build(PermitApplicationCommand original,DateTimeOffset recorded)
    {
        Require(original.Review);var d=Document.Document;var actor=original.Review.Context.Field.ActorUserId;
        return new() { APPLICATION_ATTACHMENT_ID="PAF-"+original.IntentId.ToString("N"),PERMIT_APPLICATION_ID=Document.ApplicationId,DOCUMENT_STAGE_ID=Document.StageIntentId.ToString("N"),
            FILE_NAME=d.FileName,FILE_TYPE=d.DigitalFormat,DOCUMENT_TYPE=d.DocumentType,DESCRIPTION=d.Title,FILE_SIZE=d.ContentBytes,FILE_SIZE_UOM="BYTE",
            UPLOADED_BY=Document.UploadedBy,UPLOAD_DATE=Document.StagedAtUtc.UtcDateTime,ACTIVE_IND="Y",PPDM_GUID=original.IntentId.ToString(),ROW_EFFECTIVE_DATE=recorded.UtcDateTime,
            ROW_CREATED_BY=actor,ROW_CHANGED_BY=actor,ROW_CREATED_DATE=recorded.UtcDateTime,ROW_CHANGED_DATE=recorded.UtcDateTime };
    }
}
public sealed record PermitAttachmentDocumentEvidence([property:JsonRequired] string AttachmentId,[property:JsonRequired] string State,
    [property:JsonRequired] PermitDocumentStageReceipt? Receipt)
{
    public void Require(APPLICATION_ATTACHMENT row,PERMIT_APPLICATION parent)
    {
        if(AttachmentId!=row.APPLICATION_ATTACHMENT_ID || State is not ("VERIFIED" or "CHANGED" or "UNLINKED"))throw new InvalidOperationException("Attachment content evidence is malformed or foreign.");
        if(Receipt is {} receipt)
        {
            receipt.Require();
            if(receipt.ApplicationId!=parent.PERMIT_APPLICATION_ID || receipt.Company.TenantId!=parent.OWNER_TENANT_ID || receipt.Company.BusinessAssociateId!=parent.OWNER_COMPANY_ID ||
                receipt.FieldId!=parent.FIELD_ID || row.DOCUMENT_STAGE_ID!=receipt.StageIntentId.ToString("N"))throw new InvalidOperationException("The attachment content receipt belongs to another membership.");
        }
        if(State=="VERIFIED" && (Receipt is null || !Receipt.Matches(row)))throw new InvalidOperationException("Verified attachment metadata differs from its exact staged receipt.");
    }
}
public sealed class PermitDocumentEvidenceChangedException(string message):InvalidOperationException(message);

public sealed record PermitAttachmentContentQuery([property:JsonRequired] PermitApplicationSourceReview Review,[property:JsonRequired] string AttachmentId)
{
    public void Require()
    {
        if(Review is null || !LeaseSummaryValidator.Identifier(AttachmentId))throw new InvalidOperationException("An exact reviewed attachment is required.");
        Review.Require();if(!Review.Supporting.Attachments.Any(row=>row.APPLICATION_ATTACHMENT_ID==AttachmentId))throw new InvalidOperationException("The attachment is not in the reviewed application.");
    }
}
public sealed record PermitAttachmentContent([property:JsonRequired] PermitAttachmentContentQuery Original,[property:JsonRequired] PermitDocumentStageReceipt Receipt,[property:JsonRequired] byte[] Content)
{
    public void Require(PermitAttachmentContentQuery query)
    {
        query.Require();Receipt.RequireFor(query.Review.ReadApplication().PERMIT_APPLICATION_ID,query.Review.Context);
        if(Original!=query || query.Review.Supporting.Documents.Single(row=>row.AttachmentId==query.AttachmentId) is not {State:"VERIFIED",Receipt:{} expected} || Receipt!=expected)
            throw new InvalidOperationException("Content delivery differs from the exact reviewed attachment.");
        Receipt.Document.RequireContent(Content);
    }
}
public sealed record PermitDocumentStageRequest([property:JsonRequired] PermitDocumentStageOriginal Original,[property:JsonRequired] byte[] Content)
{
    public void Require(){if(Original is null)throw new InvalidOperationException("Retain the exact document original first.");Original.Require();Original.Document.RequireContent(Content);}
}
public sealed record PermitDocumentStageResponse([property:JsonRequired] PermitDocumentStageOriginal Original,[property:JsonRequired] PermitDocumentStageReceipt? Receipt)
{
    public void Require(PermitDocumentStageOriginal original)
    {original.Require();if(Original!=original)throw new InvalidOperationException("The stage response belongs to another original.");Receipt?.RequireOriginal(original);}
}
