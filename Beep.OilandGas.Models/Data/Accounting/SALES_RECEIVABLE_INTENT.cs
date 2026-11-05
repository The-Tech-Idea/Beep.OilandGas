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

/// <summary>Append-only original AR draft command; AR_INVOICE remains the business owner.</summary>
public sealed class SALES_RECEIVABLE_INTENT:ModelEntityBase
{
    public const int MaximumCommandLength=134217728;
    [Key,Required,MaxLength(32)]public string SALES_RECEIVABLE_INTENT_ID{get;set;}="";
    [Required,MaxLength(64)]public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)]public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)]public string FIELD_ID{get;set;}="";
    [Required,MaxLength(128)]public string SOURCE_INVOICE_ID{get;set;}="";
    [Required,MaxLength(32)]public string AR_INVOICE_ID{get;set;}="";
    [Required,MaxLength(MaximumCommandLength)]public string COMMAND_JSON{get;set;}="";
    [Required,MaxLength(64)]public string RECORDED_REVISION{get;set;}="";
    public static SALES_RECEIVABLE_INTENT Create(CompanySalesReceivableCommand command,DateTime at)
    {
        new CompanySalesReceivableRecord(command,at).Require();
        var result=new SALES_RECEIVABLE_INTENT{SALES_RECEIVABLE_INTENT_ID=command.IntentId,OWNER_TENANT_ID=command.Source.Company.TenantId,OWNER_COMPANY_BA_ID=command.Source.Company.BusinessAssociateId,FIELD_ID=command.Source.FieldId,
            SOURCE_INVOICE_ID=command.Source.InvoiceId,AR_INVOICE_ID=command.ReceivableId,COMMAND_JSON=JsonSerializer.Serialize(command),PPDM_GUID=command.IntentId,ACTIVE_IND="Y",ROW_CREATED_BY=command.Source.ActorUserId,ROW_CHANGED_BY=command.Source.ActorUserId,ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};
        result.RECORDED_REVISION=Revision(result);_=result.Restore(command.Source.Company,command.Source.ActorUserId);return result;
    }
    public CompanySalesReceivableCommand Restore(CompanyScope company,string actor)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();PurchaseOrderOwnership.RequireScope(company);
        if(COMMAND_JSON is null || COMMAND_JSON.Length==0 || COMMAND_JSON.Length>MaximumCommandLength || OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || ROW_CREATED_BY!=actor || ROW_CHANGED_BY!=actor || ACTIVE_IND!="Y" ||
            ROW_CREATED_DATE is not {} created || created==default || created.Kind!=DateTimeKind.Utc || ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<created || RECORDED_REVISION!=Revision(this))throw new InvalidOperationException("The original owned receivable journal is unconfirmed.");
        CompanySalesReceivableCommand command;
        try{command=JsonSerializer.Deserialize<CompanySalesReceivableCommand>(COMMAND_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow}) ??throw new InvalidOperationException("Receivable command cannot be null.");}
        catch(JsonException){throw new InvalidOperationException("The original receivable command is malformed.");}
        new CompanySalesReceivableRecord(command,created).Require();if(JsonSerializer.Serialize(command)!=COMMAND_JSON || command.Source.Company!=company || command.Source.ActorUserId!=actor || command.IntentId!=SALES_RECEIVABLE_INTENT_ID || command.ReceivableId!=AR_INVOICE_ID || command.Source.FieldId!=FIELD_ID || command.Source.InvoiceId!=SOURCE_INVOICE_ID || PPDM_GUID!=command.IntentId)
            throw new InvalidOperationException("The retained receivable journal differs from its original command.");return CompanyInventorySourceReview.Copy(command);
    }
    public static string Revision(SALES_RECEIVABLE_INTENT row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
