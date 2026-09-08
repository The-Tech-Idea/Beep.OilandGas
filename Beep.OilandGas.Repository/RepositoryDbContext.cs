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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        IdentityExtensionMapping.Configure(builder);
        PersonaMapping.Configure(builder);
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
        builder.Entity<ModuleDatabaseBinding>(entity =>
        {
            entity.ToTable("ModuleDatabaseBindings");
            entity.HasKey(x => x.ModuleId);
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
