using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.Setup;

/// <summary>One choice in a setup list: the value stored, and the words the person reads.</summary>
/// <param name="Code">What is persisted (a code, a unit-system name, an id).</param>
/// <param name="DisplayName">What the list shows.</param>
/// <param name="Detail">An optional second line explaining the choice.</param>
public sealed record OgSetupChoice(string Code, string DisplayName, string? Detail = null)
{
    public override string ToString() => DisplayName;
}

/// <summary>
/// The lists the first-run wizard offers.
/// </summary>
/// <remarks>
/// <para>
/// Nothing here is invented: the countries are exactly the ones the wellbore environment ships a preset for, so a
/// chosen country can always be applied; the unit systems are the four the environment's <c>units.system</c>
/// accepts; the interface languages are the ones this build actually localises; and the time zones are the
/// operating system's own. The company types are the industry's own categories and are recorded as data — the
/// desktop does not act on them.
/// </para>
/// <para>
/// The currency is not a list. A country whose currency this build guessed wrong would be worse than a person
/// typing three letters, so the wizard offers the value from the country's regional data and accepts any
/// three-letter code.
/// </para>
/// </remarks>
public static class OgSetupCatalog
{
    /// <summary>Oil and gas company categories, recorded as data on the installation.</summary>
    public static IReadOnlyList<OgSetupChoice> CompanyTypes { get; } =
    [
        new("OPERATOR", "Operator", "Holds the licence and runs the operation"),
        new("NOC", "National oil company", "State-owned operator"),
        new("IOC", "International oil company", "International operator or partner"),
        new("INDEPENDENT", "Independent", "Independent exploration and production company"),
        new("SERVICE", "Service company", "Drilling, well services or engineering contractor"),
        new("CONSULTANT", "Consultant", "Technical or commercial adviser"),
        new("REGULATOR", "Regulator or ministry", "Supervises the licence"),
        new("JOINT_VENTURE", "Joint venture", "Operated or non-operated venture"),
    ];

    /// <summary>The four unit systems the wellbore environment accepts for <c>units.system</c>.</summary>
    public static IReadOnlyList<OgSetupChoice> UnitSystems { get; } =
    [
        new("oilfield", "Oilfield", "ft, psi, in, lbm/ft, klbf"),
        new("metric-kpa", "Metric (kPa)", "m, kPa, mm, kg/m"),
        new("metric-bar", "Metric (bar)", "m, bar, mm, kg/m"),
        new("metric-mpa", "Metric (MPa)", "m, MPa, mm, kg/m"),
    ];

    /// <summary>The countries the wellbore environment has a preset for, by ISO code, with its own name.</summary>
    public static IReadOnlyList<OgSetupChoice> Countries { get; } = BuildCountries();

    /// <summary>The interface languages this build localises.</summary>
    public static IReadOnlyList<OgSetupChoice> Cultures { get; } =
        OgLoc.SupportedCultures
            .Select(culture => new OgSetupChoice(culture.Name, culture.DisplayName))
            .ToList();

    /// <summary>The operating system's time zones, with their offsets shown.</summary>
    public static IReadOnlyList<OgSetupChoice> TimeZones { get; } = BuildTimeZones();

    /// <summary>The twelve fiscal-year start months.</summary>
    public static IReadOnlyList<OgSetupChoice> FiscalMonths { get; } =
        Enumerable.Range(1, 12)
            .Select(month => new OgSetupChoice(
                month.ToString(System.Globalization.CultureInfo.InvariantCulture),
                new DateTime(2000, month, 1).ToString("MMMM", System.Globalization.CultureInfo.CurrentCulture)))
            .ToList();

    /// <summary>The chosen entry, or null when <paramref name="code"/> is not offered.</summary>
    public static OgSetupChoice? Find(IReadOnlyList<OgSetupChoice> list, string? code) =>
        list.FirstOrDefault(choice =>
            string.Equals(choice.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The environment document a country and unit system describe, as stored in the company settings. Null when
    /// no country is named.
    /// </summary>
    /// <remarks>
    /// The document is deliberately compact — the country it extends and the unit system — rather than the
    /// resolved environment, which serialises the whole unit registry and runs past a megabyte. The presets ship
    /// with the application, so the document records the company's choice and the engine supplies the values; a
    /// later build that corrects a preset is then a correction for every user instead of a copy that drifts.
    /// </remarks>
    public static string? BuildEnvironmentDocument(string? countryCode, string? unitSystem)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(unitSystem)
            ? $"{{\"extends\":\"country:{countryCode}\"}}"
            : $"{{\"extends\":\"country:{countryCode}\",\"units\":{{\"system\":\"{unitSystem}\"}}}}";
    }

    private static IReadOnlyList<OgSetupChoice> BuildCountries()
    {
        var countries = new List<OgSetupChoice>();

        foreach (var code in CountryPresets.Codes)
        {
            var name = CountryPresets.NameOf(code);

            countries.Add(string.IsNullOrWhiteSpace(name)
                ? new OgSetupChoice(code, code)
                : new OgSetupChoice(code, $"{name} ({code})"));
        }

        return countries;
    }

    private static IReadOnlyList<OgSetupChoice> BuildTimeZones()
    {
        var now = DateTimeOffset.UtcNow;
        var zones = new List<OgSetupChoice>();

        foreach (var zone in TimeZoneInfo.GetSystemTimeZones())
        {
            var offset = zone.GetUtcOffset(now);
            var label = offset == TimeSpan.Zero
                ? "UTC"
                : $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset:hh\\:mm}";

            zones.Add(new OgSetupChoice(zone.Id, $"{zone.DisplayName} ({label})"));
        }

        return zones;
    }
}
