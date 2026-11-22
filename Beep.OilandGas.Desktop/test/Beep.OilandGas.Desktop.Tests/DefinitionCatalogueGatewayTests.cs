using System.Text.Json;
using Beep.OilandGas.Models.Processes;
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
/// WFL-02 against a real SQLite database: the steps are ordered with their roles (AC-1, SG-14-1); a cycle in the steps and one
/// between definitions are both displayed with their paths (AC-2); a version's difference comes from its snapshots (AC-3); the
/// catalogue writes nothing (AC-4); its instance counts equal WFL-01's for the same field (AC-5); and the graph's nodes and edges
/// are exactly the step list's (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class DefinitionCatalogueGatewayTests
{
    private const string ConnectionName = "wfl02-definitions";

    [IntegrationFact]
    public async Task The_catalogue_lists_the_steps_versions_cycles_and_the_counts_wfl01_reports()
    {        var root = Path.Combine(OgPaths.DataRoot, "wfl02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWfl02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "definitions.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "definitions.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgDefinitionCatalogueService(() => source, reporter);

        var loaded = await service.LoadAsync("F-1", null);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var catalogue = loaded.Value!;

        // ---- AC-1 and SG-14-1: the list shows version and step count, and the steps are ordered with their roles ----
        Assert.Equal(3, catalogue.Definitions.Count);

        var drilling = catalogue.Definitions.Single(definition => definition.Id == "PD-1");

        Assert.Equal("Drilling approval", drilling.Name);
        Assert.Equal("APPROVAL", drilling.Category);
        Assert.Equal("2.1", drilling.Version);
        // The live configuration is the four-step version; version 1 is the three-step snapshot the difference is measured against.
        Assert.Equal(4, drilling.Steps.Count);
        Assert.Contains("4 step(s)", drilling.Describe(), StringComparison.Ordinal);

        // The list is ordered by name, so the first definition is not the drilling one: the detail is opened by identity.
        Assert.Equal("PD-2", catalogue.Selected!.Id);

        var detail = (await service.LoadAsync("F-1", "PD-1")).Value!;
        var selected = detail.Selected!;

        Assert.Equal("PD-1", selected.Id);
        Assert.Equal(["S-1", "S-2", "S-3", "S-4"], selected.Steps.Select(step => step.Key));
        Assert.Equal("DrillingEngineer", selected.Steps[0].RequiresRole);
        Assert.Equal("PeerReviewer, TeamLead", selected.Steps[1].RequiresRole);
        // A step that records no type and no approval has one way out; the approval step lists its decision and its next step.
        Assert.Equal(["NEXT:S-2"], selected.Steps[0].Decisions);
        Assert.Equal(["APPROVAL", "APPROVE", "RETURN", "NEXT:S-3"], selected.Steps[1].Decisions);
        Assert.Equal(72, selected.Steps[1].SlaHours);
        Assert.True(selected.Steps[1].RequiresApproval);

        // ---- AC-2: both cycles are shown, and neither is hidden ----
        var stepCycle = detail.Cycles.Single(cycle => cycle.Kind == "step");

        Assert.Equal(["S-3", "S-4", "S-3"], stepCycle.Path);
        Assert.Contains("S-3 → S-4 → S-3", detail.CyclesText, StringComparison.Ordinal);

        // The dependency cycle is reported for the definitions it involves, and not for one that records no dependency.
        Assert.Equal(["PD-2", "PD-3", "PD-2"],
            catalogue.Cycles.Single(cycle => cycle.Kind == "dependency").Path);
        Assert.Contains(catalogue.Dependencies, dependency =>
            dependency.DependentDefinitionId == "PD-2" && dependency.PrerequisiteDefinitionId == "PD-3");
        Assert.Empty(detail.Dependencies);
        Assert.DoesNotContain(detail.Cycles, cycle => cycle.Kind == "dependency");

        // ---- AC-3: version differences come from the snapshots ----
        Assert.Equal(2, detail.Versions.Count);

        var second = detail.Versions.Single(version => version.Number == "2");

        Assert.Equal("Added the approval step", second.Change);
        Assert.True(second.HasSnapshot);
        Assert.Contains("S-4", second.AddedSteps);
        Assert.Empty(second.RemovedSteps);
        Assert.Contains("superseded", second.Describe(), StringComparison.Ordinal);
        Assert.Contains("added S-4", detail.VersionsText, StringComparison.Ordinal);

        // ---- AC-4: nothing was written ----
        Assert.Equal(3, Count(database, "PROCESS_DEFINITION"));
        Assert.Equal(2, Count(database, "WORKFLOW_VERSION"));

        // ---- AC-5: the instance counts equal WFL-01's for the same field ----
        var dashboard = new OgProcessDashboardService(() => source, reporter);

        var metrics = await dashboard.LoadAsync("F-1", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        Assert.True(metrics.Success, metrics.Message);

        foreach (var definition in metrics.Value!.Definitions)
        {
            Assert.Equal(definition.Instances,
                catalogue.Definitions.Single(row => row.Id == definition.DefinitionId).Instances);
        }

        Assert.Equal(metrics.Value.InstancesInScope, catalogue.Instances);

        // ---- AC-6: the graph is exactly the step list ----
        Assert.Equal(["S-1", "S-2", "S-3", "S-4"], selected.GraphNodes);
        Assert.Equal(4, selected.GraphEdges.Count);
        Assert.Contains("S-3 → S-4", selected.GraphText, StringComparison.Ordinal);
        Assert.Contains("S-4 → S-3", selected.GraphText, StringComparison.Ordinal);

        // ---- a definition that records no configuration is invalid with its reason, not dropped ----
        var broken = catalogue.Definitions.Single(definition => definition.Id == "PD-4");

        Assert.False(broken.IsValid);
        Assert.Contains("invalid", broken.StateText, StringComparison.Ordinal);
        Assert.Contains("no configuration", broken.InvalidBecause!, StringComparison.Ordinal);
        Assert.Equal(1, catalogue.Invalid);

        // ---- opening a definition by identity reads its own versions ----
        var other = await service.LoadAsync("F-1", "PD-2");

        Assert.True(other.Success, other.Message);
        Assert.Equal("PD-2", other.Value!.Selected!.Id);
        Assert.Empty(other.Value.Versions);

        // ---- a failed read is a failure, not an empty catalogue ----
        Execute(database, "DROP TABLE WORKFLOW_VERSION;");

        var failed = await service.LoadAsync("F-1", null);

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database)
    {
        var prior = JsonSerializer.Serialize(new ProcessDefinition
        {
            Steps =
            [
                new ProcessStepDefinition { StepId = "S-1", StepName = "Submit", SequenceNumber = 1, NextStepId = "S-2", RequiredRoles = ["DrillingEngineer"] },
                new ProcessStepDefinition { StepId = "S-2", StepName = "Peer review", SequenceNumber = 2, NextStepId = "S-3", RequiredRoles = ["PeerReviewer", "TeamLead"], RequiresApproval = true, SlaHours = 72, StepType = "APPROVAL" },
                new ProcessStepDefinition { StepId = "S-3", StepName = "Technical review", SequenceNumber = 3, NextStepId = "" },
            ],
        });

        var current = JsonSerializer.Serialize(new ProcessDefinition
        {
            Steps =
            [
                new ProcessStepDefinition { StepId = "S-1", StepName = "Submit", SequenceNumber = 1, NextStepId = "S-2", RequiredRoles = ["DrillingEngineer"] },
                new ProcessStepDefinition { StepId = "S-2", StepName = "Peer review", SequenceNumber = 2, NextStepId = "S-3", RequiredRoles = ["PeerReviewer", "TeamLead"], RequiresApproval = true, SlaHours = 72, StepType = "APPROVAL" },
                new ProcessStepDefinition { StepId = "S-3", StepName = "Technical review", SequenceNumber = 3, NextStepId = "S-4" },
                new ProcessStepDefinition { StepId = "S-4", StepName = "Approve", SequenceNumber = 4, NextStepId = "S-3" },
            ],
        });

        Execute(database, $"""
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

            INSERT INTO PROCESS_DEFINITION (PROCESS_DEFINITION_ID, PROCESS_NAME, PROCESS_TYPE, ENTITY_TYPE, VERSION,
                IS_ACTIVE, PROCESS_CONFIG_JSON) VALUES
                ('PD-1', 'Drilling approval', 'APPROVAL', 'WELL', '2.1', 'Y', '{current.Replace("'", "''")}'),
                ('PD-2', 'Completion sign-off', 'APPROVAL', 'FIELD', '1.0', 'Y', '{prior.Replace("'", "''")}'),
                ('PD-4', 'Orphan definition', 'APPROVAL', 'WELL', '1.0', 'Y', NULL);

            INSERT INTO WORKFLOW_VERSION (VERSION_ID, PROCESS_DEFINITION_ID, VERSION_NUMBER, CHANGE_DESCRIPTION,
                PREVIOUS_VERSION_ID, PROCESS_CONFIG_SNAPSHOT, CREATED_BY) VALUES
                ('V-1', 'PD-1', '1', 'First definition', NULL, '{prior.Replace("'", "''")}', 'sara'),
                ('V-2', 'PD-1', '2', 'Added the approval step', 'V-1', '{current.Replace("'", "''")}', 'nadia');

            INSERT INTO WORKFLOW_DEPENDENCY (DEPENDENCY_ID, DEPENDENT_PROCESS_DEF_ID, PREREQUISITE_PROCESS_DEF_ID,
                DEPENDENCY_TYPE, CONDITION_EXPRESSION) VALUES
                ('D-1', 'PD-2', 'PD-3', 'REQUIRES', NULL),
                ('D-2', 'PD-3', 'PD-2', 'REQUIRES', NULL);

            INSERT INTO PROCESS_INSTANCE (PROCESS_INSTANCE_ID, PROCESS_DEFINITION_ID, ENTITY_TYPE, ENTITY_ID, FIELD_ID,
                STATUS, STARTED_DATE) VALUES
                ('PI-1', 'PD-1', 'WELL', 'W-1', 'F-1', 'IN_PROGRESS', '2026-11-01 08:00:00'),
                ('PI-2', 'PD-1', 'WELL', 'W-2', 'F-1', 'IN_PROGRESS', '2026-11-02 08:00:00'),
                ('PI-3', 'PD-2', 'FIELD', 'F-1', 'F-2', 'IN_PROGRESS', '2026-11-03 08:00:00');
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

    private static long Count(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM " + table + ";";

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
