namespace WorkoutTracker.Application.Common;

public sealed class ServiceResult<T>
{
    public bool Succeeded { get; private init; }
    public T? Value { get; private init; }
    public string? FailureCode { get; private init; }
    public object? FailureDetails { get; private init; }

    public static ServiceResult<T> Ok(T value) =>
        new() { Succeeded = true, Value = value };

    public static ServiceResult<T> Fail(string code, object? details = null) =>
        new() { Succeeded = false, FailureCode = code, FailureDetails = details };
}
