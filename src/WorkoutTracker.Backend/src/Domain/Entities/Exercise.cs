using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ExerciseCategory Category { get; set; }
    public MuscleGroup MuscleGroup { get; set; }

    // Navigation properties
    public ICollection<WorkoutPlanExercise> WorkoutPlanExercises { get; set; } = new List<WorkoutPlanExercise>();
    public ICollection<WorkoutSessionExercise> WorkoutSessionExercises { get; set; } = new List<WorkoutSessionExercise>();
}
