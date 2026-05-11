using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Models.Request.Exercises;

public sealed class ExerciseQuery
{
    public ExerciseCategory? Category { get; init; }
    public MuscleGroup? MuscleGroup { get; init; }
    public string? Search { get; init; }
}

