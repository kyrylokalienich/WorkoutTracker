namespace WorkoutTracker.Application.Models.Response.WorkoutSessions;

public class PagedWorkoutSessionsResponse
{
    public IReadOnlyList<WorkoutSessionListItemResponse> Items { get; set; } = Array.Empty<WorkoutSessionListItemResponse>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
