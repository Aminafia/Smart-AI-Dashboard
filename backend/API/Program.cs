using API.Services;
using Application.Services;
using Application.Services.Interfaces;
using Application;
using Core.Interfaces;
using Core.Constants;
using Infrastructure.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.Validation;
using Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IAiService, AiService>(
    client =>
    {
        client.BaseAddress = new Uri("http://localhost:8000");
        client.Timeout = TimeSpan.FromSeconds(180);
    }
);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("SmartAIDashboardDb");
});
builder.Services.AddApplication();
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>)
);
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthorization();
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

if (jwtSettings == null)
{
    throw new Exception("JwtSettings is missing from configuration");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            RoleClaimType = ClaimTypes.Role

        };
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

    var adminEmail = "admin@admin.com";

    var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);

    if (existingAdmin == null)
    {
        var adminUser = new Core.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            FullName = "System Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
            Role = Roles.Admin,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(adminUser);
    }
}


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
