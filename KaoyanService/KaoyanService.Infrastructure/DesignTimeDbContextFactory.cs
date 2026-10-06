using CommonInit;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KaoyanService.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KaoyanDbContext>
{
    public KaoyanDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "KaoyanService.WebAPI"))
            .AddJsonFile("appsettings.json")
            .Build();

        var connStr = configuration.GetConnectionString("DatabaseConnStr");
        var optionsBuilder = DbContextOptionsBuilderFactory.Create<KaoyanDbContext>(connStr);
        return new KaoyanDbContext(optionsBuilder.Options);
    }
}
