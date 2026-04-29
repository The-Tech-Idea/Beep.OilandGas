using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Trading;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionAccountingControllerFacadeCompatibilityTests
{
    [Fact]
    public void TraditionalAccounting_AccountsReceivable_CreateARInvoice_ReturnsInvoice_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var request = new CreateARInvoiceRequest
        {
            InvoiceNumber = "AR-100",
            CustomerBaId = "CUST-1",
            InvoiceDate = new DateTime(2026, 4, 1),
            DueDate = new DateTime(2026, 4, 30),
            TotalAmount = 250m
        };

        var invoice = service.TraditionalAccounting.AccountsReceivable.CreateARInvoice(request, "user-0");

        Assert.NotNull(invoice);
        Assert.Equal("AR-100", invoice.INVOICE_NUMBER);
        Assert.Equal(250m, invoice.BALANCE_DUE);
    }

    [Fact]
    public void TraditionalAccounting_Inventory_CreateTransaction_ReturnsTransaction_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var transaction = service.TraditionalAccounting.Inventory.CreateTransaction(
            "INV-1",
            "RECEIPT",
            new DateTime(2026, 4, 1),
            10m,
            5m,
            "compatibility test",
            "user-1");

        Assert.NotNull(transaction);
        Assert.Equal("INV-1", transaction.INVENTORY_ITEM_ID);
        Assert.Equal(50m, transaction.TOTAL_COST);
    }

    [Fact]
    public void TraditionalAccounting_PurchaseOrder_CreatePurchaseOrder_ReturnsPurchaseOrder_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var request = new CreatePurchaseOrderRequest
        {
            VendorBaId = "VENDOR-1",
            Description = "compatibility purchase order"
        };

        var purchaseOrder = service.TraditionalAccounting.PurchaseOrder.CreatePurchaseOrder(request, "user-2");

        Assert.NotNull(purchaseOrder);
        Assert.Equal("VENDOR-1", purchaseOrder.VENDOR_BA_ID);
        Assert.Equal("DRAFT", purchaseOrder.STATUS);
    }

    [Fact]
    public void TraditionalAccounting_AccountsPayable_CreateAPInvoice_ReturnsInvoice_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var request = new CreateAPInvoiceRequest
        {
            InvoiceNumber = "AP-200",
            VendorBaId = "VENDOR-2",
            InvoiceDate = new DateTime(2026, 4, 1),
            DueDate = new DateTime(2026, 4, 30),
            TotalAmount = 300m
        };

        var invoice = service.TraditionalAccounting.AccountsPayable.CreateAPInvoice(request, "user-3");

        Assert.NotNull(invoice);
        Assert.Equal("AP-200", invoice.INVOICE_NUMBER);
        Assert.Equal(300m, invoice.BALANCE_DUE);
    }

    [Fact]
    public void TraditionalAccounting_GeneralLedger_CreateAccount_ReturnsAccount_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var request = new CreateGLAccountRequest
        {
            AccountNumber = "4000",
            AccountName = "Revenue",
            AccountType = "INCOME",
            NormalBalance = "CREDIT",
            OpeningBalance = 0m
        };

        var account = service.TraditionalAccounting.GeneralLedger.CreateAccount(request, "user-4");

        Assert.NotNull(account);
        Assert.Equal("4000", account.ACCOUNT_NUMBER);
        Assert.Equal("Revenue", account.ACCOUNT_NAME);
    }

    [Fact]
    public void TraditionalAccounting_JournalEntry_CreateJournalEntry_ReturnsEntry_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var lines = new List<JournalEntryLineData>
        {
            new() { GlAccountId = "4000", DebitAmount = 100m, CreditAmount = 0m, Description = "Debit line" },
            new() { GlAccountId = "1000", DebitAmount = 0m, CreditAmount = 100m, Description = "Credit line" }
        };

        var entry = service.TraditionalAccounting.JournalEntry.CreateJournalEntry(
            "JE-1",
            new DateTime(2026, 4, 1),
            "MANUAL",
            "compatibility journal entry",
            lines,
            "user-5");

        Assert.NotNull(entry);
        Assert.Equal("JE-1", entry.ENTRY_NUMBER);
        Assert.Equal(100m, entry.TOTAL_DEBIT);
        Assert.Equal(100m, entry.TOTAL_CREDIT);
    }

    [Fact]
    public async Task TradingService_RegisterContractAsync_ReturnsContract_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var request = new CreateExchangeContractRequest
        {
            ContractId = "EX-1",
            ContractName = "Compat Exchange Contract",
            ContractType = ExchangeContractType.PhysicalExchange,
            EffectiveDate = new DateTime(2026, 4, 1),
            ExpirationDate = new DateTime(2026, 12, 31)
        };

        var contract = await service.TradingService.RegisterContractAsync(request, "user-6");

        Assert.NotNull(contract);
        Assert.Equal("EX-1", contract.CONTRACT_ID);
        Assert.Equal("Compat Exchange Contract", contract.CONTRACT_NAME);
    }

    [Fact]
    public void RoyaltyManager_CalculateAndCreatePayment_ReturnsPayment_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var sales = new SalesTransaction
        {
            SALES_TRANSACTION_ID = "SALE-1",
            PRICE_PER_BARREL = 50m
        };

        var payment = service.RoyaltyManager.CalculateAndCreatePayment(
            sales,
            "OWNER-1",
            0.10m,
            new DateTime(2026, 4, 1));

        Assert.NotNull(payment);
        Assert.Equal("OWNER-1", payment.ROYALTY_OWNER_ID);
        Assert.Equal(5m, payment.ROYALTY_AMOUNT);
    }

    [Fact]
    public void OwnershipManager_CreateDivisionOrder_ReturnsDivisionOrder_WhenRepositoryInsertFails()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var owner = new OWNER_INFORMATION
        {
            OWNER_ID = "OWNER-2"
        };

        var divisionOrder = service.OwnershipManager.CreateDivisionOrder(
            "LEASE-2",
            owner,
            0.75m,
            0.70m,
            new DateTime(2026, 4, 1));

        Assert.NotNull(divisionOrder);
        Assert.Equal("LEASE-2", divisionOrder.PROPERTY_OR_LEASE_ID);
        Assert.Equal("OWNER-2", divisionOrder.OWNER_ID);
    }

    private static ProductionAccountingService CreateService(TestDependencies d)
    {
        return new ProductionAccountingService(
            d.AllocationService.Object,
            d.RoyaltyService.Object,
            d.JibService.Object,
            d.ImbalanceService.Object,
            d.SuccessfulEffortsService.Object,
            d.FullCostService.Object,
            d.AmortizationService.Object,
            d.JournalEntryService.Object,
            d.RevenueService.Object,
            d.MeasurementService.Object,
            d.PricingService.Object,
            d.InventoryService.Object,
            d.PeriodClosingService.Object,
            d.Metadata.Object,
            d.Editor.Object,
            d.CommonColumnHandler.Object,
            d.Defaults.Object,
            NullLogger<ProductionAccountingService>.Instance);
    }

    private static TestDependencies CreateDependencies() => new();

    private sealed class TestDependencies
    {
        public Mock<IAllocationService> AllocationService { get; } = new();
        public Mock<IRoyaltyService> RoyaltyService { get; } = new();
        public Mock<IJointInterestBillingService> JibService { get; } = new();
        public Mock<IImbalanceService> ImbalanceService { get; } = new();
        public Mock<ISuccessfulEffortsService> SuccessfulEffortsService { get; } = new();
        public Mock<IFullCostService> FullCostService { get; } = new();
        public Mock<IAmortizationService> AmortizationService { get; } = new();
        public Mock<IJournalEntryService> JournalEntryService { get; } = new();
        public Mock<IRevenueService> RevenueService { get; } = new();
        public Mock<IMeasurementService> MeasurementService { get; } = new();
        public Mock<IPricingService> PricingService { get; } = new();
        public Mock<IInventoryService> InventoryService { get; } = new();
        public Mock<IPeriodClosingService> PeriodClosingService { get; } = new();
        public Mock<IPPDMMetadataRepository> Metadata { get; } = new();
        public Mock<IDMEEditor> Editor { get; } = new();
        public Mock<ICommonColumnHandler> CommonColumnHandler { get; } = new();
        public Mock<IPPDM39DefaultsRepository> Defaults { get; } = new();

        public TestDependencies()
        {
            Defaults.Setup(x => x.GetActiveIndicatorYes()).Returns("Y");
        }
    }
}
