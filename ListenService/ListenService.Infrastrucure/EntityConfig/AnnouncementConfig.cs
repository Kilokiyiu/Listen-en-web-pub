using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListenService.Infrastrucure.EntityConfig;

public class AnnouncementConfig : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("T_Announcement");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Content).HasMaxLength(4000).IsRequired();
        builder.Property(e => e.PublishDate).IsRequired();
        builder.Property(e => e.IsPublished).IsRequired();
        builder.Property(e => e.IsPinned).IsRequired();
        builder.Property(e => e.SortOrder).IsRequired();
        builder.Property(e => e.CreationTime).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.HasIndex(e => new { e.IsPublished, e.IsPinned, e.SortOrder, e.PublishDate });
    }
}
