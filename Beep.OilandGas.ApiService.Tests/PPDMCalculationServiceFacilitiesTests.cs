using System;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Services.Calculations;
using Beep.OilandGas.CompressorAnalysis.Core.Interfaces;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Facility-scoped packaged calculations in <see cref="PPDMCalculationService"/> (e.g. pipeline alongside compressor).
/// </summary>
public class PPDMCalculationServiceFacilitiesTests
{
    private static PPDMCalculationService CreateService()
    {
        return new PPDMCalculationService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            new Mock<IPPDMMetadataRepository>().Object,
            new Mock<IFieldMappingService>().Object,
            new Mock<IChokeAnalysisService>().Object,
            new Mock<ICompressorAnalysisService>().Object,
            connectionName: "PPDM39",
            logger: null);
    }

    [Fact]
    public async Task PerformPipelineAnalysisAsync_NullRequest_ThrowsArgumentNullException()
    {
        var svc = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => svc.PerformPipelineAnalysisAsync(null!));
    }
}
