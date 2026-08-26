# Majlis Production V1 Database Schema

## Status and Migration Rule

This is the target logical PostgreSQL schema for Production V1. The committed initial migration currently contains only `DailyMajlis`, `Challenges`, and `ChallengeOptions`; its nullable `Challenges.SourceNotes` and non-localized text are known implementation gaps. Do not edit an applied migration. Add explicit reviewed forward migrations that preserve existing development data or document its reset.

All ids are UUIDs. All instants are `timestamptz` in UTC. Content days are PostgreSQL `date` values interpreted as UTC. User-visible text is Unicode `text`. Enum-like text values use named check constraints.

## Identity and Profile

```text
Users
- Id uuid primary key
- Status text not null -- active, suspended, deletion_pending, deleted
- AuthenticationNotBefore timestamptz nullable
- CreatedAt timestamptz not null
- LastLoginAt timestamptz nullable
- DeletedAt timestamptz nullable

UserIdentities
- Id uuid primary key
- UserId uuid not null references Users(Id)
- Provider text not null -- google, apple, meta, snapchat; test only outside Production
- Issuer text not null
- Subject text not null
- RevocationHandleCiphertext bytea nullable -- only when provider-required; encrypted outside database keys
- RevocationKeyVersion text nullable
- LinkedAt timestamptz not null
- LastAuthenticatedAt timestamptz not null
- ProviderAuthorizationRevokedAt timestamptz nullable
unique (Issuer, Subject)
unique (UserId, Provider)

Profiles
- UserId uuid primary key references Users(Id)
- DisplayName text not null
- DisplayNameNormalized text not null
- AgeBand text not null -- 13_17, 18_plus
- AgeBandAttestedAt timestamptz not null
- CountryCode char(2) nullable
- RegionCode text nullable
- DialectCode text nullable
- Locale text not null default 'ar'
- LeaderboardVisibility text not null default 'private' -- private, global_weekly
- CreatedAt timestamptz not null
- UpdatedAt timestamptz not null

UserPreferences
- UserId uuid primary key references Users(Id)
- ReminderEnabled boolean not null default false
- ReminderLocalTime time nullable
- ReminderTimeZoneId text nullable
- AnalyticsConsent boolean not null default false
- UpdatedAt timestamptz not null

UserRoleAssignments
- Id uuid primary key
- UserId uuid not null references Users(Id)
- Role text not null -- moderator, content_editor, content_reviewer, publisher, operations_admin
- AssignedByUserId uuid not null references Users(Id)
- AssignedAt timestamptz not null
- RevokedAt timestamptz nullable
unique (UserId, Role) where RevokedAt is null

UserConsents
- Id uuid primary key
- UserId uuid not null references Users(Id)
- ConsentType text not null -- terms, privacy, analytics
- Version text not null
- Accepted boolean not null
- RecordedAt timestamptz not null
unique (UserId, ConsentType, Version)

AccountDeletionRequests
- Id uuid primary key
- UserId uuid not null references Users(Id)
- Status text not null -- requested, identity_deleted, active_data_purged, backup_expiry_pending, completed, legal_hold
- RequestedAt timestamptz not null
- PurgeDueAt timestamptz not null
- CompletedAt timestamptz nullable
- LegalHoldReason text nullable
unique (UserId) where Status not in ('completed')

DeletionTombstones
- UserId uuid primary key
- RequestedAt timestamptz not null
- PurgedAt timestamptz nullable
- BackupExpiryDueAt timestamptz not null
```

Majlis stores no password hash, password-reset token, provider email as an identity key, or full date of birth. A provider-required revocation handle may be retained only as managed-key ciphertext and is never exposed to application logs or consumer APIs. Google, Apple, Meta, and Snapchat identities may be explicitly linked to one user, but are never merged by email equality. The `test` provider value is rejected by Production configuration.

## Editorial Content

