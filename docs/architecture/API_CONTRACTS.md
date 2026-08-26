# Majlis API Contracts

Base path: `/api/v1`

This file currently defines the initial contracts. It must be expanded and versioned for every Production V1 capability in `specs/003-production-app/`; an absent contract here does not remove that capability from the release scope.

## Auth

### POST `/auth/register`

Request:

```json
{
  "email": "user@example.com",
  "password": "StrongPassword123!",
  "displayName": "Omar",
  "region": "gulf"
}
```

Response:

```json
{
  "userId": "uuid",
  "displayName": "Omar",
  "token": "jwt"
}
```

### POST `/auth/login`

Request:

```json
{
  "email": "user@example.com",
  "password": "StrongPassword123!"
}
```

Response:

```json
{
  "userId": "uuid",
  "displayName": "Omar",
  "token": "jwt"
}
```

## Daily Majlis

Daily Majlis dates use the canonical UTC calendar date. Country-derived UTC offset metadata is deferred until authenticated profiles provide an ISO country code.

### GET `/daily-majlis/today`

Response:

```json
{
  "dailyMajlisId": "uuid",
  "date": "2026-08-26",
  "title": "The Guest Before the House",
  "topic": "hospitality",
  "challenge": {
    "id": "uuid",
    "questionText": "What does this proverb mean?",
    "type": "multipleChoice",
    "difficulty": "easy",
    "region": "panArab",
    "options": [
      { "id": "uuid", "text": "A guest is honored as a trust" },
      { "id": "uuid", "text": "A guest should not stay long" }
    ]
  },
  "discussionQuestion": "What is one hospitality habit your family still practices?",
  "userState": {
    "hasAttempted": false,
    "currentStreak": 0
  }
}
```

## Challenge Attempts

### POST `/challenges/{challengeId}/attempts`

Request:

```json
{
  "selectedOptionId": "uuid"
}
```

Response:

```json
{
  "attemptId": "uuid",
  "isCorrect": true,
  "correctOptionId": "uuid",
  "explanation": "This proverb reflects hospitality as honor and responsibility.",
  "xpAwarded": 10,
  "streak": {
    "current": 1,
    "longest": 1,
    "updated": true
  },
  "share": {
    "title": "I completed today's Majlis",
    "spoilerSafeText": "I scored today's cultural challenge. Can you?"
  }
}
```

## Discussion

### GET `/daily-majlis/{dailyMajlisId}/comments`

Response:

```json
{
  "items": [
    {
      "id": "uuid",
      "displayName": "Mariam",
      "text": "My family always starts with coffee before conversation.",
      "reactionCount": 4,
      "createdAt": "2026-08-26T10:00:00Z"
    }
  ]
}
```

### POST `/daily-majlis/{dailyMajlisId}/comments`

Request:

```json
{
  "text": "My family still uses this proverb."
}
```

Response:

```json
{
  "id": "uuid",
  "status": "visible"
}
```

### POST `/comments/{commentId}/report`

Request:

```json
{
  "reason": "abusive_or_disrespectful"
}
```

Response:

```json
{
  "reportId": "uuid",
  "status": "received"
}
```

## Admin Content

### POST `/admin/daily-majlis`

Request:

```json
{
  "date": "2026-08-27",
  "title": "A Proverb About Patience",
  "topic": "patience",
  "challengeId": "uuid",
  "storyId": "uuid",
  "proverbId": "uuid",
  "discussionQuestion": "When did patience help you?"
}
```

Response:

```json
{
  "dailyMajlisId": "uuid",
  "status": "scheduled"
}
```
