namespace TheTechIdea.Data.OilGas;

public sealed record PermitApplicationCatalog(IReadOnlyList<string> ApplicationTypes, IReadOnlyList<string> Authorities, IReadOnlyList<string> Countries, IReadOnlyList<string> Regions);
