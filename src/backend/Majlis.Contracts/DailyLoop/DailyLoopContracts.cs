namespace Majlis.Contracts.DailyLoop;

public sealed record SubmitAttemptRequest(Guid SelectedOptionId);

public sealed record AttemptResultResponse(
    Guid AttemptId,
    Guid DailyMajlisId,
    DateOnly PublishDate,
    bool IsCorrect,
    Guid CorrectOptionId,
    string Explanation,
    CulturalCardResponse CulturalCard,
    AttemptXpResponse Xp,
    AttemptStreakResponse Streak,
    Guid ContentRevisionId,
    string ResultLocale);

public sealed record CulturalCardResponse(
    string Type,
    string? Title,
    string Text,
    string? Meaning,
    string? Context,
    string? PublicAttribution);

public sealed record AttemptXpResponse(
    int Completion,
    int Correctness,
    int Awarded,
    long LifetimeTotal);

public sealed record AttemptStreakResponse(
    int Current,
    int Longest,
    bool Updated);

public sealed record AttemptHistoryResponse(
    IReadOnlyList<AttemptHistoryItemResponse> Items,
    string? NextCursor);

public sealed record AttemptHistoryItemResponse(
    Guid AttemptId,
    DateOnly PublishDate,
    string Title,
    bool IsCorrect,
    int XpAwarded,
    string ResultLocale,
    Guid ContentRevisionId);

public sealed record UserProgressResponse(
    long LifetimeXp,
    int CurrentStreak,
    int LongestStreak,
    DateOnly? LastCompletedPublishDate);

public sealed record AttemptShareResponse(
    DateOnly PublishDate,
    string ResultState,
    string Title,
    string Body,
    string Url,
    string ImageAlt);
