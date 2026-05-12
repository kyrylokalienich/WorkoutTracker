using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Models.Response.WorkoutSessions;

public class WorkoutSessionDetailResponse
{
    public int Id { get; set; }
    public int? WorkoutPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public WorkoutStatus Status { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public IReadOnlyList<WorkoutSessionExerciseResponse> Exercises { get; set; } = Array.Empty<WorkoutSessionExerciseResponse>();
}
