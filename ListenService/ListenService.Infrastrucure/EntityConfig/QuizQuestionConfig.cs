using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListenService.Infrastrucure.EntityConfig;

public class QuizQuestionConfig : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("T_QuizQuestion");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.Property(e => e.Stem).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.OptionsJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(e => e.Explanation).HasMaxLength(4000).IsRequired(false);
        builder.HasIndex(e => new { e.SectionId, e.IsVisible });
        builder.HasIndex(e => new { e.SectionId, e.Number });
    }
}
