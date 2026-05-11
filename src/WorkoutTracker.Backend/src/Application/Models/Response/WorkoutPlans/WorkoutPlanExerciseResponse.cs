namespace WorkoutTracker.Application.Models.Response.WorkoutPlans;

public class WorkoutPlanExerciseResponse
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int TargetSets { get; set; }
    public int TargetReps { get; set; }
    public decimal? TargetWeightKg { get; set; }
    public int OrderIndex { get; set; }
}
