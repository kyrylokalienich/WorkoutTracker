using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Models.Request.WorkoutPlans;
using WorkoutTracker.Application.Models.Response.WorkoutPlans;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IWorkoutPlanService
{
    Task<ServiceResult<WorkoutPlanDetailResponse>> CreateAsync(
        int userId,
        CreateWorkoutPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<WorkoutPlanListItemResponse>>> ListAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutPlanDetailResponse>> GetByIdAsync(
        int userId,
        int planId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutPlanDetailResponse>> UpdateAsync(
        int userId,
        int planId,
        UpdateWorkoutPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> DeleteAsync(
        int userId,
        int planId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutPlanDetailResponse>> AddExerciseAsync(
        int userId,
        int planId,
        AddPlanExerciseRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutPlanDetailResponse>> UpdatePlanExerciseAsync(
        int userId,
        int planId,
        int planExerciseId,
        UpdatePlanExerciseRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutPlanDetailResponse>> RemovePlanExerciseAsync(
        int userId,
        int planId,
        int planExerciseId,
        CancellationToken cancellationToken = default);
}
