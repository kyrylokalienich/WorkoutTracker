namespace WorkoutTracker.Domain.Entities;

public class WorkoutPlanExercise : BaseEntity
{
    public int WorkoutPlanId { get; set; }
    public int ExerciseId { get; set; }
    public int TargetSets { get; set; }
    public int TargetReps { get; set; }
    public decimal? TargetWeightKg { get; set; }
    public int OrderIndex { get; set; }

    // Navigation properties
    public WorkoutPlan? WorkoutPlan { get; set; }
    public Exercise? Exercise { get; set; }
}
