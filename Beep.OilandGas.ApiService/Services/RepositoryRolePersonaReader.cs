using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryRolePersonaReader(RepositoryDbContext repository, ILookupNormalizer normalizer) : IRolePersonaReader
{
    public async Task<List<string>> GetActivePersonasAsync(string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var normalized = normalizer.NormalizeName(roleName);
        var role = await repository.Roles.AsNoTracking().SingleOrDefaultAsync(x => x.NormalizedName == normalized)
            ?? throw new InvalidOperationException("The assigned role does not exist in the default repository.");
        return await (from membership in repository.UserRoles.AsNoTracking()
                      join user in repository.Users.AsNoTracking() on membership.UserId equals user.Id
                      join profile in repository.Set<AppUserPersona>().AsNoTracking() on user.Id equals profile.UserId
                      join persona in repository.Set<AppPersona>().AsNoTracking() on profile.PersonaCode equals persona.Code
                      where membership.RoleId == role.Id && user.IsActive && persona.IsActive
                      select persona.Code).Distinct().OrderBy(x => x).ToListAsync();
    }
}
