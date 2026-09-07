# Backend Testing Guide

Testing in Majlis is used to protect the daily game loop, cultural-content safety rules, spoiler-safe responses, and clean architecture boundaries.

## Minimum Test Expectations

Every backend feature should include tests for:

- Main success path
- At least one invalid input or missing data case
- Spoiler-safety behavior when relevant
- Duplicate/edge behavior when the feature affects attempts, score, or streaks

## Test Project

```text
src/backend/Majlis.Tests/
```

Use the existing test framework and patterns already present in the project.

## Commands

From repository root:

```powershell
cd src/backend
dotnet test Majlis.sln
```

Release-mode test:

```powershell
cd src/backend
dotnet test Majlis.sln --configuration Release
```

Build then test without rebuild:

```powershell
cd src/backend
dotnet build Majlis.sln --configuration Release
dotnet test Majlis.sln --configuration Release --no-build
```

Format check:

```powershell
cd src/backend
dotnet format Majlis.sln --verify-no-changes --no-restore
```

## Test Categories

### Domain Tests

Use for pure rules.

Examples:

- Challenge option validation
- Correct answer evaluation
- Streak update rules
- Daily completion rules
- Cultural content status transitions

### Application Tests

Use for use-case behavior.

Examples:

- Fetch today's Daily Majlis
- Submit answer
- Reject duplicate daily attempt
- Return result explanation after answer submission
- Prevent correct-answer leak before submission

### API Tests

Use for HTTP behavior once endpoint coverage is needed.

Examples:

- `GET /api/v1/daily-majlis/today` returns 200
- Error response shape is stable
- DTO fields match mobile expectations

## Spoiler-Safety Tests

Any endpoint that returns challenge data before submission must verify that the response does not include:

- Correct option ID
- `isCorrect` flags
- Explanation that identifies the answer
- Internal answer metadata

Example test intent:

```text
GetTodayDailyMajlis_ReturnsChallengeOptionsWithoutCorrectAnswerMetadata
```

## Date and Clock Tests

Daily Majlis behavior must be deterministic in tests.

Prefer a clock/date abstraction rather than directly calling system time inside business logic.

Test cases should cover:

- Today has scheduled content
- Today has no scheduled content and fallback behavior applies
- Date boundary behavior later when user timezone support is added

## Pull Request Quality Gate

A backend PR should report:

```text
dotnet format src/backend/Majlis.sln --verify-no-changes --no-restore
dotnet build src/backend/Majlis.sln --configuration Release --no-restore
dotnet test src/backend/Majlis.sln --configuration Release --no-build
```

If a command cannot be run, the PR must explain why.

## Test Naming

Use readable behavior names:

```text
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:

```text
GetTodayAsync_WhenContentExists_ReturnsSpoilerSafeDailyMajlis
SubmitAttemptAsync_WhenOptionIsCorrect_ReturnsCorrectResultAndScore
SubmitAttemptAsync_WhenAttemptAlreadyExists_RejectsDuplicateScoring
```

## What Not To Test

Avoid brittle tests for:

- Exact JSON property ordering
- Framework-generated wiring unless there is a real behavior risk
- Copy wording unless the copy is contractual or safety-sensitive
