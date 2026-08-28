using Majlis.Domain.Progress;

namespace Majlis.Tests.Domain;

public sealed class XpAwardTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(false, 10, 0, 10)]
    [InlineData(true, 10, 5, 15)]
    public void Calculate_ReturnsExactCompletionAndCorrectnessXp(
        bool isCorrect,
        int expectedCompletionXp,
        int expectedCorrectnessXp,
        int expectedTotalXp)
    {
        var score = AttemptScoring.Calculate(isCorrect);

        Assert.Equal(expectedCompletionXp, score.CompletionXp);
        Assert.Equal(expectedCorrectnessXp, score.CorrectnessXp);
        Assert.Equal(expectedTotalXp, score.TotalXp);
    }

    [Fact]
    public void UserAttempt_WhenScoreDoesNotMatchCorrectness_RejectsAttempt()
    {
        var exception = Assert.Throws<ArgumentException>(() => new UserAttempt(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            isCorrect: false,
            AttemptScoring.Calculate(isCorrect: true),
            "ar",
            lifetimeXpAfter: 15,
            currentStreakAfter: 1,
            longestStreakAfter: 1,
            Now));

        Assert.Equal("score", exception.ParamName);
    }
}
