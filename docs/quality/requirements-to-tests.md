# Majlis Production V1 Requirement-to-Test Traceability

## Use

Every normative requirement id must appear here before implementation starts and must link to executable or review evidence before its task is checked complete. `Planned` means the specification is complete but the implementation/test does not yet exist. `Partial` means current tests cover only part of the hardened requirement. Release evidence is stored under `artifacts/release/<version>/` or a durable CI URL recorded in the handoff; generated evidence is not committed when it contains sensitive data.

## Spec 001 - Daily Game

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| DLY-001 | UTC selection; region/timezone invariance | `DailyMajlisSelectionTests`, PostgreSQL API date-boundary cases | Partial |
| DLY-002 | Arabic completeness, language fallback/header | `DailyMajlisLocalizationTests`, `DailyMajlisRevisionTests.Revision_IsServableOnlyWhenArabicAndEveryOptionAreComplete`, and `DailyMajlisApiTests.GetToday_AcceptLanguageFallsBackFromRegionalArabicAndServesEnglishWhenRequested`; hosted Backend CI run `33184958679` | Verified |
| DLY-003 | Pre-attempt field allowlist/spoiler scan | `DailyMajlisServiceTests.GetTodayAsync_WhenPublishedMajlisExists_ReturnsValidSpoilerSafePayload` and `DailyMajlisApiTests.GetToday_WhenPublishedContentExists_ReturnsPersistedSpoilerSafePayload`; an exact full-field allowlist test remains pending | Partial |
| DLY-004 | Safe unavailable problem and Flutter state | API integration + `today_unavailable_test.dart` | Partial |
| DLY-005 | Completed-state attempt pointer | `DailyLoopPostgreSqlTests.AcceptedAttempt_SurvivesApplicationRestart` covers backend `hasAttempted`/`attemptId` state plus authoritative owned result retrieval, and duplicate-prevention cases preserve that attempt; hosted Backend CI run `33184958679`; Flutter completed-state retrieval remains pending | Partial |
| DLY-006 | Result-to-share metadata boundary | `DailyLoopServiceTests.GetShare_ReturnsOnlySpoilerSafeConfiguredMetadata` and `DailyLoopPostgreSqlTests.Share_ReturnsConfiguredSpoilerSafeContractOnly`; hosted Backend CI run `33184958679` | Verified |
| ATT-001 | Authentication/profile, option ownership, and current-published-only submission | completed-profile policy and locked-account revalidation; `DailyLoopPostgreSqlTests` covers unauthenticated and incomplete-profile rejection, option ownership, historical/future/superseded challenges, every non-published current-day status, racing unpublish, and midnight-lock rollover with zero-mutation assertions; hosted Backend CI run `33200378503` | Verified |
| ATT-002 | Atomic attempt/XP/UserProgress transaction with stored locale and post-award snapshots | `DailyLoopPostgreSqlTests.Submit_FirstAcceptedAttempt_CommitsExactAwardOnce`, `Submit_WhenLedgerPersistenceFails_RollsBackEveryDailyLoopRow`, and unsupported-locale persistence; hosted Backend CI run `33184958679` | Verified |
| ATT-003 | Same-key replay and payload mismatch | application and PostgreSQL replay/reuse cases; hosted Backend CI run `33184958679` | Verified |
| ATT-004 | Unique attempt and concurrent requests | same-key/different-key PostgreSQL race cases assert one attempt, ledger, progress, and idempotency row; hosted Backend CI run `33184958679` | Verified |
| ATT-005 | Post-attempt result, stored locale, immutable revision, and progress snapshots | PostgreSQL replay/restart/correction/later-progress cases; hosted Backend CI run `33184958679` | Verified |
| ATT-006 | No replacement/retry/rescore after correction or unpublishing | completion-conflict, correction, and unpublishing snapshot/retrieval cases; hosted Backend CI run `33184958679` | Verified |
| ATT-007 | Stable `attempt_not_found`, owned history, opaque cursor, and source revision | PostgreSQL ownership, correction, and stable-cursor cases; hosted Backend CI run `33184958679` | Verified |
| ATT-008 | Attempt rate limits and no mutation | authenticated pipeline account/IP tests plus PostgreSQL row-count proofs; hosted Backend CI run `33184958679` | Verified |
| PROG-001 | Exact incorrect/correct XP (10/15) | `XpAwardTests` plus application and PostgreSQL exact-award cases; hosted Backend CI run `33184958679` | Verified |
| PROG-002 | Ledger uniqueness and `UserProgress` as the single scoring/streak authority | persistence-model tests and PostgreSQL replay/race row-count proofs; hosted Backend CI run `33184958679` | Verified |
| PROG-003 | Correct and incorrect streak eligibility through `UserProgress` | `UserProgressServiceTests.CorrectAndIncorrectAreEligible` and PostgreSQL correct/incorrect submissions; hosted Backend CI run `33184958679` | Verified |
| PROG-004 | Consecutive/repeat/missed day behavior through `UserProgress` | domain boundary suite and PostgreSQL consecutive/skipped-day cases; hosted Backend CI run `33184958679` | Verified |
| PROG-005 | Missing publication exemption | domain missing-publication case and `DailyLoopPostgreSqlTests.Submit_MissingUnpublishedContentDay_DoesNotBreakStreak`; hosted Backend CI run `33184958679` | Verified |
| PROG-006 | Longest streak monotonicity | domain monotonicity and PostgreSQL reset/stored-snapshot cases; hosted Backend CI run `33184958679` | Verified |

