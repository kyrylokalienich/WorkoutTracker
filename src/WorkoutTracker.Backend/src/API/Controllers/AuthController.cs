using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Infrastructure;
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

    [HttpPost("sign-up")]
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
        {
            return BadRequest(new ApiErrorResponse("registration_failed", message ?? "Registration failed."));
        }

        return Ok(new { message = "User registered successfully. Please sign in." });
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        var (success, userId, accessToken, refreshToken, expiresAt) =
            await _authService.SignInAsync(request.Email, request.Password);

        if (!success)
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "Invalid email or password."));
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

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "User not authenticated."));
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "Invalid token."));
        }

        var (success, accessToken, newRefreshToken, expiresAt) =
            await _authService.RefreshTokenAsync(userId, request.RefreshToken);

        if (!success)
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "Invalid or expired refresh token."));
        }

        var response = new TokenResponse
        {
            AccessToken = accessToken ?? string.Empty,
            RefreshToken = newRefreshToken ?? string.Empty,
            ExpiresAt = expiresAt ?? DateTime.UtcNow
        };

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "User not authenticated."));
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "Invalid token."));
        }

        var success = await _authService.LogoutAsync(userId, request.RefreshToken);

        if (!success)
        {
            return BadRequest(new ApiErrorResponse("logout_failed", "Failed to revoke token."));
        }

        return Ok(new { message = "Logged out successfully." });
    }
}
