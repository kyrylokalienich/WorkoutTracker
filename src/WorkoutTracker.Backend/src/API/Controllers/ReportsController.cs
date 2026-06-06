using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Infrastructure;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Response.Reports;

namespace WorkoutTracker.API.Controllers;

/// <summary>Workout history and progress reports.</summary>
[Route("api/reports")]
[Produces("application/json")]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Completed workouts in a date range, with volume totals per session.</summary>
    /// <param name="from">Start date (UTC, inclusive). Required.</param>
    /// <param name="to">End date (UTC, inclusive). Required. Max range is 731 days.</param>
    /// <response code="400">Missing or invalid date range.</response>
    [HttpGet("workout-history")]
    [ProducesResponseType(typeof(WorkoutHistoryReportResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> GetWorkoutHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.GetWorkoutHistoryAsync(userId, from, to, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Total volume lifted, workout count, and completion rate for a date range.</summary>
    /// <param name="from">Start date (UTC, inclusive). Required.</param>
    /// <param name="to">End date (UTC, inclusive). Required. Max range is 731 days.</param>
    /// <response code="400">Missing or invalid date range.</response>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(ProgressReportResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> GetProgress(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.GetProgressAsync(userId, from, to, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Volume lifted per muscle group over a date range, sorted highest first.</summary>
    /// <param name="from">Start date (UTC, inclusive). Required.</param>
    /// <param name="to">End date (UTC, inclusive). Required. Max range is 731 days.</param>
    /// <response code="400">Missing or invalid date range.</response>
    [HttpGet("muscle-groups")]
    [ProducesResponseType(typeof(MuscleGroupReportResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> GetMuscleGroupStats(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.GetMuscleGroupsAsync(userId, from, to, cancellationToken);
        return ToApiResult(result);
    }
}
