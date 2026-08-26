# Plan 007: User-Controlled Daily Reminder

## Delivery Order

1. Add reminder preferences to the profile contract and persistence.
2. Add permission education and settings UI.
3. Implement an Android local-reminder scheduler behind a platform abstraction.
4. Reconcile schedules on startup, reboot, upgrade, timezone change, logout, and preference updates.
5. Route taps through the sharing/deep-link destination handler.
6. Add consent-aware analytics and emulator/device verification.

## Validation

- Unit tests for next-occurrence calculation across timezone and daylight-saving changes.
- Widget tests for opt-in, denial, settings, and Arabic copy.
- Android integration tests for runtime permission, replacement, cancellation, and tap routing.
