using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.Models.Request.WorkoutPlans;

public class UpdatePlanExerciseRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ExerciseId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int TargetSets { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int TargetReps { get; set; }

    public decimal? TargetWeightKg { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int OrderIndex { get; set; }
}
