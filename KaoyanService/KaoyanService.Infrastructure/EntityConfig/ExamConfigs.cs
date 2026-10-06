using KaoyanService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KaoyanService.Infrastructure.EntityConfig;

public class ExamPaperConfig : IEntityTypeConfiguration<ExamPaper>
{
    public void Configure(EntityTypeBuilder<ExamPaper> builder)
    {
        builder.ToTable("T_ExamPaper");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.Property(e => e.Series).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(e => new { e.Year, e.Series }).IsUnique();
        builder.HasIndex(e => e.IsVisible);
    }
}

public class ExamSectionConfig : IEntityTypeConfiguration<ExamSection>
{
    public void Configure(EntityTypeBuilder<ExamSection> builder)
    {
        builder.ToTable("T_ExamSection");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.Property(e => e.SectionType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Passage).HasColumnType("nvarchar(max)").IsRequired();
        builder.HasIndex(e => new { e.PaperId, e.SequenceNumber });
    }
}

public class ExamQuestionConfig : IEntityTypeConfiguration<ExamQuestion>
{
    public void Configure(EntityTypeBuilder<ExamQuestion> builder)
    {
        builder.ToTable("T_ExamQuestion");
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.Property(e => e.Stem).HasMaxLength(4000).IsRequired();
        builder.Property(e => e.OptionsJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(e => e.Explanation).HasMaxLength(4000).IsRequired(false);
        builder.HasIndex(e => new { e.SectionId, e.Number });
    }
}