```text
DailyMajlis
- Id uuid primary key
- PublishDate date not null
- Status text not null -- draft, in_review, approved, scheduled, published, unpublished
- ScheduledRevisionId uuid nullable references DailyMajlisRevisions(Id)
- PublishedRevisionId uuid nullable references DailyMajlisRevisions(Id)
- CreatedAt timestamptz not null
- UpdatedAt timestamptz not null
unique (PublishDate) where Status in ('scheduled','published')

DailyMajlisRevisions
- Id uuid primary key
- DailyMajlisId uuid not null references DailyMajlis(Id)
- RevisionNumber int not null
- TopicCode text not null
- Difficulty text not null -- easy, medium, hard
- CardType text not null -- proverb, story, saying, tradition
- SourceNotes text not null
- CreatedByUserId uuid not null references Users(Id)
- CreatedAt timestamptz not null
- SubmittedAt timestamptz nullable
- SupersedesRevisionId uuid nullable references DailyMajlisRevisions(Id)
unique (DailyMajlisId, RevisionNumber)
check (length(trim(SourceNotes)) > 0)

DailyMajlisTranslations
- RevisionId uuid not null references DailyMajlisRevisions(Id)
- Locale text not null
- Title text not null
- QuestionText text not null
- Explanation text not null
- DiscussionPrompt text not null
- CardTitle text nullable
- CardText text not null
- CardMeaning text nullable
- CardContext text nullable
- Transliteration text nullable
- PublicAttribution text nullable
- CorrectionNote text nullable
primary key (RevisionId, Locale)

Challenges
- Id uuid primary key
- RevisionId uuid not null unique references DailyMajlisRevisions(Id)
- Type text not null -- multiple_choice
unique (Id, RevisionId)

ChallengeOptions
- Id uuid primary key
- ChallengeId uuid not null references Challenges(Id)
- OptionKey text not null
- IsCorrect boolean not null
- SortOrder int not null
unique (ChallengeId, OptionKey)
unique (ChallengeId, SortOrder)
unique (Id, ChallengeId)

ChallengeOptionTranslations
- OptionId uuid not null references ChallengeOptions(Id)
- Locale text not null
- Text text not null
primary key (OptionId, Locale)

RevisionRegions
- RevisionId uuid not null references DailyMajlisRevisions(Id)
- RegionCode text not null
primary key (RevisionId, RegionCode)

RevisionDialects
- RevisionId uuid not null references DailyMajlisRevisions(Id)
- DialectCode text not null
primary key (RevisionId, DialectCode)

ContentReviews
- Id uuid primary key
- RevisionId uuid not null references DailyMajlisRevisions(Id)
- ReviewerUserId uuid not null references Users(Id)
- Decision text not null -- approved, rejected
- Note text nullable
- DecidedAt timestamptz not null
```

Publication transition rules, enforced transactionally in Domain/Application with integration tests:

- The publishing revision has a complete `ar` row and 2-4 options, each with an `ar` translation.
- Exactly one option is correct.
- `SourceNotes` is non-empty.
- The latest review is approved by someone other than `CreatedByUserId`.
- `ScheduledRevisionId` and `PublishedRevisionId` belong to the same Daily Majlis.
- A correction creates a new revision; attempts retain their original `ContentRevisionId`.

## Attempts, XP, and Streaks

```text
UserAttempts
- Id uuid primary key
- UserId uuid not null references Users(Id)
- DailyMajlisId uuid not null references DailyMajlis(Id)
- ChallengeId uuid not null references Challenges(Id)
- ContentRevisionId uuid not null references DailyMajlisRevisions(Id)
- SelectedOptionId uuid not null references ChallengeOptions(Id)
- IsCorrect boolean not null
- CompletionXp int not null -- 10
- CorrectnessXp int not null -- 0 or 5
- AttemptedAt timestamptz not null
unique (UserId, DailyMajlisId)
foreign key (ChallengeId, ContentRevisionId) references Challenges(Id, RevisionId)
foreign key (SelectedOptionId, ChallengeId) references ChallengeOptions(Id, ChallengeId)

XpLedger
- Id uuid primary key
- UserId uuid not null references Users(Id)
- AttemptId uuid not null unique references UserAttempts(Id)
- Amount int not null
- OccurredAt timestamptz not null
check (Amount in (10, 15))

UserProgress
- UserId uuid primary key references Users(Id)
- LifetimeXp bigint not null default 0
- CurrentStreak int not null default 0
- LongestStreak int not null default 0
- LastCompletedPublishDate date nullable
- UpdatedAt timestamptz not null
check (LifetimeXp >= 0 and CurrentStreak >= 0 and LongestStreak >= CurrentStreak)

IdempotencyRecords
- UserId uuid not null references Users(Id)
- Scope text not null
- IdempotencyKey uuid not null
- RequestHash text not null
- ResourceId uuid nullable
- ResponseStatus int not null
- CreatedAt timestamptz not null
- ExpiresAt timestamptz not null
primary key (UserId, Scope, IdempotencyKey)
```

Attempt, XP-ledger, and progress mutations occur in one transaction. Weekly leaderboard totals are derived from `XpLedger.OccurredAt`; an indexed/materialized projection may be added without becoming a second scoring authority.

## Discussion and Safety

