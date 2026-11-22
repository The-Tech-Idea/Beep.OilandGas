using System.Globalization;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;

/// <summary>The reserves categories and the unit rules RES-05 uses (RES-05).</summary>
public static class ReserveUnits
{
    public const string Proved = "PROVED";
    public const string Probable = "PROBABLE";
    public const string Possible = "POSSIBLE";

    public static IReadOnlyList<string> Categories { get; } = [Proved, Probable, Possible];

    /// <summary>The units a reserve entry may be stated in; totals are never converted between them.</summary>
    public static IReadOnlyList<string> Units { get; } = ["bbl", "Mbbl", "MMbbl", "scf", "MMscf", "Bcf"];

    public static bool IsCategory(string? category) =>
        !string.IsNullOrWhiteSpace(category)
        && Categories.Contains(category.Trim(), StringComparer.OrdinalIgnoreCase);

    public static bool IsUnit(string? unit) =>
        !string.IsNullOrWhiteSpace(unit)
        && Units.Contains(unit.Trim(), StringComparer.OrdinalIgnoreCase);

    public static string? Canonical(string? category) =>
        IsCategory(category)
            ? Categories.First(candidate => string.Equals(candidate, category!.Trim(), StringComparison.OrdinalIgnoreCase))
            : null;

    public static string Describe(decimal? value, string? unit) =>
        value is { } amount
            ? amount.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + (unit ?? string.Empty).Trim()
            : "(not recorded)";
}

/// <summary>One reserves entry as the screen shows it (RES-05), matching the plan's <c>OgReserveEntry</c>.</summary>
public sealed record OgReserveEntry(
    string Id,
    string PoolId,
    string? PoolName,
    string? FieldId,
    string? Category,
    decimal? Gross,
    decimal? Net,
    string? Unit,
    DateTime? EffectiveDate,
    int Revision,
    string? Classifier,
    string? SupersedesEntryId,
    string? BasisText,
    string? Actor,
    DateTime? RecordedAt)
{
    /// <summary>AC-1: an entry with no category is unclassified and stays out of the totals.</summary>
    public bool IsUnclassified => string.IsNullOrWhiteSpace(Category);

    /// <summary>AC-5: an entry with no classifier flags the disclosure as blocked.</summary>
    public bool IsUnclassifiedByPerson => string.IsNullOrWhiteSpace(Classifier);

    public string CategoryText => IsUnclassified ? "unclassified" : Category!.ToLowerInvariant();

    public string GrossText => ReserveUnits.Describe(Gross, Unit);

    public string NetText => ReserveUnits.Describe(Net, Unit);

    public string EffectiveText =>
        EffectiveDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "no effective date";

    public string ClassifierText => IsUnclassifiedByPerson ? "no classifier recorded" : Classifier!.Trim();

    public string RevisionText => "revision " + Revision.ToString(CultureInfo.InvariantCulture);

    public string SupersedesText => string.IsNullOrWhiteSpace(SupersedesEntryId)
        ? "first entry"
        : "supersedes " + SupersedesEntryId;

    public string Describe() =>
        $"{PoolName ?? PoolId} · {CategoryText} · gross {GrossText} · net {NetText} · {EffectiveText} · " +
        $"{RevisionText} · {ClassifierText} · {SupersedesText}";
}

/// <summary>One category's total, summed only within one unit (RES-05).</summary>
public sealed record OgReserveTotal(
    string Category,
    int Count,
    decimal? Gross,
    decimal? Net,
    string? Unit,
    IReadOnlyList<string> MixedUnits)
{
    public bool HasMixedUnits => MixedUnits.Count > 0;

    public string GrossText => HasMixedUnits ? "mixed units, so no total" : ReserveUnits.Describe(Gross, Unit);

    public string NetText => HasMixedUnits ? "mixed units, so no total" : ReserveUnits.Describe(Net, Unit);

    public string Describe() =>
        $"{Category.ToLowerInvariant()}: {Count.ToString(CultureInfo.InvariantCulture)} entry/entries · " +
        $"gross {GrossText} · net {NetText}";
}

/// <summary>One characterization fact with the record it came from (RES-05 AC-4).</summary>
public sealed record OgCharacterizationFact(string Fact, string Value, string Source)
{
    public string Describe() => $"{Fact}: {Value} (source: {Source})";
}

