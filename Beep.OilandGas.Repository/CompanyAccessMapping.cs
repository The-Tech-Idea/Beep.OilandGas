using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.Repository;

internal static class CompanyAccessMapping
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<AppTenant>(entity =>
        {
            entity.ToTable("APP_TENANT");
            entity.HasKey(x => x.TenantId);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        });
        builder.Entity<AppCompany>(entity =>
        {
            entity.ToTable("APP_COMPANY");
            entity.HasKey(x => new { x.TenantId, x.BusinessAssociateId });
            entity.HasOne<AppTenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        });
        builder.Entity<AppCompanyMembership>(entity =>
        {
            entity.ToTable("APP_COMPANY_MEMBERSHIP");
            entity.HasKey(x => new { x.TenantId, x.BusinessAssociateId, x.UserId });
            entity.HasOne<AppCompany>().WithMany().HasForeignKey(x => new { x.TenantId, x.BusinessAssociateId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.GrantedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        });
        builder.Entity<AppUserRoleExtension>(entity =>
        {
            entity.Property(x => x.TenantId).HasMaxLength(64);
            entity.Property(x => x.BusinessAssociateId).HasMaxLength(128);
            entity.HasOne<AppCompanyMembership>().WithMany()
                .HasForeignKey(x => new { x.TenantId, x.BusinessAssociateId, x.UserId })
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<CompanyMembershipRetirement>(entity =>
        {
            entity.ToTable("APP_COMPANY_MEMBER_RETIREMENT");
            entity.HasKey(x => x.IntentId);
            entity.Property(x => x.IntentId).HasMaxLength(32).ValueGeneratedNever();
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BusinessAssociateId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ActorUserId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.PreviousRevision).HasMaxLength(36).IsRequired();
            entity.Property(x => x.ResultRevision).HasMaxLength(36).IsRequired();
            entity.HasOne<AppCompanyMembership>().WithMany().HasForeignKey(x => new { x.TenantId, x.BusinessAssociateId, x.UserId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TenantId, x.BusinessAssociateId, x.UserId });
        });
    }
}
