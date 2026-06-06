using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.Models.Request.WorkoutPlans;

/// <summary>Payload for creating a new workout plan.</summary>
public class CreateWorkoutPlanRequest
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
