using System.Globalization;
using System.Text;
using Beep.OilandGas.Models.Data.Lease;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;

/// <summary>
/// One lease as the register shows it (LSE-01). The four lease tables are merged into this shape.
/// </summary>
/// <param name="StoredStatus">The free-text status the row carries; the register never uses it as the state.</param>
/// <param name="Status">AC-3: the expiry state derived from the dates, not read from a column.</param>
/// <param name="AcreageUnit">AC-2: the unit; when it is missing the page says so rather than showing a bare number.</param>
public sealed record OgLeaseSummary(
    string Id,
    string? LeaseNumber,
    string Kind,
    string? Name,
    decimal? Acreage,
    string? AcreageUnit,
    string? Region,
    string? County,
    string? Operator,
    DateTime? Effective,
    DateTime? Expiry,
    decimal? RoyaltyRate,
    decimal? NetRevenueInterest,
    string? FieldId,
    string? StoredStatus,
    string Status,
    string Source)
{
    public const string KindFeeMineral = "FEE_MINERAL";
    public const string KindGovernment = "GOVERNMENT";
    public const string KindNetProfit = "NET_PROFIT";
    public const string KindAcquisition = "LEASE";

    /// <summary>AC-2: false when the acreage has no recorded unit.</summary>
    public bool HasAcreageUnit => !string.IsNullOrWhiteSpace(AcreageUnit);

    public string AcreageText => Acreage is { } acreage
        ? $"{acreage.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (HasAcreageUnit ? AcreageUnit!.Trim() : "(unit not recorded)")
        : HasAcreageUnit
            ? $"(no acreage) {AcreageUnit!.Trim()}"
            : "(no acreage)";

    public string RoyaltyText => RoyaltyRate is { } rate
        ? $"{(rate * 100m).ToString("0.###", CultureInfo.InvariantCulture)}%"
        : "-";

    public string NetRevenueText => NetRevenueInterest is { } nri
        ? $"{(nri * 100m).ToString("0.###", CultureInfo.InvariantCulture)}%"
        : "-";

    public string StatusText => Status switch
    {
        LeaseStatus.Expired => "expired",
        LeaseStatus.Expiring => "expiring",
        LeaseStatus.Pending => "pending",
        LeaseStatus.Active => "active",
        _ => "unknown",
    };

    public string EffectiveText => Effective?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    public string ExpiryText => Expiry?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                LeaseNumber ?? Id,
                Kind,
                Name,
                AcreageText,
                Region,
                StatusText,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>The expiry states the register derives (AC-3).</summary>
public static class LeaseStatus
{
    public const string Active = "active";
    public const string Expiring = "expiring";
    public const string Expired = "expired";
    public const string Pending = "pending";
    public const string Unknown = "unknown";

    /// <summary>How far ahead a lease counts as expiring rather than active.</summary>
    public const int ExpiringWindowDays = 90;

    public static IReadOnlyList<string> All { get; } = [Active, Expiring, Expired, Pending, Unknown];

    /// <summary>
    /// The state from the dates alone: expired before today, pending after its effective date, expiring inside the
    /// window, active otherwise, and unknown when neither date is recorded.
    /// </summary>
    public static string Of(DateTime? effective, DateTime? expiry, DateTime asOf)
    {
        if (expiry is null && effective is null)
        {
            return Unknown;
        }

        if (expiry is { } end && end.Date < asOf.Date)
        {
            return Expired;
        }

        if (effective is { } start && start.Date > asOf.Date)
        {
            return Pending;
        }

        if (expiry is { } soon && (soon.Date - asOf.Date).TotalDays <= ExpiringWindowDays)
        {
            return Expiring;
        }

        return Active;
    }
}

/// <summary>The register's filters; every one that is set must match.</summary>
public sealed record OgLeaseFilter(
    string? Kind = null,
    string? Region = null,
    string? Operator = null,
    string? Status = null,
    int? ExpiringWithinDays = null)
{
    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Kind)
        || !string.IsNullOrWhiteSpace(Region)
        || !string.IsNullOrWhiteSpace(Operator)
        || !string.IsNullOrWhiteSpace(Status)
        || ExpiringWithinDays is not null;

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                string.IsNullOrWhiteSpace(Kind) ? null : $"type {Kind!.Trim()}",
                string.IsNullOrWhiteSpace(Region) ? null : $"region {Region!.Trim()}",
                string.IsNullOrWhiteSpace(Operator) ? null : $"operator {Operator!.Trim()}",
                string.IsNullOrWhiteSpace(Status) ? null : $"status {Status!.Trim()}",
                ExpiringWithinDays is { } days
                    ? $"expiring within {days.ToString(CultureInfo.InvariantCulture)} d"
                    : null,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>The register: the leases shown and what the filters could offer.</summary>
public sealed record OgLeaseRegister(
    IReadOnlyList<OgLeaseSummary> Leases,
    IReadOnlyList<string> Kinds,
    IReadOnlyList<string> Regions,
    IReadOnlyList<string> Operators,
    DateTime AsOf)
{
    public int Count => Leases.Count;

    /// <summary>AC-2: leases whose acreage has no unit, so the page can call them out.</summary>
    public int MissingAcreageUnitCount => Leases.Count(lease => !lease.HasAcreageUnit);

    public bool IsEmpty => Leases.Count == 0;

    /// <summary>AC-5: the CSV of exactly these rows.</summary>
    public string ToCsv()
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "Lease number,Type,Name,Acreage,Acreage unit,Region,Operator,Effective,Expiry,Royalty,NRI,Status,Source");

        foreach (var lease in Leases)
        {
            builder.Append(Csv(lease.LeaseNumber)).Append(',')
                .Append(Csv(lease.Kind)).Append(',')
                .Append(Csv(lease.Name)).Append(',')
                .Append(Csv(lease.Acreage?.ToString(CultureInfo.InvariantCulture))).Append(',')
                .Append(Csv(lease.AcreageUnit)).Append(',')
                .Append(Csv(lease.Region)).Append(',')
                .Append(Csv(lease.Operator)).Append(',')
                .Append(Csv(lease.EffectiveText)).Append(',')
                .Append(Csv(lease.ExpiryText)).Append(',')
                .Append(Csv(lease.RoyaltyText)).Append(',')
                .Append(Csv(lease.NetRevenueText)).Append(',')
                .Append(Csv(lease.StatusText)).Append(',')
                .Append(Csv(lease.Source))
                .AppendLine();
        }

        return builder.ToString();
    }

    private static string Csv(string? value) =>
        value is not null && (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            ? '"' + value.Replace("\"", "\"\"") + '"'
            : value ?? string.Empty;
}

