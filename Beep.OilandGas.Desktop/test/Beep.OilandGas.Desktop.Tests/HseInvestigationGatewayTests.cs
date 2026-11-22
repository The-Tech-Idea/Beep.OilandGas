using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-03 against a real SQLite database: the investigator is assigned once and the original is retained (AC-1), a different
/// field context and a roster change cannot clear it (AC-2, AC-3), every action carries an owner and a due date (AC-6), an
/// action is closed out only when completed and verified (AC-4), and the investigator cannot close (AC-5).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HseInvestigationGatewayTests
{
    private const string ConnectionName = "hse03-investigation";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    [IntegrationFact]
    public async Task An_investigation_is_assigned_caused_acted_and_closed_with_its_rules()
    {        var root = Path.Combine(OgPaths.DataRoot, "hse03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasHse03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "investigation.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "investigation.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var investigation = new OgInvestigation(() => source, reporter);

        // ---- the starting point ----
        var loaded = await investigation.LoadAsync("INC-1");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(IncidentState.Reported, loaded.Value!.IncidentState);
        Assert.Null(loaded.Value.Investigator);
        Assert.Empty(loaded.Value.Causes);
        Assert.Empty(loaded.Value.Actions);
        Assert.False(loaded.Value.RootCauseComplete);

        // ---- AC-1: the assignment is recorded once and retained ----
        var assigned = await investigation.AssignAsync("INC-1", "omar", "INVESTIGATOR", "He knows the separator train.",
            Scope, "sara");

        Assert.True(assigned.Success, assigned.Message);
        Assert.Equal("omar", assigned.Value!.Investigator);
        Assert.Equal("INVESTIGATOR", assigned.Value.InvestigatorRole);
        Assert.Equal(1, Count(database,
            "HSE_INCIDENT_COMPONENT WHERE INCIDENT_ID = 'INC-1' AND COMPONENT_ROLE = 'INVESTIGATOR' AND BUSINESS_ASSOCIATE_ID = 'omar'"));

        var second = await investigation.AssignAsync("INC-1", "nadia", "INVESTIGATOR", "A second opinion.", Scope, "sara");

        Assert.False(second.Success);
        Assert.Equal(InvestigationRules.CodeAssignmentRecorded, second.Reference);
        Assert.Contains("omar", second.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "HSE_INCIDENT_COMPONENT WHERE INCIDENT_ID = 'INC-1' AND COMPONENT_ROLE = 'INVESTIGATOR'"));

        // The same participant is a retry: the original is adopted, not duplicated.
        var retried = await investigation.AssignAsync("INC-1", "omar", "INVESTIGATOR", "He knows the separator train.",
            Scope, "sara");

        Assert.True(retried.Success, retried.Message);
        Assert.Equal(1, Count(database,
            "HSE_INCIDENT_COMPONENT WHERE INCIDENT_ID = 'INC-1' AND COMPONENT_ROLE = 'INVESTIGATOR'"));

        // ---- AC-2: a different field context cannot clear or move it ----
        var movedField = await investigation.AssignAsync("INC-1", "nadia", "INVESTIGATOR", "Another field.",
            new OgHseScope("SMOKE", "F-2"), "sara");

        Assert.False(movedField.Success);
        Assert.Equal(InvestigationRules.CodeFieldChanged, movedField.Reference);
        Assert.Equal("omar", (await investigation.LoadAsync("INC-1")).Value!.Investigator);

        // ---- AC-3: a roster change does not clear the pending original ----
        Execute(database,
            "INSERT INTO HSE_INCIDENT_COMPONENT (INCIDENT_ID, COMPONENT_OBS_NO, COMPONENT_ROLE, COMPONENT_TYPE, BUSINESS_ASSOCIATE_ID, ACTIVE_IND) VALUES ('INC-1', 9, 'WITNESS', 'WITNESS', 'nadia', 'Y');");

        var afterRoster = await investigation.AssignAsync("INC-1", "nadia", "INVESTIGATOR", "The roster changed.",
            Scope, "sara");

        Assert.False(afterRoster.Success);
        Assert.Equal(InvestigationRules.CodeAssignmentRecorded, afterRoster.Reference);
        Assert.Equal("omar", (await investigation.LoadAsync("INC-1")).Value!.Investigator);

        // ---- the cause chain, and the owner's root-cause rule ----
        Assert.True((await investigation.AddCauseAsync("INC-1", CauseType.Immediate, CauseCategory.Equipment,
            "The seal failed.", "omar")).Success);

        Assert.True((await investigation.AddCauseAsync("INC-1", CauseType.Root, CauseCategory.Process,
            "Pump ran dry", "omar")).Success);

        Assert.False((await investigation.LoadAsync("INC-1")).Value!.RootCauseComplete);

        Assert.True((await investigation.AddCauseAsync("INC-1", CauseType.Root, CauseCategory.Process,
            "The seal failed because the pump ran dry for six hours", "omar")).Success);

        var withCauses = (await investigation.LoadAsync("INC-1")).Value!;

        Assert.Equal(3, withCauses.Causes.Count);
        Assert.True(withCauses.RootCauseComplete);
        Assert.Equal(1, withCauses.Causes[0].Seq);
        Assert.Contains("root", withCauses.CausesText, StringComparison.OrdinalIgnoreCase);

        // ---- AC-6: every action records its owner and its due date ----
        var noOwner = await investigation.AddActionAsync("INC-1", "Seal plan", "Replace the seal", " ", 
            new DateTime(2026, 12, 1), Scope, "sara");

        Assert.False(noOwner.Success);
        Assert.Equal(InvestigationRules.CodeInputRequired, noOwner.Reference);

        var first = await investigation.AddActionAsync("INC-1", "Seal plan", "Replace the seal", "omar",
            new DateTime(2026, 12, 1), Scope, "sara");

        Assert.True(first.Success, first.Message);

        var firstAction = first.Value!.Actions.Single(action => action.Action == "Replace the seal");

        Assert.Equal("omar", firstAction.Owner);
        Assert.Equal(new DateTime(2026, 12, 1), firstAction.DueDate);
        Assert.Equal(HseReferenceCodes.CorrectiveAction.Open, firstAction.State);
        Assert.False(firstAction.IsClosedOut);

        Assert.Equal(1, Count(database,
            "PROJECT WHERE PROJECT_TYPE = 'CORRECTIVE_ACTION' AND PROJECT_NAME = 'Seal plan'"));
        Assert.Equal(1, Count(database,
            $"PROJECT_STEP WHERE STEP_NAME = 'Replace the seal' AND DUE_DATE LIKE '2026-12-01%'"));
        Assert.Equal(1, Count(database,
            "PROJECT_STEP_BA WHERE ROLE = 'RESPONSIBLE' AND BUSINESS_ASSOCIATE_ID = 'omar'"));
        Assert.Equal(1, Count(database,
            "HSE_INCIDENT_COMPONENT WHERE INCIDENT_ID = 'INC-1' AND PROJECT_ID IS NOT NULL"));

        // A second, overdue action.
        var secondAction = await investigation.AddActionAsync("INC-1", "Seal plan", "Update the procedure", "nadia",
            new DateTime(2026, 1, 1), Scope, "sara");

        Assert.True(secondAction.Success, secondAction.Message);

        var overdue = secondAction.Value!.Actions.Single(action => action.Action == "Update the procedure");

        Assert.True(overdue.Overdue);
        Assert.Contains("(overdue)", overdue.DueText, StringComparison.Ordinal);

        // ---- AC-4: verification needs completion, and the owner cannot verify its own action ----
        var early = await investigation.VerifyActionAsync("INC-1", firstAction.StepId, "nadia", "nadia");

        Assert.False(early.Success);
        Assert.Equal(InvestigationRules.CodeActionNotCompleted, early.Reference);

        Assert.True((await investigation.CompleteActionAsync("INC-1", firstAction.StepId, "omar")).Success);

        var byOwner = await investigation.VerifyActionAsync("INC-1", firstAction.StepId, "omar", "omar");

        Assert.False(byOwner.Success);
        Assert.Equal(InvestigationRules.CodeInputInvalid, byOwner.Reference);
        Assert.Contains("owns", byOwner.Message!, StringComparison.Ordinal);

        var verified = await investigation.VerifyActionAsync("INC-1", firstAction.StepId, "nadia", "sara");

        Assert.True(verified.Success, verified.Message);

        var verifiedAction = verified.Value!.Actions.Single(action => action.StepId == firstAction.StepId);

        Assert.True(verifiedAction.IsCompleted);
        Assert.True(verifiedAction.IsVerified);
        Assert.True(verifiedAction.IsClosedOut);
        Assert.Equal("nadia", verifiedAction.VerifiedBy);

        // Verifying again is an idempotent retry, not a second row.
        Assert.True((await investigation.VerifyActionAsync("INC-1", firstAction.StepId, "nadia", "sara")).Success);
        Assert.Equal(1, Count(database,
            $"PROJECT_STEP_BA WHERE ROLE = 'VERIFIER' AND STEP_ID = '{firstAction.StepId}'"));

        // ---- AC-4: closure is refused while an action is not closed out ----
        var blocked = await investigation.CloseAsync("INC-1", "Closed after review", "Actions done", "nadia");

        Assert.False(blocked.Success);
        Assert.Equal(InvestigationRules.CodeClosureBlocked, blocked.Reference);
        Assert.Contains("Update the procedure", blocked.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "HSE_INCIDENT_RESPONSE WHERE INCIDENT_ID = 'INC-1' AND RESPONSE_RESULT = 'CLOSED'"));

        Assert.True((await investigation.CompleteActionAsync("INC-1", overdue.StepId, "nadia")).Success);
        Assert.True((await investigation.VerifyActionAsync("INC-1", overdue.StepId, "omar", "sara")).Success);

        // ---- AC-5: the investigator cannot close ----
        var byInvestigator = await investigation.CloseAsync("INC-1", "Closed after review", "Actions done", "omar");

        Assert.False(byInvestigator.Success);
        Assert.Equal(InvestigationRules.CodeCloserIsInvestigator, byInvestigator.Reference);
        Assert.Contains("investigated", byInvestigator.Message!, StringComparison.Ordinal);

        // The decision and its reason are required.
        var noReason = await investigation.CloseAsync("INC-1", "Closed after review", "  ", "nadia");

        Assert.False(noReason.Success);
        Assert.Equal(InvestigationRules.CodeReasonRequired, noReason.Reference);

        var closed = await investigation.CloseAsync("INC-1", "Closed after review",
            "The seal was replaced and the procedure updated.", "nadia");

        Assert.True(closed.Success, closed.Message);
        Assert.Equal(IncidentState.Closed, closed.Value!.IncidentState);
        Assert.True(closed.Value.AllActionsVerified);
        Assert.Equal(1, Count(database,
            "HSE_INCIDENT_RESPONSE WHERE INCIDENT_ID = 'INC-1' AND RESPONSE_RESULT = 'CLOSED' AND REMARK LIKE '%Closed after review%'"));

        // A closed incident is not closed twice.
        var twice = await investigation.CloseAsync("INC-1", "Again", "Again", "nadia");

        Assert.False(twice.Success);
        Assert.Equal(InvestigationRules.CodeAlreadyClosed, twice.Reference);

        // ---- the checklist is the same sentence the command used ----
        var final = (await investigation.LoadAsync("INC-1")).Value!;

        Assert.True(final.AllActionsVerified);
        Assert.Equal(2, final.Actions.Count);
        Assert.Contains("✓", final.ChecklistText, StringComparison.Ordinal);
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

            CREATE TABLE HSE_INCIDENT_COMPONENT (
                INCIDENT_ID TEXT,
                COMPONENT_OBS_NO REAL,
                ACTIVE_IND TEXT,
                ACTIVITY_OBS_NO REAL,
                ADDRESS_OBS_NO REAL,
                ADDRESS_SOURCE TEXT,
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
                CLASSIFICATION_SYSTEM_ID TEXT,
                CLASS_LEVEL_ID TEXT,
                COMPONENT_ROLE TEXT,
                COMPONENT_TYPE TEXT,
                CONSENT_ID TEXT,
                CONSULT_ID TEXT,
                CONTEST_ID TEXT,
                CONTRACT_ID TEXT,
                CREW_COMPANY_BA_ID TEXT,
                CREW_ID TEXT,
                CREW_MEMBER_BA_ID TEXT,
                CREW_MEMBER_OBS_NO REAL,
                ECOZONE_ID TEXT,
                EFFECTIVE_DATE TEXT,
                EMPLOYEE_BA_ID TEXT,
                EMPLOYEE_OBS_NO REAL,
                EMPLOYER_BA_ID TEXT,
                ENTITLEMENT_ID TEXT,
                EQUIPMENT_ID TEXT,
                EXPIRY_DATE TEXT,
                FACILITY_ID TEXT,
                FACILITY_TYPE TEXT,
                FIELD_ID TEXT,
                FIELD_STATION_ID TEXT,
                FINANCE_ID TEXT,
                FOSSIL_ID TEXT,
                INCIDENT_SET_ID TEXT,
                INCIDENT_TYPE TEXT,
                INFORMATION_ITEM_ID TEXT,
                INFO_ITEM_SUBTYPE TEXT,
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
                PALEO_SUMMARY_ID TEXT,
                PDEN_ACTIVITY_TYPE TEXT,
                PDEN_AMENDMENT_SEQ_NO REAL,
                PDEN_ID TEXT,
                PDEN_PERIOD_ID TEXT,
                PDEN_PERIOD_TYPE TEXT,
                PDEN_SOURCE TEXT,
                PDEN_SUBTYPE TEXT,
                PDEN_VOLUME_DATE TEXT,
                PDEN_VOLUME_DATE_DESC TEXT,
                PDEN_VOLUME_METHOD TEXT,
                PHYSICAL_ITEM_ID TEXT,
                POOL_ID TEXT,
                PPDM_GUID TEXT,
                PPDM_SYSTEM_ID TEXT,
                PPDM_TABLE_NAME TEXT,
                PRODUCT_TYPE TEXT,
                PROD_STRING_ID TEXT,
                PROD_STRING_SOURCE TEXT,
                PROJECT_ID TEXT,
                PROJECT_PLAN_ID TEXT,
                PR_STR_FORM_OBS_NO REAL,
                RATE_SCHEDULE_ID TEXT,
                REFERENCED_GUID TEXT,
                REFERENCED_SYSTEM_ID TEXT,
                REFERENCED_TABLE_NAME TEXT,
                REMARK TEXT,
                RESENT_ID TEXT,
                RESERVE_CLASS_ID TEXT,
                RESTRICTION_ID TEXT,
                RESTRICTION_VERSION REAL,
                SAMPLE_ANAL_SOURCE TEXT,
                SEIS_SET_ID TEXT,
                SEIS_SET_SUBTYPE TEXT,
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
                TOTAL_INCIDENT_COUNT REAL,
                TOTAL_INCIDENT_PERIOD REAL,
                TOTAL_INCIDENT_PERIOD_UOM TEXT,
                UWI TEXT,
                WELL_ACTIVITY_SET_ID TEXT,
                WELL_ACTIVITY_SOURCE TEXT,
                WELL_ACTIVITY_TYPE_ID TEXT,
                WELL_DRILL_PERIOD_OBS_NO REAL,
                WELL_SET_ID TEXT,
                WORK_ORDER_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT);

            CREATE TABLE HSE_INCIDENT_CAUSE (
                INCIDENT_ID TEXT,
                CAUSE_OBS_NO REAL,
                ACTIVE_IND TEXT,
                CAUSE_CODE TEXT,
                CAUSE_TYPE TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
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

            CREATE TABLE PROJECT_STEP_BA (
                PROJECT_ID TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                ROLE TEXT,
                ROLE_SEQ_NO REAL,
                STEP_ID TEXT,
                STEP_BA_OBS_NO REAL,
                ACTIVE_IND TEXT,
                ACTUAL_IND TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PLAN_IND TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                SOURCE TEXT,
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

            INSERT INTO HSE_INCIDENT (INCIDENT_ID, INCIDENT_DATE, REMARK, ACTIVE_IND, SOURCE) VALUES
                ('INC-1', '2026-10-01 08:00:00', 'Spill at separator', 'Y', 'seed');

            INSERT INTO HSE_INCIDENT_RESPONSE (INCIDENT_ID, RESPONSE_OBS_NO, RESPONSE_TYPE, RESPONSE_RESULT,
                EFFECTIVE_DATE, ACTIVE_IND) VALUES
                ('INC-1', 1, 'STATUS', 'REPORTED', '2026-10-01', 'Y');

            INSERT INTO HSE_INCIDENT_COMPONENT (INCIDENT_ID, COMPONENT_OBS_NO, FIELD_ID, COMPONENT_ROLE,
                COMPONENT_TYPE, JURISDICTION, ACTIVE_IND) VALUES
                ('INC-1', 1, 'F-1', 'PRIMARY_FIELD_CONTEXT', 'INCIDENT_LOCATION', 'TX', 'Y');
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
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
