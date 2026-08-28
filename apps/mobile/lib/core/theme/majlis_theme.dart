import 'package:flutter/material.dart';
import 'package:majlis/core/theme/majlis_colors.dart';
import 'package:majlis/domain/models/app_language.dart';

abstract final class MajlisTheme {
  static const arabicFontFamily = 'NotoSansArabic';

  static ThemeData light(AppLanguage language) {
    const colorScheme = ColorScheme.light(
      primary: MajlisColors.deepCoffee,
      onPrimary: MajlisColors.softSand,
      secondary: MajlisColors.majlisAmber,
      onSecondary: MajlisColors.deepCoffee,
      surface: MajlisColors.softSand,
      onSurface: MajlisColors.nightNavy,
      error: MajlisColors.incorrect,
      onError: Colors.white,
      outline: MajlisColors.dateBrown,
    );
    final fontFamily = language == AppLanguage.arabic ? arabicFontFamily : null;
    final textTheme = ThemeData.light().textTheme
        .apply(
          fontFamily: fontFamily,
          bodyColor: MajlisColors.nightNavy,
          displayColor: MajlisColors.deepCoffee,
        )
        .copyWith(
          displaySmall: TextStyle(
            color: MajlisColors.deepCoffee,
            fontFamily: fontFamily,
            fontSize: 40,
            fontWeight: FontWeight.w700,
            height: 1.22,
            letterSpacing: language == AppLanguage.arabic ? 0 : -1,
          ),
          bodyLarge: TextStyle(
            color: MajlisColors.nightNavy,
            fontFamily: fontFamily,
            fontSize: 18,
            fontWeight: FontWeight.w400,
            height: 1.7,
          ),
          labelLarge: TextStyle(
            color: MajlisColors.dateBrown,
            fontFamily: fontFamily,
            fontSize: 14,
            fontWeight: FontWeight.w700,
            letterSpacing: language == AppLanguage.arabic ? 0 : 0.7,
          ),
        );

    return ThemeData(
      useMaterial3: true,
      colorScheme: colorScheme,
      scaffoldBackgroundColor: MajlisColors.sand,
      fontFamily: fontFamily,
      textTheme: textTheme,
      appBarTheme: const AppBarTheme(
        backgroundColor: Colors.transparent,
        foregroundColor: MajlisColors.deepCoffee,
        elevation: 0,
      ),
      focusColor: MajlisColors.majlisAmber,
      splashColor: MajlisColors.majlisAmber.withValues(alpha: 0.14),
    );
  }
}
