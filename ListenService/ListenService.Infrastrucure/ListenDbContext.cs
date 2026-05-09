using Infrastructure.EFCORE;
using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace ListenService.Infrastrucure;

public class ListenDbContext : DbContext
{
    public DbSet<Category> Categories { get; private set; }
    public DbSet<Album> Albums { get; private set; }
    public DbSet<Episode> Episodes { get; private set; }

    public ListenDbContext(DbContextOptions<ListenDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.EnableSoftDeletionGlobalFilter();
    }
}