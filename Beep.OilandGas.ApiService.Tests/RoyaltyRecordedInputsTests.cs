using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.ProductionAccounting.Services;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Accounting.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class RoyaltyRecordedInputsTests
{
    private readonly Mock<IDataSource> _source = new();
    private readonly Mock<IJournalEntryService> _journal = new();
    private readonly Dictionary<string, List<object>> _rows = new();
    private readonly RoyaltyService _service;
    private readonly JournalEntryService _journalReader;
    private readonly RUN_TICKET _ticket = new() { RUN_TICKET_ID = "ticket", LEASE_ID = "lease", ACTIVE_IND = "Y",
        TICKET_DATE_TIME = new DateTime(2026, 9, 1), PRICE_PER_BARREL = 80m, VOLUME_OUOM = "BBL" };
    private readonly ROYALTY_INTEREST _interest = new() { ROYALTY_INTEREST_ID = "interest", ROYALTY_OWNER_ID = "owner",
        PROPERTY_OR_LEASE_ID = "lease", ACTIVE_IND = "Y", ROYALTY_RATE = 12.5m };
    private readonly ALLOCATION_DETAIL _detail = new() { ALLOCATION_DETAIL_ID = "detail", ALLOCATION_RESULT_ID = "allocation",
        ENTITY_ID = "owner", ALLOCATED_VOLUME = 100m };

    public RoyaltyRecordedInputsTests()
    {
        var editor = new Mock<IDMEEditor>();
        editor.Setup(e => e.GetDataSource("test")).Returns(_source.Object);
        var metadata = new Mock<IPPDMMetadataRepository>();
        metadata.Setup(m => m.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync((string name) =>
            new PPDMTableMetadata { TableName = name, EntityTypeName = name, PrimaryKeyColumn = name + "_ID" });
        var defaults = new Mock<IPPDM39DefaultsRepository>();
        defaults.Setup(d => d.GetActiveIndicatorYes()).Returns("Y");
        defaults.Setup(d => d.FormatIdForTable(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object id) => id.ToString()!);
        var common = new Mock<ICommonColumnHandler>().Object;
        _journalReader = new JournalEntryService(editor.Object, common, defaults.Object, metadata.Object,
            new GLAccountService(editor.Object, common, defaults.Object, metadata.Object, NullLogger<GLAccountService>.Instance, () => Task.FromResult("test")),
            NullLogger<JournalEntryService>.Instance, () => Task.FromResult("test"));
        _rows["RUN_TICKET"] = new() { _ticket };
        _rows["ROYALTY_INTEREST"] = new() { _interest };
        _rows["ALLOCATION_RESULT"] = new() { new ALLOCATION_RESULT { ALLOCATION_RESULT_ID = "allocation", ALLOCATION_REQUEST_ID = "ticket" } };
        _rows["ACCOUNTING_COST"] = new();
        _detail.ACTIVE_IND = "Y";
        _ticket.FIELD_ID = "field-a";
        _rows["ALLOCATION_DETAIL"] = new() { _detail };
        _rows["ROYALTY_CALCULATION"] = new();
        _rows["ROYALTY_PAYMENT"] = new();
        _source.Setup(s => s.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>()))
            .Returns((string table, List<AppFilter> filters) => Task.FromResult<IEnumerable<object>>(_rows[table].Where(row =>
                filters.Where(f => f.Operator == "=").All(filter =>
                    row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString() == filter.FilterValue)).Select(row => System.Text.Json.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(row), row.GetType())!).ToList()));
        _source.Setup(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()))
            .Callback((string table, object row) => _rows[table].Add(
                System.Text.Json.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(row), row.GetType())!))
            .Returns(new ErrorsInfo { Flag = Errors.Ok });
        _source.Setup(s => s.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()))
            .Returns((string table, object row) => {
                var key = row.GetType().GetProperty(table + "_ID")!;
                var index = _rows[table].FindIndex(item => Equals(key.GetValue(item), key.GetValue(row)));
                _rows[table][index] = System.Text.Json.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(row), row.GetType())!;
                return new ErrorsInfo { Flag = Errors.Ok };
            });
        _journal.Setup(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), "actor", It.IsAny<string>()))
            .ReturnsAsync(new JOURNAL_ENTRY { JOURNAL_ENTRY_ID = "journal", STATUS = "POSTED" });
        _service = new RoyaltyService(editor.Object, new Mock<ICommonColumnHandler>().Object, defaults.Object, metadata.Object, _journal.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<RoyaltyService>.Instance, () => Task.FromResult("test"));
    }

    [Fact]
    public async Task UsesRecordedRateAndPriceWithoutInventedDeductions()
    {
        var result = await _service.CalculateAsync("detail", "actor");
        Assert.Equal(8000m, result.GROSS_REVENUE);
        Assert.Equal(8000m, result.NET_REVENUE);
        Assert.Equal(1000m, result.ROYALTY_AMOUNT);
        Assert.Equal("interest", result.ROYALTY_INTEREST_ID);
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), 1000m, It.IsAny<string>(), "actor", result.ROYALTY_CALCULATION_ID), Times.Once);
        Assert.Equal(100m, result.GROSS_VOLUME);
        Assert.Equal("BBL", result.GROSS_VOLUME_OUOM);
    }

    [Fact]
    public async Task PreviewReturnsRecordedCalculationWithoutPersistenceOrJournalCalls()
    {
        var result = await _service.PreviewAsync(_detail, "actor", "test");
        Assert.Equal("PREVIEW", result.ROYALTY_STATUS);
        Assert.Equal(1000m, result.ROYALTY_AMOUNT);
        _source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _journal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OnePercentIsNotTreatedAsOneHundredPercent()
    {
        _interest.ROYALTY_RATE = 1m;
        Assert.Equal(80m, (await _service.CalculateAsync("detail", "actor")).ROYALTY_AMOUNT);
    }

    [Theory]
    [InlineData("missing-interest")]
    [InlineData("overlapping-interest")]
    [InlineData("expired-interest")]
    [InlineData("missing-price")]
    [InlineData("wrong-unit")]
    [InlineData("negative-rate")]
    [InlineData("excess-rate")]
    [InlineData("unallocated-deductions")]
    public async Task InvalidCommercialInputsFailBeforeWriting(string scenario)
    {
        switch (scenario)
        {
            case "missing-interest": _rows["ROYALTY_INTEREST"].Clear(); break;
            case "overlapping-interest": _rows["ROYALTY_INTEREST"].Add(_interest); break;
            case "expired-interest": _interest.EFFECTIVE_END_DATE = new DateTime(2026, 8, 31); break;
            case "missing-price": _ticket.PRICE_PER_BARREL = null; break;
            case "wrong-unit": _ticket.VOLUME_OUOM = "MCF"; break;
            case "negative-rate": _interest.ROYALTY_RATE = -1; break;
            case "excess-rate": _interest.ROYALTY_RATE = 101; break;
            case "unallocated-deductions": _rows["ACCOUNTING_COST"].Add(new ACCOUNTING_COST
                { PROPERTY_ID = "lease", ACTIVE_IND = "Y", COST_TYPE = "TRANSPORTATION", AMOUNT = 500m }); break;
        }
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.CalculateAsync("detail", "actor"));
        _source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _journal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeductionLookupFailureCannotBecomeZeroDeductions()
    {
        _source.Setup(s => s.GetEntityAsync("ACCOUNTING_COST", It.IsAny<List<AppFilter>>()))
            .ThrowsAsync(new InvalidOperationException("provider unavailable"));
        await Assert.ThrowsAnyAsync<Exception>(() => _service.CalculateAsync("detail", "actor"));
        _source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _journal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SuccessfulRetryReturnsRecordedAccrualWithoutReposting()
    {
        var first = await _service.CalculateAsync("detail", "actor");
        _detail.ALLOCATED_VOLUME = 99999m;
        var replay = await _service.CalculateAsync("detail", "actor");
        Assert.Equal(first.ROYALTY_CALCULATION_ID, replay.ROYALTY_CALCULATION_ID);
        Assert.Equal(1000m, replay.ROYALTY_AMOUNT);
        _source.Verify(s => s.InsertEntity("ROYALTY_CALCULATION", It.IsAny<object>()), Times.Once);
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), 1000m,
            It.IsAny<string>(), "actor", It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData("journal-failure")]
    [InlineData("unconfirmed-journal")]
    [InlineData("status-write-failure")]
    public async Task IncompleteAccrualCannotAutomaticallyPostAgain(string failure)
    {
        if (failure == "journal-failure")
            _journal.Setup(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), "actor", It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("journal unavailable"));
        if (failure == "unconfirmed-journal")
            _journal.Setup(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), "actor", It.IsAny<string>()))
                .ReturnsAsync(new JOURNAL_ENTRY { JOURNAL_ENTRY_ID = "draft", STATUS = "DRAFT" });
        if (failure == "status-write-failure")
            _source.Setup(s => s.UpdateEntity("ROYALTY_CALCULATION", It.IsAny<object>())).Returns(new ErrorsInfo { Flag = Errors.Failed });
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.CalculateAsync("detail", "actor"));
        Assert.Equal("CALCULATED", ((ROYALTY_CALCULATION)Assert.Single(_rows["ROYALTY_CALCULATION"])).ROYALTY_STATUS);
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.CalculateAsync("detail", "actor"));
        _source.Verify(s => s.InsertEntity("ROYALTY_CALCULATION", It.IsAny<object>()), Times.Once);
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), "actor", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task MissingStoredDetailCannotCreateAnObligation()
    {
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.CalculateAsync("missing", "actor"));
        _source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _journal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AuthorizationFieldComesFromStoredTicket()
    {
        Assert.Equal("field-a", await _service.GetAllocationFieldAsync("detail"));
        _ticket.FIELD_ID = null!;
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.GetAllocationFieldAsync("detail"));
        _source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task ConcurrentReservationsUseSamePrimaryKeyAndOnlyWinnerPosts()
    {
        var arrivals = 0;
        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _source.Setup(s => s.GetEntityAsync("ROYALTY_CALCULATION", It.IsAny<List<AppFilter>>()))
            .Returns(async () => { if (Interlocked.Increment(ref arrivals) == 2) barrier.TrySetResult();
                await barrier.Task.WaitAsync(TimeSpan.FromSeconds(5)); return (IEnumerable<object>)Array.Empty<object>(); });
        var inserted = new System.Collections.Concurrent.ConcurrentDictionary<string, byte>();
        var attempted = new System.Collections.Concurrent.ConcurrentBag<string>();
        _source.Setup(s => s.InsertEntity("ROYALTY_CALCULATION", It.IsAny<object>()))
            .Returns((string _, object row) => {
                var id = ((ROYALTY_CALCULATION)row).ROYALTY_CALCULATION_ID;
                attempted.Add(id);
                return new ErrorsInfo { Flag = inserted.TryAdd(id, 0) ? Errors.Ok : Errors.Failed };
            });
        _source.Setup(s => s.UpdateEntity("ROYALTY_CALCULATION", It.IsAny<object>())).Returns(new ErrorsInfo { Flag = Errors.Ok });
        async Task<Exception?> Attempt() { try { await _service.CalculateAsync("detail", "actor"); return null; } catch (Exception ex) { return ex; } }
        var outcomes = await Task.WhenAll(Attempt(), Attempt());
        Assert.Single(outcomes, x => x is null);
        Assert.Single(outcomes, x => x is RoyaltyException);
        Assert.Equal(2, attempted.Count);
        Assert.Single(attempted.Distinct());
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), "actor", It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task PartialPaymentsUseOutstandingBalanceAndReplayDoesNotPostAgain()
    {
        var royalty = await _service.CalculateAsync("detail", "actor");
        var request = Guid.NewGuid();
        var first = await _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, request, 400m, "actor");
        Assert.Equal(royalty.ROYALTY_CALCULATION_ID, first.ROYALTY_CALCULATION_ID);
        Assert.Equal("journal", first.JOURNAL_ENTRY_ID);
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), 400m, It.IsAny<string>(), "actor", first.ROYALTY_PAYMENT_ID), Times.Once);
        Assert.Equal("ACCRUED", (await _service.GetAsync(royalty.ROYALTY_CALCULATION_ID))!.ROYALTY_STATUS);
        var replay = await _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, request, 400m, "actor");
        Assert.Equal(first.ROYALTY_PAYMENT_ID, replay.ROYALTY_PAYMENT_ID);
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, request, 401m, "actor"));
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, Guid.NewGuid(), 601m, "actor"));
        var final = await _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, Guid.NewGuid(), 600m, "actor");
        Assert.NotEqual(first.ROYALTY_PAYMENT_ID, final.ROYALTY_PAYMENT_ID);
        Assert.Equal("PAID", (await _service.GetAsync(royalty.ROYALTY_CALCULATION_ID))!.ROYALTY_STATUS);
        Assert.Equal(2, (await _service.GetPaymentsAsync(royalty.ROYALTY_CALCULATION_ID)).Count);
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), "actor", It.IsAny<string>()), Times.Exactly(3));
    }

    [Theory]
    [InlineData("journal")]
    [InlineData("receipt")]
    [InlineData("calculation")]
    [InlineData("payment")]
    public async Task PaymentFailuresBlockAutomaticReposting(string failure)
    {
        var royalty = await _service.CalculateAsync("detail", "actor");
        _journal.Invocations.Clear();
        if (failure == "journal") _journal.Setup(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), "actor", It.IsAny<string>())).ThrowsAsync(new Exception("offline"));
        if (failure == "receipt") _journal.Setup(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), "actor", It.IsAny<string>())).ReturnsAsync(new JOURNAL_ENTRY { STATUS = "DRAFT" });
        if (failure is "calculation" or "payment") _source.Setup(s => s.UpdateEntity(failure == "payment" ? "ROYALTY_PAYMENT" : "ROYALTY_CALCULATION", It.IsAny<object>())).Returns(new ErrorsInfo { Flag = Errors.Failed });
        var request = Guid.NewGuid();
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, request, 400m, "actor"));
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, request, 400m, "actor"));
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, Guid.NewGuid(), 400m, "actor"));
        Assert.Equal("PENDING", ((ROYALTY_PAYMENT)Assert.Single(_rows["ROYALTY_PAYMENT"])).STATUS);
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), "actor", It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    [InlineData(0.001)]
    public async Task InvalidPaymentAmountsNeverReserve(decimal amount)
    {
        var royalty = await _service.CalculateAsync("detail", "actor");
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, Guid.NewGuid(), amount, "actor"));
        Assert.Empty(_rows["ROYALTY_PAYMENT"]);
    }

    [Fact]
    public async Task ConcurrentPaymentsCannotSpendTheSameBalanceTwice()
    {
        var royalty = await _service.CalculateAsync("detail", "actor");
        _journal.Invocations.Clear();
        var arrivals = 0;
        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _source.Setup(s => s.GetEntityAsync("ROYALTY_PAYMENT", It.IsAny<List<AppFilter>>()))
            .Returns(async () => { if (Interlocked.Increment(ref arrivals) == 2) barrier.TrySetResult();
                await barrier.Task.WaitAsync(TimeSpan.FromSeconds(5)); return (IEnumerable<object>)Array.Empty<object>(); });
        var inserted = new System.Collections.Concurrent.ConcurrentDictionary<string, byte>();
        var attempted = new System.Collections.Concurrent.ConcurrentBag<string>();
        _source.Setup(s => s.InsertEntity("ROYALTY_PAYMENT", It.IsAny<object>()))
            .Returns((string _, object row) => {
                var id = ((ROYALTY_PAYMENT)row).ROYALTY_PAYMENT_ID;
                attempted.Add(id);
                return new ErrorsInfo { Flag = inserted.TryAdd(id, 0) ? Errors.Ok : Errors.Failed };
            });
        _source.Setup(s => s.UpdateEntity("ROYALTY_PAYMENT", It.IsAny<object>())).Returns(new ErrorsInfo { Flag = Errors.Ok });
        async Task<Exception?> Attempt() { try { await _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, Guid.NewGuid(), 800m, "actor"); return null; } catch (Exception ex) { return ex; } }
        var outcomes = await Task.WhenAll(Attempt(), Attempt());
        Assert.Single(outcomes, x => x is null);
        Assert.Single(outcomes, x => x is RoyaltyException);
        Assert.Equal(2, attempted.Count);
        Assert.Single(attempted.Distinct());
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), "actor", It.IsAny<string>()), Times.Once);
    }
    [Theory]
    [InlineData("match", RoyaltyPostingFinding.MatchingPostedEvidence)]
    [InlineData("missing", RoyaltyPostingFinding.MissingJournal)]
    [InlineData("ambiguous", RoyaltyPostingFinding.AmbiguousJournals)]
    [InlineData("wrong-account", RoyaltyPostingFinding.JournalMismatch)]
    [InlineData("wrong-link", RoyaltyPostingFinding.JournalMismatch)]
    [InlineData("wrong-total", RoyaltyPostingFinding.JournalMismatch)]
    [InlineData("draft", RoyaltyPostingFinding.IncompletePosting)]
    [InlineData("missing-ledger", RoyaltyPostingFinding.IncompletePosting)]
    [InlineData("duplicate-ledger", RoyaltyPostingFinding.IncompletePosting)]
    public async Task PostingReviewUsesStoredJournalAndLedgerEvidenceWithoutWrites(string scenario, RoyaltyPostingFinding expected)
    {
        var royalty = await _service.CalculateAsync("detail", "actor");
        Assert.Equal("journal", royalty.JOURNAL_ENTRY_ID);
        var description = $"Royalty accrual {royalty.ROYALTY_CALCULATION_ID} for allocation detail";
        var evidence = Evidence(description, DefaultGlAccounts.RoyaltyExpense, DefaultGlAccounts.AccruedRoyalties, 1000m);
        evidence.Header.REFERENCE_NUMBER = royalty.ROYALTY_CALCULATION_ID;
        if (scenario == "wrong-account") evidence.Lines[0].GL_ACCOUNT_ID = "wrong";
        if (scenario == "wrong-link") evidence.Header.JOURNAL_ENTRY_ID = "other";
        if (scenario == "wrong-total") evidence.Header.TOTAL_DEBIT = 999;
        if (scenario == "draft") evidence.Header.STATUS = "DRAFT";
        if (scenario == "missing-ledger") evidence.LedgerEntries.Clear();
        if (scenario == "duplicate-ledger") evidence.LedgerEntries.Add(evidence.LedgerEntries[0]);
        _journal.Setup(j => j.GetPostingEvidenceAsync(royalty.ROYALTY_CALCULATION_ID)).ReturnsAsync(scenario == "missing" ? new List<JournalPostingEvidence>() : scenario == "ambiguous" ? new List<JournalPostingEvidence>() { evidence, evidence } : new List<JournalPostingEvidence>() { evidence });
        _source.Invocations.Clear(); _journal.Invocations.Clear();
        var review = Assert.Single(await _service.ReviewPostingsAsync(royalty.ROYALTY_CALCULATION_ID));
        Assert.Equal(expected, review.Finding);
        _source.Verify(x => x.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _source.Verify(x => x.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _journal.Verify(j => j.GetPostingEvidenceAsync(royalty.ROYALTY_CALCULATION_ID), Times.Once);
        _journal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task JournalEvidenceReaderScopesRowsAndPropagatesFailure()
    {
        var evidence = Evidence("exact description", DefaultGlAccounts.RoyaltyExpense, DefaultGlAccounts.AccruedRoyalties, 1000);
        _rows["JOURNAL_ENTRY"] = new() { evidence.Header, new JOURNAL_ENTRY { JOURNAL_ENTRY_ID = "unrelated", DESCRIPTION = "other" } };
        _rows["JOURNAL_ENTRY_LINE"] = evidence.Lines.Cast<object>().ToList();
        _rows["GL_ENTRY"] = evidence.LedgerEntries.Cast<object>().ToList();
        var result = Assert.Single(await _journalReader.GetPostingEvidenceAsync("exact description"));
        Assert.Equal(2, result.Lines.Count); Assert.Equal(2, result.LedgerEntries.Count);
        _source.Setup(x => x.GetEntityAsync("GL_ENTRY", It.IsAny<List<AppFilter>>())).ThrowsAsync(new InvalidOperationException("offline"));
        await Assert.ThrowsAnyAsync<Exception>(() => _journalReader.GetPostingEvidenceAsync("exact description"));
        _source.Verify(x => x.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _source.Verify(x => x.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task PendingPaymentCanShowPostedEvidenceWithoutChangingItsStatus()
    {
        var royalty = await _service.CalculateAsync("detail", "actor");
        _source.Setup(s => s.UpdateEntity("ROYALTY_PAYMENT", It.IsAny<object>())).Returns(new ErrorsInfo { Flag = Errors.Failed });
        await Assert.ThrowsAsync<RoyaltyException>(() => _service.RecordPaymentAsync(royalty.ROYALTY_CALCULATION_ID, Guid.NewGuid(), 400m, "actor"));
        var pending = (ROYALTY_PAYMENT)Assert.Single(_rows["ROYALTY_PAYMENT"]);
        var accrualDescription = $"Royalty accrual {royalty.ROYALTY_CALCULATION_ID} for allocation detail";
        var paymentDescription = $"Royalty payment {pending.ROYALTY_PAYMENT_ID} for calculation {royalty.ROYALTY_CALCULATION_ID}";
        _journal.Setup(j => j.GetPostingEvidenceAsync(royalty.ROYALTY_CALCULATION_ID)).ReturnsAsync(new List<JournalPostingEvidence>() { Evidence(royalty.ROYALTY_CALCULATION_ID, DefaultGlAccounts.RoyaltyExpense, DefaultGlAccounts.AccruedRoyalties, 1000m) });
        _journal.Setup(j => j.GetPostingEvidenceAsync(pending.ROYALTY_PAYMENT_ID)).ReturnsAsync(new List<JournalPostingEvidence>() { Evidence(pending.ROYALTY_PAYMENT_ID, DefaultGlAccounts.AccruedRoyalties, DefaultGlAccounts.Cash, 400m) });
        _source.Invocations.Clear(); _journal.Invocations.Clear();
        var review = await _service.ReviewPostingsAsync(royalty.ROYALTY_CALCULATION_ID);
        var payment = Assert.Single(review, r => r.RecordKind == "Payment");
        Assert.Equal("PENDING", payment.RecordedStatus);
        Assert.Equal(RoyaltyPostingFinding.MatchingPostedEvidence, payment.Finding);
        Assert.Equal("PENDING", pending.STATUS);
        _source.Verify(x => x.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _journal.Verify(j => j.CreateReferencedBalancedEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReferencedJournalPersistsSourceIdentityAndReviewIgnoresDescriptionEdits()
    {
        _rows["GL_ACCOUNT"] = new() {
            new GL_ACCOUNT { GL_ACCOUNT_ID = "debit", ACCOUNT_NUMBER = "debit", ACTIVE_IND = "Y" },
            new GL_ACCOUNT { GL_ACCOUNT_ID = "credit", ACCOUNT_NUMBER = "credit", ACTIVE_IND = "Y" }
        };
        _rows["JOURNAL_ENTRY"] = new(); _rows["JOURNAL_ENTRY_LINE"] = new(); _rows["GL_ENTRY"] = new();
        var reference = Guid.NewGuid().ToString();
        var entry = await _journalReader.CreateReferencedBalancedEntryAsync("debit", "credit", 25m, "original wording", "actor", reference);
        Assert.Equal(reference, entry.REFERENCE_NUMBER);
        Assert.Equal("POSTED", entry.STATUS);
        var stored = (JOURNAL_ENTRY)Assert.Single(_rows["JOURNAL_ENTRY"]);
        Assert.Equal(reference, stored.REFERENCE_NUMBER);
        stored.DESCRIPTION = "amended wording";
        _rows["JOURNAL_ENTRY"].Add(new JOURNAL_ENTRY { JOURNAL_ENTRY_ID = "other-module", REFERENCE_NUMBER = reference, SOURCE_MODULE = "OTHER" });
        var evidence = Assert.Single(await _journalReader.GetPostingEvidenceAsync(reference));
        Assert.Equal(entry.JOURNAL_ENTRY_ID, evidence.Header.JOURNAL_ENTRY_ID);
        Assert.Equal(2, evidence.Lines.Count);
        Assert.Equal(2, evidence.LedgerEntries.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public async Task InvalidJournalReferenceDoesNotWrite(string reference)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _journalReader.CreateReferencedBalancedEntryAsync("debit", "credit", 25m, "description", "actor", reference));
        _source.Verify(x => x.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    private static JournalPostingEvidence Evidence(string description, string debit, string credit, decimal amount)
        => new(new JOURNAL_ENTRY { JOURNAL_ENTRY_ID = "journal", DESCRIPTION = description, REFERENCE_NUMBER = description, SOURCE_MODULE = "PRODUCTION_ACCOUNTING",
            STATUS = "POSTED", ACTIVE_IND = "Y", TOTAL_DEBIT = amount, TOTAL_CREDIT = amount },
            new() {
                new JOURNAL_ENTRY_LINE { JOURNAL_ENTRY_LINE_ID = "d", JOURNAL_ENTRY_ID = "journal", ACTIVE_IND = "Y", GL_ACCOUNT_ID = debit, DEBIT_AMOUNT = amount, CREDIT_AMOUNT = 0 },
                new JOURNAL_ENTRY_LINE { JOURNAL_ENTRY_LINE_ID = "c", JOURNAL_ENTRY_ID = "journal", ACTIVE_IND = "Y", GL_ACCOUNT_ID = credit, CREDIT_AMOUNT = amount, DEBIT_AMOUNT = 0 }
            }, new() {
                new GL_ENTRY { GL_ENTRY_ID = "d", JOURNAL_ENTRY_ID = "journal", ACTIVE_IND = "Y", GL_ACCOUNT_ID = debit, DEBIT_AMOUNT = amount },
                new GL_ENTRY { GL_ENTRY_ID = "c", JOURNAL_ENTRY_ID = "journal", ACTIVE_IND = "Y", GL_ACCOUNT_ID = credit, CREDIT_AMOUNT = amount }
            });

}
