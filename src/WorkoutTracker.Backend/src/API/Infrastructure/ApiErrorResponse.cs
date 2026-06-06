namespace WorkoutTracker.API.Infrastructure;

public sealed record ApiErrorResponse(string Code, string Message, object? Details = null);
