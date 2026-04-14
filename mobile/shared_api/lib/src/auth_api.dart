import 'package:dio/dio.dart';

import 'api_client.dart';

class AuthApi {
  AuthApi(this._client);

  final ApiClient _client;

  Future<Response<dynamic>> register(Map<String, dynamic> payload) {
    return _client.dio.post('/api/auth/register', data: payload);
  }

  Future<Response<dynamic>> login(Map<String, dynamic> payload) {
    return _client.dio.post('/api/auth/login', data: payload);
  }

  Future<Response<dynamic>> refresh(String refreshToken) {
    return _client.dio.post('/api/auth/refresh', data: {'refreshToken': refreshToken});
  }
}
