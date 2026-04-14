import 'package:flutter/material.dart';

import 'features/home/presentation/screens/buyer_home_screen.dart';

class BuyerApp extends StatelessWidget {
  const BuyerApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Farm Marketplace Buyer',
      theme: ThemeData(colorSchemeSeed: Colors.orange, useMaterial3: true),
      home: const BuyerHomeScreen(),
    );
  }
}
