namespace Majlis.Domain.DailyMajlis;

public sealed class DailyMajlis
{
    private DailyMajlis()
    {
        Title = string.Empty;
        Topic = string.Empty;
        Challenge = null!;
        DiscussionQuestion = string.Empty;
    }

    public DailyMajlis(
        Guid id,
        DateOnly publishDate,
        string title,
        string topic,
        Challenge challenge,
        string discussionQuestion,
        DailyMajlisStatus status)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A Daily Majlis id is required.", nameof(id));
        }

        Id = id;
        PublishDate = publishDate;
        Title = RequireText(title, nameof(title));
        Topic = RequireText(topic, nameof(topic));
        Challenge = challenge ?? throw new ArgumentNullException(nameof(challenge));
        DiscussionQuestion = RequireText(discussionQuestion, nameof(discussionQuestion));
        Status = status;
    }

    public Guid Id { get; private set; }

    public DateOnly PublishDate { get; private set; }

    public string Title { get; private set; }

    public string Topic { get; private set; }

    public Challenge Challenge { get; private set; }

    public string DiscussionQuestion { get; private set; }

    public DailyMajlisStatus Status { get; private set; }

    private static string RequireText(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value;
    }
}

public enum DailyMajlisStatus
{
    Draft,
    Scheduled,
    Published,
    Unpublished,
}
