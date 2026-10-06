using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListenService.Infrastrucure.EntityConfig;

public class QuizSectionConfig : IEntityTypeConfiguration<QuizSection>
{
    public void Configure(EntityTypeBuilder<QuizSection> builder)
    {
        builder.ToTable("T_QuizSection");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.GroupName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Transcript).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(e => e.AudioUrl).HasMaxLength(1000).IsRequired(false);
        builder.HasIndex(e => new { e.AlbumId, e.IsVisible });
        builder.HasIndex(e => new { e.AlbumId, e.SequenceNumber });
        builder.HasIndex(e => new { e.AlbumId, e.GroupName, e.SequenceNumber });
    }
}
