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
    public async Task RoyaltyReadsRequireBindingEvenWithConnectionOverride(bool allocation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var royalties = new Beep.OilandGas.ProductionAccounting.Services.RoyaltyService(
            editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            metadata.Object, Mock.Of<IJournalEntryService>(),
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
            NullLogger<ARService>.Instance, resolveConnection: Resolve);
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
            new AccountingBasisPostingService(journal), NullLogger<ARService>.Instance,
            resolveConnection: Resolve);

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
