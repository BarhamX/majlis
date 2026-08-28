# Tasks 008: Content and Moderation Administration

## Content Workflow

- [ ] Add immutable content revision, translation, review, publication, correction, and audit schema.
- [ ] Implement validation and transitions for `ADM-002` through `ADM-010`.
- [ ] Implement idempotent UTC scheduler and 14-day coverage/conflict checks.
- [ ] Build the protected browser-based editor/reviewer/publisher interface.

## Moderation Workflow

- [ ] Add moderation action, block, appeal, and audit schema.
- [ ] Implement pending-comment approval and report priority/uniqueness.
- [ ] Implement hide, restore, remove, block, suspension, and appeal use cases.
- [ ] Build the protected moderation queue and appeal interface.
- [ ] Implement and test the audited discussion availability control.

## Verification

- [ ] Pass role/separation-of-duty, MFA-policy, IDOR, concurrency, XSS, and data-leak tests.
- [ ] Pass end-to-end publisher and moderator journeys without code/database access.
- [ ] Verify every `ADM-*` and `MOD-*` requirement and update traceability.
- [ ] Update the handoff.
