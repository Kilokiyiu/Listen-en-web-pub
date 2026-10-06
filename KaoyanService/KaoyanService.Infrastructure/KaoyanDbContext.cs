using Infrastructure.EFCORE;
using KaoyanService.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace KaoyanService.Infrastructure;

public class KaoyanDbContext : DbContext
{
    public DbSet<ExamPaper> ExamPapers { get; private set; }
    public DbSet<ExamSection> ExamSections { get; private set; }
    public DbSet<ExamQuestion> ExamQuestions { get; private set; }

    public KaoyanDbContext(DbContextOptions<KaoyanDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.EnableSoftDeletionGlobalFilter();
    }
}
