using CommonInit;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentitySerivce.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "IdentityService.WebAPI"))
            .AddJsonFile("appsettings.json")
            .Build();

        var connStr = configuration.GetConnectionString("DatabaseConnStr");
        var optionsBuilder = DbContextOptionsBuilderFactory.Create<IdentityDbContext>(connStr);
        return new IdentityDbContext(optionsBuilder.Options);
    }
}
