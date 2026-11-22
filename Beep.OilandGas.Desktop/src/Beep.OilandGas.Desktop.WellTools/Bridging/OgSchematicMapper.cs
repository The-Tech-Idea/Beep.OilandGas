using TheTechIdea.OilGas.WellSchematic;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>One casing/tubing string as the schematic shows it.</summary>
public sealed record OgSchematicStringRow(
    string Id,
    string Name,
    string Symbol,
    double Top,
    double Bottom,
    double? OuterDiameter);

/// <summary>One annulus with its MAASP, which is often not computed.</summary>
public sealed record OgAnnulusRow(string Name, string Fluid, string Maasp, string Status);

/// <summary>One barrier element as the barrier panel lists it.</summary>
public sealed record OgBarrierRow(
    string Element,
    string Role,
    string Interval,
    string Status,
    string Evidence,
    bool Verified);

/// <summary>What the schematic screen says above the picture.</summary>
public sealed record OgSchematicSummary(
    string Name,
    int Strings,
    int Equipment,
    int Annuli,
    int Barriers,
    int UnverifiedBarriers,
    int Findings,
    string DepthRange);

/// <summary>
/// VSB-05: maps the tools' schematic document onto the rows the page shows, and asks the tools' own
/// validator, annulus screening and tally for the facts rather than recomputing them.
/// </summary>
/// <remarks>
/// The demo document is the tools' gas-lift producer, because PPDM module setup is blocked (G-08) and no
/// tubular records exist yet. Every row here comes from the document or a tools service, never from this
/// class's own idea of what a well should look like.
/// </remarks>
public static class OgSchematicMapper
{
    public const string NotComputed = "not computed";

    public static WellSchematicDocument Demo()
    {
        var document = WellSchematicTemplates.CreateGasLiftProducerDemo();

        // The drawing template ships without a barrier set; the production barrier template is the tools'
        // own answer for this well type, so the barrier panel and overlay have something real to show.
        document.Barriers ??= BarrierTemplates.ProductionWell(document)
                              ?? BarrierTemplates.DrillingWell(document);

        return document;
    }

    public static IReadOnlyList<OgSchematicStringRow> Strings(WellSchematicDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.Strings
            .OrderBy(item => item.TopDepth)
            .Select(item => new OgSchematicStringRow(
                item.Id,
                string.IsNullOrWhiteSpace(item.Name) ? item.SymbolId : item.Name,
                item.SymbolId,
                item.TopDepth,
                item.BottomDepth,
                item.Attributes?.OuterDiameter?.Value ?? item.OuterDiameter))
            .ToList();
    }

    /// <summary>Annuli with MAASP where the document carries it, and the exact "not computed" when it does not.</summary>
    public static IReadOnlyList<OgAnnulusRow> Annuli(WellSchematicDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return SchematicAnnuli.Identify(document)
            .Select(annulus => new OgAnnulusRow(
                annulus.Name,
                annulus.Fluid.ToString(),
                annulus.Maasp is { } maasp ? $"{maasp.Value:0.#} {maasp.Uom}" : NotComputed,
                annulus.Maasp is null
                    ? NotComputed
                    : SchematicAnnuli.Screen(document, annulus).Count == 0
                        ? "within limits"
                        : $"{SchematicAnnuli.Screen(document, annulus).Count} screening issue(s)"))
            .ToList();
    }

    public static IReadOnlyList<OgBarrierRow> Barriers(WellSchematicDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.Barriers is not { } set)
        {
            return [];
        }

        var rows = new List<OgBarrierRow>();

        foreach (var envelope in set.Envelopes)
        {
            foreach (var element in envelope.Elements)
            {
                var interval = element.Top is { } top && element.Bottom is { } bottom
                    ? $"{top.Value:0.#}–{bottom.Value:0.#} {bottom.Uom}"
                    : NotComputed;

                var verified = element.VerifiedDate.HasValue
                               || !string.IsNullOrWhiteSpace(element.VerificationMethod)
                               || !string.IsNullOrWhiteSpace(element.AcceptanceRef);

                var evidence = verified
                    ? string.Join(
                        ", ",
                        new[]
                        {
                            element.VerificationMethod,
                            element.VerifiedDate?.ToString("yyyy-MM-dd"),
                            element.AcceptanceRef,
                        }.Where(part => !string.IsNullOrWhiteSpace(part)))
                    : "no evidence recorded";

                rows.Add(new OgBarrierRow(
                    element.DisplayName,
                    envelope.Role.ToString(),
                    interval,
                    element.Status.ToString(),
                    evidence,
                    verified));
            }
        }

        return rows;
    }

    /// <summary>The tools' validation findings, each with the element it concerns (AC-5).</summary>
    public static IReadOnlyList<SchematicIssue> Findings(WellSchematicDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return WellSchematicValidator.Validate(document, SymbolCatalog.Current, includeDisplayChecks: false);
    }

    /// <summary>
    /// Unknown symbol or component types. The schematic must never draw a blank where a component is: the
    /// page lists the type and the tools fall back to their generic symbol.
    /// </summary>
    public static IReadOnlyList<string> Warnings(WellSchematicDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var known = SymbolCatalog.Current.Symbols
            .Select(symbol => symbol.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var warnings = new List<string>();

        foreach (var item in document.Strings.Where(item => !known.Contains(item.SymbolId)))
        {
            warnings.Add($"unknown symbol '{item.SymbolId}' on string '{item.Name}' — a generic symbol is drawn");
        }

        foreach (var item in document.Equipment.Where(item => !known.Contains(item.SymbolId)))
        {
            var type = string.IsNullOrWhiteSpace(item.ComponentType) ? item.SymbolId : item.ComponentType;
            warnings.Add($"unknown component type '{type}' — a generic symbol is drawn");
        }

        return warnings;
    }

    /// <summary>The tools' casing and tubing program; its rows are the strings, in order (AC-6).</summary>
    public static TallyTable Tally(WellSchematicDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return TallyBuilder.CasingProgram(document);
    }

    public static OgSchematicSummary Summarise(WellSchematicDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var barriers = Barriers(document);

        return new OgSchematicSummary(
            string.IsNullOrWhiteSpace(document.Name) ? "Well schematic" : document.Name,
            document.Strings.Count,
            document.Equipment.Count,
            SchematicAnnuli.Identify(document).Count,
            barriers.Count,
            barriers.Count(barrier => !barrier.Verified),
            Findings(document).Count,
            $"{document.TopDepth:0.#}–{document.BottomDepth:0.#} {document.DepthUnit}");
    }
}
