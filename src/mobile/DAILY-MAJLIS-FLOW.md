# Daily Majlis Mobile Flow

This file defines the mobile flow for the first playable Majlis experience.

## Core Product Loop

Majlis should feel like a daily ritual:

```text
Open app
  -> See today's Majlis
  -> Accept challenge
  -> Answer
  -> Reveal result
  -> Learn short context
  -> Keep streak
  -> Share or discuss
```

The user should be able to complete the core loop in under three minutes.

## Screen Flow

### 1. App Launch

Purpose: quickly move the user into today's experience.

Behavior:

- If first launch: show onboarding.
- If returning user: show Today's Majlis.
- If API is loading: show warm loading state, not a blank spinner.
- If offline: show cached content if available later; otherwise show friendly retry state.

### 2. Today's Majlis

Purpose: present the daily theme and invite the user into the challenge.

Content:

- Greeting
- Today's title
- Topic tag
- Challenge teaser
- Proverb/story teaser
- Streak badge
- Start challenge button

Primary CTA:

```text
Enter Today's Majlis
```

Alternative CTA:

```text
Start Challenge
```

### 3. Challenge

Purpose: focused answer interaction.

Content:

- Question
- Options
- Difficulty indicator
- Cultural region/topic tag if available
- Submit button

Rules:

- User must select an option before submit.
- Do not show the answer before submit.
- Do not allow confusing UI that makes users think they submitted when they only selected.

### 4. Result Reveal

Purpose: give feedback and teach quickly.

Content:

- Correct/incorrect state
- Correct answer after submission
- 20-30 second explanation
- Score/XP
- Streak update
- Continue button

Tone:

- If correct: celebrate with restraint.
- If incorrect: teach without shame.

Good incorrect copy:

```text
Close, but the Majlis says otherwise.
Here is the meaning behind it.
```

Avoid:

```text
You should have known this.
```

### 5. Story / Proverb Context

Purpose: convert the answer into cultural learning.

Content:

- Proverb or story title
- Short explanation
- Meaning/context
- Optional audio later

### 6. Discussion Prompt

Purpose: create social/collaborative value.

Content:

- Daily discussion question
- Add response CTA
- Community response preview after moderation exists

Example prompt:

```text
Which saying did your family use most when you were young?
```

### 7. Share Card

Purpose: bring other users into the challenge without spoiling the answer.

Share card must include:

- App name
- Daily theme
- User score/streak if available
- Non-spoiler phrase
- Invite line

Example:

```text
I entered today's Majlis and scored 4/5.
Can you solve the proverb?
```

## Empty and Error States

### No Daily Majlis Available

```text
The Majlis is being prepared. Check again soon.
```

### Network Error

```text
We could not open today's Majlis. Try again.
```

### Already Completed

```text
You completed today's Majlis. Come back tomorrow, or join the discussion.
```

## Analytics Events

Track these events when analytics is added:

```text
app_opened
daily_majlis_viewed
challenge_started
challenge_option_selected
challenge_submitted
result_viewed
explanation_viewed
share_card_opened
share_completed
discussion_opened
```

## Non-Goals for First Mobile Slice

Do not implement in the first UI slice:

- Authentication
- Family groups
- Public comments
- Audio playback
- Push notifications
- Full leaderboard
- Payment/subscription

The first goal is a clean playable daily loop.
