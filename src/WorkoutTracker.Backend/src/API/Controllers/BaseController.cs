using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Infrastructure;
using WorkoutTracker.Application.Common;

namespace WorkoutTracker.API.Controllers;

[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        throw new UnauthorizedAccessException("User ID not found in token.");
    }

    protected IActionResult ToApiResult<T>(ServiceResult<T> result)
    {
        if (result.Succeeded)
            return Ok(result.Value);

        return result.FailureCode switch
        {
            "not_found" => NotFound(
                new ApiErrorResponse("not_found", "The requested resource was not found.")),
            "validation_failed" => BadRequest(
                new ApiErrorResponse("validation_failed", "One or more fields are invalid.", result.FailureDetails)),
            "duplicate_exercise" => BadRequest(
                new ApiErrorResponse("duplicate_exercise", "This exercise is already in the plan.", result.FailureDetails)),
            "invalid_transition" => BadRequest(
                new ApiErrorResponse("invalid_transition", "Illegal status transition.", result.FailureDetails)),
            "invalid_state" => Conflict(
                new ApiErrorResponse("invalid_state", "Resource cannot be changed in its current state.", result.FailureDetails)),
            _ => BadRequest(
                new ApiErrorResponse(result.FailureCode ?? "error", "Request failed.", result.FailureDetails))
        };
    }
}
