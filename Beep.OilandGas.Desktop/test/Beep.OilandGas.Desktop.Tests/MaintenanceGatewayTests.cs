using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WRK-03 against a real SQLite database: a plan's schedule records its frequency and unit and its next due date is computed
/// from the last done date (AC-1, AC-5); the compliance state follows (AC-4); a job is generated only from a due plan and
/// records the plan and the generation date (AC-3); an inspection with findings cannot close without a linked corrective
/// action (AC-2); and every write records its actor and date (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class MaintenanceGatewayTests
{
    private const string ConnectionName = "wrk03-maintenance";

    // A generated job is stamped with the day it was generated, so the fixture's as-of date is the clock's: a pinned date
    // fails the next morning and the SQL date filter would disagree with it.
    private static readonly DateTime AsOf = DateTime.UtcNow.Date;

    [IntegrationFact]
    public async Task Plans_generate_their_jobs_and_inspections_close_only_with_an_action()
    {        var root = Path.Combine(OgPaths.DataRoot, "wrk03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWrk03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "maintenance.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "maintenance.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgMaintenanceService(() => source, reporter);

        // ---- the read: the plan's schedule and the derived state ----
        var loaded = await service.LoadAsync("UWI-1", AsOf);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;
        var plan = Assert.Single(workspace.Plans);

        Assert.Equal(30, plan.Frequency);
        Assert.Equal("DAY", plan.Unit);
        Assert.Equal(new DateTime(2026, 10, 31), plan.NextDue);
        Assert.Equal(WorkMaintenanceRules.Overdue, plan.Compliance);
        Assert.Equal(1, workspace.OverdueCount);
        Assert.Contains("day(s) overdue", plan.Describe(), StringComparison.Ordinal);
        Assert.Single(workspace.Inspections);

        // ---- AC-5: a frequency without its unit is refused and nothing is written ----
        var noUnit = await service.EditPlanAsync("PLAN-1", 7, null, AsOf, "tasks", "UWI-1", "sara", AsOf);

        Assert.False(noUnit.Success);
        Assert.Equal(WorkMaintenanceRules.CodeFrequencyUnitRequired, noUnit.Reference);
        Assert.Equal(1, Count(database, "WELL_MAINTENANCE_PLAN WHERE FREQUENCY = 30"));

        // ---- AC-3: a due plan generates a job that names its plan and the date ----
        var generated = await service.GenerateJobAsync("PLAN-1", "UWI-1", "sara", AsOf);

        Assert.True(generated.Success, generated.Message + " :: " + string.Join(" | ", reporter.Failures));

        var job = Assert.Single(generated.Value!.Jobs);

        Assert.Equal("PLAN-1", job.PlanId);
        Assert.Equal(AsOf, job.GeneratedOn);
        Assert.Contains("from plan PLAN-1", job.Describe(), StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "WELL_SERVICE_JOB WHERE MAINT_PLAN_ID = 'PLAN-1' AND GENERATED_ON LIKE '" +
            AsOf.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) +
            "%' AND ROW_CREATED_BY = 'sara'"));

        // ---- and a plan that is not due generates nothing ----
        var rescheduled = await service.EditPlanAsync("PLAN-1", 7, "DAY", new DateTime(2026, 11, 5), "tasks", "UWI-1",
            "sara", AsOf);

        Assert.True(rescheduled.Success, rescheduled.Message);
        Assert.Equal(WorkMaintenanceRules.Current, Assert.Single(rescheduled.Value!.Plans).Compliance);

        var notDue = await service.GenerateJobAsync("PLAN-1", "UWI-1", "sara", AsOf);

        Assert.False(notDue.Success);
        Assert.Equal(WorkMaintenanceRules.CodeNotDue, notDue.Reference);
        Assert.Contains("no job is generated", notDue.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "WELL_SERVICE_JOB"));

        // ---- AC-2: an inspection with findings cannot close without a linked action ----
        var refused = await service.RecordInspectionAsync("I-1", "FAIL", "Seal leaking at the packing", "UWI-1", "omar", AsOf);

        Assert.False(refused.Success);
        Assert.Equal(WorkMaintenanceRules.CodeFindingsNeedAction, refused.Reference);
        Assert.Contains("corrective action", refused.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "WORK_ORDER_INSPECTION WHERE RESULT_STATUS = 'FAIL'"));

        // A clean result is recorded with its actor and date.
        var passed = await service.RecordInspectionAsync("I-1", "PASS", "  ", "UWI-1", "omar", AsOf);

        Assert.True(passed.Success, passed.Message);
        Assert.Equal(1, Count(database,
            "WORK_ORDER_INSPECTION WHERE INSPECTION_ID = 'I-1' AND RESULT_STATUS = 'PASS' AND INSPECTOR = 'omar' AND INSPECTED_AT_UTC IS NOT NULL"));

        // With a corrective action linked to the work order, the findings may be recorded and closed.
        Execute(database, "UPDATE WORK_ORDER_COMPONENT SET PROJECT_ID = 'PC-1' WHERE WORK_ORDER_ID = 'WO-1';");

        var withAction = await service.RecordInspectionAsync("I-1", "FAIL", "Seal leaking at the packing", "UWI-1",
            "nadia", AsOf);

        Assert.True(withAction.Success, withAction.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Contains("action linked", Assert.Single(withAction.Value!.Inspections).StateText, StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "WORK_ORDER_INSPECTION WHERE INSPECTION_ID = 'I-1' AND RESULT_STATUS = 'FAIL' AND INSPECTOR = 'nadia'"));

        // ---- the failure path: no connection is not an empty plan list ----
        var offline = new OgMaintenanceService(() => null, reporter);
        var noConnection = await offline.LoadAsync("UWI-1", AsOf);

        Assert.False(noConnection.Success);
        Assert.Null(noConnection.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE WELL_MAINTENANCE_PLAN (
                FREQUENCY INTEGER,
                FREQUENCY_UNIT TEXT,
                LAST_DONE_DATE TEXT,
                TASK_LIST TEXT,
                SCHEDULE_EDITED_BY TEXT,
                SCHEDULE_EDITED_DATE TEXT,
                MAINT_PLAN_ID TEXT,
                FDP_ID TEXT,
                UWI TEXT,
                MAINTENANCE_TYPE TEXT,
                MAINTENANCE_STATUS TEXT,
                PRIORITY TEXT,
                TRIGGER_BASIS TEXT,
                PLANNED_START_DATE TEXT,
                PLANNED_END_DATE TEXT,
                SERVICE_BA_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (MAINT_PLAN_ID));

            CREATE TABLE WELL_SERVICE_JOB (
                MAINT_PLAN_ID TEXT,
                GENERATED_ON TEXT,
                JOB_ID TEXT,
                FDP_ID TEXT,
                UWI TEXT,
                JOB_TYPE TEXT,
                JOB_STATUS TEXT,
                PRIORITY TEXT,
                SERVICE_BA_ID TEXT,
                BA_SERVICE_TYPE TEXT,
                PLANNED_START_DATE TEXT,
                PLANNED_END_DATE TEXT,
                LINKED_ACTIVITY_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (JOB_ID));

            CREATE TABLE WORK_ORDER_INSPECTION (
                APPLICABLE_STAGE TEXT,
                DOCUMENT_ACCEPTANCE_DATA_JSON TEXT,
                DOCUMENT_ACCEPTANCE_REVISION TEXT,
                PERSONNEL_ACCEPTANCE_DATA_JSON TEXT,
                PERSONNEL_ACCEPTANCE_REVISION TEXT,
                INSPECTION_ID TEXT,
                WORK_ORDER_ID TEXT,
                FIELD_ID TEXT,
                COND_SEQ INTEGER,
                COND_TYPE TEXT,
                REQUIRED_TEXT TEXT,
                JURISDICTION TEXT,
                REGULATORY_REF TEXT,
                PERMIT_PREREQUISITE_KIND TEXT,
                REQUIREMENT_ACTOR TEXT,
                RESULT_STATUS TEXT,
                RESULT_NOTES TEXT,
                INSPECTOR TEXT,
                INSPECTED_AT_UTC TEXT,
                MATERIAL_ACCEPTANCE_DATA_JSON TEXT,
                MATERIAL_ACCEPTANCE_REVISION TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (INSPECTION_ID));

            CREATE TABLE WORK_ORDER (
                WORK_ORDER_ID TEXT,
                ACTIVE_IND TEXT,
                COMPLETE_DATE TEXT,
                DUE_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FINAL_BILLING_DATE TEXT,
                INSTRUCTIONS TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                REQUEST_DATE TEXT,
                RUSH_IND TEXT,
                SOURCE TEXT,
                WORK_ORDER_NUMBER TEXT,
                WORK_ORDER_TYPE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (WORK_ORDER_ID));

            CREATE TABLE WORK_ORDER_COMPONENT (
                WORK_ORDER_ID TEXT,
                COMPONENT_ID TEXT,
                ACTIVE_IND TEXT,
                ACTIVITY_OBS_NO REAL,
                ANALYSIS_ID TEXT,
                APPLICATION_ID TEXT,
                AREA_ID TEXT,
                AREA_TYPE TEXT,
                AUTHORITY_ID TEXT,
                BA_ORGANIZATION_ID TEXT,
                BA_ORGANIZATION_SEQ_NO REAL,
                BUSINESS_ASSOCIATE_ID TEXT,
                CATALOGUE_ADDITIVE_ID TEXT,
                CATALOGUE_EQUIP_ID TEXT,
                CIRC_ID TEXT,
                CLASSIFICATION_SYSTEM_ID TEXT,
                CLASS_LEVEL_ID TEXT,
                COMPONENT_TYPE TEXT,
                CONSENT_ID TEXT,
                CONSULT_ID TEXT,
                CONTEST_ID TEXT,
                CONTRACT_ID TEXT,
                ECOZONE_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EMPLOYEE_BA_ID TEXT,
                EMPLOYEE_OBS_NO REAL,
                EMPLOYER_BA_ID TEXT,
                ENTITLEMENT_ID TEXT,
                EQUIPMENT_ID TEXT,
                EQUIP_MAINT_ID TEXT,
                EXPIRY_DATE TEXT,
                FACILITY_ID TEXT,
                FACILITY_MAINTAIN_ID TEXT,
                FACILITY_TYPE TEXT,
                FIELD_ID TEXT,
                FIELD_STATION_ID TEXT,
                FINANCE_ID TEXT,
                FOSSIL_ID TEXT,
                INCIDENT_ID TEXT,
                INCIDENT_SET_ID TEXT,
                INCIDENT_TYPE TEXT,
                INFORMATION_ITEM_ID TEXT,
                INFO_ITEM_SUBTYPE TEXT,
                INSPECTION_ID TEXT,
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
                NOTIFICATION_ID TEXT,
                OBLIGATION_ID TEXT,
                OBLIGATION_SEQ_NO REAL,
                ORIGIN_SEQ_NO REAL,
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
                PROJECT_ID TEXT,
                RATE_SCHEDULE_ID TEXT,
                REFERENCED_GUID TEXT,
                REFERENCED_SYSTEM_ID TEXT,
                REFERENCED_TABLE_NAME TEXT,
                REMARK TEXT,
                RESENT_ID TEXT,
                RESERVE_CLASS_ID TEXT,
                SAMPLE_ANAL_SOURCE TEXT,
                SEIS_SET_ID TEXT,
                SEIS_SET_SUBTYPE TEXT,
                SEIS_TRANSACTION_ID TEXT,
                SF_SUBTYPE TEXT,
                SOURCE TEXT,
                SPATIAL_DESCRIPTION_ID TEXT,
                SPATIAL_OBS_NO REAL,
                STORE_ID TEXT,
                STORE_STRUCTURE_OBS_NO REAL,
                STRAT_NAME_SET_ID TEXT,
                STRAT_UNIT_ID TEXT,
                SUPPORT_FACILITY_ID TEXT,
                SW_APPLICATION_ID TEXT,
                THESAURUS_ID TEXT,
                THESAURUS_WORD_ID TEXT,
                TRANSACTION_TYPE TEXT,
                UWI TEXT,
                WELL_ACTIVITY_SET_ID TEXT,
                WELL_ACTIVITY_SOURCE TEXT,
                WELL_ACTIVITY_TYPE_ID TEXT,
                WELL_SET_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (WORK_ORDER_ID, COMPONENT_ID));

            CREATE TABLE PROJECT (
                PROJECT_ID TEXT,
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
                ROW_QUALITY TEXT,
                PRIMARY KEY (PROJECT_ID));

            INSERT INTO WELL_MAINTENANCE_PLAN (MAINT_PLAN_ID, UWI, MAINTENANCE_TYPE, MAINTENANCE_STATUS, PRIORITY,
                FREQUENCY, FREQUENCY_UNIT, LAST_DONE_DATE, TASK_LIST, SCHEDULE_EDITED_BY, SCHEDULE_EDITED_DATE,
                ACTIVE_IND) VALUES
                ('PLAN-1', 'UWI-1', 'WORKOVER', 'PLANNED', 'HIGH', 30, 'DAY', '2026-10-01', 'Inspect and replace the seal',
                 'sara', '2026-10-01', 'Y');

            INSERT INTO WORK_ORDER (WORK_ORDER_ID, WORK_ORDER_NUMBER, WORK_ORDER_TYPE, DUE_DATE, ACTIVE_IND) VALUES
                ('WO-1', 'WO-0001', 'WO-CORRECTIVE', '2026-11-10', 'Y');

            INSERT INTO WORK_ORDER_COMPONENT (WORK_ORDER_ID, COMPONENT_ID, FIELD_ID, UWI, COMPONENT_TYPE, ACTIVE_IND) VALUES
                ('WO-1', 'C-1', 'UWI-1', 'UWI-1', 'WELL', 'Y');

            INSERT INTO WORK_ORDER_INSPECTION (INSPECTION_ID, WORK_ORDER_ID, FIELD_ID, COND_SEQ, COND_TYPE, REQUIRED_TEXT,
                JURISDICTION, REQUIREMENT_ACTOR, RESULT_STATUS, RESULT_NOTES, ACTIVE_IND) VALUES
                ('I-1', 'WO-1', 'UWI-1', 1, 'CLOSE_OUT', 'Close-out inspection', 'TX', 'sara', 'PENDING', '', 'Y');

            INSERT INTO PROJECT (PROJECT_ID, PROJECT_TYPE, PROJECT_NAME, ACTIVE_IND) VALUES
                ('PC-1', 'CORRECTIVE_ACTION', 'Seal replacement', 'Y');
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
