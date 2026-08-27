namespace Majlis.Domain.DailyMajlis;

public sealed class DailyMajlis
{
    private DailyMajlis()
    {
    }

    public DailyMajlis(
        Guid id,
        DateOnly publishDate,
        DailyMajlisStatus status,
        DailyMajlisRevision publishedRevision)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A Daily Majlis id is required.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(publishedRevision);
        if (publishedRevision.DailyMajlisId != id)
        {
            throw new ArgumentException("Revision belongs to another Daily Majlis.", nameof(publishedRevision));
        }

        Id = id;
        PublishDate = publishDate;
        Status = status;
        PublishedRevision = publishedRevision;
        PublishedRevisionId = publishedRevision.Id;
    }

    public Guid Id { get; private set; }

    public DateOnly PublishDate { get; private set; }

    public DailyMajlisStatus Status { get; private set; }

    public Guid? ScheduledRevisionId { get; private set; }

    public Guid? PublishedRevisionId { get; private set; }

    public DailyMajlisRevision? ScheduledRevision { get; private set; }

    public DailyMajlisRevision? PublishedRevision { get; private set; }
}

public enum DailyMajlisStatus
{
    Draft,
    InReview,
    Approved,
    Scheduled,
    Published,
    Unpublished,
}
