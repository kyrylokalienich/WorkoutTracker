using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    Task<(bool Success, string? Message)> SignUpAsync(string email, string username, string password);

    /// <summary>
    /// Authenticates a user and returns auth tokens.
    /// </summary>
    Task<(bool Success, int? UserId, string? AccessToken, string? RefreshToken, DateTime? ExpiresAt)> SignInAsync(
        string email, string password);

    /// <summary>
    /// Rotates the refresh token and invalidates the old one.
    /// </summary>
    Task<(bool Success, string? AccessToken, string? NewRefreshToken, DateTime? ExpiresAt)> RefreshTokenAsync(
        int userId, string oldRefreshToken);

    /// <summary>
    /// Revokes the user's current refresh token.
    /// </summary>
    Task<bool> LogoutAsync(int userId, string refreshToken);

    /// <summary>
    /// Validates if an email is already in use.
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Validates if a username is already in use.
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);
}
