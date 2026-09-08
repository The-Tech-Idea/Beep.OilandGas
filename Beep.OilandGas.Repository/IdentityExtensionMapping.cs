using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

internal static class IdentityExtensionMapping
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<AppUserExtension>(entity =>
        {
            entity.ToTable("APP_USER");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).HasMaxLength(128);
            entity.HasOne<OilGasUser>().WithOne().HasForeignKey<AppUserExtension>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<AppRoleExtension>(entity =>
        {
            entity.ToTable("APP_ROLE");
            entity.HasKey(x => x.RoleId);
            entity.Property(x => x.RoleId).HasMaxLength(128);
            entity.HasOne<IdentityRole>().WithOne().HasForeignKey<AppRoleExtension>(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<AppPermissionExtension>(entity =>
        {
            entity.ToTable("APP_PERMISSION");
            entity.HasKey(x => x.PermissionId);
            entity.Property(x => x.PermissionId).HasMaxLength(128);
            entity.Property(x => x.PermissionKey).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.PermissionKey).IsUnique();
        });
        builder.Entity<AppUserRoleExtension>(entity =>
        {
            entity.ToTable("APP_USER_ROLE");
            entity.HasKey(x => x.UserRoleId);
            entity.Property(x => x.UserRoleId).HasMaxLength(128);
            entity.Property(x => x.UserId).HasMaxLength(128);
            entity.Property(x => x.RoleId).HasMaxLength(128);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<IdentityRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.RoleId });
        });
        builder.Entity<AppRolePermissionExtension>(entity =>
        {
            entity.ToTable("APP_ROLE_PERMISSION");
            entity.HasKey(x => x.RolePermissionId);
            entity.Property(x => x.RolePermissionId).HasMaxLength(128);
            entity.Property(x => x.RoleId).HasMaxLength(128);
            entity.Property(x => x.PermissionId).HasMaxLength(128);
            entity.HasOne<IdentityRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AppPermissionExtension>().WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<IdentityRoleClaim<string>>().WithMany().HasForeignKey(x => x.RoleClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.RoleId, x.PermissionId });
        });

        // Keep the legacy APP_* column convention without copying its authentication tables.
        foreach (var type in new[] { typeof(AppUserExtension), typeof(AppRoleExtension), typeof(AppPermissionExtension),
            typeof(AppUserRoleExtension), typeof(AppRolePermissionExtension) })
        {
            foreach (var property in builder.Entity(type).Metadata.GetProperties())
            {
                var column = System.Text.RegularExpressions.Regex.Replace(property.Name, "([a-z0-9])([A-Z])", "$1_$2").ToUpperInvariant();
                property.SetColumnName(column);
                if (property.ClrType == typeof(string) && property.GetMaxLength() is null) property.SetMaxLength(1000);
            }
        }
    }
}
