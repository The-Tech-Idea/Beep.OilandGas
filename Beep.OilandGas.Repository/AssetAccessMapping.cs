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
            entity.Property(x => x.FacilityType).HasMaxLength(128);
            entity.Property(x => x.OrganizationId).HasMaxLength(128);
            entity.Property(x => x.AccessLevel).HasMaxLength(8).IsRequired();
            entity.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ChangedBy).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ConcurrencyStamp).HasMaxLength(36).IsConcurrencyToken();
            entity.Property(x => x.CreatedUtc).HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
            entity.Property(x => x.ChangedUtc).HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.DatabaseScope });
        });
        builder.Entity<AssetGrantRetirement>(entity =>
        {
            entity.ToTable("APP_ASSET_GRANT_RETIREMENT");
            entity.HasKey(row => row.Revision);
            entity.Property(row => row.Revision).HasMaxLength(36).ValueGeneratedNever();
            entity.Property(row => row.GrantId).HasMaxLength(64);
            entity.Property(row => row.TenantId).HasMaxLength(64);
            entity.Property(row => row.OrganizationId).HasMaxLength(128);
            entity.Property(row => row.DatabaseScope).HasMaxLength(64);
            entity.Property(row => row.PreviousRevision).HasMaxLength(36);
            entity.Property(row => row.SnapshotJson).HasMaxLength(16000).IsRequired();
            entity.Property(row => row.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(row => row.ActorUserId).HasMaxLength(128);
            entity.Property(row => row.ChangedUtc).HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
            entity.HasOne<AppUserAssetAccess>().WithMany().HasForeignKey(row => row.GrantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AppCompany>().WithMany().HasForeignKey(row => new { row.TenantId, row.OrganizationId })
                .HasPrincipalKey(row => new { row.TenantId, row.BusinessAssociateId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(row => row.ActorUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(row => new { row.GrantId, row.ChangedUtc });
        });
    }
}
