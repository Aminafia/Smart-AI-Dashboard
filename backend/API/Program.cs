using API.Middlewares;
using API.Extensions;
using Application;
using Core.Constants;
using Infrastructure;
using Infrastructure.Auth;
using Infrastructure.Resilience;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Enrichers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configuration means external values the application needs to run. Such as database connection string, JWT secret key, Redis URL, API keys, OpenAI key, ports, environment settings.
// We use COnfiguration because these values change between environments, should NOT be hardcoded, may contain sensitive data.
// Example: Development → localhost DB, Production → cloud PostgreSQL DB
// appsettings.json is the main application configuration file. 
var configuration = builder.Configuration;

var dbconnectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(dbconnectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

// ----------------------
// Logging (Serilog)
// ----------------------

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
        "[CorrelationId: {CorrelationId}] " +
        "[Machine: {MachineName}] " +
        "{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        formatter: new Serilog.Formatting.Compact.RenderedCompactJsonFormatter(),
        path: "logs/log-.json",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ----------------------
// Core Services
// ----------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // for enabling Swagger to discover endpoints
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    // 
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ----------------------
// Application & Infrastructure
// ----------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);

// ----------------------
// Health Checks
// ----------------------
builder.Services.AddCustomHealthChecks(dbconnectionString);

// ----------------------
// External Services (AI + Polly)
// ----------------------
builder.Services.AddHttpClient("AIClient")
    .AddPolicyHandler(AIResiliencePolicy.GetRetryPolicy())
    .AddPolicyHandler(AIResiliencePolicy.GetCircuitBreakerPolicy())
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(5));

// ----------------------
// Authentication & Authorization
// ----------------------
builder.Services.AddAuth(configuration); //customized


// ----------------------
//  API Behavior
// ----------------------
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// ----------------------
// Caching (Redis)
// ----------------------
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// ----------------------
// Rate Limiting
// ----------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddConcurrencyLimiter("concurrency", opt =>
    {
        opt.PermitLimit = 2;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromSeconds(10);
    });
});

var app = builder.Build();

// ----------------------
// Middleware Pipeline
// ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// ----------------------
// Endpoints
// ----------------------
app.MapControllers()
   .RequireRateLimiting("concurrency");

app.MapCustomHealthChecks();

app.Run();