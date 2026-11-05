using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>Append-only original receivable draft receipt; AR_INVOICE remains the business owner.</summary>
public sealed class SALES_RECEIVABLE_OUTCOME:ModelEntityBase
{
    public const int MaximumSnapshotLength=134217728;
    [Key,Required,MaxLength(32)]public string SALES_RECEIVABLE_OUTCOME_ID{get;set;}="";
    [Required,MaxLength(64)]public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)]public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)]public string SOURCE_INVOICE_ID{get;set;}="";
    [Required,MaxLength(32)]public string AR_INVOICE_ID{get;set;}="";
    [Required,MaxLength(64)]public string INTENT_REVISION{get;set;}="";
    [Required,MaxLength(MaximumSnapshotLength)]public string RECEIVABLE_JSON{get;set;}="";
    [Required,MaxLength(64)]public string RECORDED_REVISION{get;set;}="";
    public static SALES_RECEIVABLE_OUTCOME Create(SALES_RECEIVABLE_INTENT intent,AR_INVOICE draft,DateTime at)
    {
        var command=intent.Restore(new(intent.OWNER_TENANT_ID,intent.OWNER_COMPANY_BA_ID),intent.ROW_CREATED_BY);_=CompanySalesReceivableProof.Require(command.Source.Company,draft);
        var result=new SALES_RECEIVABLE_OUTCOME{SALES_RECEIVABLE_OUTCOME_ID=command.IntentId,OWNER_TENANT_ID=command.Source.Company.TenantId,OWNER_COMPANY_BA_ID=command.Source.Company.BusinessAssociateId,SOURCE_INVOICE_ID=command.Source.InvoiceId,
            AR_INVOICE_ID=draft.AR_INVOICE_ID,INTENT_REVISION=intent.RECORDED_REVISION,RECEIVABLE_JSON=JsonSerializer.Serialize(draft),ACTIVE_IND="Y",PPDM_GUID=command.IntentId,ROW_CREATED_BY=command.Source.ActorUserId,ROW_CHANGED_BY=command.Source.ActorUserId,ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};
        result.RECORDED_REVISION=Revision(result);_=result.Restore(intent);return result;
    }
    public AR_INVOICE Restore(SALES_RECEIVABLE_INTENT intent)
    {
        var company=new CompanyScope(intent.OWNER_TENANT_ID,intent.OWNER_COMPANY_BA_ID);var command=intent.Restore(company,intent.ROW_CREATED_BY);
        if(OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || SALES_RECEIVABLE_OUTCOME_ID!=command.IntentId || SOURCE_INVOICE_ID!=command.Source.InvoiceId || AR_INVOICE_ID!=command.ReceivableId || INTENT_REVISION!=intent.RECORDED_REVISION ||
            PPDM_GUID!=command.IntentId || ACTIVE_IND!="Y" || ROW_CREATED_BY!=command.Source.ActorUserId || ROW_CHANGED_BY!=command.Source.ActorUserId || ROW_CREATED_DATE is not {} at || at==default || at.Kind!=DateTimeKind.Utc || at<intent.ROW_CREATED_DATE || ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<at ||
            RECORDED_REVISION!=Revision(this) || string.IsNullOrWhiteSpace(RECEIVABLE_JSON) || RECEIVABLE_JSON.Length>MaximumSnapshotLength)throw new InvalidOperationException("The exact original receivable outcome receipt is unconfirmed.");
        AR_INVOICE row;try{row=JsonSerializer.Deserialize<AR_INVOICE>(RECEIVABLE_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow}) ??throw new InvalidOperationException("Receivable snapshot cannot be null.");}
        catch(JsonException){throw new InvalidOperationException("The original receivable snapshot is malformed.");}
        if(row.STATUS!="DRAFT" || row.REVIEW_WORKFLOW_JSON is not null)throw new InvalidOperationException("The immutable preparation receipt must contain the original unreviewed draft.");
        var proof=CompanySalesReceivableProof.Require(company,row);
        if(JsonSerializer.Serialize(row)!=RECEIVABLE_JSON || JsonSerializer.Serialize(proof.Record.Command)!=JsonSerializer.Serialize(command) || row.ROW_CREATED_DATE<intent.ROW_CREATED_DATE || row.ROW_CHANGED_DATE>at)throw new InvalidOperationException("The original receivable outcome differs from its exact retained command and draft.");return CompanyInventorySourceReview.Copy(row);
    }
    public static string Revision(SALES_RECEIVABLE_OUTCOME row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
