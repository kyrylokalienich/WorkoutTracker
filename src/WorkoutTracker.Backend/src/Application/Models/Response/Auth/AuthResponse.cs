namespace WorkoutTracker.Application.Models.Response.Auth;

/// <summary>Sign-in response with user info and tokens.</summary>
public class AuthResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
