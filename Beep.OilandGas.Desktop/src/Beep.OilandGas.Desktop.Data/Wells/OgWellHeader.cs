using System.Globalization;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

/// <summary>One alternate identity of a well (WEL-02 "Identifiers").</summary>
public sealed record OgWellIdentifier(string XrefId, string Type, string? Source, bool Conflict);

/// <summary>One participant of a well, from WELL_BA_SERVICE joined to BUSINESS_ASSOCIATE.</summary>
public sealed record OgWellParticipant(
    string BusinessAssociateId,
    string Name,
    string? Description,
    bool Active,
    DateTime? EffectiveDate,
    DateTime? EndDate);

/// <summary>
/// WEL-02: the well header - identity, location, classification and dates, plus its identifiers and participants.
/// It mirrors the PPDM columns; it is not a new entity.
/// </summary>
public sealed record OgWellHeader(
    string Uwi,
    string? Name,
    string? GovernmentId,
    string? ApiNumber,
    string? Field,
    string? Operator,
    decimal? SurfaceLatitude,
    decimal? SurfaceLongitude,
    decimal? GroundElevation,
    decimal? KellyBushing,
    decimal? FinalTd,
    string? WellType,
    string? Status,
    string? WellClass,
    string? Profile,
    DateTime? SpudDate,
    DateTime? CompletionDate,
    DateTime? AbandonmentDate,
    DateTime? LastActivity)
{
    /// <summary>AC-6: status belongs to WEL-04; the header never writes it.</summary>
    public bool StatusIsReadOnly => true;

    /// <summary>The warning chip when no coordinates are recorded.</summary>
    public bool HasLocation => SurfaceLatitude is not null && SurfaceLongitude is not null;

    /// <summary>True when two identifiers of the same type disagree and no choice has been made.</summary>
    public bool HasAmbiguousIdentifiers { get; init; }
}

/// <summary>Everything the header page shows for one well.</summary>
public sealed record OgWellHeaderBundle(
    OgWellHeader Header,
    IReadOnlyList<OgWellIdentifier> Identifiers,
    IReadOnlyList<OgWellParticipant> Participants)
{
    /// <summary>The identity types that carry more than one distinct value.</summary>
    public IReadOnlyList<string> AmbiguousTypes =>
        Identifiers.Where(identifier => identifier.Conflict)
            .Select(identifier => identifier.Type)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}

