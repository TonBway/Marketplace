import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';
import '../../../profile/presentation/screens/buyer_profile_screen.dart';
import '../../../search/presentation/screens/buyer_search_screen.dart';
import '../../../favorites/presentation/screens/buyer_favorites_screen.dart';
import '../../../enquiries/presentation/screens/buyer_sent_enquiries_screen.dart';

class BuyerHomeScreen extends ConsumerStatefulWidget {
  const BuyerHomeScreen({super.key});

  @override
  ConsumerState<BuyerHomeScreen> createState() => _BuyerHomeScreenState();
}

class _BuyerHomeScreenState extends ConsumerState<BuyerHomeScreen> {
  int _index = 0;
  Map<String, dynamic>? _summary;
  bool _loadingSummary = true;

  final _screens = const [
    BuyerSearchScreen(),
    BuyerFavoritesScreen(),
    BuyerSentEnquiriesScreen(),
    BuyerProfileScreen(),
  ];

  @override
  void initState() {
    super.initState();
    _loadSummary();
  }

  Future<void> _loadSummary() async {
    setState(() => _loadingSummary = true);
    try {
      final response = await ref
          .read(apiClientProvider)
          .dio
          .get('/api/dashboard/buyer-summary');
      setState(() {
        _summary = (response.data as Map).cast<String, dynamic>();
      });
    } catch (_) {
      // Keep UI usable even if summary fails.
    } finally {
      if (mounted) {
        setState(() => _loadingSummary = false);
      }
    }
  }

  Future<void> _logout() async {
    await ref.read(authNotifierProvider.notifier).logout();
  }

  Widget _buildSummaryCard() {
    if (_loadingSummary) {
      return const Padding(
        padding: EdgeInsets.fromLTRB(16, 12, 16, 0),
        child: LinearProgressIndicator(),
      );
    }

    final summary = _summary;
    if (summary == null) {
      return const SizedBox.shrink();
    }

    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 12, 16, 0),
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Wrap(
            spacing: 18,
            runSpacing: 8,
            children: [
              Text('Favorites: ${summary['favoriteCount'] ?? 0}'),
              Text('Enquiries: ${summary['sentEnquiries'] ?? 0}'),
              Text('Credits: ${summary['availableCredits'] ?? 0}'),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authNotifierProvider).valueOrNull;
    final isGuest = authState?.isGuest ?? false;
    
    return Scaffold(
      appBar: AppBar(
        title: Text(isGuest ? 'Browse Products (Guest)' : 'Buy Farm Products'),
        actions: [
          IconButton(
            tooltip: 'Refresh summary',
            onPressed: _loadSummary,
            icon: const Icon(Icons.refresh),
          ),
          IconButton(
            tooltip: 'Logout',
            onPressed: _logout,
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: Column(
        children: [
          if (!isGuest) _buildSummaryCard(),
          if (isGuest)
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 0),
              child: Card(
                color: Colors.blue.shade50,
                child: Padding(
                  padding: const EdgeInsets.all(12),
                  child: Row(
                    children: [
                      Icon(Icons.info, size: 20, color: Colors.blue.shade700),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          'Sign in to save favorites and send enquiries',
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          Expanded(child: _screens[_index]),
        ],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (value) => setState(() => _index = value),
        destinations: const [
          NavigationDestination(
              icon: Icon(Icons.search), label: 'Search'),
          NavigationDestination(
              icon: Icon(Icons.favorite), label: 'Favorites'),
          NavigationDestination(
              icon: Icon(Icons.message), label: 'Enquiries'),
          NavigationDestination(icon: Icon(Icons.person), label: 'Profile'),
        ],
      ),
    );
  }
}
