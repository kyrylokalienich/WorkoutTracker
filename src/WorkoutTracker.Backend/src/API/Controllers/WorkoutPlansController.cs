using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.WorkoutPlans;

namespace WorkoutTracker.API.Controllers;

/// <summary>
/// API endpoints for workout plans (CRUD and nested plan exercises).
/// </summary>
[Route("api/workout-plans")]
public class WorkoutPlansController : BaseController
{
    private readonly IWorkoutPlanService _workoutPlanService;

    public WorkoutPlansController(IWorkoutPlanService workoutPlanService)
    {
        _workoutPlanService = workoutPlanService;
    }

    /// <summary>
    /// Create a new workout plan.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorkoutPlan(
        [FromBody] CreateWorkoutPlanRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.CreateAsync(userId, request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get all workout plans for the current user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkoutPlans(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.ListAsync(userId, cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a specific workout plan by ID (includes exercises).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWorkoutPlanById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.GetByIdAsync(userId, id, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update a workout plan.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWorkoutPlan(
        int id,
        [FromBody] UpdateWorkoutPlanRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.UpdateAsync(userId, id, request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete a workout plan.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteWorkoutPlan(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.DeleteAsync(userId, id, cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound(new { code = result.FailureCode, message = "Workout plan not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Add an exercise to a workout plan.
    /// </summary>
    [HttpPost("{id:int}/exercises")]
    public async Task<IActionResult> AddExerciseToWorkoutPlan(
        int id,
        [FromBody] AddPlanExerciseRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.AddExerciseAsync(userId, id, request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update an exercise in a workout plan.
    /// </summary>
    [HttpPut("{id:int}/exercises/{planExerciseId:int}")]
    public async Task<IActionResult> UpdateWorkoutPlanExercise(
        int id,
        int planExerciseId,
        [FromBody] UpdatePlanExerciseRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.UpdatePlanExerciseAsync(
            userId,
            id,
            planExerciseId,
            request,
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Remove an exercise from a workout plan.
    /// </summary>
    [HttpDelete("{id:int}/exercises/{planExerciseId:int}")]
    public async Task<IActionResult> RemoveExerciseFromWorkoutPlan(
        int id,
        int planExerciseId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.RemovePlanExerciseAsync(
            userId,
            id,
            planExerciseId,
            cancellationToken);
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
            "not_found" => NotFound(new { code = "not_found", message = "Not found." }),
            "duplicate_exercise" => BadRequest(new
            {
                code = result.FailureCode,
                message = "This exercise is already in the plan.",
                details = result.FailureDetails
            }),
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
