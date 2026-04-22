using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkoutTracker.Application.Configurations;
using WorkoutTracker.Application.Interfaces.Providers;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Providers;
using WorkoutTracker.Application.Services;

namespace WorkoutTracker.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Bind Jwt configuration from appsettings
        services.AddSingleton<JwtConfiguration>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return config.GetSection("Jwt").Get<JwtConfiguration>() ?? new JwtConfiguration();
        });

        // Register providers
        services.AddScoped<IPasswordHasher, PasswordHasherProvider>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        // Register services
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
