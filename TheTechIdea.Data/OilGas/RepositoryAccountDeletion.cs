namespace TheTechIdea.Data.OilGas;

/// <summary>
/// Whether the caller may delete their account, asked by the Web's account page before the identity server deletes
/// anything: <paramref name="Refusal"/> is null when they may, or the sentence the page shows them when they may not.
/// </summary>
public sealed record RepositoryAccountDeletion(string? Refusal);
