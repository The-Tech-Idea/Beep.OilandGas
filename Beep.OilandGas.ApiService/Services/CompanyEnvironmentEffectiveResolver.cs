using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using TheTechIdea.OilGas.Core;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Resolves system defaults, country preset, country overrides and company overrides in that order.</summary>
public sealed class CompanyEnvironmentEffectiveResolver(ICompanyEnvironmentStore companies, ISystemSetupRecordReader system)
{
    public async Task<CompanyEnvironmentEffectiveReview> ResolveAsync(CompanyScope company, Func<Task> authority) =>
        (await ResolveRuntimeAsync(company, authority)).Review;

    public async Task<CompanyEnvironmentDraftReview> PreviewDraftAsync(CompanyScope company, CompanyEnvironmentPreviewRequest draft, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(authority);
        // Validate the exact same company override contract used by a save.
        _ = CompanyEnvironmentPreviewer.ValidateDraft(company, draft);
        var baseline = await system.ReadAsync("TOOLS", "DEFAULT", authority) ?? throw new InvalidOperationException("Configure shared TOOLS/DEFAULT settings first.");
        SystemSetupIdentity.RequireValid(baseline, "TOOLS", "DEFAULT");
        var country = await system.ReadAsync("COUNTRY", draft.CountryCode, authority);
        if (country is not null) SystemSetupIdentity.RequireValid(country, "COUNTRY", draft.CountryCode);
        var environment = BuildEnvironment(draft.CountryCode, draft.OverridesJson, baseline, country);
        await authority();
        var closingBaseline = await system.ReadAsync("TOOLS", "DEFAULT", authority);
        var closingCountry = await system.ReadAsync("COUNTRY", draft.CountryCode, authority);
        await authority();
        if (closingBaseline != baseline || closingCountry != country)
            throw new InvalidOperationException("A system or country setup layer changed during review.");
        var toolsVersion = CompanyEnvironmentPreviewer.ToolsVersion;
        var draftRevision = CompanyEnvironmentDraftIdentity.ForReview(company, draft, baseline.Revision, country?.Revision, toolsVersion);
        return new(Describe(company, draft.CountryCode, environment), baseline.Revision, country?.Revision, toolsVersion, draftRevision);
    }

