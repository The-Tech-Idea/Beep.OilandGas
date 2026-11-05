using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>Exact confirmed original; source, operator and persistence still require native qualification.</summary>
public sealed record DevelopmentFacilityCreation(
    [property:JsonRequired] DevelopmentFacilityCreationReview Review,
    [property:JsonRequired] Guid IntentId,
    [property:JsonRequired] Guid FacilityGuid,
    [property:JsonRequired] Guid FieldLinkGuid,
    [property:JsonRequired] bool Confirmed)
{
    public void Require()
    {
        if(Review is null)throw new InvalidOperationException("The original source review is required.");Review.Require();
        if(IntentId==Guid.Empty || FacilityGuid==Guid.Empty || FieldLinkGuid==Guid.Empty || IntentId==FacilityGuid || IntentId==FieldLinkGuid || FacilityGuid==FieldLinkGuid ||
           !Confirmed || JsonSerializer.SerializeToUtf8Bytes(this).Length>262144)
            throw new InvalidOperationException("Distinct original identifiers, an explicit operator and confirmation are required.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public (FACILITY Facility,FACILITY_FIELD Link) Expected(DateTime recordedUtc)
    {
        Require();if(recordedUtc==default || recordedUtc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("An explicit UTC creation audit is required.");
        var draft=Review.Draft;var actor=draft.Context.ActorUserId;
        return (new FACILITY{FACILITY_ID=draft.FacilityId,FACILITY_TYPE=draft.FacilityType,PRIMARY_FIELD_ID=draft.Context.FieldId!,CURRENT_OPERATOR=draft.OperatorBusinessAssociateId,
            FACILITY_LONG_NAME=draft.LongName,DESCRIPTION=draft.Description!,ACTIVE_IND="Y",PPDM_GUID=FacilityGuid.ToString("D").ToUpperInvariant(),ROW_CREATED_BY=actor,ROW_CHANGED_BY=actor,ROW_CREATED_DATE=recordedUtc,ROW_CHANGED_DATE=recordedUtc},
            new FACILITY_FIELD{FACILITY_ID=draft.FacilityId,FACILITY_TYPE=draft.FacilityType,FIELD_ID=draft.Context.FieldId!,ACTIVE_IND="Y",PPDM_GUID=FieldLinkGuid.ToString("D").ToUpperInvariant(),ROW_CREATED_BY=actor,ROW_CHANGED_BY=actor,ROW_CREATED_DATE=recordedUtc,ROW_CHANGED_DATE=recordedUtc});
    }
}
