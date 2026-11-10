using CommonInit;
using KaoyanService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MyCache;
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
builder.Services.AddSqlConnectionKeepAlive();

builder.Services.AddMemoryCacheService(builder.Configuration);
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

// 考研解析 PDF 常达 100MB+，放宽 multipart / 请求体上限
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 220_000_000;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 220_000_000;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

var wwwroot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(Path.Combine(wwwroot, "papers"));

app.UseCors("AllowOrigin");
app.UseStaticFiles();
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
