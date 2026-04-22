using Microsoft.AspNetCore.Mvc;

namespace WorkoutTracker.API.Controllers;

/// <summary>
/// API endpoints for managing exercises (read-only for now).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExercisesController : BaseController
{
    /// <summary>
    /// Get all exercises with optional filters.
    /// </summary>
    [HttpGet]
    public IActionResult GetExercises([FromQuery] string? category, [FromQuery] string? muscleGroup, [FromQuery] string? search)
    {
        return Ok(new { message = "Exercises endpoint (Phase 3)" });
    }

    /// <summary>
    /// Get a specific exercise by ID.
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetExerciseById(int id)
    {
        return Ok(new { message = $"Exercise {id} (Phase 3)" });
    }
}
