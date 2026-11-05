using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProductionAccounting.Constants;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;
using PeriodClosingService = Beep.OilandGas.ProductionAccounting.Services.PeriodClosingService;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// The period close recognizes a closed period by the closing journal it writes, and scopes a field's production through
/// its run tickets. Driven against an in-memory data source holding copies, as a database would.
/// </summary>
public class PeriodCloseRecognitionTests
{
    private static readonly DateTime March = new(2026, 3, 31);
    private readonly Mock<IDataSource> _source = new();
    private readonly Dictionary<string, List<object>> _rows = new();
    private readonly PeriodClosingService _closing;

    public PeriodCloseRecognitionTests()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(e => e.GetDataSource("bound-production")).Returns(_source.Object);
        var metadata = new Mock<IPPDMMetadataRepository>();
        metadata.Setup(m => m.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync((string name) =>
            new PPDMTableMetadata { TableName = name, EntityTypeName = name, PrimaryKeyColumn = name + "_ID" });
        var defaults = new Mock<IPPDM39DefaultsRepository>();
        defaults.Setup(d => d.GetActiveIndicatorYes()).Returns("Y");
        defaults.Setup(d => d.FormatIdForTable(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object id) => id.ToString()!);
        var columns = Mock.Of<ICommonColumnHandler>();
        Task<string> Resolve() => Task.FromResult("bound-production");

        _source.Setup(s => s.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>()))
            .Returns((string table, List<AppFilter> filters) => Task.FromResult<IEnumerable<object>>(
                Rows(table).Where(row => filters.All(filter => Matches(row, filter))).Select(Copy).ToList()));
        _source.Setup(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()))
            .Callback((string table, object row) => Rows(table).Add(Copy(row)))
            .Returns(new ErrorsInfo { Flag = Errors.Ok });
        _source.Setup(s => s.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()))
            .Returns((string table, object row) =>
            {
                var key = row.GetType().GetProperty(table + "_ID")!;
                var rows = Rows(table);
                rows[rows.FindIndex(item => Equals(key.GetValue(item), key.GetValue(row)))] = Copy(row);
                return new ErrorsInfo { Flag = Errors.Ok };
            });

        var accounts = new GLAccountService(editor.Object, columns, defaults.Object, metadata.Object,
            NullLogger<GLAccountService>.Instance, Resolve);
        var journal = new JournalEntryService(editor.Object, columns, defaults.Object, metadata.Object, accounts,
            NullLogger<JournalEntryService>.Instance, Resolve);
        _closing = new PeriodClosingService(editor.Object, columns, defaults.Object, metadata.Object, journal, Resolve);

        foreach (var account in new[] { DefaultGlAccounts.Revenue, DefaultGlAccounts.RoyaltyExpense, DefaultGlAccounts.RetainedEarnings })
            Rows("GL_ACCOUNT").Add(new GL_ACCOUNT { GL_ACCOUNT_ID = account, ACCOUNT_NUMBER = account, ACTIVE_IND = "Y" });

        // Two fields measured on one lease: the lease names neither.
        AddProduction("a", "field-a", RoyaltyStatus.Calculated);
        AddProduction("b", "field-b", RoyaltyStatus.Accrued);
    }

    private List<object> Rows(string table) => _rows.TryGetValue(table, out var rows) ? rows : _rows[table] = new();

    private static object Copy(object row) => JsonSerializer.Deserialize(JsonSerializer.Serialize(row, row.GetType()), row.GetType())!;

    private static bool Matches(object row, AppFilter filter)
    {
        var value = row.GetType().GetProperty(filter.FieldName)?.GetValue(row);
        if (filter.Operator == "=") return value?.ToString() == filter.FilterValue;
        if (value is not DateTime date) return false;
        var bound = DateTime.Parse(filter.FilterValue, CultureInfo.InvariantCulture);
        return filter.Operator switch
        {
            "<=" => date <= bound,
            ">=" => date >= bound,
            "<" => date < bound,
            _ => throw new NotSupportedException(filter.Operator)
        };
    }

    private void AddProduction(string id, string fieldId, string royaltyStatus)
    {
        var day = new DateTime(2026, 3, 10);
        Rows("RUN_TICKET").Add(new RUN_TICKET { RUN_TICKET_ID = $"T-{id}", FIELD_ID = fieldId, LEASE_ID = "shared-lease", ACTIVE_IND = "Y" });
        Rows("ALLOCATION_RESULT").Add(new ALLOCATION_RESULT { ALLOCATION_RESULT_ID = $"AR-{id}", ALLOCATION_REQUEST_ID = $"T-{id}",
            ALLOCATION_DATE = day, ACTIVE_IND = "Y" });
        Rows("ALLOCATION_DETAIL").Add(new ALLOCATION_DETAIL { ALLOCATION_DETAIL_ID = $"D-{id}", ALLOCATION_RESULT_ID = $"AR-{id}", ACTIVE_IND = "Y" });
        Rows("ROYALTY_CALCULATION").Add(new ROYALTY_CALCULATION { ROYALTY_CALCULATION_ID = $"RC-{id}", ALLOCATION_RESULT_ID = $"AR-{id}",
            ALLOCATION_DETAIL_ID = $"D-{id}", PROPERTY_OR_LEASE_ID = "shared-lease", CALCULATION_DATE = day, ROYALTY_STATUS = royaltyStatus,
            ROYALTY_AMOUNT = 10m, ACTIVE_IND = "Y" });
        Rows("REVENUE_TRANSACTION").Add(new REVENUE_TRANSACTION { REVENUE_TRANSACTION_ID = $"RV-{id}", ALLOCATION_RESULT_ID = $"AR-{id}",
            PROPERTY_ID = "shared-lease", TRANSACTION_DATE = day, GROSS_REVENUE = 100m, OIL_VOLUME = 50m, ACTIVE_IND = "Y" });
        Rows("REVENUE_ALLOCATION").Add(new REVENUE_ALLOCATION { REVENUE_ALLOCATION_ID = $"RA-{id}", REVENUE_TRANSACTION_ID = $"RV-{id}", ACTIVE_IND = "Y" });
        Rows("MEASUREMENT_RECORD").Add(new MEASUREMENT_RECORD { MEASUREMENT_ID = $"M-{id}", RUN_TICKET_ID = $"T-{id}", LEASE_ID = "shared-lease",
            MEASUREMENT_DATETIME = day, NET_VOLUME = 50m, ACTIVE_IND = "Y" });
    }

    private void AddClosingJournal(string reference, string status, DateTime entryDate, string createdBy = "closer") =>
        Rows("JOURNAL_ENTRY").Add(new JOURNAL_ENTRY
        {
            JOURNAL_ENTRY_ID = Guid.NewGuid().ToString(), ENTRY_NUMBER = $"JE-{reference}-{status}", ENTRY_DATE = entryDate,
            ENTRY_TYPE = "GENERAL", STATUS = status, REFERENCE_NUMBER = reference, SOURCE_MODULE = AccountingSourceModuleCodes.PeriodClosing,
            TOTAL_DEBIT = 0m, TOTAL_CREDIT = 0m, ACTIVE_IND = "Y", ROW_CREATED_BY = createdBy, ROW_CREATED_DATE = entryDate.AddDays(1)
        });

    [Fact]
    public async Task ReadinessFollowsTheRunTicketsFieldNotTheLease()
    {
        Assert.False(await _closing.ValidateReadinessAsync("field-a", March));
        var outstanding = await _closing.GetUnreconciledItemsAsync("field-a", March);
        Assert.Contains("  - Royalty RC-a", outstanding);
        Assert.DoesNotContain(outstanding, item => item.Contains("RC-b", StringComparison.Ordinal));

        Assert.True(await _closing.ValidateReadinessAsync("field-b", March));
    }

    [Fact]
    public async Task AClosedPeriodReadsClosed_FromTheJournalTheCloseWrote()
    {
        Assert.Equal(PeriodCloseStatusCodes.Open, await _closing.GetPeriodStatusAsync("field-b", March));

        Assert.True(await _closing.ClosePeriodAsync("field-b", March, "actor-1"));

        Assert.Equal(PeriodCloseStatusCodes.Closed, await _closing.GetPeriodStatusAsync("field-b", new DateTime(2026, 3, 15)));
        Assert.Equal(PeriodCloseStatusCodes.Open, await _closing.GetPeriodStatusAsync("field-b", new DateTime(2026, 4, 30)));
        Assert.Equal(PeriodCloseStatusCodes.Open, await _closing.GetPeriodStatusAsync("field-a", March));
        var close = Assert.Single(await _closing.GetPeriodClosesAsync("field-b"));
        Assert.Equal(PeriodCloseStatusCodes.Closed, close.Status);
        Assert.Equal(March, close.PeriodEnd);
        Assert.Equal("actor-1", close.ClosedBy);

        // Only the field's royalty was accrued by its close.
        var royalties = Rows("ROYALTY_CALCULATION").Cast<ROYALTY_CALCULATION>().ToDictionary(r => r.ROYALTY_CALCULATION_ID);
        Assert.Equal(RoyaltyStatus.Calculated, royalties["RC-a"].ROYALTY_STATUS);
    }

    [Fact]
    public async Task APeriodWithNoActivityStillClosesAndReadsClosed()
    {
        Assert.True(await _closing.ClosePeriodAsync("field-empty", March, "actor-2"));

        Assert.Equal(PeriodCloseStatusCodes.Closed, await _closing.GetPeriodStatusAsync("field-empty", March));
        var journal = Assert.Single(Rows("JOURNAL_ENTRY").Cast<JOURNAL_ENTRY>());
        Assert.Equal(0m, journal.TOTAL_DEBIT ?? -1m);
        Assert.Empty(Rows("GL_ENTRY"));
    }

    [Fact]
    public async Task ClosingAClosedPeriodIsRefusedBeforeAnythingIsWritten()
    {
        Assert.True(await _closing.ClosePeriodAsync("field-b", March, "actor-1"));
        _source.Invocations.Clear();

        var refusal = await Assert.ThrowsAsync<RefusalException>(() => _closing.ClosePeriodAsync("field-b", March, "actor-1"));

        Assert.Equal(RefusalKind.Conflict, refusal.Kind);
        _source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _source.Verify(s => s.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        Assert.False(await _closing.ValidateReadinessAsync("field-b", March));
    }

    [Theory]
    [InlineData("POSTED", "CLOSED")]
    [InlineData("DRAFT", "CLOSING")]
    [InlineData("REVERSED", "REOPENED")]
    [InlineData("VOIDED", "OPEN")]
    public async Task StatusIsWhatTheClosingJournalSays(string journalStatus, string periodStatus)
    {
        AddClosingJournal("CLOSE-field-b-202603", journalStatus, March);

        Assert.Equal(periodStatus, await _closing.GetPeriodStatusAsync("field-b", March));
    }

    [Fact]
    public async Task ClosesAreListedForTheFieldNewestFirst()
    {
        AddClosingJournal("CLOSE-north-block-7-202602", "REVERSED", new DateTime(2026, 2, 28), "first");
        AddClosingJournal("CLOSE-north-block-7-202603", "POSTED", March, "second");
        AddClosingJournal("CLOSE-north-block-202603", "POSTED", March, "other field");
        AddClosingJournal("NOT-A-CLOSE", "POSTED", March, "not a close");

        var closes = await _closing.GetPeriodClosesAsync("north-block-7");

        Assert.Equal(new[] { "second", "first" }, closes.Select(c => c.ClosedBy));
        Assert.Equal(new[] { PeriodCloseStatusCodes.Closed, PeriodCloseStatusCodes.Reopened }, closes.Select(c => c.Status));
        Assert.Equal(3, (await _closing.GetPeriodClosesAsync(string.Empty)).Count);
    }
}
