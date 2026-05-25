using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.UnitOfWork;
using WorkoutTracker.Application.Models.Request.WorkoutSessions;
using WorkoutTracker.Application.Models.Response.WorkoutSessions;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Services;

public class WorkoutSessionService : IWorkoutSessionService
{
    private const int MaxPageSize = 100;

    private readonly IUnitOfWork _unitOfWork;

    public WorkoutSessionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<WorkoutSessionDetailResponse>> ScheduleAsync(
        int userId,
        ScheduleWorkoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var title = request.Title.Trim();

        var session = new WorkoutSession
        {
            UserId = userId,
            WorkoutPlanId = request.WorkoutPlanId,
            Title = title,
            ScheduledAtUtc = request.ScheduledAtUtc,
            Status = WorkoutStatus.Planned,
            CreatedAtUtc = now
        };

        if (request.WorkoutPlanId is int planId)
        {
            var plan = await _unitOfWork.Repository<WorkoutPlan>().AsQueryable()
                .Include(p => p.WorkoutPlanExercises)
                .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

            if (plan == null)
            {
                return ServiceResult<WorkoutSessionDetailResponse>.Fail("not_found");
            }

            foreach (var line in plan.WorkoutPlanExercises
                         .OrderBy(x => x.OrderIndex)
                         .ThenBy(x => x.Id))
            {
                session.WorkoutSessionExercises.Add(new WorkoutSessionExercise
                {
                    ExerciseId = line.ExerciseId,
                    PlannedSets = line.TargetSets,
                    PlannedReps = line.TargetReps,
                    PlannedWeightKg = line.TargetWeightKg
                });
            }
        }

        await _unitOfWork.Repository<WorkoutSession>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        var detail = await BuildDetailAsync(userId, session.Id, cancellationToken);
        return ServiceResult<WorkoutSessionDetailResponse>.Ok(detail!);
    }

    public async Task<ServiceResult<PagedWorkoutSessionsResponse>> ListAsync(
        int userId,
        WorkoutStatus? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _unitOfWork.Repository<WorkoutSession>().AsQueryable()
            .AsNoTracking()
            .Where(s => s.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(s => s.ScheduledAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(s => s.ScheduledAtUtc <= toUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.ScheduledAtUtc)
            .ThenBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new WorkoutSessionListItemResponse
            {
                Id = s.Id,
                WorkoutPlanId = s.WorkoutPlanId,
                Title = s.Title,
                ScheduledAtUtc = s.ScheduledAtUtc,
                StartedAtUtc = s.StartedAtUtc,
                CompletedAtUtc = s.CompletedAtUtc,
                Status = s.Status,
                ExerciseCount = s.WorkoutSessionExercises.Count
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedWorkoutSessionsResponse>.Ok(new PagedWorkoutSessionsResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ServiceResult<WorkoutSessionDetailResponse>> GetByIdAsync(
        int userId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var detail = await BuildDetailAsync(userId, sessionId, cancellationToken);
        if (detail == null)
        {
            return ServiceResult<WorkoutSessionDetailResponse>.Fail("not_found");
        }

        return ServiceResult<WorkoutSessionDetailResponse>.Ok(detail);
    }

    public async Task<ServiceResult<WorkoutSessionDetailResponse>> UpdateAsync(
        int userId,
        int sessionId,
        UpdateWorkoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Repository<WorkoutSession>().AsQueryable()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null)
        {
            return ServiceResult<WorkoutSessionDetailResponse>.Fail("not_found");
        }

        if (session.Status is WorkoutStatus.Completed or WorkoutStatus.Skipped)
        {
            return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                "invalid_state",
                new { message = "Completed or skipped sessions cannot be updated." });
        }

        session.Title = request.Title.Trim();
        session.ScheduledAtUtc = request.ScheduledAtUtc;
        session.Comments = string.IsNullOrWhiteSpace(request.Comments) ? null : request.Comments.Trim();
        session.UpdatedAtUtc = DateTime.UtcNow;

        if (request.Status.HasValue)
        {
            if (request.Status.Value == WorkoutStatus.Completed)
            {
                return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                    "invalid_transition",
                    new { message = "Use POST /api/workout-sessions/{id}/complete to mark a session completed." });
            }

            var (ok, failureCode, failureDetails) = TryApplyStatusTransition(session, request.Status.Value);
            if (!ok)
            {
                return ServiceResult<WorkoutSessionDetailResponse>.Fail(failureCode!, failureDetails);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        var detail = await BuildDetailAsync(userId, sessionId, cancellationToken);
        return ServiceResult<WorkoutSessionDetailResponse>.Ok(detail!);
    }

    public async Task<ServiceResult<WorkoutSessionDetailResponse>> CompleteAsync(
        int userId,
        int sessionId,
        CompleteWorkoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Repository<WorkoutSession>().AsQueryable()
            .Include(s => s.WorkoutSessionExercises)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null)
        {
            return ServiceResult<WorkoutSessionDetailResponse>.Fail("not_found");
        }

        if (session.Status is WorkoutStatus.Completed or WorkoutStatus.Skipped)
        {
            return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                "invalid_state",
                new { message = "Session is already completed or skipped." });
        }

        if (session.Status != WorkoutStatus.Planned && session.Status != WorkoutStatus.InProgress)
        {
            return ServiceResult<WorkoutSessionDetailResponse>.Fail("invalid_state");
        }

        var lines = session.WorkoutSessionExercises.ToList();
        var payloads = request.Exercises ?? new List<CompleteSessionExerciseRequest>();
        if (lines.Count != payloads.Count)
        {
            return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                "validation_failed",
                new { exercises = new[] { "Payload must include exactly one entry per session exercise." } });
        }

        var distinctIds = payloads.Select(e => e.SessionExerciseId).Distinct().ToList();
        if (distinctIds.Count != payloads.Count)
        {
            return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                "validation_failed",
                new { exercises = new[] { "Duplicate session exercise ids in payload." } });
        }

        var lineById = lines.ToDictionary(l => l.Id);
        foreach (var payload in payloads)
        {
            if (!lineById.TryGetValue(payload.SessionExerciseId, out var line))
            {
                return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                    "validation_failed",
                    new { exercises = new[] { $"Unknown session exercise id: {payload.SessionExerciseId}." } });
            }

            if (payload.ActualWeightKg.HasValue && payload.ActualWeightKg.Value < 0)
            {
                return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                    "validation_failed",
                    new { actualWeightKg = new[] { "Weight must be non-negative when provided." } });
            }

