# Spec 002: Community Majlis

## Goal

Make Majlis feel social by allowing users to answer the daily discussion question, read community responses, react, and report inappropriate comments.

Community Majlis is a required Production V1 slice and must be integrated into the full Android application before release.

## Primary User Story

As a user, I want to share my response to the daily Majlis question and read others' responses so that the app feels like a real majlis conversation.

## Scope

### In Scope

- Daily discussion question display.
- Submit response.
- List visible responses.
- React to response.
- Report response.
- Basic moderation status.
- User blocking, comment revisions, and moderation appeals through Spec 008.

### Out of Scope

- Direct messaging.
- Open groups.
- Full admin dashboard.
- AI moderation.
- Private family Majlis.

The moderation interface excluded from this slice is required separately by Spec 008; it is not excluded from Production V1.

## Requirements

- **COM-001**: Only an authenticated user who has completed the referenced Daily Majlis may list, submit, edit, react to, report, or delete discussion content for it.
- **COM-002**: A user may have one active response per Daily Majlis. Comment text shall be plain text, Unicode-normalized, 1-500 user-perceived characters, and validated against control characters, links, and configured safety rules.
- **COM-003**: A new response or edited response shall create an immutable revision with status `pending`. It is visible only to its author and moderators until approval.
- **COM-004**: Editing an approved response shall remove the prior revision from public results while the new revision is pending. Deleting a response shall remove it from public results immediately and retain only the moderation/audit data allowed by policy.
- **COM-005**: Public listing shall return only approved `visible` revisions, newest first through stable cursor pagination, and shall exclude hidden, removed, pending, deleted, suspended-author, and either-direction blocked content.
- **COM-006**: A public comment DTO shall expose comment id, approved display name, localized-safe text, allowed reaction counts, the viewer's reactions, and creation time. It shall not expose user id, profile preferences, report counts, moderation notes, or edit history.
- **COM-007**: V1 reactions are `like`, `thoughtful`, and `coffee`. A user may add or remove each type once per comment, may not react to their own comment, and cannot interact with non-visible or blocked content.
- **COM-008**: A user may submit one active report per comment using an allowed reason and optional plain-text detail of at most 500 characters. The API shall not reveal whether other reports exist.
- **COM-009**: Report author identity shall be visible only to authorized moderators and shall never be disclosed to the reported author or consumer analytics.
- **COM-010**: Consumer responses, counts, caches, search, analytics, and notifications shall apply the same moderation/deletion/block filters so hidden content cannot leak indirectly.
- **COM-011**: Comment submission shall allow at most 5 requests/hour/account, reactions 60/minute/account, and reports 10/day/account in addition to global abuse controls; a limit response shall use `429` and `Retry-After`.
- **COM-012**: All community mutations shall require an `Idempotency-Key` UUID and shall use safe, replayable semantics.

Moderation transitions, queue behavior, blocking, and appeals are normative in `specs/008-content-moderation-operations/spec.md`.

## Acceptance Criteria

- User can submit a daily response.
- User can read approved responses only after completing that Daily Majlis.
- User can report a response.
- Reported responses are visible to moderation logic.
- Pending, hidden, removed, deleted, suspended-author, and blocked comments do not appear or affect public counts.
- Comment edit, reaction toggle, report, delete, duplicate, and rate-limit behavior is deterministic and covered by PostgreSQL integration tests.
