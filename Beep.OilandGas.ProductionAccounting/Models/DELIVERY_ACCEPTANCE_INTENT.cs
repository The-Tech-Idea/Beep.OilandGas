using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.ProductionAccounting.Models;

/// <summary>Append-only native PRODUCTION journal; delivery facts and approvals remain in their existing owner.</summary>
public sealed class DELIVERY_ACCEPTANCE_INTENT:ModelEntityBase
{
    public const int MaximumCommandLength=4194304;
    public const int MaximumEntries=32;
    public const long MaximumSetCharacters=8388608;
    public static void RequirePage(int page,int size)
    {if(page<1 || size<1 || size>MaximumEntries || (long)(page-1)*size>int.MaxValue)throw new ArgumentException("A bounded original delivery journal page is required.");}
    [Key,Required,MaxLength(32)] public string DELIVERY_ACCEPTANCE_INTENT_ID{get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID{get;set;}="";
    [Required,MaxLength(128)] public string RUN_TICKET_ID{get;set;}="";
    [Required,MaxLength(11)] public string INTENT_KIND{get;set;}="";
    [Required,MaxLength(64)] public string SOURCE_REVISION{get;set;}="";
    [Required,MaxLength(64)] public string SCOPE_KEY{get;set;}="";
    [Required,MaxLength(MaximumCommandLength)] public string COMMAND_JSON{get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION{get;set;}="";
    public static string Revision(DELIVERY_ACCEPTANCE_INTENT entry)
    {var copy=CompanyInventorySourceReview.Copy(entry);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static DELIVERY_ACCEPTANCE_INTENT Create(RevenueDeliveryAcceptanceIntent command,DateTime at)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();
        if(at==default || at.Kind!=DateTimeKind.Utc || command.Original.Facts.AcceptedAtUtc>at || command.Submission?.Preparation.PreparedAtUtc>at || command.Correction?.Correction.Previous.DecidedAtUtc>at)
            throw new ArgumentException("A current explicit UTC journal audit is required.");
        var result=new DELIVERY_ACCEPTANCE_INTENT{DELIVERY_ACCEPTANCE_INTENT_ID=command.IntentId,OWNER_TENANT_ID=command.Company.TenantId,
            OWNER_COMPANY_BA_ID=command.Company.BusinessAssociateId,FIELD_ID=command.Original.Source.Source.Source.FieldId,RUN_TICKET_ID=command.Original.Source.TicketId,
            INTENT_KIND=command.Kind,SOURCE_REVISION=command.SourceRevision,SCOPE_KEY=command.ScopeKey,COMMAND_JSON=JsonSerializer.Serialize(command),
            PPDM_GUID=command.IntentId,ACTIVE_IND="Y",ROW_CREATED_BY=command.ActorUserId,ROW_CHANGED_BY=command.ActorUserId,ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};
        result.RECORDED_REVISION=Revision(result);_=result.Restore(command.Company,command.ActorUserId,command.ScopeKey);return result;
    }
    public RevenueDeliveryAcceptanceIntent Restore(CompanyScope company,string actor,string scopeKey)
    {
        APInvoiceOwnership.RequireScope(company);
        if(OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || SCOPE_KEY!=scopeKey ||
            string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) || ROW_CREATED_BY!=actor || ROW_CHANGED_BY!=actor || ACTIVE_IND!="Y" ||
            ROW_CREATED_DATE is not {} at || at.Kind!=DateTimeKind.Utc || at==default || ROW_CHANGED_DATE!=at ||
            !ProcessDefinitionRevision.IsValid(SOURCE_REVISION) || !ProcessDefinitionRevision.IsValid(SCOPE_KEY) ||
            !ProcessDefinitionRevision.IsValid(RECORDED_REVISION) || RECORDED_REVISION!=Revision(this) || string.IsNullOrWhiteSpace(COMMAND_JSON) || COMMAND_JSON.Length>MaximumCommandLength)
            throw new InvalidOperationException("Original delivery intent ownership, audit or revision is unconfirmed.");
        RevenueDeliveryAcceptanceIntent? result;
        try{result=JsonSerializer.Deserialize<RevenueDeliveryAcceptanceIntent>(COMMAND_JSON);}catch(JsonException error){throw new InvalidOperationException("Original delivery intent is malformed.",error);}
        if(result is null || JsonSerializer.Serialize(result)!=COMMAND_JSON)throw new InvalidOperationException("The complete original delivery intent is required.");
        result.Require();
        if(result.Company!=company || result.ActorUserId!=actor || result.ScopeKey!=scopeKey || result.IntentId!=DELIVERY_ACCEPTANCE_INTENT_ID || PPDM_GUID!=result.IntentId ||
            result.Original.Source.TicketId!=RUN_TICKET_ID || result.Original.Source.Source.Source.FieldId!=FIELD_ID || result.SourceRevision!=SOURCE_REVISION || result.Kind!=INTENT_KIND ||
            result.Original.Facts.AcceptedAtUtc>at || result.Submission?.Preparation.PreparedAtUtc>at || result.Correction?.Correction.Previous.DecidedAtUtc>at)
            throw new InvalidOperationException("The saved delivery identity/source differs from its original command.");
        return CompanyInventorySourceReview.Copy(result);
    }
}
