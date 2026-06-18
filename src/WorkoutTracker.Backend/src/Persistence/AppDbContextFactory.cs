using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkoutTracker.Persistence;

/// <summary>
/// Design-time factory so `dotnet ef migrations` can build the context without booting
/// the API host (and thus without AWS/SSM). The connection string is only used for
/// scaffolding — no database connection is made when generating a migration.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=workouttracker;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
