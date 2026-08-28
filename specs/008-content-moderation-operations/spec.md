# Spec 008: Content and Moderation Administration

## Goal

Give authorized operators a usable, auditable interface to prepare one culturally reviewed Daily Majlis per UTC day and safely moderate public discussion without code changes or direct database editing.

## Roles

- `content_editor`: creates and edits drafts.
- `content_reviewer`: approves or rejects submitted revisions and cannot approve their own revision.
- `publisher`: schedules, publishes, corrects, and unpublishes approved revisions.
- `moderator`: reviews pending comments, reports, blocks/suspensions, and appeals.
- `operations_admin`: assigns roles through an audited process and operates the service; it does not imply editorial approval.

All privileged accounts require managed-provider MFA. Roles are additive and least privilege applies.

## Content Requirements

- **ADM-001**: Operators shall use a protected browser-based admin interface; raw API calls may support automation but are not the sole V1 operating interface.
- **ADM-002**: Content shall use immutable revisions. Editing a submitted, approved, scheduled, or published revision shall create a new draft revision.
- **ADM-003**: A revision cannot enter review without topic, difficulty, provenance region/dialect tags, discussion prompt, one challenge with 2-4 options and exactly one correct option, explanation, and non-empty internal source notes.
- **ADM-004**: A revision cannot be approved or published without a complete Arabic translation for every user-visible field. Optional locales must be complete within that locale.
- **ADM-005**: A reviewer shall record approve/reject, note, timestamp, and revision. The editor of a revision shall not approve that revision.
- **ADM-006**: Only an approved revision may be scheduled, and the database shall permit at most one scheduled or published Daily Majlis for a UTC `PublishDate`.
- **ADM-007**: Publishing shall be automatic at `00:00:00Z`; scheduling must reject past dates and surface gaps or conflicts for the next 14 UTC dates.
- **ADM-008**: Unpublishing shall immediately stop normal delivery. A corrected publication shall reference the superseded revision, preserve historical attempts/XP, and display a localized correction note to affected users; it shall not silently rescore attempts.
- **ADM-009**: Internal source notes and operator identity shall never be returned by consumer endpoints. Public attribution is a separate localized field approved for display.
- **ADM-010**: Every content create, edit, submit, approve, reject, schedule, publish, correct, and unpublish action shall append an immutable audit event with actor, target, before/after revision ids, timestamp, and reason where required.

## Moderation Requirements

- **MOD-001**: New comments shall enter `pending`; only their author and moderators may read them until approval.
- **MOD-002**: A moderator may approve, hide, restore, or remove a comment and must choose a reason for hide/remove. Removed content shall not be restorable through ordinary moderation.
- **MOD-003**: A report shall use an allowed reason, be unique per reporter/comment, enter the moderation queue, and never reveal reporter identity to the comment author.
- **MOD-004**: Queue priority shall order credible threats/doxxing, hate/sectarian abuse, harassment, misinformation claims, and spam, while preserving creation time within a priority.
- **MOD-005**: Moderator actions shall append immutable audit events. Public APIs shall expose only a neutral content status, never moderator identity or internal notes.
- **MOD-006**: A user may block another user. The blocked user shall not be notified, and consumer queries/reactions shall enforce two-way interaction filtering.
- **MOD-007**: A user may appeal one hide, remove, or account-suspension action within 30 days. A different moderator shall accept or reject the appeal with a reason; the original action remains in history.
- **MOD-008**: Pending, hidden, removed, reported, and appealed content shall remain available only to authorized moderation/appeal use cases and retention jobs.
- **MOD-009**: Moderation endpoints shall be rate-limited, paginated, filterable, and safe against stored markup/script injection. Plain text is the only V1 comment format.
- **MOD-010**: The moderation service shall expose queue age and oldest-item metrics without comment text or personal data in metric labels.
- **MOD-011**: Operations shall acknowledge credible threats/doxxing within 1 hour, decide hate/sectarian abuse or harassment within 4 hours, and decide 95% of other pending comments within 12 hours, reports within 24 hours, and appeals within 5 calendar days. Public discussion shall remain disabled when staffed coverage cannot meet these targets.
- **MOD-012**: An operations admin shall be able to switch public discussion between `premoderated` and `disabled` through an audited control. Disabled mode shall reject new interactions, hide public lists behind a localized unavailable state, preserve data, and leave the Daily Majlis challenge playable.

## Acceptance Criteria

- An editor, distinct reviewer, and publisher can create and publish the next Arabic Daily Majlis without developer or database access.
- The system rejects missing sources, incomplete Arabic, self-approval, past schedules, and a second official UTC publication.
- Pending/hidden/removed comments cannot leak through consumer APIs, caches, counts, leaderboard data, or analytics payloads.
- A moderator can process reports and an eligible appeal with a complete immutable audit trail.
- Corrected content preserves the original attempt/XP ledger and shows a correction notice.
