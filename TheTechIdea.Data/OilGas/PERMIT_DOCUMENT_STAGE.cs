using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

/// <summary>Permit-owned upload intent and confirmation journal. File bytes belong only to native RM_FILE_CONTENT.</summary>
public sealed class PERMIT_DOCUMENT_STAGE:ModelEntityBase
{
    [Key,MaxLength(32)] public string PERMIT_DOCUMENT_STAGE_ID {get;set;}="";
    [MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [MaxLength(128)] public string OWNER_COMPANY_ID {get;set;}="";
    [MaxLength(128)] public string FIELD_ID {get;set;}="";
    [MaxLength(128)] public string PERMIT_APPLICATION_ID {get;set;}="";
    [MaxLength(2097152)] public string SEED_JSON {get;set;}="";
    [MaxLength(64)] public string SEED_HASH {get;set;}="";
    [MaxLength(131072)] public string? STAGED_JSON {get;set;}
    [MaxLength(64)] public string? STAGED_HASH {get;set;}
    public static PERMIT_DOCUMENT_STAGE Create(PermitDocumentStageSeed seed)
    {
        seed.Require();var c=seed.Original.Review.Context;var json=JsonSerializer.Serialize(seed);
        return new() { PERMIT_DOCUMENT_STAGE_ID=seed.Original.IntentId.ToString("N"),OWNER_TENANT_ID=c.Field.Company.TenantId,OWNER_COMPANY_ID=c.Field.Company.BusinessAssociateId,
            FIELD_ID=c.Field.FieldId!,PERMIT_APPLICATION_ID=seed.Original.Review.ReadApplication().PERMIT_APPLICATION_ID,SEED_JSON=json,SEED_HASH=ProcessDefinitionRevision.FromJson(json),
            ACTIVE_IND="Y",PPDM_GUID=seed.Nonce.ToString(),ROW_EFFECTIVE_DATE=seed.SeededAtUtc.UtcDateTime,ROW_CREATED_BY=c.Field.ActorUserId,ROW_CHANGED_BY=c.Field.ActorUserId,
            ROW_CREATED_DATE=seed.SeededAtUtc.UtcDateTime,ROW_CHANGED_DATE=seed.SeededAtUtc.UtcDateTime };
    }
    public PermitDocumentStageSeed Restore(PermitDocumentStageOriginal original)
    {
        original.Require();
        if(SEED_JSON is not {Length:>0 and <=2097152} || SEED_HASH!=ProcessDefinitionRevision.FromJson(SEED_JSON))throw new InvalidOperationException("Staging ownership journal is unconfirmed.");
        var seed=JsonSerializer.Deserialize<PermitDocumentStageSeed>(SEED_JSON)??throw new InvalidOperationException("The staging original is absent.");seed.Require();
        var expected=Create(seed);
        if(seed.Original!=original || PERMIT_DOCUMENT_STAGE_ID!=expected.PERMIT_DOCUMENT_STAGE_ID || OWNER_TENANT_ID!=expected.OWNER_TENANT_ID || OWNER_COMPANY_ID!=expected.OWNER_COMPANY_ID ||
            FIELD_ID!=expected.FIELD_ID || PERMIT_APPLICATION_ID!=expected.PERMIT_APPLICATION_ID || ACTIVE_IND!="Y" || PPDM_GUID!=expected.PPDM_GUID || ROW_EFFECTIVE_DATE?.Ticks!=expected.ROW_EFFECTIVE_DATE?.Ticks ||
            ROW_CREATED_BY!=expected.ROW_CREATED_BY || ROW_CHANGED_BY!=expected.ROW_CHANGED_BY || ROW_CREATED_DATE is not {} created || created<seed.SeededAtUtc.UtcDateTime || ROW_CHANGED_DATE is not {} changed || changed<created)
            throw new InvalidOperationException("The staging journal belongs to another original, company, FIELD, target, parent or actor.");
        if(STAGED_JSON is null ? STAGED_HASH is not null : STAGED_JSON.Length>131072 || STAGED_HASH!=ProcessDefinitionRevision.FromJson(STAGED_JSON))
            throw new InvalidOperationException("The recorded staging proof changed.");
        return seed;
    }
    private sealed record Confirmation(Guid IntentId,string SeedHash,string DocumentRevision,string InformationRevision,string FileRevision,DateTimeOffset StagedAtUtc);
    public void Confirm(PermitDocumentStagingProof proof)
    {
        var seed=Restore(proof.Seed.Original);proof.Require(seed.Original);
        if(seed!=proof.Seed || STAGED_JSON is not null)throw new InvalidOperationException("Staging confirmation requires its exact unconfirmed ownership seed.");
        STAGED_JSON=JsonSerializer.Serialize(new Confirmation(seed.Original.IntentId,SEED_HASH,proof.DocumentRevision,proof.InformationRevision,proof.FileRevision,proof.StagedAtUtc));
        STAGED_HASH=ProcessDefinitionRevision.FromJson(STAGED_JSON);
    }
    public PermitDocumentStagingProof? ReadProof(PermitDocumentStageOriginal original)
    {
        var seed=Restore(original);if(STAGED_JSON is null)return null;
        var confirmation=JsonSerializer.Deserialize<Confirmation>(STAGED_JSON)??throw new InvalidOperationException("The staging proof is absent.");
        if(confirmation.IntentId!=original.IntentId || confirmation.SeedHash!=SEED_HASH)throw new InvalidOperationException("Staging confirmation belongs to another ownership seed.");
        var proof=new PermitDocumentStagingProof(seed,confirmation.DocumentRevision,confirmation.InformationRevision,confirmation.FileRevision,confirmation.StagedAtUtc);
        proof.Require(original);return proof;
    }
}
