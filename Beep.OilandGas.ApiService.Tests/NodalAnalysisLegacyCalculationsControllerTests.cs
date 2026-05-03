using System;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Regression tests for legacy <c>/api/Calculations/nodal</c> (calculation store integration).
/// </summary>
public class NodalAnalysisLegacyCalculationsControllerTests
{
    [Fact]
    public async Task LegacyPerformNodal_ReturnsBadRequest_WhenBodyMissing()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var actionResult = await controller.PerformNodalAnalysis(request: null!, userId: null);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        calc.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LegacyPerformNodal_Rethrows_WhenCanceled()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.PerformNodalAnalysisAsync(It.IsAny<NodalAnalysisRequest>()))
            .ThrowsAsync(new OperationCanceledException());
        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        var request = new NodalAnalysisRequest
        {
            WellUWI = "UWI-1",
            AnalysisParameters = new NodalAnalysisParameters()
        };

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.PerformNodalAnalysis(request, userId: null));
        calc.VerifyAll();
    }

    [Fact]
    public async Task LegacyGetNodalById_Rethrows_WhenCanceled()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.GetCalculationResultAsync("CALC-1", "NODAL"))
            .ThrowsAsync(new OperationCanceledException());
        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.GetNodalAnalysisResult("CALC-1"));
        calc.VerifyAll();
    }

    [Fact]
    public async Task LegacyGetNodalList_Rethrows_WhenCanceled()
    {
        var calc = new Mock<ICalculationService>(MockBehavior.Strict);
        calc.Setup(s => s.GetCalculationResultsAsync(null, null, null, "NODAL"))
            .ThrowsAsync(new OperationCanceledException());
        var controller = new CalculationsController(
            calc.Object,
            fieldOrchestrator: null,
            progressTracking: null,
            NullLogger<CalculationsController>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.GetNodalAnalysisResults());
        calc.VerifyAll();
    }
}
