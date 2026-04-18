import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';

class SellerSubscriptionsScreen extends ConsumerStatefulWidget {
  const SellerSubscriptionsScreen({super.key});

  @override
  ConsumerState<SellerSubscriptionsScreen> createState() =>
      _SellerSubscriptionsScreenState();
}

class _SellerSubscriptionsScreenState
    extends ConsumerState<SellerSubscriptionsScreen> {
  bool _loading = true;
  Map<String, dynamic>? _active;
  List<Map<String, dynamic>> _plans = const [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final plansResp =
          await ref.read(apiClientProvider).dio.get('/api/subscriptions/plans');
      final plans = (plansResp.data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();

      Map<String, dynamic>? active;
      try {
        final activeResp = await ref
            .read(apiClientProvider)
            .dio
            .get('/api/subscriptions/active');
        active = (activeResp.data as Map).cast<String, dynamic>();
      } catch (_) {
        // No active subscription is expected for new sellers.
      }

      setState(() {
        _plans = plans;
        _active = active;
      });
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to load subscriptions: $error')),
      );
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _subscribe(int planId) async {
    try {
      await ref.read(apiClientProvider).dio.post(
        '/api/subscriptions',
        data: {'planId': planId, 'paymentMethodCode': 'MOBILE_MONEY'},
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Subscription activated successfully.')),
      );
      await _load();
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Subscription failed: $error')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(12),
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(12),
              child: _active == null
                  ? const Text('No active subscription. Choose a plan below.')
                  : Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Active Subscription',
                            style: TextStyle(fontWeight: FontWeight.bold)),
                        const SizedBox(height: 8),
                        Text('Plan: ${_active!['planName']}'),
                        Text('Status: ${_active!['statusCode']}'),
                        Text('Ends: ${_active!['endDateUtc']}'),
                      ],
                    ),
            ),
          ),
          const SizedBox(height: 8),
          const Text('Available Plans',
              style: TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 6),
          ..._plans.map(
            (plan) => Card(
              child: ListTile(
                title: Text(plan['planName']?.toString() ?? 'Plan'),
                subtitle: Text(
                    'Price: ${plan['priceAmount']} • ${plan['durationDays']} days'),
                trailing: FilledButton(
                  onPressed: () => _subscribe((plan['planId'] as num).toInt()),
                  child: const Text('Subscribe'),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
