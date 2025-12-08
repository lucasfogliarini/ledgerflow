using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerFlow.Infrastructure.EntityConfigurations;

public class LedgerSummaryConfiguration : IEntityTypeConfiguration<LedgerSummary>
{
    public void Configure(EntityTypeBuilder<LedgerSummary> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.ReferenceDate).IsRequired();
        builder.Property(t => t.TotalCredits).HasPrecision(18, 2);
        builder.Property(t => t.TotalDebits).HasPrecision(18, 2);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt);
        builder.Ignore(t => t.Balance);

        builder
            .HasMany(ls => ls.Transactions)
            .WithMany();
    }
}
