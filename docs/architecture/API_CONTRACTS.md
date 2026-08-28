# Majlis API Contracts

## Contract Rules

- Base path: `/api/v1`.
- JSON uses UTF-8 and camelCase. Times are RFC 3339 UTC instants; content dates are ISO `yyyy-MM-dd` UTC dates.
- Consumer endpoints require `Authorization: Bearer <access-token>` unless explicitly public. Production accepts only configured Google, Apple, Meta, or Snapchat identity paths; Majlis has no email/password login API.
- Localized endpoints accept `Accept-Language`, return `Content-Language`, and fall back from a regional Arabic tag to required `ar`.
- Except for profile bootstrap, mutation endpoints marked `(idempotent)` require `Idempotency-Key: <uuid>`. Profile bootstrap is naturally idempotent by external identity and does not require this header.
- Idempotency records remain replayable for at least 24 hours; persistent business uniqueness continues to prevent duplicate resources after that window.
- Errors use `application/problem+json` with `type`, `title`, `status`, `code`, `traceId`, and optional field `errors`. They never include stack traces or secrets.
- Cursor collections return `items` and nullable `nextCursor`; clients must treat cursors as opaque.
- Unknown JSON fields are ignored for compatible additions. Removing/renaming a field or changing semantics requires a new API version.

### Rate-Limit Baseline

| Route class | Per account | Per IP |
|---|---:|---:|
| Consumer reads | 120/minute | 300/minute |
| Profile/bootstrap mutations | 30/minute | 120/minute |
| Attempt submission | 10/minute | 60/minute |
| Comment submission/edit | 5/hour | 30/hour |
| Reaction mutation | 60/minute | 120/minute |
| Report/appeal submission | 10/day | 30/day |
| Account deletion request | 3/day | 10/day |
| Admin mutations | 60/minute | 120/minute |

Limits may be lowered during abuse response but not raised for release without security review. A limited response uses `429`, `Retry-After`, and `rate_limit_exceeded`. Identity-provider authentication/recovery routes additionally use provider protections verified in staging.

## Authentication and Profile

Production V1 supports Google Account, Sign in with Apple, Meta/Facebook Login, and Snapchat Login Kit. Android uses Google Credential Manager or provider-approved native/system-browser flows with required state/nonce validation and PKCE where supported. Providers own account verification/recovery; Majlis maps the validated provider/issuer/subject to a local user. A signed test issuer exists only in Development/Testing and Production startup rejects it.

### POST `/dev/auth/token` (Development/Testing only)

Accepts `{ "subject": "local-user" }` and returns an ephemeral signed bearer token with `provider: test`. The route and signing service are unavailable outside Development/Testing, and Production startup fails if test authentication is configured. This endpoint exists only to unblock local game and automated test work; it is not a production login option.

### POST `/me/bootstrap` (naturally idempotent)

Creates or locates the local user for the validated provider/issuer/subject.

Request:

```json
{
  "displayName": "مريم",
  "ageBand": "18_plus",
  "countryCode": "QA",
  "regionCode": "gulf",
  "dialectCode": "qa",
  "locale": "ar",
  "acceptedTermsVersion": "2026-08-26",
  "acceptedPrivacyVersion": "2026-08-26"
}
```

Response `201` on creation or `200` on replay:

```json
{
  "userId": "uuid",
  "profileComplete": true,
  "displayName": "مريم",
  "ageBand": "18_plus",
  "countryCode": "QA",
  "regionCode": "gulf",
  "dialectCode": "qa",
  "locale": "ar",
  "leaderboardVisibility": "private",
  "preferences": {
    "reminderEnabled": false,
    "reminderLocalTime": null,
    "reminderTimeZoneId": null,
    "analyticsConsent": false
  },
  "linkedProviders": ["google"],
  "createdAt": "2026-08-26T10:00:00Z",
  "updatedAt": "2026-08-26T10:00:00Z"
}
```

Declaring `under_13` returns `422 age_not_eligible` and does not create a user.

### GET/POST/DELETE `/me/identities`

