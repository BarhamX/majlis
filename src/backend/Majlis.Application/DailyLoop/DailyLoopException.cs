namespace Majlis.Application.DailyLoop;

public sealed class DailyLoopException(
    string code,
    string message,
    Guid? attemptId = null) : Exception(message)
{
    public string Code { get; } = code;

    public Guid? AttemptId { get; } = attemptId;
}

public sealed class DailyLoopPersistenceConflictException(
    Exception innerException) : Exception(
        "The daily-loop transaction conflicted with another request.",
        innerException);
