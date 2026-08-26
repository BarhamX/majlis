# Majlis Theme and Visual Identity

## Brand Feel

Warm, social, authentic, modern, and respectful.

Majlis should combine the emotional cues of hospitality, conversation, heritage, and evening gathering with the clarity and speed of a modern mobile game.

## Visual Keywords

- Warmth
- Sand
- Coffee
- Amber
- Majlis cushions
- Lantern light
- Card-based ritual
- Elegant Arabic geometry
- Calm competition

## Color Palette

### Primary

- Deep Coffee: `#3A2418`
- Majlis Amber: `#C9893B`
- Sand: `#F3E6D0`

### Secondary

- Date Brown: `#7A4F2A`
- Palm Green: `#506B45`
- Night Navy: `#172033`

### Feedback

- Correct: `#2F7D4F`
- Incorrect: `#A6423A`
- Warning: `#D69A2D`

## Typography

### Arabic Launch UI

Arabic is the required launch locale, not a later enhancement. Bundle Noto Sans Arabic under its license and use it for app chrome, challenge content, discussion, and share cards. Verify Arabic glyph coverage, diacritics, mixed Arabic/Latin numerals, truncation, and readability at 200% text scaling. Theme tokens must keep the family replaceable after V1 evaluation.

### English/Internal

English is optional for V1. When present, use system typography and keep both families behind semantic typography tokens.

### Directionality

- Default to RTL for Arabic and derive direction from the active locale, never from individual strings.
- Mirror navigation and directional icons; do not mirror logos, media controls, checkmarks, or numeric values.
- Keep mixed Arabic/English text and URLs readable with explicit bidi isolation where needed.

## Component Direction

### Cards

Cards are the main object in the app. They should feel like small majlis conversation pieces.

### Buttons

Primary button should feel warm and confident. Avoid aggressive game styling.

### Streaks

Streaks should feel like ritual continuity, not pressure.

### Leaderboards

Leaderboards should emphasize friendly comparison. Avoid copy that humiliates low-ranking users.

## Motion

Use subtle motion:

- Card entrance.
- Correct answer glow.
- Streak increment.
- Share card reveal.

Avoid excessive confetti and arcade-like animations in the first release.

## Share Card Style

Share cards should be clean and elegant:

- Majlis logo/name.
- Result or proverb.
- Minimal pattern background.
- No correct option or explanation in V1 share cards.
- Deep link invitation.
