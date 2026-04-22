namespace WorkoutTracker.Application.Interfaces.Providers;

public interface IJwtProvider
{
    /// <summary>
    /// Generates a JWT access token.
    /// </summary>
    /// <param name="userId">User ID to encode in token</param>
    /// <param name="email">User email to encode in token</param>
    /// <param name="role">User role to encode in token</param>
    /// <returns>JWT token string</returns>
    string GenerateAccessToken(int userId, string email, string role);

    /// <summary>
    /// Generates a refresh token string (unhashed).
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT access token and extracts user ID claim.
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>User ID from 'sub' claim, or null if invalid</returns>
    int? GetUserIdFromToken(string token);
}
