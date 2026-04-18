import 'package:farm_marketplace_shared_api/shared_api.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../features/auth/data/auth_notifier.dart';

final secureStorageProvider = Provider((_) => const FlutterSecureStorage());

final tokenStoreProvider = Provider<TokenStore>((ref) {
  return TokenStore(ref.watch(secureStorageProvider));
});

final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient(
    baseUrl: const String.fromEnvironment('API_BASE_URL', defaultValue: 'http://192.168.88.20:5000'),
    tokenStore: ref.watch(tokenStoreProvider),
  );
});

final authApiProvider = Provider<AuthApi>((ref) {
  return AuthApi(ref.watch(apiClientProvider));
});

final authNotifierProvider =
    AsyncNotifierProvider<AuthNotifier, AuthState>(AuthNotifier.new);
