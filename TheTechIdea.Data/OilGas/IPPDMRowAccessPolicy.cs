using TheTechIdea.Beep.Report;

namespace TheTechIdea.Data.OilGas;

public enum PPDMRowMutation { Insert, Update, Delete }

/// <summary>Mandatory company row checks supplied by the API to the shared Beep repository.</summary>
public interface IPPDMRowAccessPolicy
{
    // OR between groups, AND within each group; no groups means no readable rows. Groups must be nonempty.
    Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type entityType, string tableName);
    // Every constraint is ANDed with each grant predicate and caller filter. These never grant access alone.
    Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type entityType, string tableName);
    // Aggregate owners must prove that scoped reads do not omit withheld records.
    Task RequireCompleteReadScopeAsync(Type entityType, string tableName, string fieldId)
        => throw new InvalidOperationException("This row policy cannot prove a complete readable population.");
    Task<HseRestrictionReview> ReviewHseRestrictionsAsync(FieldSelectionReview review, string incidentId)
        => throw new InvalidOperationException("This row policy does not own HSE restriction review.");
    Task ValidateRowsAsync(Type entityType, string tableName, IReadOnlyList<object> rows);
    Task<string?> GetEffectiveAccessLevelAsync(Type entityType, string tableName, object row, bool includeInherited = true);
    Task ValidateMutationAsync(Type entityType, string tableName, object entity, object? previous,
        PPDMRowMutation operation, string? actor);
}

/// <summary>Uses explicit caller predicates to bound a read's authorization work, never to grant authority.</summary>
public interface IPPDMFilteredReadPolicy : IPPDMRowAccessPolicy
{
    Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsForFiltersAsync(Type entityType, string tableName,
        IReadOnlyList<AppFilter> requestedFilters);
    Task ValidateRowsForFiltersAsync(Type entityType, string tableName, IReadOnlyList<AppFilter> requestedFilters,
        IReadOnlyList<object> rows);
}
