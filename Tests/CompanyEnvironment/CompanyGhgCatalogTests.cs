using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyGhgCatalogTests
{
    private sealed class Companies(CompanyEnvironmentConfiguration saved) : ICompanyEnvironmentStore
    {
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); return saved; }
        public Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority) => throw new NotSupportedException();
    }
    private sealed class Records(SystemSetupConfiguration baseline, SystemSetupConfiguration catalog) : ISystemSetupRecordReader
    {
        public bool Change; private int reads;
        public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
        {
            await authority();
            if (category == "TOOLS") return baseline;
            if (category == "COUNTRY") return null;
            if (category == "GHG_FACTORS") return Change && ++reads > 1 ? null : catalog;
            return null;
        }
    }
    private sealed class Selections(CompanyGhgSelection? value) : ICompanyGhgSelectionStore
    {
        public async Task<CompanyGhgSelection?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); return value; }
        public Task<CompanyGhgSelection> SaveAsync(CompanyScope company, string actor, CompanyGhgSelectionRequest request, Func<Task> authority) => throw new NotSupportedException();
    }
    private static SystemSetupConfiguration Record(string category, string key, string json) =>
        SystemSetupIdentity.Prepare(new(category, key, "1", json, "", Guid.NewGuid().ToString("N")), null);

    [Fact]
    public async Task ReviewUsesTheSelectedCompanyAndRejectsChangingCatalogs()
    {
        var company = new CompanyScope("tenant", "company");
        var baseline = Record("TOOLS", "DEFAULT", "{}");
        var factor = new GhgFactorDefinition("test", "flare", "volume", "mass", 1m, new(2026, 1, 1), new(2026, 12, 31), "source");
        var catalog = Record("GHG_FACTORS", "KW", JsonSerializer.Serialize(new GhgFactorCatalog("KW", [factor])));
        var saved = CompanyEnvironmentIdentity.Prepare(company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var records = new Records(baseline, catalog);
        var reader = new CompanyGhgCatalogReader(new(new Companies(saved), records), records);
        var result = await reader.ReviewAsync(company, "KW", () => Task.CompletedTask);
        Assert.Equal(company, result.Company); Assert.Equal(saved.Revision, result.CompanyRevision); Assert.Equal(catalog, result.Catalog);
        records.Change = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReviewAsync(company, "KW", () => Task.CompletedTask));
        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReviewAsync(new("tenant", "other"), "KW", () => Task.CompletedTask));
    }

    [Fact]
    public async Task SavedSelectionDistinguishesCurrentChangedAndUnavailableInputs()
    {
        var company = new CompanyScope("tenant", "company"); var baseline = Record("TOOLS", "DEFAULT", "{}");
        var factor = new GhgFactorDefinition("test", "flare", "volume", "mass", 1m, new(2026, 1, 1), new(2026, 12, 31), "source");
        var catalog = Record("GHG_FACTORS", "KW", JsonSerializer.Serialize(new GhgFactorCatalog("KW", [factor])));
        var savedEnvironment = CompanyEnvironmentIdentity.Prepare(company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var records = new Records(baseline, catalog); var reader = new CompanyGhgCatalogReader(new(new Companies(savedEnvironment), records), records);
        var review = await reader.ReviewAsync(company, "KW", () => Task.CompletedTask);
        var request = new CompanyGhgSelectionRequest("KW", review.Catalog.Revision, review.EffectiveRevision, "reviewed", "", Guid.NewGuid().ToString("N"));
        var selection = CompanyGhgSelectionIdentity.Prepare(company, "actor", request, null);
        var status = new CompanyGhgSelectionStatusReader(new Selections(selection), reader);
        Assert.Equal("Current", (await status.ReadAsync(company, () => Task.CompletedTask)).Status);
        var stale = CompanyGhgSelectionIdentity.Prepare(company, "actor", request with { CatalogRevision = new string('C', 64) }, null);
        Assert.Equal("ReviewRequired", (await new CompanyGhgSelectionStatusReader(new Selections(stale), reader).ReadAsync(company, () => Task.CompletedTask)).Status);
        records.Change = true;
        Assert.Equal("Unconfirmed", (await status.ReadAsync(company, () => Task.CompletedTask)).Status);
        Assert.Equal("NotSelected", (await new CompanyGhgSelectionStatusReader(new Selections(null), reader).ReadAsync(company, () => Task.CompletedTask)).Status);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => status.ReadAsync(company, () => throw new UnauthorizedAccessException()));
    }
}
