using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using TheTechIdea.OilGas.Core;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reads the actual embedded OilandGasTools presets and previews company overrides without file inheritance.</summary>
public static class CompanyEnvironmentPreviewer
{
    private const int MaxOverrideLength = 128 * 1024;

    public static CompanyEnvironmentCountry[] Countries() => JsonCountryCatalog.BuiltIn.Codes
        .Select(code => new CompanyEnvironmentCountry(code, CountryName(code)))
        .ToArray();

    /// <summary>Identify the actual compiled tool assembly and its country catalog, not an assumed package version.</summary>
    public static string ToolsVersion => typeof(OilGasEnvironment).Assembly.ManifestModule.ModuleVersionId.ToString("N");

    /// <summary>Validate a change for storage; external files and credentials need a separate approved owner.</summary>
    public static CompanyEnvironmentPreview ValidateForSave(CompanyScope company, CompanyEnvironmentSaveRequest request)
    {
        _ = CompanyEnvironmentIdentity.ForChange(company, request, ToolsVersion);
        var draft = new CompanyEnvironmentPreviewRequest(request.CountryCode, request.OverridesJson);
        var result = ValidateDraft(company, draft);
        var (_, environment, _) = Load(draft);
        // Mapping-only source overlays inherit their identity from the reviewed shared layers.
        // Validate their full profile after merging, inside the owned save transaction.
        var root = JsonNode.Parse(request.OverridesJson, documentOptions: new JsonDocumentOptions
        { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject;
        if (root?.FirstOrDefault(p => p.Key.Equals("dataSources", StringComparison.OrdinalIgnoreCase)).Value is JsonObject sources)
            foreach (var (name, definition) in sources)
                if (definition is JsonObject source && !source.Any(p => p.Key.Equals("kind", StringComparison.OrdinalIgnoreCase)))
                    environment.DataSources.Remove(name);
        if (CompanyEnvironmentSources.ReviewEnvironment(environment).Length != 0)
            throw new ArgumentException("Review source profile mappings before saving.");
        return result;
    }

    public static CompanyEnvironmentPreview ValidateDraft(CompanyScope company, CompanyEnvironmentPreviewRequest request)
    {
        var preview = Preview(company, request);
        if (preview.CountryCode != request.CountryCode || preview.Warnings.Length != 0)
            throw new ArgumentException("Resolve environment warnings before saving.");
        var root = JsonNode.Parse(request.OverridesJson, documentOptions: new JsonDocumentOptions
        { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        RequireSafeSourceReferences(root, allowMappingOverlay: true);
        RequireNoSensitiveReferences(root, true);
        return preview;
    }

    public static CompanyEnvironmentPreview Preview(CompanyScope company, CompanyEnvironmentPreviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(company);
        var (code, environment, warnings) = Load(request);
        return new CompanyEnvironmentPreview(company, code, CountryName(code), environment.Culture, environment.Units.System,
            environment.Units.DepthUnit, environment.Units.PressureUnit, environment.Units.DiameterUnit,
            environment.Region.CoordinateSystems.ToArray(), environment.Region.Regulators.ToArray(), warnings.ToArray())
            { TimeZoneId = ConfirmTimeZone(environment), WeightUnit = environment.Units.WeightUnit, ForceUnit = environment.Units.ForceUnit,
              DisplayCulture = environment.Locale.Culture, DateFormat = environment.Locale.DateFormat, DateTimeFormat = environment.Locale.DateTimeFormat, Calendar = environment.Locale.Calendar };
    }

    internal static string? ConfirmTimeZone(OilGasEnvironment environment)
    {
        if (environment.Locale is null) throw new ArgumentException("The environment locale is unavailable.");
        var id = environment.Locale.TimeZone;
        if (string.IsNullOrWhiteSpace(id)) return null;
        if (id.Length > 128 || id.Any(char.IsControl)) throw new ArgumentException("The environment time zone is invalid.");
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(id.Trim()); }
        catch (Exception error) when (error is TimeZoneNotFoundException or InvalidTimeZoneException)
        { throw new ArgumentException("The configured environment time zone is not available on the API host.", error); }
        return id.Trim();
    }

    internal static (string Code, OilGasEnvironment Environment, List<string> Warnings) Load(CompanyEnvironmentPreviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = request.CountryCode?.Trim().ToUpperInvariant();
        if (code is null || code.Length != 2 || !JsonCountryCatalog.BuiltIn.TryGet(code, out _))
            throw new ArgumentException("Choose an available country preset.");
        if (request.OverridesJson is null || request.OverridesJson.Length > MaxOverrideLength)
            throw new ArgumentException("Provide an environment object within the size limit.");

        var root = JsonNode.Parse(request.OverridesJson, documentOptions: new JsonDocumentOptions
        { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new ArgumentException("Overrides must be a JSON object.");
        RequireUnambiguousKeys(root);
        if (root.Any(p => p.Key.Equals("extends", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Inheritance is selected through the country preset.");
        foreach (var property in root.Where(p => p.Key.Equals("resources", StringComparison.OrdinalIgnoreCase)))
        {
            if (property.Value is not JsonObject resources || resources.Any(p => p.Key.Equals("countryFolders", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Server country folders cannot be selected in company overrides.");
        }
        root["extends"] = "country:" + code;
        // CountryPresets normally resolves an ambient catalog. Fix it to the embedded catalog in this async scope.
        using var scope = OilGasServices.Scope(new OilGasServices { Countries = JsonCountryCatalog.BuiltIn });
        var environment = OilGasEnvironment.FromJson(root.ToJsonString(), out var warnings);
        if (environment.Region is null || environment.Units is null || environment.Region.CoordinateSystems is null ||
            environment.Region.Regulators is null || string.IsNullOrWhiteSpace(environment.Culture))
            throw new ArgumentException("The environment must contain its region, units and culture.");
        if (!string.Equals(environment.Region.Country, code, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The override country must match the selected country preset.");
        return (code, environment, warnings);
    }

    internal static void RequireUnambiguousKeys(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in obj)
            {
                if (!names.Add(property.Key)) throw new ArgumentException("Environment keys must be unique regardless of case.");
                RequireUnambiguousKeys(property.Value);
            }
        }
        else if (node is JsonArray array)
            foreach (var item in array) RequireUnambiguousKeys(item);
    }

    internal static void RequireNoSensitiveReferences(JsonNode? node, bool environmentRoot = false) =>
        RequireNoSensitiveReferencesAt(node, environmentRoot, "");

    private static void RequireNoSensitiveReferencesAt(JsonNode? node, bool environment, string path)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj)
            {
                var fileSettings = environment && path.Length == 0 && property.Key.Equals("files", StringComparison.OrdinalIgnoreCase) && property.Value is JsonObject;
                var wellPathColor = environment && IsWellPathColor(path, property.Key, property.Value);
                if (!fileSettings && !wellPathColor && IsRestrictedSetupProperty(property.Key))
                    throw new ArgumentException("Credentials and external file references must be configured through their approved owners.");
                RequireNoSensitiveReferencesAt(property.Value, environment, path.Length == 0 ? property.Key : path + "." + property.Key);
            }
        }
        else if (node is JsonArray array)
            foreach (var item in array) RequireNoSensitiveReferencesAt(item, environment, path + "[]");
        else if (node is JsonValue value && value.TryGetValue<string>(out var text) && text is not null)
        {
            var lowered = text.ToLowerInvariant();
            if (lowered.Contains("password=") || lowered.Contains("pwd=") || lowered.Contains("secret=") ||
                lowered.Contains("token=") || lowered.Contains("apikey=") || lowered.Contains("api_key=") ||
                Uri.TryCreate(text, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.UserInfo))
                throw new ArgumentException("Credentials must be configured through their approved owner.");
        }
    }

    internal static bool IsWellPathColor(string path, string property, JsonNode? value) =>
        path.Equals("styles.sectionColors", StringComparison.OrdinalIgnoreCase) && property.Equals("WellPath", StringComparison.OrdinalIgnoreCase) &&
        value is JsonValue scalar && scalar.TryGetValue<string>(out var color) && StyleColors.IsColor(color);

    internal static bool IsRestrictedSetupProperty(string property)
    {
        var key = new string(property.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
        return key.Contains("password") || key.Contains("secret") || key.Contains("credential") || key.Contains("apikey") ||
            key.Contains("connectionstring") || key.EndsWith("token", StringComparison.Ordinal) ||
            (key != "profile" && key.EndsWith("file", StringComparison.Ordinal)) || key.EndsWith("files", StringComparison.Ordinal) ||
            key.EndsWith("folder", StringComparison.Ordinal) || key.EndsWith("folders", StringComparison.Ordinal) ||
            key.EndsWith("path", StringComparison.Ordinal) || key.EndsWith("paths", StringComparison.Ordinal);
    }

    internal static void RequireSafeSystemEnvironment(JsonObject root)
    {
        RequireUnambiguousKeys(root);
        if (root.Any(p => p.Key.Equals("extends", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("System settings cannot inherit server files.");
        var inspected = root.DeepClone();
        RequireSafeSourceReferences(inspected, allowMappingOverlay: true);
        RequireNoSensitiveReferences(inspected, true);
    }

    // OilandGasTools calls its host-defined source selector "path". Permit only a Beep module identifier,
    // remove it from the copy inspected for file paths, and leave the saved document intact.
    internal static void RequireSafeSourceReferences(JsonNode? node, bool allowMappingOverlay = false)
    {
        if (node is not JsonObject root) throw new ArgumentException("The environment must be a JSON object.");
        var sourcesProperty = root.FirstOrDefault(p => p.Key.Equals("dataSources", StringComparison.OrdinalIgnoreCase));
        if (sourcesProperty.Key is null) return;
        if (sourcesProperty.Value is not JsonObject sources || sources.Count > 32)
            throw new ArgumentException("Provide at most 32 named Beep sources.");
        foreach (var source in sources)
        {
            if (string.IsNullOrWhiteSpace(source.Key) || source.Key.Length > 64 ||
                source.Key.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.')) || source.Value is not JsonObject definition)
                throw new ArgumentException("A source needs a valid name and definition.");
            var kind = definition.FirstOrDefault(p => p.Key.Equals("kind", StringComparison.OrdinalIgnoreCase));
            var path = definition.FirstOrDefault(p => p.Key.Equals("path", StringComparison.OrdinalIgnoreCase));
            var profile = definition.FirstOrDefault(p => p.Key.Equals("profile", StringComparison.OrdinalIgnoreCase));
            if (allowMappingOverlay && kind.Key is null && path.Key is null && profile.Key is null && definition.Count > 0 &&
                definition.All(p => p.Key.Equals("codes", StringComparison.OrdinalIgnoreCase) || p.Key.Equals("where", StringComparison.OrdinalIgnoreCase)))
                continue;
            if (kind.Value is not JsonValue kindValue || !kindValue.TryGetValue<string>(out var kindText) || kindText != "beep" ||
                path.Key is null || path.Value is not JsonValue pathValue || !pathValue.TryGetValue<string>(out var module) ||
                string.IsNullOrWhiteSpace(module) || module.Length > 64 || module == "SECURITY" ||
                module[0] is < 'A' or > 'Z' || module.Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')) ||
                profile.Value is not JsonValue profileValue || !profileValue.TryGetValue<string>(out var profileText) || profileText != "ppdm39")
                throw new ArgumentException("A source must reference an uppercase Beep module and the built-in ppdm39 profile.");
            definition.Remove(path.Key);
        }
    }

    private static string CountryName(string code)
    {
        if (!JsonCountryCatalog.BuiltIn.TryGet(code, out var json)) throw new ArgumentException("Unknown country preset.");
        return JsonNode.Parse(json)?["region"]?["name"]?.GetValue<string>() ?? code;
    }
}
