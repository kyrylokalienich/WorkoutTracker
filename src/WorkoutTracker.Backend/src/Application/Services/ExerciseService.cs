using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.UnitOfWork;
using WorkoutTracker.Application.Models.Request.Exercises;
using WorkoutTracker.Application.Models.Response.Exercises;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Services;

public class ExerciseService : IExerciseService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExerciseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ExerciseListItemResponse>> GetExercisesAsync(
        ExerciseQuery query,
        CancellationToken cancellationToken = default)
    {
        var exercises = await _unitOfWork.Repository<Exercise>().GetAllAsync();

        var filtered = exercises.AsEnumerable();

        if (query.Category.HasValue)
        {
            filtered = filtered.Where(e => e.Category == query.Category.Value);
        }

        if (query.MuscleGroup.HasValue)
        {
            filtered = filtered.Where(e => e.MuscleGroup == query.MuscleGroup.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            filtered = filtered.Where(e =>
                e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (e.Description != null && e.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        return filtered
            .OrderBy(e => e.Name)
            .Select(e => new ExerciseListItemResponse
            {
                Id = e.Id,
                Name = e.Name,
                Category = e.Category,
                MuscleGroup = e.MuscleGroup
            })
            .ToList();
    }

    public async Task<ExerciseResponse?> GetExerciseByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var exercise = await _unitOfWork.Repository<Exercise>().GetByIdAsync(id);
        if (exercise == null)
        {
            return null;
        }

        return new ExerciseResponse
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Category = exercise.Category,
            MuscleGroup = exercise.MuscleGroup
        };
    }
}

