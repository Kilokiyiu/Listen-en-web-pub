using KaoyanService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MyJWT;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<KaoyanDbContext>(options =>
{
    var connStr = Environment.GetEnvironmentVariable("DatabaseConnStr")
                  ?? builder.Configuration.GetConnectionString("DatabaseConnStr");
    options.UseSqlServer(connStr);
});

builder.Services.ServiceInit();
builder.ConfigureInfrastructureServices();

builder.Services.AddCors(options =>
{
    var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? new[]
    {
        "http://localhost:8080",
        "http://localhost:5173",
        "http://localhost:5000"
    };
    options.AddPolicy("AllowOrigin", policy => policy
        .WithOrigins(corsOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<KaoyanDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("KaoyanService database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "KaoyanService database migration failed.");
        throw;
    }
}

await PaperCatalogSeeder.SeedAsync(app.Services, app.Logger);

app.Run();
