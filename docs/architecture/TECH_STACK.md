# Majlis Technology Stack

## Product Target

Android-first mobile app with Flutter frontend and .NET backend.

## Mobile

- Flutter stable channel.
- Dart stable channel matched to Flutter release.
- Android production release first.
- Riverpod for state management.
- GoRouter for navigation.
- Dio or generated client for API calls.
- Freezed/json_serializable when data contracts stabilize.

## Backend

- .NET 10 LTS.
- ASP.NET Core Web API.
- Entity Framework Core.
- PostgreSQL.
- Clean architecture solution structure.
- Google Account through Android Credential Manager and Sign in with Apple through the system browser, with state/nonce validation and PKCE where supported; provider adapters share one application boundary and Majlis stores no passwords.
- A deterministic signed identity issuer is allowed only in Development/Testing and is rejected by Production startup.

## Backend Solution Structure

```text
src/backend/
  Majlis.Api/
  Majlis.Application/
  Majlis.Domain/
  Majlis.Infrastructure/
  Majlis.Contracts/
  Majlis.Tests/
```

## Infrastructure

Production vendor selection and provisioning are deliberately deferred until the `Game Ready` milestone. Candidate options, not current decisions:

- API hosting: Azure App Service, Azure Container Apps, Railway, Render, or Fly.io.
- Database: Managed PostgreSQL.
- File/media storage: S3-compatible storage or Azure Blob Storage later.
- V1 reminders: Android local notifications. Firebase Cloud Messaging requires a later remote-notification specification.
- Analytics: Firebase Analytics, PostHog, or Application Insights plus product events.

## Admin

Production V1 requires a protected browser-based admin interface backed by the same Application use cases as the API. Raw endpoints are not the sole operating interface.

Admin capabilities:

- Create content.
- Schedule Daily Majlis.
- Publish/unpublish.
- Review reports.
- Inspect analytics.

## Quality Tools

### Backend

- xUnit.
- FluentAssertions.
- Testcontainers for database integration tests.
- OpenAPI/Swagger.
- Serilog or structured logging.

### Flutter

- flutter_test.
- mocktail.
- Golden tests for critical share cards.
- integration_test for the daily loop.

## Versioning

- API route prefix: `/api/v1`.
- Spec folders are numbered: `specs/<sequence>-<feature-name>`.
- Database migrations must be committed and reviewed.

## Security Baseline

- HTTPS only.
- Secure token storage on mobile.
- Least-privilege admin roles.
- Input validation on all community content.
- Rate limiting on auth, comment, report, and answer submission endpoints.
- MFA for privileged roles, immutable audits, and separation of editorial duties.
- OWASP ASVS Level 2 and OWASP MASVS baseline verification as defined in Spec 009.
