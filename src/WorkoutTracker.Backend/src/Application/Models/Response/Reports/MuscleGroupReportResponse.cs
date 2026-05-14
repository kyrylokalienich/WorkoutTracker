using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Models.Response.Reports;

public class MuscleGroupReportItemResponse
{
    public MuscleGroup MuscleGroup { get; set; }
    public decimal TotalVolumeKg { get; set; }
    public int SessionExerciseLineCount { get; set; }
}

public class MuscleGroupReportResponse
{
    public IReadOnlyList<MuscleGroupReportItemResponse> Items { get; set; } = Array.Empty<MuscleGroupReportItemResponse>();
}
