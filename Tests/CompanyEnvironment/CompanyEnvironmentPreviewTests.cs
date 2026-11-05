using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdea.OilGas.Core;
using Xunit;

namespace CompanyEnvironment.Tests;

public class CompanyEnvironmentPreviewTests
{
    [Fact]
    public void SavedReviewRevisionsAreBoundToTheCompanyAndDraft()
    {
        var company = new CompanyScope("tenant", "company");
        var request = new CompanyEnvironmentSaveRequest("KW", "{}", "", Guid.NewGuid().ToString("N"));
        var system = new string('A', 64); var country = new string('B', 64);
        var saved = CompanyEnvironmentIdentity.Prepare(company, request, "tool-build", null, system, country);
        Assert.Equal(system, saved.ReviewedSystemRevision); Assert.Equal(country, saved.ReviewedCountryRevision);
        CompanyEnvironmentIdentity.RequireValid(saved, company);
        Assert.Throws<InvalidOperationException>(() => CompanyEnvironmentIdentity.RequireValid(saved with { ReviewedCountryRevision = new string('C', 64) }, company));
        Assert.Throws<InvalidOperationException>(() => CompanyEnvironmentIdentity.RequireValid(saved with { ReviewedSystemRevision = new string('C', 64) }, company));
    }

    [Fact]
    public void CatalogAndPreviewUseRealCountryPresets()
    {
        var countries = CompanyEnvironmentPreviewer.Countries();
        Assert.Contains(countries, c => c.Code == "KW" && c.Name.Contains("Kuwait", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(countries, c => c.Code == "NO");
        var kuwait = CompanyEnvironmentPreviewer.Preview(new("tenant", "kuwait-company"), new("KW", "{}"));
        var norway = CompanyEnvironmentPreviewer.Preview(new("tenant", "norway-company"), new("NO", "{}"));
        Assert.Equal("KW", kuwait.CountryCode);
        Assert.Equal("NO", norway.CountryCode);
        Assert.NotEmpty(kuwait.CoordinateSystems);
        Assert.NotEqual(kuwait.CountryName, norway.CountryName);
    }

    [Fact]
    public async Task OverridesAndAmbientCatalogDoNotLeakAcrossCompanies()
    {
        using var ambient = OilGasServices.Scope(new OilGasServices { Countries = new JsonCountryCatalog(new Dictionary<string, string>()) });
        var first = Task.Run(() => CompanyEnvironmentPreviewer.Preview(new("t", "one"), new("KW", "{\"culture\":\"ar-KW\",\"units\":{\"depthUnit\":\"m\"}}")));
        var second = Task.Run(() => CompanyEnvironmentPreviewer.Preview(new("t", "two"), new("NO", "{\"culture\":\"nb-NO\"}")));
        var results = await Task.WhenAll(first, second);
        Assert.Equal("ar-KW", results[0].Culture);
        Assert.Equal("nb-NO", results[1].Culture);
        Assert.Equal("m", results[0].DepthUnit);
        Assert.Equal("one", results[0].Company.BusinessAssociateId);
        Assert.Empty(OilGasServices.Current.Countries.Codes);
    }

    [Theory]
    [InlineData("ZZ", "{}")]
    [InlineData("KW", "{\"extends\":\"C:/private.json\"}")]
    [InlineData("KW", "{\"ExTeNdS\":\"country:NO\"}")]
    [InlineData("KW", "{\"resources\":{\"countryFolders\":[\"C:/private\"]}}")]
    [InlineData("KW", "{\"resources\":{},\"RESOURCES\":{}}")]
    [InlineData("KW", "{\"region\":{\"country\":\"NO\"}}")]
    public void RejectsUnknownCountriesAndUnsafeOrMismatchedOverrides(string code, string json) =>
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.Preview(new("tenant", "company"), new(code, json)));

    [Fact]
    public void UnknownOverrideKeysAreReported()
    {
        var result = CompanyEnvironmentPreviewer.Preview(new("tenant", "company"), new("KW", "{\"unitz\":{}}"));
        Assert.Contains(result.Warnings, warning => warning.Contains("unitz"));
    }

    [Theory]
    [InlineData("{\"resources\":{\"lexiconFiles\":[\"C:/private.json\"]}}")]
    [InlineData("{\"toolboxes\":{\"x\":{\"apiKey\":\"secret\"}}}")]
    [InlineData("{\"toolboxes\":{\"x\":{\"password\":\"secret\"}}}")]
    [InlineData("{\"toolboxes\":{\"x\":{\"note\":\"password=secret\"}}}")]
    [InlineData("{\"unknownSetting\":1}")]
    public void CannotSaveReferencesSecretsOrUnrecognizedSettings(string json)
    {
        var request = new CompanyEnvironmentSaveRequest("KW", json, "", Guid.NewGuid().ToString("N"));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.ValidateForSave(new("tenant", "company"), request));
    }

