import 'package:majlis/domain/models/app_language.dart';

abstract interface class AppPreferencesRepository {
  AppLanguage loadLanguage();

  Future<void> saveLanguage(AppLanguage language);
}

final class InMemoryAppPreferencesRepository
    implements AppPreferencesRepository {
  InMemoryAppPreferencesRepository({
    AppLanguage initialLanguage = AppLanguage.arabic,
  }) : _language = initialLanguage;

  AppLanguage _language;

  @override
  AppLanguage loadLanguage() => _language;

  @override
  Future<void> saveLanguage(AppLanguage language) async {
    _language = language;
  }
}
