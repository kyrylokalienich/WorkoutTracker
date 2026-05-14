namespace WorkoutTracker.Application.Models.Response.Reports;

public class WorkoutHistoryItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CompletedAtUtc { get; set; }
    public int ExerciseCount { get; set; }
    public decimal TotalVolumeKg { get; set; }
}

public class WorkoutHistoryReportResponse
{
    public IReadOnlyList<WorkoutHistoryItemResponse> Items { get; set; } = Array.Empty<WorkoutHistoryItemResponse>();
}
