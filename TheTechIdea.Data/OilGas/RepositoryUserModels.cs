using System.ComponentModel.DataAnnotations;

namespace TheTechIdea.Data.OilGas;

public sealed record RepositoryUserSummary(string UserId, string UserName, string? Email,
    string? FullName, bool IsActive, string ConcurrencyStamp);

public sealed record RepositoryUserUpdate(
    [property: MaxLength(1000)] string? FullName,
    bool? IsActive,
    [property: Required] string ConcurrencyStamp);
