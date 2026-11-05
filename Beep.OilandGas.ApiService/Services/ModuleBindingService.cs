using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>The existing binding registry's tenant ownership and concurrency boundary.</summary>
public sealed class ModuleBindingService(RepositoryDbContext db, IDMEEditor editor, TimeProvider? clock = null)
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;

    public async Task RequireActiveTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || tenantId.Length > 64)
            throw RefusalException.Invalid("Select a valid tenant.");
        if (!await db.Set<AppTenant>().AsNoTracking().AnyAsync(x => x.TenantId == tenantId && x.IsActive, cancellationToken))
            throw RefusalException.Forbidden("The selected tenant is unavailable.");
    }

    public async Task<ModuleDatabaseBinding[]> ListAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await RequireActiveTenantAsync(tenantId, cancellationToken);
        return await db.ModuleDatabases.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.ModuleId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<ModuleDatabaseBinding[]> UnownedAsync(CancellationToken cancellationToken = default) =>
        db.ModuleDatabases.AsNoTracking().Where(x => x.TenantId == null).OrderBy(x => x.BindingId).ToArrayAsync(cancellationToken);

    public async Task<ModuleDatabaseBinding> BindAsync(string tenantId, string moduleId, ModuleBindingRequest request,
        string actor, CancellationToken cancellationToken = default)
    {
        await RequireActiveTenantAsync(tenantId, cancellationToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        ArgumentNullException.ThrowIfNull(request);
        var id = NormalizeModule(moduleId);
        if (string.IsNullOrWhiteSpace(request.ConnectionName) || request.ConnectionName.Length > 128)
            throw RefusalException.Invalid("Select a valid BeepDM connection.");
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var binding = await db.ModuleDatabases.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.ModuleId == id, cancellationToken);
        if (binding is null && request.ConcurrencyStamp is not null)
            throw RefusalException.Conflict("The module binding changed. Reload before saving.");
        if (binding is not null) RequireVersion(binding, request.ConcurrencyStamp);
        var connectionKey = await RequireConnectionOwnerAsync(tenantId, request.ConnectionName, actor, cancellationToken);
        if (binding is null)
        {
            binding = new ModuleDatabaseBinding { TenantId = tenantId, ModuleId = id, ConnectionName = request.ConnectionName,
                ConnectionKey = connectionKey,
                OwnershipAssignedByUserId = actor, OwnershipAssignedUtc = time.GetUtcNow().UtcDateTime,
                OwnershipEvidence = "Explicit tenant-scoped setup binding." };
            db.ModuleDatabases.Add(binding);
        }
        else
        {
            binding.ConnectionName = request.ConnectionName;
            binding.ConnectionKey = connectionKey;
            binding.ConcurrencyStamp = Guid.NewGuid().ToString();
        }
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return binding;
    }

    public async Task<ModuleDatabaseBinding> AssignOwnershipAsync(long bindingId, ModuleOwnershipRequest request,
        string actor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        if (string.IsNullOrWhiteSpace(request.Evidence) || request.Evidence.Length > 1000)
            throw RefusalException.Invalid("Record ownership evidence of at most 1000 characters.");
        await RequireActiveTenantAsync(request.TenantId, cancellationToken);
        var binding = await db.ModuleDatabases.SingleOrDefaultAsync(x => x.BindingId == bindingId, cancellationToken)
            ?? throw RefusalException.NotFound("The unowned module binding was not found.");
        RequireVersion(binding, request.ConcurrencyStamp);
        if (binding.TenantId is not null) throw RefusalException.Conflict("This binding already has an owner; tenant transfer requires a separate reviewed migration.");
        NormalizeModule(binding.ModuleId);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (await db.ModuleDatabases.AnyAsync(x => x.TenantId == request.TenantId && x.ModuleId == binding.ModuleId, cancellationToken))
            throw RefusalException.Conflict("This tenant already has a binding for the module. Reconcile the records before assigning ownership.");
        binding.ConnectionKey = await RequireConnectionOwnerAsync(request.TenantId, binding.ConnectionName, actor, cancellationToken);
        binding.TenantId = request.TenantId;
        binding.OwnershipAssignedByUserId = actor;
        binding.OwnershipAssignedUtc = time.GetUtcNow().UtcDateTime;
        binding.OwnershipEvidence = request.Evidence.Trim();
        binding.ConcurrencyStamp = Guid.NewGuid().ToString();
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return binding;
    }

    private async Task<string> RequireConnectionOwnerAsync(string tenantId, string connection, string actor, CancellationToken token)
    {
        var name = connection.ToUpperInvariant();
        var target = ModuleDatabaseTarget.Key(editor, connection);
        var owner = await db.Set<AppModuleConnection>().SingleOrDefaultAsync(x => x.ConnectionKey == name, token);
        if (owner is not null && owner.TenantId != tenantId)
            throw RefusalException.Conflict("A BeepDM connection cannot be shared by independent tenants.");
        if (owner?.PhysicalTargetKey is not null && owner.PhysicalTargetKey != target)
            throw RefusalException.Conflict("The owned connection's database target changed. Reconcile the deployment before rebinding.");
        if (await db.Set<AppModuleConnection>().AnyAsync(x => x.PhysicalTargetKey == target && x.ConnectionKey != name, token))
            throw RefusalException.Conflict("This database target already has a canonical owned connection. Use that connection for its tenant's modules.");
        if (owner is null)
        {
            owner = new AppModuleConnection { TenantId = tenantId, ConnectionKey = name,
                AssignedByUserId = actor, AssignedUtc = time.GetUtcNow().UtcDateTime };
            db.Add(owner);
        }
        if (owner.PhysicalTargetKey is null)
        {
            owner.PhysicalTargetKey = target;
            owner.TargetVerifiedByUserId = actor;
            owner.TargetVerifiedUtc = time.GetUtcNow().UtcDateTime;
        }
        return name;
    }

    private static string NormalizeModule(string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId) || moduleId.Length > 64)
            throw RefusalException.Invalid("Select a valid module.");
        var id = moduleId.ToUpperInvariant();
        if (id == "SECURITY") throw RefusalException.Invalid("Security belongs to the default repository.");
        return id;
    }

    private static void RequireVersion(ModuleDatabaseBinding binding, string? stamp)
    {
        if (string.IsNullOrWhiteSpace(stamp) || !string.Equals(binding.ConcurrencyStamp, stamp, StringComparison.Ordinal))
            throw RefusalException.Conflict("The module binding changed. Reload before saving.");
    }
}
