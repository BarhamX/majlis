# AI Context: Conventions

## Naming

- Product name: Majlis.
- Daily content aggregate: DailyMajlis.
- Challenge attempt: UserAttempt.
- Community response: DiscussionComment.

## API

- Prefix routes with `/api/v1`.
- Use JSON.
- Do not expose correct option before answer submission.
- Require an authenticated local `User` mapped from a validated OIDC subject for user-state endpoints.
- Use BCP 47 locale tags, `Content-Language`, RFC 7807 problem details, UTC timestamps, and UTC `PublishDate` values.
- Require an `Idempotency-Key` UUID for mutation endpoints whose duplicate execution can award progress or create public content.

## C#

- Use PascalCase for public types/members.
- Use nullable reference types.
- Keep controllers thin.
- Put use cases in Application services.

## Dart/Flutter

- Use feature-first folders.
- Use Riverpod providers for state.
- Keep UI copy centralized when possible.
- Keep theme tokens centralized.

## Product Copy

- Use playful challenge language.
- Avoid shaming.
- Keep explanations short.
- Use culturally respectful framing.

## Tests

- Backend domain/application logic requires unit tests.
- Challenge scoring and streak logic require tests before release.
- API endpoints require PostgreSQL-backed integration tests for authorization, isolation, persistence, uniqueness, and response safety.
- Requirement IDs in feature specs must be mapped in `docs/quality/requirements-to-tests.md` before implementation is marked complete.
