# Majlis Product Requirements Document

## 1. Product Summary

Majlis is a daily Arab culture challenge game for Android. Users enter a digital majlis, answer a short cultural challenge, discover the meaning behind a proverb, story, saying, or tradition, and then discuss or share the result with friends and family.

The app combines daily puzzle behavior, cultural storytelling, external family/friend sharing, opt-in global competition, and respectful community discussion.

## 2. Vision

Create the most engaging daily digital majlis for Arabs to test, rediscover, and share cultural knowledge in a short, warm, social, and modern mobile experience.

## 3. Positioning

Majlis is not a generic education app. It is a daily cultural game.

Primary positioning:

> Wordle-style daily play for Arab cultural knowledge.

Emotional positioning:

> A modern majlis in your pocket.

## 4. Problem Statement

Arab cultural knowledge is increasingly fragmented across family memory, short-form videos, school content, social media posts, and informal conversations. Young Arabs may know pieces of language, dialect, proverbs, stories, and traditions, but there is no daily playful ritual that helps them test and share that knowledge with their community.

Existing cultural content is often either too academic, too passive, too long, or not social enough. Majlis turns cultural discovery into a daily challenge and conversation.

## 5. Goals

- Build a daily habit around Arab cultural learning.
- Make cultural knowledge feel playful, social, and competitive.
- Encourage family/friend discussion around proverbs, sayings, stories, and traditions.
- Create a shareable format that can travel through WhatsApp, Instagram, TikTok, Snapchat, and X.
- Build a safe, moderated community around cultural curiosity.

## 6. Post-V1 Product Expansion

- Full course marketplace.
- Long-form lectures.
- Open-ended public social network.
- Real-money rewards.
- AI-generated cultural facts without editorial review.
- Full school/university dashboard.
- iOS and web clients before the Android production release is proven.

## 7. Target Users

### Arab Youth

Users aged roughly 13-30 who want short, competitive, social content that connects them with identity and culture.

### Families

Parents, siblings, cousins, and extended families who want to challenge each other and share stories.

### Schools and Universities

Teachers and cultural clubs that want lightweight daily prompts for class discussion.

### Cultural Organizations

Museums, heritage centers, cultural ministries, festivals, and local organizations that want to sponsor or publish cultural challenges.

### Arab Diaspora

Arabs living outside the Arab world who want to preserve connection to language, sayings, heritage, and family memory.

### Culture-Curious Users

Non-Arab or Arabic-learning users interested in Arab culture, language, and social customs. This is a secondary audience after the Arab-first Android launch.

## 8. Production V1 Scope

Production V1 is the complete Android Majlis app, not a prototype or a single feature slice:

1. Google Account and Sign in with Apple registration/login.
2. Profile with display name, region preference, progress, and notification settings.
3. Today's Majlis screen.
4. One daily cultural challenge.
5. Multiple-choice answer submission.
6. Result and short explanation.
7. Curated proverb/story card with internal source traceability.
8. Persistent attempts, streaks, XP, and duplicate-award protection.
9. Basic privacy-safe leaderboard and friendly comparison.
10. Shareable spoiler-safe result/cultural card and deep-link handling.
11. Daily discussion question, comments, and reactions.
12. Reporting, moderation states, and moderator workflow.
13. Authenticated admin content creation, review, scheduling, publishing, and correction workflow.
14. User-controlled daily reminders and the consent-aware product event catalog defined by Spec 009.
15. Production PostgreSQL persistence, security controls, observability, deployment configuration, and an Android release build.

The release-wide policy decisions for content days, localization, family scope, identity, safety, privacy, scoring, reminders, and leaderboards are normative in `docs/product/v1-product-decisions.md`.

## 9. Core Product Loop

1. Trigger: notification, friend share, or daily habit.
2. Challenge: user answers the daily prompt.
3. Reward: result, explanation, XP, streak.
4. Social proof: view an opt-in global weekly leaderboard or share externally with friends/family.
5. Discussion: respond to one cultural question.
6. Viral output: share card or invite link.
7. Return: next day content and streak continuation.

## 10. Functional Requirements

### Daily Majlis

- The system shall serve one official Daily Majlis per calendar day.
- The Daily Majlis shall include title, topic, challenge, explanation, proverb/story, and discussion question.
- The system shall support region and dialect provenance tags without selecting different V1 editions.
- The system shall support scheduled publishing.

### Challenge

- The user shall be able to answer one daily challenge.
- The system shall prevent repeated scoring for the same daily challenge.
- The system shall show correct/incorrect result after submission.
- The system shall show a short explanation after answer submission.
- The first submission is final in V1; there are no answer retries.

### Gamification

- The system shall award 10 completion XP and 5 additional XP for a correct first attempt.
- The system shall track current streak and longest streak.
- The system shall support future streak protection tokens.
- The system shall show the opt-in global weekly leaderboard; aggregate answer distribution is post-V1 unless separately specified.

### Community

- The system shall show a daily discussion question.
- The user shall be able to submit a response.
- The user shall be able to react to responses.
- The user shall be able to report inappropriate responses.
- New responses shall require moderator approval before public visibility.
- Reported responses shall enter moderation review state, and users shall be able to block another user.

### Sharing

- The system shall generate a shareable result card.
- The card shall never reveal the correct option or explanation.
- The card shall include Majlis branding and an invitation to try today's challenge.

### Admin Content

- Admins shall be able to create and schedule Daily Majlis content.
- Admins shall be able to mark source notes, region tags, and review state.
- Admins shall be able to publish/unpublish content.

## 11. Non-Functional Requirements

- Daily challenge loading and every other release quality gate shall meet the measurable thresholds in `specs/009-production-operations/spec.md`.
- Core daily challenge should be completable in 1-3 minutes.
- Backend APIs should use clear versioning from the start.
- The Android release shall ship Arabic consumer UI/content and RTL behavior; English internal documentation is acceptable during development.
- Community features must be safe by default.
- Personal data must be minimized.

## 12. Success Metrics

### Activation

- Onboarding completion rate.
- First challenge completion rate.
- First share rate.

### Retention

- D1 retention.
- D7 retention.
- D30 retention.
- Average streak length.
- Percentage of users completing 3+ challenges in first week.

### Engagement

- Daily challenge completion rate.
- Discussion response rate.
- Share card generation rate.
- Spoiler-safe shared-link open and activation conversion.

### Quality

- Report rate per 1,000 comments.
- Moderator action time.
- Content correction rate.
- User rating of cultural authenticity.

## 13. Risks

- Content accuracy risk.
- Regional sensitivity risk.
- Low repeat engagement if daily loop is not fun.
- Social toxicity if competition becomes insulting.
- High content operations load.
- Overbuilding community before retention is proven.

## 14. Release Recommendation

Release Majlis Production V1 only when the complete scope in `docs/product/full-app-scope.md` works end to end. Playable Daily Majlis is an implementation milestone, not the release boundary. Advanced courses, institutional dashboards, and other explicitly post-V1 products do not block the Android release.
