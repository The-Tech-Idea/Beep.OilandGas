using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Integrity;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;

/// <summary>One pressure reading of a test, as recorded (ENG-04).</summary>
public sealed record OgPressureReading(decimal Pressure, string Unit, DateTime At);

/// <summary>
/// The classification of one recorded test (ENG-04's <c>OgClassification</c>): the API RP 90 class, the rule that decided it, the
/// limits it was judged against and what to do.
/// </summary>
/// <param name="Severity">
/// CRITICAL, WARNING or INFO. A pressure above the MAASP is always CRITICAL - the feature states that it is never a warning,
/// whatever the API class the bleed-down produced.
/// </param>
public sealed record OgClassification(
    string Class,
    string Rule,
    string Action,
    string Severity,
    double? Mawop,
    double? Maasp,
    double? Pressure,
    double? Exceedance,
    bool Complete,
    IReadOnlyList<string> Departures)
{
    public bool IsCritical => string.Equals(Severity, CasingPressureRules.Critical, StringComparison.Ordinal);

    public string LimitsText =>
        "MAWOP " + (Mawop?.ToString("0", CultureInfo.InvariantCulture) ?? "not computed") + " psi · MAASP " +
        (Maasp?.ToString("0", CultureInfo.InvariantCulture) ?? "not computed") + " psi";

    public string Describe() =>
        Class + " · " + Severity.ToLowerInvariant() + " · " + LimitsText +
        (Exceedance is { } exceedance
            ? " · " + exceedance.ToString("0", CultureInfo.InvariantCulture) + " psi above the MAASP"
            : string.Empty) +
        (Complete ? string.Empty : " · incomplete") + Environment.NewLine + Rule + Environment.NewLine + Action;
}

