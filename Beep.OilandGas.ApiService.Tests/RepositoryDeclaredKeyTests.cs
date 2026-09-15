using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryDeclaredKeyTests
{
    [Fact]
    public async Task ExtensionKeyLookupDoesNotRequireThePpdmCatalogOrIdFormatter()
    {
        var row = new PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "instance" };
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("PROCESS_INSTANCE", It.Is<List<AppFilter>>(filters => filters.Count == 1 &&
            filters[0].FieldName == "PROCESS_INSTANCE_ID" && filters[0].FilterValue == "instance" && filters[0].Operator == "=")))
            .ReturnsAsync(new List<PROCESS_INSTANCE> { row });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("selected")).Returns(source.Object);
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var repo = new PPDMGenericRepository(editor.Object, Mock.Of<ICommonColumnHandler>(), defaults.Object,
            metadata.Object, typeof(PROCESS_INSTANCE), "selected", "PROCESS_INSTANCE");
        Assert.Same(row, await repo.GetByIdAsync("instance"));
        source.VerifyAll();
        defaults.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CompositeKeyLookupRequiresExplicitFilters()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var repo = new PPDMGenericRepository(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            typeof(R_LIFECYCLE_STATE_REFERENCE), "selected", "R_LIFECYCLE_STATE_REFERENCE");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetByIdAsync("ambiguous"));
        Assert.Contains("composite key", error.Message);
        editor.VerifyNoOtherCalls();
    }
}
