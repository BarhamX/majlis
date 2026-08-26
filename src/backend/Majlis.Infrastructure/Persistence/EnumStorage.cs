using Majlis.Domain.DailyMajlis;

namespace Majlis.Infrastructure.Persistence;

internal static class EnumStorage
{
    public static string ToStorage(DailyMajlisStatus value) => value switch
    {
        DailyMajlisStatus.Draft => "draft",
        DailyMajlisStatus.Scheduled => "scheduled",
        DailyMajlisStatus.Published => "published",
        DailyMajlisStatus.Unpublished => "unpublished",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown Daily Majlis status."),
    };

    public static DailyMajlisStatus ToDailyMajlisStatus(string value) => value switch
    {
        "draft" => DailyMajlisStatus.Draft,
        "scheduled" => DailyMajlisStatus.Scheduled,
        "published" => DailyMajlisStatus.Published,
        "unpublished" => DailyMajlisStatus.Unpublished,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown stored Daily Majlis status."),
    };

    public static string ToStorage(ChallengeType value) => value switch
    {
        ChallengeType.MultipleChoice => "multipleChoice",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown challenge type."),
    };

    public static ChallengeType ToChallengeType(string value) => value switch
    {
        "multipleChoice" => ChallengeType.MultipleChoice,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown stored challenge type."),
    };

    public static string ToStorage(ChallengeDifficulty value) => value switch
    {
        ChallengeDifficulty.Easy => "easy",
        ChallengeDifficulty.Medium => "medium",
        ChallengeDifficulty.Hard => "hard",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown challenge difficulty."),
    };

    public static ChallengeDifficulty ToChallengeDifficulty(string value) => value switch
    {
        "easy" => ChallengeDifficulty.Easy,
        "medium" => ChallengeDifficulty.Medium,
        "hard" => ChallengeDifficulty.Hard,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown stored challenge difficulty."),
    };

    public static string ToStorage(ContentReviewStatus value) => value switch
    {
        ContentReviewStatus.Draft => "draft",
        ContentReviewStatus.Reviewed => "reviewed",
        ContentReviewStatus.Rejected => "rejected",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown content review status."),
    };

    public static ContentReviewStatus ToContentReviewStatus(string value) => value switch
    {
        "draft" => ContentReviewStatus.Draft,
        "reviewed" => ContentReviewStatus.Reviewed,
        "rejected" => ContentReviewStatus.Rejected,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown stored content review status."),
    };
}
