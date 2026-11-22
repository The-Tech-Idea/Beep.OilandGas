using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-02: the rules that hold without a database - an ambiguous identity is not auto-chosen, the operator cannot
/// be cleared, a location change needs its coordinate system, status is read-only, and the local notes record who
/// decided what.
/// </summary>
public sealed class WellHeaderTests
{
    [Fact]
    public void One_identifier_of_a_type_is_chosen_and_two_that_disagree_are_not()
    {
        var single = new[] { new OgWellIdentifier("42-001-1", "API", null, false) };
        Assert.Equal("42-001-1", OgWellHeaderService.ChosenIdentifier(single, "API"));

        var conflicting = new[]
        {
            new OgWellIdentifier("42-001-1", "API", null, true),
            new OgWellIdentifier("42-001-2", "API", null, true),
        };

        // AC-2
        Assert.Null(OgWellHeaderService.ChosenIdentifier(conflicting, "API"));
    }

    [Fact]
    public void Clearing_the_operator_is_refused()
    {
        var original = Header(operatorId: "BA-1");
        var updated = original with { Operator = "  " };

        var saved = new OgWellHeaderService(() => null!, new Quiet())
            .SaveAsync(updated, original, coordinateSystem: null, actor: "fahad")
            .GetAwaiter().GetResult();

        // AC-5
        Assert.False(saved.Success);
        Assert.Equal(OgWellHeaderService.CodeOperatorRequired, saved.Reference);
    }

    [Fact]
    public void A_location_change_without_a_coordinate_system_is_refused()
    {
        var original = Header(operatorId: "BA-1", latitude: 26.1m);
        var updated = original with { SurfaceLatitude = 26.2m };

        var saved = new OgWellHeaderService(() => null!, new Quiet())
            .SaveAsync(updated, original, coordinateSystem: "  ", actor: "fahad")
            .GetAwaiter().GetResult();

        // AC-3
        Assert.False(saved.Success);
        Assert.Equal(OgWellHeaderService.CodeCoordinateSystemRequired, saved.Reference);
    }

    [Fact]
    public void A_change_must_record_who_made_it()
    {
        var header = Header(operatorId: "BA-1");

        var saved = new OgWellHeaderService(() => null!, new Quiet())
            .SaveAsync(header with { Name = "ALPHA" }, header, coordinateSystem: null, actor: " ")
            .GetAwaiter().GetResult();

        Assert.False(saved.Success);
        Assert.Equal("well-actor-required", saved.Reference);
    }

    [Fact]
    public void Status_is_read_only_here_and_a_missing_location_is_visible()
    {
        var noLocation = Header(latitude: null, longitude: null);

        // AC-6
        Assert.True(noLocation.StatusIsReadOnly);

        // The warning chip's condition.
        Assert.False(noLocation.HasLocation);
        Assert.True((noLocation with { SurfaceLatitude = 26.1m, SurfaceLongitude = 50.2m }).HasLocation);
    }

    [Fact]
    public void The_coordinate_system_and_the_identifier_choice_survive_a_restart()
    {
        var path = TempNotesPath();
        var first = new OgWellHeaderNotesStore(new Quiet(), path);

        var record = first.RecordLocationSystem("UWI-1", "EPSG:4326", "fahad");
        first.Choose("UWI-1", "API", "42-001-2", "fahad");

        // A new instance reads the same file.
        var second = new OgWellHeaderNotesStore(new Quiet(), path);

        Assert.Equal("EPSG:4326", second.LocationSystem("UWI-1")!.CoordinateSystem);
        Assert.Equal("fahad", record.Actor);
        var choice = Assert.Single(second.Choices("UWI-1"));
        Assert.Equal("42-001-2", choice.XrefId);
        Assert.Equal("API", choice.Type);

        // Choosing again replaces the type's choice rather than adding another.
        second.Choose("UWI-1", "API", "42-001-1", "sara");
        Assert.Equal("42-001-1", Assert.Single(second.Choices("UWI-1")).XrefId);
    }

    private static OgWellHeader Header(
        string? operatorId = null,
        decimal? latitude = 26.1m,
        decimal? longitude = 50.2m) =>
        new(
            Uwi: "UWI-1",
            Name: "ALPHA-1",
            GovernmentId: "GOV-1",
            ApiNumber: null,
            Field: "NORTH",
            Operator: operatorId,
            SurfaceLatitude: latitude,
            SurfaceLongitude: longitude,
            GroundElevation: 100m,
            KellyBushing: 110m,
            FinalTd: 3000m,
            WellType: "OIL",
            Status: "ACTIVE",
            WellClass: "DEVELOPMENT",
            Profile: "VERTICAL",
            SpudDate: new DateTime(2020, 1, 1),
            CompletionDate: null,
            AbandonmentDate: null,
            LastActivity: new DateTime(2026, 1, 1));

    private static string TempNotesPath()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "wellnotes-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "notes.json");
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
