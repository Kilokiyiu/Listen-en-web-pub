using IdentitySerivce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentitySerivce.Infrastructure.EntityConfig;

public class AnalyticsDailyConfig : IEntityTypeConfiguration<AnalyticsDaily>
{
    public void Configure(EntityTypeBuilder<AnalyticsDaily> builder)
    {
        builder.ToTable("T_AnalyticsDaily");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Path).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.Date, x.Path }).IsUnique();
        builder.HasIndex(x => x.Date);
    }
}
