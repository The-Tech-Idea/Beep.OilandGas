using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>Validates binding review evidence for one company and one reviewed environment.</summary>
public static class CompanyEnvironmentSourceReviewValidator
{
    public static void RequireValid(CompanyEnvironmentSourceReview? result, CompanyScope company, string revision, string? effectiveRevision)
    {
        if (result is null || result.Company != company || result.Revision != revision || result.DataTested || !CompanyEnvironmentIdentity.IsRevision(result.EffectiveRevision) ||
            effectiveRevision is not null && result.EffectiveRevision != effectiveRevision ||
            result.Sources is null || result.Sources.Length > 32 || result.Sources.Any(x => x is null || string.IsNullOrWhiteSpace(x.Name) ||
                x.BindingStatus is not ("Bound" or "Unavailable") || x.MappingStatus is not ("ProfileChecked" or "ReviewRequired") || x.MappingIssues is null ||
                x.MappingIssues.Length > 64 || x.MappingIssues.Any(string.IsNullOrWhiteSpace) || (x.MappingStatus == "ProfileChecked") != (x.MappingIssues.Length == 0) ||
                x.MappedCodeValues < 0 || x.RowFilters < 0 || x.CodeMappings is null || x.CodeMappings.Length != x.MappedCodeValues ||
                x.CodeMappings.Any(m => m is null || InvalidText(m.Entity) || InvalidText(m.Field) || InvalidText(m.SourceCode) || InvalidText(m.CanonicalCode) || m.Origin is not ("System" or "CountryPreset" or "Country" or "Company")) ||
                x.CodeMappings.Select(m => (m.Entity.ToUpperInvariant(), m.Field.ToUpperInvariant(), m.SourceCode.ToUpperInvariant())).Distinct().Count() != x.CodeMappings.Length) ||
            result.Sources.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != result.Sources.Length)
            throw new InvalidOperationException("The source review does not match the selected company and revision.");
    }

    private static bool InvalidText(string? value) => string.IsNullOrWhiteSpace(value) || value.Length > 512 || value.Any(char.IsControl);
}