/// <summary>One prospect in the maturation funnel, from the prospect volume estimate (RES-05).</summary>
public sealed record OgMaturationRow(
    string ProspectId,
    decimal? OilP50,
    decimal? GasP50,
    decimal? RiskedOil,
    decimal? RiskedGas,
    string? Unit,
    string? Method,
    string? Estimator,
    DateTime? EstimateDate)
{
    public string VolumeText =>
        ReserveUnits.Describe(OilP50 ?? GasP50, Unit) +
        (RiskedOil is { } risked ? " · risked " + ReserveUnits.Describe(risked, Unit) : string.Empty);

    public string Describe() =>
        $"{ProspectId} · {VolumeText} · {Method ?? "(no method)"} · {Estimator ?? "(no estimator)"} · " +
        $"{EstimateDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(no date)"}";
}

/// <summary>The reserves workspace for one field (RES-05).</summary>
public sealed record OgReserveWorkspace(
    string FieldName,
    string? FieldId,
    DateTime AsOf,
    /// <summary>Every entry, current revision and history; the totals and the gate use the current ones only.</summary>
    IReadOnlyList<OgReserveEntry> Entries,
    IReadOnlyList<OgReserveTotal> Totals,
    int UnclassifiedCount,
    decimal? UnclassifiedGross,
    decimal? UnclassifiedNet,
    string? UnclassifiedUnit,
    IReadOnlyList<OgReserveEntry> MissingClassifier,
    IReadOnlyList<OgCharacterizationFact> Characterization,
    IReadOnlyList<OgMaturationRow> Maturation,
    IReadOnlyList<OgDisclosure> Disclosures,
    string Preview,
    bool DisclosureBlocked,
    string DisclosureBlockedReason,
    string UnitsText,
    bool HistoryFailed,
    string? HistoryReason)
{
    public int CurrentRevisionCount => Entries.Count(entry => entry.Revision > 1);

    public bool HasEntries => Entries.Count > 0;

    /// <summary>AC-1: the excluded entries are named, never silently dropped from the totals.</summary>
    public string UnclassifiedText => UnclassifiedCount == 0
        ? "every entry carries a category"
        : $"{UnclassifiedCount.ToString(CultureInfo.InvariantCulture)} entry/entries are unclassified and excluded " +
          $"from the totals (gross {ReserveUnits.Describe(UnclassifiedGross, UnclassifiedUnit)}, " +
          $"net {ReserveUnits.Describe(UnclassifiedNet, UnclassifiedUnit)})";

    /// <summary>AC-5: the disclosure gate, stated with the entries that block it.</summary>
    public string DisclosureStateText => DisclosureBlocked
        ? "disclosure blocked: " + DisclosureBlockedReason
        : "disclosure available: every entry names a classifier";

    public string Summary =>
        $"{Entries.Count.ToString(CultureInfo.InvariantCulture)} entry/entries · " +
        $"{Totals.Count.ToString(CultureInfo.InvariantCulture)} categor(ies) with totals · " +
        $"{UnclassifiedCount.ToString(CultureInfo.InvariantCulture)} unclassified · " +
        $"{MissingClassifier.Count.ToString(CultureInfo.InvariantCulture)} without a classifier";

    public string CharacterizationText => Characterization.Count == 0
        ? "no characterization fact is recorded for this field"
        : string.Join(Environment.NewLine, Characterization.Select(fact => fact.Describe()));

    public string MaturationText => Maturation.Count == 0
        ? "no prospect volume estimate is recorded"
        : string.Join(Environment.NewLine, Maturation.Select(row => row.Describe()));

    public string EntriesText => Entries.Count == 0
        ? "no reserves entry is recorded for this field"
        : string.Join(Environment.NewLine, Entries.Select(entry => entry.Describe()));

    public string TotalsText => Totals.Count == 0
        ? "no classified entry, so there is no total"
        : string.Join(Environment.NewLine, Totals.Select(total => total.Describe()));

    public string DisclosuresText => Disclosures.Count == 0
        ? "no disclosure has been approved for this field"
        : string.Join(Environment.NewLine, Disclosures.Select(disclosure => disclosure.Describe()));
}

