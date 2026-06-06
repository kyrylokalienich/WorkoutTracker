using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Models.Request.WorkoutPlans;

namespace WorkoutTracker.API.Controllers;

[Route("api/workout-plans")]
public class WorkoutPlansController : BaseController
{
    private readonly IWorkoutPlanService _workoutPlanService;

    public WorkoutPlansController(IWorkoutPlanService workoutPlanService)
    {
        _workoutPlanService = workoutPlanService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkoutPlan(
        [FromBody] CreateWorkoutPlanRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.CreateAsync(userId, request, cancellationToken);
        return ToApiResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkoutPlans(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.ListAsync(userId, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWorkoutPlanById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.GetByIdAsync(userId, id, cancellationToken);
        return ToApiResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWorkoutPlan(
        int id,
        [FromBody] UpdateWorkoutPlanRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.UpdateAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteWorkoutPlan(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.DeleteAsync(userId, id, cancellationToken);
        if (!result.Succeeded)
            return ToApiResult(result);

        return NoContent();
    }

    [HttpPost("{id:int}/exercises")]
    public async Task<IActionResult> AddExerciseToWorkoutPlan(
        int id,
        [FromBody] AddPlanExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _workoutPlanService.AddExerciseAsync(userId, id, request, cancellationToken);
        return ToApiResult(result);
    }

    [HttpPut("{id:int}/exercises/{planExerciseId:int}")]
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

    [HttpDelete("{id:int}/exercises/{planExerciseId:int}")]
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
