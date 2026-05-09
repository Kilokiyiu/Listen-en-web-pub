using ArticleService.Domain.Entity;
using Infrastructure.EFCORE;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleService.Infrastructure.EntityConfig;

public class DailyArticleConfig : IEntityTypeConfiguration<DailyArticle>
{
    public void Configure(EntityTypeBuilder<DailyArticle> builder)
    {
        builder.ToTable("T_DailyArticle");
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.OwnsOneMultilingualString(e => e.Title);
        builder.Property(e => e.CreationTime).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.IsPublished).HasDefaultValue(false);
        builder.HasIndex(e => e.PublicDate).IsUnique();
    }
}