/// <summary>One annulus with its limits and its latest state (ENG-04).</summary>
public sealed record OgAnnulusView(
    string Name,
    string InnerString,
    string OuterString,
    string Fluid,
    double? FluidDensity,
    string? FluidDensityUnit,
    double? ShoeFractureGradient,
    string? ShoeFractureGradientUnit,
    double? WellheadRating,
    string? WellheadRatingUnit,
    double? Mawop,
    string? MawopGoverning,
    IReadOnlyList<string> MawopTerms,
    IReadOnlyList<string> MawopNotes,
    double? Maasp,
    string? MaaspGoverning,
    IReadOnlyList<string> MaaspLimits,
    IReadOnlyList<string> MaaspNotes,
    double? CurrentPressure,
    string? CurrentPressureUnit,
    DateTime? CurrentAt,
    string StateText)
{
    /// <summary>Whether a classification can be computed: the MAWOP is the limit API RP 90 judges a test against.</summary>
    public bool CanClassify => Mawop is not null;

    /// <summary>The limits with their bases, which is what the feature asks to be shown (AC-2).</summary>
    public string LimitsText
    {
        get
        {
            var text = new List<string>();

            text.Add(Mawop is { } mawop
                ? "MAWOP " + mawop.ToString("0.#", CultureInfo.InvariantCulture) + " psi, governed by " +
                  (MawopGoverning ?? "the lowest term")
                : "MAWOP not computed" + (MawopNotes.Count == 0 ? string.Empty : ": " + string.Join(" ", MawopNotes)));

            text.Add(Maasp is { } maasp
                ? "MAASP " + maasp.ToString("0.#", CultureInfo.InvariantCulture) + " psi, governed by " +
                  (MaaspGoverning ?? "the lowest limit")
                : "MAASP not computed" + (MaaspNotes.Count == 0 ? string.Empty : ": " + string.Join(" ", MaaspNotes)));

            text.AddRange(MawopTerms.Select(term => "  MAWOP term: " + term));
            text.AddRange(MaaspLimits.Select(limit => "  MAASP limit: " + limit));

            return string.Join(Environment.NewLine, text);
        }
    }

    public string PressureText => CurrentPressure is { } pressure
        ? pressure.ToString("0.#", CultureInfo.InvariantCulture) + " " + (CurrentPressureUnit ?? "psi") + " on " +
          (CurrentAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date")
        : "no pressure reading is recorded";

    public string Describe() =>
        Name + " · " + InnerString + " / " + OuterString + " · " + Fluid + " · " + PressureText + " · " + StateText;
}

/// <summary>One recorded test in the history (ENG-04's <c>OgCasingPressureTest</c> summary).</summary>
public sealed record OgPressureTestRow(
    string Id,
    string Annulus,
    DateTime At,
    double? DurationHours,
    double? InitialPressure,
    double? MaxPressure,
    string? PressureUnit,
    double? Temperature,
    string? TemperatureUnit,
    string? BleedOff,
    string Class,
    string Severity,
    bool Complete,
    DateTime RecordedAt,
    string? RecordedBy)
{
    public string StateText => Class + " · " + Severity.ToLowerInvariant() + (Complete ? string.Empty : " · incomplete");

    public string Describe() =>
        At.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " · " +
        (MaxPressure?.ToString("0.#", CultureInfo.InvariantCulture) ?? "?") + " " + (PressureUnit ?? "psi") +
        (DurationHours is { } hours ? " · " + hours.ToString("0.#", CultureInfo.InvariantCulture) + " h" : " · no duration") +
        " · " + StateText;
}

/// <summary>The casing-pressure workspace (ENG-04).</summary>
public sealed record OgCasingPressureWorkspace(
    string WellId,
    IReadOnlyList<OgAnnulusView> Annuli,
    OgAnnulusView? Selected,
    IReadOnlyList<OgPressureTestRow> History,
    OgClassification? Classification,
    IReadOnlyList<string> Warnings,
    DateTime ReadAt)
{
    public string CountsText =>
        Annuli.Count.ToString(CultureInfo.InvariantCulture) + " annulus/annuli · " +
        History.Count.ToString(CultureInfo.InvariantCulture) + " recorded test(s)";

    public string AnnuliText => Annuli.Count == 0
        ? "No annulus is recorded for this well, so there is nothing to monitor."
        : string.Join(Environment.NewLine, Annuli.Select(annulus => annulus.Describe()));

    public string HistoryText => History.Count == 0
        ? "No casing-pressure test is recorded for this annulus."
        : string.Join(Environment.NewLine, History.Select(test => test.Describe()));

    public string ClassificationText => Classification?.Describe() ??
        "No classification: record a test to classify the annulus.";

    public string WarningsText => Warnings.Count == 0
        ? "Nothing was left out."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The casing-pressure rules ENG-04 states once.</summary>
public static class CasingPressureRules
{
    public const string CodeNoConnection = "casing-pressure-no-connection";
    public const string CodeReadFailed = "casing-pressure-read-failed";
    public const string CodeWriteFailed = "casing-pressure-write-failed";
    public const string CodeWellRequired = "casing-pressure-well-required";
    public const string CodeAnnulusRequired = "casing-pressure-annulus-required";
    public const string CodeAnnulusUnknown = "casing-pressure-annulus-unknown";
    public const string CodeNoMawop = "casing-pressure-no-mawop";
    public const string CodeNoReadings = "casing-pressure-no-readings";
    public const string CodeActorRequired = "casing-pressure-actor-required";
    public const string CodeNotConfigured = "casing-pressure-rule-not-configured";

    public const string Critical = "CRITICAL";
    public const string Warning = "WARNING";
    public const string Info = "INFO";

    public const string Unclassified = "UNCLASSIFIED";
    public const string BelowThreshold = "BELOW THRESHOLD";
    public const string Sustained = "SUSTAINED";
    public const string NotSustained = "NOT SUSTAINED";

    /// <summary>
    /// AC-2 and AC-6: the annulus's limits with the basis of each, from the settings the owner supplies.
    /// </summary>
    public static string TermText((string term, double psi) term) =>
        term.term + " = " + term.psi.ToString("0.#", CultureInfo.InvariantCulture) + " psi";

    public static string LimitText(MaaspLimit limit) =>
        limit.Name + " = " + limit.Psi.ToString("0.#", CultureInfo.InvariantCulture) + " psi (" + limit.Basis + ")";

    /// <summary>
    /// AC-1: the API RP 90 class, from the owner's finding. The class is the pressure state; the rule is the sentence the
    /// classification produced.
    /// </summary>
    public static string ClassOf(CasingPressureFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);

        if (!finding.AboveThreshold)
        {
            return BelowThreshold;
        }

        return finding.Sustained ? Sustained : NotSustained;
    }

    /// <summary>
    /// AC-3: the severity. A pressure above the MAASP is critical whatever the class says, because the pipe or the formation is
    /// outside its limit; a sustained pressure or a departure is a warning; everything else is information.
    /// </summary>
    public static (string Severity, double? Exceedance) SeverityOf(
        CasingPressureFinding finding,
        double? maasp)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var highest = Math.Max(finding.InitialPressure, finding.MaximumBuildUp);

        if (maasp is { } limit && highest > limit)
        {
            return (Critical, highest - limit);
        }

        if (finding.Sustained || finding.Departures.Count > 0)
        {
            return (Warning, null);
        }

        return (Info, null);
    }

    /// <summary>What to do, which follows the severity and then the class - never a bare label.</summary>
    public static string ActionOf(string severity, string classification, double? maasp)
    {
        if (string.Equals(severity, Critical, StringComparison.Ordinal))
        {
            return "Reduce the annular pressure below the MAASP" +
                   (maasp is { } limit ? " of " + limit.ToString("0.#", CultureInfo.InvariantCulture) + " psi" : string.Empty) +
                   " before continuing, and re-test after the bleed-off.";
        }

        if (string.Equals(classification, Sustained, StringComparison.Ordinal))
        {
            return "Sustained casing pressure: monitor, and plan a diagnostic test or a repair. API RP 90 calls for a risk " +
                   "assessment where the criteria are not met - not an automatic shutdown.";
        }

        if (string.Equals(classification, NotSustained, StringComparison.Ordinal))
        {
            return "The pressure did not rebuild after the bleed-down: continue to monitor on the next survey.";
        }

        return "Below the diagnostic threshold: monitor.";
    }

    /// <summary>AC-6: the classification is only possible with the settings the owner supplies.</summary>
    public static string? NotConfigured(CasingPressureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.DiagnosticThreshold.In("psi") <= 0d
            ? "the diagnostic threshold is not configured, so nothing decides whether a test is required"
            : settings.BleedHours <= 0d
                ? "the bleed-down allowance is not configured, so nothing decides whether the pressure bled off in time"
                : null;
    }
}

