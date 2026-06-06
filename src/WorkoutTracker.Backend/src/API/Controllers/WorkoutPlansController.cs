using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Infrastructure;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.WorkoutPlans;
using WorkoutTracker.Application.Models.Response.WorkoutPlans;

namespace WorkoutTracker.API.Controllers;

/// <summary>Manage workout plans and their exercises.</summary>
[Route("api/workout-plans")]
[Produces("application/json")]
public class WorkoutPlansController : BaseController
{
    private readonly IWorkoutPlanService _workoutPlanService;

    public WorkoutPlansController(IWorkoutPlanService workoutPlanService)
    {
        _workoutPlanService = workoutPlanService;
    }

    /// <summary>Create a workout plan.</summary>
    /// <response code="400">Validation error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkoutPlanDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> CreateWorkoutPlan(
        [FromBody] CreateWorkoutPlanRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.CreateAsync(userId, request, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>List your workout plans.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkoutPlanListItemResponse>), 200)]
    public async Task<IActionResult> GetWorkoutPlans(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.ListAsync(userId, cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Get a plan with its full exercise list.</summary>
    /// <response code="404">Plan not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkoutPlanDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> GetWorkoutPlanById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.GetByIdAsync(userId, id, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Update a plan.</summary>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Plan not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WorkoutPlanDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> UpdateWorkoutPlan(
        int id,
        [FromBody] UpdateWorkoutPlanRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.UpdateAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Delete a plan and all its exercises.</summary>
    /// <response code="404">Plan not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> DeleteWorkoutPlan(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.DeleteAsync(userId, id, cancellationToken);
        if (!result.Succeeded)
            return ToApiResult(result);

        return NoContent();
    }

    /// <summary>Add an exercise to a plan.</summary>
    /// <response code="400">Exercise not found, already in this plan, or validation error.</response>
    /// <response code="404">Plan not found.</response>
    [HttpPost("{id:int}/exercises")]
    [ProducesResponseType(typeof(WorkoutPlanDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> AddExerciseToWorkoutPlan(
        int id,
        [FromBody] AddPlanExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.AddExerciseAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Update sets, reps, weight, or order for an exercise in a plan.</summary>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Plan or exercise entry not found.</response>
    [HttpPut("{id:int}/exercises/{planExerciseId:int}")]
    [ProducesResponseType(typeof(WorkoutPlanDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> UpdateWorkoutPlanExercise(
        int id,
        int planExerciseId,
        [FromBody] UpdatePlanExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.UpdatePlanExerciseAsync(
            userId, id, planExerciseId, request, cancellationToken);
        return ToApiResult(result);
    }

    /// <summary>Remove an exercise from a plan.</summary>
    /// <response code="404">Plan or exercise entry not found.</response>
    [HttpDelete("{id:int}/exercises/{planExerciseId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> RemoveExerciseFromWorkoutPlan(
        int id,
        int planExerciseId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.RemovePlanExerciseAsync(
            userId, id, planExerciseId, cancellationToken);
        if (!result.Succeeded)
            return ToApiResult(result);

        return NoContent();
    }
}
