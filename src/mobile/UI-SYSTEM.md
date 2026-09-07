# Majlis Mobile UI System

This file defines the initial UI direction for the Flutter mobile app. It should guide implementation until a formal design system is created in Figma or another design tool.

## Product Feeling

Majlis should feel like:

- Entering a warm Arab majlis
- A respectful daily cultural challenge
- A modern social game, not a textbook
- A space for wit, memory, stories, and family discussion

Core emotional keywords:

```text
warm, proud, social, respectful, modern, playful, authentic, calm
```

## Visual Principles

### 1. Warm Digital Majlis

Use soft surfaces, rounded panels, and layered cards that resemble a calm gathering space.

### 2. Cultural Without Cliche

Avoid heavy ornamentation, stereotypes, or generic desert imagery everywhere. Use cultural cues carefully through spacing, typography, geometry, patterns, and copy tone.

### 3. Challenge Without Shame

The UI should provoke curiosity and friendly competition, not guilt.

Good:

```text
Can you solve today's proverb?
Your family may know this one. Do you?
This one defeated 64% of players today.
```

Avoid:

```text
You do not know your culture.
Real Arabs should know this.
Shame on you for missing this.
```

### 4. Shareable by Default

Every daily result should be easy to turn into a clean share card.

## Suggested Color Direction

Use a warm premium palette:

```text
Majlis Sand       #F5E8D3
Coffee Brown      #5A3825
Card Cream        #FFF8EC
Date Gold         #C8953C
Deep Ink          #1F1A17
Palm Green        #2F6B4F
Soft Terracotta   #B96A4B
Error Red         #B42318
Success Green     #2E7D32
```

Rules:

- Use cream/sand for background.
- Use coffee/deep ink for text.
- Use gold sparingly for highlights and streaks.
- Use green for positive progress.
- Avoid overusing red in incorrect-answer states.

## Typography Direction

Arabic and English should both feel refined and readable.

Recommended approach:

- Arabic: use a modern Arabic font with strong readability.
- English: use a neutral rounded or humanist sans-serif.
- Keep headings strong but not decorative.
- Keep body text readable at small mobile sizes.

## Core Screens

### 1. Splash / Opening Majlis

Purpose: establish the ritual.

Elements:

- App logo/name
- Warm background
- Short line: `Today's Majlis is opening...`

### 2. Onboarding

Purpose: explain the loop quickly.

Slides:

1. Daily culture challenge
2. Proverbs, stories, and sayings
3. Compete with family and friends
4. Share your result

### 3. Daily Majlis Home

Purpose: user's daily hub.

Sections:

- Greeting
- Today's theme
- Challenge card
- Story/proverb teaser
- Streak strip
- Discussion teaser

### 4. Challenge Screen

Purpose: focus on the daily question.

Elements:

- Question
- Multiple-choice options
- Difficulty indicator
- Region/topic tag
- Submit button

### 5. Result Screen

Purpose: reward and teach.

Elements:

- Correct/incorrect state
- Explanation
- Score
- Streak update
- Share button
- Continue to discussion

### 6. Discussion Screen

Purpose: reproduce the feeling of a majlis conversation.

Elements:

- Daily discussion question
- Response composer
- Moderated community responses
- Reactions
- Report option

### 7. Share Card

Purpose: viral loop.

Elements:

- Score
- Streak
- Today's theme
- Non-spoiler challenge phrase
- App branding

## Components

```text
MajlisCard
ChallengeOptionTile
StreakBadge
RegionTag
DailyThemeHeader
ProverbCard
StorySnippetCard
DiscussionPromptCard
ShareResultCard
PrimaryMajlisButton
SecondaryMajlisButton
```

## Motion Direction

Motion should feel calm and premium.

Use:

- Soft card entrance
- Light button press feedback
- Result reveal animation
- Streak increment animation

Avoid:

- Loud casino-style effects
- Excessive confetti
- Fast flashing animations
- Motion that harms accessibility

## Accessibility

- Support readable font sizes.
- Maintain strong color contrast.
- Do not rely only on color for correct/incorrect states.
- Support reduced motion.
- All tappable areas should be comfortable for mobile use.

## UI Copy Tone

Tone should be:

- Friendly
- Witty
- Respectful
- Proud without being preachy
- Competitive without insulting

Example copy:

```text
Today's Majlis is open.
Can you complete the saying?
Ask someone older. They might know.
You solved it faster than most players today.
Now bring your answer to the Majlis.
```
