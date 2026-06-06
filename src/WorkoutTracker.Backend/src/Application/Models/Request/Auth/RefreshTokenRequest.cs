namespace WorkoutTracker.Application.Models.Request.Auth;

/// <summary>Refresh token body, used for both token refresh and sign-out.</summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
