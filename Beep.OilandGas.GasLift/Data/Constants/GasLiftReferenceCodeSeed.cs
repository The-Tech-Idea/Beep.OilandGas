using System.Collections.Generic;
using System.Globalization;

namespace Beep.OilandGas.GasLift.Constants;

/// <summary>
/// Canonical seed rows for <c>R_GAS_LIFT_REFERENCE_CODE</c>. Keep aligned with <see cref="GasLiftConstants"/> and calculators.
/// </summary>
public static class GasLiftReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        foreach (var inches in GasLiftConstants.StandardPortSizes)
        {
            var code = inches.ToString("0.###", CultureInfo.InvariantCulture);
            yield return new SeedRow(GasLiftReferenceSets.PortSizeIn, code, $"Port diameter {code} in", "Y");
        }

        yield return new SeedRow(GasLiftReferenceSets.OperatingMode, "CONTINUOUS", "Continuous gas lift");
        yield return new SeedRow(GasLiftReferenceSets.OperatingMode, "INTERMITTENT", "Intermittent / cyclic gas lift");
        yield return new SeedRow(GasLiftReferenceSets.OperatingMode, "PLUNGER_COMPATIBLE", "Plunger-lift compatible gas assist");

        yield return new SeedRow(GasLiftReferenceSets.DesignMethod, "UNIFORM_SPACING", "Equal depth spacing between valves");
        yield return new SeedRow(GasLiftReferenceSets.DesignMethod, "EQUAL_PRESSURE_DROP", "Equal pressure drop per stage");
        yield return new SeedRow(GasLiftReferenceSets.DesignMethod, "GRADIENT_BASED", "Gradient-based spacing (profile-driven)");

        yield return new SeedRow(GasLiftReferenceSets.ValveService, "UNLOADING", "Unloading valve(s)");
        yield return new SeedRow(GasLiftReferenceSets.ValveService, "INTERMEDIATE", "Intermediate valve(s)");
        yield return new SeedRow(GasLiftReferenceSets.ValveService, "OPERATING", "Operating / bottom valve");

        yield return new SeedRow(GasLiftReferenceSets.InjectionGasSource, "FACILITY_MANIFOLD", "Central facility / manifold gas");
        yield return new SeedRow(GasLiftReferenceSets.InjectionGasSource, "DEDICATED_COMPRESSOR", "Dedicated compressor discharge");
        yield return new SeedRow(GasLiftReferenceSets.InjectionGasSource, "WELLHEAD_RECYCLE", "Wellhead / separator recycle gas");

        yield return new SeedRow(GasLiftReferenceSets.DesignLimit, "MIN_INJECTION_MSCFD", "Minimum injection rate (Mscf/d)", "Y");
        yield return new SeedRow(GasLiftReferenceSets.DesignLimit, "MAX_INJECTION_MSCFD", "Maximum injection rate (Mscf/d)", "Y");
        yield return new SeedRow(GasLiftReferenceSets.DesignLimit, "MIN_VALVES", "Minimum number of unloading/operating valves", "Y");
        yield return new SeedRow(GasLiftReferenceSets.DesignLimit, "MAX_VALVES", "Maximum number of unloading/operating valves", "Y");
        yield return new SeedRow(GasLiftReferenceSets.DesignLimit, "MIN_VALVE_DEPTH_FT", "Minimum valve depth (ft)", "Y");
        yield return new SeedRow(GasLiftReferenceSets.DesignLimit, "MIN_VALVE_SPACING_FT", "Minimum valve spacing (ft)", "Y");
    }
}
