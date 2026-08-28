import 'package:flutter_test/flutter_test.dart';
import 'package:majlis/data/repositories/app_preferences_repository.dart';
import 'package:majlis/domain/models/app_language.dart';
import 'package:majlis/ui/core/app_view_model.dart';

void main() {
  test('loads the language from the repository and persists changes', () async {
    final repository = _FakeAppPreferencesRepository(AppLanguage.arabic);
    final viewModel = AppViewModel(preferencesRepository: repository);
    var notifications = 0;
    viewModel.addListener(() => notifications++);

    expect(viewModel.language, AppLanguage.arabic);

    await viewModel.setLanguage(AppLanguage.english);

    expect(viewModel.language, AppLanguage.english);
    expect(repository.language, AppLanguage.english);
    expect(notifications, 1);
  });
}

final class _FakeAppPreferencesRepository implements AppPreferencesRepository {
  _FakeAppPreferencesRepository(this.language);

  AppLanguage language;

  @override
  AppLanguage loadLanguage() => language;

  @override
  Future<void> saveLanguage(AppLanguage value) async {
    language = value;
  }
}
