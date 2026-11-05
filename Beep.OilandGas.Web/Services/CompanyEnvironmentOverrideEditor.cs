using System.Text.Json;
using System.Text.Json.Nodes;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public enum EnvironmentSettingsScope { Company, Shared }
public sealed record CompanySourceCodeMapping(string Source, string Entity, string Field, string SourceCode, string CanonicalCode);

/// <summary>Updates explicit display overrides without discarding other company settings.</summary>
public static class CompanyEnvironmentOverrideEditor
{
    /// <summary>Remove common explicit values so system and country settings apply again.</summary>
    public static string UseInheritedCommonSettings(string json)
    {
        if (json is null || json.Length > 65536) throw new ArgumentException("The override document is unavailable or too large.");
        var root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new ArgumentException("The overrides must be an object.");
        RequireUnique(root);
        Remove(root, "culture");
        RemoveSectionFields(root, "locale", "timeZone");
        RemoveSectionFields(root, "units", "system", "depthUnit", "pressureUnit", "diameterUnit", "weightUnit", "forceUnit");
        return root.ToJsonString();
    }

    public static string Apply(string json, string? culture, string? depth, string? pressure, string? diameter, string? timeZone = null,
        string? unitSystem = null, string? weight = null, string? force = null,
        string? displayCulture = null, string? dateFormat = null, string? dateTimeFormat = null, string? calendar = null)
    {
        if (json is null || json.Length > 65536) throw new ArgumentException("The override document is unavailable or too large.");
        var root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new ArgumentException("The overrides must be an object.");
        RequireUnique(root);
        var values = new[] { culture, depth, pressure, diameter, timeZone, unitSystem, weight, force, displayCulture, dateFormat, dateTimeFormat, calendar };
        if (values.Any(x => x is not null && (x.Length > 128 || x.Any(char.IsControl)))) throw new ArgumentException("An override is invalid.");
        if (values.All(string.IsNullOrWhiteSpace)) throw new ArgumentException("Enter at least one override.");
        Set(root, "culture", culture);
        if (new[] { timeZone, displayCulture, dateFormat, dateTimeFormat, calendar }.Any(x => !string.IsNullOrWhiteSpace(x)))
        {
            var localeKey = Find(root, "locale");
            if (localeKey is null) { localeKey = "locale"; root[localeKey] = new JsonObject(); }
            if (root[localeKey] is not JsonObject locale) throw new ArgumentException("The locale section must be an object.");
            Set(locale, "timeZone", timeZone);
            Set(locale, "culture", displayCulture); Set(locale, "dateFormat", dateFormat);
            Set(locale, "dateTimeFormat", dateTimeFormat); Set(locale, "calendar", calendar);
        }
        if (new[] { depth, pressure, diameter, unitSystem, weight, force }.Any(x => !string.IsNullOrWhiteSpace(x)))
        {
            var unitsKey = Find(root, "units");
            if (unitsKey is null) { unitsKey = "units"; root[unitsKey] = new JsonObject(); }
            if (root[unitsKey] is not JsonObject units) throw new ArgumentException("The units section must be an object.");
            Set(units, "depthUnit", depth); Set(units, "pressureUnit", pressure); Set(units, "diameterUnit", diameter);
            Set(units, "system", unitSystem); Set(units, "weightUnit", weight); Set(units, "forceUnit", force);
        }
        var result = root.ToJsonString();
        if (result.Length > 65536) throw new ArgumentException("The override document is too large.");
        return result;
    }
    /// <summary>Remove company display-format overrides without changing the time zone.</summary>
    public static string UseInheritedDisplayFormats(string json)
    {
        if (json is null || json.Length > CompanyEnvironmentIdentity.MaxOverridesLength) throw new ArgumentException("The override document is unavailable or too large.");
        var root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new ArgumentException("The overrides must be an object.");
        RequireUnique(root);
        RemoveSectionFields(root, "locale", "culture", "dateFormat", "dateTimeFormat", "calendar");
        return root.ToJsonString();
    }

    /// <summary>Set company-specific informational region lists without changing other settings.</summary>
    public static string ApplyRegionLists(string json, string? coordinateSystems, string? regulators)
    {
        var root = ReadCompanyObject(json);
        if (string.IsNullOrWhiteSpace(coordinateSystems) && string.IsNullOrWhiteSpace(regulators))
            throw new ArgumentException("Enter at least one coordinate system or regulator.");
        var key = Find(root, "region");
        if (key is null) { key = "region"; root[key] = new JsonObject(); }
        if (root[key] is not JsonObject region) throw new ArgumentException("The region section must be an object.");
        SetList(region, "coordinateSystems", coordinateSystems);
        SetList(region, "regulators", regulators);
        var result = root.ToJsonString();
        if (result.Length > CompanyEnvironmentIdentity.MaxOverridesLength) throw new ArgumentException("The override document is too large.");
        return result;
    }

