import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_riverpod/legacy.dart';
import 'package:majlis/data/repositories/app_preferences_repository.dart';
import 'package:majlis/ui/core/app_view_model.dart';

final appPreferencesRepositoryProvider = Provider<AppPreferencesRepository>(
  (ref) => InMemoryAppPreferencesRepository(),
);

final appViewModelProvider = ChangeNotifierProvider<AppViewModel>(
  (ref) => AppViewModel(
    preferencesRepository: ref.watch(appPreferencesRepositoryProvider),
  ),
);
