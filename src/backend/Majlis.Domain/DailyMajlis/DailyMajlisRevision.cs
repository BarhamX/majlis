namespace Majlis.Domain.DailyMajlis;

public sealed class DailyMajlisRevision
{
    private readonly List<DailyMajlisTranslation> _translations = [];
    private readonly List<ChallengeOptionTranslation> _optionTranslations = [];
    private readonly List<RevisionRegion> _regions = [];
    private readonly List<RevisionDialect> _dialects = [];

    private DailyMajlisRevision()
    {
        TopicCode = string.Empty;
        SourceNotes = string.Empty;
    }

    public DailyMajlisRevision(
        Guid id,
        Guid dailyMajlisId,
        int revisionNumber,
        string topicCode,
        ChallengeDifficulty difficulty,
        CardType cardType,
        string sourceNotes,
        Guid? createdByUserId,
        DateTimeOffset createdAt,
        Guid? supersedesRevisionId = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A revision id is required.", nameof(id));
        }

        if (dailyMajlisId == Guid.Empty)
        {
            throw new ArgumentException("A Daily Majlis id is required.", nameof(dailyMajlisId));
        }

        if (revisionNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(revisionNumber));
        }

        Id = id;
        DailyMajlisId = dailyMajlisId;
        RevisionNumber = revisionNumber;
        TopicCode = RequireText(topicCode, nameof(topicCode));
        Difficulty = difficulty;
        CardType = cardType;
        SourceNotes = RequireText(sourceNotes, nameof(sourceNotes));
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        SupersedesRevisionId = supersedesRevisionId;
    }

    public Guid Id { get; private set; }

    public Guid DailyMajlisId { get; private set; }

    public int RevisionNumber { get; private set; }

    public string TopicCode { get; private set; }

    public ChallengeDifficulty Difficulty { get; private set; }

    public CardType CardType { get; private set; }

    public string SourceNotes { get; private set; }

    // Imported and Development/Testing content may have no operator identity;
    // administrative provenance becomes mandatory when the content workflow lands.
    public Guid? CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public Guid? SupersedesRevisionId { get; private set; }

    public Challenge? Challenge { get; private set; }

    public IReadOnlyList<DailyMajlisTranslation> Translations => _translations;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyList<ChallengeOptionTranslation> OptionTranslations => _optionTranslations;

    public IReadOnlyList<RevisionRegion> Regions => _regions;

    public IReadOnlyList<RevisionDialect> Dialects => _dialects;

    public bool IsImmutable => SubmittedAt.HasValue;

    public void SetChallenge(Challenge challenge)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(challenge);
        if (challenge.RevisionId != Id)
        {
            throw new ArgumentException("Challenge belongs to another revision.", nameof(challenge));
        }

        Challenge = challenge;
    }

    public void AddTranslation(DailyMajlisTranslation translation)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(translation);
        if (translation.RevisionId != Id)
        {
            throw new ArgumentException("Translation belongs to another revision.", nameof(translation));
        }

        _translations.RemoveAll(existing => existing.Locale == translation.Locale);
        _translations.Add(translation);
    }

    public void RemoveTranslation(string locale)
    {
        EnsureMutable();
        _translations.RemoveAll(existing => existing.Locale == NormalizeLocale(locale));
    }

    public void AddOptionTranslation(ChallengeOptionTranslation translation)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(translation);
        if (Challenge is null || Challenge.Options.All(option => option.Id != translation.OptionId))
        {
            throw new ArgumentException("Option translation does not belong to this revision.", nameof(translation));
        }

        _optionTranslations.RemoveAll(existing =>
            existing.OptionId == translation.OptionId && existing.Locale == translation.Locale);
        _optionTranslations.Add(translation);
        Challenge.Options.Single(option => option.Id == translation.OptionId).AddTranslation(translation);
    }

    public void AddRegion(string regionCode)
    {
        EnsureMutable();
        _regions.Add(new RevisionRegion(Id, RequireText(regionCode, nameof(regionCode))));
    }

    public void AddDialect(string dialectCode)
    {
        EnsureMutable();
        _dialects.Add(new RevisionDialect(Id, RequireText(dialectCode, nameof(dialectCode))));
    }

    public void Submit(DateTimeOffset submittedAt)
    {
        EnsureMutable();
        if (!IsCompleteForServing())
        {
            throw new InvalidOperationException("A revision must have complete Arabic content before submission.");
        }

        SubmittedAt = submittedAt;
    }

    public bool IsCompleteForServing()
    {
        if (Challenge is null || Challenge.Options.Count is < 2 or > 4 ||
            Challenge.Options.Count(option => option.IsCorrect) != 1)
        {
            return false;
        }

        var arabic = _translations.FirstOrDefault(translation => translation.Locale == "ar");
        if (arabic is null || !arabic.IsComplete)
        {
            return false;
        }

        return Challenge.Options.All(option => option.Translations.Any(translation =>
            translation.OptionId == option.Id && translation.Locale == "ar" &&
            !string.IsNullOrWhiteSpace(translation.Text)));
    }

    private void EnsureMutable()
    {
        if (IsImmutable)
        {
            throw new InvalidOperationException("Submitted content revisions are immutable.");
        }
    }

    private static string RequireText(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
    }

    private static string NormalizeLocale(string value)
    {
        return RequireText(value, nameof(value)).ToLowerInvariant();
    }
}

