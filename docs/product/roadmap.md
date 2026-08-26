# Majlis Delivery Roadmap

## Delivery Principle

Majlis is delivered as one complete production Android app. The phases below sequence implementation and validation; none of them is the final product by itself.

## Phase 0: Product and Engineering Foundation

- Product, business, safety, UX, and architecture specifications.
- Theme and content-voice systems.
- Clean-architecture backend skeleton.
- Release-wide acceptance criteria.

## Phase 1: Persistent Backend Foundation

- PostgreSQL and EF Core.
- Explicit migrations and repeatable seed/initialization flow.
- Health checks, configuration validation, logging, and integration-test infrastructure.
- Published Daily Majlis query backed by persistent data.

## Phase 2: Accounts and Daily Game Domain

- Registration, login, session renewal, and profile/preferences.
- Answer submission and exactly-once scoring.
- XP, streak, attempt history, and spoiler-safe result generation.
- Authorization and rate limiting for user actions.

## Phase 3: Complete Android Experience

- Flutter foundation, routing, Riverpod state, theme, and centralized copy.
- Onboarding, authentication, Today, challenge, result, cultural card, leaderboard, and profile flows.
- Native sharing, deep links, notifications, and all loading/error/offline/completed states.
- Widget, integration, accessibility, and representative-device checks.

## Phase 4: Community and Safety

- Daily discussion, comments, reactions, and reporting.
- Moderation statuses, visible-content filtering, rate limits, and moderation queue.
- Community rules and privacy-safe leaderboard/social behavior.

## Phase 5: Content and Operations

- Authenticated content and moderation administration.
- Source notes, regional/dialect tags, editorial review, scheduling, publishing, correction, and unpublishing.
- Product analytics and operational dashboards or equivalent protected tooling.

## Phase 6: Production Release Readiness

- End-to-end validation of every release journey.
- Security, privacy, backup/restore, observability, and failure-recovery checks.
- Deployable backend and database configuration.
- Reproducible signed Android release build and release runbook.
- Production content and moderation readiness.

## Production V1 Release Gate

Production V1 ships only after Phases 1-6 and `specs/003-production-app/tasks.md` are complete. Internal alpha and beta builds are validation channels, not reduced product definitions.

## Post-V1 Evolution

- iOS and web clients.
- Advanced audio storytelling and deeper archives.
- Expanded family/private-group experiences and regional programs.
- Premium content, sponsorships, and institutional products.
- Pan-Arab editorial partnerships and localization refinement.
