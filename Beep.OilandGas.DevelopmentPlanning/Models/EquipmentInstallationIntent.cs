using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>Confirmed original installation request; native preflight and transaction are still required.</summary>
public sealed record DevelopmentEquipmentInstallation(
    [property:JsonRequired] DevelopmentEquipmentInstallationReview Review,
    [property:JsonRequired] Guid IntentId,
    [property:JsonRequired] Guid InstallationGuid,
    [property:JsonRequired] DateTime InstalledAtUtc,
    [property:JsonRequired] bool Confirmed)
{
    public void Require()
    {
        if(Review is null)throw new InvalidOperationException("The original source review is required.");Review.Require();
        if(IntentId==Guid.Empty || InstallationGuid==Guid.Empty || IntentId==InstallationGuid || !Confirmed || InstalledAtUtc==default || InstalledAtUtc.Kind!=DateTimeKind.Utc || InstalledAtUtc>DateTime.UtcNow || JsonSerializer.SerializeToUtf8Bytes(this).Length>262144)
            throw new InvalidOperationException("Distinct original identifiers, an explicit past or current UTC installation time, and confirmation are required.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public FACILITY_EQUIPMENT Expected(decimal observation,DateTime recordedUtc)
    {
        Require();if(observation<0 || decimal.Truncate(observation)!=observation || recordedUtc==default || recordedUtc.Kind!=DateTimeKind.Utc || recordedUtc<InstalledAtUtc)throw new InvalidOperationException("An exact observation and UTC audit are required.");
        var p=Review.Proposal;return new FACILITY_EQUIPMENT{FACILITY_ID=p.FacilityId,FACILITY_TYPE=p.FacilityType,EQUIPMENT_ID=p.EquipmentId,INSTALL_OBS_NO=observation,ACTIVE_IND="Y",EFFECTIVE_DATE=InstalledAtUtc,INSTALL_DATE=InstalledAtUtc,PPDM_GUID=InstallationGuid.ToString("D").ToUpperInvariant(),ROW_CREATED_BY=p.Context.ActorUserId,ROW_CHANGED_BY=p.Context.ActorUserId,ROW_CREATED_DATE=recordedUtc,ROW_CHANGED_DATE=recordedUtc};
    }
}

/// <summary>Exact installation original and result for native transactional recovery.</summary>
public sealed class FACILITY_EQUIPMENT_INSTALL_INTENT:ModelEntityBase
{
    [Key,Required,MaxLength(32)] public string FACILITY_EQUIPMENT_INSTALL_INTENT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string FACILITY_ID {get;set;}="";
    [Required,MaxLength(128)] public string FACILITY_TYPE {get;set;}="";
    [Required,MaxLength(128)] public string EQUIPMENT_ID {get;set;}="";
    [Required,MaxLength(262144)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(262144)] public string INSTALLATION_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
    public static string EntryRevision(FACILITY_EQUIPMENT_INSTALL_INTENT entry){var copy=CompanyInventorySourceReview.Copy(entry);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static FACILITY_EQUIPMENT_INSTALL_INTENT Create(DevelopmentEquipmentInstallation command,FACILITY_EQUIPMENT installation)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(installation);command.Require();
        if(installation.ROW_CREATED_DATE is not {} utc || utc.Kind!=DateTimeKind.Utc || JsonSerializer.Serialize(command.Expected(installation.INSTALL_OBS_NO,utc))!=JsonSerializer.Serialize(installation))throw new InvalidOperationException("The installation differs from its confirmed original.");
        var commandJson=JsonSerializer.Serialize(command);var installationJson=JsonSerializer.Serialize(installation);if(commandJson.Length>262144 || installationJson.Length>262144)throw new InvalidOperationException("The installation original is too large to retain.");
        var p=command.Review.Proposal;var entry=new FACILITY_EQUIPMENT_INSTALL_INTENT{FACILITY_EQUIPMENT_INSTALL_INTENT_ID=command.IntentId.ToString("N"),OWNER_TENANT_ID=p.Context.Company.TenantId,OWNER_COMPANY_BA_ID=p.Context.Company.BusinessAssociateId,FIELD_ID=p.Context.FieldId!,FACILITY_ID=p.FacilityId,FACILITY_TYPE=p.FacilityType,EQUIPMENT_ID=p.EquipmentId,COMMAND_JSON=commandJson,INSTALLATION_JSON=installationJson,ACTIVE_IND="Y",PPDM_GUID=command.IntentId.ToString("D").ToUpperInvariant(),ROW_CREATED_BY=p.Context.ActorUserId,ROW_CHANGED_BY=p.Context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};
        entry.RECORDED_REVISION=EntryRevision(entry);return entry;
    }
    public (DevelopmentEquipmentInstallation Command,FACILITY_EQUIPMENT Installation) Restore(CompanyScope company,string actor)
    {
        if(company is null || !LeaseSummaryValidator.Identifier(actor) || OWNER_TENANT_ID!=company.TenantId || OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || ACTIVE_IND!="Y" || ROW_CREATED_BY!=actor || ROW_CHANGED_BY!=actor || ROW_CREATED_DATE is null || ROW_CHANGED_DATE!=ROW_CREATED_DATE || !ProcessDefinitionRevision.IsValid(RECORDED_REVISION) || RECORDED_REVISION!=EntryRevision(this) || string.IsNullOrEmpty(COMMAND_JSON) || COMMAND_JSON.Length>262144 || string.IsNullOrEmpty(INSTALLATION_JSON) || INSTALLATION_JSON.Length>262144)throw new InvalidOperationException("The installation original, ownership or audit is unconfirmed.");
        var command=JsonSerializer.Deserialize<DevelopmentEquipmentInstallation>(COMMAND_JSON)??throw new InvalidOperationException("The original installation command is missing.");var installation=JsonSerializer.Deserialize<FACILITY_EQUIPMENT>(INSTALLATION_JSON)??throw new InvalidOperationException("The original installation is missing.");command.Require();var expected=Create(command,installation);
        if(command.Review.Proposal.Context.Company!=company || command.Review.Proposal.Context.ActorUserId!=actor || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(this))throw new InvalidOperationException("The retained installation differs from its original proof.");return(command,installation);
    }
}

public interface ICompanyEquipmentInstallationOwner
{
    Task<FACILITY_EQUIPMENT_INSTALL_INTENT> RecordEquipmentInstallationAsync(DevelopmentEquipmentInstallation input,Func<Task> authority);
    Task<FACILITY_EQUIPMENT_INSTALL_INTENT> RecoverEquipmentInstallationAsync(DevelopmentEquipmentInstallation input,Func<Task> readAuthority);
}

public sealed record DevelopmentEquipmentInstallationReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]FACILITY_EQUIPMENT Installation,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentEquipmentInstallation original)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();
        if(Installation is null || IntentRevision!=original.Revision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || CheckedAtUtc>DateTimeOffset.UtcNow)throw new InvalidOperationException("The installation outcome is unconfirmed.");
        _=FACILITY_EQUIPMENT_INSTALL_INTENT.Create(original,Installation);
    }
}
