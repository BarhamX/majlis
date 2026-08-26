# Spec 006: Spoiler-Safe Sharing and Deep Links

## Goal

Let a user invite others into the daily ritual through Android sharing and verified web links without disclosing answers or private user data.

## Scope

- Share-result metadata and an on-device branded image.
- Android Sharesheet integration.
- HTTPS Android App Links with safe web fallback.
- Current, expired, invalid, unauthenticated, and post-auth routing.

Family-group invitations, referral rewards, public profile links, and answer-reveal sharing are post-V1.

## Requirements

- **SHR-001**: Share output shall contain the Majlis brand, UTC content date, non-answer result state such as `completed`, and a canonical HTTPS link; it shall not contain the selected option, correct option, explanation, XP total, streak, display name, user id, or tracking data that identifies the sender.
- **SHR-002**: The API shall return share metadata only after the authenticated user has an accepted attempt for the referenced Daily Majlis.
- **SHR-003**: The Flutter app shall render the share image locally from approved localized copy and design tokens; image semantics shall match the text alternative.
- **SHR-004**: Sharing shall occur only after an explicit user action through the Android Sharesheet. Majlis shall not upload the rendered card unless a future spec authorizes it.
- **SHR-005**: Canonical links shall use `https://<configured-host>/daily/{yyyy-MM-dd}` and Android App Links verified by `assetlinks.json`; custom schemes may be fallback-only and shall not accept privileged actions.
- **SHR-006**: A current-date link shall route to that Daily Majlis. A past or future date shall show a safe expired/unavailable state with a route to today's Majlis; V1 shall not expose an archive through this link.
- **SHR-007**: An unauthenticated recipient shall return to the intended safe destination after authentication without placing tokens or private state in the URL.
- **SHR-008**: Unknown paths, malformed dates, unsupported hosts, and unverified schemes shall fail closed to the app home or safe web landing page.
- **SHR-009**: Link analytics shall record only campaign-free open outcome, platform, and content date with a pseudonymous installation id and consent; sender identity shall not be inferred.
- **SHR-010**: Arabic share cards shall be RTL, legible at common messaging-app thumbnail sizes, and pass golden tests without clipped text.

## Acceptance Criteria

- Static and contract tests prove every output is spoiler-safe.
- Verified links route correctly from installed, not-installed, signed-out, current, expired, and malformed states.
- Android sharing works without storage permission.
- Share images pass Arabic/RTL golden and accessibility-text checks.
