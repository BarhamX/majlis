# Spec 009: Production Operations and Release Quality

## Goal

Define objective release gates for performance, accessibility, compatibility, reliability, security, privacy, analytics, deployment, backup, and incident recovery.

## Environment Requirements

- **OPS-001**: Local, test, staging, and production shall use documented configuration schemas. After `Game Ready`, staging shall run the same deployable API artifact, migration set, Android release flavor, TLS behavior, Google/Apple identity integration, and PostgreSQL major version as production, with isolated secrets and data.
- **OPS-002**: Production migrations shall run as an explicit reviewed deployment step with a pre-deploy backup and forward/rollback instructions. The API shall not auto-migrate or seed production.
- **OPS-003**: Secrets shall come from an environment secret store, never source, images, logs, analytics, mobile assets, or checked-in configuration.
- **OPS-004**: Liveness shall verify the process only; readiness shall verify required database connectivity and migration compatibility without leaking dependency details publicly.

## Performance and Capacity Gates

- **OPS-005**: At 50 requests/second for 15 minutes against a production-shaped staging dataset, read APIs shall achieve p95 <= 500 ms and p99 <= 1,500 ms; authenticated mutation APIs shall achieve p95 <= 750 ms and p99 <= 2,000 ms; unexpected 5xx responses shall be < 0.5%.
- **OPS-006**: On the mid-tier reference device over 10 Mbps bandwidth and 100 ms round-trip latency, Android cold start to interactive shall be <= 3.0 seconds at p75 and today's uncached challenge shall render <= 2.0 seconds after navigation at p75 across 20 measured runs.
- **OPS-007**: Critical Android journeys shall have < 5% slow frames and < 1% frozen frames under Android vitals definitions; share-card generation shall complete <= 2.0 seconds at p95.
- **OPS-008**: A release load test shall include at least 100,000 users, 365 Daily Majlis records, 10 million attempts, 1 million comments, and a 100-entry leaderboard response plan without unbounded queries.

## Android and Accessibility Gates

- **OPS-009**: Production V1 shall support Android 10/API 29 and later, and target at least the Google Play-required target SDK in effect at submission.
- **OPS-010**: The device matrix shall include: API 29 with 3 GB RAM and a 360x640 dp viewport; API 33-34 mid-tier hardware with 6 GB RAM; and the latest stable Android/API on a current flagship. Core journeys shall pass on every row.
- **OPS-011**: Core screens shall meet WCAG 2.2 AA: 4.5:1 normal-text contrast, 3:1 large text/non-text controls, visible focus, semantic labels, logical TalkBack order, 48x48 dp touch targets, no color-only meaning, and usable layout at 200% text scaling.
- **OPS-012**: Every core journey and share card shall pass Arabic RTL tests, mixed-script tests, longest approved content fixtures, and device font-scale tests without clipping or unreachable actions.

## Reliability and Recovery Gates

- **OPS-013**: The production API availability objective shall be 99.5% per calendar month, excluding announced maintenance; alerting shall fire on a 5-minute availability burn and on missing current/next-day content.
- **OPS-014**: PostgreSQL shall use automated daily full backups plus continuous write-ahead-log recovery or an equivalent service. The release target is RPO <= 15 minutes and RTO <= 4 hours.
- **OPS-015**: A staging restore drill from an encrypted production-shaped backup shall pass before launch and quarterly thereafter. Evidence shall include backup id/time, restore duration, integrity checks, application smoke tests, and deletion-tombstone reapplication.
- **OPS-016**: Deployment shall use immutable versioned artifacts, database compatibility checks, smoke tests, and a documented rollback. Rollback shall not discard accepted attempts, comments, reports, or audit events.
- **OPS-017**: UTC publishing shall be monitored. The next 14 content days shall show coverage status, and the current day's absence or publication failure shall page the operator.

## Security and Privacy Gates

- **OPS-018**: Threat modeling shall cover identity, authorization, spoiler data, admin operations, stored UGC, deep links, analytics, and backups. Release shall have no unresolved critical/high finding and a documented owner/date for every accepted medium risk.
- **OPS-019**: CI shall run secret scanning, dependency vulnerability scanning, static analysis, tests, and migration drift checks. Release dependencies shall have no known critical/high vulnerability without documented time-bound exception.
- **OPS-020**: The API shall enforce HTTPS, HSTS in production, restrictive CORS, request-size limits, safe headers, endpoint-specific rate limits, and RFC 7807 errors with a correlation id but no stack trace, token, connection string, or personal data.
- **OPS-021**: Authorization/IDOR, stored-XSS, injection, token validation, replay/idempotency, deep-link validation, and consumer/admin DTO separation shall have automated security tests.
- **OPS-022**: Logs, traces, metrics, crash reports, and analytics shall exclude access/refresh tokens, email, comment text, selected answers, correct answers, internal source notes, and precise location. User identifiers shall be pseudonymous.
- **OPS-023**: Account deletion, log retention, analytics retention, moderation retention, and backup expiry shall be exercised in staging against `V1-DEC-009` before release.

## Analytics Gates

- **OPS-024**: The approved product event catalog is: `onboarding_completed`, `challenge_viewed`, `attempt_completed`, `result_shared`, `discussion_opened`, `comment_submitted`, `reminder_enabled`, `reminder_opened`, and `leaderboard_opened`. Operational events are separate from product analytics.
- **OPS-025**: Product events shall include event version, UTC timestamp, app version, platform, locale, and pseudonymous installation/user id where consent permits. They shall not include free text, option ids/text, correctness explanations, source notes, age band, email, or region/dialect preference.
- **OPS-026**: Analytics shall honor the configured consent policy, support deletion by pseudonymous user key, document sampling and metric definitions, and be verified in staging without production credentials.

## Release Evidence

- **OPS-027**: CI shall pass backend unit/integration/contract suites; Flutter analyze/unit/widget/golden/integration suites; migration drift; security scans; and link/Arabic/accessibility checks on the candidate commit.
- **OPS-028**: The signed Android App Bundle shall be reproducible from the candidate commit, install on every device-matrix row, use production signing outside source control, and pass Play pre-launch checks with no blocking crash or policy issue.
- **OPS-029**: Google and Apple new-user/returning-user, linked-identity, expired-session, deletion, publisher, moderator, backup/restore, deployment, and rollback journeys shall pass in staging with timestamps and artifact versions recorded.
- **OPS-030**: Release approval requires a completed `specs/003-production-app/tasks.md`, mapped evidence in `docs/quality/requirements-to-tests.md`, no open release blocker in the handoff, and named product, editorial, security/privacy, engineering, and operations approvers.

## Service-Level Indicators

- API availability and unexpected 5xx rate.
- Endpoint latency by route template and status class.
- Daily content coverage and publish success.
- Oldest pending comment/report/appeal age.
- Backup age and last verified restore.
- Android crash-free users, ANR rate, slow/frozen frames, and core-flow completion.

Metric labels shall remain bounded and contain no user-generated content or personal data.
