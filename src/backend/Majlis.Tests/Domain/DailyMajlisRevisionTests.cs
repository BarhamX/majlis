using Majlis.Domain.DailyMajlis;

namespace Majlis.Tests.Domain;

public sealed class DailyMajlisRevisionTests
{
    [Fact]
    public void SubmittedRevision_CannotBeChanged()
    {
        var revision = CreateCompleteRevision();

        revision.Submit(new DateTimeOffset(2026, 8, 26, 10, 0, 0, TimeSpan.Zero));

        Assert.Throws<InvalidOperationException>(() => revision.AddTranslation(
            new DailyMajlisTranslation(
                revision.Id,
                "fr",
                "Title",
                "Question",
                "Explanation",
                "Discussion",
                "Card")));
    }

    [Fact]
    public void Revision_IsServableOnlyWhenArabicAndEveryOptionAreComplete()
    {
        var revision = CreateCompleteRevision();

        Assert.True(revision.IsCompleteForServing());

        revision.RemoveTranslation("ar");

        Assert.False(revision.IsCompleteForServing());
    }

    [Fact]
    public void OptionTranslationMutation_IsAggregateOnly()
    {
        Assert.DoesNotContain(
            typeof(ChallengeOption).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
            method => method.Name == "AddTranslation");
    }

    private static DailyMajlisRevision CreateCompleteRevision()
    {
        var revisionId = Guid.NewGuid();
        var revision = new DailyMajlisRevision(
            revisionId,
            Guid.NewGuid(),
            1,
            "hospitality",
            ChallengeDifficulty.Easy,
            CardType.Proverb,
            "Verified source notes.",
            null,
            DateTimeOffset.UtcNow);
        revision.SetChallenge(new Challenge(
            Guid.NewGuid(),
            revisionId,
            ChallengeType.MultipleChoice,
            [
                new ChallengeOption(Guid.NewGuid(), "A", true, 1),
                new ChallengeOption(Guid.NewGuid(), "B", false, 2),
            ]));
        revision.AddTranslation(new DailyMajlisTranslation(
            revision.Id,
            "ar",
            "العنوان",
            "السؤال",
            "الشرح",
            "سؤال النقاش",
            "البطاقة"));
        revision.AddTranslation(new DailyMajlisTranslation(
            revision.Id,
            "en",
            "Title",
            "Question",
            "Explanation",
            "Discussion",
            "Card"));
        foreach (var option in revision.Challenge!.Options)
        {
            revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "ar", "خيار"));
            revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "en", "Option"));
        }

        return revision;
    }
}
