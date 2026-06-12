using Infrastructure.EFCORE;
using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListenService.Infrastrucure.EntityConfig;

public class AlbumConfig : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("T_Album");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.OwnsOneMultilingualString(e => e.Name);
        builder.Property(e => e.PaperFileUrl).HasMaxLength(1000).IsRequired(false);
        builder.Property(e => e.AnswerFileUrl).HasMaxLength(1000).IsRequired(false);
    }
}