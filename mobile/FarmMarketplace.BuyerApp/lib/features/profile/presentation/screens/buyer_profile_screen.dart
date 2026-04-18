import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';

class BuyerProfileScreen extends ConsumerStatefulWidget {
  const BuyerProfileScreen({super.key});

  @override
  ConsumerState<BuyerProfileScreen> createState() => _BuyerProfileScreenState();
}

class _BuyerProfileScreenState extends ConsumerState<BuyerProfileScreen> {
  Map<String, dynamic>? _profile;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadProfile();
  }

  Future<void> _loadProfile() async {
    setState(() => _isLoading = true);
    try {
      // TODO: Implement API call to load buyer profile
      await Future.delayed(const Duration(seconds: 1));
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  Future<void> _logout() async {
    await ref.read(authNotifierProvider.notifier).logout();
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authNotifierProvider);

    if (_isLoading && !authState.hasValue) {
      return const Center(child: CircularProgressIndicator());
    }

    final auth = authState.valueOrNull;
    final isGuest = auth?.isGuest ?? false;
    final profile = _profile;

    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 90),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(18),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(18),
            ),
            child: Column(
              children: [
                const CircleAvatar(
                  radius: 42,
                  backgroundColor: Color(0xFFEFF5E4),
                  child: Icon(Icons.person_rounded, size: 44, color: Color(0xFF8DC63F)),
                ),
                const SizedBox(height: 12),
                Text(
                  profile?['fullName'] ?? auth?.fullName ?? 'Buyer',
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 2),
                Text(
                  profile?['email'] ?? 'email@example.com',
                  style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.black54),
                ),
                if (isGuest)
                  Padding(
                    padding: const EdgeInsets.only(top: 10),
                    child: Chip(
                      label: const Text('Guest Mode'),
                      backgroundColor: const Color(0xFFFFF2D8),
                      side: BorderSide.none,
                      labelStyle: TextStyle(color: Colors.orange.shade800),
                    ),
                  ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          if (!isGuest)
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(18),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Account Information',
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700),
                  ),
                  const SizedBox(height: 12),
                  _ProfileRow(label: 'Phone', value: '${profile?['phone'] ?? 'N/A'}'),
                  _ProfileRow(label: 'Email', value: '${profile?['email'] ?? 'N/A'}'),
                  _ProfileRow(label: 'Credits', value: '${profile?['availableCredits'] ?? 0}'),
                ],
              ),
            )
          else
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: const Color(0xFFEAF2FF),
                borderRadius: BorderRadius.circular(18),
              ),
              child: const Text(
                'You are browsing as a guest. Sign in to access favorites, enquiries, and personalized features.',
                style: TextStyle(height: 1.35),
              ),
            ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: FilledButton.tonal(
              onPressed: _logout,
              style: FilledButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 14)),
              child: const Text('Logout'),
            ),
          ),
        ],
      ),
    );
  }
}

class _ProfileRow extends StatelessWidget {
  const _ProfileRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 7),
      child: Row(
        children: [
          SizedBox(
            width: 82,
            child: Text(label, style: const TextStyle(color: Colors.black54)),
          ),
          const Text(': '),
          Expanded(child: Text(value, style: const TextStyle(fontWeight: FontWeight.w600))),
        ],
      ),
    );
  }
}
