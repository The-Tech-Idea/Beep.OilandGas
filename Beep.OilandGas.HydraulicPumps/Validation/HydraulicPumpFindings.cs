using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Beep.OilandGas.HydraulicPumps.Constants;
using Beep.OilandGas.Models.Data.HydraulicPumps;

namespace Beep.OilandGas.HydraulicPumps.Validation;

/// <summary>How serious a hydraulic pump finding is.</summary>
public enum HydraulicPumpFindingSeverity
{
    /// <summary>A note about the run; nothing is wrong.</summary>
    Info,

    /// <summary>The design works but a condition needs attention (cavitation, a clamped efficiency).</summary>
    Warning,

    /// <summary>The input is not usable: <see cref="HydraulicPumpValidator"/> would refuse it.</summary>
    Error,
}

/// <summary>One finding about a hydraulic pump design: its code, the sentence and its severity.</summary>
/// <param name="Code">Stable code, e.g. <c>NOZZLE_NOT_LESS_THAN_THROAT</c>.</param>
/// <param name="Message">The sentence, which is the validator's own for a refused input.</param>
/// <param name="Severity">How serious it is.</param>
public sealed record HydraulicPumpFinding(
    string Code,
    string Message,
    HydraulicPumpFindingSeverity Severity);

/// <summary>The cavitation index: available NPSH against the required NPSH, and the margin between them.</summary>
/// <param name="AvailablePsi">Suction pressure less vapour pressure.</param>
/// <param name="RequiredPsi">The NPSH the pump needs.</param>
/// <param name="MarginPsi">Available less required; negative means cavitation is predicted.</param>
/// <param name="Risk">True when the margin is negative.</param>
public sealed record HydraulicPumpCavitationIndex(
    decimal AvailablePsi,
    decimal RequiredPsi,
    decimal MarginPsi,
    bool Risk);

/// <summary>
/// Evaluates the hydraulic pump guardrails and reports every finding with a severity (LFT-06 AC-3, AC-4).
/// </summary>
/// <remarks>
/// The sentences for a refused input are the ones <see cref="HydraulicPumpValidator"/> throws, in the same order, so the
/// screen lists them instead of writing its own. The validator itself is unchanged and still throws for a caller that
/// wants to stop; this type is the list form a screen needs. Two conditions are warnings rather than refusals because the
/// engine answers them by clamping: a system efficiency outside [0, 1], and a pump efficiency under the module's minimum.
/// Cavitation is a warning too - it is a condition of the well, not an input error.
/// </remarks>
public static class HydraulicPumpFindings
{
    /// <summary>
    /// The NPSH a hydraulic pump is assumed to need, in psi - the value <c>HydraulicPumpService.Advanced</c> already uses
    /// when it screens for cavitation. It is stated once here so the index and the warning cannot disagree.
    /// </summary>
    public const decimal DefaultRequiredNpshPsi = 50m;

    /// <summary>The cavitation index for one suction condition (LFT-06 AC-2).</summary>
    public static HydraulicPumpCavitationIndex Cavitation(
        decimal suctionPressure,
        decimal vaporPressure,
        decimal requiredNpshPsi = DefaultRequiredNpshPsi)
    {
        var available = suctionPressure - vaporPressure;
        var margin = available - requiredNpshPsi;

        return new HydraulicPumpCavitationIndex(available, requiredNpshPsi, margin, margin < 0m);
    }

    /// <summary>Every finding for the well inputs alone.</summary>
    public static IReadOnlyList<HydraulicPumpFinding> EvaluateWell(HYDRAULIC_PUMP_WELL_PROPERTIES well)
    {
        ArgumentNullException.ThrowIfNull(well);

        var findings = new List<HydraulicPumpFinding>();

        void Error(string code, bool violated, string message)
        {
            if (violated)
            {
                findings.Add(new HydraulicPumpFinding(code, message, HydraulicPumpFindingSeverity.Error));
            }
        }

        Error("WELL_DEPTH_INVALID", well.WELL_DEPTH <= 0,
            "Well depth must be greater than zero.");

        Error("TUBING_DIAMETER_INVALID", well.TUBING_DIAMETER <= 0,
            "Tubing diameter must be greater than zero.");

        Error("CASING_DIAMETER_INVALID", well.CASING_DIAMETER <= 0,
            "Casing diameter must be greater than zero.");

        Error("OIL_GRAVITY_OUT_OF_RANGE", well.OIL_GRAVITY < 4m || well.OIL_GRAVITY > 75m,
            "Oil gravity (API) must be between 4 and 75 for screening correlations.");

        Error("GAS_OIL_RATIO_NEGATIVE", well.GAS_OIL_RATIO < 0,
            "Gas-oil ratio cannot be negative.");

        Error("TEMPERATURE_INVALID", well.WELLHEAD_TEMPERATURE <= 0m || well.BOTTOM_HOLE_TEMPERATURE <= 0m,
            "Wellhead and bottom-hole temperatures must be greater than zero (Rankine).");

        Error("WELLHEAD_PRESSURE_NEGATIVE", well.WELLHEAD_PRESSURE < 0,
            "Wellhead pressure cannot be negative.");

        Error("BOTTOM_HOLE_PRESSURE_INVALID", well.BOTTOM_HOLE_PRESSURE <= well.WELLHEAD_PRESSURE,
            "Bottom hole pressure must be greater than wellhead pressure.");

        Error("WATER_CUT_OUT_OF_RANGE", well.WATER_CUT < 0 || well.WATER_CUT > 1,
            "Water cut must be between 0 and 1.");

        Error("GAS_SPECIFIC_GRAVITY_INVALID", well.GAS_SPECIFIC_GRAVITY <= 0,
            "Gas specific gravity must be greater than zero.");

        Error("DESIRED_RATE_INVALID", well.DESIRED_PRODUCTION_RATE <= 0,
            "Desired production rate must be greater than zero.");

        return findings;
    }

