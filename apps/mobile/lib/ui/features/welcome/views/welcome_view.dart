import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:majlis/core/theme/majlis_colors.dart';
import 'package:majlis/core/theme/majlis_spacing.dart';
import 'package:majlis/l10n/generated/app_localizations.dart';
import 'package:majlis/ui/features/welcome/widgets/majlis_arch_mark.dart';

class WelcomeView extends StatelessWidget {
  const WelcomeView({super.key});

  @override
  Widget build(BuildContext context) {
    final localizations = AppLocalizations.of(context);

    return Scaffold(
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            final horizontalPadding = constraints.maxWidth >= 600
                ? MajlisSpacing.xLarge
                : MajlisSpacing.medium;
            final markSize = math.min(constraints.maxWidth * 0.58, 248.0);

            return SingleChildScrollView(
              padding: EdgeInsets.fromLTRB(
                horizontalPadding,
                MajlisSpacing.medium,
                horizontalPadding,
                MajlisSpacing.large,
              ),
              child: ConstrainedBox(
                constraints: BoxConstraints(
                  minHeight: constraints.maxHeight - MajlisSpacing.xLarge,
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    _Header(
                      appTitle: localizations.appTitle,
                      todayLabel: localizations.todayMajlisLabel,
                    ),
                    Padding(
                      padding: const EdgeInsets.symmetric(
                        vertical: MajlisSpacing.large,
                      ),
                      child: MajlisArchMark(
                        semanticLabel: localizations.majlisSymbolSemantics,
                        size: markSize,
                      ),
                    ),
                    _WelcomeCopy(
                      eyebrow: localizations.welcomeEyebrow,
                      title: localizations.welcomeTitle,
                      body: localizations.welcomeBody,
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}

class _Header extends StatelessWidget {
  const _Header({required this.appTitle, required this.todayLabel});

  final String appTitle;
  final String todayLabel;

  @override
  Widget build(BuildContext context) {
    return Wrap(
      alignment: WrapAlignment.spaceBetween,
      crossAxisAlignment: WrapCrossAlignment.center,
      spacing: MajlisSpacing.medium,
      runSpacing: MajlisSpacing.small,
      children: [
        Text(
          appTitle,
          style: Theme.of(context).textTheme.titleLarge?.copyWith(
            color: MajlisColors.deepCoffee,
            fontWeight: FontWeight.w800,
          ),
        ),
        DecoratedBox(
          decoration: BoxDecoration(
            color: MajlisColors.softSand,
            border: Border.all(color: MajlisColors.majlisAmber),
            borderRadius: BorderRadius.circular(999),
          ),
          child: Padding(
            padding: const EdgeInsets.symmetric(
              horizontal: MajlisSpacing.small,
              vertical: MajlisSpacing.xSmall,
            ),
            child: Text(
              todayLabel,
              style: Theme.of(context).textTheme.labelLarge,
            ),
          ),
        ),
      ],
    );
  }
}

class _WelcomeCopy extends StatelessWidget {
  const _WelcomeCopy({
    required this.eyebrow,
    required this.title,
    required this.body,
  });

  final String eyebrow;
  final String title;
  final String body;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        border: Border(
          top: BorderSide(color: MajlisColors.majlisAmber, width: 2),
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.only(top: MajlisSpacing.medium),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(eyebrow, style: Theme.of(context).textTheme.labelLarge),
            const SizedBox(height: MajlisSpacing.small),
            Text(title, style: Theme.of(context).textTheme.displaySmall),
            const SizedBox(height: MajlisSpacing.medium),
            Text(body, style: Theme.of(context).textTheme.bodyLarge),
          ],
        ),
      ),
    );
  }
}
