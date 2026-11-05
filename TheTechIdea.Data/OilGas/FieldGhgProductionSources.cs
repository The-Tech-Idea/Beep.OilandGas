namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded production entities linked to one FIELD; no emissions or reporting coverage is inferred.</summary>
public sealed record GhgProductionEntity(string Subtype, string ProductionEntityId, string Source, string? Name, string? PrimaryProduct);
public sealed record FieldGhgProductionSources(FieldSelectionReview Field, GhgProductionEntity[] Entities, string SourceRevision);

public sealed record GhgDispositionQuery(string Subtype, string ProductionEntityId, string Source, string ExpectedSourceRevision);
public sealed record GhgDispositionFact(string VolumeMethod, string ActivityType, string ProductType, string PeriodType,
    decimal Observation, decimal Amendment, decimal Volume, string Unit, DateTime? StartDate, DateTime? EndDate);
/// <summary>Recorded rows for one reviewed production entity; no emissions or period completeness is inferred.</summary>
public sealed record FieldGhgDispositions(FieldSelectionReview Field, GhgDispositionQuery Query, GhgDispositionFact[] Dispositions, string SourceRevision);
