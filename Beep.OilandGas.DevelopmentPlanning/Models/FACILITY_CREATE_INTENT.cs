using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>Original creation command and its exact facility and FIELD-link result.</summary>
public sealed class FACILITY_CREATE_INTENT:ModelEntityBase
{
    [Key,Required,MaxLength(32)] public string FACILITY_CREATE_INTENT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string FACILITY_ID {get;set;}="";
    [Required,MaxLength(128)] public string FACILITY_TYPE {get;set;}="";
    [Required,MaxLength(262144)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(262144)] public string FACILITY_JSON {get;set;}="";
    [Required,MaxLength(262144)] public string FIELD_LINK_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
    public static string EntryRevision(FACILITY_CREATE_INTENT entry){var copy=CompanyInventorySourceReview.Copy(entry);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static FACILITY_CREATE_INTENT Create(DevelopmentFacilityCreation command,FACILITY facility,FACILITY_FIELD link)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(facility);ArgumentNullException.ThrowIfNull(link);command.Require();
        if(facility.ROW_CREATED_DATE is not {} utc || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("The creation result requires a UTC audit.");
        var expected=command.Expected(utc);
        if(JsonSerializer.Serialize(expected.Facility)!=JsonSerializer.Serialize(facility) || JsonSerializer.Serialize(expected.Link)!=JsonSerializer.Serialize(link))throw new InvalidOperationException("The facility and FIELD link differ from the exact approved original.");
        var commandJson=JsonSerializer.Serialize(command);var facilityJson=JsonSerializer.Serialize(facility);var linkJson=JsonSerializer.Serialize(link);
        if(commandJson.Length>262144 || facilityJson.Length>262144 || linkJson.Length>262144)throw new InvalidOperationException("The original facility creation is too large to retain.");
        var context=command.Review.Draft.Context;var entry=new FACILITY_CREATE_INTENT{FACILITY_CREATE_INTENT_ID=command.IntentId.ToString("N"),OWNER_TENANT_ID=context.Company.TenantId,OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,FIELD_ID=context.FieldId!,FACILITY_ID=facility.FACILITY_ID,FACILITY_TYPE=facility.FACILITY_TYPE,
            COMMAND_JSON=commandJson,FACILITY_JSON=facilityJson,FIELD_LINK_JSON=linkJson,ACTIVE_IND="Y",PPDM_GUID=command.IntentId.ToString("D").ToUpperInvariant(),ROW_CREATED_BY=context.ActorUserId,ROW_CHANGED_BY=context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};
        entry.RECORDED_REVISION=EntryRevision(entry);return entry;
    }
    public (DevelopmentFacilityCreation Command,FACILITY Facility,FACILITY_FIELD Link) Restore(CompanyScope company,string actor)
    {
        if(company is null || !LeaseSummaryValidator.Identifier(actor) || OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || ACTIVE_IND!="Y" || ROW_CREATED_BY!=actor || ROW_CHANGED_BY!=actor || ROW_CREATED_DATE is null || ROW_CHANGED_DATE!=ROW_CREATED_DATE ||
           !ProcessDefinitionRevision.IsValid(RECORDED_REVISION) || RECORDED_REVISION!=EntryRevision(this) || string.IsNullOrEmpty(COMMAND_JSON) || COMMAND_JSON.Length>262144 || string.IsNullOrEmpty(FACILITY_JSON) || FACILITY_JSON.Length>262144 || string.IsNullOrEmpty(FIELD_LINK_JSON) || FIELD_LINK_JSON.Length>262144)
            throw new InvalidOperationException("The original facility creation ownership, audit or retained record is unconfirmed.");
        var command=JsonSerializer.Deserialize<DevelopmentFacilityCreation>(COMMAND_JSON)??throw new InvalidOperationException("The original command is missing.");var facility=JsonSerializer.Deserialize<FACILITY>(FACILITY_JSON)??throw new InvalidOperationException("The original facility is missing.");var link=JsonSerializer.Deserialize<FACILITY_FIELD>(FIELD_LINK_JSON)??throw new InvalidOperationException("The original FIELD link is missing.");
        command.Require();var expected=Create(command,facility,link);
        if(command.Review.Draft.Context.Company!=company || command.Review.Draft.Context.ActorUserId!=actor || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(this))throw new InvalidOperationException("The retained facility creation differs from its original proof.");
        return(command,facility,link);
    }
}
