# Majlis Product Design

## Design Intent

Majlis should feel like entering a warm, modern Arab gathering: calm, social, thoughtful, and lightly competitive. The app should not feel like a school quiz app or a generic trivia game.

## Experience Principles

1. **Enter the Majlis**: The home screen should feel like entering a daily space, not opening a content feed.
2. **One Clear Challenge**: The user should always know the next action.
3. **Warm Competition**: Competitive copy should be playful, never insulting.
4. **Micro-Learning**: Explanations should be short and memorable.
5. **Social After Learning**: Discussion should appear after the user has answered, so the user has context.
6. **Share With Pride**: Share cards should look elegant and culturally grounded.

## Core Screens

### 1. Onboarding

Purpose: Explain the concept quickly.

Content:
- App name and tagline.
- One-line promise.
- Region/dialect preference.
- Display name.
- Notification opt-in after value is shown.

### 2. Today's Majlis

Purpose: Daily ritual entry point.

Elements:
- Date and title.
- Warm greeting.
- Challenge card.
- Progress indicator for today's flow.
- Streak summary.

### 3. Challenge Screen

Purpose: Fast answer interaction.

Elements:
- Question.
- 2-4 answer options.
- Difficulty indicator.
- Optional region tag.
- Submit action.

### 4. Result Screen

Purpose: Reward, reveal, and learning.

Elements:
- Correct/incorrect feedback.
- Correct answer.
- Short explanation.
- XP/streak animation.
- Continue to story/proverb.

### 5. Cultural Card

Purpose: Give meaning and shareable value.

Elements:
- Proverb/story/saying.
- Meaning in simple language.
- Context line.
- Share button.

### 6. Discussion Screen

Purpose: Make app feel like a majlis.

Elements:
- Daily prompt.
- Response input.
- Community/family responses.
- Reactions.
- Report action.

### 7. Profile/Streak Screen

Purpose: Show progress and identity.

Elements:
- Display name.
- Region preference.
- Current streak.
- Longest streak.
- Total XP.
- Badges later.

## Navigation

Production V1 bottom navigation:

1. Today
2. Discussion
3. Leaderboard
4. Profile

Admin tooling can be web/API-first and does not need mobile navigation, but an authenticated operational interface is required before production release.

## Empty States

- No Daily Majlis: show calm fallback and invite user to check later.
- No comments: invite user to be first to open the discussion.
- No streak: invite user to start today.
- No leaderboard: show after enough users/friends participate.

## Microcopy Direction

Use short lines:

- "Today's Majlis is open."
- "Can you solve it before your friends?"
- "Your answer is in."
- "The story behind it..."
- "Ask your family this one."
- "Share without spoilers."
