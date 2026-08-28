# Majlis Constitution

## 1. Cultural Respect Is Non-Negotiable

Majlis exists to strengthen Arab cultural curiosity, pride, and shared memory. Product behavior, content, copy, community mechanics, and gamification must avoid humiliation, stereotyping, sectarian framing, tribal mockery, or regional superiority.

## 2. Specs Are the Source of Truth

Implementation must follow the relevant feature `spec.md`, `plan.md`, and `tasks.md`. Code is considered correct only when it satisfies the specification and acceptance criteria.

## 3. Healthy Retention Over Dark Patterns

Majlis may use streaks, XP, leaderboards, challenge reminders, and scarcity. It must not use guilt loops, infinite scroll, manipulative notification flooding, hidden penalties, or shame-based identity pressure.

## 4. Short Daily Ritual First

The primary user experience is a daily cultural ritual that can be completed in 1-3 minutes. Any feature that makes the daily loop slower must justify its value.

## 5. Community Requires Safety

Community responses, family groups, and public discussions must include reporting, moderation state, and safe defaults. No public community feature should ship without basic moderation and abuse handling.

## 6. Content Must Be Curated and Traceable

Cultural content must include internal source notes, region/dialect tags when relevant, and editorial review status. The app may simplify stories for short format but must not fabricate historical claims as fact.

## 7. Architecture Must Preserve Changeability

Backend domain logic must be separated from API controllers and persistence. Flutter UI must be feature-first with centralized theme tokens and copy conventions. Daily content must not be hardcoded into app screens.

## 8. Test the Core Loop

The daily Majlis loop, scoring, streak logic, content scheduling, and moderation state transitions require automated tests before release.

## 9. Privacy by Default

User profile data, family group membership, attempts, and discussion activity must be protected by least-privilege access rules. Sharing must be explicit and user-initiated.

## 10. Deliver the Complete App Through Playable Slices

Each feature slice must produce a working, testable increment, but slices are sequencing tools rather than release boundaries. Playable Daily Majlis is the first vertical slice; the delivery target remains the complete production Android app defined in `docs/product/full-app-scope.md`. The project is not complete while required mobile, backend, persistence, safety, content-operations, or release capabilities are missing.
