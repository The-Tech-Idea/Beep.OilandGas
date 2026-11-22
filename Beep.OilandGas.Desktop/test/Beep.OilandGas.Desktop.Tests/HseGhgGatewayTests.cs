using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-06 against a real SQLite database: the report is built from the recorded activity only when a usable catalog and a
/// current review exist (AC-1, AC-3), every line names its factor, unit, source and revision (AC-2), an unrepresentable value
/// refuses the report (AC-4), the unavailable state is reported apart from a zero (AC-5), and the export carries exactly the
/// on-screen figures (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HseGhgGatewayTests
{
    private const string ConnectionName = "hse06-ghg";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    private const string CatalogJson =
        @"{""jurisdiction"":""TX"",""factors"":[{""id"":""F-FLARE"",""sourceCategory"":""FLARE"",""inputUnit"":""m3"",""outputUnit"":""tCO2e"",""factor"":0.002,""effectiveFrom"":""2026-01-01"",""effectiveThrough"":""2026-12-31"",""sourceReference"":""EPA 40 CFR 98 Table C-1""},{""id"":""F-VENT"",""sourceCategory"":""VENT"",""inputUnit"":""m3"",""outputUnit"":""tCO2e"",""factor"":0.001,""effectiveFrom"":""2026-01-01"",""effectiveThrough"":""2026-12-31"",""sourceReference"":""EPA 40 CFR 98 Table W-1""}]}";

    [IntegrationFact]
    public async Task A_report_needs_a_usable_catalog_a_current_review_and_recorded_activity()
    {        var root = Path.Combine(OgPaths.DataRoot, "hse06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasHse06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "ghg.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "ghg.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var ghg = new OgGhg(() => source, reporter);

        // ---- AC-1, AC-2, AC-5: a report, from the recorded rows ----
        var loaded = await ghg.LoadAsync(Scope, 2026, "TX");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;

        Assert.True(workspace.IsAvailable, workspace.UnavailableText);
        Assert.True(workspace.Catalog.IsPresent);
        Assert.Equal("cat-rev-1", workspace.Catalog.Revision);
        Assert.Equal(GhgRules.ReviewCurrent, workspace.Review.State);

        // Two activities for the field and year: the flare and the vent. The other field's row and the other year are out.
        Assert.Equal(2, workspace.Activity.Count);
        Assert.DoesNotContain(workspace.Activity, row => row.EntityId == "PDEN-F2");
        Assert.DoesNotContain(workspace.Activity, row => row.EntityId == "PDEN-OLD");

        var report = workspace.Report!;

        Assert.Equal(2, report.Lines.Count);
        Assert.Equal(2_000m, report.Lines.Single(line => line.FactorId == "F-FLARE").Emissions);
        Assert.Equal(5m, report.Lines.Single(line => line.FactorId == "F-VENT").Emissions);
        Assert.Equal(2_005m, report.TotalCo2e);
        Assert.Contains("2005 tCO2e", report.TotalText, StringComparison.Ordinal);

        // AC-2: every line names its factor, unit, revision and source.
        Assert.All(report.Lines, line =>
        {
            Assert.False(string.IsNullOrWhiteSpace(line.FactorId));
            Assert.Equal("tCO2e", line.FactorUnit);
            Assert.Equal("cat-rev-1", line.Revision);
            Assert.StartsWith("EPA 40 CFR 98", line.SourceReference, StringComparison.Ordinal);
        });

        // AC-6: the export carries the same figures the lines show.
        Assert.Contains("2000", report.Csv(), StringComparison.Ordinal);
        Assert.Contains("total,\"tCO2e\",2005", report.Csv(), StringComparison.Ordinal);
        Assert.Equal(report.Csv(), workspace.ExportText);

        // ---- AC-1: no usable catalog for the jurisdiction ----
        var noCatalog = await ghg.LoadAsync(Scope, 2026, "ND");

        Assert.True(noCatalog.Success, noCatalog.Message);
        Assert.False(noCatalog.Value!.IsAvailable);
        Assert.Null(noCatalog.Value.Report);
        Assert.Equal("unavailable", noCatalog.Value.TotalText);
        Assert.Contains("no factor catalog is recorded for jurisdiction ND",
            noCatalog.Value.UnavailableText, StringComparison.Ordinal);

        // A missing jurisdiction is refused rather than defaulted to a country.
        var noJurisdiction = await ghg.LoadAsync(Scope, 2026, null);

        Assert.True(noJurisdiction.Success, noJurisdiction.Message);
        Assert.False(noJurisdiction.Value!.IsAvailable);
        Assert.Contains("no jurisdiction is selected", noJurisdiction.Value.UnavailableText,
            StringComparison.Ordinal);
        Assert.DoesNotContain("USA", noJurisdiction.Value.UnavailableText, StringComparison.OrdinalIgnoreCase);

        // ---- AC-1: no recorded activity for the year ----
        var noActivity = await ghg.LoadAsync(Scope, 2024, "TX");

        Assert.True(noActivity.Success, noActivity.Message);
        Assert.False(noActivity.Value!.IsAvailable);
        Assert.Contains("no verified activity", noActivity.Value.UnavailableText, StringComparison.Ordinal);

        // ---- AC-3: the catalog moved since the review ----
        Execute(database, "UPDATE SYSTEM_SETUP_RECORD SET REVISION = 'cat-rev-2' WHERE SETUP_KEY = 'TX';");

        var catalogChanged = await ghg.LoadAsync(Scope, 2026, "TX");

        Assert.True(catalogChanged.Success, catalogChanged.Message);
        Assert.False(catalogChanged.Value!.IsAvailable);
        Assert.Equal(GhgRules.ReviewCatalogChanged, catalogChanged.Value.Review.State);
        Assert.Contains("cat-rev-1", catalogChanged.Value.Review.Sentence, StringComparison.Ordinal);
        Assert.Contains("cat-rev-2", catalogChanged.Value.Review.Sentence, StringComparison.Ordinal);

        // The selection command records the new revision and the review becomes current again.
        var selected = await ghg.SelectAsync(Scope, 2026, "TX", "Re-reviewed after the 2026 factor update.", "sara");

        Assert.True(selected.Success, selected.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(GhgRules.ReviewCurrent, selected.Value!.Review.State);
        Assert.True(selected.Value.IsAvailable, selected.Value.UnavailableText);
        Assert.Equal(1, Count(database, "COMPANY_GHG_CATALOG_SELECTION WHERE JURISDICTION_KEY = 'TX'"));
        Assert.Equal(1, Count(database,
            "COMPANY_GHG_CATALOG_SELECTION WHERE JURISDICTION_KEY = 'TX' AND CATALOG_REVISION = 'cat-rev-2' AND SELECTED_BY = 'sara'"));

        // ---- AC-3: the environment moved since the review ----
        Execute(database,
            "UPDATE COMPANY_OILGAS_ENVIRONMENT SET REVISION = 'env-rev-2' WHERE COMPANY_OILGAS_ENVIRONMENT_ID = 'ENV-1';");

        var environmentChanged = await ghg.LoadAsync(Scope, 2026, "TX");

        Assert.True(environmentChanged.Success, environmentChanged.Message);
        Assert.False(environmentChanged.Value!.IsAvailable);
        Assert.Equal(GhgRules.ReviewEnvironmentChanged, environmentChanged.Value.Review.State);
        Assert.Contains("env-rev-2", environmentChanged.Value.Review.Sentence, StringComparison.Ordinal);

        // Re-reviewing after the environment change makes the review current again, so only the arithmetic can refuse now.
        var afterEnvironment = await ghg.SelectAsync(Scope, 2026, "TX",
            "Re-reviewed after the company environment change.", "sara");

        Assert.True(afterEnvironment.Success, afterEnvironment.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(GhgRules.ReviewCurrent, afterEnvironment.Value!.Review.State);

        // ---- AC-4: an unrepresentable product refuses the report ----
        Execute(database,
            "UPDATE SYSTEM_SETUP_RECORD SET SETTINGS_JSON = REPLACE(SETTINGS_JSON, '0.002', '79228162514264337593543950335') WHERE SETUP_KEY = 'TX';");

        var overflow = await ghg.LoadAsync(Scope, 2026, "TX");

        Assert.True(overflow.Success, overflow.Message);
        Assert.False(overflow.Value!.IsAvailable);
        Assert.Contains("cannot be represented", overflow.Value.UnavailableText, StringComparison.Ordinal);
        Assert.Equal("unavailable", overflow.Value.TotalText);

        // ---- a selection without its jurisdiction or rationale is refused ----
        var noSelectionJurisdiction = await ghg.SelectAsync(Scope, 2026, "  ", "why", "sara");

        Assert.False(noSelectionJurisdiction.Success);
        Assert.Equal(GhgRules.CodeJurisdictionRequired, noSelectionJurisdiction.Reference);

        var noRationale = await ghg.SelectAsync(Scope, 2026, "TX", "  ", "sara");

        Assert.False(noRationale.Success);
        Assert.Equal(GhgRules.CodeRationaleRequired, noRationale.Reference);

        // A field is required: no report is produced for "all fields".
        var noField = await ghg.LoadAsync(new OgHseScope("SMOKE", null), 2026, "TX");

        Assert.False(noField.Success);
        Assert.Equal(GhgRules.CodeFieldRequired, noField.Reference);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database)
    {
        Execute(database, """
            CREATE TABLE SYSTEM_SETUP_RECORD (
                SYSTEM_SETUP_RECORD_ID TEXT,
                CATEGORY TEXT,
                SETUP_KEY TEXT,
                VERSION TEXT,
                SETTINGS_JSON TEXT,
                REVISION TEXT,
                PREVIOUS_REVISION TEXT,
                INTENT_ID TEXT,
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
                SOURCE TEXT,
                PRIMARY KEY (SYSTEM_SETUP_RECORD_ID));

            CREATE TABLE COMPANY_GHG_CATALOG_SELECTION (
                COMPANY_GHG_CATALOG_SELECTION_ID TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                JURISDICTION_KEY TEXT,
                CATALOG_REVISION TEXT,
                ENVIRONMENT_REVISION TEXT,
                SELECTED_BY TEXT,
                RATIONALE TEXT,
                REVISION TEXT,
                PREVIOUS_REVISION TEXT,
                INTENT_ID TEXT,
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
                SOURCE TEXT,
                PRIMARY KEY (COMPANY_GHG_CATALOG_SELECTION_ID));

            CREATE TABLE COMPANY_OILGAS_ENVIRONMENT (
                COMPANY_OILGAS_ENVIRONMENT_ID TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                COUNTRY_CODE TEXT,
                OVERRIDES_JSON TEXT,
                REVISION TEXT,
                PREVIOUS_REVISION TEXT,
                INTENT_ID TEXT,
                TOOLS_VERSION TEXT,
                REVIEWED_DRAFT_REVISION TEXT,
                REVIEWED_SYSTEM_REVISION TEXT,
                REVIEWED_COUNTRY_REVISION TEXT,
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
                SOURCE TEXT,
                PRIMARY KEY (COMPANY_OILGAS_ENVIRONMENT_ID));

            CREATE TABLE PDEN (
                PDEN_SUBTYPE TEXT,
                PDEN_ID TEXT,
                SOURCE TEXT,
                ACTIVE_IND TEXT,
                AREA_ID TEXT,
                AREA_TYPE TEXT,
                CURRENT_OPERATOR TEXT,
                CURRENT_PROD_STR_NAME TEXT,
                CURRENT_STATUS_DATE TEXT,
                CURRENT_WELL_STR_NUMBER TEXT,
                EFFECTIVE_DATE TEXT,
                ENHANCED_RECOVERY_TYPE TEXT,
                EXPIRY_DATE TEXT,
                FIELD_ID TEXT,
                LAST_INJECTION_DATE TEXT,
                LAST_PRODUCTION_DATE TEXT,
                LAST_REPORTED_DATE TEXT,
                LOCATION_DESC TEXT,
                LOCATION_DESC_TYPE TEXT,
                ON_INJECTION_DATE TEXT,
                ON_PRODUCTION_DATE TEXT,
                PDEN_LONG_NAME TEXT,
                PDEN_SHORT_NAME TEXT,
                PDEN_STATUS TEXT,
                PDEN_STATUS_TYPE TEXT,
                PLOT_NAME TEXT,
                PLOT_SYMBOL TEXT,
                POOL_ID TEXT,
                PPDM_GUID TEXT,
                PRIMARY_PRODUCT TEXT,
                PRODUCTION_METHOD TEXT,
                PROPRIETARY_IND TEXT,
                REMARK TEXT,
                STATE_OR_FEDERAL_WATERS TEXT,
                STRAT_NAME_SET_ID TEXT,
                STRAT_UNIT_ID TEXT,
                STRING_SERIAL_NUMBER TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (PDEN_SUBTYPE, PDEN_ID, SOURCE));

            CREATE TABLE PDEN_VOL_DISPOSITION (
                PDEN_SUBTYPE TEXT,
                PDEN_ID TEXT,
                PDEN_SOURCE TEXT,
                VOLUME_METHOD TEXT,
                ACTIVITY_TYPE TEXT,
                PRODUCT_TYPE TEXT,
                PERIOD_TYPE TEXT,
                DISPOSITION_OBS_NO REAL,
                AMENDMENT_SEQ_NO REAL,
                ACTIVE_IND TEXT,
                AMEND_REASON TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                POSTED_DATE TEXT,
                PPDM_GUID TEXT,
                RECEIVING_PDEN_ID TEXT,
                RECEIVING_PDEN_SUBTYPE TEXT,
                REMARK TEXT,
                REPORT_IND TEXT,
                SOURCE TEXT,
                VOLUME REAL,
                VOLUME_END_DATE TEXT,
                VOLUME_END_DATE_DESC TEXT,
                VOLUME_OUOM TEXT,
                VOLUME_PERIOD REAL,
                VOLUME_PERIOD_OUOM TEXT,
                VOLUME_QUALITY REAL,
                VOLUME_QUALITY_OUOM TEXT,
                VOLUME_START_DATE TEXT,
                VOLUME_UOM TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (PDEN_SUBTYPE, PDEN_ID, PDEN_SOURCE, VOLUME_METHOD, ACTIVITY_TYPE, PRODUCT_TYPE, PERIOD_TYPE, DISPOSITION_OBS_NO, AMENDMENT_SEQ_NO));
""");

        Execute(database, """
            INSERT INTO SYSTEM_SETUP_RECORD (SYSTEM_SETUP_RECORD_ID, CATEGORY, SETUP_KEY, VERSION, SETTINGS_JSON,
                REVISION, ACTIVE_IND) VALUES
                ('SYS-GHG-TX', 'GHG_FACTORS', 'TX', '1', '{"jurisdiction":"TX","factors":[{"id":"F-FLARE","sourceCategory":"FLARE","inputUnit":"m3","outputUnit":"tCO2e","factor":0.002,"effectiveFrom":"2026-01-01","effectiveThrough":"2026-12-31","sourceReference":"EPA 40 CFR 98 Table C-1"},{"id":"F-VENT","sourceCategory":"VENT","inputUnit":"m3","outputUnit":"tCO2e","factor":0.001,"effectiveFrom":"2026-01-01","effectiveThrough":"2026-12-31","sourceReference":"EPA 40 CFR 98 Table W-1"}]}', 'cat-rev-1', 'Y');

            INSERT INTO COMPANY_OILGAS_ENVIRONMENT (COMPANY_OILGAS_ENVIRONMENT_ID, OWNER_TENANT_ID,
                OWNER_COMPANY_BA_ID, COUNTRY_CODE, OVERRIDES_JSON, REVISION, TOOLS_VERSION, REVIEWED_DRAFT_REVISION,
                REVIEWED_SYSTEM_REVISION, ACTIVE_IND) VALUES
                ('ENV-1', 'SMOKE', 'SMOKE', 'US', '{{}}', 'env-rev-1', '1.0', 'draft-1', 'system-1', 'Y');

            INSERT INTO COMPANY_GHG_CATALOG_SELECTION (COMPANY_GHG_CATALOG_SELECTION_ID, OWNER_TENANT_ID,
                OWNER_COMPANY_BA_ID, JURISDICTION_KEY, CATALOG_REVISION, ENVIRONMENT_REVISION, SELECTED_BY, RATIONALE,
                REVISION, INTENT_ID, ACTIVE_IND) VALUES
                ('GHG-SMOKE-TX', 'SMOKE', 'SMOKE', 'TX', 'cat-rev-1', 'env-rev-1', 'sara',
                 'Chosen for the 2026 filing.', 'sel-rev-1', 'sel-rev-1', 'Y');

            INSERT INTO PDEN (PDEN_SUBTYPE, PDEN_ID, SOURCE, FIELD_ID, PDEN_LONG_NAME, PRIMARY_PRODUCT, ACTIVE_IND) VALUES
                ('WELL', 'PDEN-F1-A', 'PPDM', 'F-1', 'North 1 flare', 'GAS', 'Y'),
                ('WELL', 'PDEN-F1-B', 'PPDM', 'F-1', 'North 1 vent', 'GAS', 'Y'),
                ('WELL', 'PDEN-F2', 'PPDM', 'F-2', 'South 1 flare', 'GAS', 'Y');

            INSERT INTO PDEN_VOL_DISPOSITION (PDEN_SUBTYPE, PDEN_ID, PDEN_SOURCE, VOLUME_METHOD, ACTIVITY_TYPE,
                PRODUCT_TYPE, PERIOD_TYPE, DISPOSITION_OBS_NO, AMENDMENT_SEQ_NO, VOLUME, VOLUME_UOM,
                VOLUME_START_DATE, VOLUME_END_DATE, ACTIVE_IND) VALUES
                ('WELL', 'PDEN-F1-A', 'PPDM', 'METERED', 'FLARE', 'GAS', 'ANNUAL', 1, 0, 1000000, 'm3',
                 '2026-01-01', '2026-12-31', 'Y'),
                ('WELL', 'PDEN-F1-B', 'PPDM', 'METERED', 'VENT', 'GAS', 'ANNUAL', 1, 0, 5000, 'm3',
                 '2026-01-01', '2026-12-31', 'Y'),
                ('WELL', 'PDEN-F2', 'PPDM', 'METERED', 'FLARE', 'GAS', 'ANNUAL', 1, 0, 999999, 'm3',
                 '2026-01-01', '2026-12-31', 'Y'),
                ('WELL', 'PDEN-OLD', 'PPDM', 'METERED', 'FLARE', 'GAS', 'ANNUAL', 1, 0, 777, 'm3',
                 '2025-01-01', '2025-12-31', 'Y');
            """);
    }

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

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
