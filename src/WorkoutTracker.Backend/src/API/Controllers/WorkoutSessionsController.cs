using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.WorkoutSessions;
using WorkoutTracker.Application.Models.Response.WorkoutSessions;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.API.Controllers;

/// <summary>
/// API endpoints for managing workout sessions.
/// </summary>
[Route("api/workout-sessions")]
public class WorkoutSessionsController : BaseController
{
    private readonly IWorkoutSessionService _workoutSessionService;

    public WorkoutSessionsController(IWorkoutSessionService workoutSessionService)
    {
        _workoutSessionService = workoutSessionService;
    }

    /// <summary>
    /// Schedule a new workout session (optionally from a plan snapshot).
    /// </summary>
    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleWorkoutSession(
        [FromBody] ScheduleWorkoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.ScheduleAsync(userId, request, cancellationToken);
        return ToActionResult(result, createdId: result.Value?.Id);
    }

    /// <summary>
    /// List workout sessions with filters and pagination (default sort: scheduledAtUtc ascending).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkoutSessions(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        WorkoutStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<WorkoutStatus>(status.Trim(), ignoreCase: true, out var parsed))
            {
                return BadRequest(new
                {
                    code = "validation_failed",
                    message = "Invalid status filter.",
                    details = new { status = new[] { "Use Planned, InProgress, Completed, or Skipped." } }
                });
            }

            statusFilter = parsed;
        }

        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.ListAsync(
            userId,
            statusFilter,
            from,
            to,
            page,
            pageSize,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { code = result.FailureCode, details = result.FailureDetails });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get a workout session by id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWorkoutSessionById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.GetByIdAsync(userId, id, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update session metadata and allowed status transitions.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWorkoutSession(
        int id,
        [FromBody] UpdateWorkoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.UpdateAsync(userId, id, request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Complete a session and persist actual sets, reps, weight, and notes per exercise.
    /// </summary>
    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> CompleteWorkoutSession(
        int id,
        [FromBody] CompleteWorkoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.CompleteAsync(userId, id, request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Add a new exercise to an in-progress session.
    /// </summary>
    [HttpPost("{id:int}/exercises")]
    public async Task<IActionResult> AddSessionExercise(
        int id,
        [FromBody] AddSessionExerciseRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.AddExerciseAsync(userId, id, request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete a workout session.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteWorkoutSession(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.DeleteAsync(userId, id, cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound(new { code = result.FailureCode, message = "Workout session not found." });
        }

        return NoContent();
    }

    private IActionResult ToActionResult(ServiceResult<WorkoutSessionDetailResponse> result, int? createdId = null)
    {
        if (result.Succeeded)
        {
            if (createdId.HasValue)
            {
                return CreatedAtAction(
                    nameof(GetWorkoutSessionById),
                    new { id = createdId.Value },
                    result.Value);
            }

            return Ok(result.Value);
        }

        return result.FailureCode switch
        {
            "not_found" => NotFound(new { code = "not_found", message = "Not found." }),
            "validation_failed" => BadRequest(new
            {
                code = result.FailureCode,
                message = "One or more fields are invalid.",
                details = result.FailureDetails
            }),
            "invalid_transition" => BadRequest(new
            {
                code = result.FailureCode,
                message = "Illegal status transition.",
                details = result.FailureDetails
            }),
            "invalid_state" => Conflict(new
            {
                code = result.FailureCode,
                message = "Session cannot be changed in its current state.",
                details = result.FailureDetails
            }),
            _ => BadRequest(new { code = result.FailureCode, message = "Request failed.", details = result.FailureDetails })
        };
    }
}
