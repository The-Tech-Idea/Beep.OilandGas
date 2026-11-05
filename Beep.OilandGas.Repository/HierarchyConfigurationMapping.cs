using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.Repository;

internal static class HierarchyConfigurationMapping
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<ORGANIZATION_HIERARCHY_CONFIG>(entity =>
        {
            entity.ToTable("ORGANIZATION_HIERARCHY_CONFIG");
            entity.HasKey(row => row.CONFIG_ID);
            entity.Property(row => row.CONFIG_ID).HasMaxLength(64).ValueGeneratedNever();
            entity.Property(row => row.TENANT_ID).HasMaxLength(64);
            entity.Property(row => row.ORGANIZATION_ID).HasMaxLength(128);
            entity.Property(row => row.LEVELS_JSON).HasMaxLength(16000).IsRequired();
            entity.Property(row => row.REVISION).HasMaxLength(36).IsConcurrencyToken();
            entity.Property(row => row.CREATED_BY).HasMaxLength(128);
            entity.Property(row => row.CHANGED_BY).HasMaxLength(128);
            entity.HasIndex(row => new { row.TENANT_ID, row.ORGANIZATION_ID }).IsUnique().HasDatabaseName("IX_HIERARCHY_COMPANY");
            entity.HasOne<AppCompany>().WithMany().HasForeignKey(row => new { row.TENANT_ID, row.ORGANIZATION_ID })
                .HasPrincipalKey(row => new { row.TenantId, row.BusinessAssociateId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(row => row.CREATED_BY).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(row => row.CHANGED_BY).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<OrganizationHierarchyConfigRevision>(entity =>
        {
            entity.ToTable("APP_HIERARCHY_REVISION");
            entity.HasKey(row => row.Revision);
            entity.Property(row => row.Revision).HasMaxLength(36).ValueGeneratedNever();
            entity.Property(row => row.ConfigId).HasMaxLength(64);
            entity.Property(row => row.PreviousRevision).HasMaxLength(36);
            entity.Property(row => row.LevelsJson).HasMaxLength(16000).IsRequired();
            entity.Property(row => row.ActorUserId).HasMaxLength(128);
            entity.HasOne<ORGANIZATION_HIERARCHY_CONFIG>().WithMany().HasForeignKey(row => row.ConfigId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(row => row.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
