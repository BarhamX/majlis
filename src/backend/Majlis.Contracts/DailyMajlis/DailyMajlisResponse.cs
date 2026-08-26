namespace Majlis.Contracts.DailyMajlis;

public sealed record DailyMajlisResponse(
    Guid DailyMajlisId,
    DateOnly Date,
    string Title,
    string Topic,
    DailyMajlisChallengeResponse Challenge,
    string DiscussionQuestion,
    DailyMajlisUserStateResponse UserState);

public sealed record DailyMajlisChallengeResponse(
    Guid Id,
    string QuestionText,
    string Type,
    string Difficulty,
    string? Region,
    IReadOnlyList<ChallengeOptionResponse> Options);

public sealed record ChallengeOptionResponse(Guid Id, string Text);

public sealed record DailyMajlisUserStateResponse(bool HasAttempted, int CurrentStreak);
