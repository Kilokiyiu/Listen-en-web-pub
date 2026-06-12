using Microsoft.EntityFrameworkCore;
using MyJWT;
using WordService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.ConfigureInfrastructureServices();

// Database
builder.Services.AddDbContext<WordDbContext>(options =>
{
    var connStr = Environment.GetEnvironmentVariable("DatabaseConnStr")
                  ?? builder.Configuration.GetConnectionString("DatabaseConnStr");
    options.UseSqlServer(connStr);
});

// CORS
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

// Database init & seed
builder.Services.AddWordDbContextInit();

builder.Services.Configure<WordService.WebAPI.Options.XxApiOptions>(
    builder.Configuration.GetSection(WordService.WebAPI.Options.XxApiOptions.SectionName));
builder.Services.AddHttpClient<WordService.WebAPI.Services.IWordLookupService, WordService.WebAPI.Services.XxApiWordLookupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
