// ignore: unused_import
import 'package:intl/intl.dart' as intl;

import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'Majlis';

  @override
  String get welcomeEyebrow => 'A daily gathering around culture';

  @override
  String get welcomeTitle => 'Every Majlis begins with a question';

  @override
  String get welcomeBody =>
      'Discover a new story or proverb each day, then share what you learned without spoilers.';

  @override
  String get todayMajlisLabel => 'Today\'s Majlis';

  @override
  String get majlisSymbolSemantics => 'An illuminated Majlis doorway';
}
