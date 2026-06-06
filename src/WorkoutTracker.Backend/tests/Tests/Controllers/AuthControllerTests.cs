using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WorkoutTracker.Application.Models.Response.Auth;

namespace WorkoutTracker.Tests.Controllers;

public class AuthControllerTests : IClassFixture<WorkoutTrackerFactory>
{
    private readonly WorkoutTrackerFactory _factory;

    public AuthControllerTests(WorkoutTrackerFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SignUp_ValidRequest_Returns200()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "signup_valid@example.com",
            username = "signup_valid",
            password = "Password123!",
            confirmPassword = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_PasswordMismatch_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "mismatch@example.com",
            username = "mismatch_user",
            password = "Password123!",
            confirmPassword = "Different456!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_DuplicateEmail_Returns400()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            email = "duplicate@example.com",
            username = "duplicate_user1",
            password = "Password123!",
            confirmPassword = "Password123!"
        };

        await client.PostAsJsonAsync("/api/auth/sign-up", payload);
        var response = await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "duplicate@example.com",
            username = "duplicate_user2",
            password = "Password123!",
            confirmPassword = "Password123!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SignIn_ValidCredentials_Returns200WithTokens()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "signin_valid@example.com",
            username = "signin_valid",
            password = "Password123!",
            confirmPassword = "Password123!"
        });

        var response = await client.PostAsJsonAsync("/api/auth/sign-in", new
        {
            email = "signin_valid@example.com",
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
    }

    [Fact]
    public async Task SignIn_WrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "signin_wrong@example.com",
            username = "signin_wrong",
            password = "Password123!",
            confirmPassword = "Password123!"
        });

        var response = await client.PostAsJsonAsync("/api/auth/sign-in", new
        {
            email = "signin_wrong@example.com",
            password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkoutPlans_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/workout-plans");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkoutPlans_WithValidToken_Returns200()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "plans_auth@example.com",
            username = "plans_auth",
            password = "Password123!",
            confirmPassword = "Password123!"
        });
        var signInResponse = await client.PostAsJsonAsync("/api/auth/sign-in", new
        {
            email = "plans_auth@example.com",
            password = "Password123!"
        });
        var auth = await signInResponse.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await client.GetAsync("/api/workout-plans");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkoutPlan_WithValidToken_Returns200()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "createplan@example.com",
            username = "createplan_user",
            password = "Password123!",
            confirmPassword = "Password123!"
        });
        var signInResponse = await client.PostAsJsonAsync("/api/auth/sign-in", new
        {
            email = "createplan@example.com",
            password = "Password123!"
        });
        var auth = await signInResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await client.PostAsJsonAsync("/api/workout-plans", new
        {
            name = "My Test Plan",
            description = "Created in integration test",
            isActive = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ScheduleSession_WithValidToken_Returns200()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "schedule@example.com",
            username = "schedule_user",
            password = "Password123!",
            confirmPassword = "Password123!"
        });
        var signInResponse = await client.PostAsJsonAsync("/api/auth/sign-in", new
        {
            email = "schedule@example.com",
            password = "Password123!"
        });
        var auth = await signInResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await client.PostAsJsonAsync("/api/workout-sessions/schedule", new
        {
            title = "Morning Run",
            scheduledAtUtc = DateTime.UtcNow.AddDays(1)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
