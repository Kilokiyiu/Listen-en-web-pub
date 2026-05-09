using Infrastructure.EFCORE;
using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListenService.Infrastrucure.EntityConfig;

public class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("T_Category");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.OwnsOneMultilingualString(e => e.Name);
        builder.Property(e => e.CoverUrl).IsRequired();
    }
}