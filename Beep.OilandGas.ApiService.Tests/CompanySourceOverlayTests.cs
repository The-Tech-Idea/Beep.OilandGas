#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json.Nodes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Web.Services;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanySourceOverlayTests
{
    private static SystemSetupConfiguration Baseline(string module) => SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1",
        "{\"dataSources\":{\"well\":{\"kind\":\"beep\",\"path\":\"" + module + "\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"kind\":{\"X\":\"Tubing\"}}}}}}", "", Guid.NewGuid().ToString("N")), null);

    [Fact]
    public void MappingOnlyOverrideInheritsChangesAndCanBeRemoved()
    {
        var json = CompanyEnvironmentOverrideEditor.SetSourceCodeMapping("{}", "well", "string", "kind", "X", "Casing");
        var source = JsonNode.Parse(json)!["dataSources"]!["well"]!;
        Assert.Null(source["path"]); Assert.Null(source["kind"]); Assert.Null(source["profile"]);
        _ = CompanyEnvironmentPreviewer.ValidateForSave(new("tenant", "kw"), new("KW", json, "", Guid.NewGuid().ToString("N")));
        var first = CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", json, Baseline("PPDM_CORE"), null);
        var changed = CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", json, Baseline("PPDM_OTHER"), null);
        var norway = CompanyEnvironmentEffectiveResolver.BuildEnvironment("NO", "{}", Baseline("PPDM_OTHER"), null);
        Assert.Equal("PPDM_CORE", first.DataSources["well"].Path);
        Assert.Equal("PPDM_OTHER", changed.DataSources["well"].Path);
        Assert.Equal("Casing", changed.DataSources["well"].Codes["string"]["kind"]["X"]);
        Assert.Equal("Tubing", norway.DataSources["well"].Codes["string"]["kind"]["X"]);
        Assert.Empty(CompanyEnvironmentSources.ReviewEnvironment(changed));
        json = CompanyEnvironmentOverrideEditor.RemoveSourceCodeMapping(json, "WELL", "STRING", "KIND", "x");
        Assert.Null(JsonNode.Parse(json)!["dataSources"]);
        Assert.Equal("Tubing", CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", json, Baseline("PPDM_OTHER"), null).DataSources["well"].Codes["string"]["kind"]["X"]);
    }

    [Fact]
    public void MissingInheritedSourceAndUnsafePartialIdentityAreRejected()
    {
        var json = CompanyEnvironmentOverrideEditor.SetSourceCodeMapping("{}", "missing", "string", "kind", "X", "Casing");
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", json,
            SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null), null));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.ValidateForSave(new("tenant", "kw"), new("KW", "{\"dataSources\":{\"well\":{\"path\":\"C:/secret\",\"codes\":{}}}}", "", Guid.NewGuid().ToString("N"))));
    }

    [Fact]
    public void ExplicitModuleChangePreservesMappingOnlyOverrides()
    {
        var partial = CompanyEnvironmentOverrideEditor.SetSourceCodeMapping("{}", "well", "string", "kind", "X", "Casing");
        var explicitSource = CompanyEnvironmentOverrideEditor.ChangeBeepSourceModule(partial, "WELL", "PPDM_OTHER");
        var environment = CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", explicitSource, Baseline("PPDM_CORE"), null);
        Assert.Equal("PPDM_OTHER", environment.DataSources["well"].Path);
        Assert.Equal("Casing", environment.DataSources["well"].Codes["string"]["kind"]["X"]);
        var removed = CompanyEnvironmentOverrideEditor.RemoveSourceOverride(explicitSource, "well");
        var inherited = CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", removed, Baseline("PPDM_CORE"), null);
        Assert.Equal("PPDM_CORE", inherited.DataSources["well"].Path);
        Assert.Equal("Tubing", inherited.DataSources["well"].Codes["string"]["kind"]["X"]);
    }
}
#endif
