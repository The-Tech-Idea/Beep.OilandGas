using System;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.CompressorAnalysis.Core.Interfaces;
using Beep.OilandGas.CompressorAnalysis.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Regression tests for <see cref="CompressorController"/> — parity with packaged calculation cancellation behaviour.
/// </summary>
public class CompressorControllerTests
{
    [Fact]
    public async Task Analyze_RethrowsOperationCanceledException()
    {
        var compressorMock = new Mock<ICompressorAnalysisService>();
        compressorMock
            .Setup(c => c.CalculateCentrifugalPowerAsync(It.IsAny<CENTRIFUGAL_COMPRESSOR_PROPERTIES>()))
            .ThrowsAsync(new OperationCanceledException());

        var controller = new CompressorController(NullLogger<CompressorController>.Instance, compressorMock.Object);

        var request = new COMPRESSOR_OPERATING_CONDITIONS
        {
            SUCTION_PRESSURE = 100m,
            DISCHARGE_PRESSURE = 200m,
            SUCTION_TEMPERATURE = 520m,
            DISCHARGE_TEMPERATURE = 520m,
            GAS_FLOW_RATE = 1000m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
        };

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.Analyze(request));
    }
}
