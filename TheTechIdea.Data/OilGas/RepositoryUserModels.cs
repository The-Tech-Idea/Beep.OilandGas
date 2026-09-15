using System.ComponentModel.DataAnnotations;

namespace TheTechIdea.Data.OilGas;

public sealed record RepositoryUserSummary(string UserId, string UserName, string? Email,
    string? FullName, bool IsActive, string ConcurrencyStamp);

public sealed record RepositoryUserUpdate(
    [MaxLength(1000)] string? FullName,
    bool? IsActive,
    [Required] string ConcurrencyStamp);
