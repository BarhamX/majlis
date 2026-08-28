# Plan 002: Community Majlis

## Architecture

Discussion belongs to backend Application and Domain. Flutter uses Discussion feature screens and providers. Moderation status is part of the data model from the first community release.

## Backend Work

1. Add DiscussionComment entity.
2. Add immutable CommentRevision, CommentReaction, CommentReport, and UserBlock entities.
3. Add comment submit/edit/delete/list, reaction toggle, report, and block APIs.
4. Add completion eligibility, authorization, status, deletion, suspension, and block filtering.
5. Add idempotency, uniqueness, rate limits, and stable cursor pagination.
6. Add PostgreSQL integration and consumer-leak tests.

## Flutter Work

1. Add Discussion screen.
2. Add response input.
3. Add comment list.
4. Add reaction action.
5. Add report action.
6. Add empty states.

## Safety Work

1. Add community rules copy.
2. Add report reasons.
3. Add admin moderation-ready data model.
4. Implement premoderation, audit, and appeal behavior through Spec 008.
