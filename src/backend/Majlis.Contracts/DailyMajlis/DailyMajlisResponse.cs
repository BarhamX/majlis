namespace Majlis.Contracts.DailyMajlis;

public sealed record DailyMajlisResponse(
    Guid DailyMajlisId,
    DateOnly PublishDate,
    string Title,
    string TopicCode,
    DailyMajlisChallengeResponse Challenge,
    string DiscussionPrompt,
    DailyMajlisUserStateResponse UserState);

public sealed record DailyMajlisChallengeResponse(
    Guid Id,
    string Question,
    string Type,
    string Difficulty,
    string? RegionCode,
    IReadOnlyList<ChallengeOptionResponse> Options);

public sealed record ChallengeOptionResponse(Guid Id, string Text);

public sealed record DailyMajlisUserStateResponse(bool HasAttempted, Guid? AttemptId);
