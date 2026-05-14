using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Response.Reports;

namespace WorkoutTracker.API.Controllers;

/// <summary>
/// API endpoints for viewing workout reports and progress.
/// </summary>
[Route("api/reports")]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Get completed workout history for the current user in the given UTC range (inclusive).
    /// </summary>
    [HttpGet("workout-history")]
    public async Task<IActionResult> GetWorkoutHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.GetWorkoutHistoryAsync(userId, from, to, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get progress metrics for the current user in the given UTC range (inclusive).
    /// </summary>
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.GetProgressAsync(userId, from, to, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get muscle-group volume breakdown for the current user in the given UTC range (inclusive).
    /// </summary>
    [HttpGet("muscle-groups")]
    public async Task<IActionResult> GetMuscleGroupStats(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.GetMuscleGroupsAsync(userId, from, to, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return result.FailureCode switch
        {
            "validation_failed" => BadRequest(new
            {
                code = result.FailureCode,
                message = "One or more fields are invalid.",
                details = result.FailureDetails
            }),
            _ => BadRequest(new { code = result.FailureCode, message = "Request failed.", details = result.FailureDetails })
        };
    }
}
