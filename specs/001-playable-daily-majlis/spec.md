# Spec 001: Playable Daily Majlis

## Goal

Build the first playable version of Majlis: a user can open today's Majlis, answer the daily cultural challenge, receive feedback, read a short explanation, update streak/XP, and generate a shareable result card.

## Primary User Story

As an Arab user, I want to answer a short daily cultural challenge so that I can test my cultural knowledge, learn something quickly, and challenge my friends or family.

## Scope

### In Scope

- User profile foundation.
- Fetch today's Daily Majlis.
- Display challenge and answer options.
- Submit answer.
- Server-side validation.
- Result and explanation.
- XP and streak update.
- Shareable result summary.

### Out of Scope

- Comments and discussion implementation.
- Friend groups.
- Premium access.
- Advanced audio.
- Full admin UI.

## Requirements

### Daily Content

- WHEN the app opens today's screen, THE SYSTEM SHALL fetch the published Daily Majlis for the current date.
- WHEN today's Daily Majlis exists, THE SYSTEM SHALL return challenge details without exposing the correct answer.
- WHEN no Daily Majlis exists, THE SYSTEM SHALL return a user-safe fallback.

### Challenge

- WHEN the user submits an answer, THE SYSTEM SHALL validate it on the backend.
- WHEN the answer is submitted, THE SYSTEM SHALL store exactly one scored attempt per user per challenge.
- WHEN the attempt is complete, THE SYSTEM SHALL return correct/incorrect result and explanation.

### Streak and XP

- WHEN the user completes the daily challenge for the first time that day, THE SYSTEM SHALL update XP and streak.
- WHEN the user repeats the same challenge, THE SYSTEM SHALL not duplicate XP or streak.

### Sharing

- WHEN the user completes the challenge, THE SYSTEM SHALL provide spoiler-safe share text.
- WHEN the user requests a share card, THE APP SHALL render a branded visual card.

## Acceptance Criteria

- User can complete the full daily loop in 1-3 minutes.
- Backend tests cover scoring and duplicate-attempt behavior.
- Flutter can show loading, success, error, and completed states.
- Correct answer is never sent before submission.
- Share card does not spoil the answer by default.
