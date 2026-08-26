# Tasks 002: Community Majlis

## Backend Domain

- [ ] Create `DiscussionComment` entity.
- [ ] Create `CommentReaction` entity.
- [ ] Create `CommentReport` entity.
- [ ] Add comment status enum/value object.
- [ ] Add report reason enum/value object.

## Backend Use Cases

- [ ] Implement submit comment command.
- [ ] Implement list visible comments query.
- [ ] Implement react to comment command.
- [ ] Implement report comment command.
- [ ] Implement hide comment command for admin/moderation later.

## Backend API

- [ ] Add `GET /api/v1/daily-majlis/{dailyMajlisId}/comments`.
- [ ] Add `POST /api/v1/daily-majlis/{dailyMajlisId}/comments`.
- [ ] Add `POST /api/v1/comments/{commentId}/reactions`.
- [ ] Add `POST /api/v1/comments/{commentId}/report`.

## Backend Tests

- [ ] Test comment submission.
- [ ] Test visible comment listing excludes hidden comments.
- [ ] Test duplicate reaction prevention.
- [ ] Test report creation.

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
- [ ] Update `docs/ai-context/HANDOFF.md`.
