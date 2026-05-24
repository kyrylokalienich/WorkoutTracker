using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.UnitOfWork;
using WorkoutTracker.Application.Models.Response.Reports;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Services;

public class ReportService : IReportService
{
    /// <summary>Maximum inclusive calendar span for report queries (reduces unbounded scans).</summary>
    internal const int MaxReportRangeDays = 731;

    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<WorkoutHistoryReportResponse>> GetWorkoutHistoryAsync(
        int userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateDateRange(fromUtc, toUtc);
        if (validation.Error is { } err)
        {
            return ServiceResult<WorkoutHistoryReportResponse>.Fail(err.Code, err.Details);
        }

        var (from, to) = validation.Range!.Value;

        var items = await _unitOfWork.Repository<WorkoutSession>().AsQueryable()
            .AsNoTracking()
            .Where(s =>
                s.UserId == userId
                && s.Status == WorkoutStatus.Completed
                && s.CompletedAtUtc != null
                && s.CompletedAtUtc >= from
                && s.CompletedAtUtc <= to)
            .OrderByDescending(s => s.CompletedAtUtc)
            .ThenByDescending(s => s.Id)
            .Select(s => new WorkoutHistoryItemResponse
            {
                Id = s.Id,
                Title = s.Title,
                CompletedAtUtc = s.CompletedAtUtc!.Value,
                ExerciseCount = s.WorkoutSessionExercises.Count,
                TotalVolumeKg = s.WorkoutSessionExercises.Sum(wse =>
                    (decimal)(wse.ActualSets ?? 0) * (wse.ActualReps ?? 0) * (wse.ActualWeightKg ?? 0m))
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<WorkoutHistoryReportResponse>.Ok(new WorkoutHistoryReportResponse
        {
            Items = items
        });
    }

    public async Task<ServiceResult<ProgressReportResponse>> GetProgressAsync(
        int userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateDateRange(fromUtc, toUtc);
        if (validation.Error is { } err)
        {
            return ServiceResult<ProgressReportResponse>.Fail(err.Code, err.Details);
        }

        var (from, to) = validation.Range!.Value;

        var sessions = _unitOfWork.Repository<WorkoutSession>().AsQueryable().AsNoTracking();

        var completedQuery = sessions.Where(s =>
            s.UserId == userId
            && s.Status == WorkoutStatus.Completed
            && s.CompletedAtUtc != null
            && s.CompletedAtUtc >= from
            && s.CompletedAtUtc <= to);

        var completedCount = await completedQuery.CountAsync(cancellationToken);

        var totalVolume = await completedQuery
            .SelectMany(s => s.WorkoutSessionExercises)
            .SumAsync(
                wse => (decimal)(wse.ActualSets ?? 0) * (wse.ActualReps ?? 0) * (wse.ActualWeightKg ?? 0m),
                cancellationToken);

        double? avgVolume = completedCount > 0
            ? (double)(totalVolume / completedCount)
            : null;

        var terminalScheduled = sessions.Where(s =>
            s.UserId == userId
            && s.ScheduledAtUtc >= from
            && s.ScheduledAtUtc <= to
            && (s.Status == WorkoutStatus.Completed || s.Status == WorkoutStatus.Skipped));

        var scheduledCompleted = await terminalScheduled.CountAsync(
            s => s.Status == WorkoutStatus.Completed,
            cancellationToken);

        var scheduledSkipped = await terminalScheduled.CountAsync(
            s => s.Status == WorkoutStatus.Skipped,
            cancellationToken);

        var terminalCount = scheduledCompleted + scheduledSkipped;
        double? completionRate = terminalCount > 0
            ? (double)scheduledCompleted / terminalCount
            : null;

        return ServiceResult<ProgressReportResponse>.Ok(new ProgressReportResponse
        {
            CompletedWorkoutCount = completedCount,
            TotalVolumeKg = totalVolume,
            AverageVolumeKgPerWorkout = avgVolume,
            ScheduledCompletedCount = scheduledCompleted,
            ScheduledSkippedCount = scheduledSkipped,
            CompletionRate = completionRate
        });
    }

    public async Task<ServiceResult<MuscleGroupReportResponse>> GetMuscleGroupsAsync(
        int userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateDateRange(fromUtc, toUtc);
        if (validation.Error is { } err)
        {
            return ServiceResult<MuscleGroupReportResponse>.Fail(err.Code, err.Details);
        }

        var (from, to) = validation.Range!.Value;

        var lineVolumes = _unitOfWork.Repository<WorkoutSessionExercise>().AsQueryable()
            .AsNoTracking()
            .Where(wse =>
                wse.WorkoutSession!.UserId == userId
                && wse.WorkoutSession.Status == WorkoutStatus.Completed
                && wse.WorkoutSession.CompletedAtUtc != null
                && wse.WorkoutSession.CompletedAtUtc >= from
                && wse.WorkoutSession.CompletedAtUtc <= to);

        var grouped = await lineVolumes
            .GroupBy(wse => wse.Exercise!.MuscleGroup)
            .Select(g => new MuscleGroupReportItemResponse
            {
                MuscleGroup = g.Key,
                TotalVolumeKg = g.Sum(wse =>
                    (decimal)(wse.ActualSets ?? 0) * (wse.ActualReps ?? 0) * (wse.ActualWeightKg ?? 0m)),
                SessionExerciseLineCount = g.Count()
            })
            .OrderBy(x => x.MuscleGroup)
            .ToListAsync(cancellationToken);

        return ServiceResult<MuscleGroupReportResponse>.Ok(new MuscleGroupReportResponse
        {
            Items = grouped
        });
    }

    private static (ValidationError? Error, (DateTime From, DateTime To)? Range) ValidateDateRange(
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (!fromUtc.HasValue || !toUtc.HasValue)
        {
            return (
                new ValidationError(
                    "validation_failed",
                    new { from = new[] { "Both from and to (UTC) are required." }, to = new[] { "Both from and to (UTC) are required." } }),
                null);
        }

        var from = fromUtc.Value;
        var to = toUtc.Value;

        if (from > to)
        {
            return (
                new ValidationError(
                    "validation_failed",
                    new { from = new[] { "from must be less than or equal to to." }, to = new[] { "to must be greater than or equal to from." } }),
                null);
        }

        if ((to - from).TotalDays > MaxReportRangeDays)
        {
            return (
                new ValidationError(
                    "validation_failed",
                    new { range = new[] { $"Date range must not exceed {MaxReportRangeDays} days." } }),
                null);
        }

        return (null, (DateTime.SpecifyKind(from, DateTimeKind.Utc), DateTime.SpecifyKind(to, DateTimeKind.Utc)));
    }

    private sealed record ValidationError(string Code, object Details);
}
