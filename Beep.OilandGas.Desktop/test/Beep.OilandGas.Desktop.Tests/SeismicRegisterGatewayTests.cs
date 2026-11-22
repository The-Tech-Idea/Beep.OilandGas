using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// EXP-04 against a real SQLite database: the surveys are scoped to the field and carry their acquisition and
/// reference-coded processing state, lines are scoped to the selected survey, a survey without coordinates says so,
/// and a failed read is a failure rather than an empty register.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class SeismicRegisterGatewayTests
{
    private const string ConnectionName = "exp04-seismic";

    [IntegrationFact]
    public async Task Surveys_carry_their_states_and_lines_are_scoped_to_the_selected_survey()
    {        var root = Path.Combine(OgPaths.DataRoot, "exp04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasExp04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "seismic.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "seismic.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var register = new OgSeismicRegister(() => source, reporter);

        // AC-1: the field's surveys, with both states.
        var loaded = await register.LoadSurveysAsync("FIELD-1", "North");
        Assert.True(loaded.Success, loaded.Message);
        var surveys = loaded.Value!;

        Assert.Equal(2, surveys.Count);
        Assert.DoesNotContain(surveys, survey => survey.Id == "S-9");

        var north = Assert.Single(surveys, survey => survey.Id == "S-1");
        Assert.Equal("North 3D", north.Name);
        Assert.Equal("3D", north.Kind);
        Assert.Equal(new DateTime(2026, 1, 10), north.StartDate);
        Assert.Equal(new DateTime(2026, 2, 20), north.EndDate);
        Assert.Equal("acquired", north.AcquisitionState);

        // AC-5: the survey status and the processing state are reference-coded with their labels.
        Assert.Equal("Final (FINAL)", north.StatusText);
        Assert.Equal("Final (FINAL)", north.ProcessingState);
        Assert.Single(north.Processing);
        Assert.Equal("Vendor Co", north.Processing[0].Company);

        // AC-4: this survey has a recorded extent.
        Assert.True(north.HasGeometry);
        Assert.Contains("lat 26.1..26.4", north.Processing[0].ExtentText, StringComparison.Ordinal);

        // A planned survey with no dates, no reference status and a non-reference processing code.
        var planned = Assert.Single(surveys, survey => survey.Id == "S-2");
        Assert.Equal("planned", planned.AcquisitionState);
        Assert.Equal("no status recorded", planned.StatusText);
        Assert.Equal("DRAFTING (not in the reference table)", planned.ProcessingState);
        Assert.False(planned.HasGeometry);
        Assert.Contains("map is disabled", planned.MapReason, StringComparison.OrdinalIgnoreCase);

        // AC-2: lines belong to the selected survey only.
        var northLines = await register.LoadLinesAsync("S-1");
        Assert.True(northLines.Success, northLines.Message);
        Assert.Equal(2, northLines.Value!.Count);
        Assert.All(northLines.Value, line => Assert.Equal("S-1", line.SurveyId));
        Assert.Equal(["LINE-A", "LINE-B"], northLines.Value.Select(line => line.Name));

        var southLines = await register.LoadLinesAsync("S-2");
        Assert.Single(southLines.Value!);
        Assert.Equal("S-2", southLines.Value![0].SurveyId);

        // A survey with no lines is an empty list, not a failure.
        var none = await register.LoadLinesAsync("S-NONE");
        Assert.True(none.Success);
        Assert.Empty(none.Value!);

        // AC-1: a field with no surveys is an empty register.
        var empty = await register.LoadSurveysAsync("FIELD-NONE", "Nowhere");
        Assert.True(empty.Success);
        Assert.Empty(empty.Value!);

        // A failed read is a failure, not an empty register.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var drop = connection.CreateCommand();
            drop.CommandText = "DROP TABLE SEIS_ACQTN_SURVEY;";
            drop.ExecuteNonQuery();
        }

        var failed = await register.LoadSurveysAsync("FIELD-1", "North");

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
        Assert.NotNull(failed.Reference);
        Assert.NotEmpty(reporter.Failures);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE SEIS_ACQTN_SURVEY (
                SEIS_ACQTN_SURVEY_ID TEXT NOT NULL PRIMARY KEY,
                ACQTN_SURVEY_NAME TEXT NULL,
                SEIS_DIMENSION TEXT NULL,
                AREA_ID TEXT NULL,
                AREA_TYPE TEXT NULL,
                START_DATE TEXT NULL,
                COMPLETED_DATE TEXT NULL);

            CREATE TABLE SEIS_SET_STATUS (
                SEIS_SET_ID TEXT NOT NULL,
                STATUS_ID TEXT NOT NULL,
                SEIS_STATUS TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                PRIMARY KEY (SEIS_SET_ID, STATUS_ID));

            CREATE TABLE R_SEIS_STATUS (
                SEIS_STATUS TEXT NOT NULL PRIMARY KEY,
                LONG_NAME TEXT NULL,
                SHORT_NAME TEXT NULL);

            CREATE TABLE R_SEIS_PROC_STATUS (
                PROCESS_STATUS TEXT NOT NULL PRIMARY KEY,
                LONG_NAME TEXT NULL,
                SHORT_NAME TEXT NULL);

            CREATE TABLE SEIS_PROC_SET (
                SEIS_PROC_SET_ID TEXT NOT NULL PRIMARY KEY,
                PROCESS_STATUS TEXT NULL,
                PROCESSING_COMPANY TEXT NULL,
                PROCESSING_NAME TEXT NULL,
                START_DATE TEXT NULL,
                END_DATE TEXT NULL,
                MIN_LATITUDE REAL NULL,
                MAX_LATITUDE REAL NULL,
                MIN_LONGITUDE REAL NULL,
                MAX_LONGITUDE REAL NULL);

            CREATE TABLE SEIS_SET_COMPONENT (
                SEIS_SET_ID TEXT NOT NULL,
                COMPONENT_OBS_NO REAL NOT NULL,
                SEIS_ACT_SEIS_SET_ID TEXT NULL,
                PRIMARY KEY (SEIS_SET_ID, COMPONENT_OBS_NO));

            CREATE TABLE SEIS_LINE (
                SEIS_LINE_ID TEXT NOT NULL PRIMARY KEY,
                SEIS_ACQTN_SURVEY_ID TEXT NULL,
                LINE_NAME TEXT NULL,
                DIMENSION TEXT NULL,
                REFRACTION_REFLECTION TEXT NULL);

            INSERT INTO R_SEIS_STATUS (SEIS_STATUS, LONG_NAME, SHORT_NAME) VALUES
                ('FINAL', 'Final', 'FIN'),
                ('ACQUIRING', 'Acquiring', 'ACQ');

            INSERT INTO R_SEIS_PROC_STATUS (PROCESS_STATUS, LONG_NAME, SHORT_NAME) VALUES
                ('FINAL', 'Final', 'FIN'),
                ('IN_PROGRESS', 'In progress', 'INP');

            INSERT INTO SEIS_ACQTN_SURVEY
                (SEIS_ACQTN_SURVEY_ID, ACQTN_SURVEY_NAME, SEIS_DIMENSION, AREA_ID, AREA_TYPE, START_DATE,
                 COMPLETED_DATE)
            VALUES
                ('S-1', 'North 3D', '3D', 'FIELD-1', 'FIELD', '2026-01-10', '2026-02-20'),
                ('S-2', 'South 2D', '2D', 'FIELD-1', 'FIELD', NULL, NULL),
                ('S-9', 'Other field', '3D', 'FIELD-9', 'FIELD', '2026-01-01', NULL);

            INSERT INTO SEIS_SET_STATUS (SEIS_SET_ID, STATUS_ID, SEIS_STATUS, EFFECTIVE_DATE) VALUES
                ('S-1', 'ST-1', 'FINAL', '2026-02-25');

            INSERT INTO SEIS_PROC_SET
                (SEIS_PROC_SET_ID, PROCESS_STATUS, PROCESSING_COMPANY, PROCESSING_NAME, START_DATE, END_DATE,
                 MIN_LATITUDE, MAX_LATITUDE, MIN_LONGITUDE, MAX_LONGITUDE)
            VALUES
                ('PS-1', 'FINAL', 'Vendor Co', 'PSTM 2026', '2026-02-01', '2026-02-25', 26.1, 26.4, 50.1, 50.6),
                ('PS-2', 'DRAFTING', 'Other Co', 'PSTM draft', '2026-03-01', NULL, NULL, NULL, NULL, NULL);

            INSERT INTO SEIS_SET_COMPONENT (SEIS_SET_ID, COMPONENT_OBS_NO, SEIS_ACT_SEIS_SET_ID) VALUES
                ('PS-1', 1, 'S-1'),
                ('PS-2', 1, 'S-2');

            INSERT INTO SEIS_LINE (SEIS_LINE_ID, SEIS_ACQTN_SURVEY_ID, LINE_NAME, DIMENSION, REFRACTION_REFLECTION)
            VALUES
                ('L-1', 'S-1', 'LINE-A', '3D', 'REFLECTION'),
                ('L-2', 'S-1', 'LINE-B', '3D', 'REFLECTION'),
                ('L-3', 'S-2', 'LINE-C', '2D', 'REFRACTION'),
                ('L-9', 'S-9', 'LINE-OTHER', '3D', 'REFLECTION');
            """;
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