/// <summary>One approved disclosure (RES-05).</summary>
public sealed record OgDisclosure(
    string Id,
    DateTime Period,
    decimal? GrossTotal,
    decimal? NetTotal,
    string? Unit,
    int EntryCount,
    int ExcludedCount,
    string? PreparedBy,
    string? ApprovedBy,
    DateTime? ApprovedAt)
{
    public string PeriodText => Period.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    public string Describe() =>
        $"{PeriodText} · gross {ReserveUnits.Describe(GrossTotal, Unit)} · " +
        $"net {ReserveUnits.Describe(NetTotal, Unit)} · " +
        $"{EntryCount.ToString(CultureInfo.InvariantCulture)} entry/entries · " +
        $"{ExcludedCount.ToString(CultureInfo.InvariantCulture)} excluded · " +
        $"prepared by {PreparedBy ?? "(unknown)"} · approved by {ApprovedBy ?? "(unknown)"}";
}

/// <summary>The entry values, as the page records them (RES-05).</summary>
public sealed record OgReserveEntryValues(
    string? PoolId,
    string? Category,
    decimal? Gross,
    decimal? Net,
    string? Unit,
    DateTime? EffectiveDate,
    string? Classifier,
    string? BasisText);

/// <summary>The reserves rules, stated once so the page, the service and the tests agree.</summary>
public static class ReserveRules
{
    public const string CodeNoConnection = "reserves-no-connection";
    public const string CodeReadFailed = "reserves-read-failed";
    public const string CodeWriteFailed = "reserves-write-failed";
    public const string CodeActorRequired = "reserves-actor-required";
    public const string CodePoolRequired = "reserves-pool-required";
    public const string CodePoolUnknown = "reserves-pool-unknown";
    public const string CodeCategoryInvalid = "reserves-category-invalid";
    public const string CodeGrossInvalid = "reserves-gross-invalid";
    public const string CodeNetInvalid = "reserves-net-invalid";
    public const string CodeNetAboveGross = "reserves-net-above-gross";
    public const string CodeUnitRequired = "reserves-unit-required";
    public const string CodeUnitInvalid = "reserves-unit-invalid";
    public const string CodeEffectiveDateRequired = "reserves-effective-date-required";
    public const string CodeDisclosureBlocked = "reserves-disclosure-blocked";
    public const string CodeApproverIsPreparer = "reserves-approver-is-preparer";
    public const string CodeNothingToDisclose = "reserves-nothing-to-disclose";

    /// <summary>
    /// Validation. A category is optional (an unclassified entry is allowed and excluded by AC-1) and so is a
    /// classifier (its absence flags the entry and blocks the disclosure by AC-5); everything else is required.
    /// </summary>
    public static (string Code, string Message)? Validate(OgReserveEntryValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(values.PoolId))
        {
            return (CodePoolRequired, "A reserves entry belongs to a pool.");
        }

        if (!string.IsNullOrWhiteSpace(values.Category) && !ReserveUnits.IsCategory(values.Category))
        {
            return (CodeCategoryInvalid,
                $"'{values.Category}' is not a reserves category; use {string.Join(", ", ReserveUnits.Categories)} " +
                "or leave it empty for an unclassified entry.");
        }

        if (values.Gross is null or < 0m)
        {
            return (CodeGrossInvalid, "A reserves entry records a gross volume of zero or more.");
        }

        if (values.Net is null or < 0m)
        {
            return (CodeNetInvalid, "A reserves entry records a net volume of zero or more.");
        }

        if (values.Net > values.Gross)
        {
            return (CodeNetAboveGross,
                $"The net volume {values.Net} is above the gross volume {values.Gross}; net is the share of gross.");
        }

        if (string.IsNullOrWhiteSpace(values.Unit))
        {
            return (CodeUnitRequired,
                $"A volume carries its unit; use {string.Join(", ", ReserveUnits.Units)}.");
        }

        if (!ReserveUnits.IsUnit(values.Unit))
        {
            return (CodeUnitInvalid,
                $"'{values.Unit}' is not a reserves unit; use {string.Join(", ", ReserveUnits.Units)}.");
        }

