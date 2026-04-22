namespace WorkoutTracker.Application.Models.Request.Auth;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
