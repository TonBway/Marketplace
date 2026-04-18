import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';

import '../../../enquiries/presentation/screens/seller_enquiries_screen.dart';
import '../../../listings/presentation/screens/seller_listings_screen.dart';
import '../../../profile/presentation/screens/seller_profile_screen.dart';
import '../../../subscriptions/presentation/screens/seller_subscriptions_screen.dart';

class SellerDashboardScreen extends ConsumerStatefulWidget {
  const SellerDashboardScreen({super.key});

  @override
  ConsumerState<SellerDashboardScreen> createState() =>
      _SellerDashboardScreenState();
}

class _SellerDashboardScreenState extends ConsumerState<SellerDashboardScreen> {
  int _index = 0;
  Map<String, dynamic>? _summary;
  bool _loadingSummary = true;

  final _screens = const [
    SellerListingsScreen(),
    SellerEnquiriesScreen(),
    SellerSubscriptionsScreen(),
    SellerProfileScreen(),
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
          .get('/api/dashboard/seller-summary');
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
              Text('Active listings: ${summary['activeListings'] ?? 0}'),
              Text('Enquiries: ${summary['receivedEnquiries'] ?? 0}'),
              Text('Plan: ${summary['activePlanName'] ?? 'None'}'),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Seller Dashboard'),
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
          _buildSummaryCard(),
          Expanded(child: _screens[_index]),
        ],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (value) => setState(() => _index = value),
        destinations: const [
          NavigationDestination(
              icon: Icon(Icons.storefront), label: 'Listings'),
          NavigationDestination(
              icon: Icon(Icons.question_answer), label: 'Enquiries'),
          NavigationDestination(icon: Icon(Icons.credit_card), label: 'Plans'),
          NavigationDestination(icon: Icon(Icons.person), label: 'Profile'),
        ],
      ),
    );
  }
}