    /// <summary>Remove one explicit list so its shared or country value is inherited again.</summary>
    public static string UseInheritedRegionList(string json, bool coordinateSystems)
    {
        var root = ReadCompanyObject(json);
        RemoveSectionFields(root, "region", coordinateSystems ? "coordinateSystems" : "regulators");
        return root.ToJsonString();
    }

    private static JsonObject ReadCompanyObject(string json)
    {
        if (json is null || json.Length > CompanyEnvironmentIdentity.MaxOverridesLength) throw new ArgumentException("The override document is unavailable or too large.");
        var root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new ArgumentException("The overrides must be an object.");
        RequireUnique(root);
        return root;
    }

    private static void SetList(JsonObject section, string name, string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        var entries = input.Split('\n').Select(x => x.Trim()).Where(x => x.Length != 0).ToArray();
        if (entries.Length is 0 or > 32 || entries.Any(x => x.Length > 128 || x.Any(char.IsControl)) ||
            entries.Distinct(StringComparer.OrdinalIgnoreCase).Count() != entries.Length)
            throw new ArgumentException("Enter up to 32 distinct entries of at most 128 characters, one per line.");
        section[Find(section, name) ?? name] = new JsonArray(entries.Select(x => (JsonNode?)JsonValue.Create(x)).ToArray());
    }

