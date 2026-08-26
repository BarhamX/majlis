# Majlis Database Schema

Database: PostgreSQL

## Users

```text
Users
- Id uuid primary key
- Email text unique not null
- PasswordHash text nullable if external auth
- CreatedAt timestamptz not null
- LastLoginAt timestamptz nullable
```

## Profiles

```text
Profiles
- UserId uuid primary key references Users(Id)
- DisplayName text not null
- Region text nullable
- Dialect text nullable
- AvatarUrl text nullable
- CreatedAt timestamptz not null
- UpdatedAt timestamptz not null
```

## DailyMajlis

```text
DailyMajlis
- Id uuid primary key
- PublishDate date not null
- Title text not null
- Topic text not null
- ChallengeId uuid not null references Challenges(Id)
- StoryId uuid nullable references Stories(Id)
- ProverbId uuid nullable references Proverbs(Id)
- DiscussionQuestion text not null
- Status text not null -- draft, scheduled, published, unpublished
- CreatedAt timestamptz not null
- UpdatedAt timestamptz not null
```

Unique index:

```text
DailyMajlis(PublishDate) where Status in ('scheduled','published')
```

## Challenges

```text
Challenges
- Id uuid primary key
- QuestionText text not null
- Type text not null -- multipleChoice initially
- Difficulty text not null -- easy, medium, hard
- Region text nullable
- Topic text not null
- Explanation text not null
- SourceNotes text nullable
- ReviewStatus text not null -- draft, reviewed, rejected
- CreatedAt timestamptz not null
```

## ChallengeOptions

```text
ChallengeOptions
- Id uuid primary key
- ChallengeId uuid not null references Challenges(Id)
- Text text not null
- IsCorrect boolean not null
- SortOrder int not null
```

## Proverbs

```text
Proverbs
- Id uuid primary key
- TextArabic text not null
- Transliteration text nullable
- Meaning text not null
- Context text nullable
- Region text nullable
- SourceNotes text nullable
- ReviewStatus text not null
- CreatedAt timestamptz not null
```

## Stories

```text
Stories
- Id uuid primary key
- Title text not null
- ShortText text not null
- HistoricalPeriod text nullable
- Region text nullable
- SourceNotes text nullable
- AudioUrl text nullable
- ReviewStatus text not null
- CreatedAt timestamptz not null
```

## UserAttempts

```text
UserAttempts
- Id uuid primary key
- UserId uuid not null references Users(Id)
- ChallengeId uuid not null references Challenges(Id)
- SelectedOptionId uuid not null references ChallengeOptions(Id)
- IsCorrect boolean not null
- Score int not null
- CreatedAt timestamptz not null
```

Unique index:

```text
UserAttempts(UserId, ChallengeId)
```

## UserStreaks

```text
UserStreaks
- UserId uuid primary key references Users(Id)
- CurrentStreak int not null default 0
- LongestStreak int not null default 0
- LastCompletedDate date nullable
- UpdatedAt timestamptz not null
```

## DiscussionComments

```text
DiscussionComments
- Id uuid primary key
- DailyMajlisId uuid not null references DailyMajlis(Id)
- UserId uuid not null references Users(Id)
- Text text not null
- Status text not null -- visible, pending, hidden, removed
- CreatedAt timestamptz not null
- UpdatedAt timestamptz not null
```

## CommentReactions

```text
CommentReactions
- Id uuid primary key
- CommentId uuid not null references DiscussionComments(Id)
- UserId uuid not null references Users(Id)
- Type text not null -- like, thoughtful, coffee
- CreatedAt timestamptz not null
```

Unique index:

```text
CommentReactions(CommentId, UserId, Type)
```

## CommentReports

```text
CommentReports
- Id uuid primary key
- CommentId uuid not null references DiscussionComments(Id)
- ReporterUserId uuid not null references Users(Id)
- Reason text not null
- Status text not null -- received, reviewed, dismissed, actioned
- CreatedAt timestamptz not null
```