- `GET /me/identities` returns linked provider names (`google`, `apple`, `meta`, and/or `snapchat`) and link dates; it never returns provider subject, email, or credential material.
- `POST /me/identities` requires a fresh authenticated Majlis session and a short-lived provider authorization result. Google accepts its ID token/nonce; Apple accepts its authorization code, identity token, state, and nonce; Meta and Snapchat accept only the artifacts required by their approved Login Kit flows. Provider-specific request variants are finalized with their adapters after `Game Ready`; all are validated/exchanged server-side before linking.
- `DELETE /me/identities/{provider}` requires recent authentication, is idempotent, and rejects removal with `409 last_identity_required` when it would leave no login identity.

The same provider identity cannot belong to two users. Email equality never links or merges accounts. Provider authorization results are secrets: they are accepted only over TLS and excluded from logs, errors, traces, and analytics. Any provider-required revocation handle retained from the exchange is encrypted with a managed key and used only for unlink/deletion revocation.

### GET/PUT `/me/profile`

`GET` returns the authenticated user's private profile and preferences. `PUT` accepts:

```json
{
  "displayName": "مريم",
  "ageBand": "18_plus",
  "countryCode": "QA",
  "regionCode": "gulf",
  "dialectCode": "qa",
  "locale": "ar",
  "leaderboardVisibility": "global_weekly"
}
```

The response is the updated profile. A minor leaderboard opt-in returns `422 leaderboard_age_ineligible`.

### POST `/me/sessions/revoke-all`

Sets the local authentication-not-before instant so previously issued provider credentials are rejected, clears device credentials, and performs provider grant revocation when required/supported. Returns `204`. A later explicit sign-in through a supported provider may authenticate again unless the Majlis account is suspended or deletion-pending.

### POST `/me/deletion-requests`

```json
{ "confirmation": "delete_my_account" }
```

Returns `202` with `requestId`, `requestedAt`, and `purgeDueAt`. Access is revoked immediately. After `Game Ready`, the public web deletion page shall authenticate through a linked supported provider and call this same endpoint; it shall not collect an email to locate an account. A user who cannot access a linked provider is directed to provider recovery and a documented support verification process.

## Daily Majlis and Progress

### GET `/daily-majlis/today`

Pre-attempt response:

```json
{
  "dailyMajlisId": "uuid",
  "publishDate": "2026-08-26",
  "title": "الضيف قبل البيت",
  "topicCode": "hospitality",
  "challenge": {
    "id": "uuid",
    "question": "ما معنى هذا المثل؟",
    "type": "multiple_choice",
    "difficulty": "easy",
    "regionCode": "gulf",
    "options": [
      { "id": "uuid", "text": "إكرام الضيف أمانة" },
      { "id": "uuid", "text": "لا ينبغي للضيف أن يطيل" }
    ]
  },
  "discussionPrompt": "ما عادة الضيافة التي ما زالت أسرتك تحافظ عليها؟",
  "userState": {
    "hasAttempted": false,
    "attemptId": null
  }
}
```

The response never contains correctness, explanation, internal sources, review status, or answer-derived statistics. If already attempted, `hasAttempted` is true and `attemptId` is populated; result data is retrieved from the attempt endpoint. Missing content returns `404 daily_majlis_unavailable`.

### POST `/challenges/{challengeId}/attempts` (idempotent)

A new attempt is accepted only when `challengeId` is the challenge in the one Daily Majlis whose status is currently `published` and whose `PublishDate` is the current UTC date, and `selectedOptionId` belongs to that challenge. Historical and future challenges, superseded correction revisions, and challenges belonging to scheduled, draft, or unpublished Daily Majlis content never accept a new attempt. This restriction does not affect a same-key replay or an owned read of an already accepted attempt.

Request:

```json
{ "selectedOptionId": "uuid" }
```

First accepted response `201`:

