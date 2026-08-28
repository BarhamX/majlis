# Majlis Android App

Flutter client for the Majlis Production V1 Android application.

## Local Toolchain

- Flutter 3.47.1 / Dart 3.13.1
- Android application id: `com.barhamx.majlis`
- Development emulator: `Majlis_API_35`

## Architecture

The app keeps data access, domain models, and presentation separate:

```text
lib/
  core/                 Routing and theme tokens
  data/                 Repository implementations and future API services
  domain/               Framework-independent models
  l10n/                 Arabic/English ARB resources and generated accessors
  ui/core/              App-level Riverpod providers and view models
  ui/features/          Feature-grouped views, widgets, and view models
```

Riverpod provides dependency injection around MVVM `ChangeNotifier` view
models. GoRouter owns navigation. Arabic is the default locale and derives RTL
through Flutter localization delegates; English remains available for testing
and later user preference support.

## Commands

```powershell
C:\Users\ybarham\development\flutter\bin\flutter.bat pub get
C:\Users\ybarham\development\flutter\bin\flutter.bat gen-l10n
C:\Users\ybarham\development\flutter\bin\flutter.bat analyze
C:\Users\ybarham\development\flutter\bin\flutter.bat test
C:\Users\ybarham\development\flutter\bin\flutter.bat build apk --debug
```

Noto Sans Arabic is bundled under the SIL Open Font License in
`assets/fonts/OFL.txt`.
