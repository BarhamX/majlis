import 'package:flutter/widgets.dart';
import 'package:majlis/data/repositories/app_preferences_repository.dart';
import 'package:majlis/domain/models/app_language.dart';

final class AppViewModel extends ChangeNotifier {
  AppViewModel({required AppPreferencesRepository preferencesRepository})
    : _preferencesRepository = preferencesRepository,
      _language = preferencesRepository.loadLanguage();

  final AppPreferencesRepository _preferencesRepository;
  AppLanguage _language;

  AppLanguage get language => _language;

  Locale get locale => Locale(_language.languageCode);

  Future<void> setLanguage(AppLanguage language) async {
    if (_language == language) {
      return;
    }

    await _preferencesRepository.saveLanguage(language);
    _language = language;
    notifyListeners();
  }
}