## Spec 002 - Community

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| COM-001 | Auth plus completed-Daily eligibility | `DiscussionEligibilityTests` | Planned |
| COM-002 | One active response and text validation | domain + PostgreSQL constraint tests | Planned |
| COM-003 | Pending immutable revisions and author/moderator visibility | `CommentRevisionTests` | Planned |
| COM-004 | Edit hides prior revision; delete immediate | API integration + widget state tests | Planned |
| COM-005 | Stable visible-only cursor listing and all filters | `CommentVisibilityTests` | Planned |
| COM-006 | Consumer DTO field allowlist | contract snapshot/security test | Planned |
| COM-007 | Reaction types/toggle/self/block guards | `CommentReactionTests` | Planned |
| COM-008-COM-009 | Report uniqueness, detail limits, reporter privacy | `CommentReportTests` | Planned |
| COM-010 | Indirect leak scan across counts/cache/analytics | API security integration suite | Planned |
| COM-011 | Endpoint rate limits and `Retry-After` | rate-limit integration tests | Planned |
| COM-012 | Mutation idempotency | discussion idempotency tests | Planned |

## Spec 003 - Release Umbrella

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| REL-001 | Authority boundaries | architecture review + controller/domain tests | Partial |
| REL-002 | Spoiler safety | pre-attempt contract snapshot and response scan | Partial |
| REL-003 | Exactly-once progress | application replay/conflict cases and PostgreSQL same-key/different-key races pass with one attempt, ledger, progress, and idempotency row in hosted Backend CI run `33184958679` | Verified |
| REL-004-REL-005 | Community/private-data non-disclosure | COM/MOD/Auth security suites | Planned |
| REL-006-REL-007 | Arabic/source/publish invariants | ADM/DLY publication tests | Planned |
| REL-008-REL-009 | No manual/hardcoded dependency; scope guard | release architecture and UX review | Planned |
| REL-010 | Clean-environment bootstrap/migrations | CI clean-start job | Partial |
| REL-011 | New-user Android journey | local test-identity flow before `Game Ready`; Google, Apple, Meta, and Snapchat variants before release | Planned |
| REL-012 | Restart durability | daily content and accepted attempt/result/progress restart persistence pass in hosted Backend CI run `33184958679`; community, moderation, reports, and audit-event durability remain pending | Partial |
| REL-013 | Publisher/moderator journeys | admin browser end-to-end suite | Planned |
| REL-014 | Cross-user/admin authorization | identity self-scope and daily attempt/share ownership non-enumeration pass; administrative authorization remains pending | Partial |
| REL-015-REL-016 | Complete automated/operational gates | Spec 009 evidence pack | Planned |

