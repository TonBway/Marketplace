import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'core/providers.dart';
import 'features/auth/presentation/screens/buyer_login_screen.dart';
import 'features/home/presentation/screens/buyer_home_screen.dart';

class BuyerApp extends ConsumerWidget {
  const BuyerApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authAsync = ref.watch(authNotifierProvider);

    return MaterialApp(
      title: 'Farm Marketplace Buyer',
      theme: ThemeData(colorSchemeSeed: Colors.orange, useMaterial3: true),
      home: authAsync.when(
        loading: () => const Scaffold(
          body: Center(child: CircularProgressIndicator()),
        ),
        error: (_, __) => const BuyerLoginScreen(),
        data: (auth) =>
            auth.isAuthenticated ? const BuyerHomeScreen() : const BuyerLoginScreen(),
      ),
    );
  }
}
