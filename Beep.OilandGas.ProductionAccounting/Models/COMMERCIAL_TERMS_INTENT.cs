using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.ProductionAccounting.Models;

/// <summary>Append-only reviewed amendment retained before any commercial terms mutation.</summary>
public sealed class COMMERCIAL_TERMS_INTENT:ModelEntityBase
{
    public const int MaximumCommandLength=4194304;
    [Key,Required,MaxLength(32)] public string COMMERCIAL_TERMS_INTENT_ID{get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID{get;set;}="";
    [Required,MaxLength(128)] public string SALES_CONTRACT_ID{get;set;}="";
    [Required,MaxLength(MaximumCommandLength)] public string COMMAND_JSON{get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION{get;set;}="";
    public static string Revision(COMMERCIAL_TERMS_INTENT entry)
    {var copy=CompanyInventorySourceReview.Copy(entry);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static COMMERCIAL_TERMS_INTENT Create(CompanyCommercialAmendmentSubmission command,DateTime at)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();
        if(at.Kind!=DateTimeKind.Utc)throw new ArgumentException("A UTC intent audit is required.");
        var review=command.Review;var id=command.Amendment.Preparation.IntentId;
        var result=new COMMERCIAL_TERMS_INTENT{COMMERCIAL_TERMS_INTENT_ID=id,OWNER_TENANT_ID=review.Company.TenantId,
            OWNER_COMPANY_BA_ID=review.Company.BusinessAssociateId,PROCESS_INSTANCE_ID=review.InstanceId,SALES_CONTRACT_ID=review.Source.Contract.SALES_CONTRACT_ID,
            COMMAND_JSON=JsonSerializer.Serialize(command),PPDM_GUID=id,ACTIVE_IND="Y",ROW_CREATED_BY=review.ActorUserId,ROW_CHANGED_BY=review.ActorUserId,
            ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};
        result.RECORDED_REVISION=Revision(result);_=result.Restore(review.Company,review.ActorUserId,review.InstanceId);return result;
    }
    public CompanyCommercialAmendmentSubmission Restore(CompanyScope company,string actor,string process)
    {
        APInvoiceOwnership.RequireScope(company);
        if(OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || PROCESS_INSTANCE_ID!=process ||
            string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) || ROW_CREATED_BY!=actor || ROW_CHANGED_BY!=actor || ACTIVE_IND!="Y" ||
            ROW_CREATED_DATE is null || ROW_CREATED_DATE.Value.Kind!=DateTimeKind.Utc || ROW_CHANGED_DATE!=ROW_CREATED_DATE ||
            !ProcessDefinitionRevision.IsValid(RECORDED_REVISION) || RECORDED_REVISION!=Revision(this) ||
            string.IsNullOrWhiteSpace(COMMAND_JSON) || COMMAND_JSON.Length>MaximumCommandLength)
            throw new InvalidOperationException("Original amendment intent ownership, audit or revision is unconfirmed.");
        CompanyCommercialAmendmentSubmission? result;
        try{result=JsonSerializer.Deserialize<CompanyCommercialAmendmentSubmission>(COMMAND_JSON);}catch(JsonException error){throw new InvalidOperationException("Original amendment intent is malformed.",error);}
        if(result is null || JsonSerializer.Serialize(result)!=COMMAND_JSON)throw new InvalidOperationException("Original amendment intent is not canonical.");
        result.Require();
        if(result.Review.Company!=company || result.Review.ActorUserId!=actor || result.Review.InstanceId!=process ||
            result.Review.Source.Contract.SALES_CONTRACT_ID!=SALES_CONTRACT_ID || result.Amendment.Preparation.IntentId!=COMMERCIAL_TERMS_INTENT_ID || PPDM_GUID!=COMMERCIAL_TERMS_INTENT_ID)
            throw new InvalidOperationException("Original amendment intent identity differs from its command.");
        return CompanyInventorySourceReview.Copy(result);
    }
}

/// <summary>A confirmed original-process query; a missing entry is distinct from a missing response.</summary>
public sealed record CompanyCommercialAmendmentIntentReview(CompanyScope Company,string ActorUserId,string InstanceId,string ProcessRevision,string BindingRevision,COMMERCIAL_TERMS_INTENT? Entry)
{
    public void Require(CompanyScope company,string actor,string process)
    {
        APInvoiceOwnership.RequireScope(company);
        if(Company!=company || ActorUserId!=actor || InstanceId!=process || string.IsNullOrWhiteSpace(process) || process.Length>128 || process.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) || !ProcessDefinitionRevision.IsValid(ProcessRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision))throw new InvalidOperationException("The original company/account/process intent review is required.");
        if(Entry is null)return;
        var command=Entry.Restore(company,actor,process);
        if(command.Review.ProcessRevision!=ProcessRevision || command.Review.BindingRevision!=BindingRevision)throw new InvalidOperationException("The saved intent differs from its original process/binding.");
    }
}