## Spec 004 - Authentication and Profile

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| AUTH-001-AUTH-002 | Google/Apple/Meta/Snapchat provider-flow and token validation matrix | signed test-token validation verified; live provider Android flows after `Game Ready` | Partial |
| AUTH-003 | Idempotent provider/issuer/subject bootstrap | `IdentityProfileServiceTests`, `IdentityProfileApiTests`; PostgreSQL concurrency test pending Docker | Partial |
| AUTH-004 | Verification, consent, and under-13 rejection | `IdentityProfileServiceTests` and functional API tests pass; live provider verification deferred | Partial |
| AUTH-005-AUTH-006 | Display name and profile-code validation | `UserAccountTests` and `IdentityProfileApiTests` | Partial |
| AUTH-007-AUTH-008 | Private defaults and adult-only opt-in | `UserAccountTests` and `IdentityProfileApiTests` | Partial |
| AUTH-009-AUTH-010 | Logout/revoke/recovery enumeration safety | local revocation implemented; live provider recovery deferred | Partial |
| AUTH-011 | Role audit and MFA policy | admin security test + provider evidence | Planned |
| AUTH-012 | No target-user IDOR | `IdentityProfileApiTests` self-scope passes; PostgreSQL/security suite pending | Partial |
| AUTH-013 | Deletion lifecycle/deadlines | `UserAccountTests` and `IdentityProfileApiTests` pass for request/revocation; purge/provider/staging drill pending | Partial |
| AUTH-014 | Limits and safe errors | rate-limit/problem-contract tests | Planned |
| AUTH-015 | Rename/audit identity behavior | profile/moderation integration tests | Planned |
| AUTH-016 | Explicit linking, collision, last-identity, and no email merge | domain/persistence invariants added; provider endpoint tests pending | Partial |
| AUTH-017 | Test issuer environment isolation | `TestIdentityTokenIssuerTests` and `AuthenticationConfigurationTests` | Verified |

## Spec 005 - Leaderboard

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| LDB-001 | UTC weekly bounds | parameterized week-boundary unit tests | Planned |
| LDB-002-LDB-003 | Adult opt-in and field privacy | leaderboard API integration/contract tests | Planned |
| LDB-004-LDB-005 | Top 100, self entry, ties | ranking query tests with production-shaped data | Planned |
| LDB-006 | Ledger-only contribution | XP/leaderboard concurrency test | Planned |
| LDB-007-LDB-008 | Block/rename/suspend/delete invalidation | privacy integration tests | Planned |
| LDB-009 | Non-shaming localized copy | product/editorial review + widget snapshot | Planned |
| LDB-010 | Cache staleness bound | cache invalidation integration test | Planned |

## Spec 006 - Sharing and Deep Links

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| SHR-001-SHR-002 | Spoiler/private-field absence and ownership | `DailyLoopServiceTests.GetShare_ReturnsOnlySpoilerSafeConfiguredMetadata`, `DailyLoopPostgreSqlTests.Share_ReturnsConfiguredSpoilerSafeContractOnly`, and `AttemptReads_AreOwnedAndNonEnumerating`; hosted Backend CI run `33184958679` | Verified |
| SHR-003-SHR-004 | Local render, semantics, explicit Sharesheet | Flutter widget/integration tests | Planned |
| SHR-005 | HTTPS App Link verification | hosted `assetlinks.json` check + adb test | Planned |
| SHR-006, SHR-007, SHR-008 | Current/expired/auth/invalid routing | Android deep-link matrix | Planned |
| SHR-009 | Privacy-safe open analytics | analytics payload contract test | Planned |
| SHR-010 | Arabic thumbnails and clipping | Flutter golden suite | Planned |