public sealed class DailyMajlisTranslation
{
    private DailyMajlisTranslation()
    {
        Locale = string.Empty;
        Title = string.Empty;
        QuestionText = string.Empty;
        Explanation = string.Empty;
        DiscussionPrompt = string.Empty;
        CardText = string.Empty;
    }

    public DailyMajlisTranslation(
        Guid revisionId,
        string locale,
        string title,
        string questionText,
        string explanation,
        string discussionPrompt,
        string cardText,
        string? cardTitle = null,
        string? cardMeaning = null,
        string? cardContext = null,
        string? transliteration = null,
        string? publicAttribution = null,
        string? correctionNote = null)
    {
        if (revisionId == Guid.Empty)
        {
            throw new ArgumentException("A revision id is required.", nameof(revisionId));
        }

        RevisionId = revisionId;
        Locale = RequireText(locale, nameof(locale)).ToLowerInvariant();
        Title = RequireText(title, nameof(title));
        QuestionText = RequireText(questionText, nameof(questionText));
        Explanation = RequireText(explanation, nameof(explanation));
        DiscussionPrompt = RequireText(discussionPrompt, nameof(discussionPrompt));
        CardText = RequireText(cardText, nameof(cardText));
        CardTitle = cardTitle;
        CardMeaning = cardMeaning;
        CardContext = cardContext;
        Transliteration = transliteration;
        PublicAttribution = publicAttribution;
        CorrectionNote = correctionNote;
    }

    public Guid RevisionId { get; private set; }

    public string Locale { get; private set; }

    public string Title { get; private set; }

    public string QuestionText { get; private set; }

    public string Explanation { get; private set; }

    public string DiscussionPrompt { get; private set; }

    public string? CardTitle { get; private set; }

    public string CardText { get; private set; }

    public string? CardMeaning { get; private set; }

    public string? CardContext { get; private set; }

    public string? Transliteration { get; private set; }

    public string? PublicAttribution { get; private set; }

    public string? CorrectionNote { get; private set; }

    public bool IsComplete => !string.IsNullOrWhiteSpace(Title) &&
        !string.IsNullOrWhiteSpace(QuestionText) &&
        !string.IsNullOrWhiteSpace(Explanation) &&
        !string.IsNullOrWhiteSpace(DiscussionPrompt) &&
        !string.IsNullOrWhiteSpace(CardText);

    private static string RequireText(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
    }
}

public sealed class ChallengeOptionTranslation
{
    private ChallengeOptionTranslation()
    {
        Locale = string.Empty;
        Text = string.Empty;
    }

    public ChallengeOptionTranslation(Guid optionId, string locale, string text)
    {
        if (optionId == Guid.Empty)
        {
            throw new ArgumentException("An option id is required.", nameof(optionId));
        }

        OptionId = optionId;
        Locale = string.IsNullOrWhiteSpace(locale)
            ? throw new ArgumentException("A locale is required.", nameof(locale))
            : locale.Trim().ToLowerInvariant();
        Text = string.IsNullOrWhiteSpace(text)
            ? throw new ArgumentException("Option text is required.", nameof(text))
            : text.Trim();
    }

    public Guid OptionId { get; private set; }

    public string Locale { get; private set; }

    public string Text { get; private set; }
}

public sealed class RevisionRegion
{
    private RevisionRegion()
    {
        RegionCode = string.Empty;
    }

    public RevisionRegion(Guid revisionId, string regionCode)
    {
        RevisionId = revisionId;
        RegionCode = regionCode;
    }

    public Guid RevisionId { get; private set; }

    public string RegionCode { get; private set; }
}

public sealed class RevisionDialect
{
    private RevisionDialect()
    {
        DialectCode = string.Empty;
    }

    public RevisionDialect(Guid revisionId, string dialectCode)
    {
        RevisionId = revisionId;
        DialectCode = dialectCode;
    }

    public Guid RevisionId { get; private set; }

    public string DialectCode { get; private set; }
}
