using System.Globalization;
using System.Text;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;

/// <summary>
/// The factor catalog this company may report from (HSE-06): the shared catalog for one jurisdiction, at one revision, as the
/// owner's own validation accepted it.
/// </summary>
/// <param name="WhyNot">Why the catalog is not usable, when it is not - the reason the report can be produced or not.</param>
public sealed record OgGhgCatalog(
    string? Jurisdiction,
    string? Revision,
    IReadOnlyList<GhgFactorDefinition> Factors,
    string? WhyNot)
{
    public bool IsPresent => Factors.Count > 0 && !string.IsNullOrWhiteSpace(Revision) && WhyNot is null;

    public DateTime? EffectiveFrom => Factors.Count == 0
        ? null
        : Factors.Min(factor => factor.EffectiveFrom.ToDateTime(TimeOnly.MinValue));

    public DateTime? EffectiveThrough => Factors.Count == 0
        ? null
        : Factors.Max(factor => factor.EffectiveThrough.ToDateTime(TimeOnly.MinValue));

    public string Describe() => IsPresent
        ? Jurisdiction + " · revision " + Revision + " · " +
          Factors.Count.ToString(CultureInfo.InvariantCulture) + " factor(s) · effective " +
          EffectiveFrom!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " to " +
          EffectiveThrough!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        : "no usable factor catalog: " + (WhyNot ?? "nothing is recorded for this jurisdiction");
}

/// <summary>
/// The company's review of the catalog it reports from (HSE-06, AC-3). The state is derived by comparing the revisions the
/// company recorded when it selected the catalog with the revisions that hold now.
/// </summary>
public sealed record OgGhgReview(
    string State,
    string Sentence,
    string? CatalogRevision,
    string? SelectedCatalogRevision,
    string? SelectedEnvironmentRevision,
    string? CurrentEnvironmentRevision,
    string? SelectedBy)
{
    public bool IsCurrent => string.Equals(State, GhgRules.ReviewCurrent, StringComparison.Ordinal);
}

/// <summary>One recorded activity row the report can be built from (HSE-06).</summary>
public sealed record OgGhgActivity(
    string Subtype,
    string EntityId,
    string Source,
    string? Name,
    string VolumeMethod,
    string ActivityType,
    string ProductType,
    string PeriodType,
    decimal Volume,
    string Unit,
    DateTime? Start,
    DateTime? End,
    decimal Observation,
    decimal Amendment)
{
    public string Entity => string.IsNullOrWhiteSpace(Name) ? Subtype + "/" + EntityId : Name!;

    public string PeriodText => Start is { } start
        ? start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
          (End is { } end ? " to " + end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty)
        : "no recorded period";

    public string Describe() =>
        Entity + " · " + ActivityType + " · " + GhgRules.Figure(Volume) + " " + Unit + " · " + PeriodText;
}

/// <summary>
/// One report line (HSE-06, AC-2): the activity, its quantity and unit, the factor applied with its own unit, the source and
/// the catalog revision it came from, and the emissions that multiplication produced.
/// </summary>
public sealed record OgGhgLine(
    string Activity,
    string Unit,
    decimal Quantity,
    decimal Factor,
    string FactorUnit,
    string Revision,
    decimal Emissions,
    string FactorId,
    string SourceReference)
{
    public string Describe() =>
        Activity + " · " + GhgRules.Figure(Quantity) + " " + Unit + " × " +
        GhgRules.Figure(Factor) + " " + FactorUnit + " = " + GhgRules.Figure(Emissions) + " " + FactorUnit +
        " · factor " + FactorId + " · revision " + Revision + " · source " + SourceReference;
}

/// <summary>Emissions in one unit (HSE-06). A catalog can hold factors with different output units, and they are not summed.</summary>
public sealed record OgGhgTotal(string Unit, decimal Emissions)
{
    public string Describe() => GhgRules.Figure(Emissions) + " " + Unit;
}

