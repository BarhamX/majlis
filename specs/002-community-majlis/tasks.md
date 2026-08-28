# Tasks 002: Community Majlis

## Backend Domain

- [ ] Create `DiscussionComment` entity.
- [ ] Create immutable `CommentRevision` entity.
- [ ] Create `CommentReaction` entity.
- [ ] Create `CommentReport` entity.
- [ ] Create `UserBlock` entity.
- [ ] Add comment status enum/value object.
- [ ] Add report reason enum/value object.

## Backend Use Cases

- [ ] Implement submit comment command.
- [ ] Implement edit/delete comment commands with pending revision behavior.
- [ ] Implement list visible comments query.
- [ ] Implement react to comment command.
- [ ] Implement report comment command.
- [ ] Implement block/unblock command.
- [ ] Implement eligibility, filtering, cursor pagination, idempotency, and rate limits.

## Backend API

- [ ] Add `GET /api/v1/daily-majlis/{dailyMajlisId}/comments`.
- [ ] Add `POST /api/v1/daily-majlis/{dailyMajlisId}/comments`.
- [ ] Add `POST /api/v1/comments/{commentId}/reactions`.
- [ ] Add `DELETE /api/v1/comments/{commentId}/reactions/{type}`.
- [ ] Add `POST /api/v1/comments/{commentId}/report`.
- [ ] Add edit/delete comment and block/unblock endpoints.

## Backend Tests

- [ ] Test comment submission.
- [ ] Test visible comment listing excludes hidden comments.
- [ ] Test duplicate reaction prevention.
- [ ] Test report creation.
- [ ] Test pending/edit/delete/suspension/block filters cannot leak through list or counts.
- [ ] Test completion eligibility, ownership, idempotency, cursor stability, and rate limits.

## Flutter

- [ ] Create Discussion feature folder.
- [ ] Create Discussion screen.
- [ ] Create comment input component.
- [ ] Create comment list component.
- [ ] Create reaction UI.
- [ ] Create report flow.
- [ ] Add empty/loading/error states.

## Validation

- [ ] Run backend tests.
- [ ] Run Flutter analyzer.
- [ ] Run Flutter tests.
- [ ] Map all `COM-*` requirements in `docs/quality/requirements-to-tests.md`.
- [ ] Update `docs/ai-context/HANDOFF.md`.
