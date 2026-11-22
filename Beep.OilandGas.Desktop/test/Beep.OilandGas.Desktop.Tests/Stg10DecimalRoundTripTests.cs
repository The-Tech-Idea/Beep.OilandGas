using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// STG-10 exit gate SG-10-9: every amount round-trips through persistence without precision loss.
/// </summary>
/// <remarks>
/// The money and rate precision this stage writes - two decimal places of currency, rates to four, volumes to four - is
/// written through <c>UnitofWork&lt;T&gt;</c> and read back row by row, asserting <b>exact decimal equality</b> rather than a
/// tolerance. The limit is stated rather than hidden: the SQLite provider stores a decimal as an IEEE double, so a value
/// needing more than about 15 significant digits is not representable in the store at all; the values proven here are the
/// ones this app produces.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class Stg10DecimalRoundTripTests
{
    private const string ConnectionName = "stg10-round-trip";

    /// <summary>Currency amounts the stage writes, including both ends of the cent and a large balance.</summary>
    private static readonly decimal[] Amounts =
    [
        0.01m, 0.10m, 0.99m, 1.00m, 12.34m, 393.75m, 1_234.56m, 99_999_999.99m, 123_456_789.12m,
    ];

    /// <summary>Rates and volumes: up to four decimal places.</summary>
    private static readonly decimal[] Rates =
    [
        0.075m, 0.125m, 0.0625m, 0.0001m, 0.21m, 12.5m, 1_234.5678m,
    ];

    [IntegrationFact]
    public async Task Every_amount_survives_the_round_trip_exactly()
    {        var root = Path.Combine(OgPaths.DataRoot, "stg10-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasStg10"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "roundtrip.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "roundtrip.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        // ---- money, through the journal line the ledger posts with ----
        var lines = OgUow.For<JOURNAL_ENTRY_LINE>(source);
        lines.PrimaryKey = "JOURNAL_ENTRY_LINE_ID";

        var lineIds = new Dictionary<string, decimal>(StringComparer.Ordinal);

        foreach (var amount in Amounts)
        {
            var id = "JL-" + Guid.NewGuid().ToString("N");

            lines.Add(new JOURNAL_ENTRY_LINE
            {
                JOURNAL_ENTRY_LINE_ID = id,
                JOURNAL_ENTRY_ID = "JE-1",
                GL_ACCOUNT_ID = "A-1000",
                LINE_NUMBER = lineIds.Count + 1,
                DEBIT_AMOUNT = amount,
                CREDIT_AMOUNT = 0m,
                ACTIVE_IND = "Y",
            });

            lineIds[id] = amount;
        }

        var lineCommitted = await lines.Commit().ConfigureAwait(false);
        Assert.True(OgUow.IsOk(lineCommitted), OgUow.Message(lineCommitted, "the lines were not written"));

        var readLines = await OgUow.ReadAllAsync<JOURNAL_ENTRY_LINE>(source).ConfigureAwait(false);

        foreach (var (id, expected) in lineIds)
        {
            var row = readLines.Single(candidate => candidate.JOURNAL_ENTRY_LINE_ID == id);

            // Exact equality, not a tolerance: the gate is precision, so a cent of drift is a failure.
            Assert.Equal(expected, row.DEBIT_AMOUNT ?? 0m);
        }

        // ---- rates and volumes, through the tax computation ----
        var taxes = OgUow.For<TAX_TRANSACTION>(source);
        taxes.PrimaryKey = "TAX_TRANSACTION_ID";

        var taxIds = new Dictionary<string, (decimal Rate, decimal Basis)>(StringComparer.Ordinal);

        foreach (var rate in Rates)
        {
            var id = "TX-" + Guid.NewGuid().ToString("N");
            var basis = rate * 3m;

            taxes.Add(new TAX_TRANSACTION
            {
                TAX_TRANSACTION_ID = id,
                TAX_TYPE = "SEVERANCE",
                TAX_DATE = new DateTime(2026, 12, 31),
                TAX_JURISDICTION = "TX",
                TAX_RATE = rate,
                TAX_BASIS = basis,
                TAX_AMOUNT = Math.Round(basis * rate, 2, MidpointRounding.AwayFromZero),
                ACTIVE_IND = "Y",
            });

            taxIds[id] = (rate, basis);
        }

        var taxCommitted = await taxes.Commit().ConfigureAwait(false);
        Assert.True(OgUow.IsOk(taxCommitted), OgUow.Message(taxCommitted, "the computations were not written"));

        var readTaxes = await OgUow.ReadAllAsync<TAX_TRANSACTION>(source).ConfigureAwait(false);

        foreach (var (id, expected) in taxIds)
        {
            var row = readTaxes.Single(candidate => candidate.TAX_TRANSACTION_ID == id);

            Assert.Equal(expected.Rate, row.TAX_RATE);
            Assert.Equal(expected.Basis, row.TAX_BASIS);
        }

        // ---- and once more through a fresh read of the database ----
        // The second connection proves the value survived the store, not a cache in the unit of work.
        Execute(database, "PRAGMA wal_checkpoint(TRUNCATE);");

        var reread = await OgUow.ReadAllAsync<JOURNAL_ENTRY_LINE>(source).ConfigureAwait(false);

        Assert.Equal(lineIds.Count, reread.Count);
        Assert.Equal(
            Amounts.OrderBy(amount => amount).ToList(),
            reread.Select(row => row.DEBIT_AMOUNT ?? 0m).OrderBy(amount => amount).ToList());

        var highest = reread.Max(row => row.DEBIT_AMOUNT ?? 0m);
        Assert.Equal(123_456_789.12m, highest);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE JOURNAL_ENTRY_LINE (
                JOURNAL_ENTRY_LINE_ID TEXT PRIMARY KEY,
                JOURNAL_ENTRY_ID TEXT,
                GL_ACCOUNT_ID TEXT,
                LINE_NUMBER INTEGER,
                DEBIT_AMOUNT REAL,
                CREDIT_AMOUNT REAL,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                COST_CENTER_ID TEXT,
                CURRENCY_CODE TEXT,
                EXCHANGE_RATE REAL,
                SHADOW_DEBIT_AMOUNT REAL,
                SHADOW_CREDIT_AMOUNT REAL,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE TAX_TRANSACTION (
                TAX_RATE REAL,
                TAX_BASIS REAL,
                BASIS_TEXT TEXT,
                RATE_EFFECTIVE_DATE TEXT,
                RATE_SOURCE TEXT,
                PERIOD_FROM TEXT,
                PERIOD_TO TEXT,
                FISCAL_YEAR TEXT,
                CURRENCY_CODE TEXT,
                STATUS TEXT,
                AUTHOR TEXT,
                JOURNAL_ENTRY_ID TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                TAX_TRANSACTION_ID TEXT PRIMARY KEY,
                PROPERTY_ID TEXT,
                TAX_TYPE TEXT,
                TAX_DATE TEXT,
                TAX_AMOUNT REAL,
                TAX_JURISDICTION TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                REMARKS TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class Recording : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) =>
            FailureReferenceFormat.New();
    }
}
