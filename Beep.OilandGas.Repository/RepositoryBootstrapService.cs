using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

public sealed class RepositoryBootstrapService(
    RepositoryDbContext context,
    UserManager<OilGasUser> users,
    RoleManager<IdentityRole> roles)
{
    public const string AdministratorRole = "Administrator";

    public async Task<BootstrapOutcome> BootstrapAsync(string issuer, string subject,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(subject)
            || subject.Length > 128)
            return BootstrapOutcome.NotAllowed;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var completed = await context.Bootstrap.AnyAsync(x => x.Id == 1, cancellationToken);
        // Existing installations without a marker require operator reconciliation,
        // not promotion of the next arriving user.
        if (!completed && await context.Users.AnyAsync(cancellationToken))
            return BootstrapOutcome.NotAllowed;

        // Hash only the provider identifier to fit every provider's Identity key limit.
        // The external subject is preserved exactly, never derived from an email address.
        var loginProvider = ExternalLoginProvider(issuer);
        var user = await users.FindByLoginAsync(loginProvider, subject);
        if (user is not null)
            return user.IsActive ? BootstrapOutcome.AlreadyCompleted : BootstrapOutcome.NotAllowed;
        if (user is null)
        {
            user = new OilGasUser { UserName = $"external-{Guid.NewGuid():N}" };
            RequireSuccess(await users.CreateAsync(user));
            RequireSuccess(await users.AddLoginAsync(user, new UserLoginInfo(loginProvider, subject, "OIDC")));
            context.Add(new AppUserExtension { UserId = user.Id, CreatedUtc = DateTime.UtcNow,
                ChangedUtc = DateTime.UtcNow, ChangedBy = user.Id });
            await context.SaveChangesAsync(cancellationToken);
        }
        if (!user.IsActive)
            return BootstrapOutcome.NotAllowed;

        if (completed)
        {
            await transaction.CommitAsync(cancellationToken);
            return BootstrapOutcome.Registered;
        }

        if (!await roles.RoleExistsAsync(AdministratorRole))
            RequireSuccess(await roles.CreateAsync(new IdentityRole(AdministratorRole)));
        var adminRole = (await roles.FindByNameAsync(AdministratorRole))!;
        // Identity name lookup is normalized; ASP.NET role claims use the display name.
        if (!string.Equals(adminRole.Name, AdministratorRole, StringComparison.Ordinal))
        {
            RequireSuccess(await roles.SetRoleNameAsync(adminRole, AdministratorRole));
            RequireSuccess(await roles.UpdateAsync(adminRole));
        }
        if (!await users.IsInRoleAsync(user, AdministratorRole))
            RequireSuccess(await users.AddToRoleAsync(user, AdministratorRole));
        if (!await context.Set<AppRoleExtension>().AnyAsync(x => x.RoleId == adminRole.Id, cancellationToken))
            context.Add(new AppRoleExtension { RoleId = adminRole.Id, SystemRoleInd = "Y", CreatedUtc = DateTime.UtcNow });
        context.Add(new AppUserRoleExtension
        {
            UserId = user.Id, RoleId = adminRole.Id, GrantedByUserId = user.Id,
            AssignmentReason = "First OilGas registration", EffectiveFromUtc = DateTime.UtcNow
        });

        // The singleton PK arbitrates competing requests. A loser rolls back all writes.
        context.Bootstrap.Add(new RepositoryBootstrap
        {
            Id = 1,
            AdministratorUserId = user.Id,
            CompletedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return BootstrapOutcome.Created;
    }

    public static string ExternalLoginProvider(string issuer) =>
        "oidc:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(issuer)));

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Repository bootstrap Identity operation failed: " +
                string.Join(", ", result.Errors.Select(error => error.Code)));
    }
}
