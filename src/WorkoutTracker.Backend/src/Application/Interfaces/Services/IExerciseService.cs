using WorkoutTracker.Application.Models.Request.Exercises;
using WorkoutTracker.Application.Models.Response.Exercises;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IExerciseService
{
    Task<IReadOnlyList<ExerciseListItemResponse>> GetExercisesAsync(ExerciseQuery query, CancellationToken cancellationToken = default);
    Task<ExerciseResponse?> GetExerciseByIdAsync(int id, CancellationToken cancellationToken = default);
}

