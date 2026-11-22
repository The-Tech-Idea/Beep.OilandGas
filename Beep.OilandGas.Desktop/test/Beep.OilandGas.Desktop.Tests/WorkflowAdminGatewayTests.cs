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
/// WFL-06 against a real SQLite database: a dependency that closes a cycle is refused before saving (AC-1); every change records
/// its actor, reason and previous value (AC-2); a delegation outside its window is inactive (AC-3); the segregation tester names
/// the rule (AC-4); and an orphaned instance is listed with its cause (AC-5). Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WorkflowAdminGatewayTests
{
    private const string ConnectionName = "wfl06-admin";

    [IntegrationFact]
    public async Task The_configuration_can_be_changed_with_its_cycle_reason_and_history()
    {        var root = Path.Combine(OgPaths.DataRoot, "wfl06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWfl06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "admin.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "admin.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWorkflowAdminService(() => source, reporter);

        // ---- the read ----
        var loaded = await service.LoadAsync(DateTime.UtcNow);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;

        Assert.Equal(2, workspace.Versions.Count);
        Assert.Single(workspace.Dependencies);
        Assert.Single(workspace.Triggers);
        Assert.Equal(2, workspace.SodRules.Count);

        // AC-3: the delegation whose window has closed is inactive and marked; the open one is active.
        var closed = workspace.Delegations.Single(row => row.Id == "DOA-1");

        Assert.False(closed.Active);
        Assert.Contains("outside its window", closed.StateText, StringComparison.Ordinal);

        var open = workspace.Delegations.Single(row => row.Id == "DOA-2");

        Assert.True(open.Active);

        // AC-5: the orphaned instance is listed with its cause, and the normal one is not.
        var orphan = Assert.Single(workspace.Diagnostics, finding => finding.Kind == "orphaned instance");

        Assert.Equal("PI-ORPHAN", orphan.TargetId);
        Assert.Contains("PD-GONE", orphan.Cause, StringComparison.Ordinal);
        Assert.Contains("✗ orphaned instance", workspace.DiagnosticsText, StringComparison.Ordinal);

        // ---- AC-4: the tester names the blocking rule and ignores the advisory one ----
        var failed = service.TestSod(workspace, "reserves.create", "reserves.approve");

        Assert.True(failed.Failed);
        Assert.Contains("Creator cannot approve", failed.Sentence, StringComparison.Ordinal);

        Assert.False(service.TestSod(workspace, "reserves.view", "reserves.approve").Failed);

        // ---- AC-1: a dependency that closes a cycle is refused, and nothing is written ----
        // The recorded dependency is PD-1 requiring PD-2, so PD-2 requiring PD-1 closes the loop.
        var cycle = await service.AddDependencyAsync(
            "PD-2", "PD-1", "REQUIRES", null, "Make the pairing mutual", "PD-1, PD-2", "sara");

        Assert.False(cycle.Success);
        Assert.Equal(WorkflowAdminRules.CodeDependencyCycle, cycle.Reference);
        Assert.Contains("would close a cycle", cycle.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "WORKFLOW_DEPENDENCY"));
        Assert.Equal(0, Count(database, "WORKFLOW_CONFIGURATION_CHANGE"));

        // ---- AC-2: a change without a reason or confirmation is refused ----
        var noReason = await service.AddDependencyAsync(
            "PD-3", "PD-9", "REQUIRES", null, "  ", "PD-3", "sara");

        Assert.False(noReason.Success);
        Assert.Equal(WorkflowAdminRules.CodeReasonRequired, noReason.Reference);

        var noConfirmation = await service.AddDependencyAsync(
            "PD-3", "PD-9", "REQUIRES", null, "Wire the third stage", " ", "sara");

        Assert.False(noConfirmation.Success);
        Assert.Equal(WorkflowAdminRules.CodeReasonRequired, noConfirmation.Reference);
        Assert.Equal(1, Count(database, "WORKFLOW_DEPENDENCY"));

        // ---- AC-2: a valid change is written with its actor, reason and previous value ----
        var added = await service.AddDependencyAsync(
            "PD-3", "PD-9", "REQUIRES", null, "Wire the third stage", "PD-3, PD-9", "sara");

        Assert.True(added.Success, added.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(2, added.Value!.Dependencies.Count);

        Assert.Equal(1, Count(database,
            "WORKFLOW_CONFIGURATION_CHANGE WHERE AREA = 'DEPENDENCY' AND TARGET_ID = 'PD-3' AND ACTOR = 'sara' AND REASON = 'Wire the third stage'"));

        // A trigger is disabled with its previous value recorded.
        var disabled = await service.SetTriggerActiveAsync(
            "TR-1", false, "The event is retired this quarter", "PD-1", "nadia");

        Assert.True(disabled.Success, disabled.Message);
        Assert.False(disabled.Value!.Triggers.Single().Active);
        Assert.Equal(1, Count(database,
            "WORKFLOW_CONFIGURATION_CHANGE WHERE AREA = 'TRIGGER' AND PREVIOUS_VALUE LIKE '%\"active\":true%' AND NEW_VALUE LIKE '%\"active\":false%'"));

        // Disabling it again is refused rather than recorded twice.
        var again = await service.SetTriggerActiveAsync(
            "TR-1", false, "Same", "PD-1", "nadia");

        Assert.False(again.Success);
        Assert.Equal(WorkflowAdminRules.CodeAlreadyInThatState, again.Reference);

        // A segregation rule is made advisory, and the tester then passes on the same pair.
        var advisory = await service.SetSodRuleBlockingAsync(
            "R-1", false, "Compensating control approved", "PD-1", "nadia");

        Assert.True(advisory.Success, advisory.Message);
        Assert.False(service.TestSod(advisory.Value!, "reserves.create", "reserves.approve").Failed);
        Assert.Equal(1, Count(database,
            "SOD_RULE WHERE SOD_RULE_ID = 'R-1' AND IS_BLOCKING = 'N'"));
        Assert.Equal(1, Count(database,
            "WORKFLOW_CONFIGURATION_CHANGE WHERE AREA = 'SOD_RULE' AND PREVIOUS_VALUE LIKE '%\"blocking\":true%'"));

        // ---- a version is activated from its snapshot, and what it replaced is recorded ----
        var activated = await service.ActivateVersionAsync(
            "PD-1", "V-2", "Roll out the reviewed version", "PD-1", "sara");

        Assert.True(activated.Success, activated.Message + " :: " + string.Join(" | ", reporter.Failures));

        Assert.Equal("2", activated.Value!.Versions.Single(row => row.VersionId == "V-2").Number);
        Assert.True(activated.Value.Versions.Single(row => row.VersionId == "V-2").IsLive);
        Assert.Equal(1, Count(database,
            "PROCESS_DEFINITION WHERE PROCESS_DEFINITION_ID = 'PD-1' AND VERSION = '2' AND PROCESS_CONFIG_JSON LIKE '%S-2%'"));
        Assert.Equal(1, Count(database,
            "WORKFLOW_CONFIGURATION_CHANGE WHERE AREA = 'VERSION' AND TARGET_ID = 'PD-1' AND PREVIOUS_VALUE LIKE '%\"version\":\"1\"%'"));

        // A version with no snapshot cannot be activated.
        Execute(database, "INSERT INTO WORKFLOW_VERSION (VERSION_ID, PROCESS_DEFINITION_ID, VERSION_NUMBER, CHANGE_DESCRIPTION) VALUES ('V-3', 'PD-1', '3', 'Bare');");

        var bare = await service.ActivateVersionAsync("PD-1", "V-3", "Try the bare one", "PD-1", "sara");

        Assert.False(bare.Success);
        Assert.Equal(WorkflowAdminRules.CodeNoSnapshot, bare.Reference);

        // ---- a delegation is added with its window, and the closed one stays inactive ----
        var delegation = await service.AddDelegationAsync(
            "sara", "huda", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10), 25_000m, "USD",
            "Cover during leave", "sara, huda", "nadia");

        Assert.True(delegation.Success, delegation.Message + " :: " + string.Join(" | ", reporter.Failures));

        var addedDelegation = delegation.Value!.Delegations.Single(row => row.Delegate == "huda");

        Assert.True(addedDelegation.Active);
        Assert.Contains("inside its window", addedDelegation.StateText, StringComparison.Ordinal);
        // Only the closed 2025 delegation is inactive; the current one and the new one are in force.
        Assert.Equal(1, delegation.Value.InactiveDelegations);

        Assert.Equal(1, Count(database,
            "DELEGATION_OF_AUTHORITY WHERE DELEGATOR_USER_ID = 'sara' AND DELEGATE_USER_ID = 'huda' AND IS_ACTIVE = 'Y'"));

        // A delegation naming the same person twice is refused.
        var self = await service.AddDelegationAsync(
            "sara", "sara", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 0m, "USD", "why", "sara", "nadia");

        Assert.False(self.Success);
        Assert.Equal(WorkflowAdminRules.CodeDelegationInvalid, self.Reference);

        // ---- the recorded changes are readable, newest first, with their previous values ----
        Assert.Equal(5, delegation.Value.Changes.Count);
        Assert.All(delegation.Value.Changes, change => Assert.False(string.IsNullOrWhiteSpace(change.Actor)));
        Assert.All(delegation.Value.Changes, change => Assert.False(string.IsNullOrWhiteSpace(change.Reason)));
        Assert.Contains("dependency", delegation.Value.ChangesText, StringComparison.OrdinalIgnoreCase);

        // ---- a failed read is a failure, not an empty workspace ----
        Execute(database, "DROP TABLE WORKFLOW_CONFIGURATION_CHANGE;");

        var failedRead = await service.LoadAsync(DateTime.UtcNow);

        Assert.False(failedRead.Success);
        Assert.Null(failedRead.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE WORKFLOW_VERSION (
                VERSION_ID TEXT,
                PROCESS_DEFINITION_ID TEXT,
                VERSION_NUMBER TEXT,
                CHANGE_DESCRIPTION TEXT,
                PREVIOUS_VERSION_ID TEXT,
                PROCESS_CONFIG_SNAPSHOT TEXT,
                DEPRECATED_STEP_IDS TEXT,
                STEP_REMAPPING_JSON TEXT,
                CREATED_BY TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (VERSION_ID));

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

            CREATE TABLE WORKFLOW_DEPENDENCY (
                DEPENDENCY_ID TEXT,
                DEPENDENT_PROCESS_DEF_ID TEXT,
                DEPENDENT_STEP_ID TEXT,
                PREREQUISITE_PROCESS_DEF_ID TEXT,
                PREREQUISITE_STEP_ID TEXT,
                DEPENDENCY_TYPE TEXT,
                CONDITION_EXPRESSION TEXT,
                DESCRIPTION TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (DEPENDENCY_ID));

            CREATE TABLE BUSINESS_EVENT_TRIGGER (
                TRIGGER_ID TEXT,
                TRIGGER_NAME TEXT,
                EVENT_TYPE TEXT,
                ENTITY_TYPE TEXT,
                WATCH_FIELD TEXT,
                CONDITION_EXPRESSION TEXT,
                TARGET_PROCESS_DEF_ID TEXT,
                IS_ACTIVE TEXT,
                PRIORITY INTEGER,
                DESCRIPTION TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (TRIGGER_ID));

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

            CREATE TABLE DELEGATION_OF_AUTHORITY (
                DELEGATOR_USER_ID TEXT,
                DELEGATE_USER_ID TEXT,
                VALID_FROM_UTC TEXT,
                VALID_TO_UTC TEXT,
                IS_ACTIVE TEXT,
                DOA_ID TEXT,
                DOA_NAME TEXT,
                ENTITY_TYPE TEXT,
                FIELD_NAME TEXT,
                COMPARISON_OPERATOR TEXT,
                THRESHOLD_VALUE REAL,
                THRESHOLD_VALUE_MAX REAL,
                CURRENCY_CODE TEXT,
                APPROVAL_LEVEL TEXT,
                REQUIRED_ROLE TEXT,
                APPROVAL_SEQUENCE INTEGER,
                REQUIRES_UNANIMOUS TEXT,
                ESCALATION_ROLE TEXT,
                ESCALATION_HOURS INTEGER,
                PROCESS_TYPE TEXT,
                NOTES TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (DOA_ID));

            CREATE TABLE COMPENSATING_CONTROL (
                CONTROL_ID TEXT,
                SOD_CONFLICT_ID TEXT,
                USER_ID TEXT,
                CONTROL_TYPE TEXT,
                CONTROL_DESCRIPTION TEXT,
                APPROVED_BY TEXT,
                APPROVED_DATE TEXT,
                EFFECTIVE_FROM TEXT,
                EFFECTIVE_TO TEXT,
                REVIEW_FREQUENCY TEXT,
                LAST_REVIEWED_DATE TEXT,
                LAST_REVIEWED_BY TEXT,
                STATUS TEXT,
                APPROVAL_PROCESS_INSTANCE_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (CONTROL_ID));

            CREATE TABLE WORKFLOW_CONFIGURATION_CHANGE (
                WORKFLOW_CONFIGURATION_CHANGE_ID TEXT,
                AREA TEXT,
                TARGET_ID TEXT,
                TARGET_DETAIL TEXT,
                ACTOR TEXT,
                REASON TEXT,
                PREVIOUS_VALUE TEXT,
                NEW_VALUE TEXT,
                CHANGED_AT_UTC TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (WORKFLOW_CONFIGURATION_CHANGE_ID));

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

            CREATE TABLE ROLE_HANDOFF_CONTRACT (
                HANDOFF_CONTRACT_ID TEXT,
                PROCESS_DEFINITION_ID TEXT,
                FROM_STEP_ID TEXT,
                FROM_ROLE TEXT,
                TO_STEP_ID TEXT,
                TO_ROLE TEXT,
                REQUIRED_DATA_FIELDS_JSON TEXT,
                REQUIRED_DOCUMENTS_JSON TEXT,
                SLA_CONTEXT_JSON TEXT,
                APPROVAL_CONTEXT_JSON TEXT,
                VALIDATION_RULES_JSON TEXT,
                DESCRIPTION TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (HANDOFF_CONTRACT_ID));

            INSERT INTO PROCESS_DEFINITION (PROCESS_DEFINITION_ID, PROCESS_NAME, PROCESS_TYPE, VERSION, IS_ACTIVE,
                PROCESS_CONFIG_JSON) VALUES
                ('PD-1', 'Drilling approval', 'APPROVAL', '1', 'Y', '{"Steps":[{"StepId":"S-1","SequenceNumber":1}]}'),
                ('PD-2', 'Completion sign-off', 'APPROVAL', '1', 'Y', '{"Steps":[{"StepId":"S-1","SequenceNumber":1}]}');

            INSERT INTO WORKFLOW_VERSION (VERSION_ID, PROCESS_DEFINITION_ID, VERSION_NUMBER, CHANGE_DESCRIPTION,
                PREVIOUS_VERSION_ID, PROCESS_CONFIG_SNAPSHOT) VALUES
                ('V-1', 'PD-1', '1', 'First', NULL, '{"Steps":[{"StepId":"S-1","SequenceNumber":1}]}'),
                ('V-2', 'PD-1', '2', 'Added the review step', 'V-1', '{"Steps":[{"StepId":"S-1","SequenceNumber":1},{"StepId":"S-2","SequenceNumber":2}]}');

            INSERT INTO WORKFLOW_DEPENDENCY (DEPENDENCY_ID, DEPENDENT_PROCESS_DEF_ID, PREREQUISITE_PROCESS_DEF_ID,
                DEPENDENCY_TYPE) VALUES
                ('D-1', 'PD-1', 'PD-2', 'REQUIRES');

            INSERT INTO BUSINESS_EVENT_TRIGGER (TRIGGER_ID, TRIGGER_NAME, EVENT_TYPE, ENTITY_TYPE,
                TARGET_PROCESS_DEF_ID, IS_ACTIVE, PRIORITY) VALUES
                ('TR-1', 'Start on well approval', 'WELL_APPROVED', 'WELL', 'PD-1', 'Y', 10);

            INSERT INTO SOD_RULE (SOD_RULE_ID, RULE_NAME, RULE_CATEGORY, CONFLICTING_PERMISSION_A,
                CONFLICTING_PERMISSION_B, CONFLICT_DESCRIPTION, SEVERITY, IS_BLOCKING, SCOPE_TYPE) VALUES
                ('R-1', 'Creator cannot approve', 'APPROVAL', 'reserves.create', 'reserves.approve',
                 'Preparing and approving the same reserves', 'CRITICAL', 'Y', 'PROCESS'),
                ('R-2', 'Advisory pairing', 'APPROVAL', 'reserves.view', 'reserves.approve',
                 'Visible but not blocking', 'HIGH', 'N', 'PROCESS');

            INSERT INTO DELEGATION_OF_AUTHORITY (DOA_ID, DOA_NAME, ENTITY_TYPE, FIELD_NAME, COMPARISON_OPERATOR,
                THRESHOLD_VALUE, APPROVAL_LEVEL, REQUIRED_ROLE, APPROVAL_SEQUENCE, REQUIRES_UNANIMOUS, PROCESS_TYPE,
                DELEGATOR_USER_ID, DELEGATE_USER_ID, VALID_FROM_UTC, VALID_TO_UTC, IS_ACTIVE) VALUES
                ('DOA-1', 'Old cover', 'USER', 'NONE', 'EQ', 1000, 'DELEGATE', 'DELEGATE', 0, 'N', 'ALL',
                 'sara', 'omar', '2025-01-01', '2025-12-31', 'Y'),
                ('DOA-2', 'Current cover', 'USER', 'NONE', 'EQ', 5000, 'DELEGATE', 'DELEGATE', 0, 'N', 'ALL',
                 'sara', 'nadia', '2026-01-01', '2027-12-31', 'Y');

            INSERT INTO PROCESS_INSTANCE (PROCESS_INSTANCE_ID, PROCESS_DEFINITION_ID, ENTITY_TYPE, ENTITY_ID, FIELD_ID,
                STATUS, STARTED_DATE) VALUES
                ('PI-1', 'PD-1', 'WELL', 'W-1', 'F-1', 'IN_PROGRESS', '2026-11-01 08:00:00'),
                ('PI-ORPHAN', 'PD-GONE', 'WELL', 'W-9', 'F-1', 'IN_PROGRESS', '2026-11-02 08:00:00');

            INSERT INTO PROCESS_STEP_INSTANCE (PROCESS_STEP_INSTANCE_ID, PROCESS_INSTANCE_ID, STEP_ID, STEP_NAME,
                STEP_SEQUENCE, STATUS, STARTED_DATE) VALUES
                ('PS-1', 'PI-1', 'S-1', 'Submit', 1, 'IN_PROGRESS', '2026-11-01 08:00:00');
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
