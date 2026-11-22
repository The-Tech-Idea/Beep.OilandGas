using System;
using System.Collections.Generic;
using System.Globalization;

namespace Beep.OilandGas.ProductionAccounting.Calculations;

/// <summary>
/// The production-tax and provision arithmetic (ACC-08), in <c>decimal</c>.
/// </summary>
/// <remarks>
/// The rate convention is the owner's, taken from <c>ProductionTaxService.NormalizeRate</c>: a recorded rate above 1 is
/// <b>percentage points</b> and is divided by 100, and a rate at or below 1 is already a fraction. That rule is stated here
/// once so every computation applies it identically, and a rate of zero or null is not a rate at all - it is what AC-2
/// refuses, rather than a silent zero tax.
/// </remarks>
public static class TaxMath
{
    /// <summary>The minor-unit precision money is rounded to.</summary>
    public const int CurrencyDecimals = 2;

    /// <summary>Rounds money to the currency's minor unit, away from zero.</summary>
    public static decimal Round(decimal amount) =>
        Math.Round(amount, CurrencyDecimals, MidpointRounding.AwayFromZero);

    /// <summary>
    /// The owner's rate normalisation: above 1 is a percentage, at or below 1 is a fraction, and zero or null is no rate.
    /// </summary>
    public static decimal? NormalizeRate(decimal? rate)
    {
        if (rate is not { } value || value <= 0m)
        {
            return null;
        }

        return value > 1m ? value / 100m : value;
    }

    /// <summary>The tax on a basis at a normalised rate, rounded once.</summary>
    public static decimal Amount(decimal basis, decimal rate)
    {
        if (basis < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(basis), basis, "A tax basis cannot be negative.");
        }

        if (rate < 0m || rate > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rate), rate, "A normalised tax rate is a fraction between 0 and 1.");
        }

        return Round(basis * rate);
    }

    /// <summary>
    /// The deferred-tax movement, mirroring the owner's <c>RecordDeferredTaxBalance</c>: the change in each balance against the
    /// prior period is what posts, not the balance itself, so a second provision for the same period does not double-count.
    /// </summary>
    /// <param name="asset">The closing deferred tax asset.</param>
    /// <param name="liability">The closing deferred tax liability.</param>
    /// <param name="priorAsset">The asset carried from the previous period end.</param>
    /// <param name="priorLiability">The liability carried from the previous period end.</param>
    public static DeferredMovement Movement(
        decimal asset,
        decimal liability,
        decimal priorAsset,
        decimal priorLiability) =>
        new(
            Round(asset - priorAsset),
            Round(liability - priorLiability),
            Round(asset),
            Round(liability));
}

/// <summary>The deferred-tax movement a provision produces (ACC-08).</summary>
/// <param name="AssetDelta">The change in the deferred tax asset; positive increases it.</param>
/// <param name="LiabilityDelta">The change in the deferred tax liability; positive increases it.</param>
/// <param name="Asset">The closing asset.</param>
/// <param name="Liability">The closing liability.</param>
public readonly record struct DeferredMovement(
    decimal AssetDelta,
    decimal LiabilityDelta,
    decimal Asset,
    decimal Liability)
{
    /// <summary>The net charge to income tax expense: a liability increase less an asset increase.</summary>
    public decimal Expense => Round(LiabilityDelta - AssetDelta);

    /// <summary>Whether anything moved, in which case a journal is expected (AC-4).</summary>
    public bool Moved => AssetDelta != 0m || LiabilityDelta != 0m;

    private static decimal Round(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}

/// <summary>
/// Reads the owner's <c>GOVERNMENTAL_TAX_DATA.REMARK</c> contract (ACC-08).
/// </summary>
/// <remarks>
/// The owner's <c>ProductionTaxService</c> carries its jurisdiction and its extra rates in the remark column as
/// <c>key=value</c> tokens separated by <c>;</c>, <c>,</c> or whitespace, with the keys named in
/// <c>ProductionTaxRemarkKeys</c>. That is the only place a jurisdiction or a corporate rate is recorded, so this reader
/// mirrors the owner's separators and its case-insensitive key match exactly rather than inventing a second format.
/// </remarks>
public static class TaxRemark
{
    /// <summary>The tokens of a remark, as key and value.</summary>
    public static IReadOnlyDictionary<string, string> Parse(string? remark)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(remark))
        {
            return values;
        }

        foreach (var token in remark.Split([';', ',', ' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = token.Split('=');

            if (parts.Length != 2)
            {
                continue;
            }

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            if (key.Length == 0 || value.Length == 0)
            {
                continue;
            }

            values[key] = value;
        }

        return values;
    }

    /// <summary>A value by key, or null when the remark does not carry it.</summary>
    public static string? Value(string? remark, string key) =>
        Parse(remark).TryGetValue(key, out var value) ? value : null;

    /// <summary>A decimal value by key, or null when it is absent or not a number.</summary>
    public static decimal? Rate(string? remark, string key) =>
        Value(remark, key) is { } text
        && decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
}
