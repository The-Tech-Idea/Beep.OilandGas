using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.Repository;

public sealed partial class RepositoryPersonaService
{
    // Navigation metadata on the existing app role owner. Never issued as a permission or role claim.
    public const string WorkspaceClaimType = "workspace-persona";

    public async Task<CompanyPersonaWorkspaceReview> ReviewWorkspacesAsync(string actor, CompanyScope scope,
        IRepositoryAccessService access, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actor); ArgumentNullException.ThrowIfNull(scope); ArgumentNullException.ThrowIfNull(access);
        async Task<(string[] Roles, AppPersona[] Personas, string Evidence)> CaptureAsync()
        {
            var authority = await access.GetCompanyAccessAsync(actor, scope, token);
            if (authority is null || authority.Scope != scope) throw new UnauthorizedAccessException("Current approved company membership is required.");
            var roles = authority.Roles.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            var mappings = await (from claim in db.RoleClaims.AsNoTracking()
                join role in db.Roles.AsNoTracking() on claim.RoleId equals role.Id
                where claim.ClaimType == WorkspaceClaimType && role.Name != null && roles.Contains(role.Name)
                select new { claim.Id, claim.RoleId, claim.ClaimValue }).ToArrayAsync(token);
            if (mappings.Length > 512) throw new InvalidOperationException("Workspace role mappings exceed the supported review limit.");
            var codes = mappings.Select(row => row.ClaimValue).Where(code => code is not null).Distinct().ToArray();
            var personas = await db.Set<AppPersona>().AsNoTracking().Where(row => row.IsActive && codes.Contains(row.Code))
                .OrderBy(row => row.DisplayOrder).ThenBy(row => row.Code).ToArrayAsync(token);
            if (personas.Length > 128) throw new InvalidOperationException("Eligible workspaces exceed the supported review limit.");
            var evidence = JsonSerializer.Serialize(new { Roles = roles, Mappings = mappings.OrderBy(row => row.Id), Personas = personas });
            return (roles, personas, evidence);
        }
        var initial = await CaptureAsync(); var closing = await CaptureAsync();
        if (initial.Evidence != closing.Evidence) throw new InvalidOperationException("Company roles or workspace configuration changed during review. Reload company access.");
        var revision = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new { Actor = actor, Company = scope, initial.Evidence }))));
        return new(scope, actor, Array.AsReadOnly(initial.Roles), Array.AsReadOnly(initial.Personas), revision);
    }

    public async Task SaveRoleWorkspaceAsync(string roleId, string code, bool enabled, string expectedRevision, string actor,
        IRepositoryAccessService access, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId); ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor); ArgumentNullException.ThrowIfNull(access);
        await RequireAdministratorAsync(actor, access, token);
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, token);
        var reviewed = await ReadRoleWorkspaceAsync(roleId, token);
        if (string.IsNullOrEmpty(expectedRevision) || reviewed.Revision != expectedRevision)
            throw new InvalidOperationException("Role workspace mappings changed. Reload before saving.");
        var role = await db.Roles.AsNoTracking().SingleOrDefaultAsync(row => row.Id == roleId, token)
            ?? throw new ArgumentException("The app role is unavailable.");
        var persona = await db.Set<AppPersona>().AsNoTracking().SingleOrDefaultAsync(row => row.Code == code, token)
            ?? throw new ArgumentException("The persona is unavailable.");
        if (enabled && !persona.IsActive) throw new ArgumentException("Only an active persona may be mapped.");
        var existing = await db.RoleClaims.Where(row => row.RoleId == roleId && row.ClaimType == WorkspaceClaimType && row.ClaimValue == code).ToArrayAsync(token);
        if (existing.Length > 1) throw new InvalidOperationException("Duplicate workspace mappings require explicit reconciliation.");
        if (enabled != (existing.Length == 1))
        {
            if (enabled) db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = roleId, ClaimType = WorkspaceClaimType, ClaimValue = code });
            else db.RoleClaims.Remove(existing[0]);
            db.Set<AppPersonaAudit>().Add(new AppPersonaAudit { UserId = actor, ActorUserId = actor, Action = "RoleWorkspaceMapping",
                BeforeJson = JsonSerializer.Serialize(existing.Select(row => new { row.Id, row.RoleId, row.ClaimType, row.ClaimValue })),
                AfterJson = JsonSerializer.Serialize(new { RoleId = role.Id, PersonaCode = persona.Code, Enabled = enabled }), ChangedUtc = DateTime.UtcNow });
            await db.SaveChangesAsync(token);
        }
        await RequireAdministratorAsync(actor, access, token);
        await transaction.CommitAsync(token);
    }

    private static async Task RequireAdministratorAsync(string actor, IRepositoryAccessService access, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actor); ArgumentNullException.ThrowIfNull(access);
        var current = await access.GetAccessAsync(actor, token);
        if (current is not { IsActive: true } || !current.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw new UnauthorizedAccessException("A current repository Administrator is required to configure role workspace mappings.");
    }

    public async Task<RoleWorkspaceReview> ReviewRoleWorkspaceAsync(string roleId, string actor,
        IRepositoryAccessService access, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        await RequireAdministratorAsync(actor, access, token);
        var initial = await ReadRoleWorkspaceAsync(roleId, token);
        var closing = await ReadRoleWorkspaceAsync(roleId, token);
        await RequireAdministratorAsync(actor, access, token);
        if (initial.Revision != closing.Revision) throw new InvalidOperationException("Role workspace mappings changed during review.");
        return initial;
    }
    private async Task<RoleWorkspaceReview> ReadRoleWorkspaceAsync(string roleId, CancellationToken token)
    {
        var role = await db.Roles.AsNoTracking().SingleOrDefaultAsync(row => row.Id == roleId, token)
            ?? throw new ArgumentException("The app role is unavailable.");
        var mappings = await db.RoleClaims.AsNoTracking().Where(row => row.RoleId == roleId && row.ClaimType == WorkspaceClaimType)
            .OrderBy(row => row.Id).Select(row => new { row.Id, row.ClaimValue }).ToArrayAsync(token);
        if (mappings.Length > 512 || mappings.GroupBy(row => row.ClaimValue).Any(group => group.Count() > 1))
            throw new InvalidOperationException("Workspace mappings require reconciliation.");
        var catalog = await db.Set<AppPersona>().AsNoTracking().OrderBy(row => row.DisplayOrder).ThenBy(row => row.Code).ToArrayAsync(token);
        if (catalog.Length > 128 || mappings.Any(mapping => !catalog.Any(row => row.Code == mapping.ClaimValue)))
            throw new InvalidOperationException("Workspace mappings require reconciliation.");
        var revision = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new { RoleId = role.Id, RoleName = role.Name, Mappings = mappings, Catalog = catalog }))));
        return new(role.Id, role.Name ?? "", Array.AsReadOnly(catalog.Select(row => new RoleWorkspaceChoice(row,
            mappings.Any(mapping => mapping.ClaimValue == row.Code))).ToArray()), revision);
    }
}
