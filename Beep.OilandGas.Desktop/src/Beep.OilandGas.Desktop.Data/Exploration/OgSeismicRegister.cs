using System.Globalization;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

/// <summary>One seismic line of a survey (EXP-04 AC-2).</summary>
/// <remarks>
/// <c>SEIS_LINE</c> records the line's identity, dimension and method. Direction, length and shot count have no
/// column in this model, so they are null and the page shows "-" rather than a guessed figure.
/// </remarks>
public sealed record OgSeismicLine(
    string SurveyId,
    string Id,
    string? Name,
    string? Dimension,
    string? Method,
    decimal? Length,
    string? LengthUnit,
    int? ShotCount)
{
    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Name ?? Id,
                Dimension,
                Method,
                Length is { } length ? $"{length.ToString("0.###", CultureInfo.InvariantCulture)} {LengthUnit}".TrimEnd() : null,
                ShotCount is { } shots ? $"{shots.ToString(CultureInfo.InvariantCulture)} shot(s)" : null,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>One processing run of a survey (a <c>SEIS_PROC_SET</c>), with its reference-coded state.</summary>
public sealed record OgSeismicProcessing(
    string Id,
    string? StatusCode,
    string? StatusLabel,
    bool StatusInReference,
    string? Company,
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? MinLatitude,
    decimal? MaxLatitude,
    decimal? MinLongitude,
    decimal? MaxLongitude)
{
    /// <summary>True when the four bounds are recorded and form a real box.</summary>
    public bool HasExtent =>
        MinLatitude is { } minLat && MaxLatitude is { } maxLat && MinLongitude is { } minLong && MaxLongitude is { } maxLong
        && minLat <= maxLat && minLong <= maxLong;

    /// <summary>
    /// AC-5: the state is a reference code with its label; a code the reference table does not hold is named as
    /// such instead of being replaced by a made-up label.
    /// </summary>
    public string StatusText => string.IsNullOrWhiteSpace(StatusCode)
        ? "not recorded"
        : StatusInReference
            ? $"{StatusLabel ?? StatusCode} ({StatusCode})"
            : $"{StatusCode} (not in the reference table)";

    public string ExtentText => HasExtent
        ? $"lat {Number(MinLatitude)}..{Number(MaxLatitude)}, long {Number(MinLongitude)}..{Number(MaxLongitude)}"
        : "no coordinates";

    private static string Number(decimal? value) =>
        value?.ToString("0.####", CultureInfo.InvariantCulture) ?? "-";
}

/// <summary>One acquisition survey, its status and its processing runs.</summary>
public sealed record OgSeismicSurvey(
    string Id,
    string Name,
    string Kind,
    string? AreaId,
    string? AreaType,
    DateTime? StartDate,
    DateTime? EndDate,
    string? StatusCode,
    string? StatusLabel,
    bool StatusInReference,
    IReadOnlyList<OgSeismicProcessing> Processing)
{
    /// <summary>
    /// AC-1: the acquisition state derived from the recorded dates - planned, acquiring or acquired. It is a
    /// reading of START_DATE/COMPLETED_DATE, not a separate status field.
    /// </summary>
    public string AcquisitionState => StartDate is null
        ? "planned"
        : EndDate is null
            ? "acquiring"
            : "acquired";

    public string StatusText => string.IsNullOrWhiteSpace(StatusCode)
        ? "no status recorded"
        : StatusInReference
            ? $"{StatusLabel ?? StatusCode} ({StatusCode})"
            : $"{StatusCode} (not in the reference table)";

    /// <summary>AC-1: the latest processing run's reference-coded state, or that none is recorded.</summary>
    public string ProcessingState => Processing.Count == 0
        ? "no processing recorded"
        : Processing[0].StatusText;

    /// <summary>AC-4: the map is offered only when a coordinate box is recorded.</summary>
    public bool HasGeometry => Processing.Any(run => run.HasExtent);

    public string MapReason => HasGeometry
        ? Processing.First(run => run.HasExtent).ExtentText
        : "No coordinates are recorded for this survey, so the map is disabled.";

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Name,
                Kind,
                AreaId,
                AcquisitionState,
                StatusText,
                ProcessingState,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>EXP-04: the seismic register - surveys, their lines and their processing runs.</summary>
/// <remarks>
/// No seismic data is loaded or interpreted here: the register records metadata and the page says so (AC-3).
/// </remarks>
public interface IOgSeismicRegister
{
    Task<OgResult<IReadOnlyList<OgSeismicSurvey>>> LoadSurveysAsync(
        string? fieldId,
        string fieldName,
        CancellationToken cancellationToken = default);

    /// <summary>AC-2: only the lines of <paramref name="surveyId"/>.</summary>
    Task<OgResult<IReadOnlyList<OgSeismicLine>>> LoadLinesAsync(
        string surveyId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The register over <c>SEIS_ACQTN_SURVEY</c>, <c>SEIS_SET_STATUS</c> + <c>R_SEIS_STATUS</c>,
/// <c>SEIS_SET_COMPONENT</c> + <c>SEIS_PROC_SET</c> + <c>R_SEIS_PROC_STATUS</c> and <c>SEIS_LINE</c>, read through
/// their entity models.
/// </summary>
public sealed class OgSeismicRegister : IOgSeismicRegister
{
    public const string CodeNoConnection = "seismic-register-no-connection";
    public const string CodeReadFailed = "seismic-register-read-failed";
    public const string CodeSurveyRequired = "seismic-register-survey-required";

    /// <summary>AC-3: the sentence the page shows whenever it shows the register.</summary>
    public const string NoDataLoadingNotice =
        "This screen records survey metadata. Seismic data is not loaded or interpreted.";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgSeismicRegister(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<IReadOnlyList<OgSeismicSurvey>>> LoadSurveysAsync(
        string? fieldId,
        string fieldName,
        CancellationToken cancellationToken = default)
    {
        IDataSource? source;

        try
        {
            source = _source();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "opening the connection for the seismic register",
                "the register is not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgSeismicSurvey>>.Failed(
                "The seismic register could not be read.", reference ?? CodeNoConnection);
        }

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgSeismicSurvey>>.Failed(
                "No connection is open, so the seismic register cannot be read.", CodeNoConnection);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var surveys = await OgUow.ReadAllAsync<SEIS_ACQTN_SURVEY>(source).ConfigureAwait(false);
            var statuses = await OgUow.ReadAllAsync<SEIS_SET_STATUS>(source).ConfigureAwait(false);
            var statusRefs = await OgUow.ReadAllAsync<R_SEIS_STATUS>(source).ConfigureAwait(false);
            var components = await OgUow.ReadAllAsync<SEIS_SET_COMPONENT>(source).ConfigureAwait(false);
            var processingSets = await OgUow.ReadAllAsync<SEIS_PROC_SET>(source).ConfigureAwait(false);
            var processingRefs = await OgUow.ReadAllAsync<R_SEIS_PROC_STATUS>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var statusLabels = statusRefs
                .Where(row => !string.IsNullOrWhiteSpace(row.SEIS_STATUS))
                .GroupBy(row => row.SEIS_STATUS!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => Blank(group.First().LONG_NAME) ?? Blank(group.First().SHORT_NAME),
                    StringComparer.OrdinalIgnoreCase);

            var currentStatus = statuses
                .Where(row => !string.IsNullOrWhiteSpace(row.SEIS_SET_ID))
                .GroupBy(row => row.SEIS_SET_ID!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(row => row.EFFECTIVE_DATE).First(),
                    StringComparer.OrdinalIgnoreCase);

            var processingLabels = processingRefs
                .Where(row => !string.IsNullOrWhiteSpace(row.PROCESS_STATUS))
                .GroupBy(row => row.PROCESS_STATUS!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => Blank(group.First().LONG_NAME) ?? Blank(group.First().SHORT_NAME),
                    StringComparer.OrdinalIgnoreCase);

            var sets = processingSets
                .Where(row => !string.IsNullOrWhiteSpace(row.SEIS_PROC_SET_ID))
                .GroupBy(row => row.SEIS_PROC_SET_ID!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var runsBySurvey = components
                .Where(component => !string.IsNullOrWhiteSpace(component.SEIS_ACT_SEIS_SET_ID))
                .Select(component => (component.SEIS_ACT_SEIS_SET_ID!, Set: sets.TryGetValue(
                    component.SEIS_SET_ID ?? string.Empty, out var set) ? set : null))
                .Where(pair => pair.Set is not null)
                .GroupBy(pair => pair.Item1, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(pair => pair.Set!)
                        .OrderByDescending(set => set.START_DATE)
                        .ThenBy(set => set.SEIS_PROC_SET_ID, StringComparer.Ordinal)
                        .Select(set => MapProcessing(set, processingLabels))
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var listed = surveys
                .Where(survey => FieldMatches(survey.AREA_ID, fieldId, fieldName))
                .Where(survey => !string.IsNullOrWhiteSpace(survey.SEIS_ACQTN_SURVEY_ID))
                .OrderBy(survey => survey.ACQTN_SURVEY_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(survey => survey.SEIS_ACQTN_SURVEY_ID, StringComparer.Ordinal)
                .Select(survey =>
                {
                    var id = survey.SEIS_ACQTN_SURVEY_ID!;
                    statusLabels.TryGetValue(
                        currentStatus.TryGetValue(id, out var status) ? status.SEIS_STATUS ?? string.Empty : string.Empty,
                        out var statusLabel);
                    var code = Blank(currentStatus.TryGetValue(id, out var current) ? current.SEIS_STATUS : null);

                    return new OgSeismicSurvey(
                        id,
                        Blank(survey.ACQTN_SURVEY_NAME) ?? id,
                        KindOf(survey.SEIS_DIMENSION),
                        Blank(survey.AREA_ID),
                        Blank(survey.AREA_TYPE),
                        survey.START_DATE,
                        survey.COMPLETED_DATE,
                        code,
                        statusLabel,
                        statusLabel is not null,
                        runsBySurvey.TryGetValue(id, out var runs) ? runs : []);
                })
                .ToList();

            return OgResult<IReadOnlyList<OgSeismicSurvey>>.Ok(listed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the seismic register",
                "the register is not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgSeismicSurvey>>.Failed(
                "The seismic register could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<IReadOnlyList<OgSeismicLine>>> LoadLinesAsync(
        string surveyId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(surveyId))
        {
            return OgResult<IReadOnlyList<OgSeismicLine>>.Failed(
                "Choose a survey before reading its lines.", CodeSurveyRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgSeismicLine>>.Failed(
                "No connection is open, so the seismic lines cannot be read.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<SEIS_LINE>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("SEIS_ACQTN_SURVEY_ID", surveyId.Trim())],
                cancellationToken).ConfigureAwait(false);

            var lines = rows
                .Where(row => !string.IsNullOrWhiteSpace(row.SEIS_LINE_ID))
                .OrderBy(row => row.LINE_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.SEIS_LINE_ID, StringComparer.Ordinal)
                .Select(row => new OgSeismicLine(
                    Blank(row.SEIS_ACQTN_SURVEY_ID) ?? surveyId.Trim(),
                    row.SEIS_LINE_ID!,
                    Blank(row.LINE_NAME),
                    Blank(row.DIMENSION),
                    Blank(row.REFRACTION_REFLECTION),
                    Length: null,
                    LengthUnit: null,
                    ShotCount: null))
                .ToList();

            return OgResult<IReadOnlyList<OgSeismicLine>>.Ok(lines);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading the lines of survey {surveyId}",
                "the line list is not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgSeismicLine>>.Failed(
                "The seismic lines could not be read.", reference ?? CodeReadFailed);
        }
    }

    /// <summary>The PPDM dimension code as the register's kind, with the recorded value kept when it is not 2/3/4D.</summary>
    public static string KindOf(string? dimension)
    {
        if (string.IsNullOrWhiteSpace(dimension))
        {
            return "unspecified";
        }

        var trimmed = dimension.Trim();

        return trimmed.ToUpperInvariant() switch
        {
            "2D" or "TWO_D" or "TWO-D" => "2D",
            "3D" or "THREE_D" or "THREE-D" => "3D",
            "4D" or "FOUR_D" or "FOUR-D" => "4D",
            _ => trimmed,
        };
    }

    private static OgSeismicProcessing MapProcessing(
        SEIS_PROC_SET set,
        IReadOnlyDictionary<string, string?> labels)
    {
        var code = Blank(set.PROCESS_STATUS);
        labels.TryGetValue(code ?? string.Empty, out var label);

        return new OgSeismicProcessing(
            set.SEIS_PROC_SET_ID ?? string.Empty,
            code,
            label,
            StatusInReference: label is not null,
            Blank(set.PROCESSING_COMPANY),
            Blank(set.PROCESSING_NAME),
            set.START_DATE,
            set.END_DATE,
            Coordinate(set.MIN_LATITUDE),
            Coordinate(set.MAX_LATITUDE),
            Coordinate(set.MIN_LONGITUDE),
            Coordinate(set.MAX_LONGITUDE));
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
         && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>The PPDM entity stores a bound as a non-nullable decimal; zero means it was not recorded.</summary>
    private static decimal? Coordinate(decimal value) => value == 0 ? null : value;

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
