using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.LeaseAcquisition.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class LeaseRoutingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LeaseReadsRequireCoreBindingBeforeAnyRepositoryAccess(bool evaluate)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var calls = 0;
        var access = new Mock<ICompanyLeaseDataAccess>(MockBehavior.Strict);
        access.Setup(value => value.ResolveAsync()).Returns(() => { calls++; throw new InvalidOperationException("A current company module binding is required."); });
        ILeaseAcquisitionService service = new LeaseAcquisitionService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), access.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => evaluate
            ? (Task)service.EvaluateLeaseAsync("lease", "A") : service.GetAvailableLeasesAsync(new() { ["FieldId"] = "A" }));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
    }
}
