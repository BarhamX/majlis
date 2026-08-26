# Majlis Technology Stack

## Product Target

Android-first mobile app with Flutter frontend and .NET backend.

## Mobile

- Flutter stable channel.
- Dart stable channel matched to Flutter release.
- Android MVP first.
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
- JWT authentication or hosted identity provider integration.

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

Recommended MVP options:

- API hosting: Azure App Service, Azure Container Apps, Railway, Render, or Fly.io.
- Database: Managed PostgreSQL.
- File/media storage: S3-compatible storage or Azure Blob Storage later.
- Push notifications: Firebase Cloud Messaging.
- Analytics: Firebase Analytics, PostHog, or Application Insights plus product events.

## Admin

MVP admin can start as protected API endpoints plus a simple internal web UI later.

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
- Testcontainers for integration tests later.
- OpenAPI/Swagger.
- Serilog or structured logging.

### Flutter

- flutter_test.
- mocktail.
- golden tests later for critical share cards.
- integration_test for the daily loop.

## Versioning

- API route prefix: `/api/v1`.
- Spec folders are numbered: `specs/001-feature-name`.
- Database migrations must be committed and reviewed.

## Security Baseline

- HTTPS only.
- Secure token storage on mobile.
- Least-privilege admin roles.
- Input validation on all community content.
- Rate limiting on auth, comment, report, and answer submission endpoints.
