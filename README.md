# Majlis

Majlis is a daily Arab culture challenge game for Android, built with Flutter and a .NET backend. It recreates the spirit of a traditional majlis as a modern mobile ritual: short cultural challenges, proverbs, stories, discussion prompts, family/friend competition, and shareable results.

## Product Positioning

**Majlis is Wordle-style daily play for Arab cultural knowledge.**

The app does not shame users for what they do not know. It playfully provokes curiosity:

> Today's Majlis is open. Can you answer before your friends?

## Core Loop

1. User opens today's Majlis.
2. User answers a short cultural challenge.
3. App reveals answer, meaning, and short context.
4. User sees streak, XP, and comparison with friends/family.
5. User contributes to the daily discussion.
6. User shares a result or proverb card.

## Repository Status

This repository currently contains the product, business, UX, design, architecture, AI-agent context, and Spec Kit foundation for the first playable version of Majlis.

## Important Directories

```text
.specify/memory/constitution.md      Spec Kit project constitution
specs/                              Feature specs, plans, and tasks
AGENTS.md                           Agent operating instructions
docs/product/                       PRD, personas, journeys, MVP, roadmap
docs/business/                      BRD and business model
docs/design/                        UX, theme, visual identity, content voice
docs/architecture/                  System architecture, API, database, stack
docs/ai-context/                    Files Codex/agents must read first
docs/prompts/                       Reusable prompt pack
apps/mobile/                        Future Flutter application
src/backend/                        Future .NET backend solution
```

## Recommended Implementation Order

1. Create Flutter and .NET solution skeletons.
2. Implement authentication and profile foundation.
3. Implement daily Majlis content API.
4. Implement challenge answer submission and scoring.
5. Implement streaks and result screen.
6. Implement shareable cards.
7. Implement community response and moderation.
8. Implement admin content management.

## First Milestone

**M1: Playable Daily Majlis**

A user can open the app, see today's cultural challenge, answer, receive feedback, read a short explanation, update streak, and share a result card.
