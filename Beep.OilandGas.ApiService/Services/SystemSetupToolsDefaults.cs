using System.Text.Json.Nodes;
using TheTechIdea.Data.OilGas;
using TheTechIdea.OilGas.Core;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Builds a reviewable, safe shared draft from this runtime's actual tool defaults.</summary>
public static class SystemSetupToolsDefaults
{
    public static SystemSetupToolsDefaultDraft Draft()
    {
        var root = JsonNode.Parse(OilGasEnvironment.Template()) as JsonObject
            ?? throw new InvalidOperationException("The OilandGasTools template is unavailable.");
        if (root.TryGetPropertyValue("extends", out var extends) && !Empty(extends))
            throw new InvalidOperationException("The OilandGasTools template contains external inheritance.");
        root.Remove("extends");
        RemoveEmptyRestrictedDefaults(root);
        // These values are derived by OilandGasTools from units.system. Keeping
        // the template's generic values as explicit overrides would suppress a
        // country's metric or field-unit preset. Administrators may still add
        // an explicit unit after reviewing the draft.
        if (root["units"] is JsonObject units)
            foreach (var name in new[] { "depthUnit", "pressureUnit", "diameterUnit", "weightUnit", "forceUnit" })
                units.Remove(name);
        // Template colors are ordinary defaults, not administrator overrides.
        // Leave preset-controlled colors implicit so a later country or company
        // theme preset can supply them. Explicitly saved colors still win.
        if (root["theme"] is JsonObject theme)
        {
            var presetKeys = OilGasEnvironment.ThemePresets.Values.SelectMany(x => x.Keys).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var key in theme.Select(x => x.Key).Where(presetKeys.Contains).ToArray())
                theme.Remove(key);
        }
        CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(root);
        var json = root.ToJsonString();
        if (json.Length > SystemSetupIdentity.MaximumSettingsLength)
            throw new InvalidOperationException("The OilandGasTools template exceeds the shared setup limit.");
        return new(CompanyEnvironmentPreviewer.ToolsVersion, json);
    }

    private static bool Empty(JsonNode? value) => value is null || value is JsonArray { Count: 0 } ||
        value is JsonValue scalar && scalar.TryGetValue<string>(out var text) && string.IsNullOrWhiteSpace(text);

    private static void RemoveEmptyRestrictedDefaults(JsonNode? node, string path = "")
    {
        if (node is JsonObject obj)
        {
            foreach (var item in obj.ToArray())
            {
                var fileSettings = path.Length == 0 && item.Key.Equals("files", StringComparison.OrdinalIgnoreCase) && item.Value is JsonObject;
                var wellPathColor = CompanyEnvironmentPreviewer.IsWellPathColor(path, item.Key, item.Value);
                if (!fileSettings && !wellPathColor && CompanyEnvironmentPreviewer.IsRestrictedSetupProperty(item.Key))
                {
                    if (!Empty(item.Value)) throw new InvalidOperationException($"Tool default field '{item.Key}' requires an approved owner.");
                    obj.Remove(item.Key);
                }
                else RemoveEmptyRestrictedDefaults(item.Value, path.Length == 0 ? item.Key : path + "." + item.Key);
            }
        }
        else if (node is JsonArray array) foreach (var value in array) RemoveEmptyRestrictedDefaults(value, path + "[]");
    }
}
