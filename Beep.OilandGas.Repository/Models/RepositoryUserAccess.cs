using Beep.OilandGas.Repository.Models;
namespace Beep.OilandGas.Repository.Models;

public sealed record RepositoryUserAccess(string UserId, bool IsActive, string[] Roles, string[] Permissions);