```json
{
  "attemptId": "uuid",
  "dailyMajlisId": "uuid",
  "publishDate": "2026-08-26",
  "isCorrect": true,
  "correctOptionId": "uuid",
  "explanation": "يعكس المثل مكانة إكرام الضيف بوصفه شرفاً ومسؤولية.",
  "culturalCard": {
    "type": "proverb",
    "title": "الضيف قبل البيت",
    "text": "نص المثل أو القصة القصيرة.",
    "meaning": "المعنى المختصر.",
    "context": "سياق ثقافي موجز.",
    "publicAttribution": null
  },
  "xp": {
    "completion": 10,
    "correctness": 5,
    "awarded": 15,
    "lifetimeTotal": 115
  },
  "streak": {
    "current": 3,
    "longest": 5,
    "updated": true
  },
  "contentRevisionId": "uuid",
  "resultLocale": "ar"
}
```

`resultLocale` is the negotiated BCP 47 locale stored when the first attempt is accepted. `xp.lifetimeTotal`, `streak.current`, and `streak.longest` are the exact post-award snapshots stored with the attempt, not recalculated from later progress. The explanation and cultural card are served from the immutable stored `contentRevisionId` in that stored locale. Same key/payload returns `200` with the same body. Same key/different payload returns `409 idempotency_key_reused`. A later key after completion returns `409 attempt_already_completed` with extension `attemptId`. Option/challenge mismatch returns `422 option_not_in_challenge`.

### GET `/attempts/{attemptId}`

Returns the same stored result contract only when owned by the caller. A missing or non-owned attempt always returns the same non-enumerating `404 attempt_not_found`; the response must not distinguish those cases. Result retrieval remains available after the content day, a correction, or unpublishing and never changes the accepted option, correctness, XP, streak snapshots, stored locale, or source revision.

### GET `/me/attempts?cursor=&limit=20`

Returns the user's newest-first attempt summaries with attempt id, publish date, title in the stored result locale, correctness, XP awarded, stored result locale, and content revision id. `limit` defaults to 20 and is 1-50. `cursor` is opaque and represents the final `(attemptedAt, attemptId)` boundary of the prior page under descending `attemptedAt`, then descending `attemptId` ordering; a continuation returns only rows strictly after that boundary. Attempts created after the first page that sort before its boundary do not shift, duplicate, or replace continuation items.

### GET `/me/progress`

```json
{
  "lifetimeXp": 115,
  "currentStreak": 3,
  "longestStreak": 5,
  "lastCompletedPublishDate": "2026-08-26"
}
```

## Weekly Leaderboard

### GET `/leaderboards/global/weekly`

```json
{
  "weekStartsAt": "2026-08-24T00:00:00Z",
  "weekEndsAt": "2026-08-31T00:00:00Z",
  "items": [
    { "rank": 1, "displayName": "نورة", "weeklyXp": 105 }
  ],
  "me": { "rank": 24, "displayName": "مريم", "weeklyXp": 60 }
}
```

Only opted-in adult active users are eligible. `me` is null when ineligible. The contract exposes no public user id or profile metadata.

## Sharing and Links

### GET `/attempts/{attemptId}/share`

```json
{
  "publishDate": "2026-08-26",
  "resultState": "completed",
  "title": "أكملت مجلس اليوم",
  "body": "هل تعرف الإجابة؟",
  "url": "https://<configured-host>/daily/2026-08-26",
  "imageAlt": "بطاقة مجلس خالية من حرق الإجابة"
}
```

The endpoint requires ownership and returns no correctness, answer, explanation, user identity, XP, or streak. `GET /daily/{yyyy-MM-dd}` is a public web landing route, not a JSON API; it opens the verified Android App Link or a safe current/expired/invalid fallback.

## Reminder Preference

### GET/PUT `/me/preferences/reminder`

`PUT` request:

```json
{
  "enabled": true,
  "localTime": "19:30",
  "timeZoneId": "Asia/Qatar"
}
```

Response adds `updatedAt`. The server stores preference only; Android owns local notification scheduling. Disabling, logout, or deletion requires device cancellation.

## Discussion

All discussion endpoints require completion of the referenced Daily Majlis. Consumer comment shape:

