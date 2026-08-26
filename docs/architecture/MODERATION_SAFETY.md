# Majlis Moderation and Safety Strategy

## Safety Goal

Majlis must feel like a respectful majlis. Users can disagree, joke, and compete, but the app cannot allow insults, sectarian attacks, racism, harassment, or regional humiliation.

## Production V1 Controls

- Report comment.
- Premoderate every new or edited comment before public visibility.
- Approve, hide, restore, or remove through an audited moderator action.
- Admin moderation queue.
- Comment status field.
- Endpoint-specific rate limits.
- Community rules page.
- Two-way interaction filtering after a user block.
- User-visible moderation status and one appeal within 30 days.
- Account suspension and deletion-aware filtering.

## Comment Statuses

- `visible`: comment is shown.
- `pending`: comment awaits review.
- `hidden`: comment is hidden from users.
- `removed`: comment removed for policy violation.

`pending` is the mandatory initial state. A report does not automatically publish, hide, or remove content; only an authorized moderation action changes visibility.

## Report Reasons

- abusive_or_disrespectful
- sectarian_or_racist
- harassment
- spam
- misinformation_or_fake_cultural_claim
- other

## Community Rules

1. Challenge ideas, not people.
2. No insults against families, tribes, regions, nationalities, sects, or dialects.
3. Do not present disputed cultural claims as attacks.
4. Keep discussion relevant to the daily Majlis.
5. Report problems instead of escalating arguments.

## Moderation Priority

Highest priority:

- Hate or sectarian content.
- Harassment.
- Threats.
- Doxxing/private information.
- Repeated spam.

Queue service targets:

- Credible threats or exposed private information: acknowledge within 1 hour and escalate immediately.
- Hate/sectarian abuse and harassment: decision within 4 hours.
- Other pending comments, reports, and appeals: 95% receive a decision within 12 hours, 24 hours, and 5 calendar days respectively.

If staffing cannot meet these targets, public discussion must remain disabled while the daily challenge continues.

## Minor Safety

- Accounts are 13+ and store an age band, not full date of birth.
- Minor profiles are private and excluded from the public leaderboard.
- V1 has no direct messaging, private group, contact upload, precise location, or public activity history.
- Display-name guidance tells users not to use a full legal name or contact information.
- Comment input and moderation reject personal contact details, doxxing, sexual exploitation, threats, and attempts to move minors into private contact.

## Blocking and Appeals

- Blocking is private and does not notify the target.
- Either-direction blocks remove comments/reactions between the users from consumer views without revealing why.
- A hide, removal, or account suspension may be appealed once within 30 days.
- An appeal is decided by a moderator other than the original actor; both events remain immutable.

## Retention

- Deleted comments leave public results immediately.
- Account deletion and moderation evidence follow `V1-DEC-009` in `docs/product/v1-product-decisions.md`.
- Metrics and analytics never contain comment/report/appeal text, reporter identity, or moderation notes.

## Design Implication

Competition should be between answers and knowledge, not personal worth. Copy must avoid lines like "you know nothing about your culture" in direct user feedback. Use playful provocation instead.
