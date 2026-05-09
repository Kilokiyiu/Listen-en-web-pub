using ArticleService.Infrastructure;
using CommonInit;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ListenService.Infrastrucure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ArticleDbContext>
{
    public ArticleDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "ArticleService.WebAPI"))
            .AddJsonFile("appsettings.json")
            .Build();

        var connStr = configuration.GetConnectionString("DatabaseConnStr");
        var optionsBuilder = DbContextOptionsBuilderFactory.Create<ArticleDbContext>(connStr);
        return new ArticleDbContext(optionsBuilder.Options);
    }
}