```json
{
  "id": "uuid",
  "displayName": "مريم",
  "authorActionToken": "opaque-short-lived-token",
  "text": "نبدأ دائماً بالقهوة.",
  "reactionCounts": { "like": 2, "thoughtful": 1, "coffee": 3 },
  "myReactions": ["coffee"],
  "createdAt": "2026-08-26T10:00:00Z"
}
```

### GET `/daily-majlis/{dailyMajlisId}/comments?cursor=&limit=20`

Returns newest-first visible comments only. `limit` is 1-50. Counts and items apply moderation, deletion, suspension, and block filters.

### POST `/daily-majlis/{dailyMajlisId}/comments` (idempotent)

Request `{ "text": "نبدأ دائماً بالقهوة." }`. Returns `201` with `commentId`, `revisionId`, and `status: "pending"`. An existing active response returns `409 comment_already_exists`.

### PUT/DELETE `/comments/{commentId}`

`PUT` with `{ "text": "..." }` creates a new pending revision and returns its ids/status. `DELETE` returns `204` and removes the comment from consumer results immediately.

### POST/DELETE `/comments/{commentId}/reactions/{type}` (idempotent)

Allowed types are `like`, `thoughtful`, and `coffee`. Add returns `200` with updated counts; delete returns `204`. Self, blocked, or non-visible interaction returns a safe problem code.

### POST `/comments/{commentId}/reports` (idempotent)

```json
{
  "reason": "abusive_or_disrespectful",
  "detail": "Optional plain-text context."
}
```

Returns `202` with `reportId` and `status: "received"`. A duplicate active report returns the existing receipt without exposing other reports.

### GET/POST/DELETE `/me/blocks`

- `POST /me/blocks` is idempotent and accepts `{ "authorActionToken": "opaque-short-lived-token" }`; it returns `201` or the existing private `blockId`.
- `GET /me/blocks` returns the caller's block ids and current display names only.
- `DELETE /me/blocks/{blockId}` is idempotent and returns `204`.

The target is not notified. Consumer APIs never expose a user id; the comment DTO supplies a viewer-bound, short-lived action token. A block id is scoped to the blocker and cannot address another user's relationship.

### POST `/me/moderation-actions/{actionId}/appeals` (idempotent)

Accepts `{ "reason": "plain text up to 1000 characters" }` and returns `201` with appeal id/status. Ineligible or expired actions return `422 appeal_not_eligible`.

## Content Administration

Every route below requires the named role and returns admin DTOs separate from consumer DTOs.

- `POST /admin/daily-majlis` (`content_editor`): creates a draft aggregate and revision.
- `PUT /admin/daily-majlis/{id}/revisions/{revisionId}` (`content_editor`): edits a draft revision.
- `POST /admin/daily-majlis/{id}/revisions/{revisionId}/submit` (`content_editor`): validates and submits for review.
- `POST /admin/daily-majlis/{id}/revisions/{revisionId}/reviews` (`content_reviewer`): accepts `decision: approved|rejected` and required note on rejection; self-approval returns `403 separation_of_duties`.
- `POST /admin/daily-majlis/{id}/revisions/{revisionId}/schedule` (`publisher`): accepts UTC `publishDate`; returns `409 publish_date_conflict` for a second scheduled/published item.
- `POST /admin/daily-majlis/{id}/publish` (`publisher`): manual idempotent execution of an eligible schedule.
- `POST /admin/daily-majlis/{id}/unpublish` (`publisher`): requires a reason.
- `POST /admin/daily-majlis/{id}/corrections` (`publisher`): references an approved replacement revision and localized correction note.
- `GET /admin/content-calendar?from=&to=` (editor/reviewer/publisher): returns coverage/conflict state for at most 90 days.
- `GET /admin/audit-events?cursor=&actor=&target=&action=` (authorized operator): cursor-paginated immutable audit metadata.

Command bodies are:

