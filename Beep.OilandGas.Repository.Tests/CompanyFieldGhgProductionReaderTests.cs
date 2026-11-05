using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyFieldGhgProductionReaderTests
{
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("parent-changed")][InlineData("ignored-filter")]
    public async Task DispositionReviewRequiresTheOriginalFieldAndProductionSource(string mode)
    {
        using var f = await Fixture.Create();
        var review = new FieldSelectionReview(f.Gate.Company.Scope, await f.Gate.ResolveAsync(), "actor", "revision", "A");
        var selections = new Mock<ICompanyFieldSelectionService>(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(new FieldResponse { FieldId = "A", SelectionAvailable = true, SelectionContext = review });
        var access = new Mock<IAccessControlService>(); access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", PermissionConstants.Regulatory.ViewCompliance, null)).ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        var parent = new PDEN { PDEN_SUBTYPE = "WELL", PDEN_ID = "local", SOURCE = "source", FIELD_ID = "A", ACTIVE_IND = "Y" };
        var row = new PDEN_VOL_DISPOSITION { PDEN_SUBTYPE = "WELL", PDEN_ID = "local", PDEN_SOURCE = "source", VOLUME_METHOD = "measured", ACTIVITY_TYPE = "recorded", PRODUCT_TYPE = "gas", PERIOD_TYPE = "month", DISPOSITION_OBS_NO = 1, AMENDMENT_SEQ_NO = 0, VOLUME = 12m, VOLUME_UOM = "unit", ACTIVE_IND = "Y" };
        f.Rows.AddRange([parent, row]);
        var reader = new CompanyFieldGhgProductionReader(f.Gate, selections.Object, access.Object, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object);
        var source = await reader.ReadAsync("actor");
        var query = new GhgDispositionQuery("WELL", mode == "foreign" ? "other" : "local", "source", source.SourceRevision);
        if (mode == "parent-changed") parent.PDEN_LONG_NAME = "changed";
        if (mode == "ignored-filter") { f.IgnoreFilters = true; f.Rows.Add(new PDEN_VOL_DISPOSITION { PDEN_SUBTYPE = "WELL", PDEN_ID = "foreign", PDEN_SOURCE = "source", VOLUME_METHOD = "measured", ACTIVITY_TYPE = "recorded", PRODUCT_TYPE = "gas", PERIOD_TYPE = "month", DISPOSITION_OBS_NO = 1, AMENDMENT_SEQ_NO = 0, VOLUME = 1m, VOLUME_UOM = "unit", ACTIVE_IND = "Y" }); }
        if (mode == "valid")
        {
            var result = await reader.ReadDispositionsAsync("actor", query);
            Assert.Equal(12m, Assert.Single(result.Dispositions).Volume);
            f.Source.Verify(x => x.GetEntityAsync("PDEN_VOL_DISPOSITION", It.Is<List<AppFilter>>(filters => filters.Any(filter => filter.FieldName == "PDEN_ID" && filter.FilterValue == "local"))), Times.Exactly(2));
        }
        else await Assert.ThrowsAsync<RefusalException>(() => reader.ReadDispositionsAsync("actor", query));
        Assert.Empty(f.Writes);
    }
    [Theory][InlineData("valid")][InlineData("denied")][InlineData("ignored-filter")][InlineData("changed")][InlineData("source-changed")]
    public async Task ProductionSourceReadRequiresCompanyFieldAuthorityAndStableEvidence(string mode)
    {
        using var f = await Fixture.Create();
        var review = new FieldSelectionReview(f.Gate.Company.Scope, await f.Gate.ResolveAsync(), "actor", "revision", "A"); var current = review;
        var selections = new Mock<ICompanyFieldSelectionService>(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(() => new FieldResponse { FieldId = "A", SelectionAvailable = true, SelectionContext = current });
        var access = new Mock<IAccessControlService>(); access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", PermissionConstants.Regulatory.ViewCompliance, null)).ReturnsAsync(() => new AccessCheckResponse { HasAccess = mode != "denied" });
        f.Rows.AddRange([new PDEN { PDEN_SUBTYPE = "WELL", PDEN_ID = "local", SOURCE = "source", FIELD_ID = "A", ACTIVE_IND = "Y" },
            new PDEN { PDEN_SUBTYPE = "WELL", PDEN_ID = "foreign", SOURCE = "source", FIELD_ID = "B", ACTIVE_IND = "Y" }]);
        f.IgnoreFilters = mode == "ignored-filter";
        if (mode == "changed") f.AfterRead = () => { current = review with { Revision = "changed" }; return Task.CompletedTask; };
        var reads = 0; if (mode == "source-changed") f.AfterRead = () => { if (++reads == 2) f.Rows.OfType<PDEN>().Single(x => x.PDEN_ID == "local").PDEN_LONG_NAME = "changed"; return Task.CompletedTask; };
        var reader = new CompanyFieldGhgProductionReader(f.Gate, selections.Object, access.Object, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object);
        if (mode == "valid")
        {
            var result = await reader.ReadAsync("actor"); Assert.Equal("local", Assert.Single(result.Entities).ProductionEntityId); Assert.Empty(f.Writes);
            f.Source.Verify(x => x.GetEntityAsync("PDEN", It.Is<List<AppFilter>>(filters => filters.Any(filter => filter.FieldName == "FIELD_ID" && filter.FilterValue == "A"))), Times.Exactly(2));
        }
        else { await Assert.ThrowsAsync<RefusalException>(() => reader.ReadAsync("actor")); Assert.Empty(f.Writes); }
    }
}
