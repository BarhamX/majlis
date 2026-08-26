# Spec 005: Privacy-Safe Weekly Leaderboard

## Goal

Offer friendly competition without exposing private activity, minors, region, contact relationships, or family-group membership.

## Scope

- One global weekly leaderboard using UTC boundaries.
- Adult opt-in/opt-out.
- Top 100 plus the eligible requesting user's own entry.
- Approved display name, rank, and weekly XP only.

Private family/friend boards, regional boards, direct challenges, avatars, and public attempt history are post-V1.

## Requirements

- **LDB-001**: The leaderboard week shall start Monday at `00:00:00Z` and end at the next Monday; XP shall be included by immutable XP-ledger occurrence time.
- **LDB-002**: Visibility shall default to `private`. Only an authenticated `18_plus` user may opt into `global_weekly`, and opting out shall remove the user from subsequent reads within one minute.
- **LDB-003**: An entry shall expose only displayed rank, current approved display name, and weekly XP. It shall not expose user id, age band, email, region, dialect, country, streak, attempt details, or last-active time.
- **LDB-004**: The response shall contain at most the top 100 eligible entries and the requesting user's entry when eligible and outside that set.
- **LDB-005**: Equal XP shall receive the same competition rank. Internal ordering for pagination shall use XP descending, XP-achieved timestamp ascending, then user id, without exposing the tie breakers.
- **LDB-006**: XP shall contribute at most once per attempt by referencing the unique XP-ledger event created with that attempt.
- **LDB-007**: Blocked relationships shall be filtered in both directions for an authenticated viewer without changing rank numbers.
- **LDB-008**: Display-name moderation, account suspension, deletion, or privacy changes shall remove or update the entry without rewriting XP history.
- **LDB-009**: Leaderboard copy shall not shame low rank, imply cultural worth, or create streak-loss pressure.
- **LDB-010**: Responses may be cached for at most 60 seconds and shall never be served across authorization/privacy policy changes beyond that bound.

## Acceptance Criteria

- A private or minor profile never appears.
- Opt-in, opt-out, block, rename, suspension, and deletion are reflected within one minute.
- Duplicate or concurrent attempts cannot inflate weekly XP.
- Ties and week rollover are deterministic at UTC boundaries.
- API, widget, accessibility, and Arabic/RTL tests cover the empty, top-100, own-rank, and ineligible states.
