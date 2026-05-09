using CommonInit;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ListenService.Infrastrucure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ListenDbContext>
{
    public ListenDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "ListenService.WebAPI"))
            .AddJsonFile("appsettings.json")
            .Build();

        var connStr = configuration.GetConnectionString("DatabaseConnStr");
        var optionsBuilder = DbContextOptionsBuilderFactory.Create<ListenDbContext>(connStr);
        return new ListenDbContext(optionsBuilder.Options);
    }
}