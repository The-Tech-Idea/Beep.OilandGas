using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
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
        Assert.Single(lines);
        productionAccounting.VerifyAll();
        orchestrator.VerifyAll();
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
                    new Claim(ClaimTypes.NameIdentifier, userId)
                ], "TestAuth"))
            }
        };

        return controller;
    }
}

