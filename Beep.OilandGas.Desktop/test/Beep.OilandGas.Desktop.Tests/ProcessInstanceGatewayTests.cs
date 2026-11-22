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
/// WFL-03 against a real SQLite database: the instance is opened by its exact identity and a duplicated identity is refused
/// rather than resolved by recency (AC-1, SG-14-2); a step with no SLA is unscheduled (AC-2); a stored timestamp arrives without a
/// zone and is shown as unconfirmed (AC-3); the timeline holds every step and every event in order (AC-4); every entry names its
/// actor (AC-5); and the originating record is resolved when the entity type exists and shown as a broken link when it does not
/// (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ProcessInstanceGatewayTests
{
    private const string ConnectionName = "wfl03-instances";

    [IntegrationFact]
    public async Task An_instance_is_opened_by_identity_and_its_timeline_is_complete()
    {        var root = Path.Combine(OgPaths.DataRoot, "wfl03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWfl03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "instances.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "instances.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgProcessInstancesService(() => source, reporter);
        var asOf = DateTime.UtcNow;

        // ---- the list ----
        var listed = await service.LoadAsync("F-1", null, null, null, asOf);

        Assert.True(listed.Success, listed.Message + " :: " + string.Join(" | ", reporter.Failures));
        // Every instance on this field is listed, including the identity pair the detail refuses.
        Assert.Equal(6, listed.Value!.Instances.Count);
        Assert.Null(listed.Value.Selected);

        // ---- AC-1 and SG-14-2: exact identity; a duplicated identity is refused, never resolved by recency ----
        var opened = await service.LoadAsync("F-1", "PI-1", null, null, asOf);

        Assert.True(opened.Success, opened.Message);
        Assert.Equal("PI-1", opened.Value!.Selected!.Id);

        // Two rows whose ids differ only in case are an ambiguous identity, not a duplicate key.
        var ambiguous = await service.LoadAsync("F-1", "PI-X", null, null, asOf);

        Assert.False(ambiguous.Success);
        Assert.Equal(ProcessInstanceRules.CodeAmbiguousInstance, ambiguous.Reference);
        Assert.Null(ambiguous.Value);

        // Opening by subject, where the subject owns two instances, is refused the same way.
        var bySubject = await service.LoadAsync("F-1", null, "WELL", "W-3", asOf);

        Assert.False(bySubject.Success);
        Assert.Equal(ProcessInstanceRules.CodeSubjectAmbiguous, bySubject.Reference);
        Assert.Contains("PI-X", bySubject.Message!, StringComparison.Ordinal);
        Assert.Contains("pi-x", bySubject.Message!, StringComparison.Ordinal);

        // The subject that owns one instance opens it.
        var single = await service.LoadAsync("F-1", null, "WELL", "W-2", asOf);

        Assert.True(single.Success, single.Message);
        Assert.Equal("PI-2", single.Value!.Selected!.Id);

        // ---- AC-2 and AC-3: the due date and the zone ----
        var selected = opened.Value.Selected!;

        // The open step is the one that has not completed - not the last completed one.
        Assert.Equal("Technical review", selected.CurrentStepName);
        Assert.Equal("S-3", selected.CurrentStepKey);
        Assert.Equal("omar", selected.Assignee);
        Assert.Equal(ProcessDashboardRules.Breached, selected.SlaState);
        Assert.False(selected.IsUnscheduled);
        Assert.NotNull(selected.DueAt);

        // PI-2's current step records no SLA: unscheduled, and never overdue.
        var unscheduled = single.Value.Selected!;

        Assert.True(unscheduled.IsUnscheduled);
        Assert.Equal("unscheduled", unscheduled.DueText);
        Assert.Null(unscheduled.DueAt);

        // A stored timestamp carries no zone, so it is shown as unconfirmed rather than read as UTC.
        Assert.All(opened.Value.Timeline.Where(entry => entry.At is not null),
            entry => Assert.False(entry.ZoneConfirmed));
        Assert.Contains("(unconfirmed)", opened.Value.TimelineText, StringComparison.Ordinal);
        Assert.Contains("no confirmed zone", opened.Value.BlocksText, StringComparison.Ordinal);

        // ---- AC-4: every step and every event is present, in order ----
        Assert.Equal(3, opened.Value.Timeline.Count(entry => entry.Kind == "STEP"));
        Assert.Equal(2, opened.Value.Timeline.Count(entry => entry.Kind == "EVENT"));
        Assert.Equal(5, opened.Value.Events);
        Assert.Equal(2, opened.Value.History.Count);

        Assert.Equal(
            opened.Value.Timeline.OrderBy(entry => entry.At ?? DateTime.MinValue).Select(entry => entry.StepKey),
            opened.Value.Timeline.Select(entry => entry.StepKey));

        // ---- AC-5: every entry names its actor ----
        Assert.All(opened.Value.Timeline, entry => Assert.True(entry.ActorRecorded, entry.StepKey));
        Assert.All(opened.Value.Timeline, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Actor)));
        Assert.Equal(0, opened.Value.WithoutActor);
        Assert.Contains("sara", opened.Value.TimelineText, StringComparison.Ordinal);

        // ---- AC-6: the subject resolves when its entity type exists, and a missing one is shown as a broken link ----
        Assert.Null(opened.Value.SubjectWarning);
        Assert.Equal("WELL W-1", opened.Value.SubjectText);

        var broken = await service.LoadAsync("F-1", "PI-3", null, null, asOf);

        Assert.True(broken.Success, broken.Message);
        Assert.Contains("cannot be reached", broken.Value!.SubjectWarning!, StringComparison.Ordinal);
        Assert.Contains("WELL_GONE", broken.Value.SubjectWarning!, StringComparison.Ordinal);

        // ---- an instance with no subject says so ----
        var noSubject = await service.LoadAsync("F-1", "PI-4", null, null, asOf);

        Assert.True(noSubject.Success, noSubject.Message);
        Assert.Contains("records no originating subject", noSubject.Value!.SubjectWarning!, StringComparison.Ordinal);

        // ---- identity refusals ----
        var unknown = await service.LoadAsync("F-1", "PI-9", null, null, asOf);

        Assert.False(unknown.Success);
        Assert.Equal(ProcessInstanceRules.CodeUnknownInstance, unknown.Reference);

        var noIdentity = await service.LoadAsync("F-1", " ", null, null, asOf);

        Assert.True(noIdentity.Success, "A list without a selection is a valid read.");

        // ---- a failed read is a failure, not an empty timeline ----
        Execute(database, "DROP TABLE PROCESS_STEP_INSTANCE;");

        var failed = await service.LoadAsync("F-1", "PI-1", null, null, asOf);

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
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

            CREATE TABLE WELL (UWI TEXT PRIMARY KEY);

            INSERT INTO PROCESS_DEFINITION (PROCESS_DEFINITION_ID, PROCESS_NAME, PROCESS_TYPE, VERSION, IS_ACTIVE) VALUES
                ('PD-1', 'Drilling approval', 'APPROVAL', '2.1', 'Y');

            INSERT INTO PROCESS_INSTANCE (PROCESS_INSTANCE_ID, PROCESS_DEFINITION_ID, ENTITY_TYPE, ENTITY_ID, FIELD_ID,
                CURRENT_STATE, STATUS, STARTED_DATE, CURRENT_STEP_ID) VALUES
                ('PI-1', 'PD-1', 'WELL', 'W-1', 'F-1', 'PEER_REVIEW', 'IN_PROGRESS', '2026-10-12 08:00:00', 'S-2'),
                ('PI-X', 'PD-1', 'WELL', 'W-3', 'F-1', 'PEER_REVIEW', 'IN_PROGRESS', '2026-11-01 08:00:00', 'S-2'),
                ('pi-x', 'PD-1', 'WELL', 'W-3', 'F-1', 'PEER_REVIEW', 'IN_PROGRESS', '2026-11-01 09:00:00', 'S-2'),
                ('PI-2', 'PD-1', 'WELL', 'W-2', 'F-1', 'SUBMITTED', 'IN_PROGRESS', '2026-11-02 08:00:00', 'S-1'),
                ('PI-3', 'PD-1', 'WELL_GONE', 'W-9', 'F-1', 'SUBMITTED', 'IN_PROGRESS', '2026-11-03 08:00:00', 'S-1'),
                ('PI-4', 'PD-1', NULL, NULL, 'F-1', 'SUBMITTED', 'IN_PROGRESS', '2026-11-04 08:00:00', 'S-1');

            INSERT INTO PROCESS_STEP_INSTANCE (PROCESS_STEP_INSTANCE_ID, PROCESS_INSTANCE_ID, STEP_ID, STEP_NAME,
                STEP_SEQUENCE, STATUS, ASSIGNED_TO, STARTED_DATE, COMPLETION_DATE, COMPLETED_BY, OUTCOME, NOTES,
                SLA_HOURS) VALUES
                ('PS-1', 'PI-1', 'S-1', 'Submit', 1, 'COMPLETED', 'sara', '2026-10-12 08:00:00', '2026-10-12 09:00:00',
                 'sara', 'SUBMITTED', 'Filed', 24),
                ('PS-2', 'PI-1', 'S-2', 'Peer review', 2, 'COMPLETED', 'nadia', '2026-10-13 08:00:00', '2026-10-15 08:00:00',
                 'nadia', 'APPROVED', 'Peer review passed', 72),
                ('PS-3', 'PI-1', 'S-3', 'Technical review', 3, 'IN_PROGRESS', 'omar', '2026-10-20 08:00:00', NULL,
                 NULL, NULL, NULL, 72),
                ('PS-4', 'PI-2', 'S-1', 'Submit', 1, 'IN_PROGRESS', 'sara', '2026-11-02 08:00:00', NULL, NULL, NULL, NULL, NULL);

            INSERT INTO PROCESS_HISTORY (PROCESS_HISTORY_ID, PROCESS_INSTANCE_ID, EVENT_TYPE, EVENT_DATE, USER_ID,
                FROM_STATUS, TO_STATUS, DETAILS) VALUES
                ('PH-1', 'PI-1', 'STATE_CHANGED', '2026-10-12 09:00:00', 'sara', 'DRAFT', 'SUBMITTED', 'Filed'),
                ('PH-2', 'PI-1', 'STATE_CHANGED', '2026-10-20 08:00:00', 'nadia', 'SUBMITTED', 'TECHNICAL_REVIEW', 'Passed'),
                ('PH-3', 'PI-2', 'STATE_CHANGED', '2026-11-02 08:00:00', 'sara', 'DRAFT', 'SUBMITTED', 'Filed');
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
