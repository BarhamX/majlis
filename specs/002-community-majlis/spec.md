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

### Out of Scope

- Direct messaging.
- Open groups.
- Full admin dashboard.
- AI moderation.
- Private family Majlis.

## Requirements

- WHEN a user submits a response, THE SYSTEM SHALL save it with a moderation status.
- WHEN comments are listed, THE SYSTEM SHALL return visible comments only.
- WHEN a user reports a comment, THE SYSTEM SHALL create a report record.
- WHEN a comment is hidden, THE SYSTEM SHALL not return it in public lists.
- WHEN a user reacts to a comment, THE SYSTEM SHALL prevent duplicate reaction of the same type by the same user.

## Acceptance Criteria

- User can submit a daily response.
- User can read responses after answering or entering discussion screen.
- User can report a response.
- Reported responses are visible to moderation logic.
- Hidden comments do not appear in normal list response.
