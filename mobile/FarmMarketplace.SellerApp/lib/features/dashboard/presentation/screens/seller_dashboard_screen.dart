import 'package:flutter/material.dart';

import '../../../enquiries/presentation/screens/seller_enquiries_screen.dart';
import '../../../listings/presentation/screens/seller_listings_screen.dart';
import '../../../profile/presentation/screens/seller_profile_screen.dart';
import '../../../subscriptions/presentation/screens/seller_subscriptions_screen.dart';

class SellerDashboardScreen extends StatefulWidget {
  const SellerDashboardScreen({super.key});

  @override
  State<SellerDashboardScreen> createState() => _SellerDashboardScreenState();
}

class _SellerDashboardScreenState extends State<SellerDashboardScreen> {
  int _index = 0;

  final _screens = const [
    SellerListingsScreen(),
    SellerEnquiriesScreen(),
    SellerSubscriptionsScreen(),
    SellerProfileScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Seller Dashboard')),
      body: _screens[_index],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (value) => setState(() => _index = value),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.storefront), label: 'Listings'),
          NavigationDestination(icon: Icon(Icons.question_answer), label: 'Enquiries'),
          NavigationDestination(icon: Icon(Icons.credit_card), label: 'Plans'),
          NavigationDestination(icon: Icon(Icons.person), label: 'Profile'),
        ],
      ),
    );
  }
}
