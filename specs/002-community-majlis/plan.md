# Plan 002: Community Majlis

## Architecture

Discussion belongs to backend Application and Domain. Flutter uses Discussion feature screens and providers. Moderation status is part of the data model from the first community release.

## Backend Work

1. Add DiscussionComment entity.
2. Add CommentReaction entity.
3. Add CommentReport entity.
4. Add comment submit/list/report APIs.
5. Add status filtering.
6. Add duplicate reaction protection.
7. Add tests.

## Flutter Work

1. Add Discussion screen.
2. Add response input.
3. Add comment list.
4. Add reaction action.
5. Add report action.
6. Add empty states.

## Safety Work

1. Add community rules copy.
2. Add report reasons.
3. Add admin moderation-ready data model.
