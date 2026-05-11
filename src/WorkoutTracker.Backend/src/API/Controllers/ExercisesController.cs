using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.Exercises;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.API.Controllers;

/// <summary>
/// API endpoints for managing exercises (read-only for now).
/// </summary>
[Route("api/exercises")]
public class ExercisesController : BaseController
{
    private readonly IExerciseService _exerciseService;

    public ExercisesController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    /// <summary>
    /// Get all exercises with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetExercises(
        [FromQuery] string? category,
        [FromQuery] string? muscleGroup,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        if (!TryParseEnum<ExerciseCategory>(category, out var parsedCategory))
        {
            return BadRequest(new
            {
                code = "validation_failed",
                message = "One or more fields are invalid.",
                details = new { category = new[] { "Invalid category value." } }
            });
        }

        if (!TryParseEnum<MuscleGroup>(muscleGroup, out var parsedMuscleGroup))
        {
            return BadRequest(new
            {
                code = "validation_failed",
                message = "One or more fields are invalid.",
                details = new { muscleGroup = new[] { "Invalid muscleGroup value." } }
            });
        }

        var query = new ExerciseQuery
        {
            Category = parsedCategory,
            MuscleGroup = parsedMuscleGroup,
            Search = search
        };

        var exercises = await _exerciseService.GetExercisesAsync(query, cancellationToken);
        return Ok(exercises);
    }

    /// <summary>
    /// Get a specific exercise by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetExerciseById(int id, CancellationToken cancellationToken)
    {
        var exercise = await _exerciseService.GetExerciseByIdAsync(id, cancellationToken);
        if (exercise == null)
        {
            return NotFound(new
            {
                code = "not_found",
                message = "Exercise not found."
            });
        }

        return Ok(exercise);
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum? parsed) where TEnum : struct, Enum
    {
        parsed = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsedValue))
        {
            parsed = parsedValue;
            return true;
        }

        return false;
    }
}
