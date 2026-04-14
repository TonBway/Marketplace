import 'package:flutter/material.dart';

import '../../../enquiries/presentation/screens/buyer_sent_enquiries_screen.dart';
import '../../../favorites/presentation/screens/buyer_favorites_screen.dart';
import '../../../profile/presentation/screens/buyer_profile_screen.dart';
import '../../../search/presentation/screens/buyer_search_screen.dart';

class BuyerHomeScreen extends StatefulWidget {
  const BuyerHomeScreen({super.key});

  @override
  State<BuyerHomeScreen> createState() => _BuyerHomeScreenState();
}

class _BuyerHomeScreenState extends State<BuyerHomeScreen> {
  int _index = 0;

  final _screens = const [
    BuyerSearchScreen(),
    BuyerFavoritesScreen(),
    BuyerSentEnquiriesScreen(),
    BuyerProfileScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Buyer Marketplace')),
      body: _screens[_index],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (value) => setState(() => _index = value),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.search), label: 'Browse'),
          NavigationDestination(icon: Icon(Icons.favorite), label: 'Favorites'),
          NavigationDestination(icon: Icon(Icons.send), label: 'Enquiries'),
          NavigationDestination(icon: Icon(Icons.person), label: 'Profile'),
        ],
      ),
    );
  }
}
