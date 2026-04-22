using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Domain.Entities;

public class WorkoutSession : BaseEntity
{
    public int UserId { get; set; }
    public int? WorkoutPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public WorkoutStatus Status { get; set; } = WorkoutStatus.Planned;
    public string? Comments { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public WorkoutPlan? WorkoutPlan { get; set; }
    public ICollection<WorkoutSessionExercise> WorkoutSessionExercises { get; set; } = new List<WorkoutSessionExercise>();
}
