import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:majlis/core/routing/app_router.dart';
import 'package:majlis/core/theme/majlis_theme.dart';
import 'package:majlis/l10n/generated/app_localizations.dart';
import 'package:majlis/ui/core/app_providers.dart';

class MajlisApp extends ConsumerWidget {
  const MajlisApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final appViewModel = ref.watch(appViewModelProvider);
    final router = ref.watch(appRouterProvider);

    return MaterialApp.router(
      onGenerateTitle: (context) => AppLocalizations.of(context).appTitle,
      debugShowCheckedModeBanner: false,
      locale: appViewModel.locale,
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      theme: MajlisTheme.light(appViewModel.language),
      themeMode: ThemeMode.light,
      routerConfig: router,
    );
  }
}
