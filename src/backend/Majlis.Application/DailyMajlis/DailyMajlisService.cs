using Majlis.Contracts.DailyMajlis;
using Majlis.Domain.DailyMajlis;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Application.DailyMajlis;

public sealed class DailyMajlisService(
    IDailyMajlisRepository repository,
    TimeProvider timeProvider) : IDailyMajlisService
{
    public async Task<DailyMajlisResponse?> GetTodayAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var dailyMajlis = await repository.GetPublishedByDateAsync(today, cancellationToken);

        return dailyMajlis is null ? null : MapResponse(dailyMajlis);
    }

    private static DailyMajlisResponse MapResponse(DailyMajlisEntity dailyMajlis)
    {
        var challenge = dailyMajlis.Challenge;

        return new DailyMajlisResponse(
            dailyMajlis.Id,
            dailyMajlis.PublishDate,
            dailyMajlis.Title,
            dailyMajlis.Topic,
            new DailyMajlisChallengeResponse(
                challenge.Id,
                challenge.QuestionText,
                MapType(challenge.Type),
                MapDifficulty(challenge.Difficulty),
                challenge.Region,
                challenge.Options
                    .OrderBy(option => option.SortOrder)
                    .Select(option => new ChallengeOptionResponse(option.Id, option.Text))
                    .ToArray()),
            dailyMajlis.DiscussionQuestion,
            new DailyMajlisUserStateResponse(HasAttempted: false, CurrentStreak: 0));
    }

    private static string MapType(ChallengeType type) => type switch
    {
        ChallengeType.MultipleChoice => "multipleChoice",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown challenge type."),
    };

    private static string MapDifficulty(ChallengeDifficulty difficulty) => difficulty switch
    {
        ChallengeDifficulty.Easy => "easy",
        ChallengeDifficulty.Medium => "medium",
        ChallengeDifficulty.Hard => "hard",
        _ => throw new ArgumentOutOfRangeException(
            nameof(difficulty),
            difficulty,
            "Unknown challenge difficulty."),
    };
}