/// <summary>ENG-04: the annuli, their MAWOP and MAASP, and the API RP 90 classification of a recorded test.</summary>
public interface IOgCasingPressure
{
    /// <param name="annulusName">The annulus to open; the first one when omitted.</param>
    Task<OgResult<OgCasingPressureWorkspace>> LoadAsync(
        string wellId,
        string? annulusName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a test and the classification it produces. A test with no valve-closed time is accepted and marked incomplete.
    /// </summary>
    Task<OgResult<OgCasingPressureWorkspace>> RecordTestAsync(
        string wellId,
        string annulusName,
        DateTime bleedStartUtc,
        DateTime? valveClosedUtc,
        decimal temperature,
        string temperatureUnit,
        string bleedOff,
        IReadOnlyList<OgPressureReading> readings,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-04 over the owner's <c>WELL_TUBULAR</c> and <c>WELL_DIR_SRVY_STATION</c> plus the added <c>WELL_ANNULUS</c> and
/// <c>CASING_PRESSURE_TEST</c>, using the tools' <see cref="Mawop"/>, <see cref="MaaspScreening"/> and
/// <see cref="CasingPressureDiagnostic"/>.
/// </summary>
/// <remarks>
/// <para>
/// Nothing about the limits is chosen here: MAWOP is the API RP 90 rule the owner implements, MAASP is the owner's screening with
/// its own deratings, and every threshold in the classification (the diagnostic pressure, the bleed hours, the build-up window,
/// the zero tolerance) comes from <c>CasingPressureSettings</c>. The desktop adds the annulus records, the readings and the
/// severity rule the feature states.
/// </para>
/// <para>
/// The screening is a flag, not a design: the owner's own comment says so, and the page repeats it with the basis of every limit.
/// </para>
/// </remarks>
public sealed class OgCasingPressureService : IOgCasingPressure
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgCasingPressureService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgCasingPressureWorkspace>> LoadAsync(
        string wellId,
        string? annulusName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgCasingPressureWorkspace>.Failed(
                "Casing pressure belongs to one well; none is named.", CasingPressureRules.CodeWellRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCasingPressureWorkspace>.Failed(
                "No connection is open, so the casing pressure cannot be read.",
                CasingPressureRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, wellId.Trim(), annulusName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the casing pressure for " + wellId, "the annuli are not shown", FailureSeverity.Error);

            return OgResult<OgCasingPressureWorkspace>.Failed(
                "The casing pressure could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? CasingPressureRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgCasingPressureWorkspace>> RecordTestAsync(
        string wellId,
        string annulusName,
        DateTime bleedStartUtc,
        DateTime? valveClosedUtc,
        decimal temperature,
        string temperatureUnit,
        string bleedOff,
        IReadOnlyList<OgPressureReading> readings,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCasingPressureWorkspace>.Failed(
                "A recorded test records who made it.", CasingPressureRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgCasingPressureWorkspace>.Failed(
                "A casing-pressure test belongs to one well; none is named.", CasingPressureRules.CodeWellRequired);
        }

        if (string.IsNullOrWhiteSpace(annulusName))
        {
            return OgResult<OgCasingPressureWorkspace>.Failed(
                "A casing-pressure test belongs to one annulus; none is named.", CasingPressureRules.CodeAnnulusRequired);
        }

        if (readings.Count == 0)
        {
            return OgResult<OgCasingPressureWorkspace>.Failed(
                "A casing-pressure test needs at least one pressure reading.", CasingPressureRules.CodeNoReadings);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCasingPressureWorkspace>.Failed(
                "No connection is open, so the test cannot be recorded.", CasingPressureRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, wellId.Trim(), annulusName, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var workspace = read.Value;
            var annulus = workspace.Selected;

            if (annulus is null)
            {
                return OgResult<OgCasingPressureWorkspace>.Failed(
                    "Annulus '" + annulusName + "' is not recorded for " + workspace.WellId + ".",
                    CasingPressureRules.CodeAnnulusUnknown);
            }

            // The feature's state: a classification without a MAWOP is refused with the missing input.
            if (!annulus.CanClassify)
            {
                return OgResult<OgCasingPressureWorkspace>.Failed(
                    "The test cannot be classified because the MAWOP of annulus " + annulus.Name + " cannot be computed: " +
                    string.Join(" ", annulus.MawopNotes) + ". Record the pipe the annulus is bounded by.",
                    CasingPressureRules.CodeNoMawop);
            }

            var settings = IntegrityToolbox.Settings.CasingPressure;

            if (CasingPressureRules.NotConfigured(settings) is { } notConfigured)
            {
                return OgResult<OgCasingPressureWorkspace>.Failed(
                    "The test cannot be classified: " + notConfigured + ".",
                    CasingPressureRules.CodeNotConfigured);
            }

            var complete = valveClosedUtc is { } closed && closed > bleedStartUtc;
            var duration = complete ? (valveClosedUtc!.Value - bleedStartUtc).TotalHours : (double?)null;

            var highest = readings.Max(reading => (double)reading.Pressure);
            var initial = readings
                .Where(reading => reading.At <= bleedStartUtc)
                .Select(reading => (double?)reading.Pressure)
                .LastOrDefault() ?? (double)readings[0].Pressure;

            OgClassification classification;

            if (!complete)
            {
                // AC-4: accepted, and marked incomplete with the reason.
                classification = new OgClassification(
                    CasingPressureRules.Unclassified,
                    "The test records no valve-closed time, so the bleed-down and the build-up cannot be judged: a duration is " +
                    "required before API RP 90 says anything about this annulus.",
                    "Record the bleed-down window and re-test.",
                    CasingPressureRules.Info,
                    annulus.Mawop,
                    annulus.Maasp,
                    highest,
                    null,
                    Complete: false,
                    Departures: []);
            }
            else
            {
                var test = new CasingPressureTest
                {
                    Annulus = annulus.Name,
                    BleedStart = bleedStartUtc,
                    ValveClosed = valveClosedUtc!.Value,
                };

                foreach (var reading in readings)
                {
                    test.Readings.Add(new AnnulusPressureReading
                    {
                        Date = reading.At,
                        Pressure = new Quantity((double)reading.Pressure, string.IsNullOrWhiteSpace(reading.Unit) ? "psi" : reading.Unit.Trim()),
                    });
                }

                var finding = CasingPressureDiagnostic.Classify(test, annulus.Mawop);
                var classificationText = CasingPressureRules.ClassOf(finding);
                var (severity, overLimit) = CasingPressureRules.SeverityOf(finding, annulus.Maasp);

                classification = new OgClassification(
                    classificationText,
                    finding.Summary,
                    CasingPressureRules.ActionOf(severity, classificationText, annulus.Maasp),
                    severity,
                    annulus.Mawop,
                    annulus.Maasp,
                    Math.Max(finding.InitialPressure, finding.MaximumBuildUp),
                    overLimit,
                    Complete: true,
                    finding.Departures.ToList());
            }

            var now = DateTime.UtcNow;

            var unit = OgUow.For<CASING_PRESSURE_TEST>(source);
            unit.PrimaryKey = nameof(CASING_PRESSURE_TEST.TEST_ID);

            unit.Add(new CASING_PRESSURE_TEST
            {
                TEST_ID = Guid.NewGuid().ToString("N"),
                UWI = workspace.WellId,
                ANNULUS_NAME = annulus.Name,
                BLEED_START_UTC = bleedStartUtc,
                VALVE_CLOSED_UTC = complete ? valveClosedUtc : null,
                DURATION_HOURS = duration is { } hours ? (decimal)hours : null,
                READINGS_JSON = JsonSerializer.Serialize(readings.Select(reading => new
                {
                    at = reading.At,
                    pressure = reading.Pressure,
                    unit = reading.Unit,
                })),
                INITIAL_PRESSURE = (decimal)initial,
                MAX_PRESSURE = (decimal)highest,
                PRESSURE_OUOM = string.IsNullOrWhiteSpace(readings[0].Unit) ? "psi" : readings[0].Unit.Trim(),
                TEMPERATURE = temperature,
                TEMPERATURE_OUOM = string.IsNullOrWhiteSpace(temperatureUnit) ? null : temperatureUnit.Trim(),
                BLEED_OFF_TEXT = string.IsNullOrWhiteSpace(bleedOff) ? null : bleedOff.Trim(),
                CLASS_TEXT = classification.Class,
                RULE_TEXT = classification.Rule,
                ACTION_TEXT = classification.Action,
                SEVERITY = classification.Severity,
                MAWOP = classification.Mawop is { } mawop ? (decimal)mawop : null,
                MAASP = classification.Maasp is { } maasp ? (decimal)maasp : null,
                EXCEEDANCE = classification.Exceedance is { } exceedance ? (decimal)exceedance : null,
                COMPLETE = complete ? "Y" : "N",
                RECORDED_BY = actor.Trim(),
                RECORDED_AT_UTC = now,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgCasingPressureWorkspace>.Failed(
                    OgUow.Message(committed, "The test was not recorded."), CasingPressureRules.CodeWriteFailed);
            }

            return await ReadAsync(source, workspace.WellId, annulus.Name, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording a casing-pressure test on " + annulusName, "the test is not recorded", FailureSeverity.Error);

            return OgResult<OgCasingPressureWorkspace>.Failed(
                "The test could not be recorded" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? CasingPressureRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgCasingPressureWorkspace>> ReadAsync(
        IDataSource source,
        string wellId,
        string? annulusName,
        CancellationToken cancellationToken)
    {
        var tubulars = await OgUow.ReadAllAsync<WELL_TUBULAR>(source).ConfigureAwait(false);
        var annulusRows = await OgUow.ReadAllAsync<WELL_ANNULUS>(source).ConfigureAwait(false);
        var tests = await OgUow.ReadAllAsync<CASING_PRESSURE_TEST>(source).ConfigureAwait(false);
        var stations = await OgUow.ReadAllAsync<WELL_DIR_SRVY_STATION>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var model = BuildModel(wellId, tubulars, stations);
        var warnings = new List<string>();

        // AC-5: a pressure is only ever a recorded test's, never an interpolation.
        var latestByAnnulus = tests
            .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .Where(row => row.MAX_PRESSURE is not null)
            .GroupBy(row => (row.ANNULUS_NAME ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(row => row.RECORDED_AT_UTC).First(),
                StringComparer.OrdinalIgnoreCase);

        var recorded = annulusRows
            .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.ANNULUS_NAME, StringComparer.Ordinal)
            .ToList();

        var annuli = new List<OgAnnulusView>();

        if (recorded.Count == 0)
        {
            // No annulus is recorded: the owner can still name them from the strings that reach surface, and the page says the
            // fluid, the fracture gradient and the wellhead rating are absent rather than pretending they are not needed.
            var identified = Annuli.Identify(model.Strings
                .Where(str => str.Attributes.OuterDiameter is Quantity od && od.TryIn("in", out _))
                .Select(str => (str.Id, str.Top.TryIn("m", out var top) ? top : 0d, str.Attributes.OuterDiameter!.In("in"))));

            if (identified.Count == 0)
            {
                warnings.Add(
                    "No casing or tubing reaching surface is recorded for " + wellId +
                    ", so there is no annulus to monitor.");
            }
            else
            {
                warnings.Add(
                    "No annulus record is stored, so the annuli are named from the strings that reach surface: their fluid, " +
                    "fracture gradient and wellhead rating are unknown and their MAASP has only the pipe limits.");
            }

            foreach (var derived in identified)
            {
                annuli.Add(Build(model, derived, null, Latest(latestByAnnulus, derived.Name), warnings));
            }
        }
        else
        {
            foreach (var row in recorded)
            {
                var info = new AnnulusInfo
                {
                    Name = row.ANNULUS_NAME,
                    InnerStringId = row.INNER_STRING_ID,
                    OuterStringId = row.OUTER_STRING_ID,
                    Fluid = Enum.TryParse<AnnulusFluid>(row.FLUID?.Trim(), ignoreCase: true, out var fluid)
                        ? fluid
                        : AnnulusFluid.Unknown,
                    FluidDensity = row.FLUID_DENSITY is { } density
                        ? new Quantity((double)density, row.FLUID_DENSITY_OUOM ?? "ppg")
                        : null,
                    ShoeFractureGradient = row.SHOE_FRACTURE_GRADIENT is { } gradient
                        ? new Quantity((double)gradient, row.SHOE_FRACTURE_GRADIENT_OUOM ?? "psi/ft")
                        : null,
                    WellheadRating = row.WELLHEAD_RATING is { } rating
                        ? new Quantity((double)rating, row.WELLHEAD_RATING_OUOM ?? "psi")
                        : null,
                    Maasp = row.RECORDED_MAASP is { } maasp ? new Quantity((double)maasp, row.RECORDED_MAASP_OUOM ?? "psi") : null,
                    Mawop = row.RECORDED_MAWOP is { } mawop ? new Quantity((double)mawop, row.RECORDED_MAWOP_OUOM ?? "psi") : null,
                    Notes = row.NOTES,
                };

                annuli.Add(Build(model, info, row, Latest(latestByAnnulus, info.Name), warnings));
            }
        }

        var selected = string.IsNullOrWhiteSpace(annulusName)
            ? annuli.FirstOrDefault()
            : annuli.FirstOrDefault(annulus =>
                string.Equals(annulus.Name, annulusName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (selected is null && !string.IsNullOrWhiteSpace(annulusName) && annuli.Count > 0)
        {
            return OgResult<OgCasingPressureWorkspace>.Failed(
                "Annulus '" + annulusName!.Trim() + "' is not recorded for " + wellId + ".",
                CasingPressureRules.CodeAnnulusUnknown);
        }

        var mine = tests
            .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .Where(row => selected is null ||
                          string.Equals(row.ANNULUS_NAME?.Trim(), selected.Name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.BLEED_START_UTC)
            .Select(row => new OgPressureTestRow(
                row.TEST_ID,
                row.ANNULUS_NAME,
                row.BLEED_START_UTC,
                row.DURATION_HOURS is { } hours ? (double)hours : null,
                row.INITIAL_PRESSURE is { } initial ? (double)initial : null,
                row.MAX_PRESSURE is { } maximum ? (double)maximum : null,
                row.PRESSURE_OUOM,
                row.TEMPERATURE is { } temperature ? (double)temperature : null,
                row.TEMPERATURE_OUOM,
                row.BLEED_OFF_TEXT,
                row.CLASS_TEXT ?? CasingPressureRules.Unclassified,
                row.SEVERITY ?? CasingPressureRules.Info,
                string.Equals(row.COMPLETE?.Trim(), "Y", StringComparison.OrdinalIgnoreCase),
                row.RECORDED_AT_UTC,
                row.RECORDED_BY))
            .ToList();

        var latest = tests
            .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .Where(row => selected is null ||
                          string.Equals(row.ANNULUS_NAME?.Trim(), selected.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.RECORDED_AT_UTC)
            .FirstOrDefault();

        var classification = latest is null
            ? null
            : new OgClassification(
                latest.CLASS_TEXT ?? CasingPressureRules.Unclassified,
                latest.RULE_TEXT ?? string.Empty,
                latest.ACTION_TEXT ?? string.Empty,
                latest.SEVERITY ?? CasingPressureRules.Info,
                latest.MAWOP is { } mawopStored ? (double)mawopStored : null,
                latest.MAASP is { } maaspStored ? (double)maaspStored : null,
                latest.MAX_PRESSURE is { } pressure ? (double)pressure : null,
                latest.EXCEEDANCE is { } exceedance ? (double)exceedance : null,
                string.Equals(latest.COMPLETE?.Trim(), "Y", StringComparison.OrdinalIgnoreCase),
                string.IsNullOrWhiteSpace(latest.RULE_TEXT) ? [] : [latest.RULE_TEXT]);

        return OgResult<OgCasingPressureWorkspace>.Ok(new OgCasingPressureWorkspace(
            wellId,
            annuli,
            selected,
            mine,
            classification,
            warnings,
            DateTime.UtcNow));
    }

    /// <summary>
    /// One annulus with the MAWOP the owner computes and the MAASP its screening gives, each with the terms behind it (AC-2).
    /// </summary>
    private static OgAnnulusView Build(
        WellModel model,
        AnnulusInfo annulus,
        WELL_ANNULUS? row,
        (double Pressure, DateTime At)? latest,
        List<string> warnings)
    {
        var inner = model.Strings.FirstOrDefault(str => str.Id == annulus.InnerStringId);
        var outer = model.Strings.FirstOrDefault(str => str.Id == annulus.OuterStringId);

        var mawop = Mawop.For(model, annulus);
        var (maasp, maaspGoverning, maaspLimits, maaspNotes) = MaaspOf(model, annulus, inner, outer);

        var state = StateOf(mawop.Mawop, maasp, latest);

        if (mawop.Notes.Count > 0)
        {
            warnings.Add("Annulus " + annulus.Name + ": " + string.Join(" ", mawop.Notes));
        }

        return new OgAnnulusView(
            annulus.Name,
            inner?.Name ?? annulus.InnerStringId,
            outer?.Name ?? annulus.OuterStringId,
            annulus.Fluid.ToString().ToUpperInvariant(),
            annulus.FluidDensity is { } density ? density.Value : null,
            annulus.FluidDensity?.Uom,
            annulus.ShoeFractureGradient is { } gradient ? gradient.Value : null,
            annulus.ShoeFractureGradient?.Uom,
            annulus.WellheadRating is { } rating ? rating.Value : null,
            annulus.WellheadRating?.Uom,
            mawop.Mawop,
            mawop.Governing,
            mawop.Terms.Select(CasingPressureRules.TermText).ToList(),
            mawop.Notes.ToList(),
            maasp,
            maaspGoverning,
            maaspLimits,
            maaspNotes,
            latest?.Pressure,
            "psi",
            latest?.At,
            state);
    }

    /// <summary>AC-2 and AC-3: where the recorded pressure sits against the two limits.</summary>
    private static string StateOf(double? mawop, double? maasp, (double Pressure, DateTime At)? latest)
    {
        if (latest is not { } reading)
        {
            return "no pressure recorded";
        }

        if (maasp is { } ceiling && reading.Pressure > ceiling)
        {
            return "above the MAASP by " + (reading.Pressure - ceiling).ToString("0.#", CultureInfo.InvariantCulture) + " psi";
        }

        if (mawop is { } limit && reading.Pressure > limit)
        {
            return "above the MAWOP";
        }

        return mawop is null ? "no MAWOP to compare with" : "within the MAWOP";
    }

    /// <summary>
    /// The MAASP screening: the owner's own limits and deratings, with the pipe's API 5C3 ratings supplied as the attributes the
    /// screening reads. The formation and wellhead terms appear only when those inputs are recorded.
    /// </summary>
    private static (double? Maasp, string? Governing, IReadOnlyList<string> Limits, IReadOnlyList<string> Notes) MaaspOf(
        WellModel model,
        AnnulusInfo annulus,
        TubularString? inner,
        TubularString? outer)
    {
        var notes = new List<string>();

        if (inner is null || outer is null)
        {
            notes.Add("the annulus's inner or outer string is not in the model");

            return (null, null, [], notes);
        }

        var innerAttributes = new EngineeringAttributes
        {
            OuterDiameter = inner.Attributes.OuterDiameter,
            InnerDiameter = inner.Attributes.InnerDiameter,
            Grade = inner.Attributes.Grade,
        };

        var outerAttributes = new EngineeringAttributes
        {
            OuterDiameter = outer.Attributes.OuterDiameter,
            InnerDiameter = outer.Attributes.InnerDiameter,
            Grade = outer.Attributes.Grade,
        };

        var outerRating = CasingDesign.Rate(outer);
        var innerRating = CasingDesign.Rate(inner);

        if (outerRating.Burst is { } burst)
        {
            outerAttributes.BurstRating = new Quantity(burst, "psi");
        }
        else
        {
            notes.Add("no burst rating for " + outer.Name + ", so the outer-casing limit is absent");
        }

        if (innerRating.Collapse is { } collapse)
        {
            innerAttributes.CollapseRating = new Quantity(collapse, "psi");
        }
        else
        {
            notes.Add("no collapse rating for " + inner.Name + ", so the inner-string limit is absent");
        }

        var limits = MaaspScreening.Limits(annulus, innerAttributes, outerAttributes, outer.Bottom);

        if (limits.Count == 0)
        {
            notes.Add("no limit could be computed, so no MAASP is stated");
        }

        var governing = MaaspScreening.Governing(limits);

        if (annulus.ShoeFractureGradient is null)
        {
            notes.Add("no fracture gradient is recorded at the outer shoe, so the formation limit is absent");
        }

        if (annulus.WellheadRating is null)
        {
            notes.Add("no wellhead rating is recorded, so the wellhead limit is absent");
        }

        return (governing?.Psi, governing?.Name, limits.Select(CasingPressureRules.LimitText).ToList(), notes);
    }

    /// <summary>
    /// The well model the tools see: one string per recorded tubular row, identified by the key the annulus records reference, with
    /// its OD, ID, grade, weight and depth range, and the survey the shoe TVD needs.
    /// </summary>
    private static WellModel BuildModel(
        string wellId,
        IReadOnlyList<WELL_TUBULAR> tubulars,
        IReadOnlyList<WELL_DIR_SRVY_STATION> stations)
    {
        var model = new WellModel();
        model.Header.Name = wellId;

        foreach (var row in tubulars
                     .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                     .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(row => row.OUTSIDE_DIAMETER))
        {
            var unit = row.BASE_DEPTH_OUOM?.Trim() ?? "m";

            var str = new TubularString
            {
                Id = StringId(row),
                Name = string.IsNullOrWhiteSpace(row.TUBING_TYPE) ? "TUBULAR" : row.TUBING_TYPE!.Trim(),
                Kind = StringKind.Production,
                Top = new Quantity(row.HUNG_TOP_DEPTH > 0m ? (double)row.HUNG_TOP_DEPTH : 0d, unit),
                Bottom = new Quantity(row.SHOE_DEPTH > 0m ? (double)row.SHOE_DEPTH : (double)row.BASE_DEPTH, unit),
            };

            if (row.OUTSIDE_DIAMETER > 0m)
            {
                str.Attributes.OuterDiameter = new Quantity((double)row.OUTSIDE_DIAMETER, row.OUTSIDE_DIAMETER_OUOM ?? "in");
            }

            if (row.INSIDE_DIAMETER > 0m)
            {
                str.Attributes.InnerDiameter = new Quantity((double)row.INSIDE_DIAMETER, row.INSIDE_DIAMETER_OUOM ?? "in");
            }

            if (!string.IsNullOrWhiteSpace(row.TUBING_GRADE))
            {
                str.Attributes.Grade = row.TUBING_GRADE;
            }

            if (row.TUBING_WEIGHT > 0m)
            {
                str.Attributes.Weight = new Quantity((double)row.TUBING_WEIGHT, row.TUBING_WEIGHT_OUOM ?? "lbm/ft");
            }

            model.Strings.Add(str);
        }

        foreach (var row in stations
                     .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
                     .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(row => row.STATION_MD))
        {
            model.Survey.Add(new SurveyStation
            {
                Md = new Quantity((double)row.STATION_MD, row.STATION_MD_OUOM?.Trim() ?? "m"),
                Inclination = (double)row.INCLINATION,
                Azimuth = (double)row.AZIMUTH,
            });
        }

        return model;
    }

    private static (double Pressure, DateTime At)? Latest(
        IReadOnlyDictionary<string, CASING_PRESSURE_TEST> tests,
        string annulus) =>
        tests.TryGetValue(annulus, out var row) && row.MAX_PRESSURE is { } pressure
            ? ((double)pressure, row.BLEED_START_UTC)
            : null;

    /// <summary>The identity an annulus record references: the tubular row's own key.</summary>
    public static string StringId(WELL_TUBULAR row) =>
        (row.UWI ?? string.Empty).Trim() + "|" + (row.SOURCE ?? string.Empty).Trim() + "|" +
        (row.TUBING_TYPE ?? string.Empty).Trim() + "|" +
        row.TUBING_OBS_NO.ToString(CultureInfo.InvariantCulture);
}
