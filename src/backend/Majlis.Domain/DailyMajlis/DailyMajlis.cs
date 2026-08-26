namespace Majlis.Domain.DailyMajlis;

public sealed class DailyMajlis
{
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

    public Guid Id { get; }

    public DateOnly PublishDate { get; }

    public string Title { get; }

    public string Topic { get; }

    public Challenge Challenge { get; }

    public string DiscussionQuestion { get; }

    public DailyMajlisStatus Status { get; }

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
