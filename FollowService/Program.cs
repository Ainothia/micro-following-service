using FollowingService.Data;
using FollowingService.Data.Repositories;
using FollowService.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Load MySQL connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add MySQL database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ✅ Fix Redis Connection Issue
var redisHost = builder.Configuration["Redis__Host"] ?? "redis";  // ✅ Change from "localhost" to "redis"
var redisPort = builder.Configuration["Redis__Port"] ?? "6379";
var redisConnectionString = $"{redisHost}:{redisPort},abortConnect=false"; // ✅ Ensure connection does not fail on startup

Console.WriteLine($"Connecting to Redis at: {redisConnectionString}");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "FollowingService_";
});

// Register repositories and services
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IFollowService, FollowService.Services.FollowService>();
builder.Services.AddHttpClient();

// ✅ Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add controllers and enable OpenAPI (Swagger)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Ensure Swagger works in both Development & Docker
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Following API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Enable routing, CORS, and authorization
app.UseRouting();

app.UseCors("AllowAllOrigins"); // ✅ Enable CORS

app.UseAuthorization();
app.MapControllers();

// Run the application
app.Run();
