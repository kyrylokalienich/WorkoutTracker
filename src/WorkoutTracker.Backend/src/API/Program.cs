using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WorkoutTracker.API;
using WorkoutTracker.Application;
using WorkoutTracker.Application.Configurations;
using WorkoutTracker.Persistence;

// Load .env into process environment before configuration binds (overrides appsettings via env vars).
DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

// JWT: non-sensitive defaults may live in appsettings; signing key must come from environment / .env (Jwt__SecretKey).
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfiguration>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing from appsettings.");

if (string.IsNullOrWhiteSpace(jwtConfig.SecretKey))
{
    throw new InvalidOperationException(
        "Jwt:SecretKey is not set. Add Jwt__SecretKey to a .env file (see src/API/.env.example) or set the environment variable.");
}

var signingKeyBytes = Encoding.UTF8.GetBytes(jwtConfig.SecretKey);
if (signingKeyBytes.Length < 32)
{
    throw new InvalidOperationException("Jwt:SecretKey must be at least 32 UTF-8 bytes for HS256.");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtConfig.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WorkoutTracker API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste: Bearer {your JWT token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

await AppDbInitializer.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
    .WithName("Health")
    .WithOpenApi();

app.Run();