- Review: `{ "decision": "approved|rejected", "note": null }` (note required on rejection).
- Schedule: `{ "publishDate": "2026-08-27" }`.
- Unpublish: `{ "reason": "Required operator reason." }`.
- Correction: `{ "replacementRevisionId": "uuid", "correctionNotes": { "ar": "تم تصحيح الإجابة وشرحها." } }`.

Each command returns:

```json
{
  "dailyMajlisId": "uuid",
  "revisionId": "uuid",
  "status": "scheduled",
  "auditEventId": "uuid",
  "updatedAt": "2026-08-26T10:00:00Z"
}
```

The content calendar returns each UTC date with `empty|draft|in_review|approved|scheduled|published|conflict` and the permitted next actions for the caller's role.

Draft/revision requests contain:

```json
{
  "topicCode": "hospitality",
  "difficulty": "easy",
  "regionCodes": ["gulf"],
  "dialectCodes": ["qa"],
  "sourceNotes": "Internal citation and verification notes.",
  "translations": {
    "ar": {
      "title": "الضيف قبل البيت",
      "question": "ما معنى هذا المثل؟",
      "options": [
        { "optionKey": "a", "text": "إكرام الضيف أمانة", "isCorrect": true },
        { "optionKey": "b", "text": "لا ينبغي للضيف أن يطيل", "isCorrect": false }
      ],
      "explanation": "شرح موجز.",
      "discussionPrompt": "ما عادة الضيافة في أسرتك؟",
      "publicAttribution": "اختياري"
    }
  }
}
```

The server enforces 2-4 options, exactly one correct option, complete Arabic, non-empty source notes, immutable submitted revisions, and role separation.

## Moderation Administration

- `GET /admin/moderation/comments?status=pending&cursor=`
- `POST /admin/moderation/comments/{commentId}/actions` with `approve|hide|restore|remove`, reason, and optional internal note.
- `GET /admin/moderation/reports?priority=&status=&cursor=`
- `POST /admin/moderation/reports/{reportId}/decision` with `dismissed|actioned` and reason.
- `GET /admin/moderation/appeals?status=pending&cursor=`
- `POST /admin/moderation/appeals/{appealId}/decision` with `accepted|rejected` and reason; the reviewer cannot be the original acting moderator.
- `POST /admin/users/{userId}/suspensions` and `DELETE /admin/users/{userId}/suspensions/{id}` for authorized moderation policy.
- `PUT/DELETE /admin/users/{userId}/roles/{role}` (`operations_admin`) with required reason and immutable audit.
- `PUT /admin/operations/features/discussion` (`operations_admin`) with `{ "mode": "premoderated|disabled", "reason": "..." }` and immutable audit.

Comment action body is `{ "action": "approve|hide|restore|remove", "reason": "...", "internalNote": null }`. Report and appeal decisions use `{ "decision": "dismissed|actioned|accepted|rejected", "reason": "..." }` as applicable. Suspension and role commands require `{ "reason": "..." }`; a suspension also requires nullable `expiresAt`. Every transition returns target id, resulting status, `actedAt`, and `auditEventId`; it never returns reporter identity or internal notes to consumer routes.

When discussion mode is `disabled`, consumer discussion reads/mutations return `503 discussion_unavailable` with localized retry guidance; Daily Majlis gameplay remains available.

## Operational Endpoints

- Public `GET /health/live`: process liveness only.
- Protected/internal `GET /health/ready`: required dependency and migration compatibility.
- OpenAPI is generated for consumer/admin JSON APIs in non-production and available to authorized operators in production.

## Stable Problem Codes

`authentication_required`, `identity_provider_not_supported`, `identity_already_linked`, `identity_link_conflict`, `last_identity_required`, `profile_incomplete`, `forbidden`, `resource_not_found`, `validation_failed`, `rate_limit_exceeded`, `daily_majlis_unavailable`, `option_not_in_challenge`, `idempotency_key_reused`, `attempt_already_completed`, `comment_already_exists`, `comment_not_visible`, `interaction_blocked`, `discussion_unavailable`, `appeal_not_eligible`, `separation_of_duties`, `publish_date_conflict`, and `content_revision_invalid`.
