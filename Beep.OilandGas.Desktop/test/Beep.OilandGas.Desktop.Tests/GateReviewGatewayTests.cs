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
/// WFL-04 against a real SQLite database: approve is blocked while another required approval is pending and while a blocking
/// segregation conflict exists (AC-1, SG-14-3); a reject without a rationale is refused (AC-2, SG-14-4); a delegated gate is
/// decided by its delegate and both names are recorded (AC-3, SG-14-7); a second decision is refused (AC-4); conditions reach the
/// subject and its history (AC-5); and the author cannot decide their own subject (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class GateReviewGatewayTests
{
    private const string ConnectionName = "wfl04-gates";

    [IntegrationFact]
    public async Task A_gate_is_decided_once_with_its_conditions_and_both_delegation_names()
    {        var root = Path.Combine(OgPaths.DataRoot, "wfl04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWfl04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "gates.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "gates.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgGateReviewsService(() => source, reporter);

        // ---- the queue ----
        var queue = await service.LoadAsync("F-1", null, "nadia");

        Assert.True(queue.Success, queue.Message + " :: " + string.Join(" | ", reporter.Failures));

        var gate = queue.Value!.Selected!;

        Assert.Equal("PA-1", gate.ApprovalId);
        Assert.Equal("GATE", gate.Gate);
        Assert.Equal("WELL W-1", gate.SubjectText);
        Assert.Equal("Peer review", gate.StepName);
        Assert.Equal("omar", gate.RequestedBy);
        Assert.Equal("sara", gate.AuthorOfSubject);

        // ---- AC-1: another required approval is pending, so approve is not offered ----
        Assert.False(gate.ApproveAllowed);
        Assert.Contains("TECHNICAL #2 is not satisfied: it is pending", gate.BlocksText, StringComparison.Ordinal);

        var blocked = await service.DecideAsync("PA-1", GateRules.Approve, "Looks fine", " ", "nadia");

        Assert.False(blocked.Success);
        Assert.Equal(GateRules.CodeChecklistIncomplete, blocked.Reference);
        Assert.Equal(0, Count(database, "PROCESS_APPROVAL WHERE PROCESS_APPROVAL_ID = 'PA-1' AND APPROVAL_STATUS IS NOT NULL"));

        // Once the other approval is recorded, the checklist clears.
        Execute(database, """
            UPDATE PROCESS_APPROVAL SET APPROVAL_STATUS = 'APPROVED', APPROVED_BY = 'omar'
            WHERE PROCESS_APPROVAL_ID = 'PA-2';
            """);

        var cleared = await service.LoadAsync("F-1", "PA-1", "nadia");

        Assert.True(cleared.Success, cleared.Message);
        Assert.True(cleared.Value!.Selected!.ApproveAllowed, cleared.Value.Selected.BlocksText);

        // ---- SG-14-3: a blocking conflict for the actor disables approve and names the rule ----
        Execute(database, """
            INSERT INTO SOD_CONFLICT (SOD_CONFLICT_ID, SOD_RULE_ID, RULE_NAME, USER_ID, ROLE_A, ROLE_B, DETECTED_DATE,
                CONFLICT_STATUS) VALUES
                ('C-1', 'R-1', 'Creator cannot approve', 'nadia', 'ReservoirEngineer', 'Approver', '2026-11-01', 'OPEN');
            """);

        var conflicted = await service.LoadAsync("F-1", "PA-1", "nadia");

        Assert.True(conflicted.Success, conflicted.Message);
        Assert.False(conflicted.Value!.Selected!.ApproveAllowed);
        Assert.Contains("segregation-of-duties conflict", conflicted.Value.Selected.BlocksText, StringComparison.Ordinal);

        var sodRefused = await service.DecideAsync("PA-1", GateRules.Approve, "Fine", " ", "nadia");

        Assert.False(sodRefused.Success);
        Assert.Equal(GateRules.CodeSodConflict, sodRefused.Reference);

        // A non-blocking rule does not disable it, and another person is unaffected either way.
        Execute(database, "UPDATE SOD_RULE SET IS_BLOCKING = 'N' WHERE SOD_RULE_ID = 'R-1';");

        var nonBlocking = await service.LoadAsync("F-1", "PA-1", "nadia");

        Assert.True(nonBlocking.Success, nonBlocking.Message);
        Assert.True(nonBlocking.Value!.Selected!.ApproveAllowed);

        Execute(database, "UPDATE SOD_RULE SET IS_BLOCKING = 'Y' WHERE SOD_RULE_ID = 'R-1';");

        // ---- AC-6: the author cannot decide their own subject ----
        var author = await service.DecideAsync("PA-1", GateRules.Approve, "Fine", " ", "sara");

        Assert.False(author.Success);
        Assert.Equal(GateRules.CodeAuthorCannotApprove, author.Reference);

        // ---- AC-2 and SG-14-4: a reject needs its rationale ----
        var noRationale = await service.DecideAsync("PA-1", GateRules.Reject, "   ", " ", "nadia");

        Assert.False(noRationale.Success);
        Assert.Equal(GateRules.CodeRationaleRequired, noRationale.Reference);

        // ---- the conflict is resolved, so only the delegation stands between the delegate and the decision ----
        Execute(database, """
            UPDATE SOD_CONFLICT SET CONFLICT_STATUS = 'RESOLVED', RESOLVED_DATE = '2026-11-03', RESOLVED_BY = 'sara',
                RESOLUTION_NOTES = 'Compensating control: the peer approved instead.'
            WHERE SOD_CONFLICT_ID = 'C-1';
            """);

        var resolved = await service.LoadAsync("F-1", "PA-1", "nadia");

        Assert.True(resolved.Success, resolved.Message);
        Assert.True(resolved.Value!.Selected!.ApproveAllowed, resolved.Value.Selected.BlocksText);

        // ---- AC-3 and SG-14-7: make the gate delegated, and only the delegate may act ----
        Execute(database, """
            UPDATE PROCESS_APPROVAL SET DELEGATED_FROM = 'sara', DELEGATED_TO = 'nadia', DELEGATION_REASON = 'Annual leave'
            WHERE PROCESS_APPROVAL_ID = 'PA-1';
            """);

        var delegated = await service.LoadAsync("F-1", "PA-1", "nadia");

        Assert.True(delegated.Success, delegated.Message);
        Assert.True(delegated.Value!.Selected!.IsDelegated);
        Assert.Contains("Acting for sara", delegated.Value.Selected.DelegationText, StringComparison.Ordinal);
        Assert.Contains("nadia", delegated.Value.Selected.DelegationText, StringComparison.Ordinal);

        // Somebody who is neither the requester nor the author, but is still not the delegate.
        var wrongDelegate = await service.DecideAsync("PA-1", GateRules.Approve, "Fine", " ", "huda");

        Assert.False(wrongDelegate.Success);
        Assert.Equal(GateRules.CodeDelegatedToAnother, wrongDelegate.Reference);
        Assert.Contains("delegated to nadia", wrongDelegate.Message!, StringComparison.Ordinal);

        // ---- AC-5: an approval with conditions records them and propagates them to the subject ----
        var approved = await service.DecideAsync(
            "PA-1", GateRules.ApproveWithConditions, "Approved for the 2026 programme.",
            "Monthly pressure test; no flaring", "nadia");

        Assert.True(approved.Success, approved.Message + " :: " + string.Join(" | ", reporter.Failures));

        var decided = approved.Value!.Selected!;

        Assert.Equal("APPROVED", decided.DecisionState);
        Assert.False(decided.ApproveAllowed);
        Assert.Contains("read-only", decided.DecisionText, StringComparison.Ordinal);

        Assert.Equal(1, Count(database,
            "PROCESS_APPROVAL WHERE PROCESS_APPROVAL_ID = 'PA-1' AND APPROVAL_STATUS = 'APPROVED' AND APPROVED_BY = 'nadia' AND DELEGATED_BY = 'sara'"));
        Assert.Equal(1, Count(database,
            "PROCESS_APPROVAL WHERE PROCESS_APPROVAL_ID = 'PA-1' AND APPROVAL_NOTES LIKE '%conditions: Monthly pressure test; no flaring%'"));

        // The conditions reached the instance's own data, under the named approval.
        Assert.Equal(1, Count(database,
            "PROCESS_INSTANCE WHERE PROCESS_INSTANCE_ID = 'PI-1' AND INSTANCE_DATA_JSON LIKE '%Monthly pressure test; no flaring%'"));

        // And the decision is on the instance's history.
        Assert.Equal(1, Count(database,
            "PROCESS_HISTORY WHERE PROCESS_INSTANCE_ID = 'PI-1' AND EVENT_TYPE = 'GATE_APPROVE_WITH_CONDITIONS' AND USER_ID = 'nadia'"));

        // The prior-decisions list carries it, with the delegator.
        Assert.Contains(decided.PriorDecisions, decision =>
            decision.Actor == "nadia" && decision.ActingFor == "sara");

        // ---- AC-4: a second decision is refused and the original is retained ----
        var second = await service.DecideAsync("PA-1", GateRules.Reject, "Changed my mind", " ", "nadia");

        Assert.False(second.Success);
        Assert.Equal(GateRules.CodeAlreadyDecided, second.Reference);
        Assert.Equal(1, Count(database,
            "PROCESS_APPROVAL WHERE PROCESS_APPROVAL_ID = 'PA-1' AND APPROVAL_STATUS = 'APPROVED'"));
        Assert.Equal(1, Count(database,
            "PROCESS_HISTORY WHERE PROCESS_INSTANCE_ID = 'PI-1' AND EVENT_TYPE LIKE 'GATE_%'"));

        // ---- a failed read is a failure, not an empty queue ----
        Execute(database, "DROP TABLE SOD_CONFLICT;");

        var failed = await service.LoadAsync("F-1", null, "nadia");

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE PROCESS_APPROVAL (
                PROCESS_APPROVAL_ID TEXT,
                PROCESS_INSTANCE_ID TEXT,
                PROCESS_STEP_INSTANCE_ID TEXT,
                APPROVAL_TYPE TEXT,
                APPROVAL_STATUS TEXT,
                APPROVER_USER_ID TEXT,
                APPROVER_SEQUENCE INTEGER,
                REQUIRED_ACTION TEXT,
                ESCALATION_AFTER_HOURS INTEGER,
                APPROVAL_DATE TEXT,
                APPROVED_BY TEXT,
                DELEGATED_BY TEXT,
                APPROVAL_NOTES TEXT,
                DELEGATED_TO TEXT,
                DELEGATED_FROM TEXT,
                DELEGATION_REASON TEXT,
                DELEGATION_DATE TEXT,
                ESCALATED_TO TEXT,
                ESCALATION_DATE TEXT,
                ESCALATION_REASON TEXT,
                REQUESTED_DATE TEXT,
                REQUESTED_BY TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PROCESS_APPROVAL_ID));

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

            CREATE TABLE SOD_RULE (
                SOD_RULE_ID TEXT,
                RULE_NAME TEXT,
                RULE_CATEGORY TEXT,
                CONFLICTING_PERMISSION_A TEXT,
                CONFLICTING_PERMISSION_B TEXT,
                CONFLICT_DESCRIPTION TEXT,
                SEVERITY TEXT,
                REGULATION_REFERENCE TEXT,
                IS_BLOCKING TEXT,
                SCOPE_TYPE TEXT,
                MITIGATION_GUIDANCE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (SOD_RULE_ID));

            CREATE TABLE SOD_CONFLICT (
                SOD_CONFLICT_ID TEXT,
                SOD_RULE_ID TEXT,
                RULE_NAME TEXT,
                USER_ID TEXT,
                ROLE_A TEXT,
                ROLE_B TEXT,
                DETECTED_DATE TEXT,
                CONFLICT_STATUS TEXT,
                COMPENSATING_CONTROL_ID TEXT,
                RESOLVED_DATE TEXT,
                RESOLVED_BY TEXT,
                RESOLUTION_NOTES TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (SOD_CONFLICT_ID));

            INSERT INTO PROCESS_DEFINITION (PROCESS_DEFINITION_ID, PROCESS_NAME, PROCESS_TYPE, VERSION, IS_ACTIVE) VALUES
                ('PD-1', 'Drilling approval', 'APPROVAL', '2.1', 'Y');

            INSERT INTO PROCESS_INSTANCE (PROCESS_INSTANCE_ID, PROCESS_DEFINITION_ID, ENTITY_TYPE, ENTITY_ID, FIELD_ID,
                CURRENT_STATE, STATUS, STARTED_DATE, ROW_CREATED_BY) VALUES
                ('PI-1', 'PD-1', 'WELL', 'W-1', 'F-1', 'PEER_REVIEW', 'IN_PROGRESS', '2026-11-01 08:00:00', 'sara');

            INSERT INTO PROCESS_STEP_INSTANCE (PROCESS_STEP_INSTANCE_ID, PROCESS_INSTANCE_ID, STEP_ID, STEP_NAME,
                STEP_SEQUENCE, STATUS, ASSIGNED_TO, STARTED_DATE, SLA_HOURS) VALUES
                ('PS-1', 'PI-1', 'S-2', 'Peer review', 2, 'IN_PROGRESS', 'nadia', '2026-11-02 08:00:00', 72);

            INSERT INTO PROCESS_APPROVAL (PROCESS_APPROVAL_ID, PROCESS_INSTANCE_ID, PROCESS_STEP_INSTANCE_ID,
                APPROVAL_TYPE, APPROVAL_STATUS, APPROVER_USER_ID, APPROVER_SEQUENCE, REQUIRED_ACTION,
                REQUESTED_BY, REQUESTED_DATE) VALUES
                ('PA-1', 'PI-1', 'PS-1', 'GATE', NULL, 'nadia', 1, 'GATE', 'omar', '2026-11-02 08:00:00'),
                ('PA-2', 'PI-1', 'PS-1', 'TECHNICAL', 'PENDING', 'omar', 2, 'TECHNICAL', 'omar', '2026-11-02 08:00:00');

            INSERT INTO SOD_RULE (SOD_RULE_ID, RULE_NAME, RULE_CATEGORY, CONFLICTING_PERMISSION_A,
                CONFLICTING_PERMISSION_B, CONFLICT_DESCRIPTION, SEVERITY, IS_BLOCKING, SCOPE_TYPE) VALUES
                ('R-1', 'Creator cannot approve', 'APPROVAL', 'reserves.create', 'reserves.approve',
                 'Preparing and approving the same reserves', 'CRITICAL', 'Y', 'PROCESS');
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
