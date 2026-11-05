using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyGhgArithmeticServiceTests
{
    private sealed class Companies(CompanyEnvironmentConfiguration saved) : ICompanyEnvironmentStore
    {
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); return saved; }
        public Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority) => throw new NotSupportedException();
    }
    private sealed class Records(SystemSetupConfiguration baseline, SystemSetupConfiguration catalog) : ISystemSetupRecordReader
    {
        public bool Changed;
        public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
        { await authority(); return category switch { "TOOLS" => baseline, "COUNTRY" => null, "GHG_FACTORS" => Changed ? null : catalog, _ => null }; }
    }
    private sealed class Selections(CompanyGhgSelection selection) : ICompanyGhgSelectionStore
    {
        public async Task<CompanyGhgSelection?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); return selection; }
        public Task<CompanyGhgSelection> SaveAsync(CompanyScope company, string actor, CompanyGhgSelectionRequest request, Func<Task> authority) => throw new NotSupportedException();
    }
    private static SystemSetupConfiguration Record(string category, string key, string json) => SystemSetupIdentity.Prepare(new(category, key, "1", json, "", Guid.NewGuid().ToString("N")), null);

    [Theory][InlineData("valid")][InlineData("denied")][InlineData("catalog-changed")][InlineData("catalog-mid-change")][InlineData("wrong-unit")][InlineData("stale-source")]
    public async Task ArithmeticUsesCurrentCompanyEvidenceAndDoesNotAssumeMissingInputs(string mode)
    {
        using var f = await Fixture.Create(); var company = f.Gate.Company.Scope;
        var field = new FieldSelectionReview(company, await f.Gate.ResolveAsync(), "actor", "revision", "A");
        var fields = new Mock<ICompanyFieldSelectionService>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(new FieldResponse { FieldId = "A", SelectionAvailable = true, SelectionContext = field });
        var access = new Mock<IAccessControlService>(); access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", PermissionConstants.Regulatory.ViewCompliance, null)).ReturnsAsync(new AccessCheckResponse { HasAccess = mode != "denied" });
        var baseline = Record("TOOLS", "DEFAULT", "{}");
        var factor = new GhgFactorDefinition("factor", "flare", "volume", "mass", 1.25m, new(2026, 1, 1), new(2026, 12, 31), "recorded-source");
        var catalog = Record("GHG_FACTORS", "KW", JsonSerializer.Serialize(new GhgFactorCatalog("KW", [factor])));
        var records = new Records(baseline, catalog);
        var environment = CompanyEnvironmentIdentity.Prepare(company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var effective = new CompanyEnvironmentEffectiveResolver(new Companies(environment), records); var catalogs = new CompanyGhgCatalogReader(effective, records);
        var companyReview = await catalogs.ReviewAsync(company, "KW", () => Task.CompletedTask);
        var selection = CompanyGhgSelectionIdentity.Prepare(company, "actor", new("KW", catalog.Revision, companyReview.EffectiveRevision, "reviewed", "", Guid.NewGuid().ToString("N")), null);
        var status = new CompanyGhgSelectionStatusReader(new Selections(selection), catalogs);
        f.Rows.Add(new PDEN { PDEN_SUBTYPE = "WELL", PDEN_ID = "pden", SOURCE = "source", FIELD_ID = "A", ACTIVE_IND = "Y" });
        f.Rows.Add(new PDEN_VOL_DISPOSITION { PDEN_SUBTYPE = "WELL", PDEN_ID = "pden", PDEN_SOURCE = "source", VOLUME_METHOD = "measured", ACTIVITY_TYPE = "flare", PRODUCT_TYPE = "gas", PERIOD_TYPE = "month", DISPOSITION_OBS_NO = 1, AMENDMENT_SEQ_NO = 0, VOLUME = 12m, VOLUME_UOM = mode == "wrong-unit" ? "other" : "volume", VOLUME_START_DATE = new(2026, 2, 1), VOLUME_END_DATE = new(2026, 2, 28), ACTIVE_IND = "Y" });
        var production = new CompanyFieldGhgProductionReader(f.Gate, fields.Object, access.Object, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object);
        var checks = new CompanyGhgArithmeticService(f.Gate, fields.Object, access.Object, status, catalogs, production);
        if (mode == "denied") { await Assert.ThrowsAsync<RefusalException>(() => checks.BasisAsync("actor")); return; }
        var parents = await production.ReadAsync("actor"); var query = new GhgDispositionQuery("WELL", "pden", "source", parents.SourceRevision);
        var dispositions = await production.ReadDispositionsAsync("actor", query);
        var command = new GhgArithmeticRequest(query, mode == "stale-source" ? new string('C', 64) : dispositions.SourceRevision, selection.Revision, "measured", "flare", "gas", "month", 1, 0, "factor");
        if (mode == "catalog-changed") records.Changed = true;
        var reads = 0; if (mode == "catalog-mid-change") f.AfterRead = () => { if (++reads == 3) records.Changed = true; return Task.CompletedTask; };
        if (mode == "valid") { var result = await checks.CheckAsync("actor", command); Assert.Equal(15m, result.Check.Result); Assert.Equal("recorded-source", result.Check.Factor.SourceReference); }
        else if (mode == "wrong-unit") await Assert.ThrowsAsync<InvalidOperationException>(() => checks.CheckAsync("actor", command));
        else await Assert.ThrowsAsync<RefusalException>(() => checks.CheckAsync("actor", command));
        Assert.Empty(f.Writes);
    }
}
