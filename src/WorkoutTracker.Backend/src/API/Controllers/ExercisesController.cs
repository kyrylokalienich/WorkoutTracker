using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Infrastructure;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.Exercises;
using WorkoutTracker.Application.Models.Response.Exercises;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.API.Controllers;

/// <summary>Read-only access to the exercise catalog.</summary>
[Route("api/exercises")]
[Produces("application/json")]
public class ExercisesController : BaseController
{
    private readonly IExerciseService _exerciseService;

    public ExercisesController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    /// <summary>List exercises with optional filters.</summary>
    /// <param name="category">Filter by exercise category (e.g. Strength, Cardio, Flexibility).</param>
    /// <param name="muscleGroup">Filter by target muscle group (e.g. Chest, Back, Legs).</param>
    /// <param name="search">Case-insensitive substring match on exercise name.</param>
    /// <response code="200">Filtered list of exercises sorted by name.</response>
    /// <response code="400">Unknown category or muscleGroup value.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExerciseListItemResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> GetExercises(
        [FromQuery] string? category,
        [FromQuery] string? muscleGroup,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        if (!TryParseEnum<ExerciseCategory>(category, out var parsedCategory))
        {
            return BadRequest(new ApiErrorResponse(
                "validation_failed",
                "One or more fields are invalid.",
                new { category = new[] { "Invalid category value." } }));
        }

        if (!TryParseEnum<MuscleGroup>(muscleGroup, out var parsedMuscleGroup))
        {
            return BadRequest(new ApiErrorResponse(
                "validation_failed",
                "One or more fields are invalid.",
                new { muscleGroup = new[] { "Invalid muscleGroup value." } }));
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

    /// <summary>Get a single exercise by its ID.</summary>
    /// <param name="id">Exercise ID.</param>
    /// <response code="200">Exercise detail.</response>
    /// <response code="404">Exercise not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExerciseResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> GetExerciseById(int id, CancellationToken cancellationToken)
    {
        var exercise = await _exerciseService.GetExerciseByIdAsync(id, cancellationToken);
        if (exercise == null)
            return NotFound(new ApiErrorResponse("not_found", "Exercise not found."));

        return Ok(exercise);
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum? parsed) where TEnum : struct, Enum
    {
        parsed = null;

        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsedValue))
        {
            parsed = parsedValue;
            return true;
        }

        return false;
    }
}
