using System.Globalization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

/// <summary>One month of the selection's oil (PRD-02 trend panel).</summary>
public sealed record OgMonthPoint(DateTime Month, decimal? Oil, string? Unit)
{
    public string Label => Month.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    public string Text => Oil is { } oil
        ? $"{Label} {OgProductionDay.Format(oil, Unit)}"
        : $"{Label} not recorded";
}

/// <summary>One pool of a producing field, with its recorded performance (PRD-02).</summary>
public sealed record OgPoolSummary(
    string Id,
    string? Name,
    string? FieldId,
    string? Reservoir,
    string? Drive,
    string? Status,
    string? PoolType,
    int WellCount,
    DateTime? DiscoveryDate,
    decimal? Rate,
    string? RateUnit,
    decimal? Cumulative,
    string? CumulativeUnit,
    DateTime? LastProductionDate)
{
    public bool IsPreProduction => LastProductionDate is null;

    public string NameText => Name ?? Id;

    public string ReservoirText => string.IsNullOrWhiteSpace(Reservoir) ? "not recorded" : Reservoir!.Trim();

    /// <summary>The drive mechanism comes from DEV-04's plan row for the pool; PPDM's POOL has no such column.</summary>
    public string DriveText => string.IsNullOrWhiteSpace(Drive) ? "not recorded" : Drive!.Trim();

    public string StatusText => IsPreProduction ? "pre-production" : (Status ?? "not recorded").ToLowerInvariant();

    public string WellCountText => WellCount == 0 ? "0" : WellCount.ToString(CultureInfo.InvariantCulture);

    /// <summary>Rates show an em dash for a pool that has not produced, never a zero.</summary>
    public string RateText => IsPreProduction ? "—" : OgProductionDay.Format(Rate, RateUnit);

    public string CumulativeText => IsPreProduction ? "—" : OgProductionDay.Format(Cumulative, CumulativeUnit);

    public string Describe() =>
        $"{NameText} · reservoir {ReservoirText} · drive {DriveText} · {StatusText} · " +
        $"{WellCountText} well(s) · rate {RateText} · cumulative {CumulativeText}";
}

/// <summary>
/// One producing field with its recorded performance (PRD-02). Every figure is a constructor argument taken from a
/// record; the computed members only format or derive from them.
/// </summary>
/// <remarks>
/// <see cref="Oil"/>, <see cref="Gas"/> and <see cref="Water"/> are the field's totals over the measurement window
/// (<see cref="FieldRegisterRules.WindowDays"/> days ending on the as-of day), which is the period a register reports;
/// <see cref="WaterCut"/> is computed from those same three totals. The monthly trend and the pool's rate are the
/// one-day and one-month views of the same records.
/// </remarks>
public sealed record OgFieldSummary(
    string Id,
    string? Name,
    string? Basin,
    string? FieldType,
    DateTime? DiscoveryDate,
    DateTime? FirstProduction,
    string Status,
    int WellCount,
    int PoolCount,
    decimal? Oil,
    decimal? Gas,
    decimal? Water,
    string? OilUnit,
    string? GasUnit,
    string? WaterUnit,
    decimal? WaterCut,
    decimal? Uptime,
    DateTime? LastProductionDate,
    int ExcludedUnitRows)
{
    public bool IsPreProduction => FirstProduction is null;

    public string NameText => Name ?? Id;

    /// <summary>PPDM records the field's area, not a basin; the area is what this column holds.</summary>
    public string BasinText => string.IsNullOrWhiteSpace(Basin) ? "not recorded" : Basin!.Trim();

    public string StatusText => Status.ToLowerInvariant();

    public string DiscoveryText => Date(DiscoveryDate);

    public string FirstProductionText => IsPreProduction ? "—" : Date(FirstProduction);

    public string OilText => Rate(Oil, OilUnit);

    public string GasText => Rate(Gas, GasUnit);

    public string WaterText => Rate(Water, WaterUnit);

    public string WaterCutText => IsPreProduction
        ? "—"
        : WaterCut is { } cut
            ? (cut * 100m).ToString("0.##", CultureInfo.InvariantCulture) + "%"
            : "not computable";

    public string UptimeText => Uptime is { } uptime
        ? (uptime * 100m).ToString("0.#", CultureInfo.InvariantCulture) + "%"
        : "not recorded";

    /// <summary>AC-3: the aggregate says how many rows it could not include.</summary>
    public string ExcludedText => ExcludedUnitRows == 0
        ? "no row excluded for an unknown unit"
        : $"{ExcludedUnitRows.ToString(CultureInfo.InvariantCulture)} row(s) excluded from the totals: no unit recorded";

    public string Describe() =>
        $"{NameText} · area {BasinText} · {StatusText} · {WellCount.ToString(CultureInfo.InvariantCulture)} well(s) · " +
        $"{PoolCount.ToString(CultureInfo.InvariantCulture)} pool(s) · oil {OilText} · gas {GasText} · " +
        $"water {WaterText} · water cut {WaterCutText} · uptime {UptimeText}";

    private string Rate(decimal? value, string? unit) => IsPreProduction
        ? "—"
        : OgProductionDay.Format(value, unit);

    private static string Date(DateTime? value) =>
        value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "not recorded";
}

