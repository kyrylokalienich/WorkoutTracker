namespace WorkoutTracker.Application.Models.Response.WorkoutSessions;

public class WorkoutSessionExerciseResponse
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int PlannedSets { get; set; }
    public int PlannedReps { get; set; }
    public decimal? PlannedWeightKg { get; set; }
    public int? ActualSets { get; set; }
    public int? ActualReps { get; set; }
    public decimal? ActualWeightKg { get; set; }
    public string? Notes { get; set; }
}
