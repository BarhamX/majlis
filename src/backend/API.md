# Backend API Guide

This file documents the API surface used by the Majlis mobile app. Keep this file updated whenever an endpoint is added, renamed, removed, or when a request/response contract changes.

## API Principles

- Version public endpoints under `/api/v1`.
- Keep mobile responses compact and explicit.
- Do not expose correct answers before the user submits an answer.
- Return stable identifiers for client-side analytics and future deep links.
- Use UTC timestamps in API contracts unless a feature explicitly requires user-local dates.
- For daily game behavior, the server is the source of truth.

## Current Endpoint

### Get Today's Daily Majlis

```http
GET /api/v1/daily-majlis/today
```

Returns the current Daily Majlis experience for the user.

### Intended Client Use

The Flutter app uses this endpoint to render:

- Daily Majlis title
- Cultural topic
- Short story preview
- Proverb section
- Challenge question
- Challenge options
- Discussion question
- Share-card input data

### Spoiler Safety

The endpoint must not return:

- `correctOptionId`
- `isCorrect` flags
- Correct-answer explanation that reveals the answer before submission
- Internal moderation metadata

### Example Response Shape

```json
{
  "id": "daily-2026-09-07",
  "date": "2026-09-07",
  "title": "The Guest Before the House",
  "topic": "Arab hospitality",
  "story": {
    "title": "A guest arrives before sunset",
    "shortText": "A short cultural story designed to be read in under 30 seconds."
  },
  "proverb": {
    "textArabic": "الضيف ضيف الله",
    "meaning": "A guest is treated with honor and generosity."
  },
  "challenge": {
    "id": "challenge-001",
    "questionText": "What does this proverb most closely mean?",
    "type": "multipleChoice",
    "difficulty": "easy",
    "options": [
      { "id": "a", "text": "A guest should bring gifts" },
      { "id": "b", "text": "A guest is honored as a trust" },
      { "id": "c", "text": "A guest should not stay long" }
    ]
  },
  "discussionQuestion": "What hospitality habit does your family still practice?"
}
```

## Planned MVP Endpoints

### Submit Challenge Answer

```http
POST /api/v1/daily-majlis/{dailyMajlisId}/attempts
```

Purpose: submit the user's answer and return result feedback.

Expected behavior:

- Validate that the Daily Majlis exists.
- Validate that the selected option belongs to the challenge.
- Prevent duplicate scoring for the same user/day.
- Return correct/incorrect result after submission.
- Return explanation only after answer submission.

### Get User Streak

```http
GET /api/v1/me/streak
```

Purpose: return current streak, longest streak, and last completed date.

### Get Daily Discussion

```http
GET /api/v1/daily-majlis/{dailyMajlisId}/discussion
```

Purpose: return moderated visible responses for the day.

### Add Discussion Response

```http
POST /api/v1/daily-majlis/{dailyMajlisId}/discussion/responses
```

Purpose: let users respond to the daily discussion prompt.

Must support reporting/moderation before public community scaling.

## Error Response Standard

Use a consistent error shape:

```json
{
  "code": "dailyMajlis.notFound",
  "message": "Daily Majlis was not found.",
  "traceId": "server-trace-id"
}
```

## API Change Rules

- Additive changes are preferred.
- Breaking changes require updating Flutter client contracts and docs.
- Any new endpoint must include at least one test.
- Any game-rule endpoint must include negative tests.