/// <summary>The register for one page of fields, the selected field's pools and their trend (PRD-02).</summary>
public sealed record OgFieldRegister(
    string FieldName,
    string? SelectedFieldId,
    DateTime AsOf,
    IReadOnlyList<OgFieldSummary> Fields,
    int TotalFields,
    int PageIndex,
    int PageSize,
    int PageCount,
    OgFieldSummary? Selected,
    IReadOnlyList<OgPoolSummary> Pools,
    string? SelectedPoolId,
    OgPoolSummary? SelectedPool,
    IReadOnlyList<OgMonthPoint> Trend,
    string TrendText)
{
    public bool HasPrevious => PageIndex > 0;

    public bool HasNext => PageIndex + 1 < PageCount;

    public int ExcludedUnitRows => Fields.Sum(item => item.ExcludedUnitRows);

    public string PagingText => PageCount <= 1
        ? $"{TotalFields.ToString(CultureInfo.InvariantCulture)} field(s)"
        : $"fields {((PageIndex * PageSize) + 1).ToString(CultureInfo.InvariantCulture)}–" +
          $"{Math.Min(TotalFields, (PageIndex + 1) * PageSize).ToString(CultureInfo.InvariantCulture)} of " +
          $"{TotalFields.ToString(CultureInfo.InvariantCulture)} · page " +
          $"{(PageIndex + 1).ToString(CultureInfo.InvariantCulture)} of " +
          $"{PageCount.ToString(CultureInfo.InvariantCulture)}";

    public string Summary =>
        $"{TotalFields.ToString(CultureInfo.InvariantCulture)} field(s) · " +
        $"{Fields.Count(item => item.IsPreProduction).ToString(CultureInfo.InvariantCulture)} pre-production on this page";

    public string SelectedPoolsText => Selected is null
        ? "Choose a field to see its pools."
        : Pools.Count == 0
            ? $"{Selected.NameText} has no pool recorded."
            : $"{Pools.Count.ToString(CultureInfo.InvariantCulture)} pool(s) of {Selected.NameText}";
}

/// <summary>The rules the register states once, so the page and the tests agree.</summary>
public static class FieldRegisterRules
{
    /// <summary>The window the rates and the uptime are read over.</summary>
    public const int WindowDays = 30;

    /// <summary>The months the trend panel covers.</summary>
    public const int TrendMonths = 12;

    /// <summary>The default page size; the register must page beyond 500 fields (AC-4).</summary>
    public const int DefaultPageSize = 100;

    public const int MinimumPageSize = 10;
    public const int MaximumPageSize = 500;

    public static int ClampPageSize(int pageSize) =>
        pageSize < MinimumPageSize ? DefaultPageSize
        : pageSize > MaximumPageSize ? MaximumPageSize
        : pageSize;

    public static int PageCount(int total, int pageSize)
    {
        var size = ClampPageSize(pageSize);
        return total <= 0 ? 0 : (int)Math.Ceiling(total / (double)size);
    }

    public static int ClampPageIndex(int pageIndex, int total, int pageSize)
    {
        var pages = PageCount(total, pageSize);
        return pageIndex < 0 ? 0 : pages == 0 ? 0 : Math.Min(pageIndex, pages - 1);
    }

    /// <summary>Uptime is the share of the window's days that carry a production row.</summary>
    public static decimal? Uptime(int daysWithProduction)
    {
        if (daysWithProduction <= 0)
        {
            return null;
        }

        return Math.Min(1m, daysWithProduction / (decimal)WindowDays);
    }

