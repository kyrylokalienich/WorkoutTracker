using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Persistence.Seed;

namespace WorkoutTracker.Persistence;

public static class AppDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("AppDbInitializer");
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        logger?.LogInformation("Applying database migrations...");
        if (dbContext.Database.IsRelational())
            await dbContext.Database.MigrateAsync(cancellationToken);
        else
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        logger?.LogInformation("Seeding exercises (idempotent)...");
        await ExerciseSeeder.SeedAsync(dbContext, cancellationToken);

        logger?.LogInformation("Database initialization complete.");
    }
}

