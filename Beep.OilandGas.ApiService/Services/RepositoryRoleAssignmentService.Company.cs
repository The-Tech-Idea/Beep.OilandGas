using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class RepositoryRoleAssignmentService
{
    /// <summary>Creates a company access boundary under a new or existing tenant and its initial company administrator.
    /// The PPDM business associate identifier is a reference, not evidence that the source exists.</summary>
    public async Task<CompanyScope> CreateInitialCompanyAsync(string tenantId, string tenantName, string businessAssociateId,
        string reason, string intentId, string actor, CancellationToken cancellationToken = default)
    {
        try { new InitialCompanyProvisionRequest(tenantId, tenantName, businessAssociateId, reason, intentId).Require(); }
        catch (ArgumentException) { throw RefusalException.Invalid("Enter a valid tenant ID, tenant name, company business associate ID and reason."); }
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var user = await users.FindByIdAsync(actor) ?? throw RefusalException.Forbidden("An active repository administrator is required.");
        var role = await roles.FindByNameAsync(RepositoryBootstrapService.AdministratorRole)
            ?? throw RefusalException.Conflict("The administrator role is unavailable.");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        // Serialize against existing role grant and revocation operations before rechecking authority.
        Require(await roles.UpdateAsync(role));
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var now = time.GetUtcNow().UtcDateTime;
        if (!await db.Set<AppUserRoleExtension>().AsNoTracking().AnyAsync(x => x.UserId == actor && x.RoleId == role.Id &&
            x.TenantId == null && x.BusinessAssociateId == null && x.ApprovalStatus == "Approved" &&
            x.EffectiveFromUtc <= now && (x.EffectiveToUtc == null || x.EffectiveToUtc > now), cancellationToken))
            throw RefusalException.Conflict("The repository administrator grant needs an explicit repository assignment before company setup.");
        var scope = new CompanyScope(tenantId, businessAssociateId);
        var original = await db.Set<AppUserRoleExtension>().AsNoTracking().SingleOrDefaultAsync(x => x.UserRoleId == intentId, cancellationToken);
        if (original is not null)
        {
            await RequireOriginalCompanyRequestAsync(new(tenantId, tenantName, businessAssociateId, reason, intentId), actor, role.Id, original, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return scope;
        }
        var existingTenant = await db.Set<AppTenant>().AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (existingTenant is not null && (!existingTenant.IsActive || !string.Equals(existingTenant.Name, tenantName, StringComparison.Ordinal)))
            throw RefusalException.Conflict("The tenant is inactive or its name does not match. Review existing setup before continuing.");
        if (await db.Set<AppCompany>().AnyAsync(x => x.TenantId == tenantId && x.BusinessAssociateId == businessAssociateId, cancellationToken))
            throw RefusalException.Conflict("This company already exists in the selected tenant. Review existing setup before continuing.");
        if (existingTenant is null) db.Add(new AppTenant { TenantId = tenantId, Name = tenantName });
        db.Add(new AppCompany { TenantId = tenantId, BusinessAssociateId = businessAssociateId });
        db.Add(new AppCompanyMembership { TenantId = tenantId, BusinessAssociateId = businessAssociateId,
            UserId = actor, GrantedByUserId = actor, EffectiveFromUtc = now, ChangedUtc = now });
        if (!await users.IsInRoleAsync(user, role.Name!))
            throw RefusalException.Forbidden("The repository administrator role is no longer assigned.");
        db.Add(new AppUserRoleExtension { UserRoleId = intentId, TenantId = tenantId, BusinessAssociateId = businessAssociateId,
            UserId = actor, RoleId = role.Id, GrantedByUserId = actor, AssignmentReason = reason,
            EffectiveFromUtc = now, ApprovalStatus = "Approved" });
        await db.SaveChangesAsync(cancellationToken);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Initial company boundary created: Actor={Actor} Tenant={Tenant} Company={Company}", actor, tenantId, businessAssociateId);
        return scope;
    }

    private async Task RequireOriginalCompanyRequestAsync(InitialCompanyProvisionRequest request, string actor, string roleId,
        AppUserRoleExtension original, CancellationToken cancellationToken)
    {
        var tenant = await db.Set<AppTenant>().AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == request.TenantId, cancellationToken);
        var company = await db.Set<AppCompany>().AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == request.TenantId && x.BusinessAssociateId == request.BusinessAssociateId, cancellationToken);
        var membership = await db.Set<AppCompanyMembership>().AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == request.TenantId && x.BusinessAssociateId == request.BusinessAssociateId && x.UserId == actor, cancellationToken);
        if (original.TenantId != request.TenantId || original.BusinessAssociateId != request.BusinessAssociateId || original.UserId != actor ||
            original.GrantedByUserId != actor || original.RoleId != roleId || original.AssignmentReason != request.Reason ||
            tenant?.Name != request.TenantName || company is null || membership?.GrantedByUserId != actor || membership.EffectiveFromUtc != original.EffectiveFromUtc)
            throw RefusalException.Conflict("The provisioning reference does not match the original request. Review existing setup.");
    }

    /// <summary>Read-only observation of the original request. Absence does not rule out an in-flight write.</summary>
    public async Task<InitialCompanyProvisionReview> ReviewInitialCompanyAsync(InitialCompanyProvisionRequest request, string actor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        try { request.Require(); } catch (ArgumentException) { throw RefusalException.Invalid("The original company request is invalid."); }
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var role = await roles.FindByNameAsync(RepositoryBootstrapService.AdministratorRole)
            ?? throw RefusalException.Conflict("The administrator role is unavailable.");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var original = await db.Set<AppUserRoleExtension>().AsNoTracking().SingleOrDefaultAsync(x => x.UserRoleId == request.IntentId, cancellationToken);
        if (original is not null) await RequireOriginalCompanyRequestAsync(request, actor, role.Id, original, cancellationToken);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(request.IntentId, original is null ? "NotRecorded" : "Recorded", original is null ? null : new(request.TenantId, request.BusinessAssociateId));
    }

    /// <summary>Creates one company membership and its first scoped role in the existing Identity grant owner.</summary>
    public async Task<RepositoryUserRole> CreateCompanyMembershipAsync(CompanyScope scope, string targetUserId,
        string roleId, string reason, string intentId, string actor, CancellationToken cancellationToken = default)
    {
        ValidateCompanyScope(scope);
        try { new CompanyMembershipRequest(targetUserId, roleId, reason, intentId).Require(); }
        catch (ArgumentException) { throw RefusalException.Invalid("Select a user, a role and a reason for the company membership."); }
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var user = await users.FindByIdAsync(targetUserId) ?? throw RefusalException.NotFound("User not found.");
        var role = await roles.FindByIdAsync(roleId) ?? throw RefusalException.NotFound("Role not found.");
        if (string.IsNullOrWhiteSpace(role.Name)) throw RefusalException.Conflict("The selected role is unavailable.");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        Require(await roles.UpdateAsync(role));
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var original = await db.Set<AppUserRoleExtension>().AsNoTracking().SingleOrDefaultAsync(x => x.UserRoleId == intentId, cancellationToken);
        if (original is not null)
        {
            var saved = await db.Set<AppCompanyMembership>().AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == scope.TenantId && x.BusinessAssociateId == scope.BusinessAssociateId && x.UserId == targetUserId, cancellationToken);
            if (original.TenantId != scope.TenantId || original.BusinessAssociateId != scope.BusinessAssociateId || original.UserId != targetUserId ||
                original.RoleId != roleId || original.GrantedByUserId != actor || original.AssignmentReason != reason ||
                saved?.GrantedByUserId != actor || saved.EffectiveFromUtc != original.EffectiveFromUtc)
                throw RefusalException.Conflict("The membership reference does not match the original request.");
            await transaction.CommitAsync(cancellationToken); return Assignment(original);
        }
        if (!await db.Users.AsNoTracking().AnyAsync(x => x.Id == targetUserId && x.IsActive, cancellationToken))
            throw RefusalException.Conflict("The selected account is inactive or unavailable.");
        if (!await db.Set<AppTenant>().AsNoTracking().AnyAsync(x => x.TenantId == scope.TenantId && x.IsActive, cancellationToken) ||
            !await db.Set<AppCompany>().AsNoTracking().AnyAsync(x => x.TenantId == scope.TenantId && x.BusinessAssociateId == scope.BusinessAssociateId && x.IsActive, cancellationToken))
            throw RefusalException.Conflict("The selected tenant or company is inactive or unavailable.");
        if (await db.Set<AppCompanyMembership>().AnyAsync(x => x.TenantId == scope.TenantId && x.BusinessAssociateId == scope.BusinessAssociateId && x.UserId == targetUserId, cancellationToken))
            throw RefusalException.Conflict("This account already has a company membership. Review it before assigning roles.");
        var assignments = db.Set<AppUserRoleExtension>();
        if (await users.IsInRoleAsync(user, role.Name) && !await assignments.AnyAsync(x => x.UserId == targetUserId && x.RoleId == roleId, cancellationToken))
            throw RefusalException.Conflict("Classify the existing Identity membership as a repository grant before assigning company scope.");
        if (!await users.IsInRoleAsync(user, role.Name)) Require(await users.AddToRoleAsync(user, role.Name));
        var now = time.GetUtcNow().UtcDateTime;
        db.Add(new AppCompanyMembership { TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId, UserId = targetUserId, GrantedByUserId = actor, EffectiveFromUtc = now, ChangedUtc = now });
        var grant = new AppUserRoleExtension { UserRoleId = intentId, TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId, UserId = targetUserId, RoleId = roleId, GrantedByUserId = actor, AssignmentReason = reason, EffectiveFromUtc = now, ApprovalStatus = "Approved" };
        db.Add(grant); await db.SaveChangesAsync(cancellationToken);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        if (!await db.Users.AsNoTracking().AnyAsync(x => x.Id == targetUserId && x.IsActive, cancellationToken)) throw RefusalException.Conflict("The selected account changed during setup.");
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Company membership created: Actor={Actor} Tenant={Tenant} Company={Company} User={User} Role={Role}", actor, scope.TenantId, scope.BusinessAssociateId, targetUserId, roleId);
        return Assignment(grant);
    }

    /// <summary>Repository administrators manage explicit grants; this never grants repository authority.</summary>
    public async Task<RepositoryUserRole> AssignCompanyRoleAsync(CompanyScope scope, string userId,
        AssignCompanyRoleRequest request, string actor, CancellationToken cancellationToken = default)
    {
        ValidateCompanyScope(scope);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.RoleId) || string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 1000)
            throw RefusalException.Invalid("A role and an assignment reason of at most 1000 characters are required.");
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var user = await users.FindByIdAsync(userId) ?? throw RefusalException.NotFound("User not found.");
        var role = await roles.FindByIdAsync(request.RoleId) ?? throw RefusalException.NotFound("Role not found.");
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // All grant mutations serialize through the same Identity role concurrency token.
        Require(await roles.UpdateAsync(role));
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var now = time.GetUtcNow().UtcDateTime;
        var access = new RepositoryAccessService(db, time);
        if (await access.GetCompanyAccessAsync(userId, scope, cancellationToken) is null)
            throw RefusalException.Forbidden("The account has no active membership in this company.");
        var membership = await db.Set<AppCompanyMembership>().SingleAsync(x => x.TenantId == scope.TenantId &&
            x.BusinessAssociateId == scope.BusinessAssociateId && x.UserId == userId, cancellationToken);
        var expires = request.EffectiveToUtc?.UtcDateTime ?? membership.EffectiveToUtc;
        if (expires <= now || (membership.EffectiveToUtc is { } limit && (expires is null || expires > limit)))
            throw RefusalException.Invalid("The role expiry must be in the future and within the company membership interval.");
        var assignments = db.Set<AppUserRoleExtension>();
        if (await assignments.AnyAsync(x => x.UserId == userId && x.RoleId == role.Id &&
            x.TenantId == scope.TenantId && x.BusinessAssociateId == scope.BusinessAssociateId &&
            x.ApprovalStatus == "Approved" && (x.EffectiveToUtc == null || x.EffectiveToUtc > now), cancellationToken))
            throw RefusalException.Conflict("This company role is already assigned. Revoke it before changing the grant.");
        if (await users.IsInRoleAsync(user, role.Name!) &&
            !await assignments.AnyAsync(x => x.UserId == userId && x.RoleId == role.Id, cancellationToken))
            throw RefusalException.Conflict("Classify the existing Identity membership as a repository grant before assigning company scope.");
        if (!await users.IsInRoleAsync(user, role.Name!)) Require(await users.AddToRoleAsync(user, role.Name!));
        var assignment = new AppUserRoleExtension
        {
            TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId,
            UserId = userId, RoleId = role.Id, GrantedByUserId = actor, AssignmentReason = request.Reason.Trim(),
            EffectiveFromUtc = now, EffectiveToUtc = expires, ApprovalStatus = "Approved"
        };
        db.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        if (await access.GetCompanyAccessAsync(userId, scope, cancellationToken) is null)
            throw RefusalException.Conflict("The company membership changed during the role assignment.");
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Company role assigned: Actor={Actor} Tenant={Tenant} Company={Company} User={User} Role={Role}",
            actor, scope.TenantId, scope.BusinessAssociateId, userId, role.Id);
        return Assignment(assignment);
    }

    public async Task<CompanyMembershipRetirementPage> ReviewMembershipRetirementsAsync(CompanyScope scope, string userId, string actor,
        int offset, int limit, CancellationToken cancellationToken = default)
    {
        ValidateCompanyScope(scope);
        if (string.IsNullOrWhiteSpace(userId) || userId.Length > 128 || userId != userId.Trim() || userId.Any(char.IsControl) || offset < 0 || limit is < 1 or > 100)
            throw RefusalException.Invalid("Select an account and a valid history page.");
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var query = db.Set<CompanyMembershipRetirement>().AsNoTracking().Where(x => x.TenantId == scope.TenantId &&
            x.BusinessAssociateId == scope.BusinessAssociateId && x.UserId == userId);
        var headers = await query.Select(x => new { x.IntentId, x.TenantId, x.BusinessAssociateId, x.UserId, x.RetiredUtc }).ToArrayAsync(cancellationToken);
        if (headers.Any(x => x.TenantId != scope.TenantId || x.BusinessAssociateId != scope.BusinessAssociateId || x.UserId != userId))
            throw RefusalException.Conflict("Membership history has ambiguous ownership.");
        var ids = headers.OrderByDescending(x => x.RetiredUtc).ThenBy(x => x.IntentId, StringComparer.Ordinal).Skip(offset).Take(limit).Select(x => x.IntentId).ToArray();
        var rows = await query.Where(x => ids.Contains(x.IntentId)).ToArrayAsync(cancellationToken);
        if (rows.Length != ids.Length || rows.Any(x => x.TenantId != scope.TenantId || x.BusinessAssociateId != scope.BusinessAssociateId || x.UserId != userId || !ids.Contains(x.IntentId, StringComparer.Ordinal)))
            throw RefusalException.Conflict("Membership history changed. Review it again.");
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        return new(scope, userId, offset, limit, headers.Length, rows.OrderByDescending(x => x.RetiredUtc).ThenBy(x => x.IntentId, StringComparer.Ordinal).ToArray());
    }

    /// <summary>Ends one reviewed membership and retires its scoped roles while preserving grant history.</summary>
    public async Task<CompanyMembershipRetirement> RetireCompanyMembershipAsync(CompanyScope scope, string userId,
        CompanyMembershipRetirementRequest request, string actor, CancellationToken cancellationToken = default)
    {
        ValidateCompanyScope(scope);
        if (string.IsNullOrWhiteSpace(userId) || userId.Length > 128 || userId != userId.Trim() || userId.Any(char.IsControl))
            throw RefusalException.Invalid("Select a valid user.");
        try { ArgumentNullException.ThrowIfNull(request); request.Require(); }
        catch (ArgumentException) { throw RefusalException.Invalid("Provide the reviewed membership revision, retirement reference and reason."); }
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        if (db.ChangeTracker.HasChanges() || db.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("Membership retirement requires a clean repository transaction.");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        CompanyMembershipRetirement? created = null; AppCompanyMembership? membership = null; OilGasUser? subject = null;
        List<AppUserRoleExtension> scopedRoles = []; List<IdentityRole> lockedRoles = [];
        try
        {
            await RequireRepositoryAdministratorAsync(actor, cancellationToken);
            var original = await db.Set<CompanyMembershipRetirement>().AsNoTracking().SingleOrDefaultAsync(x => x.IntentId == request.IntentId, cancellationToken);
            if (original is not null)
            {
                if (original.IntentId != request.IntentId || original.TenantId != scope.TenantId || original.BusinessAssociateId != scope.BusinessAssociateId || original.UserId != userId ||
                    original.ActorUserId != actor || original.PreviousRevision != request.ExpectedRevision || original.Reason != request.Reason)
                    throw RefusalException.Conflict("The retirement reference belongs to another request.");
                await RequireRepositoryAdministratorAsync(actor, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return original;
            }
            membership = await db.Set<AppCompanyMembership>().SingleOrDefaultAsync(x => x.TenantId == scope.TenantId &&
                x.BusinessAssociateId == scope.BusinessAssociateId && x.UserId == userId, cancellationToken);
            if (membership is null) throw RefusalException.NotFound("The company membership was not found.");
            if (membership.TenantId != scope.TenantId || membership.BusinessAssociateId != scope.BusinessAssociateId || membership.UserId != userId)
                throw RefusalException.Conflict("The membership identity is ambiguous. Review it again.");
            if (membership.ConcurrencyStamp != request.ExpectedRevision) throw RefusalException.Conflict("The membership changed. Review it again.");
            var now = time.GetUtcNow().UtcDateTime;
            if (membership.EffectiveToUtc is { } end && (end <= now || end <= membership.EffectiveFromUtc))
                throw RefusalException.Conflict("The membership is already ended. Review its history.");
            subject = await users.FindByIdAsync(userId) ?? throw RefusalException.NotFound("The account was not found.");
            scopedRoles = await db.Set<AppUserRoleExtension>().Where(x => x.TenantId == scope.TenantId &&
                x.BusinessAssociateId == scope.BusinessAssociateId && x.UserId == userId && x.ApprovalStatus == "Approved")
                .ToListAsync(cancellationToken);
            if (scopedRoles.Any(x => x.TenantId != scope.TenantId || x.BusinessAssociateId != scope.BusinessAssociateId || x.UserId != userId))
                throw RefusalException.Conflict("Company role ownership is ambiguous. Review it again.");
            foreach (var roleId in scopedRoles.Select(x => x.RoleId).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal))
            {
                var role = await roles.FindByIdAsync(roleId) ?? throw RefusalException.Conflict("A recorded company role is unavailable.");
                lockedRoles.Add(role);
                Require(await roles.UpdateAsync(role));
            }
            await RequireRepositoryAdministratorAsync(actor, cancellationToken);
            foreach (var grant in scopedRoles)
            {
                var grantEnd = grant.EffectiveFromUtc > now ? grant.EffectiveFromUtc : now;
                if (grant.EffectiveToUtc is null || grant.EffectiveToUtc > grantEnd) grant.EffectiveToUtc = grantEnd;
                grant.ApprovalStatus = "Revoked";
                grant.RevokedByUserId = actor;
            }
            var revisedEnd = membership.EffectiveFromUtc > now ? membership.EffectiveFromUtc : now;
            var revision = Guid.NewGuid().ToString();
            created = new CompanyMembershipRetirement { IntentId = request.IntentId, TenantId = scope.TenantId,
                BusinessAssociateId = scope.BusinessAssociateId, UserId = userId, ActorUserId = actor, Reason = request.Reason,
                PreviousRevision = membership.ConcurrencyStamp, ResultRevision = revision, EffectiveFromUtc = membership.EffectiveFromUtc,
                PreviousEffectiveToUtc = membership.EffectiveToUtc, EffectiveToUtc = revisedEnd, RetiredUtc = now };
            membership.EffectiveToUtc = revisedEnd; membership.ChangedUtc = now; membership.ConcurrencyStamp = revision;
            db.Add(created); await db.SaveChangesAsync(cancellationToken);
            foreach (var role in lockedRoles) await RemoveUnusedIdentityMembershipAsync(subject, role, now, cancellationToken);
            await RequireRepositoryAdministratorAsync(actor, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Company membership retired: Actor={Actor} Tenant={Tenant} Company={Company} User={User} Intent={Intent}", actor, scope.TenantId, scope.BusinessAssociateId, userId, request.IntentId);
            return created;
        }
        finally
        {
            if (membership is not null) db.Entry(membership).State = EntityState.Detached;
            if (created is not null) db.Entry(created).State = EntityState.Detached;
            foreach (var grant in scopedRoles) db.Entry(grant).State = EntityState.Detached;
            foreach (var role in lockedRoles) db.Entry(role).State = EntityState.Detached;
            if (subject is not null) db.Entry(subject).State = EntityState.Detached;
        }
    }

    public async Task<bool> RevokeCompanyRoleAsync(CompanyScope scope, string assignmentId, string actor,
        CancellationToken cancellationToken = default)
    {
        ValidateCompanyScope(scope);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var assignment = await db.Set<AppUserRoleExtension>().SingleOrDefaultAsync(x => x.UserRoleId == assignmentId &&
            x.TenantId == scope.TenantId && x.BusinessAssociateId == scope.BusinessAssociateId, cancellationToken);
        if (assignment is null || assignment.ApprovalStatus == "Revoked") return false;
        var user = await users.FindByIdAsync(assignment.UserId) ?? throw RefusalException.NotFound("User not found.");
        var role = await roles.FindByIdAsync(assignment.RoleId) ?? throw RefusalException.NotFound("Role not found.");
        Require(await roles.UpdateAsync(role));
        await db.Entry(assignment).ReloadAsync(cancellationToken);
        if (db.Entry(assignment).State == EntityState.Detached || assignment.TenantId != scope.TenantId ||
            assignment.BusinessAssociateId != scope.BusinessAssociateId || assignment.UserId != user.Id ||
            assignment.RoleId != role.Id || assignment.ApprovalStatus == "Revoked")
            throw RefusalException.Conflict("The company role assignment changed. Review it again.");
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var now = time.GetUtcNow().UtcDateTime;
        // Revoking an already expired grant records the actor without moving its expiry forward.
        if (assignment.EffectiveToUtc is null || assignment.EffectiveToUtc > now) assignment.EffectiveToUtc = now;
        assignment.ApprovalStatus = "Revoked";
        assignment.RevokedByUserId = actor;
        await db.SaveChangesAsync(cancellationToken);
        await RemoveUnusedIdentityMembershipAsync(user, role, now, cancellationToken);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Company role revoked: Actor={Actor} Tenant={Tenant} Company={Company} User={User} Role={Role}",
            actor, scope.TenantId, scope.BusinessAssociateId, user.Id, role.Id);
        return true;
    }

    public async Task<RepositoryUserRole[]> GetCompanyRoleAssignmentsAsync(CompanyScope scope, string userId, string actor,
        CancellationToken cancellationToken = default)
    {
        ValidateCompanyScope(scope);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var records = await db.Set<AppUserRoleExtension>().AsNoTracking().Where(x => x.UserId == userId &&
            x.TenantId == scope.TenantId && x.BusinessAssociateId == scope.BusinessAssociateId)
            .OrderByDescending(x => x.EffectiveFromUtc).ThenBy(x => x.UserRoleId).ToArrayAsync(cancellationToken);
        return records.Select(Assignment).ToArray();
    }

    public async Task<CompanyMemberAccessReview> ReviewCompanyMemberAccessAsync(CompanyScope scope, string userId, string actor,
        CancellationToken cancellationToken = default)
    {
        ValidateCompanyScope(scope);
        if (string.IsNullOrWhiteSpace(userId) || userId.Length > 128 || userId != userId.Trim() || userId.Any(char.IsControl))
            throw RefusalException.Invalid("Select a valid user.");
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        var access = new RepositoryAccessService(db, time);
        async Task<CompanyMembershipObservation?> Membership()
        {
            var row = await db.Set<AppCompanyMembership>().AsNoTracking().SingleOrDefaultAsync(x =>
                x.TenantId == scope.TenantId && x.BusinessAssociateId == scope.BusinessAssociateId && x.UserId == userId, cancellationToken);
            if (row is null) return null;
            if (row.TenantId != scope.TenantId || row.BusinessAssociateId != scope.BusinessAssociateId || row.UserId != userId)
                throw RefusalException.Conflict("The membership identity is ambiguous. Review it again.");
            return new(row.EffectiveFromUtc, row.EffectiveToUtc, row.GrantedByUserId, row.ChangedUtc, row.ConcurrencyStamp);
        }
        var firstMembership = await Membership();
        var first = await access.GetCompanyAccessAsync(userId, scope, cancellationToken);
        var secondMembership = await Membership();
        var second = await access.GetCompanyAccessAsync(userId, scope, cancellationToken);
        await RequireRepositoryAdministratorAsync(actor, cancellationToken);
        if (firstMembership != secondMembership || second is not null && secondMembership is null || (first is null) != (second is null) || first is not null && second is not null &&
            (first.Scope != scope || second.Scope != scope || !first.Roles.SequenceEqual(second.Roles) || !first.Permissions.SequenceEqual(second.Permissions)))
            throw RefusalException.Conflict("Company access changed during review. Refresh and try again.");
        return new(scope, userId, second) { Membership = secondMembership };
    }

    public async Task RequireRepositoryAdministratorAsync(string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor)) throw RefusalException.Forbidden("A repository administrator is required.");
        var access = await new RepositoryAccessService(db, time).GetAccessAsync(actor, cancellationToken);
        if (access is not { IsActive: true } || !access.Roles.Any(role => string.Equals(roles.NormalizeKey(role),
            roles.NormalizeKey(RepositoryBootstrapService.AdministratorRole), StringComparison.Ordinal)))
            throw RefusalException.Forbidden("A repository administrator is required.");
    }

    private async Task RemoveUnusedIdentityMembershipAsync(OilGasUser user, IdentityRole role, DateTime now,
        CancellationToken cancellationToken = default)
    {
        // Future approved grants also need the standard Identity membership when their interval begins.
        if (!await db.Set<AppUserRoleExtension>().AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id &&
            x.ApprovalStatus == "Approved" && (x.EffectiveToUtc == null || x.EffectiveToUtc > now), cancellationToken) &&
            await users.IsInRoleAsync(user, role.Name!)) Require(await users.RemoveFromRoleAsync(user, role.Name!));
    }

    private static void ValidateCompanyScope(CompanyScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (string.IsNullOrWhiteSpace(scope.TenantId) || scope.TenantId.Length > 64 ||
            string.IsNullOrWhiteSpace(scope.BusinessAssociateId) || scope.BusinessAssociateId.Length > 128)
            throw RefusalException.Invalid("Select a valid tenant and company.");
    }
}
