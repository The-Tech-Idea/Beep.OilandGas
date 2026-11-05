using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Append-only reviewer command recovery evidence. Business lifecycle remains on AR_INVOICE.</summary>
public sealed class SALES_RECEIVABLE_REVIEW_INTENT:ModelEntityBase
{
    public const int MaximumCommandLength=134217728;
    [Key,Required,MaxLength(32)] public string SALES_RECEIVABLE_REVIEW_INTENT_ID{get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID{get;set;}="";
    [Required,MaxLength(32)] public string AR_INVOICE_ID{get;set;}="";
    [Required,MaxLength(64)] public string SOURCE_REVISION{get;set;}="";
    [Required,MaxLength(MaximumCommandLength)] public string COMMAND_JSON{get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION{get;set;}="";
    public static string Revision(SALES_RECEIVABLE_REVIEW_INTENT row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static SALES_RECEIVABLE_REVIEW_INTENT Create(CompanySalesReceivableDecisionCommand command,DateTime at)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();
        var row=new SALES_RECEIVABLE_REVIEW_INTENT{SALES_RECEIVABLE_REVIEW_INTENT_ID=command.IntentId,OWNER_TENANT_ID=command.Company.TenantId,OWNER_COMPANY_BA_ID=command.Company.BusinessAssociateId,
            FIELD_ID=command.Review.FieldId,AR_INVOICE_ID=command.ReceivableId,SOURCE_REVISION=command.Review.Revision,COMMAND_JSON=JsonSerializer.Serialize(command),PPDM_GUID=command.IntentId,ACTIVE_IND="Y",
            ROW_CREATED_BY=command.ActorUserId,ROW_CHANGED_BY=command.ActorUserId,ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};row.RECORDED_REVISION=Revision(row);_=row.Restore(command.Company,command.ActorUserId);return row;
    }
    public CompanySalesReceivableDecisionCommand Restore(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);
        if(OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || ACTIVE_IND!="Y" || ROW_CREATED_BY!=actor || ROW_CHANGED_BY!=actor ||
            ROW_CREATED_DATE is not {} at || at==default || at.Kind!=DateTimeKind.Utc || ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<at ||
            string.IsNullOrWhiteSpace(COMMAND_JSON) || COMMAND_JSON.Length>MaximumCommandLength || RECORDED_REVISION!=Revision(this))throw new InvalidOperationException("Original reviewer journal ownership, audit or revision is unconfirmed.");
        CompanySalesReceivableDecisionCommand command;
        try{command=JsonSerializer.Deserialize<CompanySalesReceivableDecisionCommand>(COMMAND_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow})??throw new InvalidOperationException("The reviewer command cannot be null.");}
        catch(JsonException){throw new InvalidOperationException("The original reviewer command is malformed.");}
        command.Require();
        if(command.Company!=company || command.ActorUserId!=actor || command.IntentId!=SALES_RECEIVABLE_REVIEW_INTENT_ID || command.ReceivableId!=AR_INVOICE_ID || command.Review.FieldId!=FIELD_ID || command.Review.Revision!=SOURCE_REVISION ||
            command.Review.Original.Receipt.ROW_CREATED_DATE>at || PPDM_GUID!=command.IntentId || JsonSerializer.Serialize(command)!=COMMAND_JSON)throw new InvalidOperationException("The reviewer journal differs from its exact original independent command.");
        return CompanyInventorySourceReview.Copy(command);
    }
}
