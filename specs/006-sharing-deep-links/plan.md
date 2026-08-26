# Plan 006: Spoiler-Safe Sharing and Deep Links

## Delivery Order

1. Finalize canonical public host and Android package/signing fingerprints per environment.
2. Implement authenticated share-metadata retrieval.
3. Build the deterministic localized Flutter share-card renderer and golden tests.
4. Integrate the Android Sharesheet without storage permission.
5. Configure verified App Links, web fallback, post-auth continuation, and invalid-link handling.
6. Add privacy-safe analytics and end-to-end link tests.

## Validation

- Contract snapshot tests for spoiler/private-field absence.
- Golden tests for supported dimensions and Arabic text.
- Android integration tests for installed and fresh-install link routes.
- Hosted verification of `assetlinks.json` before release.
