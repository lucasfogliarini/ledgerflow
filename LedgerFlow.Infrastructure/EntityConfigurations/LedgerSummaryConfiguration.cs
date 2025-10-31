using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerFlow.Infrastructure.EntityConfigurations;

public class LedgerSummaryConfiguration : IEntityTypeConfiguration<LedgerSummary>
{
    public void Configure(EntityTypeBuilder<LedgerSummary> builder)
    {
        builder.HasKey(t => t.Id);
    }
}
