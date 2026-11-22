using System;
using System.Collections.Generic;
using System.Globalization;

namespace Beep.OilandGas.EconomicAnalysis.Data;

/// <summary>
/// The fiscal terms one economic evaluation is run under (ECO-01).
/// </summary>
/// <remarks>
/// The module had no fiscal regime at all - only a bare <c>taxRate</c> argument on the after-tax analysis - so the regime
/// shape is stated here, as the feature's open question asks. It is a <b>screening</b> regime: a royalty taken off gross
/// revenue and an income tax on positive pre-tax cash flow. It is not a production-sharing contract and it has no
/// depreciation schedule, loss carry-forward or investment credit, and the evaluation says so.
///
/// Every term is nullable on purpose: <b>0 is a stated value and null is a missing one</b>, which is what lets a run be
/// blocked with the missing term named instead of silently treating an unset royalty as zero.
/// </remarks>
/// <param name="Name">The regime's name as the user states it, e.g. "Concession - 12.5% royalty".</param>
/// <param name="RoyaltyRate">Royalty as a fraction of gross revenue; null when the term is not configured.</param>
/// <param name="TaxRate">Income tax as a fraction of positive pre-tax cash flow; null when not configured.</param>
public sealed record FiscalRegime(string? Name, decimal? RoyaltyRate, decimal? TaxRate)
{
    /// <summary>The terms a run cannot start without, in the order they are asked for.</summary>
    public static IReadOnlyList<string> RequiredTerms { get; } = ["royalty rate", "tax rate"];

    /// <summary>The terms that are not configured, by name - empty when the regime is complete.</summary>
    public IReadOnlyList<string> MissingTerms()
    {
        var missing = new List<string>();

        if (RoyaltyRate is null)
        {
            missing.Add("royalty rate");
        }

        if (TaxRate is null)
        {
            missing.Add("tax rate");
        }

        return missing;
    }

    /// <summary>True when every required term has a value (a stated zero counts).</summary>
    public bool IsComplete => MissingTerms().Count == 0;

    /// <summary>The sentence a blocked run shows, naming every missing term.</summary>
    public string BlockedReason =>
        "The fiscal regime is not configured: " + string.Join(", ", MissingTerms()) +
        " missing. Set the term on the regime before running the evaluation.";

    /// <summary>The regime as the record stores it, so an approved evaluation says what it was run under.</summary>
    public string Describe() =>
        (string.IsNullOrWhiteSpace(Name) ? "(unnamed regime)" : Name!.Trim()) +
        " · royalty " + Percent(RoyaltyRate) + " · tax " + Percent(TaxRate) +
        " · screening: royalty off gross revenue, tax on positive pre-tax cash flow, no depreciation or loss carry-forward";

    private static string Percent(decimal? value) =>
        value is { } rate
            ? (rate * 100m).ToString("0.####", CultureInfo.InvariantCulture) + " %"
            : "(not configured)";
}

/// <summary>What a fiscal regime can go wrong with, as one place the screen can read.</summary>
public static class FiscalRegimeRules
{
    /// <summary>
    /// Checks a rate is a fraction in [0, 1], so a regime cannot carry "12.5" meaning 1250 %.
    /// </summary>
    public static (string Code, string Message)? Check(FiscalRegime regime)
    {
        ArgumentNullException.ThrowIfNull(regime);

        if (regime.MissingTerms().Count > 0)
        {
            return ("economics-fiscal-term-missing", regime.BlockedReason);
        }

        foreach (var (value, name) in new (decimal?, string)[]
                 {
                     (regime.RoyaltyRate, "royalty rate"),
                     (regime.TaxRate, "tax rate"),
                 })
        {
            if (value is < 0m or > 1m)
            {
                return ("economics-fiscal-term-out-of-range",
                    $"The {name} {value!.Value.ToString("0.####", CultureInfo.InvariantCulture)} is not a fraction " +
                    "between 0 and 1; a 12.5 % royalty is entered as 0.125.");
            }
        }

        return null;
    }
}
