namespace TheTechIdea.Data.OilGas;

public sealed record RepositoryUserAccess(string UserId, bool IsActive, string[] Roles, string[] Permissions);