    [Fact]
    public void SavedRevisionIsSpecificToCompanyContentAndExpectedRevision()
    {
        var first = new CompanyScope("tenant", "one"); var second = new CompanyScope("tenant", "two");
        var request = new CompanyEnvironmentSaveRequest("KW", "{\"culture\":\"ar-KW\"}", "", Guid.NewGuid().ToString("N"));
        _ = CompanyEnvironmentPreviewer.ValidateForSave(first, request);
        var revision = CompanyEnvironmentIdentity.ForChange(first, request, CompanyEnvironmentPreviewer.ToolsVersion);
        Assert.True(CompanyEnvironmentIdentity.IsRevision(revision));
        Assert.NotEqual(CompanyEnvironmentIdentity.ForCompany(first), CompanyEnvironmentIdentity.ForCompany(second));
        Assert.NotEqual(revision, CompanyEnvironmentIdentity.ForChange(second, request, CompanyEnvironmentPreviewer.ToolsVersion));
        Assert.NotEqual(revision, CompanyEnvironmentIdentity.ForChange(first, request with { ExpectedRevision = revision }, CompanyEnvironmentPreviewer.ToolsVersion));
    }

    [Fact]
    public void ChangesRequireCurrentRevisionAndExactRetry()
    {
        var company = new CompanyScope("tenant", "company");
        var intent = Guid.NewGuid().ToString("N");
        var request = new CompanyEnvironmentSaveRequest("KW", "{}", "", intent);
        var saved = CompanyEnvironmentIdentity.Prepare(company, request, "build", null, new string('A', 64), null);
        Assert.Equal(saved, CompanyEnvironmentIdentity.Prepare(company, request, "build", saved, new string('A', 64), null));
        Assert.Throws<InvalidOperationException>(() => CompanyEnvironmentIdentity.Prepare(company, request with { OverridesJson = "{\"culture\":\"ar-KW\"}" }, "build", saved, new string('A', 64), null));
        Assert.Throws<InvalidOperationException>(() => CompanyEnvironmentIdentity.Prepare(company, request with { IntentId = Guid.NewGuid().ToString("N") }, "build", saved, new string('A', 64), null));
        Assert.Throws<InvalidOperationException>(() => CompanyEnvironmentIdentity.Prepare(new("tenant", "other"), request, "build", saved, new string('A', 64), null));
        var next = request with { ExpectedRevision = saved.Revision, IntentId = Guid.NewGuid().ToString("N") };
        var changed = CompanyEnvironmentIdentity.Prepare(company, next, "build", saved, new string('A', 64), null);
        Assert.NotEqual(saved.Revision, changed.Revision);
        Assert.Equal(saved.Revision, changed.PreviousRevision);
        Assert.Throws<InvalidOperationException>(() => CompanyEnvironmentIdentity.RequireValid(changed with { OverridesJson = "{\"culture\":\"en-US\"}" }, company));
    }

    [Fact]
    public void CompanyCanSaveNamedBeepModuleAndMappingsWithoutAFilePath()
    {
        var json = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"well\":{\"status\":{\"A\":\"Active\"}}}}}}";
        var request = new CompanyEnvironmentSaveRequest("KW", json, "", Guid.NewGuid().ToString("N"));
        var result = CompanyEnvironmentPreviewer.ValidateForSave(new("tenant", "company"), request);
        Assert.Equal("KW", result.CountryCode);
        Assert.Contains("PPDM_CORE", request.OverridesJson);
    }

    [Theory]
    [InlineData("csv", "PPDM_CORE")]
    [InlineData("beep", "../private")]
    [InlineData("beep", "SECURITY")]
    [InlineData("beep", "Server=host;Password=secret")]
    public void CompanySourceCannotReferToAFileSecretOrSecurityStore(string kind, string path)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new { dataSources = new { wells = new { kind, path } } });
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.ValidateForSave(new("tenant", "company"), new("KW", json, "", Guid.NewGuid().ToString("N"))));
    }
}