/// <summary>WEL-02: reading and writing one well's header, identifiers and participants.</summary>
public interface IOgWellHeader
{
    Task<OgResult<OgWellHeaderBundle>> LoadAsync(string uwi, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the changed columns only, guarded by the values the header was read with (AC-1). Status is never
    /// written (AC-6), the operator cannot be cleared (AC-5), and a location change needs the coordinate system
    /// that produced it (AC-3).
    /// </summary>
    Task<OgResult> SaveAsync(
        OgWellHeader updated,
        OgWellHeader original,
        string? coordinateSystem,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult> AddParticipantAsync(
        string uwi,
        string businessAssociateId,
        string? description,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a participant; the operator's own row is refused (AC-5).</summary>
    Task<OgResult> RemoveParticipantAsync(
        string uwi,
        string businessAssociateId,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The header over <c>WELL</c>, <c>WELL_XREF</c>, <c>WELL_BA_SERVICE</c> and <c>BUSINESS_ASSOCIATE</c>, read and
/// written through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// The row is re-read inside the unit of work and every column the record carries is compared with what the page
/// started from; a row somebody else changed is reported as a conflict instead of overwritten. Only the changed
/// properties are assigned, so an untouched column keeps the value it was read with (AC-1).
/// </remarks>
public sealed class OgWellHeaderService : IOgWellHeader
{
    public const string CodeConflict = "well-header-conflict";
    public const string CodeReadFailed = "well-header-read-failed";
    public const string CodeWriteFailed = "well-header-write-failed";
    public const string CodeNoConnection = "well-header-no-connection";
    public const string CodeOperatorRequired = "well-operator-required";
    public const string CodeCoordinateSystemRequired = "well-coordinate-system-required";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellHeaderService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgWellHeaderBundle>> LoadAsync(
        string uwi,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uwi);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellHeaderBundle>.Failed(
                "No connection is open, so the well cannot be read.", CodeNoConnection);
        }

        try
        {
            var identifiers = await ReadIdentifiersAsync(source, uwi, cancellationToken).ConfigureAwait(false);
            var header = await ReadHeaderAsync(source, uwi, identifiers, cancellationToken).ConfigureAwait(false);

            if (header is null)
            {
                return OgResult<OgWellHeaderBundle>.Failed(
                    $"Well '{uwi}' is not in this database.", "well-header-unknown");
            }

            var participants = await ReadParticipantsAsync(source, uwi, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWellHeaderBundle>.Ok(
                new OgWellHeaderBundle(header, identifiers, participants));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading the header of well {uwi}",
                "the well header is not shown",
                FailureSeverity.Error);

            return OgResult<OgWellHeaderBundle>.Failed(
                "The well header could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult> SaveAsync(
        OgWellHeader updated,
        OgWellHeader original,
        string? coordinateSystem,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updated);
        ArgumentNullException.ThrowIfNull(original);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult.Failed("A change must record who made it.", "well-actor-required");
        }

        // AC-5: the operator is part of the well's identity; clearing it is not an edit.
        if (string.IsNullOrWhiteSpace(updated.Operator))
        {
            return OgResult.Failed(
                "A well must keep an operator. Change it to another business associate instead of clearing it.",
                CodeOperatorRequired);
        }

        var locationChanged =
            updated.SurfaceLatitude != original.SurfaceLatitude
            || updated.SurfaceLongitude != original.SurfaceLongitude;

        // AC-3: a coordinate without the system it was measured in is not a location.
        if (locationChanged && string.IsNullOrWhiteSpace(coordinateSystem))
        {
            return OgResult.Failed(
                "A location change must record the coordinate system it used.",
                CodeCoordinateSystemRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed(
                "No connection is open, so the well cannot be saved.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<WELL>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", updated.Uwi)], cancellationToken)
                .ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult.Failed($"Well '{updated.Uwi}' is not in this database.", "well-header-unknown");
            }

            if (!SameAsRead(row, original))
            {
                return OgResult.Failed(
                    "The well changed since it was read; reload the header before saving.", CodeConflict);
            }

            if (!Apply(row, updated, original))
            {
                return OgResult.Ok("No change to save.");
            }

            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updatedResult = unit.Update(row);

            if (!OgUow.IsOk(updatedResult))
            {
                return OgResult.Failed(
                    OgUow.Message(updatedResult, "The well could not be saved."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok("Well saved.")
                : OgResult.Failed(OgUow.Message(committed, "The well could not be saved."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving the header of well {updated.Uwi}",
                "the change was not applied",
                FailureSeverity.Error);

            return OgResult.Failed("The well could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult> AddParticipantAsync(
        string uwi,
        string businessAssociateId,
        string? description,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessAssociateId))
        {
            return OgResult.Failed("A participant needs a business associate.", "well-participant-required");
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult.Failed("A participant change must record who made it.", "well-actor-required");
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed(
                "No connection is open, so the participant cannot be added.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<WELL_BA_SERVICE>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken)
                .ConfigureAwait(false);
            var now = DateTime.UtcNow;

            var participant = new WELL_BA_SERVICE
            {
                UWI = uwi,
                PROVIDED_BY = businessAssociateId.Trim(),
                SERVICE_SEQ_NO = rows.Count == 0 ? 1 : rows.Max(row => row.SERVICE_SEQ_NO) + 1,
                ACTIVE_IND = "Y",
                DESCRIPTION = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            };

            unit.Add(participant);

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok("Participant added.")
                : OgResult.Failed(OgUow.Message(committed, "The participant could not be added."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"adding a participant to well {uwi}",
                "the participant was not added",
                FailureSeverity.Error);

            return OgResult.Failed("The participant could not be added.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult> RemoveParticipantAsync(
        string uwi,
        string businessAssociateId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult.Failed("A participant change must record who made it.", "well-actor-required");
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed(
                "No connection is open, so the participant cannot be removed.", CodeNoConnection);
        }

        try
        {
            var wellUnit = OgUow.For<WELL>(source);
            var well = (await OgUow.ReadAsync(wellUnit, [OgUow.Equal("UWI", uwi)], cancellationToken)
                .ConfigureAwait(false)).FirstOrDefault();

            // AC-5
            if (!string.IsNullOrWhiteSpace(well?.OPERATOR)
                && string.Equals(well.OPERATOR, businessAssociateId, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult.Failed(
                    "The operator cannot be removed from the well's participants. Assign another operator first.",
                    CodeOperatorRequired);
            }

            var unit = OgUow.For<WELL_BA_SERVICE>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken)
                .ConfigureAwait(false);

            var participant = rows.FirstOrDefault(row =>
                string.Equals(row.PROVIDED_BY, businessAssociateId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (participant is null)
            {
                return OgResult.Ok("Participant removed.");
            }

            var deleted = unit.Delete(participant);

            if (!OgUow.IsOk(deleted))
            {
                return OgResult.Failed(
                    OgUow.Message(deleted, "The participant could not be removed."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok("Participant removed.")
                : OgResult.Failed(OgUow.Message(committed, "The participant could not be removed."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"removing a participant from well {uwi}",
                "the participant was not removed",
                FailureSeverity.Error);

            return OgResult.Failed("The participant could not be removed.", reference ?? CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ reads

    private static async Task<OgWellHeader?> ReadHeaderAsync(
        IDataSource source,
        string uwi,
        IReadOnlyList<OgWellIdentifier> identifiers,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<WELL>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken).ConfigureAwait(false);
        var row = rows.FirstOrDefault();

        if (row is null)
        {
            return null;
        }

        return new OgWellHeader(
            Uwi: row.UWI ?? uwi,
            Name: Blank(row.WELL_NAME),
            GovernmentId: Blank(row.WELL_GOVERNMENT_ID),
            ApiNumber: ChosenIdentifier(identifiers, "API"),
            Field: Blank(row.ASSIGNED_FIELD),
            Operator: Blank(row.OPERATOR),
            SurfaceLatitude: Coordinate(row.SURFACE_LATITUDE),
            SurfaceLongitude: Coordinate(row.SURFACE_LONGITUDE),
            GroundElevation: row.GROUND_ELEV,
            KellyBushing: row.KB_ELEV,
            FinalTd: row.FINAL_TD == 0 ? null : row.FINAL_TD,
            WellType: Blank(row.WELL_LEVEL_TYPE),
            Status: Blank(row.CURRENT_STATUS),
            WellClass: Blank(row.CURRENT_CLASS),
            Profile: Blank(row.PROFILE_TYPE),
            SpudDate: row.SPUD_DATE,
            CompletionDate: row.COMPLETION_DATE,
            AbandonmentDate: row.ABANDONMENT_DATE,
            LastActivity: row.CURRENT_STATUS_DATE)
        {
            HasAmbiguousIdentifiers = identifiers.Any(identifier => identifier.Conflict),
        };
    }

    /// <summary>
    /// AC-2: an identity type with more than one distinct value is conflicting, and no value is chosen for it.
    /// </summary>
    public static string? ChosenIdentifier(IReadOnlyList<OgWellIdentifier> identifiers, string type)
    {
        ArgumentNullException.ThrowIfNull(identifiers);

        var matching = identifiers
            .Where(identifier => string.Equals(identifier.Type, type, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matching.Count == 1 && !matching[0].Conflict ? matching[0].XrefId : null;
    }

    private static async Task<List<OgWellIdentifier>> ReadIdentifiersAsync(
        IDataSource source,
        string uwi,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<WELL_XREF>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken).ConfigureAwait(false);

        var conflicts = rows
            .GroupBy(row => row.XREF_TYPE ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(row => row.XREF_ID ?? string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return rows
            .OrderBy(row => row.XREF_TYPE, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.XREF_ID, StringComparer.OrdinalIgnoreCase)
            .Select(row => new OgWellIdentifier(
                row.XREF_ID ?? string.Empty,
                row.XREF_TYPE ?? string.Empty,
                Blank(row.SOURCE),
                conflicts.Contains(row.XREF_TYPE ?? string.Empty)))
            .ToList();
    }

    private static async Task<List<OgWellParticipant>> ReadParticipantsAsync(
        IDataSource source,
        string uwi,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<WELL_BA_SERVICE>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken).ConfigureAwait(false);

        var associates = await OgUow.ReadAllAsync<BUSINESS_ASSOCIATE>(source).ConfigureAwait(false);
        var names = associates
            .Where(associate => !string.IsNullOrWhiteSpace(associate.BUSINESS_ASSOCIATE_ID))
            .GroupBy(associate => associate.BUSINESS_ASSOCIATE_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return rows
            .OrderBy(row => row.SERVICE_SEQ_NO)
            .Select(row =>
            {
                var id = row.PROVIDED_BY ?? string.Empty;
                names.TryGetValue(id, out var associate);

                return new OgWellParticipant(
                    id,
                    Blank(associate?.BA_LONG_NAME) ?? Blank(associate?.BA_SHORT_NAME) ?? id,
                    Blank(row.DESCRIPTION),
                    string.Equals(row.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase),
                    row.EFFECTIVE_DATE,
                    row.END_DATE);
            })
            .ToList();
    }

    // ----------------------------------------------------------------- writes

    /// <summary>Applies the changed properties and reports whether anything changed.</summary>
    private static bool Apply(WELL row, OgWellHeader updated, OgWellHeader original)
    {
        var changed = false;

        changed |= Set(original.Name, updated.Name, value => row.WELL_NAME = value);
        changed |= Set(original.GovernmentId, updated.GovernmentId, value => row.WELL_GOVERNMENT_ID = value);
        changed |= Set(original.Field, updated.Field, value => row.ASSIGNED_FIELD = value);
        changed |= Set(original.Operator, updated.Operator, value => row.OPERATOR = value);
        changed |= Set(original.SurfaceLatitude, updated.SurfaceLatitude, value => row.SURFACE_LATITUDE = value ?? 0);
        changed |= Set(original.SurfaceLongitude, updated.SurfaceLongitude, value => row.SURFACE_LONGITUDE = value ?? 0);
        changed |= Set(original.GroundElevation, updated.GroundElevation, value => row.GROUND_ELEV = value ?? 0);
        changed |= Set(original.KellyBushing, updated.KellyBushing, value => row.KB_ELEV = value ?? 0);
        changed |= Set(original.FinalTd, updated.FinalTd, value => row.FINAL_TD = value ?? 0);

        // Current status, well class and profile are not written here: status changes belong to WEL-04 (AC-6), and a
        // header edit does not reclassify a well.

        return changed;
    }

    /// <summary>True when the row still carries every value the page started from.</summary>
    private static bool SameAsRead(WELL row, OgWellHeader original) =>
        Same(Blank(row.WELL_NAME), Normalise(original.Name))
        && Same(Blank(row.WELL_GOVERNMENT_ID), Normalise(original.GovernmentId))
        && Same(Blank(row.ASSIGNED_FIELD), Normalise(original.Field))
        && Same(Blank(row.OPERATOR), Normalise(original.Operator))
        && Same(Coordinate(row.SURFACE_LATITUDE), original.SurfaceLatitude)
        && Same(Coordinate(row.SURFACE_LONGITUDE), original.SurfaceLongitude)
        && Same(row.GROUND_ELEV, original.GroundElevation)
        && Same(row.KB_ELEV, original.KellyBushing)
        && Same(row.FINAL_TD == 0 ? null : row.FINAL_TD, original.FinalTd);

    private static bool Same(object? left, object? right) =>
        string.Equals(
            Convert.ToString(left, CultureInfo.InvariantCulture),
            Convert.ToString(right, CultureInfo.InvariantCulture),
            StringComparison.Ordinal);

    private static bool Set<T>(T original, T updated, Action<T> assign)
    {
        if (Equals(Normalise(original), Normalise(updated)))
        {
            return false;
        }

        assign(updated);
        return true;
    }

    private static object? Normalise(object? value) => value switch
    {
        null => null,
        string text => text.Trim(),
        _ => value,
    };

    /// <summary>
    /// The PPDM entity declares coordinates as a non-nullable decimal, so an unrecorded coordinate arrives as
    /// zero. Zero is not a well location and is reported as missing; a recorded zero elevation stays zero.
    /// </summary>
    private static decimal? Coordinate(decimal value) => value == 0 ? null : value;

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