    public async Task<ResolvedCompanyEnvironment> ResolveRuntimeAsync(CompanyScope company, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(authority);
        var selected = await companies.ReadAsync(company, authority) ?? throw new InvalidOperationException("The company environment is not saved.");
        CompanyEnvironmentIdentity.RequireValid(selected, company);
        if (selected.ToolsVersion != CompanyEnvironmentPreviewer.ToolsVersion)
            throw new InvalidOperationException("The company environment was reviewed with another OilandGasTools build.");
        _ = CompanyEnvironmentPreviewer.ValidateForSave(company, new(selected.CountryCode, selected.OverridesJson, selected.PreviousRevision, selected.IntentId));

        var baseline = await system.ReadAsync("TOOLS", "DEFAULT", authority) ?? throw new InvalidOperationException("Configure shared TOOLS/DEFAULT settings first.");
        SystemSetupIdentity.RequireValid(baseline, "TOOLS", "DEFAULT");
        var country = await system.ReadAsync("COUNTRY", selected.CountryCode, authority);
        if (country is not null) SystemSetupIdentity.RequireValid(country, "COUNTRY", selected.CountryCode);
        var environment = BuildEnvironment(selected.CountryCode, selected.OverridesJson, baseline, country, out var mappingOrigins);

        await authority();
        var closingCompany = await companies.ReadAsync(company, authority);
        var closingBaseline = await system.ReadAsync("TOOLS", "DEFAULT", authority);
        var closingCountry = await system.ReadAsync("COUNTRY", selected.CountryCode, authority);
        await authority();
        if (closingCompany != selected || closingBaseline != baseline || closingCountry != country)
            throw new InvalidOperationException("A system, country or company setup layer changed during review.");
        var revision = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        { Company = company, CompanyRevision = selected.Revision, SystemRevision = baseline.Revision, CountryRevision = country?.Revision, selected.ToolsVersion })));
        var settings = Describe(company, selected.CountryCode, environment);
        var reviewStatus = settings.Warnings.Length == 0 && selected.ReviewedSystemRevision == baseline.Revision && selected.ReviewedCountryRevision == country?.Revision
            ? "Current" : "ReviewRequired";
        return new(new CompanyEnvironmentEffectiveReview(company, selected.Revision, baseline.Revision, country?.Revision, revision, settings)
        { ReviewStatus = reviewStatus }, environment, mappingOrigins);
    }

    internal static OilGasEnvironment BuildEnvironment(string countryCode, string overrides, SystemSetupConfiguration baseline, SystemSetupConfiguration? country)
        => BuildEnvironment(countryCode, overrides, baseline, country, out _);

    private static OilGasEnvironment BuildEnvironment(string countryCode, string overrides, SystemSetupConfiguration baseline, SystemSetupConfiguration? country,
        out IReadOnlyDictionary<string, SetupMappingOrigin> mappingOrigins)
    {
        if (!JsonCountryCatalog.BuiltIn.TryGet(countryCode, out var preset))
            throw new InvalidOperationException("The country preset is unavailable.");

        var origins = new Dictionary<string, SetupMappingOrigin>(StringComparer.OrdinalIgnoreCase);
        var merged = new JsonObject();
        Merge(merged, ParseSystem(baseline.SettingsJson), "System", "", origins);
        Merge(merged, Parse(preset), "CountryPreset", "", origins);
        if (country is not null) Merge(merged, ParseSystem(country.SettingsJson), "Country", "", origins);
        Merge(merged, Parse(overrides), "Company", "", origins);
        // Partial mappings may inherit a source, but the effective result must contain
        // a complete, safe Beep source identity before OilandGasTools can use it.
        CompanyEnvironmentPreviewer.RequireSafeSourceReferences(merged.DeepClone());
        if (merged.Any(p => p.Key.Equals("extends", StringComparison.OrdinalIgnoreCase)) ||
            merged.Where(p => p.Key.Equals("resources", StringComparison.OrdinalIgnoreCase)).Any(p =>
                p.Value is not JsonObject resources || resources.Any(r => r.Key.Equals("countryFolders", StringComparison.OrdinalIgnoreCase))))
            throw new InvalidOperationException("Effective settings cannot resolve server files or country folders.");
        using var scope = OilGasServices.Scope(new OilGasServices { Countries = JsonCountryCatalog.BuiltIn });
        var environment = OilGasEnvironment.FromJson(merged.ToJsonString(), out var warnings);
        if (warnings.Count != 0 || environment.Region is null || environment.Units is null ||
            environment.Region.CoordinateSystems is null || environment.Region.Regulators is null ||
            !string.Equals(environment.Region.Country, countryCode, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(environment.Culture))
            throw new InvalidOperationException("The shared and company settings cannot be resolved for this country.");
        _ = CompanyEnvironmentPreviewer.ConfirmTimeZone(environment);
        mappingOrigins = origins;
        return environment;
    }

    private static CompanyEnvironmentPreview Describe(CompanyScope company, string countryCode, OilGasEnvironment environment) =>
        new CompanyEnvironmentPreview(company, countryCode, environment.Region.Name ?? countryCode,
            environment.Culture, environment.Units.System, environment.Units.DepthUnit, environment.Units.PressureUnit,
            environment.Units.DiameterUnit, environment.Region.CoordinateSystems.ToArray(), environment.Region.Regulators.ToArray(), ReviewSettings(environment))
            { TimeZoneId = CompanyEnvironmentPreviewer.ConfirmTimeZone(environment), WeightUnit = environment.Units.WeightUnit, ForceUnit = environment.Units.ForceUnit,
              DisplayCulture = environment.Locale.Culture, DateFormat = environment.Locale.DateFormat, DateTimeFormat = environment.Locale.DateTimeFormat, Calendar = environment.Locale.Calendar };

    internal static string[] ReviewSettings(OilGasEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        return environment.Validate().Concat(CompanyEnvironmentSources.ReviewEnvironment(environment)).Take(64).ToArray();
    }

    private static JsonObject ParseSystem(string json)
    {
        var root = Parse(json);
        CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(root);
        return root;
    }
    private static JsonObject Parse(string json)
    {
        var root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new InvalidOperationException("A setup layer must be a JSON object.");
        CompanyEnvironmentPreviewer.RequireUnambiguousKeys(root);
        return root;
    }
    // Match OilandGasTools: objects merge case-insensitively; arrays and scalar values replace earlier layers.
    private static void Merge(JsonObject target, JsonObject overlay, string layer, string path, Dictionary<string, SetupMappingOrigin> origins)
    {
        foreach (var pair in overlay)
        {
            var child = path + "/" + Escape(pair.Key);
            var old = target.FirstOrDefault(x => x.Key.Equals(pair.Key, StringComparison.OrdinalIgnoreCase));
            if (old.Value is JsonObject previous && pair.Value is JsonObject next) { Merge(previous, next, layer, child, origins); continue; }
            if (old.Key is not null) target.Remove(old.Key);
            target[pair.Key] = pair.Value?.DeepClone();
            if (child.Equals("/dataSources", StringComparison.OrdinalIgnoreCase) || child.StartsWith("/dataSources/", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var key in origins.Keys.Where(k => k.Equals(child, StringComparison.OrdinalIgnoreCase) || k.StartsWith(child + "/", StringComparison.OrdinalIgnoreCase)).ToArray()) origins.Remove(key);
                TrackMappings(pair.Value, child, layer, origins);
            }
        }
    }

    private static void TrackMappings(JsonNode? node, string path, string layer, Dictionary<string, SetupMappingOrigin> origins)
    {
        if (node is JsonObject obj)
        {
            foreach (var pair in obj) TrackMappings(pair.Value, path + "/" + Escape(pair.Key), layer, origins);
            return;
        }
        var parts = path.Split('/');
        if (parts.Length == 7 && parts[1].Equals("dataSources", StringComparison.OrdinalIgnoreCase) && parts[3].Equals("codes", StringComparison.OrdinalIgnoreCase) &&
            node is JsonValue scalar && scalar.TryGetValue<string>(out var value))
            origins[path] = new(layer, value);
    }
    private static string Escape(string value) => value.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);
    internal static string MappingKey(string source, string entity, string field, string code) =>
        "/dataSources/" + Escape(source) + "/codes/" + Escape(entity) + "/" + Escape(field) + "/" + Escape(code);
}

public sealed record SetupMappingOrigin(string Layer, string Value);
/// <summary>Request-local OilandGasTools environment; never assign it to a process-wide global.</summary>
public sealed record ResolvedCompanyEnvironment(CompanyEnvironmentEffectiveReview Review, OilGasEnvironment Environment, IReadOnlyDictionary<string, SetupMappingOrigin> MappingOrigins);
