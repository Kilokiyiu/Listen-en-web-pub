using ArticleService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleService.Infrastructure.EntityConfig;

public class UserArticleStatusConfig : IEntityTypeConfiguration<UserArticleStatus>
{
    public void Configure(EntityTypeBuilder<UserArticleStatus> builder)
    {
        builder.ToTable("T_UserArticleStatus");
        builder.HasIndex(e => new {e.UserId, e.ArticleId}).IsUnique();
        builder.HasOne(e => e.Article) //配置主键
            .WithMany()
            .HasForeignKey(e => e.ArticleId) //外键关联，外键为ArticleId
            .OnDelete(DeleteBehavior.Cascade);
    }
}