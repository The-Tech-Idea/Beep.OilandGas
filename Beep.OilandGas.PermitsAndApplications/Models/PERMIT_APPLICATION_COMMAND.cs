using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.PermitsAndApplications.Models;

/// <summary>Immutable original and confirmed native result, installed by the PERMITS Beep Migration manifest.</summary>
public sealed class PERMIT_APPLICATION_COMMAND : ModelEntityBase
{
    [Key, MaxLength(32)] public string PERMIT_APPLICATION_COMMAND_ID {get;set;} = "";
    [MaxLength(64)] public string OWNER_TENANT_ID {get;set;} = "";
    [MaxLength(128)] public string OWNER_COMPANY_ID {get;set;} = "";
    [MaxLength(128)] public string FIELD_ID {get;set;} = "";
    [MaxLength(128)] public string PERMIT_APPLICATION_ID {get;set;} = "";
    [MaxLength(64)] public string SOURCE_REVISION {get;set;} = "";
    [MaxLength(8388608)] public string OUTCOME_JSON {get;set;} = "";
    [MaxLength(64)] public string OUTCOME_HASH {get;set;} = "";
    public static PERMIT_APPLICATION_COMMAND Create(PermitApplicationCommandOutcome outcome)
    {
        outcome.Require(); var original = outcome.Original; var context = original.Review.Context;
        var json = JsonSerializer.Serialize(outcome);
        return new() {PERMIT_APPLICATION_COMMAND_ID = original.IntentId.ToString("N"), OWNER_TENANT_ID = context.Field.Company.TenantId,
            OWNER_COMPANY_ID = context.Field.Company.BusinessAssociateId, FIELD_ID = context.Field.FieldId!, PERMIT_APPLICATION_ID = original.Review.ReadApplication().PERMIT_APPLICATION_ID,
            SOURCE_REVISION = original.Review.Revision, OUTCOME_JSON = json, OUTCOME_HASH = ProcessDefinitionRevision.FromJson(json), ACTIVE_IND = "Y",
            PPDM_GUID = original.IntentId.ToString(), ROW_EFFECTIVE_DATE = outcome.RecordedAtUtc.UtcDateTime,
            ROW_CREATED_BY = context.Field.ActorUserId, ROW_CHANGED_BY = context.Field.ActorUserId, ROW_CREATED_DATE = outcome.RecordedAtUtc.UtcDateTime, ROW_CHANGED_DATE = outcome.RecordedAtUtc.UtcDateTime};
    }
    public PermitApplicationCommandOutcome Restore(PermitApplicationDataScope context)
    {
        context.Require();
        if (ACTIVE_IND != "Y" || OUTCOME_JSON is not {Length: > 0 and <= 8388608} || OUTCOME_HASH != ProcessDefinitionRevision.FromJson(OUTCOME_JSON))
            throw new InvalidOperationException("The recorded permit command is incomplete or changed.");
        var outcome = JsonSerializer.Deserialize<PermitApplicationCommandOutcome>(OUTCOME_JSON) ?? throw new InvalidOperationException("The permit command outcome is unavailable.");
        outcome.Require(); var original = outcome.Original;
        if (original.Review.Context != context || PERMIT_APPLICATION_COMMAND_ID != original.IntentId.ToString("N") || OWNER_TENANT_ID != context.Field.Company.TenantId ||
            OWNER_COMPANY_ID != context.Field.Company.BusinessAssociateId || FIELD_ID != context.Field.FieldId || PERMIT_APPLICATION_ID != original.Review.ReadApplication().PERMIT_APPLICATION_ID ||
            SOURCE_REVISION != original.Review.Revision || ROW_CREATED_BY != context.Field.ActorUserId || ROW_CHANGED_BY != context.Field.ActorUserId ||
            ROW_CREATED_DATE is not { } created || created < outcome.RecordedAtUtc.UtcDateTime || ROW_CHANGED_DATE is not { } changed || changed < created)
            throw new InvalidOperationException("The retained permit command belongs to another original, context or audit.");
        return outcome;
    }
}