## Spec 007 - Reminders

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| NTF-001-NTF-002 | Off-by-default permission and one reminder | widget + Android permission tests | Planned |
| NTF-003-NTF-004 | Stored preference and replacement scheduling | API + platform scheduler tests | Planned |
| NTF-005 | Copy policy | localized copy snapshot/editorial review | Planned |
| NTF-006 | Safe tap route | Android integration test | Planned |
| NTF-007 | Reboot/upgrade/timezone/DST recomputation | platform integration matrix | Planned |
| NTF-008 | Disable/logout/deletion cancellation | Android lifecycle tests | Planned |
| NTF-009 | Analytics allowlist | event contract test | Planned |
| NTF-010 | Android permission-version matrix | API 29/33/latest device run | Planned |

## Spec 008 - Content and Moderation Operations

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| ADM-001 | Usable protected browser interface | publisher end-to-end test | Planned |
| ADM-002 | Immutable revision behavior | domain/PostgreSQL tests | Planned |
| ADM-003-ADM-004 | Required fields, sources, complete Arabic | validation and publish-transition tests | Planned |
| ADM-005 | Review audit and separation of duty | authorization/domain tests | Planned |
| ADM-006-ADM-007 | Approved-only schedule, uniqueness, 00:00Z, coverage | scheduler/concurrency integration tests | Planned |
| ADM-008 | Correction preserves attempts/XP and shows note | correction end-to-end test | Planned |
| ADM-009 | Internal-source/operator exclusion | consumer contract security scan | Planned |
| ADM-010 | Atomic immutable audit | transaction/integration tests | Planned |
| MOD-001, MOD-002, MOD-003 | Pending visibility, actions, report privacy/uniqueness | moderation integration suite | Planned |
| MOD-004 | Priority ordering | queue query tests | Planned |
| MOD-005-MOD-006 | Audit/public privacy and block filtering | security integration suite | Planned |
| MOD-007 | Appeal eligibility and different reviewer | appeal domain/API tests | Planned |
| MOD-008-MOD-009 | Restricted retention, pagination, rate, XSS | security/retention integration tests | Planned |
| MOD-010-MOD-011 | Safe metrics and response-time targets | metrics label test + staging queue drill | Planned |
| MOD-012 | Audited discussion disable/restore and challenge continuity | admin/consumer end-to-end test | Planned |

## Spec 009 - Production Operations

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| OPS-001, OPS-002, OPS-003, OPS-004 | Environment parity, migration, secrets, health | configuration contract, clean deploy, secret scan | Partial |
| OPS-005 | API load/latency/error thresholds | `artifacts/release/<version>/api-load-report` | Planned |
| OPS-006, OPS-007, OPS-008 | Android timing, frame quality, capacity dataset | benchmark/vitals/load evidence | Planned |
| OPS-009-OPS-010 | Supported Android/device matrix | device-farm/emulator matrix report | Planned |
| OPS-011-OPS-012 | WCAG, TalkBack, text scale, Arabic/RTL | accessibility scanner + manual/device report | Planned |
| OPS-013 | Availability objective/alerts | staging synthetic and alert drill | Planned |
| OPS-014-OPS-015 | Backup RPO/RTO and restore | timestamped restore-drill report | Planned |
| OPS-016-OPS-017 | Rollback and content coverage alerting | deployment/rollback and scheduler drills | Planned |
| OPS-018 | Threat model and finding gate | signed security review | Planned |
| OPS-019, OPS-020, OPS-021, OPS-022, OPS-023 | CI security/privacy/deletion controls | CI jobs + staging privacy drill | Planned |
| OPS-024, OPS-025, OPS-026 | Event catalog, payload, consent/deletion | analytics contract and staging inspection | Planned |
| OPS-027 | Candidate CI suites | immutable CI run URL | Planned |
| OPS-028 | Signed reproducible App Bundle/Play checks | provenance and Play pre-launch report | Planned |
| OPS-029 | Complete staging journeys | timestamped end-to-end evidence pack | Planned |
| OPS-030 | Checklist, blockers, and approvals | signed release checklist/handoff | Planned |

## Traceability Audit

Before a release candidate, run a repository check that extracts requirement ids matching `REL|DLY|ATT|PROG|COM|AUTH|LDB|SHR|NTF|ADM|MOD|OPS` and fails if an id is missing from this file or has no non-`Planned` evidence. The release owner records the command and result in `docs/ai-context/HANDOFF.md`.
