using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
namespace TheTechIdea.Data.OilGas;

/// <summary>One supporting amendment projection for the existing reviewed command owner.</summary>
public static class PermitSupportingAmendments
{
    public static Type? EntityType(string action)=>action switch {
        "AMEND_DRILLING"=>typeof(DRILLING_PERMIT_APPLICATION),"AMEND_ENVIRONMENTAL"=>typeof(ENVIRONMENTAL_PERMIT_APPLICATION),
        "AMEND_INJECTION"=>typeof(INJECTION_PERMIT_APPLICATION),_=>null };
    public static ModelEntityBase? Read(PermitApplicationSupportingSnapshot snapshot,string action)=>action switch {
        "AMEND_DRILLING"=>snapshot.Drilling,"AMEND_ENVIRONMENTAL"=>snapshot.Environmental,"AMEND_INJECTION"=>snapshot.Injection,
        _=>throw new InvalidOperationException("An explicit supporting amendment action is required.") };
    public static PermitApplicationSupportingSnapshot Replace(PermitApplicationSupportingSnapshot snapshot,ModelEntityBase row)=>row switch {
        DRILLING_PERMIT_APPLICATION d=>snapshot with { DrillingJson=JsonSerializer.Serialize(d) },
        ENVIRONMENTAL_PERMIT_APPLICATION e=>snapshot with { EnvironmentalJson=JsonSerializer.Serialize(e) },
        INJECTION_PERMIT_APPLICATION i=>snapshot with { InjectionJson=JsonSerializer.Serialize(i) },
        _=>throw new InvalidOperationException("An exact canonical supporting row is required.") };
    public static PermitApplicationSupportingSnapshot Prepare(PermitApplicationCommand original,DateTimeOffset recorded)
    {
        var snapshot=original.Review.Supporting;var before=Read(snapshot,original.Action);
        var id=original.Review.ReadApplication().PERMIT_APPLICATION_ID;var actor=original.Review.Context.Field.ActorUserId;
        ModelEntityBase child=before ?? (original.Action switch {
            "AMEND_DRILLING"=>new DRILLING_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=id },
            "AMEND_ENVIRONMENTAL"=>new ENVIRONMENTAL_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=id },
            "AMEND_INJECTION"=>new INJECTION_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=id },
            _=>throw new InvalidOperationException("An exact supporting action is required.") });
        if(before is null)
        {
            child.ACTIVE_IND="Y";child.PPDM_GUID=original.IntentId.ToString();child.ROW_EFFECTIVE_DATE=recorded.UtcDateTime;
            child.ROW_CREATED_BY=actor;child.ROW_CREATED_DATE=recorded.UtcDateTime;
        }
        switch(child)
        {
            case DRILLING_PERMIT_APPLICATION d: original.DrillingChanges!.Apply(d);break;
            case ENVIRONMENTAL_PERMIT_APPLICATION e: original.EnvironmentalChanges!.Apply(e);break;
            case INJECTION_PERMIT_APPLICATION i: original.InjectionChanges!.Apply(i);break;
        }
        child.ROW_CHANGED_BY=actor;child.ROW_CHANGED_DATE=recorded.UtcDateTime;
        return Replace(snapshot,child);
    }
}
