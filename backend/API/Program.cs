using Application;
using Infrastructure;
using Infrastructure.Auth;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using API.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Core.Constants;
using Serilog;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// 1. Core Services
builder.Services.AddControllers();

// 2. Application layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 3. External integrations (Polly / HttpClient)
builder.Services.AddHttpClient("AIClient")
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(5))
    .AddPolicyHandler(GetRetryPolicy());

// 4. API tools
builder.Services.AddEndpointsApiExplorer();
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

// 5. Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)
        )
    };
});


builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(Roles.Admin));
    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole(Roles.User));
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Host.UseSerilog();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

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

app.MapControllers().RequireRateLimiting("concurrency");

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (outcome, timespan, retryAttempt, context) =>
            {
                Log.Warning(
                    "Retry {RetryAttempt} after {Delay}s due to {Reason}",
                    retryAttempt,
                    timespan.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()
                );
            });
}