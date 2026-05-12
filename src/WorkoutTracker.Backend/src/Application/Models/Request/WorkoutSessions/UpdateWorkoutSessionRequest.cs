using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Models.Request.WorkoutSessions;

public class UpdateWorkoutSessionRequest
{
    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledAtUtc { get; set; }

    [MaxLength(2000)]
    public string? Comments { get; set; }

    /// <summary>When set, must be a legal transition from the session's current status.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WorkoutStatus? Status { get; set; }
}
