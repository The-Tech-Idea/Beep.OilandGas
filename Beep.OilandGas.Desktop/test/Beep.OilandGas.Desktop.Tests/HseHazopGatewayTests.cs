using Beep.OilandGas.Models.Constants;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-04 against a real SQLite database: the study is opened by its recorded id and never falls back (AC-1), a duplicate
/// node key is refused (AC-3), a deviation keeps the owner's two-field payload while its extra fields live in a linked
/// component, an action needs its node and its deviation (AC-4, SG-11-5), only the legal transitions are applied and a
/// mismatched acknowledgement is unconfirmed (AC-2, AC-6), and a field change during the read clears the study (AC-5).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HseHazopGatewayTests
{
    private const string ConnectionName = "hse04-hazop";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    [IntegrationFact]
    public async Task A_study_is_opened_by_identity_and_its_rows_follow_the_owners_shape()
    {        var root = Path.Combine(OgPaths.DataRoot, "hse04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasHse04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "hazop.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "hazop.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var hazop = new OgHazop(() => source, reporter);
        var stable = () => Scope;

        // ---- AC-1: the recorded id, never "the latest" ----
        var loaded = await hazop.LoadAsync(hazop.BeginLoad(), "S-1", Scope, stable);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal("S-1", loaded.Value!.Study.Id);
        Assert.Equal("Separator HAZOP", loaded.Value.Study.Name);
        Assert.Equal("F-1", loaded.Value.Study.FieldId);
        Assert.Equal(HseReferenceCodes.Hazop.Draft, loaded.Value.Study.State);
        Assert.Single(loaded.Value.Study.Nodes);
        Assert.Equal(HazopRules.NextStates(HseReferenceCodes.Hazop.Draft), loaded.Value.LegalNextStates);

        var missing = await hazop.LoadAsync(hazop.BeginLoad(), "S-NOT-THERE", Scope, stable);

        Assert.False(missing.Success);
        Assert.Null(missing.Value);
        Assert.Equal(HazopRules.CodeStudyNotFound, missing.Reference);
        Assert.Contains("No other study is opened", missing.Message!, StringComparison.Ordinal);

        // ---- create a study, with its field context and DRAFT state ----
        var created = await hazop.CreateStudyAsync("S-9", "Compression HAZOP", "Compression train", "F-1", "sara");

        Assert.True(created.Success, created.Message);
        Assert.Equal(HseReferenceCodes.Hazop.Draft, created.Value!.Study.State);
        Assert.Equal("F-1", created.Value.Study.FieldId);
        Assert.Equal(1, Count(database, "PROJECT_STATUS WHERE PROJECT_ID = 'S-9' AND STATUS_TYPE = 'HAZOP_STUDY' AND STATUS = 'DRAFT'"));

        // ---- nodes ----
        var node1 = await hazop.AddNodeAsync("S-9", "Separator inlet", "Take gas from the HP separator", "sara");

        Assert.True(node1.Success, node1.Message);
        Assert.Equal("S-9-NODE-001", node1.Value!.Study.Nodes[0].StepId);

        var node2 = await hazop.AddNodeAsync("S-9", "Compressor suction", "Feed the compressor", "sara");

        Assert.True(node2.Success, node2.Message);
        Assert.Equal(2, node2.Value!.Study.Nodes.Count);

        // ---- AC-3: a duplicate node key is refused ----
        // A duplicate node key is a second node claiming the same sequence: the identity is unique, the key is not.
        Execute(database,
            "INSERT INTO PROJECT_STEP (PROJECT_ID, STEP_ID, STEP_TYPE, STEP_NAME, STEP_SEQ_NO, ACTIVE_IND) VALUES ('S-9', 'S-9-NODE-002-DUP', 'HAZOP_NODE', 'Duplicate key', 2, 'Y');");

        var duplicate = await hazop.LoadAsync(hazop.BeginLoad(), "S-9", Scope, stable);

        Assert.False(duplicate.Success);
        Assert.Equal(HazopRules.CodeDuplicateNodeKey, duplicate.Reference);
        Assert.Contains("duplicate node key", duplicate.Message!, StringComparison.Ordinal);

        Execute(database,
            "DELETE FROM PROJECT_STEP WHERE PROJECT_ID = 'S-9' AND STEP_ID = 'S-9-NODE-002-DUP';");

        // ---- a deviation keeps the owner's two-field payload ----
        var deviation = await hazop.AddDeviationAsync("S-9", "S-9-NODE-001", "NO", "flow", "Blocked strainer",
            "No flow to the separator", "Strainer and low-flow alarm", "HIGH", "sara");

        Assert.True(deviation.Success, deviation.Message + " :: " + string.Join(" | ", reporter.Failures));

        var recorded = deviation.Value!.Study.Nodes
            .Single(node => node.StepId == "S-9-NODE-001").Deviations.Single();

        Assert.Equal("NO", recorded.GuideWord);
        Assert.Equal("flow", recorded.Parameter);
        Assert.Equal("No flow to the separator", recorded.Consequence);
        Assert.Equal("Blocked strainer", recorded.Cause);
        Assert.Equal("HIGH", recorded.Risk);
        Assert.Equal(HseReferenceCodes.Hazop.Open, recorded.State);
        Assert.Null(recorded.Action);

        // The owner's reader requires exactly Deviation and Consequence in REMARK.
        var payload = Text(database,
            "SELECT REMARK FROM PROJECT_CONDITION WHERE PROJECT_ID = 'S-9' AND CONDITION_OBS_NO = 1");

        using var document = System.Text.Json.JsonDocument.Parse(payload!);
        var properties = document.RootElement.EnumerateObject().Select(value => value.Name).ToList();

        Assert.Equal(2, properties.Count);
        Assert.Contains("Deviation", properties);
        Assert.Contains("Consequence", properties);

        // The desktop's extra fields are in the linked component, not the payload.
        Assert.Equal(1, Count(database,
            "PROJECT_COMPONENT WHERE PROJECT_ID = 'S-9' AND COMPONENT_TYPE = 'HAZOP_DEVIATION_DETAIL' AND CONDITION_ID = 'S-9-NODE-001-DEV-001' AND DESCRIPTION LIKE 'parameter=flow%'"));
        Assert.Equal(1, Count(database,
            "PROJECT_STATUS WHERE PROJECT_ID = 'S-9' AND STATUS_TYPE = 'HAZOP_DEVIATION' AND STATUS_ID = 'S-9-NODE-001-DEV-001' AND STATUS = 'OPEN'"));

        // ---- AC-4 and SG-11-5: an action needs its node and its deviation ----
        var noNode = await hazop.AddActionAsync("S-9", "  ", 1, "Fit a guard", "omar", new DateTime(2026, 12, 1), "sara");

        Assert.False(noNode.Success);
        Assert.Equal(HazopRules.CodeActionNeedsNodeAndDeviation, noNode.Reference);

        var noDeviation = await hazop.AddActionAsync("S-9", "S-9-NODE-001", 99, "Fit a guard", "omar",
            new DateTime(2026, 12, 1), "sara");

        Assert.False(noDeviation.Success);
        Assert.Equal(HazopRules.CodeUnknownDeviation, noDeviation.Reference);

        var action = await hazop.AddActionAsync("S-9", "S-9-NODE-001", 1, "Fit a guard", "omar",
            new DateTime(2026, 12, 1), "sara");

        Assert.True(action.Success, action.Message);

        var withAction = action.Value!.Study.Nodes
            .Single(node => node.StepId == "S-9-NODE-001").Deviations.Single().Action!;

        Assert.Equal("Fit a guard", withAction.Action);
        Assert.Equal("omar", withAction.Owner);
        Assert.Equal(new DateTime(2026, 12, 1), withAction.DueDate);
        Assert.Equal(1, Count(database, "PROJECT_STEP WHERE PROJECT_ID = 'S-9' AND STEP_TYPE = 'HAZOP_ACTION' AND DUE_DATE LIKE '2026-12-01%'"));
        Assert.Equal(1, Count(database,
            "PROJECT_STEP_XREF WHERE PROJECT_ID = 'S-9' AND STEP_ID1 = 'S-9-NODE-001' AND STEP_XREF_TYPE = 'HAZOP_ACTION_NODE'"));
        Assert.Equal(1, Count(database,
            "PROJECT_STEP_BA WHERE PROJECT_ID = 'S-9' AND ROLE = 'RESPONSIBLE' AND BUSINESS_ASSOCIATE_ID = 'omar'"));

        // ---- AC-2 and AC-6: unconfirmed before illegal, and only legal transitions ----
        var unconfirmed = await hazop.TransitionStudyAsync("S-9", HseReferenceCodes.Hazop.Open,
            HseReferenceCodes.Hazop.Closed, "sara");

        Assert.False(unconfirmed.Success);
        Assert.Equal(HazopRules.CodeUnconfirmed, unconfirmed.Reference);
        Assert.Contains("not applied", unconfirmed.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "PROJECT_STATUS WHERE PROJECT_ID = 'S-9' AND STATUS_ID = 'S-9-STUDY' AND STATUS = 'DRAFT'"));

        var opened = await hazop.TransitionStudyAsync("S-9", HseReferenceCodes.Hazop.Open,
            HseReferenceCodes.Hazop.Draft, "sara");

        Assert.True(opened.Success, opened.Message);
        Assert.Equal(HseReferenceCodes.Hazop.Open, opened.Value!.Study.State);
        Assert.Equal([HseReferenceCodes.Hazop.Closed], opened.Value.LegalNextStates);

        var backwards = await hazop.TransitionStudyAsync("S-9", HseReferenceCodes.Hazop.Draft,
            HseReferenceCodes.Hazop.Open, "sara");

        Assert.False(backwards.Success);
        Assert.Equal(HazopRules.CodeTransitionIllegal, backwards.Reference);

        var deviationMoved = await hazop.TransitionDeviationAsync("S-9", "S-9-NODE-001", 1,
            HseReferenceCodes.Hazop.ActionRequired, HseReferenceCodes.Hazop.Open, "sara");

        Assert.True(deviationMoved.Success, deviationMoved.Message);
        Assert.Equal(1, Count(database,
            "PROJECT_STATUS WHERE PROJECT_ID = 'S-9' AND STATUS_ID = 'S-9-NODE-001-DEV-001' AND STATUS = 'ACTION_REQUIRED'"));

        // One status row per deviation, updated rather than duplicated - the owner's own shape.
        Assert.Equal(1, Count(database,
            "PROJECT_STATUS WHERE PROJECT_ID = 'S-9' AND STATUS_ID = 'S-9-NODE-001-DEV-001'"));

        Assert.True((await hazop.TransitionDeviationAsync("S-9", "S-9-NODE-001", 1,
            HseReferenceCodes.Hazop.Closed, HseReferenceCodes.Hazop.ActionRequired, "sara")).Success);

        var closedDeviation = await hazop.TransitionDeviationAsync("S-9", "S-9-NODE-001", 1,
            HseReferenceCodes.Hazop.Open, HseReferenceCodes.Hazop.Closed, "sara");

        Assert.False(closedDeviation.Success);
        Assert.Equal(HazopRules.CodeTransitionIllegal, closedDeviation.Reference);

        // ---- AC-5: a field change during the read clears the study ----
        var calls = 0;

        OgHseScope Changing()
        {
            calls++;
            return calls == 1 ? new OgHseScope("SMOKE", "F-1") : new OgHseScope("SMOKE", "F-2");
        }

        var changed = await hazop.LoadAsync(hazop.BeginLoad(), "S-9", Scope, Changing);

        Assert.False(changed.Success);
        Assert.Null(changed.Value);
        Assert.Equal(HazopRules.CodeScopeChanged, changed.Reference);
        Assert.Contains("discarded", changed.Message!, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database,
            """
            CREATE TABLE PROJECT (
                PROJECT_ID TEXT PRIMARY KEY,
                ACTIVE_IND TEXT,
                COMPLETE_DATE TEXT,
                CONFIDENTIAL_IND TEXT,
                CONFIDENTIAL_RELEASE_DATE TEXT,
                DESCRIPTION TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FIELD_STATION_IND TEXT,
                LAND_RIGHT_IND TEXT,
                PDEN_IND TEXT,
                PPDM_GUID TEXT,
                PROJECT_NAME TEXT,
                PROJECT_NUM TEXT,
                PROJECT_TYPE TEXT,
                REMARK TEXT,
                SEIS_SET_IND TEXT,
                SOURCE TEXT,
                START_DATE TEXT,
                STRAT_COLUMN_IND TEXT,
                STRAT_INTERPRETATION_IND TEXT,
                WELL_IND TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE PROJECT_COMPONENT (
                PROJECT_ID TEXT,
                COMPONENT_OBS_NO REAL,
                ACTIVE_IND TEXT,
                ACTIVITY_OBS_NO REAL,
                ANALYSIS_ID TEXT,
                APPLICATION_ID TEXT,
                AREA_ID TEXT,
                AREA_TYPE TEXT,
                AUTHORITY_ID TEXT,
                BA_LICENSEE_BA_ID TEXT,
                BA_LICENSE_ID TEXT,
                BA_ORGANIZATION_ID TEXT,
                BA_ORGANIZATION_SEQ_NO REAL,
                BUSINESS_ASSOCIATE_ID TEXT,
                CATALOGUE_ADDITIVE_ID TEXT,
                CATALOGUE_EQUIP_ID TEXT,
                CLASSIFICATION_SYSTEM_ID TEXT,
                CLASS_LEVEL_ID TEXT,
                COMPONENT_REASON TEXT,
                COMPONENT_TYPE TEXT,
                CONDITION_ID TEXT,
                CONSENT_ID TEXT,
                CONSULT_ID TEXT,
                CONTEST_ID TEXT,
                CONTRACT_ID TEXT,
                DESCRIPTION TEXT,
                ECOZONE_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EMPLOYEE_BA_ID TEXT,
                EMPLOYEE_OBS_NO REAL,
                EMPLOYER_BA_ID TEXT,
                ENTITLEMENT_ID TEXT,
                EQUIPMENT_ID TEXT,
                EXPIRY_DATE TEXT,
                FACILITY_ID TEXT,
                FACILITY_LICENSE_ID TEXT,
                FACILITY_TYPE TEXT,
                FIELD_ID TEXT,
                FIELD_SOURCE TEXT,
                FIELD_STATION_ID TEXT,
                FIELD_STRAT_INTERP_ID TEXT,
                FIELD_STRAT_UNIT_ID TEXT,
                FINANCE_ID TEXT,
                FOSSIL_ID TEXT,
                HSE_INCIDENT_ID TEXT,
                INCIDENT_SET_ID TEXT,
                INCIDENT_TYPE TEXT,
                INFORMATION_ITEM_ID TEXT,
                INFO_ITEM_SUBTYPE TEXT,
                INPUT_IND TEXT,
                INSTRUMENT_ID TEXT,
                INTEREST_SET_ID TEXT,
                INTEREST_SET_SEQ_NO REAL,
                JURISDICTION TEXT,
                LAND_RIGHT_ID TEXT,
                LAND_RIGHT_SUBTYPE TEXT,
                LAND_SALE_NUMBER TEXT,
                LANGUAGE TEXT,
                LITHOLOGY_LOG_ID TEXT,
                LITH_LOG_SOURCE TEXT,
                LITH_SAMPLE_ID TEXT,
                NOTIFICATION_ID TEXT,
                OBLIGATION_ID TEXT,
                OBLIGATION_SEQ_NO REAL,
                OTHER_PROJECT_ID TEXT,
                OUTPUT_IND TEXT,
                PALEO_SUMMARY_ID TEXT,
                PDEN_ID TEXT,
                PDEN_SOURCE TEXT,
                PDEN_SUBTYPE TEXT,
                PHYSICAL_ITEM_ID TEXT,
                POOL_ID TEXT,
                PPDM_GUID TEXT,
                PPDM_SYSTEM_ID TEXT,
                PPDM_TABLE_NAME TEXT,
                PRODUCT_TYPE TEXT,
                PROD_STRING_ID TEXT,
                PROD_STRING_SOURCE TEXT,
                PROJECT_PLAN_ID TEXT,
                PROJECT_STEP_ID TEXT,
                PROVISION_ID TEXT,
                RATE_SCHEDULE_ID TEXT,
                REFERENCED_GUID TEXT,
                REFERENCED_SYSTEM_ID TEXT,
                REFERENCED_TABLE_NAME TEXT,
                REMARK TEXT,
                RESENT_ID TEXT,
                RESERVE_CLASS_ID TEXT,
                RESERVE_PRODUCT_TYPE TEXT,
                RESERVE_REVISION_ID TEXT,
                RESERVE_REVISION_OBS_NO REAL,
                RESERVE_SUMMARY_ID TEXT,
                RESERVE_SUMMARY_OBS_NO REAL,
                SAMPLE_ANAL_SOURCE TEXT,
                SEIS_LICENSE_ID TEXT,
                SEIS_PROC_PLAN_ID TEXT,
                SEIS_SET_ID TEXT,
                SEIS_SET_SUBTYPE TEXT,
                SF_SUBTYPE TEXT,
                SOURCE TEXT,
                SPATIAL_DESCRIPTION_ID TEXT,
                SPATIAL_OBS_NO REAL,
                STORE_ID TEXT,
                STORE_STRUCTURE_OBS_NO REAL,
                STRAT_COLUMN_ID TEXT,
                STRAT_COLUMN_SOURCE TEXT,
                STRAT_NAME_SET_ID TEXT,
                STRAT_UNIT_ID TEXT,
                SUPPORT_FACILITY_ID TEXT,
                SW_APPLICATION_ID TEXT,
                THESAURUS_ID TEXT,
                THESAURUS_WORD_ID TEXT,
                UWI TEXT,
                WELL_ACTIVITY_SET_ID TEXT,
                WELL_ACTIVITY_SOURCE TEXT,
                WELL_ACTIVITY_TYPE_ID TEXT,
                WELL_INT_INTERP_ID TEXT,
                WELL_INT_SOURCE TEXT,
                WELL_INT_STRAT_UNIT_ID TEXT,
                WELL_INT_UWI TEXT,
                WELL_LICENSE_ID TEXT,
                WELL_LICENSE_SOURCE TEXT,
                WELL_SET_ID TEXT,
                WORK_ORDER_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (PROJECT_ID, COMPONENT_OBS_NO));

            CREATE TABLE PROJECT_STEP (
                PROJECT_ID TEXT,
                STEP_ID TEXT,
                ACTIVE_IND TEXT,
                ACTUAL_END_DATE TEXT,
                ACTUAL_START_DATE TEXT,
                ACTUAL_TIME_ELAPSED REAL,
                ACTUAL_TIME_ELAPSED_OUOM TEXT,
                ACTUAL_TIME_ELAPSED_UOM TEXT,
                CRITICAL_DATE TEXT,
                DESCRIPTION TEXT,
                DUE_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                ESTIMATED_TIME_ELAPSED REAL,
                ESTIMATED_TIME_ELAPSED_OUOM TEXT,
                ESTIMATED_TIME_ELAPSED_UOM TEXT,
                EXPIRY_DATE TEXT,
                PLAN_END_DATE TEXT,
                PLAN_START_DATE TEXT,
                PLAN_STEP_ID TEXT,
                PPDM_GUID TEXT,
                PROJECT_PLAN_ID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                STEP_NAME TEXT,
                STEP_SEQ_NO REAL,
                STEP_TYPE TEXT,
                WHERE_COMPLETED TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (PROJECT_ID, STEP_ID));

            CREATE TABLE PROJECT_STEP_XREF (
                PROJECT_ID TEXT,
                STEP_ID1 TEXT,
                STEP_ID2 TEXT,
                XREF_OBS_NO REAL,
                ACTIVE_IND TEXT,
                DESCRIPTION TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                STEP_SEQ_NO REAL,
                STEP_XREF_TYPE TEXT,
                USE_RULE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (PROJECT_ID, STEP_ID1, STEP_ID2, XREF_OBS_NO));

            CREATE TABLE PROJECT_CONDITION (
                PROJECT_ID TEXT,
                CONDITION_OBS_NO REAL,
                ACTIVE_IND TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                CONDITION_TYPE TEXT,
                EFFECTIVE_DATE TEXT,
                END_DATE TEXT,
                EXPIRY_DATE TEXT,
                PPDM_GUID TEXT,
                PROJECT_TYPE TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                START_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (PROJECT_ID, CONDITION_OBS_NO));

            CREATE TABLE PROJECT_STATUS (
                PROJECT_ID TEXT,
                STATUS_ID TEXT,
                ACTIVE_IND TEXT,
                DEFINED_BY_BA_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                STATUS TEXT,
                STATUS_TYPE TEXT,
                STEP_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (PROJECT_ID, STATUS_ID));

            CREATE TABLE PROJECT_STEP_BA (
                PROJECT_ID TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                ROLE TEXT,
                ROLE_SEQ_NO REAL,
                STEP_ID TEXT,
                STEP_BA_OBS_NO REAL,
                ACTIVE_IND TEXT,
                ACTUAL_IND TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PLAN_IND TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (PROJECT_ID, STEP_ID, BUSINESS_ASSOCIATE_ID, ROLE, ROLE_SEQ_NO, STEP_BA_OBS_NO));

            INSERT INTO PROJECT (PROJECT_ID, PROJECT_TYPE, PROJECT_NAME, DESCRIPTION, START_DATE, ACTIVE_IND, SOURCE) VALUES
                ('S-1', 'HAZOP', 'Separator HAZOP', 'HP separator train', '2026-10-01', 'Y', 'seed'),
                ('S-2', 'HAZOP', 'Newer HAZOP', 'A later study that must not be chosen', '2026-11-01', 'Y', 'seed');

            INSERT INTO PROJECT_COMPONENT (PROJECT_ID, COMPONENT_OBS_NO, FIELD_ID, FIELD_SOURCE, ACTIVE_IND) VALUES
                ('S-1', 1, 'F-1', 'Beep.OilandGas.HSE.Field', 'Y'),
                ('S-2', 1, 'F-1', 'Beep.OilandGas.HSE.Field', 'Y');

            INSERT INTO PROJECT_STEP (PROJECT_ID, STEP_ID, STEP_TYPE, STEP_NAME, STEP_SEQ_NO, ACTIVE_IND) VALUES
                ('S-1', 'S-1-NODE-001', 'HAZOP_NODE', 'Separator inlet', 1, 'Y');

            INSERT INTO PROJECT_STATUS (PROJECT_ID, STATUS_ID, STATUS_TYPE, STATUS, EFFECTIVE_DATE, ACTIVE_IND) VALUES
                ('S-1', 'S-1-STUDY', 'HAZOP_STUDY', 'DRAFT', '2026-10-01', 'Y');
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

    private static string? Text(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return command.ExecuteScalar() as string;
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
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
