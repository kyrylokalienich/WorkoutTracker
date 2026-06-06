using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.Interfaces.Services;

namespace WorkoutTracker.API.Controllers;

[Route("api/reports")]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("workout-history")]
    public async Task<IActionResult> GetWorkoutHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.GetWorkoutHistoryAsync(userId, from, to, cancellationToken);
        return ToApiResult(result);
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.GetProgressAsync(userId, from, to, cancellationToken);
        return ToApiResult(result);
    }

    [HttpGet("muscle-groups")]
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
