using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class LedgerRoutingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TankInventoryStoreRequiresBinding(bool create)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var store = new Beep.OilandGas.ApiService.Services.TankInventoryStore(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, () => Task.FromResult(""));
        await Assert.ThrowsAsync<InvalidOperationException>(() => create
            ? (Task)store.CreateAsync(new Beep.OilandGas.Models.Data.Inventory.CreateTankInventoryRequest
                { TankBatteryId = "tank" }, "actor")
            : store.GetAsync("inventory"));
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("get")]
    [InlineData("list")]
    [InlineData("create")]
    public async Task RunTicketStoreRequiresBindingBeforeAccess(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var store = new Beep.OilandGas.ApiService.Services.RunTicketStore(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, () => Task.FromResult(""));
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "get" => (Task)store.GetAsync("ticket"),
            "list" => store.ListAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow),
            _ => store.CreateAsync(new Beep.OilandGas.Models.Data.ProductionAccounting.CreateRunTicketRequest(), "actor")
        });
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("record")]
    [InlineData("lookup")]
    [InlineData("well")]
    [InlineData("lease")]
    public async Task MeasurementStorageRequiresBinding(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var service = new Beep.OilandGas.ProductionAccounting.Services.MeasurementService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, () => Task.FromResult(""));
        var date = DateTime.UtcNow;
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "record" => (Task)service.RecordAsync(new Beep.OilandGas.Models.Data.ProductionAccounting.RUN_TICKET
                { RUN_TICKET_ID = "ticket", NET_VOLUME = 10m }, "actor", "other-db"),
            "lookup" => service.GetAsync("measurement", "other-db"),
            "well" => service.GetByWellAsync("well", date, date, "other-db"),
            _ => service.GetByLeaseAsync("lease", date, date, "other-db")
        });
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PricingRequiresBindingBeforeCurrentOrHistoricalLookup(bool history)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var pricing = new Beep.OilandGas.ProductionAccounting.Services.PricingService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, () => Task.FromResult(""));
        await Assert.ThrowsAsync<InvalidOperationException>(() => history
            ? (Task)pricing.GetHistoryAsync("OIL", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "other-db")
            : pricing.GetPriceAsync("OIL", DateTime.UtcNow, "other-db"));
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("ownership")]
    [InlineData("royalty")]
    [InlineData("validation")]
    public async Task LeaseInterestsRequireBindingBeforeAccess(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var interests = new Beep.OilandGas.ProductionAccounting.Services.LeaseEconomicInterestService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "ownership" => (Task)interests.GetOwnershipInterestsAsync("lease", null, "other-db"),
            "royalty" => interests.GetRoyaltyInterestsAsync("lease", null, "other-db"),
            _ => interests.ValidateEconomicInterestsAsync("lease", null, "other-db")
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RevenueRecognitionRejectsMissingOrFailedBindingBeforeAccess(bool failure)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve()
        {
            calls++;
            return failure ? Task.FromException<string>(new InvalidOperationException("Repository unavailable"))
                : Task.FromResult("");
        }
        var revenue = new Beep.OilandGas.ProductionAccounting.Services.RevenueService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => revenue.RecognizeRevenueAsync(
            new Beep.OilandGas.Models.Data.ProductionAccounting.ALLOCATION_DETAIL(), "actor", "other-db"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("allocate")]
    [InlineData("result")]
    [InlineData("details")]
    public async Task DirectAllocationEngineCallsRequireBinding(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var engine = new Beep.OilandGas.ProductionAccounting.Services.AllocationEngine(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, Resolve);
        var ticket = new Beep.OilandGas.Models.Data.ProductionAccounting.RUN_TICKET { NET_VOLUME = 10m };

        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "allocate" => (Task)engine.AllocateAsync(ticket, "ProRata", "actor", "other-db"),
            "result" => engine.GetAllocationAsync("allocation", "other-db"),
            _ => engine.GetAllocationDetailsAsync("allocation", "other-db")
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AllocationPassesSavedConnectionToEngineInsteadOfCallerOverride()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var engine = new Mock<IAllocationEngine>(MockBehavior.Strict);
        var ticket = new Beep.OilandGas.Models.Data.ProductionAccounting.RUN_TICKET();
        var result = new Beep.OilandGas.Models.Data.ProductionAccounting.ALLOCATION_RESULT();
        engine.Setup(x => x.AllocateAsync(ticket, "ProRata", "actor", "selected-production"))
            .ReturnsAsync(result);
        var service = new Beep.OilandGas.ProductionAccounting.Services.AllocationService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, engine.Object, () => Task.FromResult("selected-production"));

        Assert.Same(result, await service.AllocateAsync(ticket, "ProRata", "actor", "other-db"));
        engine.VerifyAll();
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("result")]
    [InlineData("details")]
    [InlineData("history")]
    public async Task AllocationReadsRequireBinding(string read)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var service = new Beep.OilandGas.ProductionAccounting.Services.AllocationService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, Mock.Of<IAllocationEngine>(), () => Task.FromResult(""));
        await Assert.ThrowsAsync<InvalidOperationException>(() => read switch
        {
            "result" => (Task)service.GetAsync("allocation", "other-db"),
            "details" => service.GetDetailsAsync("allocation", "other-db"),
            _ => service.GetHistoryAsync("ticket", "other-db")
        });
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TankInventoryRequiresBindingEvenWithConnectionOverride(bool update)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var inventory = new Beep.OilandGas.ProductionAccounting.Services.InventoryService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, Resolve);
        var error = await Assert.ThrowsAsync<Beep.OilandGas.ProductionAccounting.Exceptions.ProductionAccountingException>(() => update
            ? (Task)inventory.UpdateInventoryAsync("tank", 10m, "actor", "other-db")
            : inventory.GetInventoryAsync("tank", "other-db"));
        Assert.IsType<InvalidOperationException>(error.InnerException);
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("readiness")]
    [InlineData("close")]
    [InlineData("unreconciled")]
    public async Task ProductionPeriodCloseRequiresBindingBeforeAnyChecks(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var columns = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var accounts = new GLAccountService(editor.Object, columns, defaults, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var journal = new JournalEntryService(editor.Object, columns, defaults, metadata.Object, accounts,
            NullLogger<JournalEntryService>.Instance, Resolve);
        var closing = new Beep.OilandGas.ProductionAccounting.Services.PeriodClosingService(
            editor.Object, columns, defaults, metadata.Object, journal, Resolve);

        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "readiness" => (Task)closing.ValidateReadinessAsync("field", DateTime.UtcNow, "other-db"),
            "close" => closing.ClosePeriodAsync("field", DateTime.UtcNow, "actor", "other-db"),
            _ => closing.GetUnreconciledItemsAsync("field", DateTime.UtcNow, "other-db")
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RoyaltyReadsRequireBindingEvenWithConnectionOverride(bool allocation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var royalties = new Beep.OilandGas.ProductionAccounting.Services.RoyaltyService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, Mock.Of<IJournalEntryService>(), new Infrastructure.RecordingFailureReporter(),
            NullLogger<Beep.OilandGas.ProductionAccounting.Services.RoyaltyService>.Instance, Resolve);

        await Assert.ThrowsAsync<InvalidOperationException>(() => allocation
            ? (Task)royalties.GetByAllocationAsync("allocation", "other-db")
            : royalties.GetAsync("royalty", "other-db"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BankReconciliationRequiresBindingBeforeReadingPaymentsOrLines(bool aged)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var columns = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var accounts = new GLAccountService(editor.Object, columns, defaults, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var bank = new BankReconciliationService(editor.Object, columns, defaults, metadata.Object,
            accounts, NullLogger<BankReconciliationService>.Instance, Resolve);

        await Assert.ThrowsAsync<InvalidOperationException>(() => aged
            ? (Task)bank.AnalyzeAgedOutstandingItemsAsync("account", DateTime.UtcNow)
            : bank.AnalyzeCheckClearingAsync("account", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("receivables")]
    [InlineData("payables")]
    [InlineData("inventory")]
    public async Task ReconciliationRequiresBindingBeforeReadingSubledgers(string ledger)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var columns = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var accounts = new GLAccountService(editor.Object, columns, defaults, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var reconciliation = new ReconciliationService(editor.Object, columns, defaults, metadata.Object,
            accounts, NullLogger<ReconciliationService>.Instance, resolveConnection: Resolve);

        await Assert.ThrowsAsync<InvalidOperationException>(() => ledger switch
        {
            "receivables" => reconciliation.ReconcileAccountsReceivableAsync(),
            "payables" => reconciliation.ReconcileAccountsPayableAsync(),
            _ => reconciliation.ReconcileInventoryAsync()
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PeriodReopenRequiresBindingBeforeReadingClosingEntries()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var columns = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var accounts = new GLAccountService(editor.Object, columns, defaults, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var journal = new JournalEntryService(editor.Object, columns, defaults, metadata.Object, accounts,
            NullLogger<JournalEntryService>.Instance, Resolve);
        var posting = new AccountingBasisPostingService(journal);
        var trial = new TrialBalanceService(editor.Object, columns, defaults, metadata.Object, accounts,
            NullLogger<TrialBalanceService>.Instance);
        var ap = new APInvoiceService(editor.Object, columns, defaults, metadata.Object, posting,
            NullLogger<APInvoiceService>.Instance, resolveConnection: Resolve);
        var ar = new ARService(editor.Object, columns, defaults, metadata.Object, posting,
            new Infrastructure.RecordingFailureReporter(), NullLogger<ARService>.Instance, resolveConnection: Resolve);
        var closing = new PeriodClosingService(editor.Object, columns, defaults, metadata.Object,
            trial, journal, posting, ap, ar, NullLogger<PeriodClosingService>.Instance,
            resolveConnection: Resolve);

        await Assert.ThrowsAsync<InvalidOperationException>(() => closing.ReopenPeriodAsync(DateTime.UtcNow, "actor"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReceivableReadsRequireBindingEvenWithConnectionOverride(bool payments)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var columns = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var accounts = new GLAccountService(editor.Object, columns, defaults, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var journal = new JournalEntryService(editor.Object, columns, defaults, metadata.Object, accounts,
            NullLogger<JournalEntryService>.Instance, Resolve);
        var receivables = new ARService(editor.Object, columns, defaults, metadata.Object,
            new AccountingBasisPostingService(journal), new Infrastructure.RecordingFailureReporter(),
            NullLogger<ARService>.Instance, resolveConnection: Resolve);

        await Assert.ThrowsAsync<InvalidOperationException>(() => payments
            ? (Task)receivables.GetPaymentsByInvoiceAsync("invoice", "other-db")
            : receivables.GetInvoiceAsync("invoice", "other-db"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InventoryValuationRequiresBindingEvenWithConnectionOverride(bool adjust)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var valuation = new InventoryLcmService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object,
            NullLogger<InventoryLcmService>.Instance, resolveConnection: Resolve);

        await Assert.ThrowsAsync<InvalidOperationException>(() => adjust
            ? (Task)valuation.ApplyLowerOfCostOrMarketAsync("item", DateTime.UtcNow, "actor", "other-db")
            : valuation.GetMarketValueAsync("item", DateTime.UtcNow, "other-db"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("create")]
    [InlineData("lookup")]
    [InlineData("list")]
    [InlineData("transactions")]
    public async Task InventoryRequiresBindingBeforeMetadataOrDatasourceAccess(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var columns = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var accounts = new GLAccountService(editor.Object, columns, defaults, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var journal = new JournalEntryService(editor.Object, columns, defaults, metadata.Object, accounts,
            NullLogger<JournalEntryService>.Instance, Resolve);
        var inventory = new InventoryService(editor.Object, columns, defaults, metadata.Object,
            new AccountingBasisPostingService(journal), NullLogger<InventoryService>.Instance,
            resolveConnection: Resolve);

        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "create" => (Task)inventory.CreateInventoryItemAsync("item", "Item", "PART", "EA", 10m),
            "lookup" => inventory.GetInventoryItemByIdAsync("item"),
            "list" => inventory.GetAllInventoryItemsAsync(),
            _ => inventory.GetItemTransactionsAsync("item")
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PurchaseOrderLookupPropagatesMissingBindingBeforeMetadataAccess()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var orders = new PurchaseOrderService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, NullLogger<PurchaseOrderService>.Instance, Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => orders.GetPOByIdAsync("order"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PayablesRequireBindingBeforeMetadataOrDatasourceAccess(bool payment)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var columns = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var accounts = new GLAccountService(editor.Object, columns, defaults, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var journal = new JournalEntryService(editor.Object, columns, defaults, metadata.Object, accounts,
            NullLogger<JournalEntryService>.Instance, Resolve);
        var posting = new AccountingBasisPostingService(journal);
        var invoices = new APInvoiceService(editor.Object, columns, defaults, metadata.Object, posting,
            NullLogger<APInvoiceService>.Instance, resolveConnection: Resolve);
        var payments = new APPaymentService(editor.Object, columns, defaults, metadata.Object, posting,
            NullLogger<APPaymentService>.Instance, resolveConnection: Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => payment
            ? (Task)payments.GetPaymentByIdAsync("payment") : invoices.GetBillByIdAsync("invoice"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LedgerReadsRequireSavedBindingEvenWithConnectionOverride(bool journal)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var columns = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var accounts = new GLAccountService(editor.Object, columns, defaults, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var entries = new JournalEntryService(editor.Object, columns, defaults, metadata.Object, accounts,
            NullLogger<JournalEntryService>.Instance, Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => journal
            ? (Task)entries.GetEntriesByAccountAsync("account", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, "other-db")
            : accounts.GetAllAccountsAsync());
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }
}
