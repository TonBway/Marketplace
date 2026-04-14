import 'package:flutter/material.dart';

import 'features/dashboard/presentation/screens/seller_dashboard_screen.dart';

class SellerApp extends StatelessWidget {
  const SellerApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Farm Marketplace Seller',
      theme: ThemeData(colorSchemeSeed: Colors.green, useMaterial3: true),
      home: const SellerDashboardScreen(),
    );
  }
}
