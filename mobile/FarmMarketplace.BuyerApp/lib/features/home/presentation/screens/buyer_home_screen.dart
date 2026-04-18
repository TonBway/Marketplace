import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';
import '../../../bag/data/bag_notifier.dart';
import '../../../bag/presentation/screens/buyer_bag_screen.dart';
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

  Future<void> _logout() async {
    await ref.read(authNotifierProvider.notifier).logout();
  }

  Widget _buildDrawer(BuildContext context, bool isGuest) {
    final authState = ref.read(authNotifierProvider).valueOrNull;
    final name = authState?.fullName ?? 'Guest';

    return Drawer(
      child: Column(
        children: [
          UserAccountsDrawerHeader(
            decoration: const BoxDecoration(color: Color(0xFF2E3138)),
            accountName: Text(name,
                style: const TextStyle(fontWeight: FontWeight.w700)),
            accountEmail: Text(isGuest ? 'Guest user' : 'Buyer'),
            currentAccountPicture: CircleAvatar(
              backgroundColor: const Color(0xFF8DC63F),
              child: Text(
                name.isNotEmpty ? name[0].toUpperCase() : 'G',
                style: const TextStyle(
                    color: Colors.white,
                    fontSize: 22,
                    fontWeight: FontWeight.w700),
              ),
            ),
          ),
          _DrawerItem(
            icon: Icons.home_rounded,
            label: 'Home',
            onTap: () {
              setState(() => _index = 0);
              Navigator.of(context).pop();
            },
          ),
          _DrawerItem(
            icon: Icons.search_rounded,
            label: 'Browse Products',
            onTap: () {
              setState(() => _index = 0);
              Navigator.of(context).pop();
            },
          ),
          _DrawerItem(
            icon: Icons.favorite_border_rounded,
            label: 'My Favorites',
            onTap: () {
              setState(() => _index = 1);
              Navigator.of(context).pop();
            },
          ),
          _DrawerItem(
            icon: Icons.compare_arrows_rounded,
            label: 'My Enquiries',
            onTap: () {
              setState(() => _index = 2);
              Navigator.of(context).pop();
            },
          ),
          _DrawerItem(
            icon: Icons.person_outline_rounded,
            label: 'Profile',
            onTap: () {
              setState(() => _index = 3);
              Navigator.of(context).pop();
            },
          ),
          _DrawerItem(
            icon: Icons.notifications_none_rounded,
            label: 'Notifications',
            onTap: () => Navigator.of(context).pop(),
          ),
          const Divider(),
          if (isGuest)
            _DrawerItem(
              icon: Icons.login_rounded,
              label: 'Sign In / Register',
              onTap: () {
                Navigator.of(context).pop();
                _logout(); // clears guest state → shows login screen
              },
            )
          else
            _DrawerItem(
              icon: Icons.logout_rounded,
              label: 'Logout',
              iconColor: Colors.red.shade400,
              labelColor: Colors.red.shade400,
              onTap: () {
                Navigator.of(context).pop();
                _logout();
              },
            ),
          const Spacer(),
          const Padding(
            padding: EdgeInsets.all(16),
            child: Text('Farm Marketplace v1.0',
                style: TextStyle(color: Colors.black38, fontSize: 12)),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authNotifierProvider).valueOrNull;
    final isGuest = authState?.isGuest ?? false;
    final bagCount = ref.watch(bagProvider).fold<int>(0, (sum, e) => sum + e.quantity);

    return Scaffold(
      drawer: _buildDrawer(context, isGuest),
      appBar: AppBar(
        leading: Builder(
          builder: (ctx) => IconButton(
            tooltip: 'Menu',
            onPressed: () => Scaffold.of(ctx).openDrawer(),
            icon: const Icon(Icons.menu_rounded),
          ),
        ),
        title: Text(isGuest ? 'Browse Products' : 'Farm Marketplace'),
        actions: [
          IconButton(
            tooltip: 'Notifications',
            onPressed: () {},
            icon: const Icon(Icons.notifications_none_rounded),
          ),
        ],
      ),
      floatingActionButtonLocation: FloatingActionButtonLocation.centerDocked,
      floatingActionButton: Stack(
        clipBehavior: Clip.none,
        children: [
          Container(
            height: 64,
            width: 64,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: const Color(0xFF8DC63F),
              boxShadow: [
                BoxShadow(
                  color: const Color(0xFF8DC63F).withValues(alpha: 0.35),
                  blurRadius: 14,
                  offset: const Offset(0, 6),
                ),
              ],
            ),
            child: IconButton(
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => const BuyerBagScreen(),
                  ),
                );
              },
              icon: const Icon(Icons.shopping_bag_outlined, color: Colors.white),
            ),
          ),
          if (bagCount > 0)
            Positioned(
              right: -2,
              top: -2,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 3),
                decoration: BoxDecoration(
                  color: Colors.red.shade600,
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  bagCount > 99 ? '99+' : '$bagCount',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 10,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
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
      bottomNavigationBar: BottomAppBar(
        color: const Color(0xFF2E3138),
        shape: const CircularNotchedRectangle(),
        notchMargin: 8,
        child: SizedBox(
          height: 66,
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceAround,
            children: [
              _NavIcon(
                icon: Icons.home_rounded,
                active: _index == 0,
                onTap: () => setState(() => _index = 0),
              ),
              _NavIcon(
                icon: Icons.favorite_border_rounded,
                active: _index == 1,
                onTap: () => setState(() => _index = 1),
              ),
              const SizedBox(width: 40),
              _NavIcon(
                icon: Icons.compare_arrows_rounded,
                active: _index == 2,
                onTap: () => setState(() => _index = 2),
              ),
              _NavIcon(
                icon: Icons.person_outline_rounded,
                active: _index == 3,
                onTap: () => setState(() => _index = 3),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _NavIcon extends StatelessWidget {
  const _NavIcon({required this.icon, required this.active, required this.onTap});

  final IconData icon;
  final bool active;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return IconButton(
      onPressed: onTap,
      icon: Icon(
        icon,
        size: 24,
        color: active ? const Color(0xFF8DC63F) : Colors.white70,
      ),
    );
  }
}

class _DrawerItem extends StatelessWidget {
  const _DrawerItem({
    required this.icon,
    required this.label,
    required this.onTap,
    this.iconColor,
    this.labelColor,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;
  final Color? iconColor;
  final Color? labelColor;

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: Icon(icon, color: iconColor ?? const Color(0xFF2E3138)),
      title: Text(label,
          style: TextStyle(
              color: labelColor ?? Colors.black87,
              fontWeight: FontWeight.w500)),
      onTap: onTap,
      dense: true,
    );
  }
}
