import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:majlis/ui/features/welcome/views/welcome_view.dart';

abstract final class AppRoutes {
  static const welcome = '/';
}

final appRouterProvider = Provider<GoRouter>((ref) {
  final router = GoRouter(
    initialLocation: AppRoutes.welcome,
    routes: [
      GoRoute(
        path: AppRoutes.welcome,
        builder: (context, state) => const WelcomeView(),
      ),
    ],
  );
  ref.onDispose(router.dispose);
  return router;
});
