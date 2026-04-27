using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Operations;
using Beep.OilandGas.Models.Core.Interfaces;
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
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
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
        prospect.Setup(s => s.CreateProspectAsync(It.IsAny<Prospect>(), "SYSTEM"))
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
            "SYSTEM"), Times.Once);
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

        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-42") })) };
        sut.ControllerContext = new ControllerContext { HttpContext = http };

        var actionResult = await sut.CreateProspect(new Prospect { ProspectName = "X", FieldId = "F" }, userId: null);
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
}
