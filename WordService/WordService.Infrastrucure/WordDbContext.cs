using Microsoft.EntityFrameworkCore;
using WordService.Domain.Entity;

namespace WordService.Infrastructure;

public class WordDbContext : DbContext
{
    public DbSet<WordRoot> WordRoots { get; set; }
    public DbSet<WordRootExample> WordRootExamples { get; set; }
    public DbSet<WordRootQuiz> WordRootQuizzes { get; set; }
    public DbSet<UserWordRootProgress> UserWordRootProgresses { get; set; }
    public DbSet<UserWord> UserWords { get; set; }
    public DbSet<WordReviewLog> WordReviewLogs { get; set; }

    public WordDbContext(DbContextOptions<WordDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
