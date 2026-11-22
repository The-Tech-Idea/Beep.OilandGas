using System.Globalization;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

/// <summary>One physical hole under a well (WEL-03).</summary>
public sealed record OgWellboreSummary(
    string Uwi,
    string Id,
    string? Name,
    string? ParentId,
    string Kind,
    string? Status,
    DateTime? SpudDate,
    decimal? TotalDepth,
    string? DepthUnit)
{
    /// <summary>AC-4: a depth without its unit is shown as unspecified and disables survey/geometry screens.</summary>
    public bool HasDepthUnit => !string.IsNullOrWhiteSpace(DepthUnit);

    public bool SurveyEnabled => HasDepthUnit;

    public string DepthText =>
        TotalDepth is { } depth
            ? $"{depth.ToString("0.##", CultureInfo.InvariantCulture)} {DepthUnit ?? "(unit unspecified)"}"
            : string.Empty;

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Name ?? Id,
                Kind,
                string.IsNullOrWhiteSpace(ParentId) ? null : $"from {ParentId}",
                string.IsNullOrWhiteSpace(Status) ? null : Status,
                string.IsNullOrWhiteSpace(DepthText) ? null : $"TD {DepthText}",
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>What to create. Kind is ORIGINAL, SIDETRACK or REDRILL.</summary>
public sealed record OgWellboreRequest(
    string Uwi,
    string Id,
    string? Name = null,
    string? ParentId = null,
    string? Kind = null,
    string? Status = null,
    DateTime? SpudDate = null,
    decimal? TotalDepth = null,
    string? DepthUnit = null);

/// <summary>WEL-03: the wellbores of one well.</summary>
public interface IOgWellbores
{
    /// <summary>Only the wellbores of <paramref name="uwi"/> (AC-1).</summary>
    Task<OgResult<IReadOnlyList<OgWellboreSummary>>> ListAsync(
        string uwi,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgWellboreSummary>> CreateAsync(
        OgWellboreRequest request,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a bore; refused with the count when surveys are linked (AC-3).</summary>
    Task<OgResult> DeleteAsync(
        string uwi,
        string wellboreId,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The wellbores over the LifeCycle-owned <c>WELLBORE</c> and <c>WELLBORE_SURVEY</c> tables, read and written
/// through their entity models and a unit of work.
/// </summary>
public sealed class OgWellbores : IOgWellbores
{
    public const string CodeNoConnection = "wellbores-no-connection";
    public const string CodeReadFailed = "wellbores-read-failed";
    public const string CodeWriteFailed = "wellbores-write-failed";
    public const string CodeParentWellMissing = "wellbore-parent-well-missing";
    public const string CodeParentBoreRequired = "wellbore-parent-required";
    public const string CodeParentBoreUnknown = "wellbore-parent-unknown";
    public const string CodeHasSurveys = "wellbore-has-surveys";
    public const string CodeHasChildren = "wellbore-has-children";
    public const string CodeLastBore = "wellbore-last";
    public const string CodeDuplicate = "wellbore-exists";
    public const string CodeInvalidKind = "wellbore-kind-invalid";
    public const string CodeActorRequired = "wellbore-actor-required";

    private static readonly string[] Kinds = ["ORIGINAL", "SIDETRACK", "REDRILL"];

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellbores(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<IReadOnlyList<OgWellboreSummary>>> ListAsync(
        string uwi,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uwi);

        var source = _source();

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgWellboreSummary>>.Failed(
                "No connection is open, so the wellbores cannot be read.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<WELLBORE>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken)
                .ConfigureAwait(false);

            var bores = rows
                .OrderBy(row => row.KIND, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.WELLBORE_ID, StringComparer.OrdinalIgnoreCase)
                .Select(ToSummary)
                .ToList();

            return OgResult<IReadOnlyList<OgWellboreSummary>>.Ok(bores);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"listing the wellbores of {uwi}",
                "the wellbore list is not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgWellboreSummary>>.Failed(
                "The wellbores could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellboreSummary>> CreateAsync(
        OgWellboreRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellboreSummary>.Failed(
                "A wellbore change must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(request.Uwi) || string.IsNullOrWhiteSpace(request.Id))
        {
            return OgResult<OgWellboreSummary>.Failed(
                "A wellbore needs its well and its own identifier.", "wellbore-identity-required");
        }

        var kind = string.IsNullOrWhiteSpace(request.Kind) ? "ORIGINAL" : request.Kind.Trim().ToUpperInvariant();

        if (!Kinds.Contains(kind, StringComparer.Ordinal))
        {
            return OgResult<OgWellboreSummary>.Failed(
                $"'{request.Kind}' is not a wellbore kind; use ORIGINAL, SIDETRACK or REDRILL.", CodeInvalidKind);
        }

        // AC-5: a sidetrack (and a re-drill) names the bore it came from.
        if (kind is "SIDETRACK" or "REDRILL" && string.IsNullOrWhiteSpace(request.ParentId))
        {
            return OgResult<OgWellboreSummary>.Failed(
                $"A {kind.ToLowerInvariant()} must name the wellbore it comes from.", CodeParentBoreRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellboreSummary>.Failed(
                "No connection is open, so the wellbore cannot be created.", CodeNoConnection);
        }

        try
        {
            var wellUnit = OgUow.For<WELL>(source);
            var wells = await OgUow.ReadAsync(wellUnit, [OgUow.Equal("UWI", request.Uwi)], cancellationToken)
                .ConfigureAwait(false);

            // AC-2
            if (wells.Count == 0)
            {
                return OgResult<OgWellboreSummary>.Failed(
                    $"Well '{request.Uwi}' does not exist, so a wellbore cannot be created under it.",
                    CodeParentWellMissing);
            }

            var unit = OgUow.For<WELLBORE>(source);
            var siblings = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", request.Uwi)], cancellationToken)
                .ConfigureAwait(false);

            if (siblings.Any(bore =>
                    string.Equals(bore.WELLBORE_ID, request.Id.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgWellboreSummary>.Failed(
                    $"Wellbore '{request.Id}' already exists on this well.", CodeDuplicate);
            }

            if (!string.IsNullOrWhiteSpace(request.ParentId)
                && !siblings.Any(bore =>
                    string.Equals(bore.WELLBORE_ID, request.ParentId.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgWellboreSummary>.Failed(
                    $"Parent wellbore '{request.ParentId}' is not on this well.", CodeParentBoreUnknown);
            }

            var now = DateTime.UtcNow;

            var bore = new WELLBORE
            {
                UWI = request.Uwi.Trim(),
                WELLBORE_ID = request.Id.Trim(),
                WELLBORE_NAME = Trimmed(request.Name),
                PARENT_WELLBORE_ID = Trimmed(request.ParentId),
                KIND = kind,
                STATUS = Trimmed(request.Status),
                SPUD_DATE = request.SpudDate,
                TOTAL_DEPTH = request.TotalDepth,
                TOTAL_DEPTH_OUOM = Trimmed(request.DepthUnit),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            };

            unit.Add(bore);
            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult<OgWellboreSummary>.Ok(new OgWellboreSummary(
                    bore.UWI,
                    bore.WELLBORE_ID,
                    bore.WELLBORE_NAME,
                    bore.PARENT_WELLBORE_ID,
                    bore.KIND ?? kind,
                    bore.STATUS,
                    bore.SPUD_DATE,
                    bore.TOTAL_DEPTH,
                    bore.TOTAL_DEPTH_OUOM))
                : OgResult<OgWellboreSummary>.Failed(
                    OgUow.Message(committed, "The wellbore was not created."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating wellbore {request.Id} on {request.Uwi}",
                "the wellbore was not created",
                FailureSeverity.Error);

            return OgResult<OgWellboreSummary>.Failed(
                "The wellbore could not be created.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult> DeleteAsync(
        string uwi,
        string wellboreId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult.Failed("A wellbore change must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed("No connection is open, so the wellbore cannot be deleted.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<WELLBORE>(source);
            var bores = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken)
                .ConfigureAwait(false);

            var bore = bores.FirstOrDefault(row =>
                string.Equals(row.WELLBORE_ID, wellboreId, StringComparison.OrdinalIgnoreCase));

            if (bore is null)
            {
                return OgResult.Failed($"Wellbore '{wellboreId}' is not on this well.", "wellbore-unknown");
            }

            if (bores.Count <= 1)
            {
                return OgResult.Failed(
                    "A well must keep at least one wellbore; create another before deleting this one.", CodeLastBore);
            }

            var surveys = await OgUow.ReadAsync(
                OgUow.For<WELLBORE_SURVEY>(source),
                [OgUow.Equal("UWI", uwi)],
                cancellationToken).ConfigureAwait(false);

            var linked = surveys.Count(row =>
                string.Equals(row.WELLBORE_ID, wellboreId, StringComparison.OrdinalIgnoreCase));

            if (linked > 0)
            {
                // AC-3: the count is part of the refusal, so the person knows what to unlink.
                return OgResult.Failed(
                    $"Wellbore '{wellboreId}' has {linked} survey(s) linked; remove the link before deleting it.",
                    CodeHasSurveys);
            }

            if (bores.Any(row =>
                    string.Equals(row.PARENT_WELLBORE_ID, wellboreId, StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult.Failed(
                    $"Wellbore '{wellboreId}' is the parent of another bore; reassign it before deleting.",
                    CodeHasChildren);
            }

            var deleted = unit.Delete(bore);

            if (!OgUow.IsOk(deleted))
            {
                return OgResult.Failed(
                    OgUow.Message(deleted, "The wellbore could not be deleted."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok("Wellbore deleted.")
                : OgResult.Failed(OgUow.Message(committed, "The wellbore could not be deleted."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"deleting wellbore {wellboreId} on {uwi}",
                "the wellbore was not deleted",
                FailureSeverity.Error);

            return OgResult.Failed("The wellbore could not be deleted.", reference ?? CodeWriteFailed);
        }
    }

    private static OgWellboreSummary ToSummary(WELLBORE row) =>
        new(
            Uwi: row.UWI ?? string.Empty,
            Id: row.WELLBORE_ID ?? string.Empty,
            Name: Blank(row.WELLBORE_NAME),
            ParentId: Blank(row.PARENT_WELLBORE_ID),
            Kind: string.IsNullOrWhiteSpace(row.KIND) ? "ORIGINAL" : row.KIND,
            Status: Blank(row.STATUS),
            SpudDate: row.SPUD_DATE,
            TotalDepth: row.TOTAL_DEPTH,
            DepthUnit: Blank(row.TOTAL_DEPTH_OUOM));

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
