using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-08 against a real SQLite database: a milestone cannot complete without evidence (AC-1); overdue is computed from
/// the planned date (AC-2); a rejected evidence leaves the facility's create/edit controls hidden until it is resolved
/// (AC-3); the handover records recipient, date, conditions and signature (AC-4); recorded equipment never claims
/// installation, testing or acceptance (AC-5); and the facility is revalidated before and after every write (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ConstructionGatewayTests
{
    private const string ConnectionName = "dev08-construction";
    private static readonly DateTime AsOf = new(2026, 10, 1);

    [IntegrationFact]
    public async Task Milestones_complete_on_evidence_and_the_facility_is_handed_over_with_its_signature()
    {        var root = Path.Combine(OgPaths.DataRoot, "dev08-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDev08"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "construction.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "construction.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgConstructionService(() => source, reporter);

        // ---- the facility list -------------------------------------------------
        var loaded = await service.LoadAsync("F-1", "Alpha field", null, AsOf);

        Assert.True(loaded.Success, loaded.Message);

        var workspace = loaded.Value!;

        Assert.Equal(2, workspace.Facilities.Count);                       // the F-2 facility is not offered
        Assert.DoesNotContain(workspace.Facilities, facility => facility.Id == "FI-9");
        Assert.Equal("FI-1", workspace.Selected!.Id);
        Assert.Equal(3, workspace.Selected.PlanVersion);
        Assert.Equal(1, workspace.OverdueFacilities);                      // fabrication is overdue on 2026-10-01

        var facility = workspace.Selected;

        Assert.Equal("Central processing facility", facility.Name);
        Assert.Equal(1, facility.CompletedMilestones);
        Assert.Equal(2, facility.TotalMilestones);
        Assert.Equal("1 of 2 milestone(s) complete", facility.CompletionText);
        Assert.Equal("fabrication", facility.StateText);
        Assert.True(facility.Milestones.Single(m => m.Name == "FABRICATION").IsOverdue(AsOf));
        Assert.Equal(11, facility.Milestones.Single(m => m.Name == "FABRICATION").DaysOverdue(AsOf));

        // Spend vs budget is read from the investment row.
        Assert.Equal("80 USD", facility.BudgetText);
        Assert.Equal("95 USD", facility.SpendText);
        Assert.Contains("15 over budget", facility.VarianceText, StringComparison.Ordinal);

        // AC-5: equipment is shown, with the claim it does not make.
        Assert.Equal(2, facility.Equipment.Count);
        Assert.Contains("2 equipment item(s) recorded", facility.EquipmentText, StringComparison.Ordinal);
        Assert.Contains("not evidence", facility.EquipmentText, StringComparison.Ordinal);
        Assert.Equal(1, facility.Equipment.Count(item => item.Active));

        // DEV-07's accepted completion is this screen's upstream record.
        Assert.Contains("WELL-1 accepted by reviewer", workspace.UpstreamText, StringComparison.Ordinal);
        Assert.Contains("Install the flow meter within 30 days", workspace.UpstreamText, StringComparison.Ordinal);

        // ---- AC-1: no evidence, no completion ---------------------------------
        var fabrication = facility.Milestones.Single(m => m.Name == "FABRICATION");

        Assert.False(fabrication.CanComplete);

        var noEvidence = await service.CompleteMilestoneAsync(fabrication.Id, AsOf, "sara", 3);

        Assert.False(noEvidence.Success);
        Assert.Equal(ConstructionRules.CodeNoEvidence, noEvidence.Reference);

        // ---- AC-3: a rejected submission hides create/edit --------------------
        var rejected = await service.AddEvidenceAsync(
            fabrication.Id,
            new OgMilestoneEvidenceValues("FAT certificate", "fat-fab-2026-09-18.pdf", AsOf.AddDays(-13), "reviewer", "REJECTED"),
            "sara",
            3);

        Assert.True(rejected.Success, rejected.Message);
        Assert.False(rejected.Value!.Selected!.CanEdit);
        Assert.NotNull(rejected.Value.Selected.UnresolvedReason);
        Assert.Contains("FAT certificate", rejected.Value.Selected.UnresolvedReason!, StringComparison.Ordinal);

        var blocked = await service.CompleteMilestoneAsync(fabrication.Id, AsOf, "sara", 3);

        Assert.False(blocked.Success);
        Assert.Equal(ConstructionRules.CodeUnresolved, blocked.Reference);

        // ---- resolving it re-enables editing and permits completion ----------
        var accepted = await service.AddEvidenceAsync(
            fabrication.Id,
            new OgMilestoneEvidenceValues("FAT certificate", "fat-fab-2026-09-28.pdf", AsOf.AddDays(-3), "reviewer", "ACCEPTED_WITH_NOTES"),
            "sara",
            3);

        Assert.True(accepted.Success, accepted.Message);
        Assert.True(accepted.Value!.Selected!.CanEdit);
        Assert.Null(accepted.Value.Selected.UnresolvedReason);

        var completed = await service.CompleteMilestoneAsync(fabrication.Id, AsOf, "sara", 3);

        Assert.True(completed.Success, completed.Message);

        var afterCompletion = completed.Value!.Selected!;

        Assert.True(afterCompletion.Milestones.Single(m => m.Name == "FABRICATION").IsComplete);
        Assert.Equal(0, afterCompletion.OverdueCount(AsOf));                 // AC-2 recomputed, not stored
        Assert.Contains("ready for handover", afterCompletion.StateText, StringComparison.Ordinal);
        Assert.True(afterCompletion.CanHandover);

        // A duplicate milestone is refused.
        var duplicate = await service.CreateMilestoneAsync(
            "FI-1", new OgMilestoneValues("DESIGN", AsOf.AddDays(5)), "sara", 3);

        Assert.False(duplicate.Success);
        Assert.Equal(OgConstructionService.CodeMilestoneExists, duplicate.Reference);

        // A milestone created with a past planned date is overdue immediately (AC-2).
        var commissioning = await service.CreateMilestoneAsync(
            "FI-1", new OgMilestoneValues("COMMISSIONING", AsOf.AddDays(-4)), "sara", 3);

        Assert.True(commissioning.Success, commissioning.Message);

        var newMilestone = commissioning.Value!.Selected!.Milestones.Single(m => m.Name == "COMMISSIONING");

        Assert.True(newMilestone.IsOverdue(AsOf));
        Assert.Equal(4, newMilestone.DaysOverdue(AsOf));
        Assert.False(commissioning.Value.Selected.CanHandover);

        // ---- AC-4: handover needs every milestone and its signature -----------
        var early = await service.HandoverAsync(
            "FI-1",
            new OgFacilityHandoverValues("operations", AsOf, [], "signed-2026-10-01", "reviewer"),
            "sara",
            3);

        Assert.False(early.Success);
        Assert.Equal(ConstructionRules.CodeMilestonesIncomplete, early.Reference);
        Assert.Contains("commissioning", early.Message!, StringComparison.OrdinalIgnoreCase);

        Assert.True((await service.AddEvidenceAsync(
            newMilestone.Id,
            new OgMilestoneEvidenceValues("commissioning report", "comm-FI-1.pdf", AsOf, "reviewer", "ACCEPTED"),
            "sara",
            3)).Success);
        Assert.True((await service.CompleteMilestoneAsync(
            newMilestone.Id, AsOf, "sara", 3)).Success);

        var unsigned = await service.HandoverAsync(
            "FI-1",
            new OgFacilityHandoverValues("operations", AsOf, [], " ", "reviewer"),
            "sara",
            3);

        Assert.False(unsigned.Success);
        Assert.Equal(ConstructionRules.CodeSignatureRequired, unsigned.Reference);

        var handedOver = await service.HandoverAsync(
            "FI-1",
            new OgFacilityHandoverValues(
                "operations",
                AsOf,
                ["Connect the flow line within 14 days", "Hand over the spare parts list"],
                "signed-2026-10-01",
                "reviewer"),
            "sara",
            3);

        Assert.True(handedOver.Success, handedOver.Message);

        var record = handedOver.Value!.Selected!.Handover;

        Assert.NotNull(record);
        Assert.Equal("operations", record!.HandedOverTo);
        Assert.Equal(AsOf, record.HandedOverAt);
        Assert.Equal(2, record.Conditions.Count);
        Assert.Equal("signed-2026-10-01", record.Signature);
        Assert.Equal("reviewer", record.SignedBy);
        Assert.True(record.Accepted);
        Assert.True(handedOver.Value.Selected.IsHandedOver);
        Assert.False(handedOver.Value.Selected.CanEdit);
        Assert.Contains("Connect the flow line within 14 days", record.Describe(), StringComparison.Ordinal);

        var twice = await service.HandoverAsync(
            "FI-1",
            new OgFacilityHandoverValues("operations", AsOf, [], "signed-2026-10-01", "reviewer"),
            "sara",
            3);

        Assert.False(twice.Success);
        Assert.Equal("construction-handover-already-recorded", twice.Reference);

        // ---- AC-6: the facility is revalidated before and after a write ------
        var gasPlant = (await service.LoadAsync("F-1", "Alpha field", "FI-2", AsOf)).Value!.Selected!;

        Assert.Equal(3, gasPlant.PlanVersion);

        Assert.True((await service.CreateMilestoneAsync(
            "FI-2", new OgMilestoneValues("DESIGN", AsOf.AddDays(-10)), "sara", 3)).Success);

        // The plan moves on while the screen still holds version 3.
        BumpPlanVersion(database, "FI-2", 4);

        var moved = await service.CreateMilestoneAsync(
            "FI-2", new OgMilestoneValues("PROCUREMENT", AsOf.AddDays(-5)), "sara", 3);

        Assert.False(moved.Success);
        Assert.Equal(OgConstructionService.CodeFacilityChanged, moved.Reference);
        Assert.Contains("plan version 3", moved.Message!, StringComparison.Ordinal);
        Assert.Contains("to 4", moved.Message!, StringComparison.Ordinal);

        // With the version it actually saw, the same write goes through.
        Assert.True((await service.CreateMilestoneAsync(
            "FI-2", new OgMilestoneValues("PROCUREMENT", AsOf.AddDays(-5)), "sara", 4)).Success);

        // After the write: a facility that changes underneath the commit is not reported as a success.
        MutateFacilityAfterMilestoneInsert(database, "FI-2");

        var afterWrite = await service.CreateMilestoneAsync(
            "FI-2", new OgMilestoneValues("FABRICATION", AsOf.AddDays(-2)), "sara", 4);

        Assert.False(afterWrite.Success);
        Assert.Equal(OgConstructionService.CodeFacilityChanged, afterWrite.Reference);
        Assert.Contains(
            reporter.Failures,
            failure => failure.Contains("revalidating the facility after a construction write", StringComparison.Ordinal));

        DropMutatingTrigger(database);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE FIELD_DEVELOPMENT_PLAN (
                FDP_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                FDP_NAME TEXT NULL,
                FDP_VERSION INTEGER NOT NULL);

            CREATE TABLE FACILITY_INVESTMENT (
                FACILITY_INV_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NOT NULL,
                PLAN_VERSION INTEGER NOT NULL,
                FIELD_ID TEXT NULL,
                FACILITY_ID TEXT NULL,
                FACILITY_NAME TEXT NULL,
                FACILITY_TYPE TEXT NULL,
                INVESTMENT_PHASE TEXT NULL,
                CAPACITY_PLANNED_BOPD REAL NULL,
                CAPACITY_OUOM TEXT NULL,
                CAPEX_PLANNED_MM REAL NULL,
                CAPEX_ACTUAL_MM REAL NULL,
                COST_CURRENCY TEXT NULL,
                PLANNED_START_DATE TEXT NULL,
                PLANNED_COMPLETION_DATE TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE FACILITY_MILESTONE (
                MILESTONE_ID TEXT NOT NULL PRIMARY KEY,
                FACILITY_INV_ID TEXT NOT NULL,
                FACILITY_ID TEXT NULL,
                FDP_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                MILESTONE_NAME TEXT NOT NULL,
                MILESTONE_SEQ INTEGER NOT NULL,
                PLANNED_DATE TEXT NULL,
                ACTUAL_DATE TEXT NULL,
                STATE TEXT NULL,
                NOTES TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE FACILITY_MILESTONE_EVIDENCE (
                EVIDENCE_ID TEXT NOT NULL PRIMARY KEY,
                MILESTONE_ID TEXT NOT NULL,
                FACILITY_INV_ID TEXT NOT NULL,
                DOCUMENT TEXT NULL,
                REFERENCE TEXT NULL,
                EVIDENCE_DATE TEXT NULL,
                REVIEWER TEXT NULL,
                OUTCOME TEXT NOT NULL,
                ADDED_BY TEXT NULL,
                ADDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE FACILITY_HANDOVER (
                HANDOVER_ID TEXT NOT NULL PRIMARY KEY,
                FACILITY_INV_ID TEXT NOT NULL,
                FACILITY_ID TEXT NULL,
                FDP_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                HANDED_OVER_TO TEXT NOT NULL,
                HANDED_OVER_AT TEXT NULL,
                CONDITIONS_TEXT TEXT NULL,
                SIGNATURE TEXT NULL,
                SIGNED_BY TEXT NULL,
                SIGNED_AT TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_COMPLETION_SUBMISSION (
                SUBMISSION_ID TEXT NOT NULL PRIMARY KEY,
                WELL_ID TEXT NOT NULL,
                PROGRAM_ID TEXT NULL,
                SUMMARY TEXT NULL,
                STATE TEXT NULL,
                AUTHOR TEXT NULL,
                SUBMITTED_AT TEXT NULL,
                REVIEWER TEXT NULL,
                RATIONALE TEXT NULL,
                CONDITIONS_TEXT TEXT NULL,
                ACCEPTED_AT TEXT NULL,
                ACCEPTED_BY TEXT NULL,
                HANDOVER_TO TEXT NULL,
                HANDOVER_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE FACILITY_EQUIPMENT (
                FACILITY_ID TEXT NOT NULL,
                FACILITY_TYPE TEXT NULL,
                EQUIPMENT_ID TEXT NOT NULL,
                INSTALL_OBS_NO REAL NOT NULL,
                ACTIVE_IND TEXT NULL,
                INSTALL_DATE TEXT NULL,
                REMOVE_DATE TEXT NULL,
                REMOVE_REASON TEXT NULL,
                USE_DESCRIPTION TEXT NULL,
                REMARK TEXT NULL,
                PRIMARY KEY (FACILITY_ID, EQUIPMENT_ID, INSTALL_OBS_NO));

            INSERT INTO FIELD_DEVELOPMENT_PLAN (FDP_ID, FIELD_ID, FDP_NAME, FDP_VERSION) VALUES
                ('FDP-1', 'F-1', 'Alpha FDP', 3),
                ('FDP-2', 'F-2', 'Other field FDP', 1);

            INSERT INTO FACILITY_INVESTMENT
                (FACILITY_INV_ID, FDP_ID, PLAN_VERSION, FIELD_ID, FACILITY_ID, FACILITY_NAME, FACILITY_TYPE,
                 INVESTMENT_PHASE, CAPACITY_PLANNED_BOPD, CAPACITY_OUOM, CAPEX_PLANNED_MM, CAPEX_ACTUAL_MM,
                 COST_CURRENCY, PLANNED_START_DATE, PLANNED_COMPLETION_DATE) VALUES
                ('FI-1', 'FDP-1', 3, 'F-1', 'FAC-1', 'Central processing facility', 'CPF', 'CONSTRUCTION',
                 1000, 'BOPD', 80, 95, 'USD', '2026-01-01', '2026-12-01'),
                ('FI-2', 'FDP-1', 3, 'F-1', 'FAC-2', 'Gas plant', 'GAS', 'CONSTRUCTION',
                 40, 'MMSCFD', 30, NULL, 'USD', '2026-03-01', '2027-06-01'),
                ('FI-9', 'FDP-2', 1, 'F-2', 'FAC-9', 'Other field plant', 'CPF', 'PLANNED',
                 9000, 'BOPD', 90, NULL, 'USD', '2027-01-01', '2028-01-01');

            INSERT INTO FACILITY_MILESTONE
                (MILESTONE_ID, FACILITY_INV_ID, FACILITY_ID, FDP_ID, FIELD_ID, MILESTONE_NAME, MILESTONE_SEQ,
                 PLANNED_DATE, ACTUAL_DATE, STATE) VALUES
                ('MS-1', 'FI-1', 'FAC-1', 'FDP-1', 'F-1', 'DESIGN', 1, '2026-01-15', '2026-01-14', 'COMPLETE'),
                ('MS-3', 'FI-1', 'FAC-1', 'FDP-1', 'F-1', 'FABRICATION', 3, '2026-09-20', NULL, 'IN_PROGRESS');

            INSERT INTO FACILITY_MILESTONE_EVIDENCE
                (EVIDENCE_ID, MILESTONE_ID, FACILITY_INV_ID, DOCUMENT, REFERENCE, EVIDENCE_DATE, REVIEWER, OUTCOME,
                 ADDED_BY, ADDED_AT) VALUES
                ('EV-1', 'MS-1', 'FI-1', 'signed design drawing', 'design-FI-1.pdf', '2026-01-13', 'reviewer',
                 'ACCEPTED', 'sara', '2026-01-13');

            INSERT INTO DEVELOPMENT_COMPLETION_SUBMISSION
                (SUBMISSION_ID, WELL_ID, STATE, AUTHOR, REVIEWER, CONDITIONS_TEXT, ACCEPTED_AT, ACCEPTED_BY,
                 HANDOVER_TO, HANDOVER_AT) VALUES
                ('SUB-1', 'WELL-1', 'ACCEPTED_WITH_CONDITIONS', 'sara', 'reviewer',
                 'Install the flow meter within 30 days', '2026-09-10', 'reviewer', 'production', '2026-09-10');

            INSERT INTO FACILITY_EQUIPMENT
                (FACILITY_ID, FACILITY_TYPE, EQUIPMENT_ID, INSTALL_OBS_NO, ACTIVE_IND, INSTALL_DATE, USE_DESCRIPTION) VALUES
                ('FAC-1', 'CPF', 'EQ-1', 1, 'Y', '2026-02-01', 'inlet separator'),
                ('FAC-1', 'CPF', 'EQ-2', 1, 'N', '2025-11-01', 'old surge drum');
            """;
        command.ExecuteNonQuery();
    }

    private static void BumpPlanVersion(string database, string facilityInvId, int version)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE FACILITY_INVESTMENT SET PLAN_VERSION = $version WHERE FACILITY_INV_ID = $id;";
        command.Parameters.AddWithValue("$version", version);
        command.Parameters.AddWithValue("$id", facilityInvId);
        command.ExecuteNonQuery();
    }

    private static void MutateFacilityAfterMilestoneInsert(string database, string facilityInvId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TRIGGER MUTATE_FACILITY AFTER INSERT ON FACILITY_MILESTONE
             WHEN NEW.FACILITY_INV_ID = '{facilityInvId}'
             BEGIN
                 UPDATE FACILITY_INVESTMENT SET FDP_ID = 'FDP-MUTATED' WHERE FACILITY_INV_ID = NEW.FACILITY_INV_ID;
             END;
             """;
        command.ExecuteNonQuery();
    }

    private static void DropMutatingTrigger(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TRIGGER MUTATE_FACILITY;";
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
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
