using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-02 against a real SQLite database: an edit round-trips and leaves untouched columns alone, a stale edit is a
/// conflict, status is never written here, participant changes carry actor and date, and the operator cannot be
/// removed.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class WellHeaderGatewayTests
{
    private const string ConnectionName = "wel02-header";

    [IntegrationFact]
    public async Task An_edit_round_trips_preserves_untouched_columns_and_refuses_the_operator_removal()
    {        var root = Path.Combine(OgPaths.DataRoot, "wel02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWel02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "header.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "header.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWellHeaderService(() => source, reporter);

        // AC-2: two API identities disagree, so none is chosen automatically.
        var loaded = await service.LoadAsync("UWI-A");
        Assert.True(loaded.Success, loaded.Message);
        var bundle = loaded.Value!;

        Assert.True(bundle.Header.HasAmbiguousIdentifiers);
        Assert.Null(bundle.Header.ApiNumber);
        Assert.Equal(["API"], bundle.AmbiguousTypes);
        Assert.Single(bundle.Participants);

        // AC-1: change two fields; everything else must survive.
        var edited = bundle.Header with
        {
            Name = "ALPHA-RENAMED",
            KellyBushing = 123m,
            Status = "SUSPENDED",   // AC-6: must be ignored
        };

        var saved = await service.SaveAsync(edited, bundle.Header, coordinateSystem: null, actor: "fahad");
        Assert.True(saved.Success, saved.Message);

        var reloaded = (await service.LoadAsync("UWI-A")).Value!;
        Assert.Equal("ALPHA-RENAMED", reloaded.Header.Name);
        Assert.Equal(123m, reloaded.Header.KellyBushing);
        Assert.Equal("GOV-1", reloaded.Header.GovernmentId);
        Assert.Equal("BA-1", reloaded.Header.Operator);
        Assert.Equal(26.1m, reloaded.Header.SurfaceLatitude);
        Assert.Equal(3000m, reloaded.Header.FinalTd);

        // AC-6: status is untouched by the header.
        Assert.Equal("ACTIVE", reloaded.Header.Status);

        // AC-1 (concurrency): the same edit again, with the stale original, no longer matches.
        var stale = await service.SaveAsync(edited with { Name = "OTHER" }, bundle.Header, null, "fahad");
        Assert.False(stale.Success);
        Assert.Equal(OgWellHeaderService.CodeConflict, stale.Reference);

        // AC-4: adding a participant records who and when.
        var added = await service.AddParticipantAsync("UWI-A", "BA-2", "contractor", "sara");
        Assert.True(added.Success, added.Message);

        var withParticipant = (await service.LoadAsync("UWI-A")).Value!;
        var partner = Assert.Single(withParticipant.Participants, participant => participant.BusinessAssociateId == "BA-2");
        Assert.Equal("PARTNER LTD", partner.Name);
        Assert.Equal("contractor", partner.Description);

        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT ROW_CHANGED_BY, ROW_CHANGED_DATE FROM WELL_BA_SERVICE WHERE UWI = 'UWI-A' AND PROVIDED_BY = 'BA-2';";
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal("sara", reader.GetString(0));
            Assert.False(string.IsNullOrWhiteSpace(reader.GetString(1)));
        }

        // AC-5: the operator cannot be removed; another participant can.
        var operatorRemoval = await service.RemoveParticipantAsync("UWI-A", "BA-1", "sara");
        Assert.False(operatorRemoval.Success);
        Assert.Equal(OgWellHeaderService.CodeOperatorRequired, operatorRemoval.Reference);

        var partnerRemoval = await service.RemoveParticipantAsync("UWI-A", "BA-2", "sara");
        Assert.True(partnerRemoval.Success, partnerRemoval.Message);

        var afterRemoval = (await service.LoadAsync("UWI-A")).Value!;
        Assert.DoesNotContain(afterRemoval.Participants, participant => participant.BusinessAssociateId == "BA-2");
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE WELL (
                UWI TEXT NOT NULL PRIMARY KEY,
                WELL_NAME TEXT NULL,
                WELL_GOVERNMENT_ID TEXT NULL,
                ASSIGNED_FIELD TEXT NULL,
                OPERATOR TEXT NULL,
                SURFACE_LATITUDE REAL NULL,
                SURFACE_LONGITUDE REAL NULL,
                GROUND_ELEV REAL NULL,
                KB_ELEV REAL NULL,
                FINAL_TD REAL NULL,
                WELL_LEVEL_TYPE TEXT NULL,
                CURRENT_STATUS TEXT NULL,
                CURRENT_CLASS TEXT NULL,
                PROFILE_TYPE TEXT NULL,
                SPUD_DATE TEXT NULL,
                COMPLETION_DATE TEXT NULL,
                ABANDONMENT_DATE TEXT NULL,
                CURRENT_STATUS_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE WELL_XREF (
                UWI TEXT NOT NULL,
                XREF_ID TEXT NOT NULL,
                XREF_TYPE TEXT NULL,
                SOURCE TEXT NULL,
                PRIMARY KEY (UWI, XREF_ID));

            CREATE TABLE WELL_BA_SERVICE (
                UWI TEXT NOT NULL,
                PROVIDED_BY TEXT NOT NULL,
                SERVICE_SEQ_NO INTEGER NOT NULL,
                ACTIVE_IND TEXT NULL,
                DESCRIPTION TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                END_DATE TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (UWI, PROVIDED_BY, SERVICE_SEQ_NO));

            CREATE TABLE BUSINESS_ASSOCIATE (
                BUSINESS_ASSOCIATE_ID TEXT NOT NULL PRIMARY KEY,
                BA_LONG_NAME TEXT NULL,
                BA_SHORT_NAME TEXT NULL,
                ACTIVE_IND TEXT NULL);

            INSERT INTO WELL (UWI, WELL_NAME, WELL_GOVERNMENT_ID, ASSIGNED_FIELD, OPERATOR,
                              SURFACE_LATITUDE, SURFACE_LONGITUDE, GROUND_ELEV, KB_ELEV, FINAL_TD,
                              WELL_LEVEL_TYPE, CURRENT_STATUS, CURRENT_CLASS, PROFILE_TYPE,
                              SPUD_DATE, CURRENT_STATUS_DATE)
            VALUES ('UWI-A', 'ALPHA-1', 'GOV-1', 'NORTH', 'BA-1',
                    26.1, 50.2, 100, 110, 3000,
                    'OIL', 'ACTIVE', 'DEVELOPMENT', 'VERTICAL',
                    '2020-01-01', '2026-01-01');

            INSERT INTO WELL_XREF (UWI, XREF_ID, XREF_TYPE, SOURCE) VALUES
                ('UWI-A', '42-001-0001', 'API', 'regulator'),
                ('UWI-A', '42-001-0002', 'API', 'legacy'),
                ('UWI-A', 'GOV-1', 'GOVERNMENT', 'ministry');

            INSERT INTO BUSINESS_ASSOCIATE (BUSINESS_ASSOCIATE_ID, BA_LONG_NAME, BA_SHORT_NAME, ACTIVE_IND) VALUES
                ('BA-1', 'OPERATOR CO', 'OPCO', 'Y'),
                ('BA-2', 'PARTNER LTD', 'PARTNER', 'Y');

            INSERT INTO WELL_BA_SERVICE (UWI, PROVIDED_BY, SERVICE_SEQ_NO, ACTIVE_IND, DESCRIPTION,
                                         ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE)
            VALUES ('UWI-A', 'BA-1', 1, 'Y', 'operator', 'system', '2026-01-01', 'system', '2026-01-01');
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
