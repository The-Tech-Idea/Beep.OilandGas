using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Operations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Operations;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Legacy <c>/api/prospect/*</c> and core <c>api/ProspectIdentification</c> routes (not workflow POSTs).
/// </summary>
public class ProspectIdentificationControllerCompatibilityTests
{
    private static ProspectIdentificationController CreateSut(out Mock<IProspectIdentificationService> prospectService)
    {
        prospectService = new Mock<IProspectIdentificationService>();
        var technical = new Mock<IProspectTechnicalMaturationService>();
        var riskEconomic = new Mock<IProspectRiskEconomicAnalysisService>();
        var portfolio = new Mock<IProspectPortfolioOptimizationService>();
        var sut = new ProspectIdentificationController(
            prospectService.Object,
            technical.Object,
            riskEconomic.Object,
            portfolio.Object,
            NullLogger<ProspectIdentificationController>.Instance);
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "user-42")], "TestAuth"))
            }
        };
        return sut;
    }

    [Fact]
    public async Task IdentifyProspectCompatibility_NullBody_ReturnsBadRequest()
    {
        var sut = CreateSut(out var prospect);
        var result = await sut.IdentifyProspectCompatibility(null!);
        Assert.IsType<BadRequestObjectResult>(result.Result);
        prospect.Verify(
            s => s.CreateProspectAsync(It.IsAny<Prospect>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task IdentifyProspectCompatibility_CreatesProspect_ReturnsOkWithId()
    {
        var sut = CreateSut(out var prospect);
        prospect.Setup(s => s.CreateProspectAsync(It.IsAny<Prospect>(), "user-42"))
            .ReturnsAsync("new-prospect-id");

        var body = new PROSPECT
        {
            PROSPECT_NAME = "Wildcat A",
            PRIMARY_FIELD_ID = "FIELD-1",
            PROSPECT_STATUS = "Draft"
        };

        var actionResult = await sut.IdentifyProspectCompatibility(body);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returned = Assert.IsType<PROSPECT>(ok.Value);
        Assert.Equal("new-prospect-id", returned.PROSPECT_ID);
        Assert.Equal("Draft", returned.PROSPECT_STATUS);
        prospect.Verify(s => s.CreateProspectAsync(
            It.Is<Prospect>(p => p.ProspectName == "Wildcat A" && p.FieldId == "FIELD-1"),
            "user-42"), Times.Once);
    }

    [Fact]
    public async Task GetProspectCompatibility_EmptyId_ReturnsBadRequest()
    {
        var sut = CreateSut(out var prospect);
        var result = await sut.GetProspectCompatibility("   ");
        Assert.IsType<BadRequestObjectResult>(result.Result);
        prospect.Verify(s => s.GetProspectsAsync(It.IsAny<Dictionary<string, string>>()), Times.Never);
    }

    [Fact]
    public async Task GetProspectCompatibility_WhenMissing_ReturnsNotFound()
    {
        var sut = CreateSut(out var prospect);
        prospect.Setup(s => s.GetProspectsAsync(It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new List<Prospect>());

        var result = await sut.GetProspectCompatibility("missing-id");
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetProspectCompatibility_WhenFound_ReturnsOk()
    {
        var sut = CreateSut(out var prospect);
        prospect.Setup(s => s.GetProspectsAsync(It.Is<Dictionary<string, string>>(d =>
                    d.ContainsKey("PROSPECT_ID") && d["PROSPECT_ID"] == "P-1")))
            .ReturnsAsync(new List<Prospect>
            {
                new()
                {
                    ProspectId = "P-1",
                    ProspectName = "Alpha",
                    FieldId = "F-9",
                    Status = "EVALUATED"
                }
            });

        var actionResult = await sut.GetProspectCompatibility("P-1");
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var legacy = Assert.IsType<PROSPECT>(ok.Value);
        Assert.Equal("P-1", legacy.PROSPECT_ID);
        Assert.Equal("Alpha", legacy.PROSPECT_NAME);
        Assert.Equal("F-9", legacy.PRIMARY_FIELD_ID);
    }

    [Fact]
    public async Task EvaluateRiskCompatibility_WithProspectId_DelegatesToEvaluate()
    {
        var sut = CreateSut(out var prospect);
        prospect.Setup(s => s.EvaluateProspectAsync("P-2")).ReturnsAsync(new ProspectEvaluation
        {
            ProspectId = "P-2",
            EvaluationId = "E-1",
            RiskScore = 0.4m,
            RiskLevel = "MEDIUM",
            ProbabilityOfSuccess = 0.55m,
            EstimatedOilResources = 10m,
            EstimatedGasResources = 2m,
            ResourceUnit = "MMBBL"
        });

        var actionResult = await sut.EvaluateRiskCompatibility(new PROSPECT { PROSPECT_ID = "P-2" });
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var risk = Assert.IsType<PROSPECT_RISK_ASSESSMENT>(ok.Value);
        Assert.Equal("P-2", risk.PROSPECT_ID);
        Assert.Equal("E-1", risk.ASSESSMENT_ID);
        prospect.Verify(s => s.EvaluateProspectAsync("P-2"), Times.Once);
    }

    [Fact]
    public async Task EvaluateRiskCompatibility_WithoutProspectId_UsesRequestOnly()
    {
        var sut = CreateSut(out var prospect);
        var actionResult = await sut.EvaluateRiskCompatibility(new PROSPECT
        {
            PROSPECT_ID = "",
            RISK_LEVEL = "LOW",
            ESTIMATED_OIL_VOLUME = 100m,
            DESCRIPTION = "Desk review"
        });
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var risk = Assert.IsType<PROSPECT_RISK_ASSESSMENT>(ok.Value);
        Assert.True(risk.TRAP_RISK < 0.5m);
        prospect.Verify(s => s.EvaluateProspectAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetProspects_Delegates_ReturnsOk()
    {
        var sut = CreateSut(out var prospect);
        var list = new List<Prospect> { new() { ProspectId = "A", ProspectName = "One" } };
        prospect.Setup(s => s.GetProspectsAsync(null)).ReturnsAsync(list);

        var actionResult = await sut.GetProspects(null);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(list, ok.Value);
    }

    [Fact]
    public async Task CreateProspect_Delegates_ReturnsOk()
    {
        var sut = CreateSut(out var prospect);
        prospect.Setup(s => s.CreateProspectAsync(It.IsAny<Prospect>(), "user-42"))
            .ReturnsAsync("created-id");

        var actionResult = await sut.CreateProspect(new Prospect { ProspectName = "X", FieldId = "F" });
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(ok.Value);
        prospect.Verify(s => s.CreateProspectAsync(It.IsAny<Prospect>(), "user-42"), Times.Once);
    }

    [Fact]
    public async Task EvaluateProspect_ReturnsOk()
    {
        var sut = CreateSut(out var prospect);
        var evaluation = new ProspectEvaluation { ProspectId = "P-9", EvaluationId = "EV-1" };
        prospect.Setup(s => s.EvaluateProspectAsync("P-9")).ReturnsAsync(evaluation);

        var actionResult = await sut.EvaluateProspect("P-9");
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(evaluation, ok.Value);
    }

    [Fact]
    public async Task RankProspects_Delegates_ReturnsOk()
    {
        var sut = CreateSut(out var prospect);
        var rankings = new List<ProspectRanking> { new() { ProspectId = "A", Rank = 1 } };
        prospect.Setup(s => s.RankProspectsAsync(
                It.Is<List<string>>(ids => ids.SequenceEqual(new[] { "A", "B" })),
                It.IsAny<Dictionary<string, decimal>>()))
            .ReturnsAsync(rankings);

        var actionResult = await sut.RankProspects(new RankProspectsRequest
        {
            ProspectIds = new List<string> { "A", "B" },
            RankingCriteria = new Dictionary<string, decimal> { ["w"] = 1m }
        });
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(rankings, ok.Value);
    }

    private static void ProspectExists(Mock<IProspectIdentificationService> prospect, string prospectId) =>
        prospect.Setup(s => s.GetProspectsAsync(It.Is<Dictionary<string, string>>(d =>
                d.ContainsKey("PROSPECT_ID") && d["PROSPECT_ID"] == prospectId)))
            .ReturnsAsync(new List<Prospect>
            {
                // A single resource figure: the endpoint had derived P10 and P90 from it as +/-20%.
                new() { ProspectId = prospectId, ProspectName = "Alpha", EstimatedResources = 100m, RiskLevel = "LOW" }
            });

    [Fact]
    public async Task GetVolumetricsCompatibility_AnswersTheLatestRecordedEstimate_NotOneDerivedFromTheProspect()
    {
        var sut = CreateSut(out var prospect);
        ProspectExists(prospect, "P-1");
        var recorded = new PROSPECT_VOLUME_ESTIMATE
        {
            PROSPECT_ID = "P-1",
            ESTIMATE_ID = "EST-2",
            OIL_VOLUME_P90 = 40m,
            OIL_VOLUME_P50 = 100m,
            OIL_VOLUME_P10 = 210m,
            ESTIMATE_METHOD = "MONTE_CARLO",
            ESTIMATE_DATE = new DateTime(2026, 9, 1)
        };
        prospect.Setup(s => s.GetVolumeEstimatesAsync("P-1"))
            .ReturnsAsync(new List<PROSPECT_VOLUME_ESTIMATE> { recorded, new() { PROSPECT_ID = "P-1", ESTIMATE_ID = "EST-1" } });

        var actionResult = await sut.GetVolumetricsCompatibility("P-1");

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var estimate = Assert.IsType<PROSPECT_VOLUME_ESTIMATE>(ok.Value);
        Assert.Same(recorded, estimate);
        Assert.Equal(40m, estimate.OIL_VOLUME_P90);
        Assert.Equal(210m, estimate.OIL_VOLUME_P10);
    }

    [Fact]
    public async Task GetVolumetricsCompatibility_WhenNoEstimateIsRecorded_RefusesNotFound()
    {
        var sut = CreateSut(out var prospect);
        ProspectExists(prospect, "P-1");
        prospect.Setup(s => s.GetVolumeEstimatesAsync("P-1")).ReturnsAsync(new List<PROSPECT_VOLUME_ESTIMATE>());

        var refusal = await Assert.ThrowsAsync<RefusalException>(() => sut.GetVolumetricsCompatibility("P-1"));

        Assert.Equal(RefusalKind.NotFound, refusal.Kind);
    }

    [Fact]
    public async Task GetVolumeEstimates_AnswersEveryRecordedEstimate_AndAnEmptyListWhenNone()
    {
        var sut = CreateSut(out var prospect);
        ProspectExists(prospect, "P-1");
        var estimates = new List<PROSPECT_VOLUME_ESTIMATE> { new() { PROSPECT_ID = "P-1", ESTIMATE_ID = "EST-1" } };
        prospect.SetupSequence(s => s.GetVolumeEstimatesAsync("P-1"))
            .ReturnsAsync(estimates)
            .ReturnsAsync(new List<PROSPECT_VOLUME_ESTIMATE>());

        var first = Assert.IsType<OkObjectResult>((await sut.GetVolumeEstimates("P-1")).Result);
        Assert.Same(estimates, first.Value);

        var none = Assert.IsType<OkObjectResult>((await sut.GetVolumeEstimates("P-1")).Result);
        Assert.Empty(Assert.IsType<List<PROSPECT_VOLUME_ESTIMATE>>(none.Value));
    }

    [Fact]
    public async Task GetVolumeEstimates_ForAnUnknownProspect_ReturnsNotFound()
    {
        var sut = CreateSut(out var prospect);
        prospect.Setup(s => s.GetProspectsAsync(It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new List<Prospect>());

        var result = await sut.GetVolumeEstimates("missing-id");

        Assert.IsType<NotFoundObjectResult>(result.Result);
        prospect.Verify(s => s.GetVolumeEstimatesAsync(It.IsAny<string>()), Times.Never);
    }
}
