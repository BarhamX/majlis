using Majlis.Domain.DailyMajlis;

namespace Majlis.Tests.Domain;

public sealed class ChallengeTests
{
    [Fact]
    public void Constructor_WhenMultipleChoiceHasMoreThanOneCorrectOption_Throws()
    {
        var options = new[]
        {
            new ChallengeOption(Guid.NewGuid(), "First", isCorrect: true, sortOrder: 1),
            new ChallengeOption(Guid.NewGuid(), "Second", isCorrect: true, sortOrder: 2),
        };

        var exception = Assert.Throws<ArgumentException>(() => new Challenge(
            Guid.NewGuid(),
            "Question",
            ChallengeType.MultipleChoice,
            ChallengeDifficulty.Easy,
            "panArab",
            "hospitality",
            "Explanation",
            "Editorial source notes",
            ContentReviewStatus.Reviewed,
            options));

        Assert.Equal("options", exception.ParamName);
        Assert.Contains("exactly one correct option", exception.Message, StringComparison.Ordinal);
    }
}
