namespace Beep.OilandGas.Models.Constants;

/// <summary>
/// Drilling reference values — canonical codes for drilling operations and completions.
/// </summary>
public static class DrillingReferenceCodes
{
    public static class RigType
    {
        public const string Land = "LAND";
        public const string Jackup = "JACKUP";
        public const string SemiSub = "SEMI_SUBMERSIBLE";
        public const string Drillship = "DRILLSHIP";
        public const string Platform = "PLATFORM";
        public const string CoiledTubing = "COILED_TUBING";
        public static readonly string[] All = { Land, Jackup, SemiSub, Drillship, Platform, CoiledTubing };
    }

    public static class DrillingPhase
    {
        public const string Spud = "SPUD";
        public const string Surface = "SURFACE";
        public const string Intermediate = "INTERMEDIATE";
        public const string Production = "PRODUCTION_HOLE";
        public const string Liner = "LINER";
        public const string Sidetrack = "SIDETRACK";
        public static readonly string[] All = { Spud, Surface, Intermediate, Production, Liner, Sidetrack };
    }

    public static class MudType
    {
        public const string WaterBased = "WATER_BASED";
        public const string OilBased = "OIL_BASED";
        public const string Synthetic = "SYNTHETIC";
        public const string AirFoam = "AIR_FOAM";
        public const string Brine = "BRINE";
        public static readonly string[] All = { WaterBased, OilBased, Synthetic, AirFoam, Brine };
    }
}
