using Microsoft.EntityFrameworkCore;

namespace CommonInit;

public static class DbContextOptionsBuilderFactory
{
    public static DbContextOptionsBuilder<TDbContext> Create<TDbContext>(string connectionString) where TDbContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return optionsBuilder;
    }
}