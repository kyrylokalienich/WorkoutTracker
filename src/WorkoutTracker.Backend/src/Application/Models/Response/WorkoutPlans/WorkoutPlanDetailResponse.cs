namespace WorkoutTracker.Application.Models.Response.WorkoutPlans;

public class WorkoutPlanDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public IReadOnlyList<WorkoutPlanExerciseResponse> Exercises { get; set; } = Array.Empty<WorkoutPlanExerciseResponse>();
}
