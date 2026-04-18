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
    final theme = ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: const Color(0xFF8DC63F),
        brightness: Brightness.light,
      ),
      scaffoldBackgroundColor: const Color(0xFFF4F5F7),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide.none,
        ),
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
      ),
      cardTheme: const CardThemeData(
        color: Colors.white,
        elevation: 0,
        margin: EdgeInsets.zero,
      ),
    );

    return MaterialApp(
      title: 'Farm Marketplace Buyer',
      debugShowCheckedModeBanner: false,
      theme: theme,
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
