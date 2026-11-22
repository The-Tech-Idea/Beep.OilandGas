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
/// WRK-02 against a real SQLite database: only the owner's graph offers a transition and only it can be applied (AC-1); the
/// actual cost is the sum of the labour and material lines with the uncosted ones counted (AC-2); execution start is refused
/// without a valid permit (AC-3); completion is refused without recorded evidence (AC-4); the submitter cannot complete their own
/// order (AC-5); a line records its actor and date (AC-6); and a reservation above availability is refused with the short
/// quantity (SG-13-3). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WorkOrderDetailGatewayTests
{
    private const string ConnectionName = "wrk02-detail";

    private static readonly DateTime AsOf = DateTime.UtcNow;

    [IntegrationFact]
    public async Task The_order_costs_its_lines_and_refuses_unpermitted_or_unevidenced_moves()
    {        var root = Path.Combine(OgPaths.DataRoot, "wrk02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWrk02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "detail.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "detail.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWorkOrderDetailService(() => source, reporter);

        // ---- the read: state, lines, cost and gates ----
        var loaded = await service.LoadAsync("WO-1", AsOf);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var detail = loaded.Value!;

        Assert.Equal("PLANNED", detail.State);
        Assert.Equal("Corrective maintenance", detail.Kind);
        Assert.Equal("WORK-C-1", Assert.Single(detail.Materials).ItemName);
        Assert.Equal(2, detail.Labour.Count);

        // AC-6: every line carries its actor and its date.
        Assert.All(detail.Labour, line => Assert.Equal("omar", line.Actor));
        Assert.All(detail.Labour, line => Assert.NotEqual(default, line.FromUtc));

        var material = detail.Materials[0];

        Assert.Equal(37.50m, material.Cost);
        Assert.Equal("USD", material.Currency);

        // AC-2: the total is the lines' sum, and the uncosted line is counted rather than costed at zero.
        var usd = Assert.Single(detail.Costs);

        Assert.Equal(100m, usd.Labour);
        Assert.Equal(37.50m, usd.Materials);
        Assert.Equal(137.50m, usd.Total);
        Assert.Equal(1, usd.UncostedLines);
        Assert.False(usd.Complete);
        Assert.Contains("partial", detail.CostText, StringComparison.Ordinal);

        // AC-3 and AC-4: the permit covers now and the close-out is recorded.
        Assert.True(detail.Permit.IsValid);
        Assert.True(detail.Evidence.IsRecorded);
        Assert.Contains("IN_PROGRESS", detail.AvailableTransitions);

        // ---- AC-1: a transition the owner's graph does not offer is refused ----
        var skipped = await service.TransitionAsync("WO-1", "COMPLETED", "going straight to complete", "nadia", AsOf);

        Assert.False(skipped.Success);
        Assert.Equal(WorkOrderDetailRules.CodeTransitionUnavailable, skipped.Reference);
        Assert.Contains("may move to", skipped.Message!, StringComparison.Ordinal);

        // ---- AC-3: without a permit, execution cannot start ----
        Execute(database, "DELETE FROM WORK_ORDER_EXECUTION_PERMIT;");

        var noPermit = await service.TransitionAsync("WO-1", "IN_PROGRESS", "starting", "omar", AsOf);

        Assert.False(noPermit.Success);
        Assert.Equal(WorkOrderDetailRules.CodePermitRequired, noPermit.Reference);
        Assert.Contains("No execution permit", noPermit.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "PROJECT_STATUS WHERE PROJECT_ID = 'P-1' AND STATUS = 'IN_PROGRESS'"));

        // With the permit restored the start is recorded on the linked project.
        Execute(database, """
            INSERT INTO WORK_ORDER_EXECUTION_PERMIT (PERMIT_ID, WORK_ORDER_ID, STATUS, VALID_FROM_UTC, VALID_TO_UTC, ACTIVE_IND)
            VALUES ('PT-1', 'WO-1', 'ISSUED', '2026-01-01', '2027-01-01', 'Y');
            """);

        var started = await service.TransitionAsync("WO-1", "IN_PROGRESS", "starting", "omar", AsOf);

        Assert.True(started.Success, started.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal("IN_PROGRESS", started.Value!.State);
        Assert.Equal(1, Count(database,
            "PROJECT_STATUS WHERE PROJECT_ID = 'P-1' AND STATUS_TYPE = 'WO_STATE' AND STATUS = 'IN_PROGRESS' AND ROW_CREATED_BY = 'omar'"));

        // ---- AC-5: the submitter cannot complete their own order ----
        var self = await service.TransitionAsync("WO-1", "COMPLETED", "done", "omar", AsOf);

        Assert.False(self.Success);
        Assert.Equal(WorkOrderDetailRules.CodeSelfApproval, self.Reference);
        Assert.Contains("cannot approve or complete", self.Message!, StringComparison.Ordinal);

        // ---- AC-4: without evidence, nobody can complete it ----
        Execute(database, "DELETE FROM WORK_ORDER_CONDITION;");

        var noEvidence = await service.TransitionAsync("WO-1", "COMPLETED", "done", "nadia", AsOf);

        Assert.False(noEvidence.Success);
        Assert.Equal(WorkOrderDetailRules.CodeEvidenceRequired, noEvidence.Reference);

        Execute(database, """
            INSERT INTO WORK_ORDER_CONDITION (WORK_ORDER_ID, CONDITION_OBS_NO, CONDITION_TYPE, BUSINESS_ASSOCIATE_ID, REMARK,
                EFFECTIVE_DATE, ACTIVE_IND) VALUES ('WO-1', 1, 'CLOSE_OUT', 'nadia', 'Inspection passed.', '2026-11-07', 'Y');
            """);

        var completed = await service.TransitionAsync("WO-1", "COMPLETED", "all work done", "nadia", AsOf);

        Assert.True(completed.Success, completed.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal("COMPLETED", completed.Value!.State);
        Assert.True(completed.Value.IsComplete);

        // ---- SG-13-3: a reservation above availability is refused with the short quantity ----
        var shortReservation = await service.ReserveMaterialAsync("WO-1", "ITEM-1", 7m, "EA", "WH-1", "omar", AsOf);

        Assert.False(shortReservation.Success);
        Assert.Equal(WorkOrderDetailRules.CodeStockShort, shortReservation.Reference);
        Assert.Contains("the short quantity is 1", shortReservation.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "WORK_ORDER_MATERIAL_ALLOCATION"));

        var reserved = await service.ReserveMaterialAsync("WO-1", "ITEM-1", 6m, "EA", "WH-1", "omar", AsOf);

        Assert.True(reserved.Success, reserved.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(2, Count(database, "WORK_ORDER_MATERIAL_ALLOCATION"));

        // ---- a labour line is recorded with its rate and its actor ----
        var now = DateTime.UtcNow;

        var labour = await service.AddLabourAsync("WO-1", "nadia", "GENERAL", now.AddHours(-2), now, 2m, "HR", 45m, "USD",
            "nadia", AsOf);

        Assert.True(labour.Success, labour.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Contains(labour.Value!.Labour, line => line.Person == "nadia" && line.Cost == 90m);
        Assert.Equal(3, Count(database, "WORK_ORDER_LABOR_USAGE WHERE WORK_ORDER_ID = 'WO-1'"));

        // ---- a failed read is a failure, not an empty order ----
        var offline = new OgWorkOrderDetailService(() => null, reporter);
        var noConnection = await offline.LoadAsync("WO-1", AsOf);

        Assert.False(noConnection.Success);
        Assert.Null(noConnection.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
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

            CREATE TABLE R_WO_TYPE (
                WORK_ORDER_TYPE TEXT,
                ABBREVIATION TEXT,
                ACTIVE_IND TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                LONG_NAME TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SHORT_NAME TEXT,
                SOURCE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (WORK_ORDER_TYPE));

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

            CREATE TABLE WORK_ORDER_LABOR_USAGE (
                UNIT_COST REAL,
                CURRENCY_CODE TEXT,
                LABOR_ID TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                FIELD_ID TEXT,
                WORK_ORDER_ID TEXT,
                PROCESS_INSTANCE_ID TEXT,
                START_ID TEXT,
                ASSIGNMENT_ID TEXT,
                EMPLOYEE_BA_ID TEXT,
                EXECUTOR_USER_ID TEXT,
                DUTY_CODE TEXT,
                LABOR_FROM_UTC TEXT,
                LABOR_TO_UTC TEXT,
                QUANTITY REAL,
                QUANTITY_UNIT TEXT,
                COMMAND_JSON TEXT,
                RECORDED_REVISION TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (LABOR_ID));

            CREATE TABLE WORK_ORDER_MATERIAL_USAGE (
                USAGE_ID TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                FIELD_ID TEXT,
                WORK_ORDER_ID TEXT,
                PROCESS_INSTANCE_ID TEXT,
                INVENTORY_ITEM_ID TEXT,
                ALLOCATION_ID TEXT,
                START_ID TEXT,
                QUANTITY REAL,
                USED_AT_UTC TEXT,
                COMMAND_JSON TEXT,
                RECORDED_REVISION TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (USAGE_ID));

            CREATE TABLE WORK_ORDER_MATERIAL_ALLOCATION (
                ALLOCATION_ID TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                INVENTORY_ITEM_ID TEXT,
                FIELD_ID TEXT,
                WORK_ORDER_ID TEXT,
                PROCESS_INSTANCE_ID TEXT,
                QUANTITY_UNIT TEXT,
                WAREHOUSE_ID TEXT,
                LOCATION_ID TEXT,
                QUANTITY REAL,
                VALID_FROM_UTC TEXT,
                VALID_TO_UTC TEXT,
                COMMAND_JSON TEXT,
                RECORDED_REVISION TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (ALLOCATION_ID));

            CREATE TABLE WORK_ORDER_CONDITION (
                WORK_ORDER_ID TEXT,
                CONDITION_OBS_NO REAL,
                ACTIVE_IND TEXT,
                BA_ROLE TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                CONDITION_DESC TEXT,
                CONDITION_TYPE TEXT,
                CURRENCY_CONVERSION REAL,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PAYMENT_AMOUNT REAL,
                PAYMENT_AMOUNT_OUOM TEXT,
                PAYMENT_PERCENT REAL,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                WORK_ORDER_TYPE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (WORK_ORDER_ID, CONDITION_OBS_NO));

            CREATE TABLE WORK_ORDER_EXECUTION_PERMIT (
                PERMIT_ID TEXT,
                TENANT_ID TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                FIELD_ID TEXT,
                WORK_ORDER_ID TEXT,
                PROCESS_INSTANCE_ID TEXT,
                ISSUE_INTENT_ID TEXT,
                ISSUED_BY TEXT,
                STATUS TEXT,
                INSPECTION_REVISION TEXT,
                PERMIT_TYPE TEXT,
                JURISDICTION TEXT,
                SCOPE_DESCRIPTION TEXT,
                ISOLATION_REFERENCE TEXT,
                RISK_REFERENCE TEXT,
                PERSONNEL_REFERENCE TEXT,
                MATERIAL_REFERENCE TEXT,
                VALID_FROM_UTC TEXT,
                VALID_TO_UTC TEXT,
                ISSUE_DATA_JSON TEXT,
                INSPECTION_DATA_JSON TEXT,
                DECISION_INTENT_ID TEXT,
                DECIDED_BY TEXT,
                DECIDED_AT_UTC TEXT,
                DECISION_DATA_JSON TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PERMIT_ID));

            CREATE TABLE INVENTORY_ITEM (
                LINKED_ASSET_TYPE TEXT,
                LINKED_ASSET_ID TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                WAREHOUSE_ID TEXT,
                LOCATION_ID TEXT,
                CURRENCY_CODE TEXT,
                INVENTORY_ITEM_ID TEXT,
                ITEM_NUMBER TEXT,
                ITEM_NAME TEXT,
                ITEM_TYPE TEXT,
                UNIT_OF_MEASURE TEXT,
                QUANTITY_ON_HAND REAL,
                UNIT_COST REAL,
                TOTAL_VALUE REAL,
                VALUATION_METHOD TEXT,
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
                PRIMARY KEY (INVENTORY_ITEM_ID));

            INSERT INTO R_WO_TYPE (WORK_ORDER_TYPE, LONG_NAME, ACTIVE_IND) VALUES
                ('WO-CORRECTIVE', 'Corrective maintenance', 'Y');

            INSERT INTO WORK_ORDER (WORK_ORDER_ID, WORK_ORDER_NUMBER, WORK_ORDER_TYPE, REQUEST_DATE, DUE_DATE, RUSH_IND,
                ACTIVE_IND) VALUES
                ('WO-1', 'WO-0001', 'WO-CORRECTIVE', '2026-11-01', '2026-11-10', 'N', 'Y');

            INSERT INTO WORK_ORDER_COMPONENT (WORK_ORDER_ID, COMPONENT_ID, FIELD_ID, UWI, EMPLOYEE_BA_ID, PROJECT_ID,
                COMPONENT_TYPE, ACTIVE_IND) VALUES
                ('WO-1', 'C-1', 'F-1', 'UWI-1', 'omar', 'P-1', 'WELL', 'Y');

            INSERT INTO PROJECT_STATUS (PROJECT_ID, STATUS_ID, STATUS_TYPE, STATUS, EFFECTIVE_DATE, ACTIVE_IND,
                ROW_CREATED_BY) VALUES
                ('P-1', 'S-1', 'WO_STATE', 'PLANNED', '2026-11-02', 'Y', 'sara');

            INSERT INTO WORK_ORDER_LABOR_USAGE (LABOR_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, FIELD_ID, WORK_ORDER_ID,
                PROCESS_INSTANCE_ID, START_ID, ASSIGNMENT_ID, EMPLOYEE_BA_ID, EXECUTOR_USER_ID, DUTY_CODE, LABOR_FROM_UTC,
                LABOR_TO_UTC, QUANTITY, QUANTITY_UNIT, COMMAND_JSON, RECORDED_REVISION, UNIT_COST, CURRENCY_CODE,
                ACTIVE_IND) VALUES
                ('L-1', 'APP', 'APP', 'F-1', 'WO-1', '', '', '', 'omar', 'omar', 'GENERAL', '2026-11-03 08:00:00',
                 '2026-11-03 10:00:00', 2, 'HR', '{}', 'r1', 50, 'USD', 'Y'),
                ('L-2', 'APP', 'APP', 'F-1', 'WO-1', '', '', '', 'omar', 'omar', 'GENERAL', '2026-11-03 10:00:00',
                 '2026-11-03 11:00:00', 1, 'HR', '{}', 'r2', NULL, 'USD', 'Y');

            INSERT INTO INVENTORY_ITEM (INVENTORY_ITEM_ID, ITEM_NUMBER, ITEM_NAME, ITEM_TYPE, UNIT_OF_MEASURE,
                QUANTITY_ON_HAND, UNIT_COST, CURRENCY_CODE, ACTIVE_IND) VALUES
                ('ITEM-1', 'MAT-1', 'WORK-C-1', 'MATERIAL', 'EA', 10, 12.50, 'USD', 'Y');

            INSERT INTO WORK_ORDER_MATERIAL_ALLOCATION (ALLOCATION_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID,
                INVENTORY_ITEM_ID, FIELD_ID, WORK_ORDER_ID, QUANTITY, QUANTITY_UNIT, ACTIVE_IND) VALUES
                ('AL-1', 'APP', 'APP', 'ITEM-1', 'F-1', 'WO-9', 4, 'EA', 'Y');

            INSERT INTO WORK_ORDER_MATERIAL_USAGE (USAGE_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, FIELD_ID, WORK_ORDER_ID,
                INVENTORY_ITEM_ID, ALLOCATION_ID, START_ID, QUANTITY, USED_AT_UTC, COMMAND_JSON, RECORDED_REVISION,
                ACTIVE_IND) VALUES
                ('U-1', 'APP', 'APP', 'F-1', 'WO-1', 'ITEM-1', 'AL-1', '', 3, '2026-11-04 09:00:00', '{}', 'r1', 'Y');

            INSERT INTO WORK_ORDER_CONDITION (WORK_ORDER_ID, CONDITION_OBS_NO, CONDITION_TYPE, BUSINESS_ASSOCIATE_ID,
                REMARK, EFFECTIVE_DATE, ACTIVE_IND) VALUES
                ('WO-1', 1, 'CLOSE_OUT', 'nadia', 'Completion inspection passed.', '2026-11-07', 'Y');

            INSERT INTO WORK_ORDER_EXECUTION_PERMIT (PERMIT_ID, WORK_ORDER_ID, STATUS, VALID_FROM_UTC, VALID_TO_UTC,
                ACTIVE_IND) VALUES
                ('PT-1', 'WO-1', 'ISSUED', '2026-01-01', '2027-01-01', 'Y');
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
