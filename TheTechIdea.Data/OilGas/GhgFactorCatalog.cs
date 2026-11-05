using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>Administrator-supplied factors for one jurisdiction; storage alone does not approve their use.</summary>
public sealed record GhgFactorCatalog(string Jurisdiction, IReadOnlyList<GhgFactorDefinition> Factors);

/// <summary>One company's review of an installation-wide catalog, without claiming applicability or approval.</summary>
public sealed record CompanyGhgCatalogReview(CompanyScope Company, string CompanyRevision, string EffectiveRevision, SystemSetupConfiguration Catalog);

public sealed record GhgFactorDefinition(string Id, string SourceCategory, string InputUnit, string OutputUnit,
    [property: JsonRequired] decimal Factor, DateOnly EffectiveFrom, DateOnly EffectiveThrough, string SourceReference);

public static class GhgFactorCatalogValidation
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };

    public static GhgFactorCatalog Parse(string key, string json)
    {
        _ = SystemSetupIdentity.ForKey("GHG_FACTORS", key);
        var catalog = JsonSerializer.Deserialize<GhgFactorCatalog>(json, JsonOptions)
            ?? throw new ArgumentException("Provide a factor catalog.");
        if (catalog.Jurisdiction != key || catalog.Factors is not { Count: > 0 and <= 256 })
            throw new ArgumentException("Provide the selected jurisdiction and between 1 and 256 factors.");
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var factor in catalog.Factors)
        {
            if (factor is null || !ValidText(factor.Id, 128) || !ids.Add(factor.Id) ||
                !ValidText(factor.SourceCategory, 128) || !ValidText(factor.InputUnit, 64) ||
                !ValidText(factor.OutputUnit, 64) || !ValidText(factor.SourceReference, 512) ||
                factor.Factor < 0 || factor.EffectiveFrom == default || factor.EffectiveThrough < factor.EffectiveFrom)
                throw new ArgumentException("Every factor needs an identity, category, units, nonnegative value, effective dates and source.");
        }
        foreach (var group in catalog.Factors.GroupBy(x => x.SourceCategory, StringComparer.OrdinalIgnoreCase))
        {
            var ordered = group.OrderBy(x => x.EffectiveFrom).ToArray();
            for (var i = 1; i < ordered.Length; i++)
                if (ordered[i].EffectiveFrom <= ordered[i - 1].EffectiveThrough)
                    throw new ArgumentException("A source category cannot have overlapping factor periods.");
        }
        return catalog;
    }

    private static bool ValidText(string? text, int maximum) =>
        !string.IsNullOrWhiteSpace(text) && text.Length <= maximum && text == text.Trim() && !text.Any(char.IsControl);
}
