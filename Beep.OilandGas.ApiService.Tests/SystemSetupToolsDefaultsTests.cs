using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json.Nodes;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.OilGas.Core;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SystemSetupToolsDefaultsTests
{
    [Fact]
    public void DefaultDraftComesFromInstalledToolTemplateAndRemainsCountrySpecific()
    {
        var draft = SystemSetupToolsDefaults.Draft();
        Assert.Equal(CompanyEnvironmentPreviewer.ToolsVersion, draft.ToolsVersion);
        var root = JsonNode.Parse(draft.SettingsJson)!.AsObject();
        CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(root);
        Assert.NotNull(root["units"]); Assert.NotNull(root["survey"]); Assert.NotNull(root["toolboxes"]); Assert.NotNull(root["files"]);
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", draft.ToolsVersion, draft.SettingsJson, "", Guid.NewGuid().ToString("N")), null);
        var kuwait = CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", "{}", baseline, null);
        var norway = CompanyEnvironmentEffectiveResolver.BuildEnvironment("NO", "{}", baseline, null);
        Assert.Equal("KW", kuwait.Region.Country); Assert.Equal("NO", norway.Region.Country);
        Assert.Equal(CompanyEnvironmentPreviewer.Load(new("KW", "{}")).Environment.Units.DepthUnit, kuwait.Units.DepthUnit);
        Assert.Equal(CompanyEnvironmentPreviewer.Load(new("NO", "{}")).Environment.Units.DepthUnit, norway.Units.DepthUnit);
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("print")]
    [InlineData("highContrast")]
    public void GeneratedDefaultsPreserveCompanyThemePresetsAndExplicitColors(string preset)
    {
        var draft = SystemSetupToolsDefaults.Draft();
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", draft.ToolsVersion, draft.SettingsJson, "", Guid.NewGuid().ToString("N")), null);
        var overrides = "{\"theme\":{\"preset\":\"" + preset + "\",\"text\":\"#123456\"}}";
        var expected = CompanyEnvironmentPreviewer.Load(new("KW", overrides)).Environment;
        var actual = CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", overrides, baseline, null);
        Assert.Equal(expected.Theme.TrackBackground, actual.Theme.TrackBackground);
        Assert.Equal(expected.Theme.Text, actual.Theme.Text);
        Assert.Equal(expected.Theme.Surface, actual.Theme.Surface);
        Assert.Equal(expected.Theme.Grid, actual.Theme.Grid);
    }

    [Fact]
    public void ExplicitSharedAndCompanyColorsTakePrecedenceOverPresets()
    {
        var draft = SystemSetupToolsDefaults.Draft();
        var root = JsonNode.Parse(draft.SettingsJson)!.AsObject();
        root["theme"]!["trackBackground"] = "#123456";
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", draft.ToolsVersion, root.ToJsonString(), "", Guid.NewGuid().ToString("N")), null);
        var shared = CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", "{\"theme\":{\"preset\":\"dark\"}}", baseline, null);
        var company = CompanyEnvironmentEffectiveResolver.BuildEnvironment("NO", "{\"theme\":{\"preset\":\"dark\",\"trackBackground\":\"#abcdef\"}}", baseline, null);
        Assert.Equal("#123456", shared.Theme.TrackBackground);
        Assert.Equal("#abcdef", company.Theme.TrackBackground);
        Assert.Equal("#123456", shared.Theme.TrackBackground);
    }

    [Fact]
    public void GeneratedDefaultsPreserveEveryEmbeddedCountryUnitPreset()
    {
        var draft = SystemSetupToolsDefaults.Draft();
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", draft.ToolsVersion, draft.SettingsJson, "", Guid.NewGuid().ToString("N")), null);
        foreach (var code in JsonCountryCatalog.BuiltIn.Codes)
        {
            var expected = CompanyEnvironmentPreviewer.Load(new(code, "{}")).Environment;
            var actual = CompanyEnvironmentEffectiveResolver.BuildEnvironment(code, "{}", baseline, null);
            Assert.True(expected.Units.System == actual.Units.System && expected.Units.DepthUnit == actual.Units.DepthUnit &&
                        expected.Units.PressureUnit == actual.Units.PressureUnit && expected.Units.DiameterUnit == actual.Units.DiameterUnit &&
                        expected.Units.WeightUnit == actual.Units.WeightUnit && expected.Units.ForceUnit == actual.Units.ForceUnit,
                $"The generated defaults changed units for {code}.");
            Assert.Equal(expected.Culture, actual.Culture);
            Assert.Equal(expected.Region.Country, actual.Region.Country);
            var expectedJson = JsonNode.Parse(expected.ToJson())!.AsObject();
            var actualJson = JsonNode.Parse(actual.ToJson())!.AsObject();
            expectedJson.Remove("extends"); actualJson.Remove("extends");
            var differences = expectedJson.Select(x => x.Key).Union(actualJson.Select(x => x.Key))
                .Where(key => !JsonNode.DeepEquals(expectedJson[key], actualJson[key])).ToArray();
            Assert.True(differences.Length == 0, $"The generated defaults changed {code}: {string.Join(", ", differences)}.");
        }
    }

    [Fact]
    public void FileEncodingSettingsAreAllowedButExternalFileReferencesRemainForbidden()
    {
        CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(JsonNode.Parse("{\"files\":{\"legacyEncoding\":\"windows-1256\"}}")!.AsObject());
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(JsonNode.Parse("{\"files\":{\"password\":\"secret\"}}")!.AsObject()));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(JsonNode.Parse("{\"resources\":{\"vocabularyFiles\":[\"outside.json\"]}}")!.AsObject()));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(JsonNode.Parse("{\"files\":[\"outside.json\"]}")!.AsObject()));
        CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(JsonNode.Parse("{\"styles\":{\"sectionColors\":{\"WellPath\":\"#123456\"}}}")!.AsObject());
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(JsonNode.Parse("{\"styles\":{\"sectionColors\":{\"WellPath\":\"C:/outside\"}}}")!.AsObject()));
    }
}
#endif
