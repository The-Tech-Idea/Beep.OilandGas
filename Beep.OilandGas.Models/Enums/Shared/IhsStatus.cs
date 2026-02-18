using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum IhsStatus
    {
        [Description("Location / Permit (LOC)")] Location,
        [Description("Cancelled / Voided (CNCL)")] Cancelled,
        [Description("Drilling (DNG)")] Drilling,
        [Description("Spudded (SOW)")] Spudded,
        [Description("Suspended / Wait on Compl (SUSP)")] Suspended,
        [Description("Oil Producer (OIL)")] OilProducer,
        [Description("Gas Producer (GAS)")] GasProducer,
        [Description("Oil & Gas Producer (OG)")] OilAndGasProducer,
        [Description("Coalbed Methane (CBM)")] CoalbedMethane,
        [Description("Condensate (CON)")] Condensate,
        [Description("Bitumen / Oil Sands (BIT)")] Bitumen,
        [Description("Water Injection (WIW)")] WaterInjection,
        [Description("Gas Injection (GIW)")] GasInjection,
        [Description("Salt Water Disposal (SWD)")] SaltWaterDisposal,
        [Description("Acid Gas Injection (AGI)")] AcidGasInjection,
        [Description("Steam Injection (STM)")] SteamInjection,
        [Description("Water Supply / Source (WS)")] WaterSupply,
        [Description("Observation / Monitor (OBS)")] Observation,
        [Description("Stratigraphic Test (STRAT)")] StratigraphicTest,
        [Description("Shut-In Oil (SI)")] ShutInOil,
        [Description("Shut-In Gas (SIG)")] ShutInGas,
        [Description("Temporarily Abandoned (TA)")] TemporarilyAbandoned,
        [Description("Plugged & Abandoned (ABD)")] PluggedAndAbandoned,
        [Description("Dry Hole (DRY)")] DryHole,
        [Description("Junked & Abandoned (JNK)")] Junked,
        [Description("Service Well (SVC)")] ServiceWell,
        [Description("Core Hole (CORE)")] CoreHole,
        [Description("Unknown")] Unknown
    }
}
