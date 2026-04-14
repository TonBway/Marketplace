import 'package:dio/dio.dart';

import 'token_store.dart';

class ApiClient {
  ApiClient({required String baseUrl, required TokenStore tokenStore})
      : _tokenStore = tokenStore,
        dio = Dio(BaseOptions(baseUrl: baseUrl)) {
    dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _tokenStore.readAccessToken();
          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
      ),
    );
  }

  final Dio dio;
  final TokenStore _tokenStore;
}
