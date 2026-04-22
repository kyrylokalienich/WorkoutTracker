namespace WorkoutTracker.Application.Interfaces.Providers;

public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC time.
    /// </summary>
    /// <returns>Current UTC DateTime</returns>
    DateTime UtcNow { get; }
}
