using IdentitySerivce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentitySerivce.Infrastructure.EntityConfig;

public class StudyActivityConfig : IEntityTypeConfiguration<StudyActivity>
{
    public void Configure(EntityTypeBuilder<StudyActivity> builder)
    {
        builder.ToTable("T_StudyActivity");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActivityType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ContentId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.ActivityType, x.ContentId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.UpdatedAt });
    }
}
