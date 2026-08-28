// ignore: unused_import
import 'package:intl/intl.dart' as intl;

import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Arabic (`ar`).
class AppLocalizationsAr extends AppLocalizations {
  AppLocalizationsAr([String locale = 'ar']) : super(locale);

  @override
  String get appTitle => 'مجلس';

  @override
  String get welcomeEyebrow => 'موعد يومي مع الثقافة';

  @override
  String get welcomeTitle => 'المجلس يبدأ بسؤال';

  @override
  String get welcomeBody =>
      'اكتشف حكايةً ومثلاً جديداً كل يوم، وشارك ما تعلّمته بلا حرق للإجابة.';

  @override
  String get todayMajlisLabel => 'مجلس اليوم';

  @override
  String get majlisSymbolSemantics => 'بوابة مجلس مضاءة';
}
