using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.Models.Request.WorkoutSessions;

/// <summary>Actual performance data to record when completing a session.</summary>
public class CompleteWorkoutSessionRequest
{
    [MaxLength(2000)]
    public string? Comments { get; set; }

    [Required]
    public List<CompleteSessionExerciseRequest> Exercises { get; set; } = new();
}

/// <summary>Actual sets, reps, and weight logged for a single session exercise.</summary>
public class CompleteSessionExerciseRequest
{
    [Required]
    public int SessionExerciseId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int ActualSets { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int ActualReps { get; set; }

    public decimal? ActualWeightKg { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