/// <summary>LSE-01: the lease register over the four lease tables.</summary>
public interface IOgLeaseRegister
{
    Task<OgResult<OgLeaseRegister>> LoadAsync(
        string? fieldId,
        string fieldName,
        OgLeaseFilter? filter,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>AC-4: links a lease to a field and records who did it and when.</summary>
    Task<OgResult> LinkFieldAsync(
        string leaseId,
        string fieldId,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The register over <c>LEASE_ACQUISITION</c>, <c>FEE_MINERAL_LEASE</c>, <c>GOVERNMENT_LEASE</c> and
/// <c>NET_PROFIT_LEASE</c>, read and written through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// The four tables are read separately and merged by lease id, so a lease that exists only in a typed table is on
/// the register too (AC-1). The typed row enriches the base row where the base has no value (lease number, NRI) and
/// adds the row when the base has none. The state comes from the dates (AC-3), and the acreage unit is never
/// dropped (AC-2).
/// </remarks>
public sealed class OgLeaseRegisterService : IOgLeaseRegister
{
    public const string CodeNoConnection = "lease-register-no-connection";
    public const string CodeReadFailed = "lease-register-read-failed";
    public const string CodeWriteFailed = "lease-register-write-failed";
    public const string CodeActorRequired = "lease-register-actor-required";
    public const string CodeFieldRequired = "lease-register-field-required";
    public const string CodeUnknown = "lease-register-unknown";
    public const string CodeTypedOnly = "lease-register-no-base-row";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgLeaseRegisterService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgLeaseRegister>> LoadAsync(
        string? fieldId,
        string fieldName,
        OgLeaseFilter? filter,
        DateTime asOf,
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
                "opening the connection for the lease register",
                "the register is not shown",
                FailureSeverity.Error);

            return OgResult<OgLeaseRegister>.Failed(
                "The lease register could not be read.", reference ?? CodeNoConnection);
        }

        if (source is null)
        {
            return OgResult<OgLeaseRegister>.Failed(
                "No connection is open, so the lease register cannot be read.", CodeNoConnection);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasField = !string.IsNullOrWhiteSpace(fieldId) || !string.IsNullOrWhiteSpace(fieldName);
            var leases = new Dictionary<string, OgLeaseSummary>(StringComparer.OrdinalIgnoreCase);

            // 1. The acquisition table: the base record of a lease.
            foreach (var row in await OgUow.ReadAllAsync<LEASE_ACQUISITION>(source).ConfigureAwait(false))
            {
                if (hasField && !MatchesField(row.FIELD_ID, fieldId, fieldName))
                {
                    continue;
                }

                Add(leases, MapAcquisition(row), asOf.Date);
            }

            // 2. The three typed tables. A row enriches its base row and appears on its own when there is none.
            //    They carry no field column, so a field-scoped register shows typed rows only through their base.
            if (!hasField)
            {
                foreach (var row in await OgUow.ReadAllAsync<FEE_MINERAL_LEASE>(source).ConfigureAwait(false))
                {
                    Add(leases, MapFeeMineral(row), asOf.Date);
                }

                foreach (var row in await OgUow.ReadAllAsync<GOVERNMENT_LEASE>(source).ConfigureAwait(false))
                {
                    Add(leases, MapGovernment(row), asOf.Date);
                }

                foreach (var row in await OgUow.ReadAllAsync<NET_PROFIT_LEASE>(source).ConfigureAwait(false))
                {
                    Add(leases, MapNetProfit(row), asOf.Date);
                }
            }

            var all = leases.Values.ToList();
            var shown = Filter(all, filter, asOf.Date).ToList();

            return OgResult<OgLeaseRegister>.Ok(new OgLeaseRegister(
                shown,
                Options(all.Select(lease => lease.Kind)),
                Options(all.Select(lease => lease.Region)),
                Options(all.Select(lease => lease.Operator)),
                asOf.Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the lease register",
                "the register is not shown",
                FailureSeverity.Error);

            return OgResult<OgLeaseRegister>.Failed(
                "The lease register could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult> LinkFieldAsync(
        string leaseId,
        string fieldId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult.Failed(
                "Linking a lease to a field must record who did it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(fieldId))
        {
            return OgResult.Failed("Choose the field to link the lease to.", CodeFieldRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed(
                "No connection is open, so the lease cannot be linked.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<LEASE_ACQUISITION>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("LEASE_ACQUISITION_ID", leaseId.Trim())],
                cancellationToken).ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            // The field lives on the acquisition row; a lease that exists only in a typed table has nowhere to
            // record it, and saying so is better than writing a row the register would then not read.
            if (row is null)
            {
                return OgResult.Failed(
                    $"Lease '{leaseId}' has no acquisition row, so it cannot be linked to a field. Add its " +
                    "acquisition record first.",
                    CodeTypedOnly);
            }

            var now = DateTime.UtcNow;

            row.FIELD_ID = fieldId.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult.Failed(
                    OgUow.Message(updated, "The lease could not be linked."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok(
                    $"Lease {leaseId.Trim()} linked to {fieldId.Trim()} by {actor.Trim()} on " +
                    $"{now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.")
                : OgResult.Failed(OgUow.Message(committed, "The lease could not be linked."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"linking lease {leaseId} to a field",
                "the link was not recorded",
                FailureSeverity.Error);

            return OgResult.Failed(
                "The lease could not be linked.", reference ?? CodeWriteFailed);
        }
    }

    private static OgLeaseSummary MapAcquisition(LEASE_ACQUISITION row) =>
        new(
            row.LEASE_ACQUISITION_ID ?? string.Empty,
            null,
            Blank(row.LEASE_TYPE) ?? OgLeaseSummary.KindAcquisition,
            Blank(row.LEASE_NAME),
            Zero(row.GROSS_ACREAGE),
            Blank(row.ACREAGE_OUOM),
            Blank(row.STATE_ID),
            Blank(row.COUNTY_ID),
            Blank(row.OPERATOR),
            row.ACQUISITION_DATE,
            row.LEASE_EXPIRATION,
            Zero(row.ROYALTY_RATE),
            Zero(row.NET_REVENUE_INTEREST),
            Blank(row.FIELD_ID),
            Blank(row.ACQUISITION_STATUS),
            LeaseStatus.Unknown,
            "LEASE_ACQUISITION");

    private static OgLeaseSummary MapFeeMineral(FEE_MINERAL_LEASE row) =>
        new(
            row.LEASE_ID ?? string.Empty,
            Blank(row.LEASE_NUMBER),
            OgLeaseSummary.KindFeeMineral,
            Blank(row.LEASE_NAME),
            Zero(row.GROSS_ACREAGE),
            Blank(row.ACREAGE_OUOM),
            Blank(row.STATE_ID),
            Blank(row.COUNTY_ID),
            null,
            row.ACQUISITION_DATE,
            row.EXPIRATION_DATE,
            row.ROYALTY_RATE,
            row.NET_REVENUE_INTEREST,
            null,
            Blank(row.ACQUISITION_STATUS),
            LeaseStatus.Unknown,
            "FEE_MINERAL_LEASE");

    private static OgLeaseSummary MapGovernment(GOVERNMENT_LEASE row) =>
        new(
            row.LEASE_ID ?? string.Empty,
            Blank(row.LEASE_NUMBER),
            OgLeaseSummary.KindGovernment,
            Blank(row.LEASE_NAME),
            Zero(row.GROSS_ACREAGE),
            Blank(row.ACREAGE_OUOM),
            Blank(row.STATE_ID),
            Blank(row.COUNTY_ID),
            null,
            null,
            row.EXPIRATION_DATE,
            row.ROYALTY_RATE,
            row.NET_REVENUE_INTEREST,
            null,
            null,
            LeaseStatus.Unknown,
            "GOVERNMENT_LEASE");

    private static OgLeaseSummary MapNetProfit(NET_PROFIT_LEASE row) =>
        new(
            row.LEASE_ID ?? string.Empty,
            Blank(row.LEASE_NUMBER),
            OgLeaseSummary.KindNetProfit,
            Blank(row.LEASE_NAME),
            null,
            null,
            null,
            null,
            Blank(row.BUSINESS_ASSOCIATE_ID),
            null,
            row.EXPIRATION_DATE,
            row.ROYALTY_RATE == 0 ? null : row.ROYALTY_RATE,
            row.NET_REVENUE_INTEREST == 0 ? null : row.NET_REVENUE_INTEREST,
            Blank(row.AREA_ID),
            null,
            LeaseStatus.Unknown,
            "NET_PROFIT_LEASE");

    private static bool MatchesField(string? value, string? fieldId, string? fieldName) =>
        (Matches(value, fieldId) || Matches(value, fieldName));

    private static bool Matches(string? value, string? expected) =>
        !string.IsNullOrWhiteSpace(expected)
        && string.Equals(value ?? string.Empty, expected.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Merges one candidate row into the register: the first row creates it, a later row fills only the values the
    /// register does not already have (a typed row never overwrites the acquisition row).
    /// </summary>
    private static void Add(
        Dictionary<string, OgLeaseSummary> leases,
        OgLeaseSummary candidate,
        DateTime asOf)
    {
        if (string.IsNullOrWhiteSpace(candidate.Id))
        {
            return;
        }

        if (!leases.TryGetValue(candidate.Id, out var existing))
        {
            leases[candidate.Id] = Finish(candidate, asOf);
            return;
        }

        leases[candidate.Id] = Finish(
            existing with
            {
                LeaseNumber = existing.LeaseNumber ?? candidate.LeaseNumber,
                Name = existing.Name ?? candidate.Name,
                Acreage = existing.Acreage ?? candidate.Acreage,
                AcreageUnit = existing.AcreageUnit ?? candidate.AcreageUnit,
                Region = existing.Region ?? candidate.Region,
                County = existing.County ?? candidate.County,
                Operator = existing.Operator ?? candidate.Operator,
                Effective = existing.Effective ?? candidate.Effective,
                Expiry = existing.Expiry ?? candidate.Expiry,
                RoyaltyRate = existing.RoyaltyRate ?? candidate.RoyaltyRate,
                NetRevenueInterest = existing.NetRevenueInterest ?? candidate.NetRevenueInterest,
                FieldId = existing.FieldId ?? candidate.FieldId,
                StoredStatus = existing.StoredStatus ?? candidate.StoredStatus,
                Source = existing.Source,
            },
            asOf);
    }

    private static OgLeaseSummary Finish(OgLeaseSummary lease, DateTime asOf) =>
        lease with { Status = LeaseStatus.Of(lease.Effective, lease.Expiry, asOf) };

    /// <summary>The rows a filter selects. Public so the rule can be tested without a database.</summary>
    public static IEnumerable<OgLeaseSummary> Filter(
        IEnumerable<OgLeaseSummary> leases,
        OgLeaseFilter? filter,
        DateTime asOf)
    {
        if (filter is null || !filter.HasFilters)
        {
            return leases;
        }

        var filtered = leases;

        if (!string.IsNullOrWhiteSpace(filter.Kind))
        {
            filtered = filtered.Where(lease => Matches(lease.Kind, filter.Kind));
        }

        if (!string.IsNullOrWhiteSpace(filter.Region))
        {
            filtered = filtered.Where(lease => Matches(lease.Region, filter.Region));
        }

        if (!string.IsNullOrWhiteSpace(filter.Operator))
        {
            filtered = filtered.Where(lease => Matches(lease.Operator, filter.Operator));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            filtered = filtered.Where(lease => Matches(lease.Status, filter.Status));
        }

        if (filter.ExpiringWithinDays is { } days)
        {
            filtered = filtered.Where(lease =>
                lease.Expiry is { } expiry
                && expiry.Date >= asOf.Date
                && (expiry.Date - asOf.Date).TotalDays <= days);
        }

        return filtered;
    }

    private static IReadOnlyList<string> Options(IEnumerable<string?> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>A PPDM measure is a non-nullable decimal in the entity; zero means it was not recorded.</summary>
    private static decimal? Zero(decimal value) => value == 0 ? null : value;

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
