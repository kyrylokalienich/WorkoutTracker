using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WorkoutTracker.Application.Configurations;
using WorkoutTracker.Persistence;

namespace WorkoutTracker.Tests.Controllers;

public class WorkoutTrackerFactory : WebApplicationFactory<Program>
{
    internal const string TestJwtSecret = "integration-test-secret-key-for-tests-only!!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=test;Database=test;Username=test;Password=test",
                ["Jwt:SecretKey"] = TestJwtSecret,
                ["Jwt:Issuer"] = "WorkoutTracker.API",
                ["Jwt:Audience"] = "WorkoutTracker.Client"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace Postgres DbContext with InMemory
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            var dbName = "IntegrationTests_" + Guid.NewGuid();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            // Replace JwtConfiguration singleton so JwtProvider signs with the test key
            var jwtDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(JwtConfiguration));
            if (jwtDescriptor != null)
                services.Remove(jwtDescriptor);

            services.AddSingleton(new JwtConfiguration
            {
                SecretKey = TestJwtSecret,
                Issuer = "WorkoutTracker.API",
                Audience = "WorkoutTracker.Client",
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 7
            });

            // Override JWT Bearer validation parameters to match the test signing key
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, opts =>
            {
                opts.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
            });
        });
    }
}
