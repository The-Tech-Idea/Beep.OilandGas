namespace Beep.OilandGas.Models.Constants;

/// <summary>
/// Production reference values — canonical codes for production operations and measurement.
/// </summary>
public static class ProductionReferenceCodes
{
    public static class ProductionMethod
    {
        public const string NaturalFlow = "NATURAL_FLOW";
        public const string GasLift = "GAS_LIFT";
        public const string ESP = "ESP";
        public const string RodPump = "ROD_PUMP";
        public const string PCP = "PCP";
        public const string PlungerLift = "PLUNGER_LIFT";
        public const string HydraulicPump = "HYDRAULIC_PUMP";
        public const string JetPump = "JET_PUMP";
        public static readonly string[] All = { NaturalFlow, GasLift, ESP, RodPump, PCP, PlungerLift, HydraulicPump, JetPump };
    }

    public static class FluidType
    {
        public const string Oil = "OIL";
        public const string Gas = "GAS";
        public const string Water = "WATER";
        public const string Condensate = "CONDENSATE";
        public const string NGL = "NGL";
        public const string CO2 = "CO2";
        public static readonly string[] All = { Oil, Gas, Water, Condensate, NGL, CO2 };
    }

    public static class MeasurementMethod
    {
        public const string TankGauge = "TANK_GAUGE";
        public const string LACT = "LACT";
        public const string Coriolis = "CORIOLIS";
        public const string Orifice = "ORIFICE";
        public const string Turbine = "TURBINE";
        public const string Ultrasonic = "ULTRASONIC";
        public const string VCone = "V_CONE";
        public static readonly string[] All = { TankGauge, LACT, Coriolis, Orifice, Turbine, Ultrasonic, VCone };
    }

    public static class AllocationMethod
    {
        public const string ByWellTest = "BY_WELL_TEST";
        public const string ByChokeModel = "BY_CHOKE_MODEL";
        public const string ByMeter = "BY_METER";
        public const string ProRata = "PRO_RATA";
        public const string Theoretical = "THEORETICAL";
        public static readonly string[] All = { ByWellTest, ByChokeModel, ByMeter, ProRata, Theoretical };
    }
}
