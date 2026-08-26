# Majlis Full App Scope

## Release Name

Majlis Production V1 for Android.

## Release Outcome

Deliver a complete, running product that users, content editors, and moderators can use end to end. This is not a prototype, demo, backend-only milestone, or reduced release.

## Product Scope

### Account and Profile

- Registration, login, logout, session renewal, and account recovery.
- Display name, avatar or initials, region/dialect preference, and notification preferences.
- Private-by-default account and activity data.

### Daily Cultural Experience

- Backend-published Daily Majlis selected using the configured content timezone.
- Multiple-choice cultural challenge with no pre-submission answer leakage.
- Server-side answer validation, result, concise explanation, proverb or story context, and source traceability.
- Persistent attempt history with exactly one scored attempt per user and challenge.
- Safe loading, empty, offline, error, already-completed, and retry states.

### Progress and Friendly Competition

- XP, current streak, longest streak, and duplicate-award protection.
- Profile progress and a basic privacy-safe leaderboard.
- Friendly, non-shaming comparison language.

### Sharing and Return Loop

- Branded spoiler-safe result and cultural cards.
- Native Android sharing and inbound deep-link routing.
- User-controlled daily reminders with no manipulative notification loops.

### Community and Safety

- Daily discussion question, comment submission/listing, and reactions.
- Reporting, moderation status, moderator review, and hidden-content filtering.
- Community rules, rate limits, and privacy-safe defaults.

### Content and Moderation Operations

- Authenticated admin workflow to create, review, schedule, publish, unpublish, and correct Daily Majlis content.
- Required source notes, regional/dialect tags, difficulty, and editorial status.
- Moderation queue for reports and auditable moderation actions.
- Content changes without a mobile app release.

### Production Platform

- .NET 10 API with PostgreSQL/EF Core persistence and explicit migrations.
- Versioned API contracts, authentication/authorization, validation, health checks, structured logging, rate limiting, and safe error responses.
- Flutter Android client using Riverpod, feature-first organization, centralized theme/copy, and Arabic-ready layout.
- Analytics for activation, completion, retention, sharing, and moderation operations.
- Repeatable local setup, automated tests, deployable backend configuration, and a verified Android release build.

## Full-App Definition of Done

The production release is complete only when:

1. A new user can install the Android build, create an account, and complete the full daily journey without developer intervention.
2. Attempts, progress, discussion, reports, and content survive service restarts in PostgreSQL.
3. An authorized operator can publish the next Daily Majlis and moderate reports without editing code or the database manually.
4. Core privacy, authorization, spoiler-safety, duplicate-award, moderation, and content-publishing rules have automated coverage.
5. The app has usable loading, empty, offline, validation, and failure recovery states.
6. Backend and Android release procedures are documented and reproducible, with secrets kept outside source control.
7. The end-to-end release checklist in `specs/003-production-app/tasks.md` is complete.

## Post-V1 Expansion

The following are separate products or later expansions, not missing pieces of the Android V1:

- iOS and web clients.
- Full course marketplace and institutional dashboards.
- Paid subscriptions and premium content packs.
- Advanced narrated audio experiences.
- AI-assisted cultural-content generation.
- Broad public social-network features outside the moderated daily Majlis.

## Delivery Rule

Feature specs may intentionally defer work to another feature spec, but they may not redefine the project as a partial product. The team may release internal builds during development; the named Production V1 is not delivered until the full-app definition of done is met.