    /// <summary>Read only explicit company code mappings from the current draft.</summary>
    public static IReadOnlyList<CompanySourceCodeMapping> ReadCompanyCodeMappings(string json)
    {
        var root = ReadCompanyObject(json);
        var sourcesKey = Find(root, "dataSources");
        if (sourcesKey is null) return [];
        if (root[sourcesKey] is not JsonObject sources) throw new ArgumentException("The sources section must be an object.");
        var rows = new List<CompanySourceCodeMapping>();
        foreach (var (sourceName, definition) in sources)
        {
            RequireSourceName(sourceName);
            if (definition is not JsonObject source) throw new ArgumentException("The source must be an object.");
            var codesKey = Find(source, "codes");
            if (codesKey is null) continue;
            if (source[codesKey] is not JsonObject codes) throw new ArgumentException("The code mappings must be an object.");
            foreach (var (entity, fieldsNode) in codes)
            {
                if (!MappingName(entity) || fieldsNode is not JsonObject fields) throw new ArgumentException("The profile entity is invalid.");
                foreach (var (field, valuesNode) in fields)
                {
                    if (!MappingName(field) || valuesNode is not JsonObject values) throw new ArgumentException("The profile field is invalid.");
                    foreach (var (code, value) in values)
                    {
                        if (!MappingValue(code) || value is not JsonValue scalar || !scalar.TryGetValue<string>(out var canonical) || !MappingValue(canonical))
                            throw new ArgumentException("A code mapping is invalid.");
                        rows.Add(new(sourceName, entity, field, code, canonical));
                    }
                }
            }
        }
        return rows.OrderBy(x => x.Source, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.Entity, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Field, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.SourceCode, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Edit a company mapping, including an overlay on an inherited source.</summary>
    public static string SetSourceCodeMapping(string json, string sourceName, string entity, string field, string sourceCode, string canonicalCode)
        => EditSourceCodeMapping(json, sourceName, entity, field, sourceCode, canonicalCode, remove: false);

    /// <summary>Remove an explicit company mapping so an inherited mapping can apply again.</summary>
    public static string RemoveSourceCodeMapping(string json, string sourceName, string entity, string field, string sourceCode)
        => EditSourceCodeMapping(json, sourceName, entity, field, sourceCode, null, remove: true);

    private static string EditSourceCodeMapping(string json, string sourceName, string entity, string field, string sourceCode, string? canonicalCode, bool remove)
    {
        RequireSourceName(sourceName);
        if (!MappingName(entity) || !MappingName(field) || !MappingValue(sourceCode) || (!remove && !MappingValue(canonicalCode)))
            throw new ArgumentException("Enter a valid profile entity, field and code values.");
        var root = ReadCompanyObject(json);
        var sourcesKey = Find(root, "dataSources");
        if (sourcesKey is null && !remove) { sourcesKey = "dataSources"; root[sourcesKey] = new JsonObject(); }
        if (sourcesKey is null || root[sourcesKey] is not JsonObject sources)
            throw new ArgumentException("The source mapping is not defined in the company draft.");
        var sourceKey = Find(sources, sourceName);
        if (sourceKey is null && !remove)
        {
            if (sources.Count >= 32) throw new ArgumentException("The source section is full.");
            sourceKey = sourceName; sources[sourceKey] = new JsonObject();
        }
        if (sourceKey is null || sources[sourceKey] is not JsonObject source)
            throw new ArgumentException("The source mapping is not defined in the company draft.");
        var kindKey = Find(source, "kind"); var profileKey = Find(source, "profile"); var pathKey = Find(source, "path");
        if (kindKey is null && profileKey is null && pathKey is null)
        {
            if (source.Any(p => !p.Key.Equals("codes", StringComparison.OrdinalIgnoreCase) && !p.Key.Equals("where", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("The inherited source override may contain only mappings and filters.");
        }
        else
        {
            if (kindKey is null || profileKey is null || pathKey is null ||
                source[kindKey] is not JsonValue kind || !kind.TryGetValue<string>(out var kindText) || kindText != "beep" ||
                source[profileKey] is not JsonValue profile || !profile.TryGetValue<string>(out var profileText) || profileText != "ppdm39" ||
                source[pathKey] is not JsonValue path || !path.TryGetValue<string>(out var module))
                throw new ArgumentException("The source must use a Beep module and the PPDM profile.");
            RequireSourceIdentity(sourceName, module);
        }
        var codes = MappingSection(source, "codes", !remove);
        var entities = codes is null ? null : MappingSection(codes, entity, !remove);
        var values = entities is null ? null : MappingSection(entities, field, !remove);
        if (values is null) throw new ArgumentException("The mapping is not defined in the company draft.");
        var key = Find(values, sourceCode);
        if (remove)
        {
            if (key is null) throw new ArgumentException("The mapping is not defined in the company draft.");
            values.Remove(key);
            if (values.Count == 0) entities!.Remove(Find(entities, field)!);
            if (entities!.Count == 0) codes!.Remove(Find(codes, entity)!);
            if (codes!.Count == 0) source.Remove(Find(source, "codes")!);
            if (source.Count == 0) sources.Remove(sourceKey);
            if (sources.Count == 0) root.Remove(sourcesKey);
        }
        else
        {
            if (key is null && values.Count >= 256) throw new ArgumentException("The code mapping section is full.");
            values[key ?? sourceCode] = canonicalCode;
        }
        var result = root.ToJsonString();
        if (result.Length > CompanyEnvironmentIdentity.MaxOverridesLength) throw new ArgumentException("The override document is too large.");
        return result;
    }

    private static JsonObject? MappingSection(JsonObject parent, string name, bool create)
    {
        var key = Find(parent, name);
        if (key is null)
        {
            if (!create) return null;
            if (parent.Count >= 256) throw new ArgumentException("The mapping section is full.");
            key = name; parent[key] = new JsonObject();
        }
        return parent[key] as JsonObject ?? throw new ArgumentException("The mapping section must be an object.");
    }

    private static bool MappingName(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');
    private static bool MappingValue(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 &&
        value == value.Trim() && !value.Any(char.IsControl);
    /// <summary>Add a named Beep source without replacing existing source mappings.</summary>
    public static string AddBeepSource(string json, string name, string module, EnvironmentSettingsScope scope = EnvironmentSettingsScope.Company)
    {
        var limit = SizeLimit(scope);
        if (json is null || json.Length > limit) throw new ArgumentException("The settings document is unavailable or too large.");
        RequireSourceIdentity(name, module);
        var root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new ArgumentException("The overrides must be an object.");
        RequireUnique(root);
        var key = Find(root, "dataSources");
        if (key is null) { key = "dataSources"; root[key] = new JsonObject(); }
        if (root[key] is not JsonObject sources || sources.Count >= 32) throw new ArgumentException("The source section is invalid or full.");
        if (Find(sources, name) is not null) throw new ArgumentException("This source already exists in company overrides.");
        sources[name] = new JsonObject { ["kind"] = "beep", ["path"] = module, ["profile"] = "ppdm39" };
        var result = root.ToJsonString();
        if (result.Length > limit) throw new ArgumentException("The settings document is too large.");
        return result;
    }

    /// <summary>Change only the module of an explicit Beep source, preserving its mapping and other settings.</summary>
    public static string ChangeBeepSourceModule(string json, string name, string module, EnvironmentSettingsScope scope = EnvironmentSettingsScope.Company)
    {
        var limit = SizeLimit(scope);
        if (json is null || json.Length > limit) throw new ArgumentException("The settings document is unavailable or too large.");
        RequireSourceIdentity(name, module);
        var root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new ArgumentException("The overrides must be an object.");
        RequireUnique(root);
        var sourcesKey = Find(root, "dataSources");
        if (sourcesKey is null || root[sourcesKey] is not JsonObject sources) throw new ArgumentException("The source section is unavailable.");
        var sourceKey = Find(sources, name);
        if (sourceKey is null || sources[sourceKey] is not JsonObject source) throw new ArgumentException("The source is not defined in this draft.");
        var kindKey = Find(source, "kind"); var profileKey = Find(source, "profile"); var pathKey = Find(source, "path");
        if (scope == EnvironmentSettingsScope.Company && kindKey is null && profileKey is null && pathKey is null &&
            source.Count > 0 && source.All(p => p.Key.Equals("codes", StringComparison.OrdinalIgnoreCase) || p.Key.Equals("where", StringComparison.OrdinalIgnoreCase)))
        {
            source["kind"] = "beep"; source["profile"] = "ppdm39"; source["path"] = module;
        }
        else
        {
            if (kindKey is null || profileKey is null || pathKey is null ||
                source[kindKey] is not JsonValue kind || !kind.TryGetValue<string>(out var kindText) || kindText != "beep" ||
                source[profileKey] is not JsonValue profile || !profile.TryGetValue<string>(out var profileText) || profileText != "ppdm39" ||
                source[pathKey] is not JsonValue path || !path.TryGetValue<string>(out _))
                throw new ArgumentException("The source must use the Beep PPDM profile.");
            source[pathKey] = module;
        }
        var result = root.ToJsonString();
        if (result.Length > limit) throw new ArgumentException("The settings document is too large.");
        return result;
    }

    /// <summary>Remove one explicit source override; inherited sources are resolved by the API.</summary>
    public static string RemoveSourceOverride(string json, string name, EnvironmentSettingsScope scope = EnvironmentSettingsScope.Company)
    {
        var limit = SizeLimit(scope);
        if (json is null || json.Length > limit) throw new ArgumentException("The settings document is unavailable or too large.");
        RequireSourceName(name);
        var root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
            ?? throw new ArgumentException("The overrides must be an object.");
        RequireUnique(root);
        var key = Find(root, "dataSources");
        if (key is null || root[key] is not JsonObject sources) throw new ArgumentException("The source section is unavailable.");
        var sourceKey = Find(sources, name);
        if (sourceKey is null) throw new ArgumentException("The source is not defined in this draft.");
        sources.Remove(sourceKey);
        if (sources.Count == 0) root.Remove(key);
        var result = root.ToJsonString();
        if (result.Length > limit) throw new ArgumentException("The settings document is too large.");
        return result;
    }

    private static int SizeLimit(EnvironmentSettingsScope scope) => scope switch
    {
        EnvironmentSettingsScope.Company => CompanyEnvironmentIdentity.MaxOverridesLength,
        EnvironmentSettingsScope.Shared => SystemSetupIdentity.MaximumSettingsLength,
        _ => throw new ArgumentOutOfRangeException(nameof(scope))
    };

    private static void RequireSourceIdentity(string name, string module)
    {
        RequireSourceName(name);
        if (string.IsNullOrWhiteSpace(module) || module.Length > 64 || module == "SECURITY" || module[0] is < 'A' or > 'Z' ||
            module.Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')))
            throw new ArgumentException("Enter a valid source name and uppercase Beep module.");
    }
    private static void RequireSourceName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 64 || name.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.')))
            throw new ArgumentException("Enter a valid source name.");
    }
    private static string? Find(JsonObject obj, string name) => obj.Select(x => x.Key).SingleOrDefault(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
    private static void Remove(JsonObject obj, string name)
    {
        var key = Find(obj, name);
        if (key is not null) obj.Remove(key);
    }
    private static void RemoveSectionFields(JsonObject root, string name, params string[] fields)
    {
        var key = Find(root, name);
        if (key is null) return;
        if (root[key] is not JsonObject section) throw new ArgumentException($"The {name} section must be an object.");
        foreach (var field in fields) Remove(section, field);
        if (section.Count == 0) root.Remove(key);
    }
    private static void Set(JsonObject obj, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        obj[Find(obj, name) ?? name] = value.Trim();
    }
    private static void RequireUnique(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in obj) { if (!names.Add(property.Key)) throw new ArgumentException("Override names must be unique regardless of case."); RequireUnique(property.Value); }
        }
        else if (node is JsonArray array) foreach (var item in array) RequireUnique(item);
    }
}
