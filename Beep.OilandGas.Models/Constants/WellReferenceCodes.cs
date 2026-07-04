namespace Beep.OilandGas.Models.Constants;

/// <summary>
/// Well reference values — canonical codes for well lifecycle, status, type, and completion.
/// Mirrors PPDM 3.9 reference data. Seeded into R_* tables at setup.
/// </summary>
public static class WellReferenceCodes
{
    public static class WellStatus
    {
        public const string Planned = "PLANNED";
        public const string Drilling = "DRILLING";
        public const string Completing = "COMPLETING";
        public const string Producing = "PRODUCING";
        public const string ShutIn = "SHUT_IN";
        public const string Workover = "WORKOVER";
        public const string Suspended = "SUSPENDED";
        public const string Abandoned = "ABANDONED";
        public const string Plugged = "PLUGGED";
        public static readonly string[] All = { Planned, Drilling, Completing, Producing, ShutIn, Workover, Suspended, Abandoned, Plugged };
    }

    public static class WellType
    {
        public const string OilProducer = "OIL_PRODUCER";
        public const string GasProducer = "GAS_PRODUCER";
        public const string WaterInjector = "WATER_INJECTOR";
        public const string GasInjector = "GAS_INJECTOR";
        public const string Disposal = "DISPOSAL";
        public const string Observation = "OBSERVATION";
        public const string Exploratory = "EXPLORATORY";
        public const string Development = "DEVELOPMENT";
        public static readonly string[] All = { OilProducer, GasProducer, WaterInjector, GasInjector, Disposal, Observation, Exploratory, Development };
    }

    public static class CompletionType
    {
        public const string OpenHole = "OPEN_HOLE";
        public const string CasedHole = "CASED_HOLE";
        public const string Perforated = "PERFORATED";
        public const string SlottedLiner = "SLOTTED_LINER";
        public const string GravelPack = "GRAVEL_PACK";
        public const string FracPack = "FRAC_PACK";
        public const string Multizone = "MULTIZONE";
        public const string Horizontal = "HORIZONTAL";
        public static readonly string[] All = { OpenHole, CasedHole, Perforated, SlottedLiner, GravelPack, FracPack, Multizone, Horizontal };
    }
}