    /// <summary>A water cut needs oil and water in the same unit (AC-3: never across units).</summary>
    public static decimal? WaterCut(decimal? oil, decimal? water, string? oilUnit, string? waterUnit) =>
        OperationsCentreRules.WaterCut(oil, water, oilUnit, waterUnit);

    /// <summary>The unit most rows are stated in; a row in another unit is excluded and counted (AC-3).</summary>
    public static string? DominantUnit(IEnumerable<string?> units) =>
        OperationsCentreRules.DominantUnit(units);

    /// <summary>The status of a field: it has never produced, it produces, or it is inactive.</summary>
    public static string Status(bool everProduced, string? activeInd) => everProduced
        ? string.Equals((activeInd ?? string.Empty).Trim(), "N", StringComparison.OrdinalIgnoreCase)
            ? "INACTIVE"
            : "PRODUCING"
        : "PRE_PRODUCTION";
}

/// <summary>PRD-02: the register of producing fields and pools (PRD-02).</summary>
public interface IOgFieldPoolRegister
{
    /// <summary>
    /// One page of fields with the selected field's pools and trend. A failed read is a failure and never a page of
    /// zeroed figures (AC-2).
    /// </summary>
    Task<OgResult<OgFieldRegister>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? poolId = null,
        int pageIndex = 0,
        int pageSize = FieldRegisterRules.DefaultPageSize,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The register over the PPDM <c>FIELD</c>, <c>POOL</c>, <c>PDEN_FIELD</c>, <c>PDEN_POOL</c>, <c>PDEN_WELL</c> and
/// <c>PDEN_VOL_SUMMARY</c> rows and the DevelopmentPlanning <c>DEVELOPMENT_POOL_PLAN</c> row for the pool's drive
/// mechanism, read through their entity models and a unit of work. It writes nothing.
/// </summary>
/// <remarks>
/// Every figure comes from a record: field and pool identity from <c>FIELD</c>/<c>POOL</c>, well counts from
/// <c>PDEN_FIELD</c>/<c>PDEN_POOL</c> and <c>PDEN_WELL</c>, volumes and dates from <c>PDEN_VOL_SUMMARY</c> (latest
/// amendment per entity/day/activity), and the drive mechanism from DEV-04's plan row. Aggregates are summed only within
/// one unit per fluid; a row whose unit is not recorded is excluded and counted (AC-3).
/// </remarks>
public sealed class OgFieldPoolRegisterService : IOgFieldPoolRegister
{
    public const string CodeNoConnection = "field-register-no-connection";
    public const string CodeReadFailed = "field-register-read-failed";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgFieldPoolRegisterService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgFieldRegister>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? poolId = null,
        int pageIndex = 0,
        int pageSize = FieldRegisterRules.DefaultPageSize,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgFieldRegister>.Failed(
                "No connection is open, so the field and pool register cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var day = (asOf ?? DateTime.Today).Date;
            var windowStart = day.AddDays(-(FieldRegisterRules.WindowDays - 1));

            var size = FieldRegisterRules.ClampPageSize(pageSize);

            var summaries = model.Fields
                .Where(row => !string.IsNullOrWhiteSpace(row.FIELD_ID))
                .GroupBy(row => row.FIELD_ID!, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Select(row => BuildField(row, model, day, windowStart))
                .OrderBy(summary => summary.NameText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(summary => summary.Id, StringComparer.Ordinal)
                .ToList();

            var pages = FieldRegisterRules.PageCount(summaries.Count, size);
            var index = FieldRegisterRules.ClampPageIndex(pageIndex, summaries.Count, size);

            var page = summaries
                .Skip(index * size)
                .Take(size)
                .ToList();

            var selected = summaries.FirstOrDefault(summary =>
                               !string.IsNullOrWhiteSpace(fieldId)
                               && string.Equals(summary.Id, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
                           ?? summaries.FirstOrDefault(summary =>
                               !string.IsNullOrWhiteSpace(fieldName)
                               && string.Equals(summary.NameText, fieldName.Trim(), StringComparison.OrdinalIgnoreCase))
                           ?? page.FirstOrDefault();

            var pools = selected is null
                ? []
                : BuildPools(selected.Id, model, day, windowStart);

            // The trend is the field's unless a pool is named: a first pool is not a selection the reader made.
            var selectedPool = string.IsNullOrWhiteSpace(poolId)
                ? null
                : pools.FirstOrDefault(pool =>
                    string.Equals(pool.Id, poolId.Trim(), StringComparison.OrdinalIgnoreCase));

            var (trend, trendText) = BuildTrend(selected, selectedPool, model, day);

            return OgResult<OgFieldRegister>.Ok(new OgFieldRegister(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                selected?.Id,
                day,
                page,
                summaries.Count,
                index,
                size,
                pages,
                selected,
                pools,
                selectedPool?.Id,
                selectedPool,
                trend,
                trendText));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the field and pool register",
                "the register is not shown and no figure is rendered",
                FailureSeverity.Error);

            return OgResult<OgFieldRegister>.Failed(
                "The field and pool register could not be read.", reference ?? CodeReadFailed);
        }
    }

    // ------------------------------------------------------------------ field

    private static OgFieldSummary BuildField(
        FIELD field,
        RegisterModel model,
        DateTime day,
        DateTime windowStart)
    {
        var id = field.FIELD_ID!;

        var entityIds = model.PdenFields
            .Where(row => string.Equals(row.FIELD_ID, id, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Select(row => row.PDEN_ID)
            .Where(pdenId => !string.IsNullOrWhiteSpace(pdenId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = LatestProduction(model, entityIds);

        var everProduced = rows.Count > 0;

        var inWindow = rows
            .Where(row => row.VOLUME_DATE!.Value.Date >= windowStart && row.VOLUME_DATE!.Value.Date <= day)
            .ToList();

        var (oil, oilUnit, oilExcluded) = Sum(inWindow, row => row.OIL_VOLUME, row => row.OIL_VOLUME_OUOM);
        var (gas, gasUnit, gasExcluded) = Sum(inWindow, row => row.GAS_VOLUME, row => row.GAS_VOLUME_OUOM);
        var (water, waterUnit, waterExcluded) = Sum(inWindow, row => row.WATER_VOLUME, row => row.WATER_VOLUME_OUOM);

        var wellCount = RecordedWellCount(model, id, entityIds);

        var pools = model.Pools
            .Count(row => string.Equals(row.FIELD_ID, id, StringComparison.OrdinalIgnoreCase));

        var daysWithProduction = inWindow
            .Select(row => row.VOLUME_DATE!.Value.Date)
            .Distinct()
            .Count();

        return new OgFieldSummary(
            id,
            Blank(field.FIELD_NAME),
            Blank(field.AREA_ID),
            Blank(field.FIELD_TYPE),
            field.DISCOVERY_DATE,
            rows.Count == 0 ? null : rows.Min(row => row.VOLUME_DATE!.Value.Date),
            FieldRegisterRules.Status(everProduced, field.ACTIVE_IND),
            wellCount,
            pools,
            oil,
            gas,
            water,
            oilUnit,
            gasUnit,
            waterUnit,
            FieldRegisterRules.WaterCut(oil, water, oilUnit, waterUnit),
            FieldRegisterRules.Uptime(daysWithProduction),
            inWindow.Count == 0 ? null : inWindow.Max(row => row.VOLUME_DATE!.Value.Date),
            oilExcluded + gasExcluded + waterExcluded);
    }

    /// <summary>The recorded well count of the field, falling back to the wells actually linked to its entities.</summary>
    private static int RecordedWellCount(
        RegisterModel model,
        string fieldId,
        IReadOnlySet<string> entityIds)
    {
        var recorded = model.PdenFields
            .Where(row => string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
            .Sum(row => row.NO_OF_OIL_WELLS + row.NO_OF_GAS_WELLS);

        if (recorded > 0)
        {
            return (int)recorded;
        }

        return model.PdenWells
            .Where(row => row.PDEN_ID is not null && entityIds.Contains(row.PDEN_ID))
            .Select(row => row.PRIMARY_UWI ?? string.Empty)
            .Where(uwi => uwi.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    // ------------------------------------------------------------------- pool

    private static IReadOnlyList<OgPoolSummary> BuildPools(
        string fieldId,
        RegisterModel model,
        DateTime day,
        DateTime windowStart)
    {
        var plans = model.PoolPlans
            .Where(row => string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.POOL_ID))
            .GroupBy(row => row.POOL_ID!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return model.Pools
            .Where(row => string.Equals(row.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.POOL_ID))
            .Select(row => row.POOL_ID!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(poolId => BuildPool(poolId, model, plans, day, windowStart))
            .Where(pool => pool is not null)
            .Select(pool => pool!)
            .OrderBy(pool => pool.NameText, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static OgPoolSummary? BuildPool(
        string poolId,
        RegisterModel model,
        IReadOnlyDictionary<string, DEVELOPMENT_POOL_PLAN> plans,
        DateTime day,
        DateTime windowStart)
    {
        var pool = model.Pools.FirstOrDefault(row =>
            string.Equals(row.POOL_ID, poolId, StringComparison.OrdinalIgnoreCase));

        if (pool is null)
        {
            return null;
        }

        var entityIds = model.PdenPools
            .Where(row => string.Equals(row.POOL_ID, poolId, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Select(row => row.PDEN_ID)
            .Where(pdenId => !string.IsNullOrWhiteSpace(pdenId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = LatestProduction(model, entityIds);

        var inWindow = rows
            .Where(row => row.VOLUME_DATE!.Value.Date >= windowStart && row.VOLUME_DATE!.Value.Date <= day)
            .ToList();

        var latestDay = inWindow.Count == 0
            ? (DateTime?)null
            : inWindow.Max(row => row.VOLUME_DATE!.Value.Date);

        var todayRows = latestDay is { } last
            ? inWindow.Where(row => row.VOLUME_DATE!.Value.Date == last).ToList()
            : [];

        var (rate, rateUnit, _) = Sum(todayRows, row => row.OIL_VOLUME, row => row.OIL_VOLUME_OUOM);

        // The cumulative volume is the value each entity last recorded, summed only within one unit.
        var cumulativeRows = rows
            .GroupBy(row => row.PDEN_ID!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(row => row.VOLUME_DATE).First())
            .ToList();

        var cumulativeUnit = FieldRegisterRules.DominantUnit(cumulativeRows.Select(row => row.OIL_VOLUME_OUOM));

        decimal? cumulative = cumulativeUnit is null
            ? null
            : cumulativeRows
                .Where(row => string.Equals((row.OIL_VOLUME_OUOM ?? string.Empty).Trim(), cumulativeUnit,
                    StringComparison.OrdinalIgnoreCase))
                .Sum(row => row.OIL_CUM_VOLUME);

        var wells = model.PdenWells
            .Where(row => row.PDEN_ID is not null && entityIds.Contains(row.PDEN_ID))
            .Select(row => row.PRIMARY_UWI ?? string.Empty)
            .Where(uwi => uwi.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        var recordedWells = model.PdenPools
            .Where(row => string.Equals(row.POOL_ID, poolId, StringComparison.OrdinalIgnoreCase))
            .Sum(row => row.NO_OF_OIL_WELLS + row.NO_OF_GAS_WELLS);

        plans.TryGetValue(poolId, out var plan);

        return new OgPoolSummary(
            pool.POOL_ID!,
            Blank(pool.POOL_NAME),
            Blank(pool.FIELD_ID),
            Blank(pool.STRAT_UNIT_ID),
            Blank(plan?.DRIVE_MECHANISM),
            Blank(pool.POOL_STATUS),
            Blank(pool.POOL_TYPE),
            wells > 0 ? wells : (int)recordedWells,
            pool.DISCOVERY_DATE,
            rate,
            rateUnit,
            cumulativeUnit is null ? null : cumulative,
            cumulativeUnit,
            latestDay);
    }

    // ------------------------------------------------------------------ trend

    private static (IReadOnlyList<OgMonthPoint> Points, string Text) BuildTrend(
        OgFieldSummary? field,
        OgPoolSummary? pool,
        RegisterModel model,
        DateTime day)
    {
        if (field is null)
        {
            return ([], "no field selected, so there is no trend");
        }

        IReadOnlySet<string> entityIds;

        if (pool is not null)
        {
            entityIds = model.PdenPools
                .Where(row => string.Equals(row.POOL_ID, pool.Id, StringComparison.OrdinalIgnoreCase))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Select(row => row.PDEN_ID)
                .Where(pdenId => !string.IsNullOrWhiteSpace(pdenId))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            entityIds = model.PdenFields
                .Where(row => string.Equals(row.FIELD_ID, field.Id, StringComparison.OrdinalIgnoreCase))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Select(row => row.PDEN_ID)
                .Where(pdenId => !string.IsNullOrWhiteSpace(pdenId))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        var rows = LatestProduction(model, entityIds);
        var firstMonth = new DateTime(day.Year, day.Month, 1).AddMonths(-(FieldRegisterRules.TrendMonths - 1));

        var inTrend = rows
            .Where(row => row.VOLUME_DATE!.Value.Date >= firstMonth)
            .ToList();

        var unit = FieldRegisterRules.DominantUnit(inTrend.Select(row => row.OIL_VOLUME_OUOM));

        var points = new List<OgMonthPoint>();

        for (var index = 0; index < FieldRegisterRules.TrendMonths; index++)
        {
            var month = firstMonth.AddMonths(index);

            var matching = inTrend
                .Where(row => row.VOLUME_DATE!.Value.Year == month.Year
                              && row.VOLUME_DATE!.Value.Month == month.Month)
                .Where(row => string.Equals((row.OIL_VOLUME_OUOM ?? string.Empty).Trim(), unit ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            points.Add(new OgMonthPoint(
                month,
                matching.Count == 0 ? null : matching.Sum(row => row.OIL_VOLUME),
                unit));
        }

        var named = pool is null ? field.NameText : $"{field.NameText} / {pool.NameText}";

        return (points,
            points.All(point => point.Oil is null)
                ? $"no production is recorded for {named} in the last " +
                  $"{FieldRegisterRules.TrendMonths.ToString(CultureInfo.InvariantCulture)} month(s)"
                : $"{named} · monthly oil · " +
                  string.Join(" · ", points.Select(point => point.Text)));
    }

    // ------------------------------------------------------------------ reads

    private static async Task<RegisterModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var fields = await OgUow.ReadAllAsync<FIELD>(source).ConfigureAwait(false);
        var pools = await OgUow.ReadAllAsync<POOL>(source).ConfigureAwait(false);
        var pdenFields = await OgUow.ReadAllAsync<PDEN_FIELD>(source).ConfigureAwait(false);
        var pdenPools = await OgUow.ReadAllAsync<PDEN_POOL>(source).ConfigureAwait(false);
        var pdenWells = await OgUow.ReadAllAsync<PDEN_WELL>(source).ConfigureAwait(false);
        var volumes = await OgUow.ReadAllAsync<PDEN_VOL_SUMMARY>(source).ConfigureAwait(false);
        var poolPlans = await OgUow.ReadAllAsync<DEVELOPMENT_POOL_PLAN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new RegisterModel(fields, pools, pdenFields, pdenPools, pdenWells, volumes, poolPlans);
    }

    /// <summary>The production rows of the entities, one per entity/day/activity, taking the highest amendment.</summary>
    private static List<PDEN_VOL_SUMMARY> LatestProduction(
        RegisterModel model,
        IReadOnlySet<string> entityIds) =>
        model.Volumes
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityIds.Contains(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.VOLUME_DATE is not null)
            .Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE))
            .GroupBy(row => (
                row.PDEN_ID!.Trim(),
                Day: row.VOLUME_DATE!.Value.Date,
                Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
            .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
            .ToList();

    private static (decimal? Total, string? Unit, int Excluded) Sum(
        IReadOnlyList<PDEN_VOL_SUMMARY> rows,
        Func<PDEN_VOL_SUMMARY, decimal> value,
        Func<PDEN_VOL_SUMMARY, string?> unit)
    {
        if (rows.Count == 0)
        {
            return (null, null, 0);
        }

        var dominant = FieldRegisterRules.DominantUnit(rows.Select(unit));
        var known = rows
            .Where(row => !string.IsNullOrWhiteSpace(unit(row)))
            .ToList();

        if (dominant is null || known.Count == 0)
        {
            // AC-3: no unit is recorded on any row, so there is no total to state and every row is excluded.
            return (null, null, rows.Count);
        }

        var matching = known
            .Where(row => string.Equals((unit(row) ?? string.Empty).Trim(), dominant, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return (matching.Count == 0 ? 0m : matching.Sum(value), dominant, rows.Count - matching.Count);
    }

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record RegisterModel(
        IReadOnlyList<FIELD> Fields,
        IReadOnlyList<POOL> Pools,
        IReadOnlyList<PDEN_FIELD> PdenFields,
        IReadOnlyList<PDEN_POOL> PdenPools,
        IReadOnlyList<PDEN_WELL> PdenWells,
        IReadOnlyList<PDEN_VOL_SUMMARY> Volumes,
        IReadOnlyList<DEVELOPMENT_POOL_PLAN> PoolPlans);
}
