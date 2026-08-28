# Plan 006: Spoiler-Safe Sharing and Deep Links

## Delivery Order

1. Implement authenticated share-metadata retrieval with a configurable link origin.
2. Build the deterministic localized Flutter share-card renderer and golden tests.
3. Integrate the Android Sharesheet without storage permission.
4. Implement and test internal route parsing plus current/expired/invalid states against a local placeholder origin.
5. After `Game Ready`, finalize canonical public hosts and Android package/signing fingerprints, then configure verified App Links and web fallback.
6. Add privacy-safe analytics and hosted end-to-end link tests.

## Validation

- Contract snapshot tests for spoiler/private-field absence.
- Golden tests for supported dimensions and Arabic text.
- Android integration tests for installed and fresh-install link routes.
- Hosted verification of `assetlinks.json` during the deferred logistics phase and before release.
