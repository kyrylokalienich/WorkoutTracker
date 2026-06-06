namespace WorkoutTracker.Application.Models.Request.Auth;

/// <summary>Sign-in credentials.</summary>
public class SignInRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
