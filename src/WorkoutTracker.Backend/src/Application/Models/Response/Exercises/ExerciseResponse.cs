using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Models.Response.Exercises;

public sealed class ExerciseResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ExerciseCategory Category { get; init; }
    public MuscleGroup MuscleGroup { get; init; }
}

