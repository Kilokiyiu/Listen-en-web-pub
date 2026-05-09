using ArticleService.Domain.Entity;
using Infrastructure.EFCORE;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure;

public class ArticleDbContext : DbContext
{
    public DbSet<DailyArticle>  DailyArticles { get; set; }
    public DbSet<UserArticleStatus> UserArticleStatuses { get; set; }
    
    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.EnableSoftDeletionGlobalFilter();
    }
}