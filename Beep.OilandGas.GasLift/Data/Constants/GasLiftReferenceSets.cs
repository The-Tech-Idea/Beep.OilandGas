namespace Beep.OilandGas.GasLift.Constants;

/// <summary>
/// <c>REFERENCE_SET</c> values for <c>R_GAS_LIFT_REFERENCE_CODE</c>.
/// </summary>
public static class GasLiftReferenceSets
{
    /// <summary>Standard GLV port diameters (inches) — aligns with <see cref="GasLiftConstants.StandardPortSizes"/>.</summary>
    public const string PortSizeIn = "GAS_LIFT_PORT_SIZE_IN";

    /// <summary>Surface / well operating mode for gas lift.</summary>
    public const string OperatingMode = "GAS_LIFT_OPERATING_MODE";

    /// <summary>Valve spacing / design policy labels for UI and reporting.</summary>
    public const string DesignMethod = "GAS_LIFT_DESIGN_METHOD";

    /// <summary>Valve role in unloading / intermediate / operating string.</summary>
    public const string ValveService = "GAS_LIFT_VALVE_SERVICE";

    /// <summary>Source of injection gas (facility, compressor, etc.).</summary>
    public const string InjectionGasSource = "GAS_LIFT_INJECTION_SOURCE";

    /// <summary>Numeric guardrails aligned with <see cref="GasLiftConstants"/> (LOV for picklists / validation messages).</summary>
    public const string DesignLimit = "GAS_LIFT_DESIGN_LIMIT";
}
