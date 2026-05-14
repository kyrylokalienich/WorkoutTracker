namespace WorkoutTracker.Application.Models.Response.Reports;

/// <summary>
/// Progress metrics. Volume aggregates use completed sessions only (actual sets × reps × weight per line).
/// Completion rate uses sessions scheduled in the range that reached a terminal outcome (completed vs skipped).
/// </summary>
public class ProgressReportResponse
{
    public int CompletedWorkoutCount { get; set; }
    public decimal TotalVolumeKg { get; set; }
    public double? AverageVolumeKgPerWorkout { get; set; }

    public int ScheduledCompletedCount { get; set; }
    public int ScheduledSkippedCount { get; set; }
    public double? CompletionRate { get; set; }
}