    /// <summary>Every finding for a jet pump design, including the clamped-efficiency and cavitation warnings.</summary>
    public static IReadOnlyList<HydraulicPumpFinding> EvaluateJet(
        HYDRAULIC_PUMP_WELL_PROPERTIES well,
        HYDRAULIC_JET_PUMP_PROPERTIES pump,
        HYDRAULIC_JET_PUMP_RESULT result)
    {
        ArgumentNullException.ThrowIfNull(well);
        ArgumentNullException.ThrowIfNull(pump);
        ArgumentNullException.ThrowIfNull(result);

        var findings = new List<HydraulicPumpFinding>(EvaluateWell(well));

        void Error(string code, bool violated, string message)
        {
            if (violated)
            {
                findings.Add(new HydraulicPumpFinding(code, message, HydraulicPumpFindingSeverity.Error));
            }
        }

        Error("NOZZLE_DIAMETER_INVALID", pump.NOZZLE_DIAMETER <= 0,
            "Nozzle diameter must be greater than zero.");

        Error("THROAT_DIAMETER_INVALID", pump.THROAT_DIAMETER <= 0,
            "Throat diameter must be greater than zero.");

        // The state table's "invalid combination of nozzle/throat": refused by the validator's own rule.
        Error("NOZZLE_NOT_LESS_THAN_THROAT", pump.NOZZLE_DIAMETER >= pump.THROAT_DIAMETER,
            "Nozzle diameter must be less than throat diameter.");

        Error("POWER_FLUID_PRESSURE_INVALID", pump.POWER_FLUID_PRESSURE <= 0,
            "Power fluid pressure must be greater than zero.");

        Error("POWER_FLUID_RATE_INVALID", pump.POWER_FLUID_RATE <= 0,
            "Power fluid rate must be greater than zero.");

        Error("POWER_FLUID_SG_INVALID", pump.POWER_FLUID_SPECIFIC_GRAVITY <= 0,
            "Power fluid specific gravity must be greater than zero.");

        AddScreening(findings, well, result.PUMP_INTAKE_PRESSURE, result.POWER_FLUID_HORSEPOWER,
            result.HYDRAULIC_HORSEPOWER, result.SYSTEM_EFFICIENCY, result.PUMP_EFFICIENCY,
            HydraulicPumpConstants.MinimumJetPumpEfficiency);

        return findings;
    }

    /// <summary>Every finding for a piston pump design, including the clamped-efficiency and cavitation warnings.</summary>
    public static IReadOnlyList<HydraulicPumpFinding> EvaluatePiston(
        HYDRAULIC_PUMP_WELL_PROPERTIES well,
        HYDRAULIC_PISTON_PUMP_PROPERTIES pump,
        HYDRAULIC_PISTON_PUMP_RESULT result)
    {
        ArgumentNullException.ThrowIfNull(well);
        ArgumentNullException.ThrowIfNull(pump);
        ArgumentNullException.ThrowIfNull(result);

        var findings = new List<HydraulicPumpFinding>(EvaluateWell(well));

        void Error(string code, bool violated, string message)
        {
            if (violated)
            {
                findings.Add(new HydraulicPumpFinding(code, message, HydraulicPumpFindingSeverity.Error));
            }
        }

        Error("PISTON_DIAMETER_INVALID", pump.PISTON_DIAMETER <= 0,
            "Piston diameter must be greater than zero.");

        Error("ROD_NOT_LESS_THAN_PISTON", pump.ROD_DIAMETER >= pump.PISTON_DIAMETER,
            "Rod diameter must be less than piston diameter.");

        Error("STROKE_LENGTH_INVALID", pump.STROKE_LENGTH <= 0,
            "Stroke length must be greater than zero.");

        Error("STROKES_PER_MINUTE_INVALID", pump.STROKES_PER_MINUTE <= 0,
            "Strokes per minute must be greater than zero.");

        Error("POWER_FLUID_PRESSURE_INVALID", pump.POWER_FLUID_PRESSURE <= 0,
            "Power fluid pressure must be greater than zero.");

        Error("POWER_FLUID_RATE_INVALID", pump.POWER_FLUID_RATE <= 0,
            "Power fluid rate must be greater than zero.");

        Error("POWER_FLUID_SG_INVALID", pump.POWER_FLUID_SPECIFIC_GRAVITY <= 0,
            "Power fluid specific gravity must be greater than zero.");

        AddScreening(findings, well, result.PUMP_INTAKE_PRESSURE, result.POWER_FLUID_HORSEPOWER,
            result.HYDRAULIC_HORSEPOWER, result.SYSTEM_EFFICIENCY, result.VOLUMETRIC_EFFICIENCY,
            HydraulicPumpConstants.MinimumVolumetricEfficiency);

        return findings;
    }

