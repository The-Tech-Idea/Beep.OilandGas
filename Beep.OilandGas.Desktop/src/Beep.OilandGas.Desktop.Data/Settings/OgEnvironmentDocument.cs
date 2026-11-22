using System.Text.Json;
using System.Text.Json.Nodes;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;

/// <summary>
/// Builds and reads the shared environment document: the country preset, the unit system, and the company's own
/// overrides.
/// </summary>
/// <remarks>
/// <para>
/// The document the database holds is deliberately compact - <c>{"extends":"country:AE","units":{"system":"oilfield"}}</c>
/// plus whatever the company overrides - rather than the resolved environment, which serialises the whole unit
/// registry and runs past a megabyte. The presets ship with the application, so the document records the choice and
/// the engine supplies the values.
/// </para>
/// <para>
/// An explicit override always wins over the preset and over the unit-system choice: a person who typed
/// <c>"units": {"system": "metric-kpa"}</c> meant it, and silently ignoring it would be worse than refusing the
/// document.
/// </para>
/// </remarks>
public static class OgEnvironmentDocument
{
    /// <summary>The key that names the country preset the document extends.</summary>
    public const string ExtendsKey = "extends";

    /// <summary>Result code when the overrides are not a JSON object.</summary>
    public const string CodeOverridesInvalid = "environment-overrides-invalid";

    /// <summary>Result code when no country was chosen.</summary>
    public const string CodeCountryMissing = "environment-country-missing";

    /// <summary>
    /// Composes the document from a country, an optional unit system and optional free-form overrides. Returns a
    /// failure with a sentence a person can act on when the input cannot become a document — nothing is written here.
    /// </summary>
    public static OgResult<string> Compose(
        string? countryCode,
        string? unitSystem,
        string? overridesJson,
        IFailureReporter? failures = null)
    {
        var country = countryCode?.Trim().ToUpperInvariant() ?? string.Empty;

        if (country.Length != 2 || country.Any(character => character is < 'A' or > 'Z'))
        {
            return OgResult<string>.Failed(
                "Choose the country whose preset the environment extends.",
                CodeCountryMissing);
        }

        var root = new JsonObject { [ExtendsKey] = $"country:{country}" };

        if (!string.IsNullOrWhiteSpace(unitSystem))
        {
            root["units"] = new JsonObject { ["system"] = unitSystem.Trim() };
        }

        if (!string.IsNullOrWhiteSpace(overridesJson))
        {
            if (ParseObject(overridesJson!, failures) is not { } overrides)
            {
                return OgResult<string>.Failed(
                    "The overrides must be a JSON object, for example {\"printing\": {\"paper\": \"A3\"}}.",
                    CodeOverridesInvalid);
            }

            Merge(root, overrides);
        }

        return OgResult<string>.Ok(root.ToJsonString());
    }

    /// <summary>
    /// The country, unit system and overrides a stored document names, for prefilling the editor. An unreadable
    /// document reads as empty values rather than throwing.
    /// </summary>
    public static (string? Country, string? UnitSystem, string? OverridesJson) Read(
        string? documentJson,
        IFailureReporter? failures = null)
    {
        if (string.IsNullOrWhiteSpace(documentJson))
        {
            return (null, null, null);
        }

        if (ParseObject(documentJson!, failures) is not { } root)
        {
            return (null, null, null);
        }

        var country = root[ExtendsKey] is JsonValue value
                      && value.TryGetValue<string>(out var extends)
                      && extends.StartsWith("country:", StringComparison.OrdinalIgnoreCase)
            ? extends["country:".Length..]
            : null;

        var unitSystem = root["units"] is JsonObject units && units["system"] is JsonValue system
                         && system.TryGetValue<string>(out var unit)
            ? unit
            : null;

        // Everything that is not the country or the unit system is the company's own override.
        var rest = new JsonObject();

        foreach (var property in root)
        {
            if (string.Equals(property.Key, ExtendsKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(property.Key, "units", StringComparison.OrdinalIgnoreCase)
                && property.Value is JsonObject unitObject)
            {
                var remaining = new JsonObject();

                foreach (var member in unitObject)
                {
                    if (!string.Equals(member.Key, "system", StringComparison.OrdinalIgnoreCase))
                    {
                        remaining[member.Key] = member.Value?.DeepClone();
                    }
                }

                if (remaining.Count > 0)
                {
                    rest["units"] = remaining;
                }

                continue;
            }

            rest[property.Key] = property.Value?.DeepClone();
        }

        return (country, unitSystem, rest.Count == 0 ? null : rest.ToJsonString());
    }

    /// <summary>Merges overrides into the document; an explicit value wins, and objects are merged one level deep.</summary>
    private static void Merge(JsonObject target, JsonObject overrides)
    {
        foreach (var property in overrides)
        {
            if (property.Value is JsonObject incoming
                && target[property.Key] is JsonObject existing)
            {
                foreach (var member in incoming)
                {
                    existing[member.Key] = member.Value?.DeepClone();
                }

                continue;
            }

            target[property.Key] = property.Value?.DeepClone();
        }
    }

    /// <summary>
    /// Parses a JSON object without letting a malformed text reach the caller as an exception; the failure is
    /// recorded when a reporter is supplied, because it means a person's input or a stored record is unreadable.
    /// </summary>
    private static JsonObject? ParseObject(string json, IFailureReporter? failures)
    {
        try
        {
            return JsonNode.Parse(json) as JsonObject;
        }
        catch (JsonException ex)
        {
            failures?.ReportHandled(
                ex,
                "reading an environment document",
                "the document is treated as unreadable",
                FailureSeverity.Degraded);

            return null;
        }
    }
}
