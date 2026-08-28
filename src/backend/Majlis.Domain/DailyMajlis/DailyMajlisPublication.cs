namespace Majlis.Domain.DailyMajlis;

public sealed class DailyMajlisPublication
{
    private DailyMajlisPublication()
    {
    }

    internal DailyMajlisPublication(
        Guid dailyMajlisId,
        DateOnly publishDate,
        DateTimeOffset publishedAt)
    {
        DailyMajlisId = dailyMajlisId;
        PublishDate = publishDate;
        PublishedAt = publishedAt;
    }

    public Guid DailyMajlisId { get; private set; }

    public DateOnly PublishDate { get; private set; }

    public DateTimeOffset PublishedAt { get; private set; }
}