/// <summary>The report (HSE-06, <c>OgGhgReport</c>).</summary>
public sealed record OgGhgReport(
    int Year,
    IReadOnlyList<OgGhgLine> Lines,
    IReadOnlyList<OgGhgTotal> Totals,
    string CatalogRevision,
    string ReviewState)
{
    /// <summary>The total in the catalog's single output unit, or null when the catalog mixes units.</summary>
    public decimal? TotalCo2e => Totals.Count == 1 ? Totals[0].Emissions : null;

    public string TotalText => Totals.Count == 0
        ? "no line produced an emission"
        : string.Join(" · ", Totals.Select(total => total.Describe())) +
          (Totals.Count > 1 ? " (the catalog's factors use more than one output unit, so these are not summed)" : string.Empty);

    public string LinesText => Lines.Count == 0
        ? "No line."
        : string.Join(Environment.NewLine, Lines.Select(line => line.Describe()));

    /// <summary>
    /// AC-6: the export is built from the same line values with the same formatting, so it cannot disagree with the screen.
    /// </summary>
    public string Csv()
    {
        var text = new StringBuilder();

        text.AppendLine("activity,unit,quantity,factor,factor_unit,revision,emissions,factor_id,source");
        text.AppendLine("year," + Year.ToString(CultureInfo.InvariantCulture));

        foreach (var line in Lines)
        {
            text.AppendLine(string.Join(",",
                Quote(line.Activity),
                Quote(line.Unit),
                GhgRules.Figure(line.Quantity),
                GhgRules.Figure(line.Factor),
                Quote(line.FactorUnit),
                Quote(line.Revision),
                GhgRules.Figure(line.Emissions),
                Quote(line.FactorId),
                Quote(line.SourceReference)));
        }

        foreach (var total in Totals)
        {
            text.AppendLine("total," + Quote(total.Unit) + "," + GhgRules.Figure(total.Emissions));
        }

        return text.ToString();
    }

    private static string Quote(string? value) =>
        value is null ? "\"\"" : "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

/// <summary>The GHG workspace (HSE-06).</summary>
public sealed record OgGhgWorkspace(
    OgHseScope Scope,
    int Year,
    string? Jurisdiction,
    OgGhgCatalog Catalog,
    OgGhgReview Review,
    IReadOnlyList<OgGhgActivity> Activity,
    OgGhgReport? Report,
    IReadOnlyList<string> Unavailable,
    DateTime ReadAt)
{
    public bool IsAvailable => Report is not null;

    /// <summary>AC-1 and AC-5: an unavailable report says why, and never prints a zero total.</summary>
    public string StatusText => IsAvailable
        ? "report produced"
        : "report unavailable - not zero";

    public string UnavailableText => IsAvailable
        ? string.Empty
        : "The report is unavailable: " + string.Join(" ", Unavailable);

    public string TotalText => Report?.TotalText ?? "unavailable";

    public string LinesText => Report?.LinesText ?? "unavailable";

    public string ExportText => Report?.Csv() ?? "unavailable";

    public string ScopeText => "field " + (Scope.FieldId ?? "(none)") +
                               " · year " + Year.ToString(CultureInfo.InvariantCulture) +
                               " · jurisdiction " + (string.IsNullOrWhiteSpace(Jurisdiction)
                                   ? "not specified"
                                   : Jurisdiction);

    public string ActivityText => Activity.Count == 0
        ? "no recorded activity for this field and year"
        : string.Join(Environment.NewLine, Activity.Select(row => row.Describe()));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The GHG rules HSE-06 states once, so the service, the page and the tests cannot disagree.</summary>
public static class GhgRules
{
    public const string CodeNoConnection = "ghg-no-connection";
    public const string CodeReadFailed = "ghg-read-failed";
    public const string CodeActorRequired = "ghg-actor-required";
    public const string CodeInputRequired = "ghg-input-required";
    public const string CodeFieldRequired = "ghg-field-required";
    public const string CodeJurisdictionRequired = "ghg-jurisdiction-required";
    public const string CodeRationaleRequired = "ghg-rationale-required";
    public const string CodeWriteFailed = "ghg-write-failed";
    public const string CodeNoEnvironment = "ghg-no-environment";

    /// <summary>Why a report is unavailable (HSE-06's states, each as its own reason).</summary>
    public const string ReasonNoFactors = "no usable factor catalog is recorded for the jurisdiction";
    public const string ReasonNoActivity = "no verified activity is recorded for this field and year";
    public const string ReasonNotReviewed = "the company has not reviewed a catalog for this jurisdiction";
    public const string ReasonCatalogChanged = "the catalog has changed since the company reviewed it";
    public const string ReasonEnvironmentChanged = "the company environment has changed since the catalog was reviewed";
    public const string ReasonNoEnvironment = "no company environment is recorded to review the catalog against";

    /// <summary>The review states.</summary>
    public const string ReviewCurrent = "CURRENT";
    public const string ReviewNotReviewed = "NOT_REVIEWED";
    public const string ReviewCatalogChanged = "CATALOG_CHANGED";
    public const string ReviewEnvironmentChanged = "ENVIRONMENT_CHANGED";
    public const string ReviewNoEnvironment = "NO_ENVIRONMENT";

    /// <summary>The table and categories the owner uses for the shared factor catalog.</summary>
    public const string FactorCategory = "GHG_FACTORS";

    /// <summary>
    /// The factor that applies to one activity: its category must match the activity type, its input unit the recorded unit,
    /// and its effective period must cover the recorded period. Nothing is chosen by approximation.
    /// </summary>
    public static GhgFactorDefinition? ApplicableFactor(
        IReadOnlyList<GhgFactorDefinition> factors,
        string? activityType,
        string? unit,
        DateTime? start,
        DateTime? end)
    {
        ArgumentNullException.ThrowIfNull(factors);

        if (string.IsNullOrWhiteSpace(activityType) || string.IsNullOrWhiteSpace(unit) || start is null || end is null)
        {
            return null;
        }

        var from = DateOnly.FromDateTime(start.Value);
        var through = DateOnly.FromDateTime(end.Value);

        return factors.FirstOrDefault(factor =>
            string.Equals(factor.SourceCategory, activityType.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(factor.InputUnit, unit.Trim(), StringComparison.OrdinalIgnoreCase) &&
            factor.EffectiveFrom <= from && factor.EffectiveThrough >= through);
    }

    /// <summary>
    /// AC-3: the review state, from the revisions the company recorded when it selected the catalog and the revisions that
    /// hold now. A difference in either direction means the catalog must be reviewed again before it is used.
    /// </summary>
    public static (string State, string Sentence) ReviewOf(
        COMPANY_GHG_CATALOG_SELECTION? selection,
        string? catalogRevision,
        string? environmentRevision)
    {
        if (string.IsNullOrWhiteSpace(environmentRevision))
        {
            return (ReviewNoEnvironment, ReasonNoEnvironment + ".");
        }

        if (selection is null)
        {
            return (ReviewNotReviewed, ReasonNotReviewed + ".");
        }

        var selectedCatalog = selection.CATALOG_REVISION?.Trim();
        var selectedEnvironment = selection.ENVIRONMENT_REVISION?.Trim();

        if (!string.Equals(selectedCatalog, catalogRevision?.Trim(), StringComparison.Ordinal))
        {
            return (ReviewCatalogChanged,
                "The catalog has changed since the company reviewed it: the selection recorded revision " +
                (selectedCatalog ?? "(none)") + " and the catalog is now at " + (catalogRevision ?? "(none)") +
                ". Review it again before reporting.");
        }

        if (!string.Equals(selectedEnvironment, environmentRevision.Trim(), StringComparison.Ordinal))
        {
            return (ReviewEnvironmentChanged,
                "The company environment has changed since the catalog was reviewed: the selection recorded revision " +
                (selectedEnvironment ?? "(none)") + " and the environment is now at " + environmentRevision +
                ". Review the catalog again before reporting.");
        }

        return (ReviewCurrent,
            "The catalog is reviewed: the selection records this catalog revision (" + selectedCatalog +
            ") and this environment revision (" + selectedEnvironment + ").");
    }

    /// <summary>
    /// One number, formatted the same way for the screen and the export (AC-6), with no rounding that the recorded values did
    /// not already carry.
    /// </summary>
    public static string Figure(decimal value) =>
        value.ToString("0.############################", CultureInfo.InvariantCulture);

    /// <summary>Validates a catalog selection (HSE-06's review and select command).</summary>
    public static (string Code, string Message)? ValidateSelection(string? fieldId, string? jurisdiction, string? rationale)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
        {
            return (CodeFieldRequired, "A report is produced for one field, so a catalog is selected for one field.");
        }

        if (string.IsNullOrWhiteSpace(jurisdiction))
        {
            return (CodeJurisdictionRequired,
                "A catalog is selected for a jurisdiction. None is assumed from the field, the company or a default country.");
        }

        return string.IsNullOrWhiteSpace(rationale)
            ? (CodeRationaleRequired,
                "Selecting a catalog needs its rationale, so the choice can be reviewed later.")
            : null;
    }
}

/// <summary>HSE-06: the factor catalog, its review state and the emissions report.</summary>
public interface IOgGhg
{
    /// <summary>
    /// Reads the catalog, the company's review of it, the field's recorded activity and - only when all three are usable - the
    /// report. A report that cannot be produced is null with every reason stated, never a zero total (AC-1, AC-5).
    /// </param>
    Task<OgResult<OgGhgWorkspace>> LoadAsync(
        OgHseScope scope,
        int year,
        string? jurisdiction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews and selects the catalog for this company and jurisdiction, recording the catalog and environment revisions it
    /// was reviewed against (AC-3). The reviewed revisions are the whole review record in this build.
    /// </summary>
    Task<OgResult<OgGhgWorkspace>> SelectAsync(
        OgHseScope scope,
        int year,
        string jurisdiction,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HSE-06 over the owner's <c>SYSTEM_SETUP_RECORD</c> (the shared <c>GHG_FACTORS</c> catalog), <c>COMPANY_GHG_CATALOG_SELECTION</c>
/// (the company's reviewed revision), <c>COMPANY_OILGAS_ENVIRONMENT</c> (the environment revision it was reviewed against), and
/// <c>PDEN</c>/<c>PDEN_VOL_DISPOSITION</c> (the field's recorded activity).
/// </summary>
/// <remarks>
/// <para>
/// The owner's <c>GHGReportingService</c> is a refusal stub - every method throws "GHG reporting requires verified
/// company-scoped activity data and emission factors" - so the report is built here from the recorded rows, using the owner's
/// own <c>GhgFactorCatalogValidation</c> to validate the catalog and <c>GhgArithmetic.Check</c> to multiply a volume by a
/// factor. Both are the owner's rules, not a second implementation of them.
/// </para>
/// <para>
/// Nothing is defaulted: the jurisdiction chooses the catalog and is refused when absent, a missing due date or period is not
/// filled in, and a factor is used only when its category, input unit and effective period all match the recorded activity.
/// </para>
/// </remarks>
public sealed class OgGhg : IOgGhg
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgGhg(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgGhgWorkspace>> LoadAsync(
        OgHseScope scope,
        int year,
        string? jurisdiction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(scope.FieldId))
        {
            return OgResult<OgGhgWorkspace>.Failed(
                "A GHG report is produced for one field; none is selected.",
                GhgRules.CodeFieldRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgGhgWorkspace>.Failed(
                "No connection is open, so the GHG report cannot be read.", GhgRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, scope, year, jurisdiction, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgGhgWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the GHG report", "the GHG report is not shown; no total is inferred",
                FailureSeverity.Error);

            return OgResult<OgGhgWorkspace>.Failed(
                "The GHG report could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? GhgRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgGhgWorkspace>> SelectAsync(
        OgHseScope scope,
        int year,
        string jurisdiction,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgGhgWorkspace>.Failed(
                "A catalog selection must record who made it.", GhgRules.CodeActorRequired);
        }

        if (GhgRules.ValidateSelection(scope.FieldId, jurisdiction, rationale) is { } refusal)
        {
            return OgResult<OgGhgWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgGhgWorkspace>.Failed(
                "No connection is open, so the catalog cannot be selected.", GhgRules.CodeNoConnection);
        }

        try
        {
            var catalog = await ReadCatalogAsync(source, jurisdiction.Trim(), cancellationToken)
                .ConfigureAwait(false);

            if (!catalog.IsPresent)
            {
                return OgResult<OgGhgWorkspace>.Failed(
                    "The catalog cannot be selected because it is not usable: " +
                    (catalog.WhyNot ?? "nothing is recorded for " + jurisdiction) + ".",
                    GhgRules.ReasonNoFactors);
            }

            var environments = await OgUow.ReadAllAsync<COMPANY_OILGAS_ENVIRONMENT>(source).ConfigureAwait(false);

            var environment = environments.FirstOrDefault(row =>
                !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(row.OWNER_COMPANY_BA_ID?.Trim(), scope.CompanyId, StringComparison.OrdinalIgnoreCase));

            if (environment is null || string.IsNullOrWhiteSpace(environment.REVISION))
            {
                return OgResult<OgGhgWorkspace>.Failed(
                    GhgRules.ReasonNoEnvironment + ", so there is no revision to review the catalog against.",
                    GhgRules.CodeNoEnvironment);
            }

            var selections = await OgUow.ReadAllAsync<COMPANY_GHG_CATALOG_SELECTION>(source).ConfigureAwait(false);

            var existing = selections.FirstOrDefault(row =>
                string.Equals(row.OWNER_COMPANY_BA_ID?.Trim(), scope.CompanyId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(row.JURISDICTION_KEY?.Trim(), jurisdiction.Trim(), StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            var revision = Guid.NewGuid().ToString("N");
            var id = existing?.COMPANY_GHG_CATALOG_SELECTION_ID ?? IdFor(scope.CompanyId, jurisdiction.Trim());

            var unit = OgUow.For<COMPANY_GHG_CATALOG_SELECTION>(source);
            unit.PrimaryKey = nameof(COMPANY_GHG_CATALOG_SELECTION.COMPANY_GHG_CATALOG_SELECTION_ID);

            if (existing is null)
            {
                unit.Add(new COMPANY_GHG_CATALOG_SELECTION
                {
                    COMPANY_GHG_CATALOG_SELECTION_ID = id,
                    OWNER_TENANT_ID = scope.CompanyId,
                    OWNER_COMPANY_BA_ID = scope.CompanyId,
                    JURISDICTION_KEY = jurisdiction.Trim(),
                    CATALOG_REVISION = catalog.Revision!,
                    ENVIRONMENT_REVISION = environment.REVISION.Trim(),
                    SELECTED_BY = actor.Trim(),
                    RATIONALE = rationale.Trim(),
                    REVISION = revision,
                    INTENT_ID = revision,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }
            else
            {
                existing.CATALOG_REVISION = catalog.Revision!;
                existing.ENVIRONMENT_REVISION = environment.REVISION.Trim();
                existing.SELECTED_BY = actor.Trim();
                existing.RATIONALE = rationale.Trim();
                existing.PREVIOUS_REVISION = existing.REVISION;
                existing.REVISION = revision;
                existing.INTENT_ID = revision;
                existing.ROW_CHANGED_BY = actor.Trim();
                existing.ROW_CHANGED_DATE = now;

                var updated = unit.Update(existing);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgGhgWorkspace>.Failed(
                        OgUow.Message(updated, "The catalog selection was not saved."), GhgRules.CodeWriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgGhgWorkspace>.Failed(
                    OgUow.Message(committed, "The catalog selection was not saved."), GhgRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, scope, year, jurisdiction.Trim(), cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgGhgWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "selecting the GHG factor catalog", "the catalog selection was not saved",
                FailureSeverity.Error);

            return OgResult<OgGhgWorkspace>.Failed(
                "The catalog could not be selected" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? GhgRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgGhgWorkspace> ReadAsync(
        IDataSource source,
        OgHseScope scope,
        int year,
        string? jurisdiction,
        CancellationToken cancellationToken)
    {
        var catalog = await ReadCatalogAsync(source, jurisdiction, cancellationToken).ConfigureAwait(false);

        var environments = await OgUow.ReadAllAsync<COMPANY_OILGAS_ENVIRONMENT>(source).ConfigureAwait(false);
        var selections = await OgUow.ReadAllAsync<COMPANY_GHG_CATALOG_SELECTION>(source).ConfigureAwait(false);
        var entities = await OgUow.ReadAllAsync<PDEN>(source).ConfigureAwait(false);
        var dispositions = await OgUow.ReadAllAsync<PDEN_VOL_DISPOSITION>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var environment = environments.FirstOrDefault(row =>
            !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(row.OWNER_COMPANY_BA_ID?.Trim(), scope.CompanyId, StringComparison.OrdinalIgnoreCase));

        var selection = selections.FirstOrDefault(row =>
            !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(row.OWNER_COMPANY_BA_ID?.Trim(), scope.CompanyId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(row.JURISDICTION_KEY?.Trim(), jurisdiction?.Trim(), StringComparison.OrdinalIgnoreCase));

        var (state, sentence) = GhgRules.ReviewOf(selection, catalog.Revision, environment?.REVISION);

        var review = new OgGhgReview(
            state,
            sentence,
            catalog.Revision,
            selection?.CATALOG_REVISION,
            selection?.ENVIRONMENT_REVISION,
            environment?.REVISION,
            selection?.SELECTED_BY);

        // The field's own production entities, then the dispositions recorded against them for the year.
        var mine = entities
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.FIELD_ID?.Trim(), scope.FieldId!.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var names = mine
            .GroupBy(row => (row.PDEN_SUBTYPE, row.PDEN_ID, row.SOURCE))
            .ToDictionary(group => group.Key, group => group.First().PDEN_LONG_NAME);

        var activity = dispositions
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => names.ContainsKey((row.PDEN_SUBTYPE, row.PDEN_ID, row.PDEN_SOURCE)))
            .Where(row => InYear(row, year))
            .Select(row => new OgGhgActivity(
                row.PDEN_SUBTYPE,
                row.PDEN_ID,
                row.PDEN_SOURCE,
                names.TryGetValue((row.PDEN_SUBTYPE, row.PDEN_ID, row.PDEN_SOURCE), out var name) ? name : null,
                row.VOLUME_METHOD ?? string.Empty,
                row.ACTIVITY_TYPE ?? string.Empty,
                row.PRODUCT_TYPE ?? string.Empty,
                row.PERIOD_TYPE ?? string.Empty,
                row.VOLUME,
                row.VOLUME_UOM ?? string.Empty,
                row.VOLUME_START_DATE,
                row.VOLUME_END_DATE,
                row.DISPOSITION_OBS_NO,
                row.AMENDMENT_SEQ_NO))
            .OrderBy(row => row.Entity, StringComparer.Ordinal)
            .ThenBy(row => row.ActivityType, StringComparer.Ordinal)
            .ThenBy(row => row.Observation)
            .ThenBy(row => row.Amendment)
            .ToList();

        var unavailable = new List<string>();

        // AC-1: no usable factors, no report - and the reason, not a zero.
        if (!catalog.IsPresent)
        {
            unavailable.Add(catalog.WhyNot ?? GhgRules.ReasonNoFactors);
        }

        // AC-3: an unreviewed or out-of-date catalog cannot be reported from.
        if (!review.IsCurrent)
        {
            unavailable.Add(sentence);
        }

        // AC-1: no verified activity, no report.
        if (activity.Count == 0)
        {
            unavailable.Add(GhgRules.ReasonNoActivity);
        }

        var lines = new List<OgGhgLine>();

        if (unavailable.Count == 0)
        {
            foreach (var row in activity)
            {
                var factor = GhgRules.ApplicableFactor(
                    catalog.Factors, row.ActivityType, row.Unit, row.Start, row.End);

                if (factor is null)
                {
                    unavailable.Add(
                        "no factor applies to " + row.Entity + " (" + row.ActivityType + ", " + row.Unit +
                        ", " + row.PeriodText + "): the catalog has no factor for that category, unit and period.");
                    continue;
                }

                var fact = new GhgDispositionFact(
                    row.VolumeMethod, row.ActivityType, row.ProductType, row.PeriodType,
                    row.Observation, row.Amendment, row.Volume, row.Unit, row.Start, row.End);

                try
                {
                    // The owner's own arithmetic: checked multiplication, with its own refusal message.
                    var check = GhgArithmetic.Check(fact, factor);

                    lines.Add(new OgGhgLine(
                        row.Entity + " · " + row.ActivityType + " · " + row.PeriodText,
                        row.Unit,
                        row.Volume,
                        factor.Factor,
                        check.OutputUnit,
                        catalog.Revision!,
                        check.Result,
                        factor.Id,
                        factor.SourceReference));
                }
                catch (Exception ex) when (ex is OverflowException or InvalidOperationException)
                {
                    // AC-4: an unrepresentable value is refused, and the refusal says which activity it was.
                    var reference = _failures.ReportHandled(
                        ex, "computing emissions for " + row.Entity,
                        "the line is refused and the report is not produced", FailureSeverity.Error);

                    unavailable.Add(
                        "the emissions for " + row.Entity + " cannot be represented within the supported calculation " +
                        "range, so the report is refused" +
                        (string.IsNullOrWhiteSpace(reference) ? string.Empty : " (reference " + reference + ")"));
                }
            }
        }

        OgGhgReport? report = null;

        if (unavailable.Count == 0)
        {
            var totals = lines
                .GroupBy(line => line.FactorUnit, StringComparer.OrdinalIgnoreCase)
                .Select(group => new OgGhgTotal(group.First().FactorUnit, group.Sum(line => line.Emissions)))
                .OrderBy(total => total.Unit, StringComparer.Ordinal)
                .ToList();

            report = new OgGhgReport(year, lines, totals, catalog.Revision!, review.State);
        }

        return new OgGhgWorkspace(
            scope,
            year,
            string.IsNullOrWhiteSpace(jurisdiction) ? null : jurisdiction.Trim(),
            catalog,
            review,
            activity,
            report,
            unavailable,
            DateTime.UtcNow);
    }

    private async Task<OgGhgCatalog> ReadCatalogAsync(
        IDataSource source,
        string? jurisdiction,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(jurisdiction))
        {
            return new OgGhgCatalog(null, null, [],
                "no jurisdiction is selected, so no factor catalog applies");
        }

        var records = await OgUow.ReadAllAsync<SYSTEM_SETUP_RECORD>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var row = records
            .Where(record => !string.Equals(record.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(record => string.Equals(record.CATEGORY?.Trim(), GhgRules.FactorCategory,
                StringComparison.OrdinalIgnoreCase))
            .Where(record => string.Equals(record.SETUP_KEY?.Trim(), jurisdiction.Trim(), StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (row is null || string.IsNullOrWhiteSpace(row.SETTINGS_JSON))
        {
            return new OgGhgCatalog(jurisdiction.Trim(), null, [],
                "no factor catalog is recorded for jurisdiction " + jurisdiction.Trim());
        }

        try
        {
            // The owner's own validation: 1-256 factors, non-negative values, no overlapping periods, explicit sources.
            var catalog = GhgFactorCatalogValidation.Parse(jurisdiction.Trim(), row.SETTINGS_JSON);

            return new OgGhgCatalog(
                catalog.Jurisdiction,
                row.REVISION,
                catalog.Factors.ToList(),
                null);
        }
        catch (Exception ex) when (ex is ArgumentException or System.Text.Json.JsonException
                                       or NotSupportedException)
        {
            // A malformed catalog is a data defect this screen cannot read; it is reported and becomes the reason the report
            // is unavailable rather than an exception on the page.
            var reference = _failures.ReportHandled(
                ex, "parsing the GHG factor catalog for " + jurisdiction,
                "the catalog is not used, so no report is produced", FailureSeverity.Error);

            return new OgGhgCatalog(jurisdiction.Trim(), row.REVISION, [],
                "the recorded factor catalog does not satisfy the catalog contract (its factors, units, effective dates or " +
                "sources are incomplete or overlap)" +
                (string.IsNullOrWhiteSpace(reference) ? string.Empty : " (reference " + reference + ")"));
        }
    }

    private static bool InYear(PDEN_VOL_DISPOSITION row, int year)
    {
        if (row.VOLUME_START_DATE is { } start)
        {
            return start.Year == year ||
                   (row.VOLUME_END_DATE is { } finish && finish.Year >= year && start.Year <= year);
        }

        return row.VOLUME_END_DATE is { } only && only.Year == year;
    }

    private static string IdFor(string companyId, string jurisdiction)
    {
        var id = "GHG-" + companyId + "-" + jurisdiction;

        return id.Length <= 64 ? id : id[..64];
    }
}
