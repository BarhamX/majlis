namespace Majlis.Domain.DailyMajlis;

public sealed class DailyMajlis
{
    private DailyMajlis()
    {
    }

    public DailyMajlis(Guid id, DateOnly publishDate)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A Daily Majlis id is required.", nameof(id));
        }

        Id = id;
        PublishDate = publishDate;
        Status = DailyMajlisStatus.Draft;
    }

    public DailyMajlis(
        Guid id,
        DateOnly publishDate,
        DailyMajlisStatus status,
        DailyMajlisRevision publishedRevision)
        : this(id, publishDate)
    {
        switch (status)
        {
            case DailyMajlisStatus.Scheduled:
                Schedule(publishedRevision);
                break;
            case DailyMajlisStatus.Published:
                Publish(publishedRevision);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "A revision may be assigned only when scheduling or publishing a Daily Majlis.");
        }
    }

    public void Schedule(DailyMajlisRevision revision)
    {
        ValidatePublicationRevision(revision);
        Status = DailyMajlisStatus.Scheduled;
        ScheduledRevision = revision;
        ScheduledRevisionId = revision.Id;
        PublishedRevision = null;
        PublishedRevisionId = null;
    }

    public void Publish(DailyMajlisRevision revision) => Publish(
        revision,
        revision.SubmittedAt ?? throw new InvalidOperationException(
            "A submitted revision is required for publication."));

    public void Publish(DailyMajlisRevision revision, DateTimeOffset publishedAt)
    {
        ValidatePublicationRevision(revision);
        Publication ??= new DailyMajlisPublication(Id, PublishDate, publishedAt);
        Status = DailyMajlisStatus.Published;
        ScheduledRevision = null;
        ScheduledRevisionId = null;
        PublishedRevision = revision;
        PublishedRevisionId = revision.Id;
    }

    public Guid Id { get; private set; }

    public DateOnly PublishDate { get; private set; }

    public DailyMajlisStatus Status { get; private set; }

    public Guid? ScheduledRevisionId { get; private set; }

    public Guid? PublishedRevisionId { get; private set; }

    public DailyMajlisRevision? ScheduledRevision { get; private set; }

    public DailyMajlisRevision? PublishedRevision { get; private set; }

    public DailyMajlisPublication? Publication { get; private set; }

    private void ValidatePublicationRevision(DailyMajlisRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        if (revision.DailyMajlisId != Id)
        {
            throw new ArgumentException("Revision belongs to another Daily Majlis.", nameof(revision));
        }

        if (!revision.IsImmutable || !revision.IsCompleteForServing())
        {
            throw new InvalidOperationException(
                "Only a complete submitted revision may be scheduled or published.");
        }
    }
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
