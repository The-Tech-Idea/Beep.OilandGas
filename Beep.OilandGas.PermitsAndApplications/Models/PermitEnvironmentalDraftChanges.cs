using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
namespace Beep.OilandGas.PermitsAndApplications.Models;

/// <summary>Explicit supporting proposal; parent identity, filing state, links and audit are not editable.</summary>
public sealed record PermitEnvironmentalDraftChanges(
    [property: JsonRequired] string PermitType,
    [property: JsonRequired] string? WasteType,
    [property: JsonRequired] decimal? WasteVolume,
    [property: JsonRequired] string? WasteVolumeUnit,
    [property: JsonRequired] string? FacilityLocation,
    [property: JsonRequired] string? MonitoringPlan,
    [property: JsonRequired] string? EnvironmentalImpact,
    [property: JsonRequired] string? MitigationPlan,
    [property: JsonRequired] string? DisposalMethod,
    [property: JsonRequired] string? WaterSource,
    [property: JsonRequired] string? NormInvolved,
    [property: JsonRequired] string? Remarks)
{
    public static PermitEnvironmentalDraftChanges From(ENVIRONMENTAL_PERMIT_APPLICATION row)=>new(row.ENVIRONMENTAL_PERMIT_TYPE??"",row.WASTE_TYPE,row.WASTE_VOLUME,row.WASTE_VOLUME_UNIT,row.FACILITY_LOCATION,row.MONITORING_PLAN,row.ENVIRONMENTAL_IMPACT,row.MITIGATION_PLAN,row.DISPOSAL_METHOD,row.WATER_SOURCE,row.NORM_INVOLVED_IND,row.REMARKS);
    public void Require(PermitApplicationSourceReview review)
    {
        PermitSupportingDraftRules.Stage(review,PermitApplicationType.Environmental,false);
        PermitSupportingDraftRules.Named(PermitType);
        PermitSupportingDraftRules.Text(128,false,WasteType,FacilityLocation,DisposalMethod,WaterSource);
        PermitSupportingDraftRules.Text(4096,true,MonitoringPlan,EnvironmentalImpact,MitigationPlan,Remarks);
        PermitSupportingDraftRules.Flags(NormInvolved);
        PermitSupportingDraftRules.Measurement(WasteVolume,WasteVolumeUnit,true);
        if(review.Supporting.Environmental is {} before && From(before)==this)throw new InvalidOperationException("The proposed environmental details are unchanged.");
    }
    public void Apply(ENVIRONMENTAL_PERMIT_APPLICATION row)
    {
        row.ENVIRONMENTAL_PERMIT_TYPE=PermitType;
        row.WASTE_TYPE=WasteType;
        row.WASTE_VOLUME=WasteVolume;
        row.WASTE_VOLUME_UNIT=WasteVolumeUnit;
        row.FACILITY_LOCATION=FacilityLocation;
        row.MONITORING_PLAN=MonitoringPlan;
        row.ENVIRONMENTAL_IMPACT=EnvironmentalImpact;
        row.MITIGATION_PLAN=MitigationPlan;
        row.DISPOSAL_METHOD=DisposalMethod;
        row.WATER_SOURCE=WaterSource;
        row.NORM_INVOLVED_IND=NormInvolved;
        row.REMARKS=Remarks;
    }
}