        return values.EffectiveDate is null
            ? (CodeEffectiveDateRequired, "A reserves entry records the date it is effective from.")
            : null;
    }

    /// <summary>
    /// AC-1: totals are summed only within one unit per category, and a category whose entries are stated in more than
    /// one unit reports no total rather than a silently converted one.
    /// </summary>
    public static IReadOnlyList<OgReserveTotal> Totals(IReadOnlyList<OgReserveEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return entries
            .Where(entry => !entry.IsUnclassified)
            .GroupBy(entry => ReserveUnits.Canonical(entry.Category) ?? entry.Category!, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => Array.IndexOf(ReserveUnits.Categories.ToArray(), group.Key))
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var units = group
                    .Select(entry => entry.Unit?.Trim())
                    .Where(unit => !string.IsNullOrWhiteSpace(unit))
                    .Select(unit => unit!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(unit => unit, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (units.Count > 1)
                {
                    return new OgReserveTotal(group.Key, group.Count(), null, null, null, units);
                }

                return new OgReserveTotal(
                    group.Key,
                    group.Count(),
                    group.Sum(entry => entry.Gross ?? 0m),
                    group.Sum(entry => entry.Net ?? 0m),
                    units.Count == 0 ? null : units[0],
                    []);
            })
            .ToList();
    }

    /// <summary>
    /// AC-5: the disclosure is blocked while any entry lacks a classifier, and the reason names them.
    /// </summary>
    public static string BlockedReason(IReadOnlyList<OgReserveEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var missing = entries.Where(entry => entry.IsUnclassifiedByPerson).ToList();

        return missing.Count == 0
            ? string.Empty
            : $"{missing.Count.ToString(CultureInfo.InvariantCulture)} entry/entries carry no classifier " +
              $"({string.Join(", ", missing.Select(entry => entry.PoolName ?? entry.PoolId).Distinct(StringComparer.OrdinalIgnoreCase))})";
    }

    /// <summary>AC-3: only the newest revision of a pool and category is current; the rest are history.</summary>
    public static IReadOnlyList<OgReserveEntry> Current(IReadOnlyList<OgReserveEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return entries
            .GroupBy(entry => (entry.PoolId.ToUpperInvariant(), (entry.Category ?? string.Empty).ToUpperInvariant()))
            .Select(group => group.OrderByDescending(entry => entry.Revision).First())
            .OrderBy(entry => entry.PoolName ?? entry.PoolId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.CategoryText, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

/// <summary>RES-05: reserves classification and reservoir characterization.</summary>
public interface IOgReserves
{
    Task<OgResult<OgReserveWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? poolId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Records a new revision; the previous one is kept (AC-3).</summary>
    Task<OgResult<OgReserveWorkspace>> AddRevisionAsync(
        OgReserveEntryValues values,
        string actor,
        string? fieldName = null,
        CancellationToken cancellationToken = default);

    /// <summary>Approves the disclosure for a period; blocked while an entry lacks a classifier (AC-5).</summary>
    Task<OgResult<OgReserveWorkspace>> ApproveDisclosureAsync(
        string? fieldId,
        DateTime period,
        string actor,
        string? fieldName = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// RES-05 over the LifeCycle <c>RESERVE_ENTRY</c> and <c>RESERVE_DISCLOSURE</c> rows, the PPDM <c>FIELD</c> and
/// <c>POOL</c> rows, the DevelopmentPlanning <c>DEVELOPMENT_POOL_PLAN</c> (drive mechanism and OOIP), the LifeCycle
/// <c>RESERVOIR_STATUS</c> rows and the ProspectIdentification <c>PROSPECT_VOLUME_ESTIMATE</c> rows, through a unit of
/// work.
/// </summary>
/// <remarks>
/// A revision is an append: the service only ever adds, and the superseded row stays. Every characterization fact
/// carries the record it came from, including the facts nothing records (AC-4) - pressure and temperature have no table
/// in this repository, and the screen says so rather than leaving them out.
/// </remarks>
public sealed class OgReservesService : IOgReserves
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgReservesService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgReserveWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? poolId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgReserveWorkspace>.Failed(
                "No connection is open, so the reserves cannot be read.", ReserveRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgReserveWorkspace>.Ok(Build(model, fieldId, fieldName, poolId, DateTime.Today));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the reserves",
                "the reserves are not shown",
                FailureSeverity.Error);

            return OgResult<OgReserveWorkspace>.Failed(
                "The reserves could not be read.", reference ?? ReserveRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgReserveWorkspace>> AddRevisionAsync(
        OgReserveEntryValues values,
        string actor,
        string? fieldName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgReserveWorkspace>.Failed(
                "A reserves entry must record who made it.", ReserveRules.CodeActorRequired);
        }

        if (ReserveRules.Validate(values) is { } refusal)
        {
            return OgResult<OgReserveWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReserveWorkspace>.Failed(
                "No connection is open, so the entry cannot be saved.", ReserveRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var poolId = values.PoolId!.Trim();

            var pool = model.Pools.FirstOrDefault(row =>
                string.Equals(row.POOL_ID, poolId, StringComparison.OrdinalIgnoreCase));

            if (pool is null)
            {
                return OgResult<OgReserveWorkspace>.Failed(
                    $"Pool '{poolId}' is not in this database.", ReserveRules.CodePoolUnknown);
            }

            var category = ReserveUnits.Canonical(values.Category);

            // The revision continues this pool and category's chain; nothing is ever rewritten.
            var previous = model.Entries
                .Where(row => string.Equals(row.POOL_ID, poolId, StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(
                    ReserveUnits.Canonical(row.CATEGORY) ?? string.Empty,
                    category ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.REVISION)
                .FirstOrDefault();

            var now = DateTime.UtcNow;
            var unit = OgUow.For<RESERVE_ENTRY>(source);
            unit.PrimaryKey = "ENTRY_ID";

            unit.Add(new RESERVE_ENTRY
            {
                ENTRY_ID = Guid.NewGuid().ToString("N"),
                POOL_ID = poolId,
                FIELD_ID = pool.FIELD_ID,
                CATEGORY = category,
                GROSS_VOLUME = values.Gross,
                NET_VOLUME = values.Net,
                VOLUME_OUOM = values.Unit!.Trim(),
                EFFECTIVE_DATE = values.EffectiveDate!.Value.Date,
                REVISION = (previous?.REVISION ?? 0) + 1,
                CLASSIFIER = string.IsNullOrWhiteSpace(values.Classifier) ? null : values.Classifier.Trim(),
                SUPERSEDES_ENTRY_ID = previous?.ENTRY_ID,
                BASIS_TEXT = string.IsNullOrWhiteSpace(values.BasisText) ? null : values.BasisText.Trim(),
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgReserveWorkspace>.Failed(
                    OgUow.Message(committed, "The entry was not saved."), ReserveRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgReserveWorkspace>.Ok(Build(after, pool.FIELD_ID, fieldName ?? string.Empty, poolId, DateTime.Today));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording a reserves revision for pool {values.PoolId}",
                "the entry was not saved",
                FailureSeverity.Error);

            return OgResult<OgReserveWorkspace>.Failed(
                "The entry could not be saved.", reference ?? ReserveRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgReserveWorkspace>> ApproveDisclosureAsync(
        string? fieldId,
        DateTime period,
        string actor,
        string? fieldName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgReserveWorkspace>.Failed(
                "A disclosure must record who approved it.", ReserveRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReserveWorkspace>.Failed(
                "No connection is open, so the disclosure cannot be approved.", ReserveRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var workspace = Build(model, fieldId, fieldName ?? string.Empty, null, period.Date);

            if (!workspace.HasEntries)
            {
                return OgResult<OgReserveWorkspace>.Failed(
                    "There is no reserves entry to disclose for this field.", ReserveRules.CodeNothingToDisclose);
            }

            // AC-5: the gate.
            if (workspace.DisclosureBlocked)
            {
                return OgResult<OgReserveWorkspace>.Failed(
                    "The disclosure is blocked: " + workspace.DisclosureBlockedReason,
                    ReserveRules.CodeDisclosureBlocked);
            }

            var current = ReserveRules.Current(workspace.Entries);

            // Separation of duties: the person who classified the entries does not approve their disclosure.
            var preparer = current
                .Select(entry => entry.Classifier)
                .Where(className => !string.IsNullOrWhiteSpace(className))
                .Select(className => className!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (preparer.Count == 1
                && string.Equals(preparer[0], actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgReserveWorkspace>.Failed(
                    $"{actor.Trim()} classified these entries, so they cannot also approve the disclosure; " +
                    "separation of duties needs a second person.",
                    ReserveRules.CodeApproverIsPreparer);
            }

            var unit = OgUow.For<RESERVE_DISCLOSURE>(source);
            unit.PrimaryKey = "DISCLOSURE_ID";

            var now = DateTime.UtcNow;

            unit.Add(new RESERVE_DISCLOSURE
            {
                DISCLOSURE_ID = Guid.NewGuid().ToString("N"),
                FIELD_ID = workspace.FieldId,
                PERIOD = new DateTime(period.Year, period.Month, 1),
                GROSS_TOTAL = workspace.Totals.Sum(total => total.Gross ?? 0m),
                NET_TOTAL = workspace.Totals.Sum(total => total.Net ?? 0m),
                VOLUME_OUOM = workspace.Totals
                    .Select(total => total.Unit)
                    .FirstOrDefault(unitName => !string.IsNullOrWhiteSpace(unitName)),
                ENTRY_COUNT = current.Count,
                EXCLUDED_COUNT = workspace.UnclassifiedCount,
                PREPARED_BY = preparer.Count == 0 ? null : string.Join(", ", preparer),
                APPROVED_BY = actor.Trim(),
                APPROVED_AT = now,
                STATE = "APPROVED",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgReserveWorkspace>.Failed(
                    OgUow.Message(committed, "The disclosure was not approved."), ReserveRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgReserveWorkspace>.Ok(
                Build(after, fieldId, fieldName ?? string.Empty, null, period.Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "approving a reserves disclosure",
                "the disclosure was not approved",
                FailureSeverity.Error);

            return OgResult<OgReserveWorkspace>.Failed(
                "The disclosure could not be approved.", reference ?? ReserveRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ build

    private static OgReserveWorkspace Build(
        ReserveModel model,
        string? fieldId,
        string fieldName,
        string? poolId,
        DateTime asOf)
    {
        var field = model.Fields.FirstOrDefault(row =>
            (!string.IsNullOrWhiteSpace(fieldId)
             && string.Equals(row.FIELD_ID, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(fieldName)
                && string.Equals(row.FIELD_NAME, fieldName.Trim(), StringComparison.OrdinalIgnoreCase)));

        var resolvedFieldId = field?.FIELD_ID ?? fieldId?.Trim();

        var pools = model.Pools
            .Where(row => string.IsNullOrWhiteSpace(resolvedFieldId)
                          || string.Equals(row.FIELD_ID, resolvedFieldId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var poolIds = pools
            .Select(row => row.POOL_ID?.Trim())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var poolNames = pools
            .Where(row => !string.IsNullOrWhiteSpace(row.POOL_ID))
            .GroupBy(row => row.POOL_ID!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => string.IsNullOrWhiteSpace(group.First().POOL_NAME) ? null : group.First().POOL_NAME!.Trim(),
                StringComparer.OrdinalIgnoreCase);

        var entries = model.Entries
            .Where(row => !string.IsNullOrWhiteSpace(row.POOL_ID) && poolIds.Contains(row.POOL_ID!))
            .Select(row => ToEntry(row, poolNames))
            .OrderBy(entry => entry.PoolName ?? entry.PoolId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.CategoryText, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(entry => entry.Revision)
            .ToList();

        var current = ReserveRules.Current(entries);
        var totals = ReserveRules.Totals(current);

        var unclassified = current.Where(entry => entry.IsUnclassified).ToList();
        var unclassifiedUnit = unclassified
            .Select(entry => entry.Unit)
            .FirstOrDefault(unitName => !string.IsNullOrWhiteSpace(unitName));

        var missingClassifier = current.Where(entry => entry.IsUnclassifiedByPerson).ToList();
        var blockedReason = ReserveRules.BlockedReason(current);

        var characterization = Characterize(model, pools, field, poolId);

        var maturation = model.ProspectEstimates
            .GroupBy(row => row.PROSPECT_ID ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Key.Length > 0)
            .Select(group => group.OrderByDescending(row => row.ESTIMATE_DATE ?? DateTime.MinValue).First())
            .OrderBy(row => row.PROSPECT_ID, StringComparer.OrdinalIgnoreCase)
            .Select(row => new OgMaturationRow(
                row.PROSPECT_ID ?? string.Empty,
                row.OIL_VOLUME_P50,
                row.GAS_VOLUME_P50,
                row.RISKED_OIL_VOLUME,
                row.RISKED_GAS_VOLUME,
                row.VOLUME_OUOM,
                row.ESTIMATE_METHOD,
                row.ESTIMATOR,
                row.ESTIMATE_DATE))
            .ToList();

        var disclosures = model.Disclosures
            .Where(row => string.IsNullOrWhiteSpace(resolvedFieldId)
                          || string.Equals(row.FIELD_ID, resolvedFieldId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.PERIOD)
            .ThenByDescending(row => row.APPROVED_AT)
            .Select(row => new OgDisclosure(
                row.DISCLOSURE_ID ?? string.Empty,
                row.PERIOD,
                row.GROSS_TOTAL,
                row.NET_TOTAL,
                row.VOLUME_OUOM,
                row.ENTRY_COUNT,
                row.EXCLUDED_COUNT,
                row.PREPARED_BY,
                row.APPROVED_BY,
                row.APPROVED_AT))
            .ToList();

        return new OgReserveWorkspace(
            string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
            resolvedFieldId,
            asOf,
            // Every row is shown, current revision and history, so a reader can see that a revision superseded
            // something rather than replaced it (AC-3). The totals and the disclosure gate read current only.
            entries,
            totals,
            unclassified.Count,
            unclassified.Count == 0 ? null : unclassified.Sum(entry => entry.Gross ?? 0m),
            unclassified.Count == 0 ? null : unclassified.Sum(entry => entry.Net ?? 0m),
            unclassifiedUnit,
            missingClassifier,
            characterization,
            maturation,
            disclosures,
            Preview(current, totals, unclassified, resolvedFieldId, asOf),
            blockedReason.Length > 0,
            blockedReason,
            string.Join(", ", ReserveUnits.Units),
            false,
            null);
    }

    /// <summary>
    /// AC-4: every fact names the record it came from. The two facts nothing records - reservoir pressure and
    /// temperature - are listed with "not recorded" and the source that would hold them, rather than omitted.
    /// </summary>
    private static IReadOnlyList<OgCharacterizationFact> Characterize(
        ReserveModel model,
        IReadOnlyList<POOL> pools,
        FIELD? field,
        string? poolId)
    {
        var facts = new List<OgCharacterizationFact>();

        if (field is not null)
        {
            facts.Add(new OgCharacterizationFact(
                "Field type",
                string.IsNullOrWhiteSpace(field.FIELD_TYPE) ? "not recorded" : field.FIELD_TYPE!.Trim(),
                "FIELD.FIELD_TYPE"));

            facts.Add(new OgCharacterizationFact(
                "Field area",
                string.IsNullOrWhiteSpace(field.AREA_ID) ? "not recorded" : field.AREA_ID!.Trim(),
                "FIELD.AREA_ID"));

            facts.Add(new OgCharacterizationFact(
                "Discovery date",
                field.DISCOVERY_DATE?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "not recorded",
                "FIELD.DISCOVERY_DATE"));
        }

        var pool = string.IsNullOrWhiteSpace(poolId)
            ? pools.FirstOrDefault()
            : pools.FirstOrDefault(row => string.Equals(row.POOL_ID, poolId.Trim(), StringComparison.OrdinalIgnoreCase));

        if (pool is not null)
        {
            var plan = model.PoolPlans.FirstOrDefault(row =>
                string.Equals(row.POOL_ID, pool.POOL_ID, StringComparison.OrdinalIgnoreCase));

            facts.Add(new OgCharacterizationFact(
                "Pool status",
                string.IsNullOrWhiteSpace(pool.POOL_STATUS) ? "not recorded" : pool.POOL_STATUS!.Trim(),
                "POOL.POOL_STATUS"));

            facts.Add(new OgCharacterizationFact(
                "Fluid",
                string.IsNullOrWhiteSpace(pool.POOL_TYPE) ? "not recorded" : pool.POOL_TYPE!.Trim(),
                "POOL.POOL_TYPE"));

            facts.Add(new OgCharacterizationFact(
                "Drive mechanism",
                string.IsNullOrWhiteSpace(plan?.DRIVE_MECHANISM) ? "not recorded" : plan!.DRIVE_MECHANISM!.Trim(),
                string.IsNullOrWhiteSpace(plan?.DRIVE_MECHANISM)
                    ? "DEVELOPMENT_POOL_PLAN.DRIVE_MECHANISM (no plan row for this pool)"
                    : "DEVELOPMENT_POOL_PLAN.DRIVE_MECHANISM"));

            facts.Add(new OgCharacterizationFact(
                "Original oil in place",
                plan is { OOIP: > 0m } ? ReserveUnits.Describe(plan.OOIP, plan.OOIP_OUOM) : "not recorded",
                "DEVELOPMENT_POOL_PLAN.OOIP"));

            facts.Add(new OgCharacterizationFact(
                "Recovery factor",
                plan is { RECOVERY_FACTOR: > 0m }
                    ? (plan.RECOVERY_FACTOR * 100m).ToString("0.##", CultureInfo.InvariantCulture) + "%"
                    : "not recorded",
                "DEVELOPMENT_POOL_PLAN.RECOVERY_FACTOR"));

            var status = model.ReservoirStatuses
                .Where(row => string.Equals(row.POOL_ID, pool.POOL_ID, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.STATUS_DATE ?? DateTime.MinValue)
                .FirstOrDefault();

            facts.Add(new OgCharacterizationFact(
                "Reservoir state",
                status is null
                    ? "not recorded"
                    : (string.IsNullOrWhiteSpace(status.STATUS) ? "(no status)" : status.STATUS!.Trim()) +
                      " on " + (status.STATUS_DATE?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "no date"),
                "RESERVOIR_STATUS.STATUS"));
        }

        // Nothing in this repository records these two; saying so with the source that would hold them is the honest
        // answer, and it keeps the summary from looking complete when it is not.
        facts.Add(new OgCharacterizationFact(
            "Reservoir pressure",
            "not recorded",
            "no table in this repository records reservoir pressure"));

        facts.Add(new OgCharacterizationFact(
            "Reservoir temperature",
            "not recorded",
            "no table in this repository records reservoir temperature"));

        return facts;
    }

    private static string Preview(
        IReadOnlyList<OgReserveEntry> current,
        IReadOnlyList<OgReserveTotal> totals,
        IReadOnlyList<OgReserveEntry> unclassified,
        string? fieldId,
        DateTime asOf)
    {
        var lines = new List<string>
        {
            $"Reserves disclosure preview · field {fieldId ?? "(no field)"} · " +
            $"{new DateTime(asOf.Year, asOf.Month, 1).ToString("yyyy-MM", CultureInfo.InvariantCulture)}",
        };

        if (totals.Count == 0)
        {
            lines.Add("  no classified entry, so there is no total to disclose");
        }
        else
        {
            lines.AddRange(totals.Select(total => "  " + total.Describe()));
        }

        lines.Add(unclassified.Count == 0
            ? "  every entry carries a category"
            : $"  excluded (unclassified): {unclassified.Count.ToString(CultureInfo.InvariantCulture)} entry/entries");

        lines.Add($"  entries considered: {current.Count.ToString(CultureInfo.InvariantCulture)}");

        return string.Join(Environment.NewLine, lines);
    }

    // ------------------------------------------------------------------ reads

    private static async Task<ReserveModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var fields = await OgUow.ReadAllAsync<FIELD>(source).ConfigureAwait(false);
        var pools = await OgUow.ReadAllAsync<POOL>(source).ConfigureAwait(false);
        var entries = await OgUow.ReadAllAsync<RESERVE_ENTRY>(source).ConfigureAwait(false);
        var disclosures = await OgUow.ReadAllAsync<RESERVE_DISCLOSURE>(source).ConfigureAwait(false);
        var statuses = await OgUow.ReadAllAsync<RESERVOIR_STATUS>(source).ConfigureAwait(false);
        var plans = await OgUow.ReadAllAsync<DEVELOPMENT_POOL_PLAN>(source)
            .ConfigureAwait(false);
        var prospects = await OgUow.ReadAllAsync<PROSPECT_VOLUME_ESTIMATE>(source)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new ReserveModel(fields, pools, entries, disclosures, statuses, plans, prospects);
    }

    private static OgReserveEntry ToEntry(
        RESERVE_ENTRY row,
        IReadOnlyDictionary<string, string?> poolNames)
    {
        var poolId = row.POOL_ID?.Trim() ?? string.Empty;

        return new OgReserveEntry(
            row.ENTRY_ID ?? string.Empty,
            poolId,
            poolNames.TryGetValue(poolId, out var name) ? name : null,
            row.FIELD_ID,
            ReserveUnits.Canonical(row.CATEGORY),
            row.GROSS_VOLUME,
            row.NET_VOLUME,
            string.IsNullOrWhiteSpace(row.VOLUME_OUOM) ? null : row.VOLUME_OUOM!.Trim(),
            row.EFFECTIVE_DATE,
            row.REVISION,
            string.IsNullOrWhiteSpace(row.CLASSIFIER) ? null : row.CLASSIFIER!.Trim(),
            row.SUPERSEDES_ENTRY_ID,
            row.BASIS_TEXT,
            row.ACTOR,
            row.RECORDED_AT);
    }

    private sealed record ReserveModel(
        IReadOnlyList<FIELD> Fields,
        IReadOnlyList<POOL> Pools,
        IReadOnlyList<RESERVE_ENTRY> Entries,
        IReadOnlyList<RESERVE_DISCLOSURE> Disclosures,
        IReadOnlyList<RESERVOIR_STATUS> ReservoirStatuses,
        IReadOnlyList<DEVELOPMENT_POOL_PLAN> PoolPlans,
        IReadOnlyList<PROSPECT_VOLUME_ESTIMATE> ProspectEstimates);
}
