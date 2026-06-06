using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Infrastructure;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.Auth;
using WorkoutTracker.Application.Models.Response.Auth;

namespace WorkoutTracker.API.Controllers;

/// <summary>Sign up, sign in, refresh tokens, and sign out.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Create a new account.</summary>
    /// <response code="200">Account created. Call sign-in to get tokens.</response>
    /// <response code="400">Email or username already taken, or passwords don't match.</response>
    [HttpPost("sign-up")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new ApiErrorResponse(
                "validation_failed",
                "One or more fields are invalid.",
                new { confirmPassword = new[] { "Passwords do not match." } }));
        }

        var (success, message) = await _authService.SignUpAsync(request.Email, request.Username, request.Password);

        if (!success)
            return BadRequest(new ApiErrorResponse("registration_failed", message ?? "Registration failed."));

        return Ok(new { message = "User registered successfully. Please sign in." });
    }

    /// <summary>Sign in and receive access and refresh tokens.</summary>
    /// <response code="401">Wrong email or password.</response>
    [HttpPost("sign-in")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        var (success, userId, accessToken, refreshToken, expiresAt) =
            await _authService.SignInAsync(request.Email, request.Password);

        if (!success)
            return Unauthorized(new ApiErrorResponse("unauthorized", "Invalid email or password."));

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

    /// <summary>Get a new token pair using a refresh token. The old refresh token is revoked.</summary>
    /// <response code="401">Not authenticated, or the refresh token is invalid or expired.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new ApiErrorResponse("unauthorized", "User not authenticated."));

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new ApiErrorResponse("unauthorized", "Invalid token."));

        var (success, accessToken, newRefreshToken, expiresAt) =
            await _authService.RefreshTokenAsync(userId, request.RefreshToken);

        if (!success)
            return Unauthorized(new ApiErrorResponse("unauthorized", "Invalid or expired refresh token."));

        var response = new TokenResponse
        {
            AccessToken = accessToken ?? string.Empty,
            RefreshToken = newRefreshToken ?? string.Empty,
            ExpiresAt = expiresAt ?? DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>Sign out and revoke the refresh token.</summary>
    /// <response code="400">Token could not be revoked.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new ApiErrorResponse("unauthorized", "User not authenticated."));

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new ApiErrorResponse("unauthorized", "Invalid token."));

        var success = await _authService.LogoutAsync(userId, request.RefreshToken);

        if (!success)
            return BadRequest(new ApiErrorResponse("logout_failed", "Failed to revoke token."));

        return Ok(new { message = "Logged out successfully." });
    }
}
