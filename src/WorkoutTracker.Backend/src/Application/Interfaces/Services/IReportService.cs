using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Models.Response.Reports;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IReportService
{
    Task<ServiceResult<WorkoutHistoryReportResponse>> GetWorkoutHistoryAsync(
        int userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ProgressReportResponse>> GetProgressAsync(
        int userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MuscleGroupReportResponse>> GetMuscleGroupsAsync(
        int userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default);
}
