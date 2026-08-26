# Majlis Production V1 Requirement-to-Test Traceability

## Use

Every normative requirement id must appear here before implementation starts and must link to executable or review evidence before its task is checked complete. `Planned` means the specification is complete but the implementation/test does not yet exist. `Partial` means current tests cover only part of the hardened requirement. Release evidence is stored under `artifacts/release/<version>/` or a durable CI URL recorded in the handoff; generated evidence is not committed when it contains sensitive data.

## Spec 001 - Daily Game

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| DLY-001 | UTC selection; region/timezone invariance | `DailyMajlisSelectionTests`, PostgreSQL API date-boundary cases | Partial |
| DLY-002 | Arabic completeness, language fallback/header | `DailyMajlisLocalizationTests` | Planned |
| DLY-003 | Pre-attempt field allowlist/spoiler scan | `DailyMajlisApiTests.GetToday_PreAttemptContractIsSafe` | Partial |
| DLY-004 | Safe unavailable problem and Flutter state | API integration + `today_unavailable_test.dart` | Partial |
| DLY-005 | Completed-state attempt pointer | API integration + Flutter state test | Planned |
| DLY-006 | Result-to-share metadata boundary | contract snapshot + Spec 006 suite | Planned |
| ATT-001 | Authentication/profile and option ownership | `AttemptAuthorizationTests` | Planned |
| ATT-002 | Atomic attempt/XP/streak transaction | `AttemptTransactionTests` | Planned |
| ATT-003 | Same-key replay and payload mismatch | `AttemptIdempotencyTests` | Planned |
| ATT-004 | Unique attempt and concurrent requests | `AttemptConcurrencyTests` against PostgreSQL | Planned |
| ATT-005 | Post-attempt result allowlist/localization | `AttemptContractTests` | Planned |
| ATT-006 | No replacement/retry/rescore | domain + API integration tests | Planned |
| ATT-007 | History ownership, pagination, revision | `AttemptHistoryTests` | Planned |
| ATT-008 | Attempt rate limits and no mutation | rate-limit integration tests | Planned |
| PROG-001 | Exact incorrect/correct XP (10/15) | `XpAwardTests` | Planned |
| PROG-002 | Ledger uniqueness | PostgreSQL uniqueness/concurrency tests | Planned |
| PROG-003 | Correct and incorrect streak eligibility | `StreakServiceTests` | Planned |
| PROG-004 | Consecutive/repeat/missed day behavior | `StreakServiceTests` with fake time | Planned |
| PROG-005 | Missing publication exemption | `StreakServiceTests.MissingPublicationDoesNotBreakStreak` | Planned |
| PROG-006 | Longest streak monotonicity | property/unit tests | Planned |

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
| REL-003 | Exactly-once progress | ATT/PROG concurrency suite | Planned |
| REL-004-REL-005 | Community/private-data non-disclosure | COM/MOD/Auth security suites | Planned |
| REL-006-REL-007 | Arabic/source/publish invariants | ADM/DLY publication tests | Planned |
| REL-008-REL-009 | No manual/hardcoded dependency; scope guard | release architecture and UX review | Planned |
| REL-010 | Clean-environment bootstrap/migrations | CI clean-start job | Partial |
| REL-011 | New-user Android journey | `integration_test/new_user_daily_flow_test.dart` | Planned |
| REL-012 | Restart durability | PostgreSQL restart end-to-end test | Partial |
| REL-013 | Publisher/moderator journeys | admin browser end-to-end suite | Planned |
| REL-014 | Cross-user/admin authorization | security integration suite | Planned |
| REL-015-REL-016 | Complete automated/operational gates | Spec 009 evidence pack | Planned |

## Spec 004 - Authentication and Profile

| Requirement(s) | Verification | Planned test/evidence | Status |
|---|---|---|---|
| AUTH-001-AUTH-002 | OIDC PKCE and token validation matrix | test issuer integration + Android auth flow | Planned |
| AUTH-003 | Idempotent issuer/subject bootstrap | `UserBootstrapTests` | Planned |
| AUTH-004 | Verification, consent, and under-13 rejection | auth/profile integration tests | Planned |
| AUTH-005-AUTH-006 | Display name and controlled profile codes | domain validation/property tests | Planned |
| AUTH-007-AUTH-008 | Private defaults and adult-only opt-in | authorization + profile integration tests | Planned |
| AUTH-009-AUTH-010 | Logout/revoke/recovery enumeration safety | provider staging journey + API tests | Planned |
| AUTH-011 | Role audit and MFA policy | admin security test + provider evidence | Planned |
| AUTH-012 | No target-user IDOR | cross-user fuzz/integration suite | Planned |
| AUTH-013 | Deletion lifecycle/deadlines | deletion state tests + staging retention drill | Planned |
| AUTH-014 | Limits and safe errors | rate-limit/problem-contract tests | Planned |
| AUTH-015 | Rename/audit identity behavior | profile/moderation integration tests | Planned |

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
| SHR-001-SHR-002 | Spoiler/private-field absence and ownership | share contract snapshots/security tests | Planned |
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
