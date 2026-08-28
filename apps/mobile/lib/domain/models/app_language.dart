enum AppLanguage {
  arabic(languageCode: 'ar'),
  english(languageCode: 'en');

  const AppLanguage({required this.languageCode});

  final String languageCode;
}
