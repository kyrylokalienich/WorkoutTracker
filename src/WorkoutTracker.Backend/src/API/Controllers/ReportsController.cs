using Microsoft.AspNetCore.Mvc;

namespace WorkoutTracker.API.Controllers;

/// <summary>
/// API endpoints for viewing workout reports and progress.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportsController : BaseController
{
    /// <summary>
    /// Get workout history for the current user.
    /// </summary>
    [HttpGet("workout-history")]
    public IActionResult GetWorkoutHistory([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(new { message = "Workout history endpoint (Phase 6)" });
    }

    /// <summary>
    /// Get progress data for the current user.
    /// </summary>
    [HttpGet("progress")]
    public IActionResult GetProgress([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(new { message = "Progress endpoint (Phase 6)" });
    }

    /// <summary>
    /// Get muscle group aggregations for the current user.
    /// </summary>
    [HttpGet("muscle-groups")]
    public IActionResult GetMuscleGroupStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(new { message = "Muscle groups endpoint (Phase 6)" });
    }
}
