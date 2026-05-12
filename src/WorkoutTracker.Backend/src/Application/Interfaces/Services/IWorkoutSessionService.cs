using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Models.Request.WorkoutSessions;
using WorkoutTracker.Application.Models.Response.WorkoutSessions;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IWorkoutSessionService
{
    Task<ServiceResult<WorkoutSessionDetailResponse>> ScheduleAsync(
        int userId,
        ScheduleWorkoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PagedWorkoutSessionsResponse>> ListAsync(
        int userId,
        WorkoutStatus? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutSessionDetailResponse>> GetByIdAsync(
        int userId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutSessionDetailResponse>> UpdateAsync(
        int userId,
        int sessionId,
        UpdateWorkoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutSessionDetailResponse>> CompleteAsync(
        int userId,
        int sessionId,
        CompleteWorkoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> DeleteAsync(
        int userId,
        int sessionId,
        CancellationToken cancellationToken = default);
}