    /// <summary>
    /// The two screening conditions that are warnings rather than refusals, plus the cavitation index.
    /// </summary>
    /// <remarks>
    /// AC-3: the engine clamps the system efficiency into [0, 1], so a raw ratio outside it would otherwise be presented
    /// as if the clamp were the answer. The raw ratio is the same division the calculators make
    /// (<c>HYDRAULIC_HORSEPOWER / POWER_FLUID_HORSEPOWER</c>), so the warning can be raised without changing them.
    /// </remarks>
    private static void AddScreening(
        List<HydraulicPumpFinding> findings,
        HYDRAULIC_PUMP_WELL_PROPERTIES well,
        decimal intakePressure,
        decimal powerFluidHorsepower,
        decimal hydraulicHorsepower,
        decimal systemEfficiency,
        decimal pumpEfficiency,
        decimal minimumPumpEfficiency)
    {
        if (powerFluidHorsepower > 0m)
        {
            var raw = hydraulicHorsepower / powerFluidHorsepower;

            if (raw < 0m || raw > 1m)
            {
                findings.Add(new HydraulicPumpFinding(
                    "SYSTEM_EFFICIENCY_CLAMPED",
                    $"The engine clamped the system efficiency from {Number(raw)} to {Number(systemEfficiency)}: a value " +
                    "outside 0 to 1 is not a real efficiency, so it is reported rather than shown as the answer.",
                    HydraulicPumpFindingSeverity.Warning));
            }
        }

        if (pumpEfficiency < minimumPumpEfficiency)
        {
            findings.Add(new HydraulicPumpFinding(
                "PUMP_EFFICIENCY_LOW",
                $"The pump efficiency {Number(pumpEfficiency)} is below the module's {Number(minimumPumpEfficiency)} " +
                "minimum for this pump type.",
                HydraulicPumpFindingSeverity.Warning));
        }

        var cavitation = Cavitation(intakePressure, VapourPressure(well));

        if (cavitation.Risk)
        {
            findings.Add(new HydraulicPumpFinding(
                "CAVITATION_PREDICTED",
                $"Cavitation is predicted: the available NPSH is {Number(cavitation.AvailablePsi)} psi against the " +
                $"{Number(cavitation.RequiredPsi)} psi the pump needs, a margin of " +
                $"{Number(cavitation.MarginPsi)} psi.",
                HydraulicPumpFindingSeverity.Warning));
        }
    }

    /// <summary>
    /// The vapour pressure the cavitation index is judged at, in psi.
    /// </summary>
    /// <remarks>
    /// The module has no vapour-pressure correlation and the screen does not ask for one, so the index uses the
    /// water vapour pressure at 60 degF that <c>NPSHCalculations</c> in the pump-performance module states (0.256 psia).
    /// That number is written here as well rather than shared, because the two modules do not reference each other; it is
    /// a physical constant at a stated temperature, and the screen prints the value it used.
    /// </remarks>
    public const decimal VapourPressureAt60FPsi = 0.256m;

    /// <summary>The vapour pressure the index uses.</summary>
    public static decimal VapourPressure(HYDRAULIC_PUMP_WELL_PROPERTIES well) => VapourPressureAt60FPsi;

    private static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);

    /// <summary>
    /// The findings that block a save: every <see cref="HydraulicPumpFindingSeverity.Error"/>.
    /// </summary>
    public static IReadOnlyList<HydraulicPumpFinding> Blocking(IEnumerable<HydraulicPumpFinding> findings) =>
        findings.Where(finding => finding.Severity == HydraulicPumpFindingSeverity.Error).ToList();
}
