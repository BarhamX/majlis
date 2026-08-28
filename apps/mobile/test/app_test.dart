import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:majlis/app.dart';
import 'package:majlis/data/repositories/app_preferences_repository.dart';
import 'package:majlis/domain/models/app_language.dart';
import 'package:majlis/ui/core/app_providers.dart';

void main() {
  testWidgets('starts in localized Arabic with RTL and the bundled typeface', (
    tester,
  ) async {
    await tester.pumpWidget(const ProviderScope(child: MajlisApp()));
    await tester.pumpAndSettle();

    final title = find.text('المجلس يبدأ بسؤال');
    expect(title, findsOneWidget);
    expect(find.text('موعد يومي مع الثقافة'), findsOneWidget);
    expect(find.bySemanticsLabel('بوابة مجلس مضاءة'), findsOneWidget);
    expect(Directionality.of(tester.element(title)), TextDirection.rtl);
    expect(
      Theme.of(tester.element(title)).textTheme.bodyMedium?.fontFamily,
      'NotoSansArabic',
    );
    expect(tester.takeException(), isNull);
  });

  testWidgets('derives English copy and LTR direction from app state', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          appPreferencesRepositoryProvider.overrideWithValue(
            _FixedAppPreferencesRepository(AppLanguage.english),
          ),
        ],
        child: const MajlisApp(),
      ),
    );
    await tester.pumpAndSettle();

    final title = find.text('Every Majlis begins with a question');
    expect(title, findsOneWidget);
    expect(find.text('A daily gathering around culture'), findsOneWidget);
    expect(Directionality.of(tester.element(title)), TextDirection.ltr);
    expect(
      Theme.of(tester.element(title)).textTheme.bodyMedium?.fontFamily,
      isNot('NotoSansArabic'),
    );
    expect(tester.takeException(), isNull);
  });

  testWidgets('keeps Arabic content reachable at 200 percent text scale', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(320, 640);
    tester.view.devicePixelRatio = 1;
    tester.platformDispatcher.textScaleFactorTestValue = 2;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    addTearDown(tester.platformDispatcher.clearTextScaleFactorTestValue);

    await tester.pumpWidget(const ProviderScope(child: MajlisApp()));
    await tester.pumpAndSettle();

    expect(find.text('المجلس يبدأ بسؤال'), findsOneWidget);
    expect(find.text('مجلس اليوم'), findsOneWidget);
    expect(find.byType(SingleChildScrollView), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
}

final class _FixedAppPreferencesRepository implements AppPreferencesRepository {
  _FixedAppPreferencesRepository(this._language);

  AppLanguage _language;

  @override
  AppLanguage loadLanguage() => _language;

  @override
  Future<void> saveLanguage(AppLanguage language) async {
    _language = language;
  }
}
