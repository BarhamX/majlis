# Spec 007: User-Controlled Daily Reminder

## Goal

Support a voluntary daily ritual with one predictable local reminder and no pressure, surveillance, or notification spam.

## Scope

- Android notification permission education and request.
- Off-by-default reminder preference, local time, and IANA timezone.
- Local scheduling, rescheduling, cancellation, and deep-link routing.
- Reboot, timezone, daylight-saving, app-update, and permission-revocation behavior.

Remote campaigns, friend-activity notifications, streak-loss warnings, marketing pushes, and operator broadcast notifications are post-V1.

## Requirements

- **NTF-001**: Reminders shall default to disabled. The app shall explain the value before requesting Android notification permission and shall continue normally after denial.
- **NTF-002**: A user may enable one daily reminder, choose a local time in 15-minute increments, and disable it in one action.
- **NTF-003**: The preference shall store `Enabled`, local time, IANA timezone, and update time in the user profile so it survives reinstall; the Android device shall own actual local scheduling.
- **NTF-004**: The app shall schedule at most one future daily challenge notification and replace, not duplicate, an existing schedule after preference, timezone, or permission changes.
- **NTF-005**: The reminder shall use warm neutral copy and shall not mention a lost streak, another user's activity, rank loss, urgency, guilt, or an unverified claim that new content is available.
- **NTF-006**: Tapping a reminder shall use the same safe route as today's verified deep link and shall pass through authentication when required.
- **NTF-007**: On reboot, app upgrade, timezone change, or daylight-saving transition, the next reminder shall be recomputed in the saved local timezone without producing a duplicate.
- **NTF-008**: Disabling reminders, signing out, requesting account deletion, or revoking notification permission shall cancel scheduled notifications immediately on that device.
- **NTF-009**: Reminder analytics shall record scheduled, opened, and disabled events only with consent and no notification text, cultural answers, or comment content.
- **NTF-010**: Notification behavior shall pass Android 13+ runtime-permission tests and remain usable on supported pre-Android-13 devices.

## Acceptance Criteria

- No reminder appears before explicit opt-in and platform permission.
- One enabled preference cannot create duplicate notifications across rescheduling events.
- Denial/revocation, timezone changes, reboot, logout, and deletion have verified recovery behavior.
- Arabic copy and RTL destination screens are tested.
