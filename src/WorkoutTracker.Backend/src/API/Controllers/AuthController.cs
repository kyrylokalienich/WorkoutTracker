using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.Auth;
using WorkoutTracker.Application.Models.Response.Auth;

namespace WorkoutTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        var (success, message) = await _authService.SignUpAsync(request.Email, request.Username, request.Password);

        if (!success)
        {
            return BadRequest(message);
        }

        return Ok(new { message = "User registered successfully. Please sign in." });
    }

    /// <summary>
    /// Authenticate a user and return access/refresh tokens.
    /// </summary>
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, userId, accessToken, refreshToken, expiresAt) =
            await _authService.SignInAsync(request.Email, request.Password);

        if (!success)
        {
            return Unauthorized("Invalid email or password.");
        }

        var response = new AuthResponse
        {
            Id = userId ?? 0,
            Email = request.Email,
            AccessToken = accessToken ?? string.Empty,
            RefreshToken = refreshToken ?? string.Empty,
            ExpiresAt = expiresAt ?? DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// Refresh the access token using a refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract user ID from the request header or token
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized("User not authenticated.");
        }

        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid token.");
        }

        var (success, accessToken, newRefreshToken, expiresAt) =
            await _authService.RefreshTokenAsync(userId, request.RefreshToken);

        if (!success)
        {
            return Unauthorized("Invalid refresh token.");
        }

        var response = new TokenResponse
        {
            AccessToken = accessToken ?? string.Empty,
            RefreshToken = newRefreshToken ?? string.Empty,
            ExpiresAt = expiresAt ?? DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// Logout by revoking the refresh token.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract user ID from the token
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized("User not authenticated.");
        }

        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid token.");
        }

        var success = await _authService.LogoutAsync(userId, request.RefreshToken);

        if (!success)
        {
            return BadRequest("Failed to logout.");
        }

        return Ok(new { message = "Logged out successfully." });
    }
}
