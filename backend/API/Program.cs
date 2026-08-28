using API.Middlewares;
using API.Extensions;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var dbConnectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(dbConnectionString)) throw new InvalidOperationException("Database connection string is not configured.");

Log.Logger = new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().Enrich.WithMachineName().Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [CorrelationId: {CorrelationId}] [Machine: {MachineName}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(formatter: new RenderedCompactJsonFormatter(), path: "logs/log-.json", rollingInterval: RollingInterval.Day).CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Name = "Authorization", Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = Microsoft.OpenApi.Models.ParameterLocation.Header, Description = "Enter: Bearer {your token}" });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {{ new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }});
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddCustomHealthChecks(dbConnectionString);
builder.Services.AddAuth(configuration);

builder.Services.AddCors(options => options.AddPolicy("AngularPolicy", policy =>
{
    var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

var redisConnection = configuration.GetConnectionString("Redis");
if (string.IsNullOrWhiteSpace(redisConnection)) throw new InvalidOperationException("Redis connection string is not configured.");
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);

builder.Services.AddRateLimiter(options =>
{
    options.AddConcurrencyLimiter("concurrency", opt => { opt.PermitLimit = 2; opt.QueueLimit = 0; });
    options.AddFixedWindowLimiter("fixed", opt => { opt.PermitLimit = 5; opt.Window = TimeSpan.FromSeconds(10); });
});

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors("AngularPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("concurrency");
app.MapCustomHealthChecks();
app.Run();