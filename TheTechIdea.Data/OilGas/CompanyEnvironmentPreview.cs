namespace TheTechIdea.Data.OilGas;

/// <summary>An OilandGasTools country preset available for company setup.</summary>
public sealed record CompanyEnvironmentCountry(string Code, string Name);

/// <summary>Overrides are an OilandGasTools environment JSON object, without inheritance or server paths.</summary>
public sealed record CompanyEnvironmentPreviewRequest(string CountryCode, string OverridesJson);

/// <summary>A preview only: nothing in this response has been saved or verified for operation.</summary>
public sealed record CompanyEnvironmentPreview(
    CompanyScope Company, string CountryCode, string CountryName, string Culture,
    string? UnitSystem, string DepthUnit, string PressureUnit, string DiameterUnit,
    string[] CoordinateSystems, string[] Regulators, string[] Warnings)
{
    /// <summary>The configured tool time zone; null means no company display zone is confirmed.</summary>
    public string? TimeZoneId { get; init; }
    public string? WeightUnit { get; init; }
    public string? ForceUnit { get; init; }
    public string? DisplayCulture { get; init; }
    public string? DateFormat { get; init; }
    public string? DateTimeFormat { get; init; }
    public string? Calendar { get; init; }
}

/// <summary>Exact shared revisions and tool build used to interpret an unsaved company draft.</summary>
public sealed record CompanyEnvironmentDraftReview(CompanyEnvironmentPreview Settings, string SystemRevision,
    string? CountryRevision, string ToolsVersion, string DraftRevision);

public static class CompanyEnvironmentDraftIdentity
{
    public static string ForReview(CompanyScope company, CompanyEnvironmentPreviewRequest draft, string systemRevision, string? countryRevision, string toolsVersion)
    {
        _ = CompanyEnvironmentIdentity.ForCompany(company);
        ArgumentNullException.ThrowIfNull(draft);
        if (!CompanyEnvironmentIdentity.IsRevision(systemRevision) ||
            (countryRevision is not null && !CompanyEnvironmentIdentity.IsRevision(countryRevision)) ||
            string.IsNullOrWhiteSpace(toolsVersion) || string.IsNullOrWhiteSpace(draft.CountryCode) || draft.OverridesJson is null)
            throw new ArgumentException("The environment draft review is incomplete.");
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new
        { Company = company, draft.CountryCode, draft.OverridesJson, SystemRevision = systemRevision, CountryRevision = countryRevision, ToolsVersion = toolsVersion })));
    }
}

/// <summary>One reviewed change. Empty expected revision means the company has no saved environment.</summary>
public sealed record CompanyEnvironmentSaveRequest(string CountryCode, string OverridesJson, string ExpectedRevision, string IntentId);

/// <summary>A company change and the shared setup evidence reviewed for that exact draft.</summary>
public sealed record CompanyEnvironmentReviewedSaveRequest(CompanyEnvironmentSaveRequest Change, CompanyEnvironmentDraftReview Review);

/// <summary>Saved company configuration and the exact OilandGasTools build which interpreted it.</summary>
public sealed record CompanyEnvironmentConfiguration(CompanyScope Company, string CountryCode, string OverridesJson,
    string ToolsVersion, string Revision, string PreviousRevision, string IntentId, string ReviewedDraftRevision,
    string ReviewedSystemRevision, string? ReviewedCountryRevision);

public sealed record CompanyEnvironmentReadResult(CompanyScope Company, CompanyEnvironmentConfiguration? Configuration);

public sealed record CompanyEnvironmentReferenceResult(string Name, string Kind, string Status, string? Detail);

/// <summary>Checks of one saved revision only; source coverage and operational readiness are separate.</summary>
public sealed record CompanyEnvironmentVerification(CompanyScope Company, string Revision, string ToolsVersion,
    string[] ValidationIssues, CompanyEnvironmentReferenceResult[] ReferenceCases, bool SourcesVerified, string EffectiveRevision);

public sealed record CompanyEnvironmentCodeMapping(string Entity, string Field, string SourceCode, string CanonicalCode, string Origin);

public sealed record CompanyEnvironmentSource(string Name, string Module, string? Profile, int MappedCodeValues, int RowFilters, string BindingStatus)
{
    /// <summary>Profile checks do not establish database connectivity, source coverage or code semantics.</summary>
    public string MappingStatus { get; init; } = "Unconfirmed";
    public string[] MappingIssues { get; init; } = [];
    /// <summary>Effective settings only; values are not verified against source records or operational vocabulary.</summary>
    public CompanyEnvironmentCodeMapping[] CodeMappings { get; init; } = [];
}

/// <summary>Binding evidence only; no source data or mapping coverage has been tested.</summary>
public sealed record CompanyEnvironmentSourceReview(CompanyScope Company, string Revision, CompanyEnvironmentSource[] Sources, bool DataTested, string EffectiveRevision);

public sealed record CompanyEnvironmentWellTestRequest(string SourceName, string WellId, string ExpectedRevision, string ExpectedEffectiveRevision);

/// <summary>Result of testing one authorized WELL header; related tables and source coverage are not tested.</summary>
public sealed record CompanyEnvironmentWellTest(CompanyScope Company, string Revision, string SourceName, string WellId,
    string Status, string? WellName, string? Field, bool RelatedTablesTested, string LoaderVersion, string EffectiveRevision);

public sealed record SystemSetupRepositoryStatus(string Status);

/// <summary>Current installation, country and company layers resolved for one authorized company.</summary>
public sealed record CompanyEnvironmentEffectiveReview(CompanyScope Company, string CompanyRevision, string SystemRevision,
    string? CountryRevision, string EffectiveRevision, CompanyEnvironmentPreview Settings)
{
    /// <summary>Current means the saved company review used the same shared layers; it does not mean operational approval.</summary>
    public string ReviewStatus { get; init; } = "Unconfirmed";
}
