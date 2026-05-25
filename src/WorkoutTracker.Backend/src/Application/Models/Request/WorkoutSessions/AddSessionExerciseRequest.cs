using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.Models.Request.WorkoutSessions;

public class AddSessionExerciseRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ExerciseId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int PlannedSets { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int PlannedReps { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PlannedWeightKg { get; set; }
}
