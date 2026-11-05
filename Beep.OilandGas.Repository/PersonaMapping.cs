using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

internal static class PersonaMapping
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<AppPersona>(entity =>
        {
            entity.ToTable("APP_PERSONA");
            entity.HasKey(x => x.Code);
        });
        builder.Entity<AppUserPersona>(entity =>
        {
            entity.ToTable("APP_USER_PERSONA");
            entity.HasKey(x => new { x.UserId, x.TenantId, x.BusinessAssociateId });
            entity.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AppCompany>().WithMany().HasForeignKey(x => new { x.TenantId, x.BusinessAssociateId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AppPersona>().WithMany().HasForeignKey(x => x.PersonaCode).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<AppPersonaPreference>(entity =>
        {
            entity.ToTable("APP_PERSONA_PREFERENCE");
            // The full company/view key can exceed SQL Server's clustered-key limit.
            entity.HasKey(x => new { x.UserId, x.TenantId, x.BusinessAssociateId, x.PersonaCode, x.ViewKey }).IsClustered(false);
            entity.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AppCompany>().WithMany().HasForeignKey(x => new { x.TenantId, x.BusinessAssociateId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AppPersona>().WithMany().HasForeignKey(x => x.PersonaCode).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<AppPersonaAudit>(entity =>
        {
            entity.ToTable("APP_PERSONA_AUDIT");
            entity.HasKey(x => x.Id);
            entity.HasOne<OilGasUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.ChangedUtc });
        });
    }
}
