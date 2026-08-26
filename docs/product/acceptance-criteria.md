# Majlis Acceptance Criteria

## Account and Profile

- WHEN a user signs up successfully, THE SYSTEM SHALL create a user profile with display name, created date, and default preferences.
- WHEN a user selects region preference, THE SYSTEM SHALL save it to the profile.
- WHEN a user is unauthenticated, THE SYSTEM SHALL allow previewing today's challenge only if preview mode is enabled.

## Daily Majlis

- WHEN the app opens, THE SYSTEM SHALL request today's published Daily Majlis for the user's date/time context.
- WHEN no Daily Majlis is published for today, THE SYSTEM SHALL return a safe fallback response instead of crashing.
- WHEN Daily Majlis content is returned, THE SYSTEM SHALL include challenge, explanation, topic, proverb/story card, and discussion question.

## Challenge Submission

- WHEN a user submits an answer for today's challenge, THE SYSTEM SHALL validate the selected option.
- WHEN the answer is correct, THE SYSTEM SHALL return correct result, explanation, XP awarded, and streak status.
- WHEN the answer is incorrect, THE SYSTEM SHALL return incorrect result, correct answer, explanation, XP awarded according to rules, and streak status.
- WHEN the user submits again for the same daily challenge, THE SYSTEM SHALL not award duplicate XP or duplicate streak progress.

## Streaks

- WHEN a user completes a daily challenge for the first time on a date, THE SYSTEM SHALL update current streak.
- WHEN a user misses a day, THE SYSTEM SHALL reset current streak unless a future streak protection rule applies.
- WHEN current streak exceeds longest streak, THE SYSTEM SHALL update longest streak.

## Discussion

- WHEN a user submits a response, THE SYSTEM SHALL store it with pending or visible status according to moderation rules.
- WHEN a response is reported, THE SYSTEM SHALL mark it for moderation review.
- WHEN a response is hidden by moderation, THE SYSTEM SHALL not show it in public discussion results.

## Sharing

- WHEN a user completes today's challenge, THE SYSTEM SHALL allow generating a result card.
- WHEN spoiler protection is active, THE SYSTEM SHALL not include the correct answer in share card text.
- WHEN a recipient opens a shared link, THE SYSTEM SHALL route them to today's Majlis or fallback landing screen.

## Progress and Friendly Competition

- WHEN a user completes a scored challenge, THE SYSTEM SHALL persist the resulting XP and leaderboard contribution exactly once.
- WHEN leaderboard data is shown, THE SYSTEM SHALL expose only the display information allowed by the user's privacy settings.
- WHEN rankings are presented, THE APP SHALL use friendly, non-shaming language.

## Android Experience

- WHEN a user follows any core journey, THE APP SHALL provide usable loading, empty, validation, offline, retry, and completed states where applicable.
- WHEN the app is opened through a supported Majlis link, THE APP SHALL route to the intended safe destination after authentication if required.
- WHEN reminders are enabled or disabled, THE SYSTEM SHALL honor the user's preference without guilt-based messaging or excessive notifications.

## Admin Content

- WHEN an admin creates Daily Majlis content, THE SYSTEM SHALL require title, challenge, correct answer, explanation, and publish date.
- WHEN an admin schedules content, THE SYSTEM SHALL prevent two official Daily Majlis records for the same date unless explicitly marked as segmented regional content.
- WHEN content is unpublished, THE SYSTEM SHALL stop serving it to users.

## Production Operations

- WHEN the service starts in a clean environment, THE SYSTEM SHALL apply or clearly expose the required migration procedure and validate required configuration.
- WHEN the API or database is unhealthy, THE SYSTEM SHALL expose a useful protected operational signal without leaking secrets.
- WHEN the API restarts, THE SYSTEM SHALL retain all production accounts, content, attempts, progress, comments, reports, and moderation state.
- WHEN a release is produced, THE TEAM SHALL be able to reproduce the backend deployment and installable Android build from documented steps.
