using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// EXP-04: the rules that hold without a database - the acquisition state comes from the recorded dates, the
/// processing state is a reference code and its label (never free text), a survey without coordinates disables the
/// map, and the "no data loading" notice is a constant.
/// </summary>
public sealed class SeismicRegisterTests
{
    [Theory]
    [InlineData(null, null, "planned")]
    [InlineData("2026-01-01", null, "acquiring")]
    [InlineData("2026-01-01", "2026-02-01", "acquired")]
    public void The_acquisition_state_comes_from_the_recorded_dates(string? start, string? end, string expected)
    {
        var survey = Survey(
            start: start is null ? null : DateTime.Parse(start),
            end: end is null ? null : DateTime.Parse(end));

        // AC-1
        Assert.Equal(expected, survey.AcquisitionState);
    }

    [Fact]
    public void The_processing_state_is_the_reference_code_with_its_label()
    {
        var finalized = Run("FINAL", "Final", inReference: true);
        var unknown = Run("DRAFTING", null, inReference: false);
        var missing = Run(null, null, inReference: false);

        // AC-5
        Assert.Equal("Final (FINAL)", finalized.StatusText);
        Assert.Equal("DRAFTING (not in the reference table)", unknown.StatusText);
        Assert.Equal("not recorded", missing.StatusText);

        // A survey with no run says so rather than showing an empty state.
        Assert.Equal("no processing recorded", Survey().ProcessingState);
        Assert.Equal("Final (FINAL)", Survey(processing: [finalized]).ProcessingState);

        // The latest run is the first one read, so the survey's state is that run's.
        Assert.Equal("DRAFTING (not in the reference table)", Survey(processing: [unknown, finalized]).ProcessingState);
    }

    [Fact]
    public void A_survey_without_coordinates_disables_the_map_and_says_why()
    {
        var withExtent = Run("FINAL", "Final", inReference: true, minLat: 26.1m, maxLat: 26.4m, minLong: 50.1m, maxLong: 50.6m);
        var withoutExtent = Run("FINAL", "Final", inReference: true);

        // AC-4
        var located = Survey(processing: [withExtent]);
        Assert.True(located.HasGeometry);
        Assert.DoesNotContain("disabled", located.MapReason, StringComparison.OrdinalIgnoreCase);

        var unlocated = Survey(processing: [withoutExtent]);
        Assert.False(unlocated.HasGeometry);
        Assert.Contains("map is disabled", unlocated.MapReason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No coordinates", unlocated.MapReason, StringComparison.Ordinal);

        // A box that is not a box (min above max) is not geometry.
        var crossed = Run("FINAL", "Final", inReference: true, minLat: 26.4m, maxLat: 26.1m, minLong: 50.1m, maxLong: 50.6m);
        Assert.False(Survey(processing: [crossed]).HasGeometry);
    }

    [Fact]
    public void The_no_data_loading_notice_is_a_constant()
    {
        // AC-3
        Assert.False(string.IsNullOrWhiteSpace(OgSeismicRegister.NoDataLoadingNotice));
        Assert.Contains("not loaded or interpreted", OgSeismicRegister.NoDataLoadingNotice, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("3D", "3D")]
    [InlineData("3d", "3D")]
    [InlineData("THREE_D", "3D")]
    [InlineData("2D", "2D")]
    [InlineData("4D", "4D")]
    [InlineData(" 2D ", "2D")]
    [InlineData("5D", "5D")]
    [InlineData("", "unspecified")]
    [InlineData(null, "unspecified")]
    public void The_dimension_is_read_as_the_register_kind(string? dimension, string expected)
    {
        Assert.Equal(expected, OgSeismicRegister.KindOf(dimension));
    }

    [Fact]
    public async Task A_line_read_needs_a_survey_and_a_connection()
    {
        var register = new OgSeismicRegister(() => null!, new Quiet());

        // AC-2
        Assert.Equal(
            OgSeismicRegister.CodeSurveyRequired,
            (await register.LoadLinesAsync(" ")).Reference);

        Assert.Equal(
            OgSeismicRegister.CodeNoConnection,
            (await register.LoadLinesAsync("S-1")).Reference);

        Assert.Equal(
            OgSeismicRegister.CodeNoConnection,
            (await register.LoadSurveysAsync("FIELD-1", "North")).Reference);
    }

    private static OgSeismicSurvey Survey(
        DateTime? start = null,
        DateTime? end = null,
        IReadOnlyList<OgSeismicProcessing>? processing = null) =>
        new(
            "S-1",
            "North 3D",
            "3D",
            "FIELD-1",
            "FIELD",
            start,
            end,
            "FINAL",
            "Final",
            true,
            processing ?? []);

    private static OgSeismicProcessing Run(
        string? code,
        string? label,
        bool inReference,
        decimal? minLat = null,
        decimal? maxLat = null,
        decimal? minLong = null,
        decimal? maxLong = null) =>
        new("P-1", code, label, inReference, "Vendor", "PSTM", new DateTime(2026, 3, 1), null,
            minLat, maxLat, minLong, maxLong);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
