using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Models.Response.Exercises;

public sealed class ExerciseListItemResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ExerciseCategory Category { get; init; }
    public MuscleGroup MuscleGroup { get; init; }
}

