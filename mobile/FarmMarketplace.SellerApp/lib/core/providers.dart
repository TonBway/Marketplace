import 'package:dio/dio.dart';
import 'package:farm_marketplace_shared_api/shared_api.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../features/auth/data/auth_notifier.dart';

final secureStorageProvider = Provider((_) => const FlutterSecureStorage());

final tokenStoreProvider = Provider<TokenStore>((ref) {
  return TokenStore(ref.watch(secureStorageProvider));
});

final apiClientProvider = Provider<ApiClient>((ref) {
  final client = ApiClient(
    baseUrl: const String.fromEnvironment('API_BASE_URL', defaultValue: 'http://192.168.88.20:5000'),
    tokenStore: ref.watch(tokenStoreProvider),
  );

  // Auto-logout when the server returns 401 (token expired / invalid).
  client.dio.interceptors.add(
    InterceptorsWrapper(
      onError: (DioException error, ErrorInterceptorHandler handler) async {
        final status = error.response?.statusCode;
        if (status == 401 || status == 403) {
          await ref.read(authNotifierProvider.notifier).logout();
        }
        handler.next(error);
      },
    ),
  );

  return client;
});

final authApiProvider = Provider<AuthApi>((ref) {
  return AuthApi(ref.watch(apiClientProvider));
});

final authNotifierProvider =
    AsyncNotifierProvider<AuthNotifier, AuthState>(AuthNotifier.new);
