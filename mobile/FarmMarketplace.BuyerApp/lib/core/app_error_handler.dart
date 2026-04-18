import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

/// Converts any exception into a short, user-friendly message.
String friendlyError(Object? error) {
  if (error is DioException) {
    final status = error.response?.statusCode;
    switch (status) {
      case 400:
        final msg = _extractMessage(error.response?.data);
        return msg ?? 'The request was invalid. Please check your input.';
      case 401:
        return 'Your session has expired. Please sign in again.';
      case 403:
        return 'You don\'t have permission to do that.';
      case 404:
        return 'The requested item could not be found.';
      case 409:
        return 'A conflict occurred. This item may already exist.';
      case 422:
        return 'The data provided was not valid.';
      case 500:
      case 502:
      case 503:
        return 'The server is temporarily unavailable. Please try again later.';
      default:
        if (error.type == DioExceptionType.connectionTimeout ||
            error.type == DioExceptionType.receiveTimeout ||
            error.type == DioExceptionType.sendTimeout) {
          return 'Connection timed out. Please check your internet connection.';
        }
        if (error.type == DioExceptionType.connectionError) {
          return 'Unable to connect to the server. Please check your internet connection.';
        }
        return 'Something went wrong. Please try again.';
    }
  }
  return 'An unexpected error occurred. Please try again.';
}

String? _extractMessage(dynamic data) {
  if (data is Map) {
    final msg = data['message'] ?? data['error'] ?? data['title'];
    if (msg is String && msg.isNotEmpty) return msg;
  }
  return null;
}

/// Shows a friendly error dialog.
Future<void> showErrorDialog(BuildContext context, Object? error) {
  return showDialog<void>(
    context: context,
    builder: (ctx) => AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      title: Row(
        children: [
          Icon(Icons.error_outline_rounded,
              color: Colors.red.shade400, size: 22),
          const SizedBox(width: 8),
          const Text('Something went wrong'),
        ],
      ),
      content: Text(friendlyError(error)),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(ctx).pop(),
          child: const Text('OK'),
        ),
      ],
    ),
  );
}

/// Shows a friendly error snackbar (lightweight — no dialog).
void showErrorSnackBar(BuildContext context, Object? error) {
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Text(friendlyError(error)),
      backgroundColor: Colors.red.shade700,
      behavior: SnackBarBehavior.floating,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
    ),
  );
}
