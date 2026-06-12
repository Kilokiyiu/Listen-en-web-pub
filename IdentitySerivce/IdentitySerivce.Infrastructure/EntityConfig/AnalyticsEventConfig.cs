using IdentitySerivce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentitySerivce.Infrastructure.EntityConfig;

public class AnalyticsEventConfig : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.ToTable("T_AnalyticsEvent");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(200).IsRequired();
        builder.Property(x => x.VisitorId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.EventType, x.CreatedAt });
    }
}
