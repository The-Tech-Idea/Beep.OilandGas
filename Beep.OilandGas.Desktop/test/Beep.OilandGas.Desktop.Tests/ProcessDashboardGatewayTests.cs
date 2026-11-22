using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WFL-01 against a real SQLite database: throughput is counted from the history record and not from a status column (AC-1);
/// an instance with no SLA is reported as "no SLA" rather than met, and an unstarted step as unscheduled (AC-2, AC-3); the
/// bottleneck is the step with the longest median dwell (AC-4); a failed read carries no dashboard (AC-5); and the
/// per-definition counts reconcile to the instance list (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ProcessDashboardGatewayTests
{
    private const string ConnectionName = "wfl01-processes";

    /// <summary>
    /// The end of the period under test: fixed, because the seed dates are fixed. Aging against the wall clock made the
    /// "within SLA" expectation expire about three and a half days after the seed, which is how this test was found.
    /// </summary>
    private static readonly DateTime AsOf = new(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);

    [IntegrationFact]
    public async Task Throughput_ageing_and_the_bottleneck_come_from_the_records()
    {        var root = Path.Combine(OgPaths.DataRoot, "wfl01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWfl01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "processes.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "processes.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgProcessDashboardService(() => source, reporter);

        var loaded = await service.LoadAsync("F-1", AsOf.AddDays(-30), AsOf);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var dashboard = loaded.Value!;

        // ---- AC-1: throughput is the history record, per definition ----
        var drilling = dashboard.Definitions.Single(definition => definition.DefinitionId == "PD-1");

        Assert.Equal("Drilling approval", drilling.Name);
        Assert.Equal("2.1", drilling.Version);
        // Three instances recorded a history event inside the period: two moved between states and one completed.
        Assert.Equal(3, drilling.Moved);
        Assert.Equal(4, drilling.Instances);
        Assert.Equal(3, drilling.Running);
        Assert.Equal(1, drilling.Completed);

        Assert.Equal(3, dashboard.MovedInPeriod);

        // ---- AC-6: the per-definition counts reconcile to the instances in scope ----
        Assert.Equal(4, dashboard.InstancesInScope);
        Assert.Equal(dashboard.Ageing.Count, dashboard.Kpis.Running);
        Assert.Equal(1, dashboard.Kpis.Completed);

        // The second definition is on another field: it is absent here and present when the field filter is removed, and its
        // only instance recorded its event outside the period, so it moved nothing.
        Assert.DoesNotContain(dashboard.Definitions, definition => definition.DefinitionId == "PD-2");

        var allFields = await service.LoadAsync(null, AsOf.AddDays(-30), AsOf);

        Assert.True(allFields.Success, allFields.Message);

        var completions = allFields.Value!.Definitions.Single(definition => definition.DefinitionId == "PD-2");

        Assert.Equal(0, completions.Moved);
        Assert.Equal(1, completions.Instances);
        Assert.Equal(5, allFields.Value.InstancesInScope);

        // ---- AC-2 and AC-3: the states, each naming its own reason ----
        var breached = dashboard.Ageing.Single(row => row.InstanceId == "PI-1");

        Assert.Equal("breached", breached.SlaState);
        Assert.True(breached.IsBreached);
        Assert.Equal(72d, breached.SlaHours);
        Assert.Contains("day(s)", breached.DaysText, StringComparison.Ordinal);

        var unscheduled = dashboard.Ageing.Single(row => row.InstanceId == "PI-2");

        Assert.Equal("unscheduled", unscheduled.SlaState);
        Assert.Equal("no start recorded", unscheduled.DaysText);

        var withinSla = dashboard.Ageing.Single(row => row.InstanceId == "PI-3");

        Assert.Equal("within SLA", withinSla.SlaState);

        Assert.Equal(1, dashboard.Kpis.Breached);
        Assert.Equal(1, dashboard.Kpis.Unscheduled);

        // The instance whose current step records no SLA is "no SLA", and is not counted as met.
        Execute(database, "UPDATE PROCESS_STEP_INSTANCE SET SLA_HOURS = NULL WHERE PROCESS_STEP_INSTANCE_ID = 'PS-3';");

        var noSla = await service.LoadAsync("F-1", AsOf.AddDays(-30), AsOf);

        Assert.True(noSla.Success, noSla.Message);
        Assert.Equal("no SLA",
            noSla.Value!.Ageing.Single(row => row.InstanceId == "PI-3").SlaState);

        // ---- AC-4: the bottleneck is the step with the longest median dwell ----
        var bottleneck = dashboard.Bottleneck;

        Assert.NotNull(bottleneck);
        Assert.Equal("Peer review", bottleneck!.Step);
        Assert.Equal(4m, bottleneck.MedianDays);
        Assert.Equal(3, bottleneck.Count);
        Assert.Contains("median 4.0 day(s) over 3 recorded step(s)", dashboard.BottleneckText, StringComparison.Ordinal);

        // ---- the average cycle time comes from the completed instance's two dates ----
        Assert.Equal(10m, dashboard.Kpis.AverageCycleDays);

        // ---- AC-5: a failed read is a failure, not a zero ----
        Execute(database, "DROP TABLE PROCESS_HISTORY;");

        var broken = await service.LoadAsync("F-1", AsOf.AddDays(-30), AsOf);

        Assert.False(broken.Success);
        Assert.Null(broken.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE PROCESS_DEFINITION (
                PROCESS_DEFINITION_ID TEXT,
                PROCESS_NAME TEXT,
                PROCESS_TYPE TEXT,
                ENTITY_TYPE TEXT,
                DESCRIPTION TEXT,
                VERSION TEXT,
                IS_ACTIVE TEXT,
                PROCESS_CONFIG_JSON TEXT,
                REQUIRED_ROLE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PROCESS_DEFINITION_ID));

            CREATE TABLE PROCESS_INSTANCE (
                PROCESS_INSTANCE_ID TEXT,
                PROCESS_DEFINITION_ID TEXT,
                ENTITY_TYPE TEXT,
                ENTITY_ID TEXT,
                FIELD_ID TEXT,
                CURRENT_STATE TEXT,
                STATUS TEXT,
                STARTED_DATE TEXT,
                COMPLETED_DATE TEXT,
                COMPLETED_BY TEXT,
                CURRENT_STEP_ID TEXT,
                CURRENT_STEP_SEQUENCE INTEGER,
                INSTANCE_DATA_JSON TEXT,
                PARENT_PROCESS_INSTANCE_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PROCESS_INSTANCE_ID));

            CREATE TABLE PROCESS_STEP_INSTANCE (
                PROCESS_STEP_INSTANCE_ID TEXT,
                PROCESS_INSTANCE_ID TEXT,
                STEP_ID TEXT,
                STEP_NAME TEXT,
                STEP_SEQUENCE INTEGER,
                STATUS TEXT,
                ASSIGNED_TO TEXT,
                STARTED_DATE TEXT,
                COMPLETION_DATE TEXT,
                COMPLETED_BY TEXT,
                STEP_DATA_JSON TEXT,
                OUTCOME TEXT,
                NOTES TEXT,
                REQUIRED_ROLE TEXT,
                SLA_HOURS REAL,
                APPROVAL_REQUIRED INTEGER,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PROCESS_STEP_INSTANCE_ID));

            CREATE TABLE PROCESS_HISTORY (
                PROCESS_HISTORY_ID TEXT,
                PROCESS_INSTANCE_ID TEXT,
                PROCESS_STEP_INSTANCE_ID TEXT,
                EVENT_TYPE TEXT,
                EVENT_DATE TEXT,
                USER_ID TEXT,
                DETAILS TEXT,
                EVENT_DATA_JSON TEXT,
                FROM_STATUS TEXT,
                TO_STATUS TEXT,
                IP_ADDRESS TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PROCESS_HISTORY_ID));

            INSERT INTO PROCESS_DEFINITION (PROCESS_DEFINITION_ID, PROCESS_NAME, PROCESS_TYPE, VERSION, IS_ACTIVE) VALUES
                ('PD-1', 'Drilling approval', 'APPROVAL', '2.1', 'Y'),
                ('PD-2', 'Completion sign-off', 'APPROVAL', '1.0', 'Y');

            INSERT INTO PROCESS_INSTANCE (PROCESS_INSTANCE_ID, PROCESS_DEFINITION_ID, ENTITY_TYPE, ENTITY_ID, FIELD_ID,
                CURRENT_STATE, STATUS, STARTED_DATE, COMPLETED_DATE) VALUES
                ('PI-1', 'PD-1', 'WELL', 'W-1', 'F-1', 'PEER_REVIEW', 'IN_PROGRESS', '2026-10-12 08:00:00', NULL),
                ('PI-2', 'PD-1', 'WELL', 'W-2', 'F-1', 'SUBMITTED', 'IN_PROGRESS', '2026-11-01 08:00:00', NULL),
                ('PI-3', 'PD-1', 'WELL', 'W-3', 'F-1', 'TECHNICAL_REVIEW', 'IN_PROGRESS', '2026-11-06 08:00:00', NULL),
                ('PI-4', 'PD-1', 'WELL', 'W-4', 'F-1', 'COMPLETED', 'COMPLETED', '2026-10-20 08:00:00', '2026-10-30 08:00:00'),
                ('PI-5', 'PD-2', 'FIELD', 'F-1', 'F-2', 'SUBMITTED', 'IN_PROGRESS', '2026-09-01 08:00:00', NULL);

            INSERT INTO PROCESS_STEP_INSTANCE (PROCESS_STEP_INSTANCE_ID, PROCESS_INSTANCE_ID, STEP_ID, STEP_NAME,
                STEP_SEQUENCE, STATUS, STARTED_DATE, COMPLETION_DATE, SLA_HOURS, APPROVAL_REQUIRED) VALUES
                ('PS-1', 'PI-1', 'S-2', 'Peer review', 2, 'IN_PROGRESS', '2026-10-12 08:00:00', NULL, 72, 1),
                ('PS-2', 'PI-2', 'S-1', 'Submit', 1, 'IN_PROGRESS', NULL, NULL, 48, 1),
                ('PS-3', 'PI-3', 'S-3', 'Technical review', 3, 'IN_PROGRESS', '2026-11-06 08:00:00', NULL, 168, 1),
                ('PS-4', 'PI-4', 'S-2', 'Peer review', 2, 'COMPLETED', '2026-10-20 08:00:00', '2026-10-24 08:00:00', 72, 1),
                ('PS-5', 'PI-5', 'S-1', 'Submit', 1, 'IN_PROGRESS', '2026-09-01 08:00:00', NULL, 24, 1),
                ('PS-6', 'PI-4', 'S-2', 'Peer review', 2, 'COMPLETED', '2026-10-25 08:00:00', '2026-10-27 08:00:00', 72, 1),
                ('PS-7', 'PI-4', 'S-2', 'Peer review', 2, 'COMPLETED', '2026-10-20 08:00:00', '2026-10-30 08:00:00', 72, 1);

            INSERT INTO PROCESS_HISTORY (PROCESS_HISTORY_ID, PROCESS_INSTANCE_ID, EVENT_TYPE, EVENT_DATE, USER_ID,
                FROM_STATUS, TO_STATUS) VALUES
                ('PH-1', 'PI-1', 'STATE_CHANGED', '2026-11-02 09:00:00', 'sara', 'SUBMITTED', 'PEER_REVIEW'),
                ('PH-2', 'PI-2', 'STATE_CHANGED', '2026-11-03 09:00:00', 'sara', 'DRAFT', 'SUBMITTED'),
                ('PH-3', 'PI-4', 'COMPLETED', '2026-10-30 09:00:00', 'nadia', 'TECHNICAL_REVIEW', 'COMPLETED'),
                ('PH-4', 'PI-5', 'STATE_CHANGED', '2026-07-01 09:00:00', 'sara', 'DRAFT', 'SUBMITTED');
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
