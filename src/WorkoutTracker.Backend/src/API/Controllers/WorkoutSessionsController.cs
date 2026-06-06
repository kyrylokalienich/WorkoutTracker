using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Infrastructure;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.WorkoutSessions;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.API.Controllers;

[Route("api/workout-sessions")]
public class WorkoutSessionsController : BaseController
{
    private readonly IWorkoutSessionService _workoutSessionService;

    public WorkoutSessionsController(IWorkoutSessionService workoutSessionService)
    {
        _workoutSessionService = workoutSessionService;
    }

    [HttpPost("schedule")]
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWorkoutSessionById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.GetByIdAsync(userId, id, cancellationToken);
        return ToApiResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWorkoutSession(
        int id,
        [FromBody] UpdateWorkoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.UpdateAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> CompleteWorkoutSession(
        int id,
        [FromBody] CompleteWorkoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.CompleteAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    [HttpPost("{id:int}/exercises")]
    public async Task<IActionResult> AddSessionExercise(
        int id,
        [FromBody] AddSessionExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.AddExerciseAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteWorkoutSession(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutSessionService.DeleteAsync(userId, id, cancellationToken);
        if (!result.Succeeded)
            return ToApiResult(result);

        return NoContent();
    }
}
