import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/providers.dart';

// ─── State ───────────────────────────────────────────────────────────────────

class AuthState {
  const AuthState({
    this.isAuthenticated = false,
    this.fullName,
    this.roleCode,
    this.isGuest = false,
  });

  final bool isAuthenticated;
  final String? fullName;
  final String? roleCode;
  final bool isGuest;

  AuthState copyWith({
    bool? isAuthenticated,
    String? fullName,
    String? roleCode,
    bool? isGuest,
  }) =>
      AuthState(
        isAuthenticated: isAuthenticated ?? this.isAuthenticated,
        fullName: fullName ?? this.fullName,
        roleCode: roleCode ?? this.roleCode,
        isGuest: isGuest ?? this.isGuest,
      );
}

// ─── Notifier ─────────────────────────────────────────────────────────────────

class AuthNotifier extends AsyncNotifier<AuthState> {
  @override
  Future<AuthState> build() async {
    // Restore session from secure storage if a token already exists
    final token = await ref.read(tokenStoreProvider).readAccessToken();
    if (token != null && token.isNotEmpty) {
      return const AuthState(isAuthenticated: true);
    }
    return const AuthState();
  }

  Future<void> login(String emailOrPhone, String password) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final resp = await ref.read(authApiProvider).login({
        'emailOrPhone': emailOrPhone,
        'password': password,
      });
      final data = resp.data as Map<String, dynamic>;
      await ref
          .read(tokenStoreProvider)
          .saveTokens(data['accessToken'] as String, data['refreshToken'] as String);
      return AuthState(
        isAuthenticated: true,
        fullName: data['fullName'] as String?,
        roleCode: data['roleCode'] as String?,
      );
    });
  }

  Future<void> register({
    required String fullName,
    required String email,
    required String phone,
    required String password,
  }) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      await ref.read(authApiProvider).register({
        'fullName': fullName,
        'email': email,
        'phone': phone,
        'password': password,
        'roleCode': 'BUYER',
      });
      // Auto-login after successful registration
      final resp = await ref.read(authApiProvider).login({
        'emailOrPhone': email,
        'password': password,
      });
      final data = resp.data as Map<String, dynamic>;
      await ref
          .read(tokenStoreProvider)
          .saveTokens(data['accessToken'] as String, data['refreshToken'] as String);
      return AuthState(
        isAuthenticated: true,
        fullName: data['fullName'] as String?,
        roleCode: data['roleCode'] as String?,
      );
    });
  }

  Future<void> logout() async {
    await ref.read(tokenStoreProvider).clear();
    state = const AsyncData(AuthState());
  }

  Future<void> loginAsGuest() async {
    // Guest login doesn't require token storage, just sets authentication state
    state = const AsyncData(
      AuthState(
        isAuthenticated: true,
        fullName: 'Guest',
        roleCode: 'BUYER',
        isGuest: true,
      ),
    );
  }

  /// Extract a user-friendly message from a Dio error response.
  static String friendlyError(Object error) {
    if (error is DioException) {
      final data = error.response?.data;
      if (data is Map<String, dynamic>) {
        return (data['message'] ?? data['title'] ?? 'Request failed').toString();
      }
      if (data is String && data.isNotEmpty) return data;
      return 'Network error (${error.response?.statusCode ?? 'no response'})';
    }
    return error.toString();
  }
}

