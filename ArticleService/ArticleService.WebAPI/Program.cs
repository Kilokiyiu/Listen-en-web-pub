using ArticleService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MyJWT;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ArticleDbContext>(options =>
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

// ⚠️ 开发环境注释掉 HTTPS 重定向，避免本地 HTTP 请求被拦截
// 生产环境请配置 HTTPS 并取消注释
// app.UseHttpsRedirection();
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 自动执行 EF Core 数据库迁移
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ArticleDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
