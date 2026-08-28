namespace Majlis.Domain.Progress;

public static class AttemptScoring
{
    public const int CompletionXp = 10;

    public const int CorrectnessXp = 5;

    public static AttemptScore Calculate(bool isCorrect) => new(
        CompletionXp,
        isCorrect ? CorrectnessXp : 0);
}

public sealed record AttemptScore
{
    internal AttemptScore(int completionXp, int correctnessXp)
    {
        CompletionXp = completionXp;
        CorrectnessXp = correctnessXp;
    }

    public int CompletionXp { get; }

    public int CorrectnessXp { get; }

    public int TotalXp => CompletionXp + CorrectnessXp;
}
