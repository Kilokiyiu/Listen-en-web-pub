using ListenService.Infrastrucure;
using ListenService.WebAPI;
using Microsoft.EntityFrameworkCore;
using MyJWT;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

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

builder.Services.AddDbContext<ListenDbContext>(options =>
{
    var connStr = Environment.GetEnvironmentVariable("DatabaseConnStr")
        ?? builder.Configuration.GetConnectionString("DatabaseConnStr");
    options.UseSqlServer(connStr);
});

builder.Services.ServiceInit();
builder.ConfigureInfrastructureServices(); //注册 JWT 认证

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 60_000_000; //60MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 60_000_000; //60MB
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowOrigin");
app.UseAuthentication(); // JWT 认证
app.UseAuthorization();  // 角色授权
app.UseStaticFiles();
app.MapControllers();

//测试数据库用
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ListenDbContext>();
    await DataSeeder.SeedAsync(dbContext);
}

app.Run();