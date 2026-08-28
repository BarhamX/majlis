# Plan 008: Content and Moderation Administration

## Delivery Order

1. Add immutable content revisions, translation completeness, editorial transitions, and audit persistence.
2. Add moderation actions, blocks, appeals, and report uniqueness/priority.
3. Implement Application commands/queries with role and separation-of-duty policies.
4. Expose protected admin APIs and a browser-based internal interface.
5. Add the scheduler, 14-day coverage view, correction flow, and queue metrics.
6. Verify consumer data exclusion and operator end-to-end journeys.

## Architecture

- Admin UI calls the same Application use cases as APIs; it never accesses DbContext directly.
- State transitions live in Domain/Application and are transactionally persisted with audit events.
- Scheduler execution is idempotent and guarded by database uniqueness.
- Consumer DTOs are separate from admin DTOs.

## Validation

- Domain tests for every editorial and moderation transition.
- PostgreSQL integration tests for uniqueness, concurrency, authorization, audit atomicity, and consumer filtering.
- Browser end-to-end tests for editor/reviewer/publisher/moderator journeys.
- Security tests for stored XSS, forged roles, IDOR, and sensitive-data leakage.
