using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.LeaseAcquisition.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

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
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        ILeaseAcquisitionService service = new LeaseAcquisitionService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "global-db", resolveConnection: Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => evaluate
            ? (Task)service.EvaluateLeaseAsync("lease") : service.GetAvailableLeasesAsync(null));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
    }
}
