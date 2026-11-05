using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning;

/// <summary>Original facility edit and its exact recorded result, owned with the FACILITY transaction.</summary>
public sealed class FACILITY_EDIT_INTENT : ModelEntityBase
{
    [Key,Required,MaxLength(32)] public string FACILITY_EDIT_INTENT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string FACILITY_ID {get;set;}="";
    [Required,MaxLength(128)] public string FACILITY_TYPE {get;set;}="";
    [Required,MaxLength(262144)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(262144)] public string RESULT_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RESULT_REVISION {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";

    public static string EntryRevision(FACILITY_EDIT_INTENT value)
    {
        var copy=CompanyInventorySourceReview.Copy(value);copy.RECORDED_REVISION="";
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    public static FACILITY_EDIT_INTENT Create(DevelopmentFacilityEdit command,FACILITY result)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(result);command.Require();
        var context=command.Review.Context;var expected=CompanyInventorySourceReview.Copy(command.Review.Original);
        if(result.ROW_CHANGED_DATE is null || result.ROW_CHANGED_DATE.Value.Kind==DateTimeKind.Local || result.ROW_CHANGED_BY!=context.ActorUserId)
            throw new InvalidOperationException("The recorded facility edit requires the original actor and an explicit non-local audit timestamp.");
        expected.FACILITY_LONG_NAME=command.LongName;expected.DESCRIPTION=command.Description!;
        expected.ROW_CHANGED_BY=context.ActorUserId;expected.ROW_CHANGED_DATE=result.ROW_CHANGED_DATE;
        if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(result))
            throw new InvalidOperationException("The recorded facility differs from the exact approved descriptive change and audit.");
        var commandJson=JsonSerializer.Serialize(command);var resultJson=JsonSerializer.Serialize(result);
        if(commandJson.Length>262144 || resultJson.Length>262144)throw new InvalidOperationException("The original facility edit is too large to retain.");
        var entry=new FACILITY_EDIT_INTENT{FACILITY_EDIT_INTENT_ID=command.IntentId.ToString("N"),OWNER_TENANT_ID=context.Company.TenantId,
            OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,FIELD_ID=context.FieldId!,FACILITY_ID=result.FACILITY_ID,FACILITY_TYPE=result.FACILITY_TYPE,
            COMMAND_JSON=commandJson,RESULT_JSON=resultJson,RESULT_REVISION=DevelopmentFacilityEditReview.SourceDigest(result),
            ROW_CREATED_BY=context.ActorUserId,ROW_CREATED_DATE=result.ROW_CHANGED_DATE,ROW_CHANGED_BY=context.ActorUserId,ROW_CHANGED_DATE=result.ROW_CHANGED_DATE,ACTIVE_IND="Y",PPDM_GUID=command.IntentId.ToString("D").ToUpperInvariant()};
        entry.RECORDED_REVISION=EntryRevision(entry);return entry;
    }
    public (DevelopmentFacilityEdit Command,FACILITY Result) Restore(CompanyScope company,string actor)
    {
        if(company is null || !LeaseSummaryValidator.Identifier(actor) || OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId ||
            ACTIVE_IND!="Y" || ROW_CREATED_BY!=actor || ROW_CHANGED_BY!=actor || ROW_CREATED_DATE is null || ROW_CHANGED_DATE!=ROW_CREATED_DATE ||
            !ProcessDefinitionRevision.IsValid(RECORDED_REVISION) || RECORDED_REVISION!=EntryRevision(this) || string.IsNullOrEmpty(COMMAND_JSON) || COMMAND_JSON.Length>262144 ||
            string.IsNullOrEmpty(RESULT_JSON) || RESULT_JSON.Length>262144)
            throw new InvalidOperationException("The original facility edit ownership, audit or retained record is unconfirmed.");
        var command=JsonSerializer.Deserialize<DevelopmentFacilityEdit>(COMMAND_JSON) ?? throw new InvalidOperationException("The original facility edit is missing.");
        var result=JsonSerializer.Deserialize<FACILITY>(RESULT_JSON) ?? throw new InvalidOperationException("The original facility result is missing.");
        command.Require();var expected=Create(command,result);
        if(command.Review.Context.Company!=company || command.Review.Context.ActorUserId!=actor || FACILITY_EDIT_INTENT_ID!=command.IntentId.ToString("N") ||
            FIELD_ID!=command.Review.Context.FieldId || FACILITY_ID!=result.FACILITY_ID || FACILITY_TYPE!=result.FACILITY_TYPE ||
            RESULT_REVISION!=DevelopmentFacilityEditReview.SourceDigest(result) || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(this))
            throw new InvalidOperationException("The retained original facility edit or result differs from its exact recorded proof.");
        return(command,result);
    }
}
