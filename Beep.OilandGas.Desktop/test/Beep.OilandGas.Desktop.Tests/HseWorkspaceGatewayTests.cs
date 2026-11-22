using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-01 against a real SQLite database: the five counts are counted from the records - an incident's latest status
/// response, an obligation's recorded date, a corrective-action step's due date and latest status, a HAZOP deviation's latest
/// status and a permit's latest history entry - the strip separates closed, cancelled and unrecognised states from open, and a
/// failed read returns no counts at all. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HseWorkspaceGatewayTests
{
    private const string ConnectionName = "hse01-workspace";

    private static readonly DateTime AsOf = new(2026, 11, 8);

    [IntegrationFact]
    public async Task Every_count_comes_from_the_records_and_the_states_are_separated()
    {        var root = Path.Combine(OgPaths.DataRoot, "hse01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasHse01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "hse.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "hse.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var workspace = new OgHseWorkspace(() => source, new Inbox(), reporter);

        var loaded = await workspace.LoadAsync(
            "FIELD-1",
            "North Field",
            null,
            "HSE_OFFICER",
            [OgRoleNames.HseOfficer],
            AsOf,
            "UTC",
            route => route == "wells");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var snapshot = loaded.Value!;

        // ---- the counts ----
        // Six incidents are in one of the five active states; one is closed, one cancelled, and two are unrecognised or
        // have no status row.
        Assert.Equal(6, snapshot.Counts.OpenIncidents);
        Assert.Equal(6, snapshot.OpenFromStrip);

        // The latest status wins: I-3 has REPORTED then UNDER_INVESTIGATION, so it is under investigation.
        Assert.Equal(1, snapshot.IncidentStates.Single(row => row.State == "UNDER_INVESTIGATION").Count);

        // An inactive status row and a non-STATUS response change nothing.
        Assert.Equal(1, snapshot.IncidentStates.Single(row => row.State == "PENDING_RCA").Count);

        Assert.Equal(1, snapshot.IncidentStates.Single(row => row.State == "CLOSED").Count);
        Assert.Equal(1, snapshot.IncidentStates.Single(row => row.State == "CANCELLED").Count);
        Assert.Equal(2, snapshot.IncidentStates.Single(row => row.State == HseRules.OtherState).Count);

        // Two of five obligations have reached their recorded date and are unfulfilled.
        Assert.Equal(2, snapshot.Counts.ObligationsDue);

        // Two corrective-action steps are past due and not completed; the completed one and the future one are not.
        Assert.Equal(2, snapshot.Counts.OverdueActions);

        // Two HAZOP deviations are still open; the one whose latest status is closed is not.
        Assert.Equal(2, snapshot.Counts.OpenHazopActions);

        // Two applications are still waiting; the approved and the draft ones are not.
        Assert.Equal(2, snapshot.Counts.PermitsPending);

        // The strip carries every row, so a closed incident is never silently folded into open.
        Assert.Equal(8, snapshot.IncidentStates.Count);
        Assert.All(snapshot.IncidentStates.Where(row => !row.Active), row => Assert.False(row.Active));

        // ---- the jurisdiction is not invented ----
        Assert.Equal("jurisdiction not specified", snapshot.JurisdictionText);
        Assert.Equal(string.Empty, snapshot.Jurisdiction);

        // ---- the links: only what the shell would open, and the rest with their reason ----
        Assert.True(snapshot.Links.Single(link => link.Route == "wells").Available);
        Assert.False(snapshot.Links.Single(link => link.Route == "tasks").Available);
        Assert.Contains("may not open", snapshot.Links.Single(link => link.Route == "tasks").WhyNot!,
            StringComparison.Ordinal);
        Assert.Contains("may not open", snapshot.Links.Single(link => link.Route == "incidents").WhyNot!,
            StringComparison.Ordinal);
        Assert.Single(snapshot.AvailableLinks);

        // ---- the composed inbox ----
        Assert.True(snapshot.Inbox.Available);
        Assert.Equal(3, snapshot.Inbox.Overdue);
        Assert.Equal(7, snapshot.Inbox.All);

        Assert.Contains("last refreshed", snapshot.RefreshedText, StringComparison.Ordinal);
        Assert.Contains("open incidents 6", snapshot.Counts.Summary, StringComparison.Ordinal);
        Assert.Contains("1 source(s) available", snapshot.NoticeText, StringComparison.Ordinal);

        // ---- AC-3: a failed read carries no counts ----
        Execute(database, "DROP TABLE HSE_INCIDENT_RESPONSE;");

        var broken = await workspace.LoadAsync(
            "FIELD-1", "North Field", null, "HSE_OFFICER", [OgRoleNames.HseOfficer], AsOf, "UTC", _ => true);

        Assert.False(broken.Success);
        Assert.Null(broken.Value);
        Assert.False(string.IsNullOrWhiteSpace(broken.Message));
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database,
            """
            CREATE TABLE HSE_INCIDENT (
                INCIDENT_ID TEXT,
                ACTIVE_IND TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                INCIDENT_CLASS_ID TEXT,
                INCIDENT_DATE TEXT,
                INCIDENT_DURATION REAL,
                INCIDENT_DURATION_OUOM TEXT,
                INCIDENT_DURATION_UOM TEXT,
                LOST_TIME_IND TEXT,
                PPDM_GUID TEXT,
                RECORDED_TIME TEXT,
                RECORDED_TIMEZONE TEXT,
                REMARK TEXT,
                REPORTED_BY_BA_ID TEXT,
                REPORTED_BY_NAME TEXT,
                REPORTED_IND TEXT,
                SOURCE TEXT,
                WORK_RELATED_IND TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE HSE_INCIDENT_RESPONSE (
                INCIDENT_ID TEXT,
                RESPONSE_OBS_NO REAL,
                ACTIVE_IND TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PPDM_GUID TEXT,
                PROJECT_ID TEXT,
                REMARK TEXT,
                RESPONSE_RESULT TEXT,
                RESPONSE_TYPE TEXT,
                SOURCE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE OBLIGATION (
                OBLIGATION_ID TEXT PRIMARY KEY,
                OBLIGATION_SEQ_NO REAL,
                ACTIVE_IND TEXT,
                CALCULATION_METHOD TEXT,
                CONVERTIBLE_IND TEXT,
                CRITICAL_DATE TEXT,
                CURRENCY_CONVERSION REAL,
                CURRENCY_OUOM TEXT,
                DESCRIPTION TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FULFILLED_DATE TEXT,
                FULFILLED_IND TEXT,
                GROSS_OBLIGATION_COST REAL,
                INSTRUMENT_ID TEXT,
                LIABILITY_RELEASE_DATE TEXT,
                NET_OBLIGATION_COST REAL,
                NOTICE_PERIOD_LENGTH REAL,
                NOTICE_PERIOD_OUOM TEXT,
                OBLIGATION_CATEGORY TEXT,
                OBLIGATION_DURATION REAL,
                OBLIGATION_DURATION_OUOM TEXT,
                OBLIGATION_FREQUENCY TEXT,
                OBLIGATION_TYPE TEXT,
                PAYMENT_IND TEXT,
                PAYMENT_RESPONSIBILITY TEXT,
                PERCENTAGE REAL,
                POTENTIAL_OBLIGATION_DESC TEXT,
                POTENTIAL_OBLIGATION_IND TEXT,
                PPDM_GUID TEXT,
                PREPAID_IND TEXT,
                REMARK TEXT,
                RESP_PARTY_BA_ID TEXT,
                REVIEW_FREQUENCY TEXT,
                ROYALTY_OWNER_BA_ID TEXT,
                ROYALTY_PAYOR_BA_ID TEXT,
                ROYALTY_TYPE TEXT,
                SOURCE TEXT,
                START_DATE TEXT,
                SUBSTANCE_ID TEXT,
                TRIGGER_METHOD TEXT,
                WORK_OBLIGATION_DESC TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

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
                ROW_QUALITY TEXT);

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
                ROW_QUALITY TEXT);

            CREATE TABLE PERMIT_STATUS_HISTORY (
                PERMIT_STATUS_HISTORY_ID TEXT PRIMARY KEY,
                PERMIT_APPLICATION_ID TEXT,
                STATUS TEXT,
                STATUS_DATE TEXT,
                STATUS_REMARKS TEXT,
                UPDATED_BY TEXT,
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

            INSERT INTO HSE_INCIDENT (INCIDENT_ID, ACTIVE_IND, INCIDENT_DATE) VALUES
                ('I-1', 'Y', '2026-10-01'), ('I-2', 'Y', '2026-10-02'), ('I-3', 'Y', '2026-10-03'),
                ('I-4', 'Y', '2026-10-04'), ('I-5', 'Y', '2026-10-05'), ('I-6', 'Y', '2026-10-06'),
                ('I-7', 'Y', '2026-09-01'), ('I-8', 'Y', '2026-09-02'), ('I-9', 'Y', '2026-10-09'),
                ('I-10', 'Y', '2026-10-10');

            INSERT INTO HSE_INCIDENT_RESPONSE (INCIDENT_ID, RESPONSE_OBS_NO, RESPONSE_TYPE, RESPONSE_RESULT,
                EFFECTIVE_DATE, ACTIVE_IND) VALUES
                ('I-1', 1, 'STATUS', 'REPORTED', '2026-10-01', 'Y'),
                ('I-2', 1, 'STATUS', 'REPORTED', '2026-10-02', 'Y'),
                ('I-3', 1, 'STATUS', 'REPORTED', '2026-10-03', 'Y'),
                ('I-3', 2, 'STATUS', 'UNDER_INVESTIGATION', '2026-10-05', 'Y'),
                ('I-4', 1, 'STATUS', 'PENDING_RCA', '2026-10-04', 'Y'),
                ('I-5', 1, 'STATUS', 'PENDING_CORRECTIVE_ACTION', '2026-10-05', 'Y'),
                ('I-6', 1, 'STATUS', 'PENDING_CLOSE_OUT', '2026-10-06', 'Y'),
                ('I-7', 1, 'STATUS', 'PENDING_CLOSE_OUT', '2026-09-01', 'Y'),
                ('I-7', 2, 'STATUS', 'CLOSED', '2026-09-20', 'Y'),
                -- An inactive status row must not win over the active one.
                ('I-7', 3, 'STATUS', 'REPORTED', '2026-09-25', 'N'),
                ('I-8', 1, 'STATUS', 'CANCELLED', '2026-09-02', 'Y'),
                ('I-9', 1, 'STATUS', 'ONSITE_REVIEW', '2026-10-09', 'Y'),
                -- A non-STATUS response is not a state.
                ('I-4', 2, 'CAUSE', 'EQUIPMENT', '2026-10-06', 'Y');

            INSERT INTO OBLIGATION (OBLIGATION_ID, OBLIGATION_SEQ_NO, ACTIVE_IND, FULFILLED_IND,
                CRITICAL_DATE, EXPIRY_DATE) VALUES
                ('OB-1', 1, 'Y', 'N', '2026-11-01', NULL),
                ('OB-2', 1, 'Y', 'N', NULL, '2026-11-05'),
                ('OB-3', 1, 'Y', 'N', '2026-12-01', NULL),
                ('OB-4', 1, 'Y', 'Y', '2026-01-01', NULL),
                ('OB-5', 1, 'Y', 'N', NULL, NULL);

            INSERT INTO PROJECT (PROJECT_ID, PROJECT_TYPE, PROJECT_NAME, ACTIVE_IND) VALUES
                ('P-CA1', 'CORRECTIVE_ACTION', 'Incident I-1 actions', 'Y'),
                ('P-HZ1', 'HAZOP', 'Compression HAZOP', 'Y'),
                ('P-HZ2', 'HAZOP', 'Separator HAZOP', 'Y');

            INSERT INTO PROJECT_STEP (PROJECT_ID, STEP_ID, STEP_NAME, STEP_SEQ_NO, DUE_DATE, ACTIVE_IND) VALUES
                ('P-CA1', 'S-1', 'Replace seal', 1, '2026-11-01', 'Y'),
                ('P-CA1', 'S-2', 'Update procedure', 2, '2026-11-01', 'Y'),
                ('P-CA1', 'S-3', 'Train crew', 3, '2026-12-01', 'Y'),
                ('P-CA1', 'S-4', 'Inspect valve', 4, '2026-11-01', 'Y');

            INSERT INTO PROJECT_STATUS (PROJECT_ID, STATUS_ID, STEP_ID, STATUS_TYPE, STATUS, EFFECTIVE_DATE,
                ACTIVE_IND) VALUES
                ('P-CA1', 'CS-1', 'S-1', 'CA_STEP', 'OPEN', '2026-10-01', 'Y'),
                -- S-2 was open, then completed: the latest recorded status wins.
                ('P-CA1', 'CS-2', 'S-2', 'CA_STEP', 'OPEN', '2026-09-01', 'Y'),
                ('P-CA1', 'CS-3', 'S-2', 'CA_STEP', 'COMPLETED', '2026-10-20', 'Y'),
                ('P-HZ1', 'D-1', NULL, 'HAZOP_DEVIATION', 'OPEN', '2026-09-01', 'Y'),
                ('P-HZ1', 'D-2', NULL, 'HAZOP_DEVIATION', 'ACTION_REQUIRED', '2026-09-02', 'Y'),
                ('P-HZ2', 'D-3', NULL, 'HAZOP_DEVIATION', 'OPEN', '2026-08-01', 'Y'),
                ('P-HZ2', 'D-3', NULL, 'HAZOP_DEVIATION', 'CLOSED', '2026-09-15', 'Y');

            INSERT INTO PERMIT_STATUS_HISTORY (PERMIT_STATUS_HISTORY_ID, PERMIT_APPLICATION_ID, STATUS,
                STATUS_DATE, UPDATED_BY) VALUES
                ('H-1', 'A-1', 'SUBMITTED', '2026-10-01', 'sara'),
                ('H-2', 'A-1', 'UNDER_REVIEW', '2026-10-05', 'sara'),
                ('H-3', 'A-2', 'SUBMITTED', '2026-10-02', 'sara'),
                ('H-4', 'A-3', 'SUBMITTED', '2026-10-03', 'sara'),
                ('H-5', 'A-3', 'APPROVED', '2026-10-10', 'sara'),
                ('H-6', 'A-4', 'DRAFT', '2026-10-04', 'sara');
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class Inbox : IOgTaskInbox
    {
        public Task<OgResult<OgTaskInboxSnapshot>> LoadAsync(
            string personaCode,
            IReadOnlyCollection<string> roles,
            string? fieldId,
            string fieldName,
            OgTaskTab tab,
            DateTime asOf,
            string timeZoneId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgTaskInboxSnapshot>.Ok(new OgTaskInboxSnapshot(
                personaCode,
                tab,
                [],
                new OgTaskCounts(1, 3, 0, 7, 0, 0),
                asOf)));

        public long BeginLoad() => 1;

        public bool IsCurrent(long version) => version == 1;
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
