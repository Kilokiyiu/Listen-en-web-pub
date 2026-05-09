using Infrastructure.EFCORE;
using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListenService.Infrastrucure.EntityConfig;

public class EpisodeConfig : IEntityTypeConfiguration<Episode>
{
    public void Configure(EntityTypeBuilder<Episode> builder)
    {
        builder.ToTable("T_Episode");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.HasIndex(e => new { e.AlbumId, e.IsVisible });
        builder.OwnsOneMultilingualString(e => e.Name);
        builder.Property(e => e.AudioUrl).HasMaxLength(1000).IsUnicode().IsRequired();
        builder.Property(e => e.Subtitle).HasMaxLength(int.MaxValue).IsUnicode().IsRequired();
        builder.Property(e => e.SubtitleType).HasMaxLength(10).IsUnicode(false).IsRequired();
    }
}