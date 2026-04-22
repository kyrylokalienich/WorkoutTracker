namespace WorkoutTracker.Domain.Entities;

public class WorkoutSessionExercise : BaseEntity
{
    public int WorkoutSessionId { get; set; }
    public int ExerciseId { get; set; }
    public int PlannedSets { get; set; }
    public int PlannedReps { get; set; }
    public decimal? PlannedWeightKg { get; set; }
    public int? ActualSets { get; set; }
    public int? ActualReps { get; set; }
    public decimal? ActualWeightKg { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public WorkoutSession? WorkoutSession { get; set; }
    public Exercise? Exercise { get; set; }
}
