using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

public sealed class RepositoryRoleCatalogService(RepositoryDbContext db, RoleManager<IdentityRole> roles)
{
    public async Task<List<RepositoryRoleSummary>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await (from role in db.Roles.AsNoTracking()
               join extension in db.Set<AppRoleExtension>().AsNoTracking() on role.Id equals extension.RoleId into metadata
               from extension in metadata.DefaultIfEmpty()
               orderby role.Name
               select new RepositoryRoleSummary(role.Id, role.Name ?? "", extension == null ? null : extension.Description))
            .ToListAsync(cancellationToken);

    public async Task<RepositoryRoleSummary> CreateAsync(RepositoryRoleRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.RoleName?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 256 || name.Contains(',') || request.Description?.Length > 1000)
            throw new ArgumentException("A role name of up to 256 characters without commas and a description of up to 1000 characters are required.");
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var role = new IdentityRole(name);
        var result = await roles.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Code)));
        db.Add(new AppRoleExtension { RoleId = role.Id, Description = request.Description,
            SystemRoleInd = "N", CreatedUtc = DateTime.UtcNow });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(role.Id, name, request.Description);
    }
}
