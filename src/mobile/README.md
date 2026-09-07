# Majlis Mobile App

The Majlis mobile app is the Android-first Flutter client for the daily Arab cultural challenge game.

The mobile app should feel like entering a warm, modern digital majlis: short daily rituals, friendly challenge, cultural pride, family/community participation, and shareable moments.

## Current Status

The mobile source is planned but not implemented yet.

This folder is reserved for the Flutter app once the mobile implementation begins.

## MVP Mobile Scope

The first mobile milestone should implement:

1. App shell and theme
2. Onboarding intro
3. Daily Majlis home screen
4. Challenge screen
5. Result/reveal screen
6. Streak display
7. Shareable result card
8. Basic API client for `GET /api/v1/daily-majlis/today`

## Recommended Flutter Structure

```text
src/mobile/
  majlis_app/
    lib/
      main.dart
      app/
        app.dart
        router.dart
      core/
        api/
        theme/
        localization/
        widgets/
      features/
        onboarding/
        daily_majlis/
        challenge/
        results/
        streak/
        share_card/
        discussion/
```

## State Management

Recommended: Riverpod.

Reason:

- Good fit for small feature states.
- Works well with async API loading.
- Keeps screen logic testable.
- Avoids heavy architecture too early.

## Routing

Recommended: GoRouter.

Initial routes:

```text
/
/onboarding
/today
/challenge
/result
/profile
```

## API Boundary

The mobile app must consume backend contracts, not duplicate game rules.

Client may:

- Render challenge options
- Submit selected answers later
- Display result returned by backend
- Generate share-card visuals from backend-safe data

Client must not:

- Decide correct answer locally
- Hardcode daily cultural content
- Calculate authoritative score
- Modify streaks locally without backend confirmation

## Design Direction

The app should balance:

- Traditional warmth
- Modern clarity
- Playful challenge
- Cultural respect
- Shareability

Avoid:

- Heavy textbook design
- Dark-pattern retention
- Shame-based copy
- Overly childish game visuals
- Regional stereotypes

## First Flutter Implementation Prompt

Use this after backend daily endpoint is stable:

```text
Implement the Flutter Android app shell for Majlis. Read AGENTS.md, docs/ai-context files, src/mobile/README.md, src/mobile/UI-SYSTEM.md, and specs/001-playable-daily-majlis. Create the app shell, theme tokens, routing, and a Daily Majlis screen that calls GET /api/v1/daily-majlis/today. Do not implement authentication, comments, or answer submission yet. Update HANDOFF.md with files changed and validation commands.
```
