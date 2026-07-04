namespace Beep.OilandGas.Models.Constants;

/// <summary>
/// Facility & Pipeline reference values — canonical codes for surface facilities and pipeline infrastructure.
/// </summary>
public static class FacilityReferenceCodes
{
    public static class FacilityType
    {
        public const string Wellhead = "WELLHEAD";
        public const string Gathering = "GATHERING";
        public const string Separation = "SEPARATION";
        public const string Compression = "COMPRESSION";
        public const string Processing = "PROCESSING";
        public const string Storage = "STORAGE";
        public const string Export = "EXPORT";
        public const string Injection = "INJECTION";
        public static readonly string[] All = { Wellhead, Gathering, Separation, Compression, Processing, Storage, Export, Injection };
    }

    public static class PipelineType
    {
        public const string Flowline = "FLOWLINE";
        public const string Gathering = "GATHERING_LINE";
        public const string Trunkline = "TRUNKLINE";
        public const string Export = "EXPORT_LINE";
        public const string Injection = "INJECTION_LINE";
        public const string Disposal = "DISPOSAL_LINE";
        public static readonly string[] All = { Flowline, Gathering, Trunkline, Export, Injection, Disposal };
    }

    public static class InspectionMethod
    {
        public const string Visual = "VISUAL";
        public const string Ultrasonic = "ULTRASONIC";
        public const string Radiography = "RADIOGRAPHY";
        public const string MagneticParticle = "MAGNETIC_PARTICLE";
        public const string Hydrotest = "HYDROTEST";
        public const string ILI = "ILI";
        public const string CIPS = "CIPS";
        public static readonly string[] All = { Visual, Ultrasonic, Radiography, MagneticParticle, Hydrotest, ILI, CIPS };
    }
}
