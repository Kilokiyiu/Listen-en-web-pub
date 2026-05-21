using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordService.Domain.Entity;

namespace WordService.Infrastructure.EntityConfig;

public class WordRootConfig : IEntityTypeConfiguration<WordRoot>
{
    public void Configure(EntityTypeBuilder<WordRoot> builder)
    {
        builder.ToTable("T_WordRoot");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Root).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Origin).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Meaning).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MeaningEn).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => x.RootId).IsUnique();
    }
}

public class WordRootExampleConfig : IEntityTypeConfiguration<WordRootExample>
{
    public void Configure(EntityTypeBuilder<WordRootExample> builder)
    {
        builder.ToTable("T_WordRootExample");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Word).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Prefix).HasMaxLength(50);
        builder.Property(x => x.Root).HasMaxLength(50);
        builder.Property(x => x.Suffix).HasMaxLength(50);
        builder.Property(x => x.Meaning).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Explanation).HasMaxLength(1000);
        builder.HasOne(x => x.WordRoot).WithMany(x => x.Examples).HasForeignKey(x => x.WordRootId);
    }
}

public class WordRootQuizConfig : IEntityTypeConfiguration<WordRootQuiz>
{
    public void Configure(EntityTypeBuilder<WordRootQuiz> builder)
    {
        builder.ToTable("T_WordRootQuiz");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Question).HasMaxLength(500).IsRequired();
        builder.Property(x => x.OptionsJson).HasMaxLength(2000).IsRequired();
        builder.HasOne(x => x.WordRoot).WithMany(x => x.Quizzes).HasForeignKey(x => x.WordRootId);
    }
}

public class UserWordRootProgressConfig : IEntityTypeConfiguration<UserWordRootProgress>
{
    public void Configure(EntityTypeBuilder<UserWordRootProgress> builder)
    {
        builder.ToTable("T_UserWordRootProgress");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.WordRootId }).IsUnique();
        builder.HasOne(x => x.WordRoot).WithMany().HasForeignKey(x => x.WordRootId);
    }
}

public class UserWordConfig : IEntityTypeConfiguration<UserWord>
{
    public void Configure(EntityTypeBuilder<UserWord> builder)
    {
        builder.ToTable("T_UserWord");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Word).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Definition).HasMaxLength(1000);
        builder.Property(x => x.Example).HasMaxLength(2000);
        builder.HasIndex(x => new { x.UserId, x.Word });
        builder.Property(x => x.EaseFactor).HasDefaultValue(2.5);
    }
}

public class WordReviewLogConfig : IEntityTypeConfiguration<WordReviewLog>
{
    public void Configure(EntityTypeBuilder<WordReviewLog> builder)
    {
        builder.ToTable("T_WordReviewLog");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.WordId, x.CreationTime });
    }
}
