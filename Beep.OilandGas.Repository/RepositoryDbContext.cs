using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

public abstract class RepositoryDbContext(DbContextOptions options)
    : IdentityDbContext<OilGasUser>(options)
{
    public DbSet<RepositoryBootstrap> Bootstrap => Set<RepositoryBootstrap>();
    public DbSet<ModuleDatabaseBinding> ModuleDatabases => Set<ModuleDatabaseBinding>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RequireAppendOnlyEvidence(); return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        RequireAppendOnlyEvidence(); return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    private void RequireAppendOnlyEvidence()
    {
        if (ChangeTracker.Entries<AssetGrantRetirement>().Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Grant retirement evidence is append-only and cannot be changed or deleted.");
        if (ChangeTracker.Entries<CompanyMembershipRetirement>().Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Membership retirement evidence is append-only and cannot be changed or deleted.");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        IdentityExtensionMapping.Configure(builder);
        CompanyAccessMapping.Configure(builder);
        HierarchyConfigurationMapping.Configure(builder);
        AssetAccessMapping.Configure(builder);
        PersonaMapping.Configure(builder);
        FinancialOperationClaimMapping.Configure(builder);
        builder.HasAnnotation("Relational:MaxIdentifierLength", 30);
        builder.Entity<OilGasUser>().Property(x => x.Id).HasMaxLength(128);
        builder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.ProviderKey).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(x => x.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(x => x.Name).HasMaxLength(128);
        builder.Entity<RepositoryBootstrap>(entity =>
        {
            entity.ToTable("RepositoryBootstrap");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.AdministratorUserId).HasMaxLength(128).IsRequired();
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.AdministratorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<AppModuleConnection>(entity =>
        {
            entity.ToTable("APP_MODULE_CONNECTION");
            entity.HasKey(x => x.ConnectionKey);
            entity.Property(x => x.ConnectionKey).HasMaxLength(128);
            entity.Property(x => x.TenantId).HasMaxLength(64);
            entity.Property(x => x.AssignedByUserId).HasMaxLength(128);
            entity.Property(x => x.PhysicalTargetKey).HasMaxLength(64);
            entity.Property(x => x.TargetVerifiedByUserId).HasMaxLength(128);
            entity.HasIndex(x => x.PhysicalTargetKey).IsUnique().HasDatabaseName("IX_APP_MOD_CONN_TARGET");
            entity.HasAlternateKey(x => new { x.TenantId, x.ConnectionKey });
            entity.HasOne<AppTenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ModuleDatabaseBinding>(entity =>
        {
            entity.ToTable("ModuleDatabaseBindings");
            entity.HasKey(x => x.BindingId);
            entity.Property(x => x.BindingId).ValueGeneratedOnAdd();
            entity.Property(x => x.TenantId).HasMaxLength(64);
            entity.HasOne<AppTenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TenantId, x.ModuleId }).IsUnique();
            entity.Property(x => x.OwnershipAssignedByUserId).HasMaxLength(128);
            entity.Property(x => x.OwnershipEvidence).HasMaxLength(1000);
            entity.Property(x => x.ConnectionKey).HasMaxLength(128);
            entity.HasOne<AppModuleConnection>().WithMany()
                .HasForeignKey(x => new { x.TenantId, x.ConnectionKey })
                .HasPrincipalKey(x => new { x.TenantId, x.ConnectionKey }).OnDelete(DeleteBehavior.Restrict);
            entity.Property(x => x.ModuleId).HasMaxLength(64);
            entity.Property(x => x.ConnectionName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ConcurrencyStamp).HasMaxLength(36).IsConcurrencyToken();
        });
    }
}

public sealed class SqlServerRepositoryDbContext(DbContextOptions<SqlServerRepositoryDbContext> options)
    : RepositoryDbContext(options);

public sealed class PostgreSqlRepositoryDbContext(DbContextOptions<PostgreSqlRepositoryDbContext> options)
    : RepositoryDbContext(options);

public sealed class OracleRepositoryDbContext(DbContextOptions<OracleRepositoryDbContext> options)
    : RepositoryDbContext(options);
