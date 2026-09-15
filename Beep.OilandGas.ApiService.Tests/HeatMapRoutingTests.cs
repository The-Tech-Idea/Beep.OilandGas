using Beep.OilandGas.HeatMap.Services;
using Beep.OilandGas.HeatMap.Modules;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HeatMap;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class HeatMapRoutingTests
{
    [Fact]
    public async Task ReadsResolveCurrentBindingForEveryOperation()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var sources = new List<Mock<IDataSource>>();
        foreach (var target in new[] { "first", "second" })
        {
            var source = new Mock<IDataSource>(MockBehavior.Strict);
            sources.Add(source);
            source.Setup(x => x.GetEntityAsync("HEAT_MAP_CONFIGURATION", It.Is<List<AppFilter>>(filters =>
                filters.Count == 2 && filters.Any(f => f.FieldName == "HEAT_MAP_ID" && f.FilterValue == "config" && f.Operator == "=") &&
                filters.Any(f => f.FieldName == "ACTIVE_IND" && f.FilterValue == "Y" && f.Operator == "="))))
                .ReturnsAsync(new List<HEAT_MAP_CONFIGURATION>
                {
                    new() { HEAT_MAP_ID = "config", CONFIGURATION_NAME = target, ACTIVE_IND = "Y" }
                });
            editor.Setup(x => x.GetDataSource(target)).Returns(source.Object);
        }
        var binding = "first";
        var resolves = 0;
        var service = Create(editor.Object, () => { resolves++; return Task.FromResult(binding); });
        Assert.Equal("first", (await service.GetHeatMapConfigurationAsync("config"))!.ConfigurationName);
        binding = "second";
        Assert.Equal("second", (await service.GetHeatMapConfigurationAsync("config"))!.ConfigurationName);
        Assert.Equal(2, resolves);
        editor.Verify(x => x.GetDataSource("first"), Times.Once);
        editor.Verify(x => x.GetDataSource("second"), Times.Once);
        foreach (var source in sources) source.VerifyAll();
        editor.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task MissingBindingRejectsReadsAndWritesBeforeOpeningDataSource(string binding)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, () => Task.FromResult(binding));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetHeatMapConfigurationAsync("config"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveHeatMapConfigurationAsync(new(), "user"));
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public void ModuleOwnsConfigurationTableWithOneDeclaredKeyAndNoShadowColumns()
    {
        var module = new HeatMapModule();
        Assert.Equal("HEAT_MAP", module.ModuleId);
        var entity = Assert.Single(module.EntityTypes);
        Assert.Equal(typeof(HEAT_MAP_CONFIGURATION), entity);
        Assert.Single(entity.GetProperties().Where(property => Attribute.IsDefined(property,
            typeof(System.ComponentModel.DataAnnotations.KeyAttribute))));
        Assert.Single(entity.GetProperties().Where(property => property.Name == "ACTIVE_IND"));
    }

    private static HeatMapService Create(IDMEEditor editor, Func<Task<string>> resolve) => new(editor,
        Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
        Mock.Of<IPPDMMetadataRepository>(), resolve);
}
