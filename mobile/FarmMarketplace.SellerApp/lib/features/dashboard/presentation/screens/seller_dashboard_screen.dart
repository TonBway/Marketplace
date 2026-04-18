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
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(18),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.06),
              blurRadius: 10,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Overview',
              style: TextStyle(fontWeight: FontWeight.w800, fontSize: 16),
            ),
            const SizedBox(height: 10),
            Row(
              children: [
                Expanded(
                  child: _MetricTile(
                    label: 'Active Listings',
                    value: '${summary['activeListings'] ?? 0}',
                    icon: Icons.storefront_rounded,
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: _MetricTile(
                    label: 'Enquiries',
                    value: '${summary['receivedEnquiries'] ?? 0}',
                    icon: Icons.chat_bubble_outline_rounded,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            _MetricTile(
              label: 'Current Plan',
              value: '${summary['activePlanName'] ?? 'None'}',
              icon: Icons.workspace_premium_outlined,
              compact: true,
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Seller Hub'),
        automaticallyImplyLeading: false,
        leading: PopupMenuButton<String>(
          icon: const Icon(Icons.menu_rounded),
          tooltip: 'Menu',
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
          onSelected: (value) {
            if (value == 'refresh') {
              _loadSummary();
            } else if (value == 'plans') {
              Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (_) => const SellerSubscriptionsScreen(),
                ),
              );
            } else if (value == 'logout') {
              _logout();
            }
          },
          itemBuilder: (context) => const [
            PopupMenuItem(
              value: 'refresh',
              child: Row(
                children: [
                  Icon(Icons.refresh_rounded),
                  SizedBox(width: 10),
                  Text('Refresh'),
                ],
              ),
            ),
            PopupMenuItem(
              value: 'plans',
              child: Row(
                children: [
                  Icon(Icons.workspace_premium_outlined),
                  SizedBox(width: 10),
                  Text('Plans'),
                ],
              ),
            ),
            PopupMenuDivider(),
            PopupMenuItem(
              value: 'logout',
              child: Row(
                children: [
                  Icon(Icons.logout_rounded, color: Colors.redAccent),
                  SizedBox(width: 10),
                  Text('Logout', style: TextStyle(color: Colors.redAccent)),
                ],
              ),
            ),
          ],
        ),
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
              icon: Icon(Icons.storefront_rounded), label: 'Listings'),
          NavigationDestination(
              icon: Icon(Icons.question_answer_rounded), label: 'Enquiries'),
          NavigationDestination(icon: Icon(Icons.person_rounded), label: 'Profile'),
        ],
      ),
    );
  }
}

class _MetricTile extends StatelessWidget {
  const _MetricTile({
    required this.label,
    required this.value,
    required this.icon,
    this.compact = false,
  });

  final String label;
  final String value;
  final IconData icon;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(
        horizontal: 10,
        vertical: compact ? 10 : 12,
      ),
      decoration: BoxDecoration(
        color: const Color(0xFFF5F8EF),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Icon(icon, color: const Color(0xFF8DC63F), size: 20),
          const SizedBox(width: 8),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: const TextStyle(fontSize: 12, color: Colors.black54),
                ),
                const SizedBox(height: 2),
                Text(
                  value,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontWeight: FontWeight.w800,
                    color: Color(0xFF2E3138),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
