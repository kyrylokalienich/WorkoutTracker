using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using WorkoutTracker.API;
using WorkoutTracker.API.Infrastructure;
using WorkoutTracker.Application;
using WorkoutTracker.Application.Configurations;
using WorkoutTracker.Persistence;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// In non-Development environments, load secrets from AWS SSM Parameter Store.
if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddSystemsManager("/workouttracker/");
}

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

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

// CORS — the static frontend (CloudFront) calls the API from a different origin.
// Allowed origins come from config: Cors:AllowedOrigins (set via Cors__AllowedOrigins__0 in prod).
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Map model-binding validation failures to the standard error envelope.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

        var response = new ApiErrorResponse("validation_failed", "One or more fields are invalid.", errors);
        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WorkoutTracker API",
        Version = "v1",
        Description = "REST API for managing workout plans, sessions, and progress reports."
    });

    // Include XML comments from API and Application projects.
    foreach (var xmlFile in new[] { "WorkoutTracker.API.xml", "WorkoutTracker.Application.xml" })
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);
    }

    c.OperationFilter<SwaggerExamplesFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token: Bearer {token}"
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

app.UseExceptionHandler();
app.UseSerilogRequestLogging(opts =>
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
    .WithName("Health")
    .WithOpenApi();

app.Run();

public partial class Program { }
