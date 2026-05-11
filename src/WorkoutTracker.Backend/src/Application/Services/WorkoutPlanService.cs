using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.UnitOfWork;
using WorkoutTracker.Application.Models.Request.WorkoutPlans;
using WorkoutTracker.Application.Models.Response.WorkoutPlans;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Services;

public class WorkoutPlanService : IWorkoutPlanService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutPlanService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<WorkoutPlanDetailResponse>> CreateAsync(
        int userId,
        CreateWorkoutPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var plan = new WorkoutPlan
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = request.IsActive,
            CreatedAtUtc = now
        };

        await _unitOfWork.Repository<WorkoutPlan>().AddAsync(plan);
        await _unitOfWork.SaveChangesAsync();

        var detail = await BuildDetailAsync(userId, plan.Id, cancellationToken);
        return ServiceResult<WorkoutPlanDetailResponse>.Ok(detail!);
    }

    public async Task<ServiceResult<IReadOnlyList<WorkoutPlanListItemResponse>>> ListAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<WorkoutPlan>().AsQueryable()
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .Select(p => new WorkoutPlanListItemResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc,
                ExerciseCount = p.WorkoutPlanExercises.Count
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<WorkoutPlanListItemResponse>>.Ok(items);
    }

    public async Task<ServiceResult<WorkoutPlanDetailResponse>> GetByIdAsync(
        int userId,
        int planId,
        CancellationToken cancellationToken = default)
    {
        var detail = await BuildDetailAsync(userId, planId, cancellationToken);
        if (detail == null)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail("not_found");
        }

        return ServiceResult<WorkoutPlanDetailResponse>.Ok(detail);
    }

    public async Task<ServiceResult<WorkoutPlanDetailResponse>> UpdateAsync(
        int userId,
        int planId,
        UpdateWorkoutPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.Repository<WorkoutPlan>().AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        if (plan == null)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail("not_found");
        }

        plan.Name = request.Name.Trim();
        plan.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        plan.IsActive = request.IsActive;
        plan.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        var detail = await BuildDetailAsync(userId, planId, cancellationToken);
        return ServiceResult<WorkoutPlanDetailResponse>.Ok(detail!);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
        int userId,
        int planId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.Repository<WorkoutPlan>().AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        if (plan == null)
        {
            return ServiceResult<bool>.Fail("not_found");
        }

        await _unitOfWork.Repository<WorkoutPlan>().DeleteAsync(plan);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<WorkoutPlanDetailResponse>> AddExerciseAsync(
        int userId,
        int planId,
        AddPlanExerciseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TargetWeightKg.HasValue && request.TargetWeightKg.Value < 0)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail(
                "validation_failed",
                new { targetWeightKg = new[] { "Weight must be non-negative when provided." } });
        }

        var plan = await _unitOfWork.Repository<WorkoutPlan>().AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        if (plan == null)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail("not_found");
        }

        if (!await ExerciseExistsAsync(request.ExerciseId, cancellationToken))
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail(
                "validation_failed",
                new { exerciseId = new[] { "Exercise does not exist." } });
        }

        if (await HasDuplicateExerciseAsync(planId, request.ExerciseId, excludePlanExerciseId: null, cancellationToken))
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail(
                "duplicate_exercise",
                new { message = "This exercise is already in the plan." });
        }

        var line = new WorkoutPlanExercise
        {
            WorkoutPlanId = planId,
            ExerciseId = request.ExerciseId,
            TargetSets = request.TargetSets,
            TargetReps = request.TargetReps,
            TargetWeightKg = request.TargetWeightKg,
            OrderIndex = request.OrderIndex
        };

        await _unitOfWork.Repository<WorkoutPlanExercise>().AddAsync(line);
        try
        {
            plan.UpdatedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail(
                "duplicate_exercise",
                new { message = "This exercise is already in the plan." });
        }

        var detail = await BuildDetailAsync(userId, planId, cancellationToken);
        return ServiceResult<WorkoutPlanDetailResponse>.Ok(detail!);
    }

    public async Task<ServiceResult<WorkoutPlanDetailResponse>> UpdatePlanExerciseAsync(
        int userId,
        int planId,
        int planExerciseId,
        UpdatePlanExerciseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TargetWeightKg.HasValue && request.TargetWeightKg.Value < 0)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail(
                "validation_failed",
                new { targetWeightKg = new[] { "Weight must be non-negative when provided." } });
        }

        var plan = await _unitOfWork.Repository<WorkoutPlan>().AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        if (plan == null)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail("not_found");
        }

        var line = await _unitOfWork.Repository<WorkoutPlanExercise>().AsQueryable()
            .FirstOrDefaultAsync(
                x => x.Id == planExerciseId && x.WorkoutPlanId == planId,
                cancellationToken);

        if (line == null)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail("not_found");
        }

        if (!await ExerciseExistsAsync(request.ExerciseId, cancellationToken))
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail(
                "validation_failed",
                new { exerciseId = new[] { "Exercise does not exist." } });
        }

        if (await HasDuplicateExerciseAsync(planId, request.ExerciseId, planExerciseId, cancellationToken))
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail(
                "duplicate_exercise",
                new { message = "This exercise is already in the plan." });
        }

        line.ExerciseId = request.ExerciseId;
        line.TargetSets = request.TargetSets;
        line.TargetReps = request.TargetReps;
        line.TargetWeightKg = request.TargetWeightKg;
        line.OrderIndex = request.OrderIndex;
        plan.UpdatedAtUtc = DateTime.UtcNow;

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail(
                "duplicate_exercise",
                new { message = "This exercise is already in the plan." });
        }

        var detail = await BuildDetailAsync(userId, planId, cancellationToken);
        return ServiceResult<WorkoutPlanDetailResponse>.Ok(detail!);
    }

    public async Task<ServiceResult<WorkoutPlanDetailResponse>> RemovePlanExerciseAsync(
        int userId,
        int planId,
        int planExerciseId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.Repository<WorkoutPlan>().AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        if (plan == null)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail("not_found");
        }

        var line = await _unitOfWork.Repository<WorkoutPlanExercise>().AsQueryable()
            .FirstOrDefaultAsync(
                x => x.Id == planExerciseId && x.WorkoutPlanId == planId,
                cancellationToken);

        if (line == null)
        {
            return ServiceResult<WorkoutPlanDetailResponse>.Fail("not_found");
        }

        await _unitOfWork.Repository<WorkoutPlanExercise>().DeleteAsync(line);
        plan.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var detail = await BuildDetailAsync(userId, planId, cancellationToken);
        return ServiceResult<WorkoutPlanDetailResponse>.Ok(detail!);
    }

    private static WorkoutPlanDetailResponse MapPlan(WorkoutPlan plan)
    {
        var exercises = plan.WorkoutPlanExercises
            .OrderBy(x => x.OrderIndex)
            .ThenBy(x => x.Id)
            .Select(x => new WorkoutPlanExerciseResponse
            {
                Id = x.Id,
                ExerciseId = x.ExerciseId,
                ExerciseName = x.Exercise?.Name ?? string.Empty,
                TargetSets = x.TargetSets,
                TargetReps = x.TargetReps,
                TargetWeightKg = x.TargetWeightKg,
                OrderIndex = x.OrderIndex
            })
            .ToList();

        return new WorkoutPlanDetailResponse
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            IsActive = plan.IsActive,
            CreatedAtUtc = plan.CreatedAtUtc,
            UpdatedAtUtc = plan.UpdatedAtUtc,
            Exercises = exercises
        };
    }

    private async Task<WorkoutPlanDetailResponse?> BuildDetailAsync(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.Repository<WorkoutPlan>().AsQueryable()
            .AsNoTracking()
            .Include(p => p.WorkoutPlanExercises)
            .ThenInclude(x => x.Exercise)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        return plan == null ? null : MapPlan(plan);
    }

    private Task<bool> ExerciseExistsAsync(int exerciseId, CancellationToken cancellationToken)
    {
        return _unitOfWork.Repository<Exercise>().AsQueryable()
            .AnyAsync(e => e.Id == exerciseId, cancellationToken);
    }

    private Task<bool> HasDuplicateExerciseAsync(
        int workoutPlanId,
        int exerciseId,
        int? excludePlanExerciseId,
        CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<WorkoutPlanExercise>().AsQueryable()
            .Where(x => x.WorkoutPlanId == workoutPlanId && x.ExerciseId == exerciseId);

        if (excludePlanExerciseId.HasValue)
        {
            query = query.Where(x => x.Id != excludePlanExerciseId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }
}
