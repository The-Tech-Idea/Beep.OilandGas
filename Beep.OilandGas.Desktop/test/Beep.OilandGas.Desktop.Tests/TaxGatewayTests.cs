using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.ProductionAccounting.Constants;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-08 against a real SQLite database: a production tax is computed from the configured rate and names its jurisdiction,
/// rate and effective date (AC-1, AC-5); a jurisdiction with no rate, and a rate that starts after the period, are both
/// refused (AC-2); a provision is refused until every month of the year is closed (AC-3, SG-10-8); posting writes exactly one
/// journal per computation and the author cannot post it (AC-4, AC-6); and a deferred provision posts its movement, not its
/// balance. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class TaxGatewayTests
{
    private const string ConnectionName = "acc08-tax";

    [IntegrationFact]
    public async Task Tax_is_computed_from_configured_rates_and_posted_once()
    {        var root = Path.Combine(OgPaths.DataRoot, "acc08-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAcc08"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "tax.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "tax.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgTaxService(() => source, reporter);
        var context = new OgTaxContext("SMOKE", "MAIN");

        // ---- the rate table is read, and nothing is entered here (AC-5) --------
        var loaded = await service.LoadAsync(context);

        Assert.True(loaded.Success, loaded.Message);
        Assert.False(loaded.Value!.HistoryFailed);
        Assert.Empty(loaded.Value.Computations);

        Assert.Equal(3, loaded.Value.Jurisdictions.Count);

        var texas = loaded.Value.Jurisdictions.Single(row => row.Jurisdiction == "TX");

        Assert.Equal(7.5m, texas.SeveranceRate);
        Assert.Equal(1.5m, texas.AdValoremRate);
        Assert.Equal(21m, texas.IncomeTaxRate);
        Assert.Equal(new DateTime(2026, 1, 1), texas.EffectiveFrom);
        Assert.Contains("severance 7.5 %", texas.RatesText, StringComparison.Ordinal);

        // The fiscal year is open, so the workspace says provisions are blocked.
        Assert.False(loaded.Value.FiscalYearComplete);
        Assert.Contains("incomplete", loaded.Value.FiscalYearText, StringComparison.Ordinal);

        // ---- AC-1: a production tax names its jurisdiction, rate and date ------
        var severance = await service.ComputeAsync(
            Inputs() with { Kind = ProductionTaxAssessmentCodes.Severance }, context, "sara");

        Assert.True(severance.Success, severance.Message + " :: " + string.Join(" | ", reporter.Failures));

        var tax = severance.Value!.SelectedComputation!;

        // 10,000 gross at 7.5 % is 750.
        Assert.Equal(10_000m, tax.Basis);
        Assert.Equal(0.075m, tax.Rate);
        Assert.Equal(750m, tax.Amount);
        Assert.Equal("TX", tax.Jurisdiction);
        Assert.Equal(new DateTime(2026, 1, 1), tax.EffectiveFrom);
        Assert.True(tax.NamesItsBasis);
        Assert.Equal(TaxComputationCodes.StatusComputed, tax.Status);

        Assert.Equal(1, Count(database,
            "TAX_TRANSACTION WHERE TAX_TYPE = 'SEVERANCE' AND TAX_JURISDICTION = 'TX' AND TAX_AMOUNT = 750 " +
            "AND TAX_RATE = 0.075 AND TAX_BASIS = 10000 AND RATE_EFFECTIVE_DATE IS NOT NULL " +
            "AND RATE_SOURCE LIKE 'GOVERNMENTAL_TAX_DATA%' AND STATUS = 'COMPUTED' AND AUTHOR = 'sara'"));

        // ---- AC-2: a jurisdiction with no rate row is refused ------------------
        var unknown = await service.ComputeAsync(
            Inputs() with { Jurisdiction = "WY", Kind = ProductionTaxAssessmentCodes.Severance }, context, "sara");

        Assert.False(unknown.Success);
        Assert.Equal(TaxRules.CodeUnknownJurisdiction, unknown.Reference);
        Assert.Contains("WY", unknown.Message!, StringComparison.Ordinal);

        // ... and one whose row carries no provision rate is refused naming it.
        var noRate = await service.ComputeAsync(
            Inputs() with { Jurisdiction = "OK", Kind = TaxComputationCodes.ProvisionCurrent, GrossRevenue = null,
                TaxableIncome = 100_000m },
            context, "sara");

        Assert.False(noRate.Success);
        Assert.Equal(TaxRules.CodeNoRate, noRate.Reference);
        Assert.Contains("OK", noRate.Message!, StringComparison.Ordinal);

        // A rate that only takes effect after the period is refused with its date.
        var notEffective = await service.ComputeAsync(
            Inputs() with { Jurisdiction = "NM", Kind = TaxComputationCodes.ProvisionCurrent, GrossRevenue = null,
                TaxableIncome = 100_000m },
            context, "sara");

        Assert.False(notEffective.Success);
        Assert.Equal(TaxRules.CodeRateNotEffective, notEffective.Reference);
        Assert.Contains("2027-01-01", notEffective.Message!, StringComparison.Ordinal);

        Assert.Equal(1, Count(database, "TAX_TRANSACTION"));

        // ---- AC-3 and SG-10-8: a provision needs the year closed --------------
        var tooEarly = await service.ComputeAsync(
            Inputs() with { Kind = TaxComputationCodes.ProvisionCurrent, GrossRevenue = null,
                TaxableIncome = 100_000m },
            context, "sara");

        Assert.False(tooEarly.Success);
        Assert.Equal(TaxRules.CodeFiscalYearIncomplete, tooEarly.Reference);
        Assert.Contains("2026-01", tooEarly.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "TAX_TRANSACTION"));

        // With the year complete the same provision goes through: 100,000 at 21 % is 21,000. Every month before the
        // period-end month is closed; December stays open because the provision is one of its closing entries.
        Execute(database,
            "UPDATE COMPANY_LEDGER_PERIOD SET STATUS = 'CLOSED' WHERE PERIOD_CODE <> '2026-12';");

        var provision = await service.ComputeAsync(
            Inputs() with { Kind = TaxComputationCodes.ProvisionCurrent, GrossRevenue = null,
                TaxableIncome = 100_000m },
            context, "sara");

        Assert.True(provision.Success, provision.Message);

        var current = provision.Value!.SelectedComputation!;

        Assert.Equal(21_000m, current.Amount);
        Assert.Equal(0.21m, current.Rate);
        Assert.Equal("taxable income", current.BasisText);
        Assert.Equal(2, Count(database, "TAX_TRANSACTION"));

        // ---- AC-6: the author cannot post ------------------------------------
        var byAuthor = await service.PostAsync(current.Id, context, "sara");

        Assert.False(byAuthor.Success);
        Assert.Equal(TaxRules.CodeAuthorCannotPost, byAuthor.Reference);
        Assert.Contains("sara", byAuthor.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "JOURNAL_ENTRY"));

        // ---- AC-4: posting writes exactly one journal ------------------------
        var posted = await service.PostAsync(current.Id, context, "nadia");

        Assert.True(posted.Success, posted.Message);

        var postedTax = posted.Value!.SelectedComputation!;

        Assert.Equal(TaxComputationCodes.StatusPosted, postedTax.Status);
        Assert.NotNull(postedTax.JournalId);

        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'TAX' AND TOTAL_DEBIT = 21000 AND TOTAL_CREDIT = 21000"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 21000 AND GL_ACCOUNT_ID = 'A-6150'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 21000 AND GL_ACCOUNT_ID = 'A-2101'"));

        // Posting again writes no second entry.
        Assert.True((await service.PostAsync(current.Id, context, "nadia")).Success);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'TAX'"));

        // A lost acknowledgement adopts the entry it already wrote.
        Execute(database,
            $"UPDATE TAX_TRANSACTION SET JOURNAL_ENTRY_ID = NULL, STATUS = 'COMPUTED', POSTED_BY = NULL, " +
            $"POSTED_AT = NULL WHERE TAX_TRANSACTION_ID = '{current.Id}';");

        Assert.True((await service.PostAsync(current.Id, context, "nadia")).Success);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'TAX'"));

        // ---- a deferred provision posts its movement, not its balance --------
        var firstDeferred = await service.ComputeAsync(
            Inputs() with { Kind = TaxComputationCodes.ProvisionDeferred, GrossRevenue = null,
                TaxableIncome = 100_000m, DeferredTaxAsset = 0m, DeferredTaxLiability = 5_000m },
            context, "sara");

        Assert.True(firstDeferred.Success, firstDeferred.Message);
        Assert.Equal(5_000m, firstDeferred.Value!.SelectedComputation!.Amount);

        Assert.True((await service.PostAsync(firstDeferred.Value.SelectedComputation.Id, context, "nadia")).Success);

        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 5000 AND GL_ACCOUNT_ID = 'A-6150'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 5000 AND GL_ACCOUNT_ID = 'A-2102'"));
        Assert.Equal(1, Count(database,
            "DEFERRED_TAX_BALANCE WHERE DEFERRED_TAX_LIABILITY = 5000"));

        // The second provision sees the first as its prior balance, so it posts only the 1,000 increase.
        var secondDeferred = await service.ComputeAsync(
            Inputs() with { Kind = TaxComputationCodes.ProvisionDeferred, GrossRevenue = null,
                TaxableIncome = 100_000m, DeferredTaxAsset = 0m, DeferredTaxLiability = 6_000m },
            context, "sara");

        Assert.True(secondDeferred.Success, secondDeferred.Message);
        Assert.Equal(1_000m, secondDeferred.Value!.SelectedComputation!.Amount);

        Assert.True((await service.PostAsync(secondDeferred.Value.SelectedComputation.Id, context, "nadia")).Success);
        Assert.Equal(2, Count(database, "DEFERRED_TAX_BALANCE"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'TAX' AND TOTAL_DEBIT = 1000"));

        // ---- nothing to compute is said, not posted --------------------------
        var zero = await service.ComputeAsync(
            Inputs() with { Kind = ProductionTaxAssessmentCodes.Severance, GrossRevenue = 0m }, context, "sara");

        Assert.False(zero.Success);
        Assert.Equal(TaxRules.CodeNothingToPost, zero.Reference);

        // ---- the register, reloaded ------------------------------------------
        var reloaded = await service.LoadAsync(context);

        Assert.True(reloaded.Success, reloaded.Message);
        Assert.Equal(4, reloaded.Value!.Computations.Count);
        Assert.Equal(2, reloaded.Value.Deferred.Count);
        Assert.True(reloaded.Value.FiscalYearComplete);

        // ---- an unavailable register is a state ------------------------------
        Execute(database, "DROP TABLE TAX_TRANSACTION;");
        Execute(database, "DROP TABLE GOVERNMENTAL_TAX_DATA;");

        var unavailable = await service.LoadAsync(context);

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Computations);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database,
            """
            CREATE TABLE GOVERNMENTAL_TAX_DATA (
                GOVERNMENTAL_TAX_DATA_ID TEXT PRIMARY KEY,
                SEVERANCE_TAX REAL,
                AD_VALOREM_TAX REAL,
                TOTAL_TAXES REAL,
                ROW_ID TEXT,
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

            CREATE TABLE DEFERRED_TAX_BALANCE (
                DEFERRED_TAX_BALANCE_ID TEXT PRIMARY KEY,
                PROPERTY_ID TEXT,
                PERIOD_END_DATE TEXT,
                DEFERRED_TAX_ASSET REAL,
                DEFERRED_TAX_LIABILITY REAL,
                NOTES TEXT,
                ROW_ID TEXT,
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

            CREATE TABLE GL_ACCOUNT (
                CONFIGURATION_COMMAND_JSON TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                CURRENCY_CODE TEXT,
                GL_ACCOUNT_ID TEXT PRIMARY KEY,
                ACCOUNT_NUMBER TEXT,
                ACCOUNT_NAME TEXT,
                ACCOUNT_TYPE TEXT,
                PARENT_ACCOUNT_ID TEXT,
                NORMAL_BALANCE TEXT,
                OPENING_BALANCE REAL,
                CURRENT_BALANCE REAL,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
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

            CREATE TABLE JOURNAL_ENTRY (
                AUTHOR TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                PERIOD_CODE TEXT,
                REVERSAL_OF_ENTRY_ID TEXT,
                REVERSED_AT TEXT,
                JOURNAL_ENTRY_ID TEXT PRIMARY KEY,
                ENTRY_NUMBER TEXT,
                ENTRY_DATE TEXT,
                ENTRY_TYPE TEXT,
                STATUS TEXT,
                DESCRIPTION TEXT,
                REFERENCE_NUMBER TEXT,
                SOURCE_MODULE TEXT,
                TOTAL_DEBIT REAL,
                TOTAL_CREDIT REAL,
                ROW_ID TEXT,
                JOURNAL_DATE TEXT,
                JOURNAL_TYPE TEXT,
                JOURNAL_DESCRIPTION TEXT,
                SOURCE_SYSTEM TEXT,
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

            CREATE TABLE COMPANY_LEDGER_PERIOD (
                COMPANY_LEDGER_PERIOD_ID TEXT PRIMARY KEY,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                CURRENCY_CODE TEXT,
                CALENDAR_ID TEXT,
                PERIOD_CODE TEXT,
                START_DATE TEXT,
                END_DATE TEXT,
                STATUS TEXT,
                AUTHORITY_JSON TEXT,
                STATE_HISTORY_JSON TEXT,
                CLOSED_BY TEXT,
                CLOSED_AT TEXT,
                REOPENED_BY TEXT,
                REOPENED_AT TEXT,
                REOPEN_REASON TEXT,
                CLOSE_NOTE TEXT,
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

            INSERT INTO GL_ACCOUNT (GL_ACCOUNT_ID, ACCOUNT_NUMBER, ACCOUNT_NAME, ACCOUNT_TYPE, NORMAL_BALANCE,
                CURRENCY_CODE, ACTIVE_IND) VALUES
                ('A-6150', '6150', 'Income tax expense', 'EXPENSE', 'DEBIT', 'USD', 'Y'),
                ('A-2101', '2101', 'Income tax payable', 'LIABILITY', 'CREDIT', 'USD', 'Y'),
                ('A-1160', '1160', 'Deferred tax asset', 'ASSET', 'DEBIT', 'USD', 'Y'),
                ('A-2102', '2102', 'Deferred tax liability', 'LIABILITY', 'CREDIT', 'USD', 'Y'),
                ('A-6001', '6001', 'Operating expense', 'EXPENSE', 'DEBIT', 'USD', 'Y'),
                ('A-2000', '2000', 'Accounts payable', 'LIABILITY', 'CREDIT', 'USD', 'Y');

            INSERT INTO GOVERNMENTAL_TAX_DATA (GOVERNMENTAL_TAX_DATA_ID, SEVERANCE_TAX, AD_VALOREM_TAX,
                TOTAL_TAXES, REMARK, EFFECTIVE_DATE, ACTIVE_IND) VALUES
                ('GT-1', 7.5, 1.5, 9, 'JURISDICTION=TX; DEFERRED_TAX_RATE=21; PROPERTY_ID=PR-1', '2026-01-01', 'Y'),
                ('GT-2', 5, NULL, 5, 'JURISDICTION=OK', '2026-01-01', 'Y'),
                ('GT-3', NULL, NULL, NULL, 'JURISDICTION=NM; DEFERRED_TAX_RATE=5.9', '2027-01-01', 'Y');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-01', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-01', '2026-01-01', '2026-01-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-02', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-02', '2026-02-01', '2026-02-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-03', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-03', '2026-03-01', '2026-03-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-04', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-04', '2026-04-01', '2026-04-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-05', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-05', '2026-05-01', '2026-05-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-06', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-06', '2026-06-01', '2026-06-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-07', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-07', '2026-07-01', '2026-07-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-08', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-08', '2026-08-01', '2026-08-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-09', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-09', '2026-09-01', '2026-09-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-10', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-10', '2026-10-01', '2026-10-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-11', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-11', '2026-11-01', '2026-11-28', 'OPEN', '{}');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES ('P-12', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-12', '2026-12-01', '2026-12-28', 'OPEN', '{}');
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static long Count(string database, string fromAndWhere)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM " + fromAndWhere + ";";

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static OgTaxInputs Inputs() =>
        new(
            "TX",
            ProductionTaxAssessmentCodes.Severance,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31),
            "USD",
            GrossRevenue: 10_000m,
            TaxableIncome: null,
            DeferredTaxAsset: null,
            DeferredTaxLiability: null,
            PropertyId: "PR-1");

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
