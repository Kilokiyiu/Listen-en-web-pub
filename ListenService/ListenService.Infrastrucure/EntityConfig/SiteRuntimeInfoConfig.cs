using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListenService.Infrastrucure.EntityConfig;

public class SiteRuntimeInfoConfig : IEntityTypeConfiguration<SiteRuntimeInfo>
{
    public void Configure(EntityTypeBuilder<SiteRuntimeInfo> builder)
    {
        builder.ToTable("T_SiteRuntimeInfo");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.Property(e => e.Status).HasMaxLength(32).IsRequired();
        builder.Property(e => e.StatusText).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ExtraInfo).HasMaxLength(500);
        builder.Property(e => e.UpdatedAt).IsRequired();
    }
}
