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
- API endpoints require integration tests when infrastructure is ready.
