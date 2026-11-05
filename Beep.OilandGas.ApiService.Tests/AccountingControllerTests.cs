using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class AccountingControllerTests
{
    [Fact]
    public async Task GetProductionSummaryAsync_ReturnsOk_WhenAccountingStatusIsAvailable()
    {
        const string fieldId = "FIELD-PA-1";

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns(fieldId);

        var accountingService = new Mock<IAccountingService>(MockBehavior.Loose);
        var productionAccounting = new Mock<IProductionAccountingService>(MockBehavior.Strict);
        productionAccounting
            .Setup(x => x.GetAccountingStatusAsync(fieldId, It.IsAny<DateTime?>(), "PPDM39"))
            .ReturnsAsync(new AccountingStatusData
            {
                FieldId = fieldId,
                TotalRevenue = 1000m,
                TotalCosts = 200m,
                TotalRoyalty = 100m,
                NetIncome = 700m,
                PeriodStatus = "OPEN"
            });

        var controller = CreateController(orchestrator.Object, accountingService.Object, productionAccounting.Object, "user-1");

        var result = await controller.GetProductionSummaryAsync();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ProductionAccountingSummaryDto>(ok.Value);
        productionAccounting.VerifyAll();
        orchestrator.VerifyAll();
    }

    [Fact]
    public async Task GetRevenueLinesAsync_ReturnsOk_WhenTransactionsExist()
    {
        const string fieldId = "FIELD-PA-2";

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns(fieldId);

        var accountingService = new Mock<IAccountingService>(MockBehavior.Loose);
        var productionAccounting = new Mock<IProductionAccountingService>(MockBehavior.Strict);
        productionAccounting
            .Setup(x => x.GetRevenueTransactionsAsync(
                fieldId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                "PPDM39"))
            .ReturnsAsync(
            [
                new REVENUE_TRANSACTION
                {
                    REVENUE_TRANSACTION_ID = "REV-1",
                    WELL_ID = "WELL-7",
                    TRANSACTION_DATE = new DateTime(2026, 2, 14),
                    DESCRIPTION = "Oil sale",
                    REVENUE_TYPE = "OIL",
                    OIL_VOLUME = 10m,
                    OIL_PRICE = 80m,
                    NET_REVENUE = 800m
                }
            ]);

        var controller = CreateController(orchestrator.Object, accountingService.Object, productionAccounting.Object, "user-2");

        var result = await controller.GetRevenueLinesAsync();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var lines = Assert.IsType<List<RevenueLine>>(ok.Value);
        var line = Assert.Single(lines);
        // The revenue recognition page groups the year by month and lists each line's well from these.
        Assert.Equal(new DateTime(2026, 2, 14), line.TransactionDate);
        Assert.Equal("WELL-7", line.WellId);
        productionAccounting.VerifyAll();
        orchestrator.VerifyAll();
    }

    [Fact]
    public async Task GetCloseReadinessAsync_AnswersReady_WithNothingOutstanding_WhenTheChecksPass()
    {
        const string fieldId = "FIELD-PA-5";

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns(fieldId);
        var periodClosing = new Mock<IPeriodClosingService>(MockBehavior.Strict);
        periodClosing
            .Setup(x => x.ValidateReadinessAsync(fieldId, new DateTime(2026, 3, 31), "PPDM39"))
            .ReturnsAsync(true);
        periodClosing
            .Setup(x => x.GetPeriodStatusAsync(fieldId, new DateTime(2026, 3, 31)))
            .ReturnsAsync(PeriodCloseStatusCodes.Open);

        var controller = CreateController(
            orchestrator.Object, new Mock<IAccountingService>().Object, new Mock<IProductionAccountingService>(MockBehavior.Strict).Object, "user-5");

        var result = await controller.GetCloseReadinessAsync(new DateTime(2026, 3, 31, 15, 0, 0), periodClosing.Object);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var readiness = Assert.IsType<PeriodCloseReadinessResult>(ok.Value);
        Assert.True(readiness.Ready);
        Assert.Empty(readiness.OutstandingItems);
        Assert.Equal(new DateTime(2026, 3, 31), readiness.PeriodEnd);
        Assert.Equal(PeriodCloseStatusCodes.Open, readiness.PeriodStatus);
        periodClosing.VerifyAll();
        periodClosing.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCloseReadinessAsync_SaysTheStatusTheCloseRecognizes_WhenThePeriodIsClosed()
    {
        const string fieldId = "FIELD-PA-8";
        var periodEnd = new DateTime(2026, 5, 31);

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns(fieldId);
        var periodClosing = new Mock<IPeriodClosingService>(MockBehavior.Strict);
        periodClosing.Setup(x => x.ValidateReadinessAsync(fieldId, periodEnd, "PPDM39")).ReturnsAsync(false);
        periodClosing.Setup(x => x.GetPeriodStatusAsync(fieldId, periodEnd)).ReturnsAsync(PeriodCloseStatusCodes.Closed);
        periodClosing
            .Setup(x => x.GetUnreconciledItemsAsync(fieldId, periodEnd, "PPDM39"))
            .ReturnsAsync(["PERIOD: the period ending 2026-05-31 is already closed"]);

        var controller = CreateController(
            orchestrator.Object, new Mock<IAccountingService>().Object, new Mock<IProductionAccountingService>(MockBehavior.Strict).Object, "user-8");

        var result = await controller.GetCloseReadinessAsync(periodEnd, periodClosing.Object);

        var readiness = Assert.IsType<PeriodCloseReadinessResult>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.False(readiness.Ready);
        Assert.Equal(PeriodCloseStatusCodes.Closed, readiness.PeriodStatus);
        periodClosing.VerifyAll();
    }

    [Fact]
    public async Task GetPeriodClosesAsync_AnswersTheCurrentFieldsCloses()
    {
        const string fieldId = "FIELD-PA-9";
        var closes = new List<PeriodCloseSummary>
        {
            new() { FieldId = fieldId, PeriodEnd = new DateTime(2026, 4, 30), Status = PeriodCloseStatusCodes.Closed, JournalEntryNumber = "JE-2" },
            new() { FieldId = fieldId, PeriodEnd = new DateTime(2026, 3, 31), Status = PeriodCloseStatusCodes.Reopened, JournalEntryNumber = "JE-1" }
        };

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns(fieldId);
        var periodClosing = new Mock<IPeriodClosingService>(MockBehavior.Strict);
        periodClosing.Setup(x => x.GetPeriodClosesAsync(fieldId)).ReturnsAsync(closes);

        var controller = CreateController(
            orchestrator.Object, new Mock<IAccountingService>().Object, new Mock<IProductionAccountingService>(MockBehavior.Strict).Object, "user-9");

        var result = await controller.GetPeriodClosesAsync(periodClosing.Object);

        Assert.Same(closes, Assert.IsType<OkObjectResult>(result.Result).Value);
        periodClosing.VerifyAll();
        periodClosing.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPeriodClosesAsync_Refuses_WhenNoFieldIsSelected()
    {
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns((string?)null);
        var periodClosing = new Mock<IPeriodClosingService>(MockBehavior.Strict);

        var controller = CreateController(
            orchestrator.Object, new Mock<IAccountingService>().Object, new Mock<IProductionAccountingService>(MockBehavior.Strict).Object, "user-10");

        var refusal = await Assert.ThrowsAsync<RefusalException>(() => controller.GetPeriodClosesAsync(periodClosing.Object));

        Assert.Equal(RefusalKind.Invalid, refusal.Kind);
        periodClosing.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCloseReadinessAsync_AnswersWhatIsOutstanding_WhenACheckFails()
    {
        const string fieldId = "FIELD-PA-6";
        var periodEnd = new DateTime(2026, 4, 30);

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns(fieldId);
        var periodClosing = new Mock<IPeriodClosingService>(MockBehavior.Strict);
        periodClosing.Setup(x => x.ValidateReadinessAsync(fieldId, periodEnd, "PPDM39")).ReturnsAsync(false);
        periodClosing.Setup(x => x.GetPeriodStatusAsync(fieldId, periodEnd)).ReturnsAsync(PeriodCloseStatusCodes.Open);
        periodClosing
            .Setup(x => x.GetUnreconciledItemsAsync(fieldId, periodEnd, "PPDM39"))
            .ReturnsAsync(["ROYALTIES: 1 unreconciled items", "  - Royalty RC-9"]);

        var controller = CreateController(
            orchestrator.Object, new Mock<IAccountingService>().Object, new Mock<IProductionAccountingService>(MockBehavior.Strict).Object, "user-6");

        var result = await controller.GetCloseReadinessAsync(periodEnd, periodClosing.Object);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var readiness = Assert.IsType<PeriodCloseReadinessResult>(ok.Value);
        Assert.False(readiness.Ready);
        Assert.Equal(new[] { "ROYALTIES: 1 unreconciled items", "  - Royalty RC-9" }, readiness.OutstandingItems);
        periodClosing.VerifyAll();
    }

    [Fact]
    public async Task GetCloseReadinessAsync_Refuses_WhenPeriodEndMissing()
    {
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns("FIELD-PA-7");
        var periodClosing = new Mock<IPeriodClosingService>(MockBehavior.Strict);

        var controller = CreateController(
            orchestrator.Object, new Mock<IAccountingService>().Object, new Mock<IProductionAccountingService>(MockBehavior.Strict).Object, "user-7");

        var refusal = await Assert.ThrowsAsync<RefusalException>(() => controller.GetCloseReadinessAsync(default, periodClosing.Object));

        Assert.Equal(RefusalKind.Invalid, refusal.Kind);
        periodClosing.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ClosePeriodAsync_ReturnsBadRequest_WhenPeriodEndMissing()
    {
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns("FIELD-PA-3");

        var accountingService = new Mock<IAccountingService>(MockBehavior.Loose);
        var productionAccounting = new Mock<IProductionAccountingService>(MockBehavior.Strict);
        var controller = CreateController(orchestrator.Object, accountingService.Object, productionAccounting.Object, "user-3");

        var result = await controller.ClosePeriodAsync(new CloseAccountingPeriodRequest());

        Assert.IsType<BadRequestObjectResult>(result);
        productionAccounting.VerifyNoOtherCalls();
        orchestrator.VerifyAll();
    }

    [Fact]
    public async Task ClosePeriodAsync_ReturnsOk_WhenCloseSucceeds()
    {
        const string fieldId = "FIELD-PA-4";

        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(x => x.CurrentFieldId).Returns(fieldId);

        var accountingService = new Mock<IAccountingService>(MockBehavior.Loose);
        var productionAccounting = new Mock<IProductionAccountingService>(MockBehavior.Strict);
        productionAccounting
            .Setup(x => x.ClosePeriodAsync(fieldId, new DateTime(2026, 3, 31), "user-4", "PPDM39"))
            .ReturnsAsync(true);

        var controller = CreateController(orchestrator.Object, accountingService.Object, productionAccounting.Object, "user-4");

        var result = await controller.ClosePeriodAsync(new CloseAccountingPeriodRequest
        {
            PeriodEnd = new DateTime(2026, 3, 31)
        });

        Assert.IsType<OkResult>(result);
        productionAccounting.VerifyAll();
        orchestrator.VerifyAll();
    }

    private static AccountingController CreateController(
        IFieldOrchestrator fieldOrchestrator,
        IAccountingService accountingService,
        IProductionAccountingService productionAccountingService,
        string userId)
    {
        var controller = new AccountingController(
            fieldOrchestrator,
            accountingService,
            productionAccountingService,
            NullLogger<AccountingController>.Instance);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("party_id", userId)
                ], "TestAuth"))
            }
        };

        return controller;
    }
}

