using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
namespace TheTechIdea.Data.OilGas;

/// <summary>Explicit supporting proposal; parent identity, filing state, links and audit are not editable.</summary>
public sealed record PermitInjectionDraftChanges(
    [property: JsonRequired] string InjectionType,
    [property: JsonRequired] string? InjectionZone,
    [property: JsonRequired] string InjectionFluid,
    [property: JsonRequired] decimal? MaximumPressure,
    [property: JsonRequired] string? PressureUnit,
    [property: JsonRequired] decimal? MaximumRate,
    [property: JsonRequired] string? RateUnit,
    [property: JsonRequired] string? MonitoringRequirements,
    [property: JsonRequired] string? Co2Storage,
    [property: JsonRequired] string? GasStorage,
    [property: JsonRequired] string? BrineMining,
    [property: JsonRequired] string? Remarks)
{
    public static PermitInjectionDraftChanges From(INJECTION_PERMIT_APPLICATION row)=>new(row.INJECTION_TYPE??"",row.INJECTION_ZONE,row.INJECTION_FLUID??"",row.MAXIMUM_INJECTION_PRESSURE,row.INJECTION_PRESSURE_UNIT,row.MAXIMUM_INJECTION_RATE,row.INJECTION_RATE_UNIT,row.MONITORING_REQUIREMENTS,row.IS_CO2_STORAGE_IND,row.IS_GAS_STORAGE_IND,row.IS_BRINE_MINING_IND,row.REMARKS);
    public void Require(PermitApplicationSourceReview review)
    {
        PermitSupportingDraftRules.Stage(review,PermitApplicationType.Injection,true);
        PermitSupportingDraftRules.Named(InjectionType,InjectionFluid);
        PermitSupportingDraftRules.Text(128,false,InjectionZone);
        PermitSupportingDraftRules.Text(4096,true,MonitoringRequirements,Remarks);
        PermitSupportingDraftRules.Flags(Co2Storage,GasStorage,BrineMining);
        PermitSupportingDraftRules.Measurement(MaximumPressure,PressureUnit,false);
        PermitSupportingDraftRules.Measurement(MaximumRate,RateUnit,false);
        if(review.Supporting.Injection is {} before && From(before)==this)throw new InvalidOperationException("The proposed injection details are unchanged.");
    }
    public void Apply(INJECTION_PERMIT_APPLICATION row)
    {
        row.INJECTION_TYPE=InjectionType;
        row.INJECTION_ZONE=InjectionZone;
        row.INJECTION_FLUID=InjectionFluid;
        row.MAXIMUM_INJECTION_PRESSURE=MaximumPressure;
        row.INJECTION_PRESSURE_UNIT=PressureUnit;
        row.MAXIMUM_INJECTION_RATE=MaximumRate;
        row.INJECTION_RATE_UNIT=RateUnit;
        row.MONITORING_REQUIREMENTS=MonitoringRequirements;
        row.IS_CO2_STORAGE_IND=Co2Storage;
        row.IS_GAS_STORAGE_IND=GasStorage;
        row.IS_BRINE_MINING_IND=BrineMining;
        row.REMARKS=Remarks;
    }
}
