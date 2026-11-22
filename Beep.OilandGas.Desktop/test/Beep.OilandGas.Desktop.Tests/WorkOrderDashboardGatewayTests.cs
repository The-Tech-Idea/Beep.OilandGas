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
/// WRK-01 against a real SQLite database: the queues are filters over the orders the grid lists (AC-1), a resource conflict is
/// found from two overlapping planned spans of one assignee (AC-2), overdue is computed from the owner's due date (AC-3), the
/// calendar holds the same orders as the grid (AC-4), a failed read is a failure rather than an empty queue (AC-5), and
/// unassigned work is its own queue (AC-6). The state comes from the linked project's recorded status, not the header.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WorkOrderDashboardGatewayTests
{
    private const string ConnectionName = "wrk01-work";

    private static readonly DateTime AsOf = new(2026, 11, 8);

    [IntegrationFact]
    public async Task The_dashboard_reads_the_orders_their_state_and_their_conflicts()
    {        var root = Path.Combine(OgPaths.DataRoot, "wrk01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWrk01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "work.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "work.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var work = new OgWorkOrders(() => source, reporter);

        var loaded = await work.LoadAsync("F-1", null, null, "omar", AsOf);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;

        // ---- the grid: this field's orders, with the owner's type name and the linked project's state ----
        Assert.Equal(3, workspace.Total);
        Assert.DoesNotContain(workspace.Orders, order => order.Number == "WO-0004");
        Assert.Equal(3, workspace.CalendarOrders);

        var first = workspace.Orders.Single(order => order.Number == "WO-0001");

        Assert.Equal("Corrective maintenance", first.Kind);
        Assert.Equal("F-1", first.FieldId);
        Assert.Equal("UWI-1", first.AssetId);
        Assert.Equal("RUSH", first.Priority);
        Assert.Equal("omar", first.Assignee);
        Assert.Equal("IN_PROGRESS", first.State);
        Assert.Equal("in progress", first.StateText);
        Assert.True(first.Overdue);
        Assert.Contains("(overdue)", first.DueText, StringComparison.Ordinal);
        Assert.Equal("not recorded", first.CostText);

        // The preventive order has no state row on its linked project: it says so rather than showing a draft.
        var unstated = workspace.Orders.Single(order => order.Number == "WO-0002");

        Assert.Equal(string.Empty, unstated.State);
        Assert.Equal("no recorded state", unstated.StateText);
        Assert.True(unstated.IsUnassigned);

        // ---- AC-1 and AC-6: every queue is the same filter the grid applies ----
        foreach (var queue in workspace.Queues)
        {
            Assert.Equal(queue.Count, workspace.In(queue.Key).Count);
        }

        Assert.Equal(2, workspace.In(WorkRules.QueueMine).Count);
        Assert.Equal(1, workspace.In(WorkRules.QueueUnassigned).Count);
        Assert.Equal(1, workspace.In(WorkRules.QueueOverdue).Count);
        Assert.Equal(1, workspace.In(WorkRules.QueueApproval).Count);
        Assert.Equal(3, workspace.In(WorkRules.QueueAll).Count);

        // ---- AC-2: omar is planned on two overlapping orders ----
        var conflict = Assert.Single(workspace.Conflicts);

        Assert.Equal("omar", conflict.Assignee);
        Assert.Contains("WO-0001", conflict.Describe(), StringComparison.Ordinal);
        Assert.Contains("WO-0003", conflict.Describe(), StringComparison.Ordinal);
        Assert.Contains("omar is planned on", workspace.ConflictText, StringComparison.Ordinal);

        // ---- AC-3: the completed order on another field is neither listed nor overdue ----
        var allFields = await work.LoadAsync(null, null, null, "omar", AsOf);

        Assert.True(allFields.Success, allFields.Message);
        Assert.Equal(4, allFields.Value!.Total);

        var completed = allFields.Value.Orders.Single(order => order.Number == "WO-0004");

        Assert.Equal("COMPLETED", completed.State);
        Assert.False(completed.Overdue);

        // ---- filters: a state filter is the same predicate the queue uses ----
        var underReview = await work.LoadAsync("F-1", "UNDER_REVIEW", null, "omar", AsOf);

        Assert.True(underReview.Success, underReview.Message);
        Assert.Equal("WO-0003", Assert.Single(underReview.Value!.Orders).Number);

        // ---- AC-4: the calendar covers each order's planned span ----
        var day = workspace.Calendar.Single(entry => entry.Date == new DateTime(2026, 11, 3));

        Assert.Contains("WO-0001", day.Orders);
        Assert.Contains("WO-0003", day.Orders);
        Assert.Contains("2026-11-03", workspace.CalendarText, StringComparison.Ordinal);

        // ---- AC-5: a failed read is a failure, not an empty queue ----
        Execute(database, "DROP TABLE WORK_ORDER_COMPONENT;");

        var broken = await work.LoadAsync("F-1", null, null, "omar", AsOf);

        Assert.False(broken.Success);
        Assert.Null(broken.Value);
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

            INSERT INTO R_WO_TYPE (WORK_ORDER_TYPE, LONG_NAME, SHORT_NAME, ACTIVE_IND) VALUES
                ('WO-CORRECTIVE', 'Corrective maintenance', 'Corrective', 'Y'),
                ('WO-PREVENTIVE', 'Preventive maintenance', 'Preventive', 'Y');

            INSERT INTO WORK_ORDER (WORK_ORDER_ID, WORK_ORDER_NUMBER, WORK_ORDER_TYPE, REQUEST_DATE, DUE_DATE,
                COMPLETE_DATE, RUSH_IND, INSTRUCTIONS, ACTIVE_IND) VALUES
                ('WO-1', 'WO-0001', 'WO-CORRECTIVE', '2026-11-01', '2026-11-05', NULL, 'Y', 'Replace the seal', 'Y'),
                ('WO-2', 'WO-0002', 'WO-PREVENTIVE', '2026-11-18', '2026-11-20', NULL, 'N', 'Annual service', 'Y'),
                ('WO-3', 'WO-0003', 'WO-CORRECTIVE', '2026-11-03', '2026-11-10', NULL, 'N', 'Pressure test', 'Y'),
                ('WO-4', 'WO-0004', 'WO-CORRECTIVE', '2026-10-01', '2026-10-05', '2026-10-04', 'N', 'Done work', 'Y');

            INSERT INTO WORK_ORDER_COMPONENT (WORK_ORDER_ID, COMPONENT_ID, FIELD_ID, UWI, EMPLOYEE_BA_ID,
                PROJECT_ID, COMPONENT_TYPE, ACTIVE_IND) VALUES
                ('WO-1', 'C-1', 'F-1', 'UWI-1', 'omar', 'P-1', 'WELL', 'Y'),
                ('WO-2', 'C-2', 'F-1', 'UWI-1', NULL, 'P-2', 'WELL', 'Y'),
                ('WO-3', 'C-3', 'F-1', 'UWI-1', 'omar', 'P-3', 'WELL', 'Y'),
                ('WO-4', 'C-4', 'F-2', 'UWI-9', 'nadia', 'P-4', 'WELL', 'Y');

            INSERT INTO PROJECT_STATUS (PROJECT_ID, STATUS_ID, STATUS_TYPE, STATUS, EFFECTIVE_DATE, ACTIVE_IND) VALUES
                ('P-1', 'S-1', 'WO_STATE', 'IN_PROGRESS', '2026-11-02', 'Y'),
                ('P-3', 'S-3', 'WO_STATE', 'UNDER_REVIEW', '2026-11-04', 'Y'),
                ('P-4', 'S-4', 'WO_STATE', 'COMPLETED', '2026-10-04', 'Y');
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
