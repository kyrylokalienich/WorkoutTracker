namespace WorkoutTracker.Application.Models.Response.Auth;

/// <summary>New token pair returned after a successful refresh.</summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
