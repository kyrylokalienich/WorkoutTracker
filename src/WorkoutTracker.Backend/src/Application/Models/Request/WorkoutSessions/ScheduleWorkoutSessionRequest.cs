using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.Models.Request.WorkoutSessions;

/// <summary>Schedules a new workout session, optionally from a plan snapshot.</summary>
public class ScheduleWorkoutSessionRequest
{
    /// <summary>Optional plan to snapshot into session exercises; must belong to the current user.</summary>
    public int? WorkoutPlanId { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledAtUtc { get; set; }
}
