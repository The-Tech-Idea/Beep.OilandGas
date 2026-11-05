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

/// <summary>Append-only original invoice preparation journal; invoices remain in the existing INVOICE owner.</summary>
public sealed class SALES_INVOICE_INTENT:ModelEntityBase
{
    public const int MaximumCommandLength=4194304;
    [Key,Required,MaxLength(32)] public string SALES_INVOICE_INTENT_ID{get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID{get;set;}="";
    [Required,MaxLength(128)] public string RUN_TICKET_ID{get;set;}="";
    [Required,MaxLength(32)] public string INVOICE_ID{get;set;}="";
    [Required,MaxLength(64)] public string SOURCE_REVISION{get;set;}="";
    [Required,MaxLength(64)] public string SCOPE_KEY{get;set;}="";
    [Required,MaxLength(MaximumCommandLength)] public string COMMAND_JSON{get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION{get;set;}="";
    public static string Revision(SALES_INVOICE_INTENT row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static SALES_INVOICE_INTENT Create(CompanySalesInvoiceDraftCommand command,DateTime at)
    {
        command=CompanyInventorySourceReview.Copy(command);new CompanySalesInvoiceDraftRecord(command,at).Require();var source=command.Review;
        var row=new SALES_INVOICE_INTENT{SALES_INVOICE_INTENT_ID=command.IntentId,OWNER_TENANT_ID=source.Company.TenantId,OWNER_COMPANY_BA_ID=source.Company.BusinessAssociateId,FIELD_ID=source.FieldId,RUN_TICKET_ID=source.TicketId,
            INVOICE_ID=command.InvoiceId,SOURCE_REVISION=source.Revision,SCOPE_KEY=source.SourceKey,COMMAND_JSON=JsonSerializer.Serialize(command),PPDM_GUID=command.IntentId,ACTIVE_IND="Y",ROW_CREATED_BY=source.ActorUserId,ROW_CHANGED_BY=source.ActorUserId,ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};
        row.RECORDED_REVISION=Revision(row);_=row.Restore(source.Company,source.ActorUserId);return row;
    }
    public CompanySalesInvoiceDraftCommand Restore(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);
        if(OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || ACTIVE_IND!="Y" || ROW_CREATED_BY!=actor || ROW_CHANGED_BY!=actor || ROW_CREATED_DATE is not {} at || at==default || at.Kind!=DateTimeKind.Utc || ROW_CHANGED_DATE is not {} changed || changed<at || changed.Kind!=DateTimeKind.Utc ||
            RECORDED_REVISION!=Revision(this) || string.IsNullOrWhiteSpace(COMMAND_JSON) || COMMAND_JSON.Length>MaximumCommandLength)throw new InvalidOperationException("Original invoice journal ownership, audit or revision is unconfirmed.");
        CompanySalesInvoiceDraftCommand command;try{command=JsonSerializer.Deserialize<CompanySalesInvoiceDraftCommand>(COMMAND_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow}) ??throw new InvalidOperationException("Invoice command cannot be null.");}catch(JsonException){throw new InvalidOperationException("Original invoice command is malformed.");}
        new CompanySalesInvoiceDraftRecord(command,at).Require();var source=command.Review;
        if(JsonSerializer.Serialize(command)!=COMMAND_JSON || source.Company!=company || source.ActorUserId!=actor || source.FieldId!=FIELD_ID || source.TicketId!=RUN_TICKET_ID || source.Revision!=SOURCE_REVISION || source.SourceKey!=SCOPE_KEY || command.IntentId!=SALES_INVOICE_INTENT_ID || command.InvoiceId!=INVOICE_ID || PPDM_GUID!=command.IntentId)
            throw new InvalidOperationException("The native invoice journal differs from its exact original command.");return CompanyInventorySourceReview.Copy(command);
    }
}
