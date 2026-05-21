using CommonInit;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WordService.Infrastructure;

namespace WordService.Infrastrucure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WordDbContext>
{
    public WordDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "WordService.WebAPI"))
            .AddJsonFile("appsettings.json")
            .Build();

        var connStr = configuration.GetConnectionString("DatabaseConnStr");
        var optionsBuilder = DbContextOptionsBuilderFactory.Create<WordDbContext>(connStr);
        return new WordDbContext(optionsBuilder.Options);
    }
}
