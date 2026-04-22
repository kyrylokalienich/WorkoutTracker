using WorkoutTracker.Application.Interfaces.Providers;

namespace WorkoutTracker.Application.Providers;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
