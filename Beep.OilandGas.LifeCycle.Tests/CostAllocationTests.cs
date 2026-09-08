using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.LifeCycle.Services.Accounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.LifeCycle.Tests;

public class CostAllocationTests
{
    [Theory]
    [InlineData(CostAllocationMethod.DirectAllocation)]
    [InlineData(CostAllocationMethod.StepDown)]
    [InlineData(CostAllocationMethod.Reciprocal)]
    [InlineData(CostAllocationMethod.ActivityBasedCosting)]
    public async Task CalculatesNewOperatingAndCapitalAllocationsWithoutWriting(CostAllocationMethod method)
    {
        var (service, source) = Create();
        var result = await service.AllocateCostsAsync("FIELD", new(2026, 9, 1), new(2026, 9, 5), method, "TEST");
        Assert.Equal(120m, result.TotalOperatingCosts);
        Assert.Equal(60m, result.TotalCapitalCosts);
        Assert.Collection(result.AllocationDetails,
            first => { Assert.Equal(45m, first.AllocatedOperatingCost); Assert.Equal(15m, first.AllocatedCapitalCost); },
            second => { Assert.Equal(75m, second.AllocatedOperatingCost); Assert.Equal(45m, second.AllocatedCapitalCost); });
        source.Verify(s => s.GetEntityAsync("COST_TRANSACTION", It.Is<List<AppFilter>>(f =>
            f.Any(x => x.FieldName == "FIELD_ID" && x.FilterValue == "FIELD") &&
            f.Any(x => x.FieldName == "TRANSACTION_DATE" && x.Operator == "<" && x.FilterValue.StartsWith("2026-09-06")))), Times.Once);
        source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        source.Verify(s => s.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("zero")]
    [InlineData("unclassified")]
    [InlineData("amount")]
    public async Task RejectsIncompleteRulesOrSourceCosts(string failure)
    {
        var (service, _) = Create(failure);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AllocateCostsAsync(
            "FIELD", new(2026, 9, 1), new(2026, 9, 5), CostAllocationMethod.DirectAllocation, "TEST"));
    }

    private static (PPDMAccountingService, Mock<IDataSource>) Create(string? failure = null)
    {
        var settings = new Dictionary<string, string?>();
        var ids = new[] { "SUPPORT", "A", "B" };
        for (var i = 0; i < ids.Length && failure != "missing"; i++)
        {
            var prefix = $"CostAllocation:Fields:FIELD:Centers:{i}:";
            settings[prefix + "CostCenterId"] = failure == "foreign" && i == 0 ? "OTHER" : ids[i];
            settings[prefix + "CostCenterType"] = i == 0 ? "SUPPORT" : "REVENUE";
            settings[prefix + "AllocationBasisValue"] = failure == "zero" ? "0" : i == 2 ? "3" : "1";
            settings[prefix + "ActivityUnits"] = i == 2 ? "3" : "1";
        }
        settings["CostAllocation:Fields:FIELD:ActivityBases:0:CostCenterId"] = "SUPPORT";
        settings["CostAllocation:Fields:FIELD:ActivityBases:0:ActivityPercent"] = "1";
        var costs = new List<COST_TRANSACTION>
        {
            new() { COST_CENTER_ID = "SUPPORT", AMOUNT = failure == "amount" ? null : 100m, IS_EXPENSED = failure == "unclassified" ? "N" : "Y" },
            new() { COST_CENTER_ID = "SUPPORT", AMOUNT = 60m, IS_CAPITALIZED = "Y" },
            new() { COST_CENTER_ID = "A", AMOUNT = 20m, IS_EXPENSED = "Y" }
        };
        var source = new Mock<IDataSource>();
        source.Setup(s => s.GetEntityAsync("COST_CENTER", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(ids.Select(id => new COST_CENTER { COST_CENTER_ID = id, COST_CENTER_NAME = id }).ToList());
        source.Setup(s => s.GetEntityAsync("COST_TRANSACTION", It.IsAny<List<AppFilter>>())).ReturnsAsync(costs);
        var editor = new Mock<IDMEEditor>();
        editor.Setup(e => e.GetDataSource("TEST")).Returns(source.Object);
        var common = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var metadata = Mock.Of<IPPDMMetadataRepository>();
        var gl = new GLAccountService(editor.Object, common, defaults, metadata, NullLogger<GLAccountService>.Instance);
        var engine = new CostAllocationService(editor.Object, common, defaults, metadata, gl, NullLogger<CostAllocationService>.Instance);
        return (new PPDMAccountingService(editor.Object, common, defaults, metadata,
            configuration: new ConfigurationBuilder().AddInMemoryCollection(settings).Build(), costAllocationService: engine), source);
    }
}