            line.ActualSets = payload.ActualSets;
            line.ActualReps = payload.ActualReps;
            line.ActualWeightKg = payload.ActualWeightKg;
            line.Notes = string.IsNullOrWhiteSpace(payload.Notes) ? null : payload.Notes.Trim();
        }

        var now = DateTime.UtcNow;
        session.StartedAtUtc ??= now;
        session.CompletedAtUtc = now;
        session.Status = WorkoutStatus.Completed;
        session.UpdatedAtUtc = now;
        if (request.Comments != null)
        {
            session.Comments = string.IsNullOrWhiteSpace(request.Comments) ? null : request.Comments.Trim();
        }

        await _unitOfWork.SaveChangesAsync();

        var detail = await BuildDetailAsync(userId, sessionId, cancellationToken);
        return ServiceResult<WorkoutSessionDetailResponse>.Ok(detail!);
    }

    public async Task<ServiceResult<WorkoutSessionDetailResponse>> AddExerciseAsync(
        int userId,
        int sessionId,
        AddSessionExerciseRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Repository<WorkoutSession>().AsQueryable()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null)
            return ServiceResult<WorkoutSessionDetailResponse>.Fail("not_found");

        if (session.Status != WorkoutStatus.InProgress)
            return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                "invalid_state",
                new { message = "Exercises can only be added to in-progress sessions." });

        var exercise = await _unitOfWork.Repository<Exercise>().AsQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise == null)
            return ServiceResult<WorkoutSessionDetailResponse>.Fail(
                "validation_failed",
                new { exerciseId = new[] { "Exercise not found." } });

        await _unitOfWork.Repository<WorkoutSessionExercise>().AddAsync(new WorkoutSessionExercise
        {
            WorkoutSessionId = session.Id,
            ExerciseId = request.ExerciseId,
            PlannedSets = request.PlannedSets,
            PlannedReps = request.PlannedReps,
            PlannedWeightKg = request.PlannedWeightKg
        });

        await _unitOfWork.SaveChangesAsync();

        var detail = await BuildDetailAsync(userId, sessionId, cancellationToken);
        return ServiceResult<WorkoutSessionDetailResponse>.Ok(detail!);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
        int userId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Repository<WorkoutSession>().AsQueryable()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null)
        {
            return ServiceResult<bool>.Fail("not_found");
        }

        await _unitOfWork.Repository<WorkoutSession>().DeleteAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    private static (bool Ok, string? FailureCode, object? FailureDetails) TryApplyStatusTransition(
        WorkoutSession session,
        WorkoutStatus next)
    {
        if (next == session.Status)
        {
            return (true, null, null);
        }

        switch (session.Status)
        {
            case WorkoutStatus.Planned when next == WorkoutStatus.InProgress:
                session.Status = WorkoutStatus.InProgress;
                session.StartedAtUtc = DateTime.UtcNow;
                return (true, null, null);
            case WorkoutStatus.Planned when next == WorkoutStatus.Skipped:
                session.Status = WorkoutStatus.Skipped;
                return (true, null, null);
            default:
                return (
                    false,
                    "invalid_transition",
                    new { message = $"Cannot change status from {session.Status} to {next}." });
        }
    }

    private static WorkoutSessionExerciseResponse MapExercise(WorkoutSessionExercise x)
    {
        return new WorkoutSessionExerciseResponse
        {
            Id = x.Id,
            ExerciseId = x.ExerciseId,
            ExerciseName = x.Exercise?.Name ?? string.Empty,
            PlannedSets = x.PlannedSets,
            PlannedReps = x.PlannedReps,
            PlannedWeightKg = x.PlannedWeightKg,
            ActualSets = x.ActualSets,
            ActualReps = x.ActualReps,
            ActualWeightKg = x.ActualWeightKg,
            Notes = x.Notes
        };
    }

    private async Task<WorkoutSessionDetailResponse?> BuildDetailAsync(
        int userId,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<WorkoutSession>().AsQueryable()
            .AsNoTracking()
            .Include(s => s.WorkoutSessionExercises)
            .ThenInclude(x => x.Exercise)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null)
        {
            return null;
        }

        var exercises = session.WorkoutSessionExercises
            .OrderBy(x => x.Id)
            .Select(MapExercise)
            .ToList();

        return new WorkoutSessionDetailResponse
        {
            Id = session.Id,
            WorkoutPlanId = session.WorkoutPlanId,
            Title = session.Title,
            ScheduledAtUtc = session.ScheduledAtUtc,
            StartedAtUtc = session.StartedAtUtc,
            CompletedAtUtc = session.CompletedAtUtc,
            Status = session.Status,
            Comments = session.Comments,
            CreatedAtUtc = session.CreatedAtUtc,
            UpdatedAtUtc = session.UpdatedAtUtc,
            Exercises = exercises
        };
    }
}
