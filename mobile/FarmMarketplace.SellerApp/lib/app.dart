import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'core/providers.dart';
import 'features/auth/presentation/screens/seller_login_screen.dart';
import 'features/dashboard/presentation/screens/seller_dashboard_screen.dart';

class SellerApp extends ConsumerWidget {
  const SellerApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authAsync = ref.watch(authNotifierProvider);

    return MaterialApp(
      title: 'Farm Marketplace Seller',
      theme: ThemeData(colorSchemeSeed: Colors.green, useMaterial3: true),
      home: authAsync.when(
        loading: () => const Scaffold(
          body: Center(child: CircularProgressIndicator()),
        ),
        error: (_, __) => const SellerLoginScreen(),
        data: (auth) =>
            auth.isAuthenticated ? const SellerDashboardScreen() : const SellerLoginScreen(),
      ),
    );
  }
}
