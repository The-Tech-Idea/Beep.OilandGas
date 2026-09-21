using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

internal static class FinancialOperationClaimMapping
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<FinancialOperationClaim>(entity => {
            entity.ToTable("FINANCIAL_OPERATION_CLAIM");
            entity.HasKey(x => x.OperationKey);
            entity.Property(x => x.OperationKey).HasMaxLength(128);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.Property(x => x.OwnerId).HasMaxLength(128);
            entity.Property(x => x.Token).HasMaxLength(36);
            entity.Property(x => x.ChangedBy).HasMaxLength(128).IsRequired();
        });
    }
}
