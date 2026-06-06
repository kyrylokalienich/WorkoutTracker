using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Infrastructure;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.WorkoutSessions;
using WorkoutTracker.Application.Models.Response.WorkoutSessions;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.API.Controllers;

/// <summary>Schedule, track, and complete workout sessions.</summary>
[Route("api/workout-sessions")]
[Produces("application/json")]
public class WorkoutSessionsController : BaseController
{
    private readonly IWorkoutSessionService _workoutSessionService;

    public WorkoutSessionsController(IWorkoutSessionService workoutSessionService)
    {
        _workoutSessionService = workoutSessionService;
    }

    /// <summary>Schedule a session. If <c>workoutPlanId</c> is set, exercises are copied from the plan.</summary>
    /// <response code="400">Validation error or plan not found.</response>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(WorkoutSessionDetailResponse), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> ScheduleWorkoutSession(
        [FromBody] ScheduleWorkoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.ScheduleAsync(userId, request, cancellationToken);
        if (!result.Succeeded)
            return ToApiResult(result);

        return CreatedAtAction(nameof(GetWorkoutSessionById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>List sessions sorted by scheduled date, with optional filters and pagination.</summary>
    /// <param name="status">Planned, InProgress, Completed, or Skipped.</param>
    /// <param name="from">Only sessions scheduled on or after this date (UTC).</param>
    /// <param name="to">Only sessions scheduled on or before this date (UTC).</param>
    /// <param name="page">Page number, 1-based (default 1).</param>
    /// <param name="pageSize">Page size, max 100 (default 10).</param>
    /// <response code="400">Unknown status value.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedWorkoutSessionsResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
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
                return BadRequest(new ApiErrorResponse(
                    "validation_failed",
                    "One or more fields are invalid.",
                    new { status = new[] { "Use Planned, InProgress, Completed, or Skipped." } }));
            }

            statusFilter = parsed;
        }

        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.ListAsync(
            userId, statusFilter, from, to, page, pageSize, cancellationToken);

        return ToApiResult(result);
    }

    /// <summary>Get a session with all exercise details.</summary>
    /// <response code="404">Session not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkoutSessionDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> GetWorkoutSessionById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.GetByIdAsync(userId, id, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Update a session. Allowed status moves: Planned → InProgress or Planned → Skipped.</summary>
    /// <response code="400">Validation error or illegal status transition.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="409">Session state doesn't allow changes.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WorkoutSessionDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> UpdateWorkoutSession(
        int id,
        [FromBody] UpdateWorkoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.UpdateAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Complete a session and log actual sets, reps, and weight for each exercise.</summary>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="409">Session is already completed or skipped.</response>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(WorkoutSessionDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> CompleteWorkoutSession(
        int id,
        [FromBody] CompleteWorkoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.CompleteAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Add an exercise to an in-progress session.</summary>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Session or exercise not found.</response>
    /// <response code="409">Session is not in progress.</response>
    [HttpPost("{id:int}/exercises")]
    [ProducesResponseType(typeof(WorkoutSessionDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> AddSessionExercise(
        int id,
        [FromBody] AddSessionExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.AddExerciseAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Delete a session.</summary>
    /// <response code="404">Session not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> DeleteWorkoutSession(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.DeleteAsync(userId, id, cancellationToken);
        if (!result.Succeeded)
            return ToApiResult(result);

        return NoContent();
    }
}
