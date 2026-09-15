using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

internal static class AssetAccessMapping
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<AppUserAssetAccess>(entity =>
        {
            entity.ToTable("APP_USER_ASSET_ACCESS");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DatabaseScope).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AssetType).HasMaxLength(16).IsRequired();
            entity.Property(x => x.AssetId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.OrganizationId).HasMaxLength(128);
            entity.Property(x => x.AccessLevel).HasMaxLength(8).IsRequired();
            entity.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ChangedBy).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ConcurrencyStamp).HasMaxLength(36).IsConcurrencyToken();
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.DatabaseScope });
        });
    }
}