```text
DiscussionComments
- Id uuid primary key
- DailyMajlisId uuid not null references DailyMajlis(Id)
- AuthorUserId uuid not null references Users(Id)
- CurrentRevisionId uuid nullable references CommentRevisions(Id)
- CreatedAt timestamptz not null
- DeletedAt timestamptz nullable
unique (DailyMajlisId, AuthorUserId) where DeletedAt is null

CommentRevisions
- Id uuid primary key
- CommentId uuid not null references DiscussionComments(Id)
- RevisionNumber int not null
- Text text not null
- Status text not null -- pending, visible, hidden, removed
- CreatedAt timestamptz not null
- ModeratedAt timestamptz nullable
unique (CommentId, RevisionNumber)

CommentReactions
- CommentId uuid not null references DiscussionComments(Id)
- UserId uuid not null references Users(Id)
- Type text not null -- like, thoughtful, coffee
- CreatedAt timestamptz not null
primary key (CommentId, UserId, Type)

CommentReports
- Id uuid primary key
- CommentId uuid not null references DiscussionComments(Id)
- ReporterUserId uuid not null references Users(Id)
- Reason text not null
- Detail text nullable
- Status text not null -- received, reviewed, dismissed, actioned
- Priority int not null
- CreatedAt timestamptz not null
- ResolvedAt timestamptz nullable
unique (CommentId, ReporterUserId) where Status in ('received','reviewed')

UserBlocks
- BlockerUserId uuid not null references Users(Id)
- BlockedUserId uuid not null references Users(Id)
- CreatedAt timestamptz not null
primary key (BlockerUserId, BlockedUserId)
check (BlockerUserId <> BlockedUserId)

ModerationActions
- Id uuid primary key
- TargetType text not null -- comment_revision, user
- TargetId uuid not null
- Action text not null -- approve, hide, restore, remove, suspend, unsuspend
- Reason text not null
- InternalNote text nullable
- ActorUserId uuid not null references Users(Id)
- CreatedAt timestamptz not null
- SupersedesActionId uuid nullable references ModerationActions(Id)

ModerationAppeals
- Id uuid primary key
- ModerationActionId uuid not null references ModerationActions(Id)
- AppellantUserId uuid not null references Users(Id)
- Reason text not null
- Status text not null -- pending, accepted, rejected
- ReviewerUserId uuid nullable references Users(Id)
- DecisionReason text nullable
- CreatedAt timestamptz not null
- DecidedAt timestamptz nullable
unique (ModerationActionId, AppellantUserId)
```

Consumer queries start from the current comment revision and require `visible`, active author/comment, no deletion, no suspension, and no block in either direction. Reaction counts apply the same filter.

## Audit and Analytics Delivery

```text
AdminAuditEvents
- Id uuid primary key
- ActorUserId uuid nullable references Users(Id)
- Action text not null
- TargetType text not null
- TargetId uuid not null
- BeforeRevisionId uuid nullable
- AfterRevisionId uuid nullable
- Reason text nullable
- CorrelationId text not null
- CreatedAt timestamptz not null

ProductEventOutbox
- Id uuid primary key
- EventName text not null
- EventVersion int not null
- UserPseudonym text nullable
- InstallationPseudonym text nullable
- AppVersion text not null
- Platform text not null
- Locale text not null
- OccurredAt timestamptz not null
- Payload jsonb not null
- DeliveredAt timestamptz nullable

OperationalFeatureFlags
- Key text primary key -- discussion
- Value text not null -- premoderated, disabled
- ChangedByUserId uuid not null references Users(Id)
- Reason text not null
- ChangedAt timestamptz not null
```

The outbox accepts only events/fields allowlisted by Spec 009. It must not contain user-generated text, option ids/text, correctness explanations, internal source notes, email, tokens, or precise location.

## Required Indexes

- `UserIdentities(Issuer, Subject)` unique and `UserIdentities(UserId, Provider)` unique.
- `DailyMajlis(PublishDate)` unique partial index for `scheduled|published`.
- `UserAttempts(UserId, AttemptedAt desc)` and unique `(UserId, DailyMajlisId)`.
- `XpLedger(OccurredAt, Amount)` and `XpLedger(UserId, OccurredAt)`.
- `DiscussionComments(DailyMajlisId, CreatedAt desc)`.
- `CommentRevisions(CommentId, RevisionNumber desc)` and `(Status, CreatedAt)`.
- `CommentReports(Status, Priority desc, CreatedAt)`.
- `ModerationAppeals(Status, CreatedAt)`.
- `AdminAuditEvents(TargetType, TargetId, CreatedAt)` and `(ActorUserId, CreatedAt)`.
- `ProductEventOutbox(DeliveredAt, OccurredAt)` for undelivered events.

## Retention

Deletion jobs implement `V1-DEC-009`: active personal data purge within 30 days, deletion tombstones through backup expiry, operational logs for 30 days, analytics for 13 months, and pseudonymous security/moderation audit data for at most 180 days unless legal hold applies.
