import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';
import '../../data/bag_notifier.dart';

class BuyerBagScreen extends ConsumerStatefulWidget {
  const BuyerBagScreen({super.key});

  @override
  ConsumerState<BuyerBagScreen> createState() => _BuyerBagScreenState();
}

class _BuyerBagScreenState extends ConsumerState<BuyerBagScreen> {
  bool _placing = false;

  Future<void> _placeOrder() async {
    final auth = ref.read(authNotifierProvider).valueOrNull;
    final isGuest = auth?.isGuest ?? true;
    if (isGuest) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please sign in to place an order.')),
      );
      return;
    }

    final items = ref.read(bagProvider);
    if (items.isEmpty) return;

    setState(() => _placing = true);
    try {
      final dio = ref.read(apiClientProvider).dio;
      for (final item in items) {
        final listingId = item.listingId;
        if (listingId.isEmpty) continue;
        await dio.post(
          '/api/enquiries',
          data: {
            'listingId': listingId,
            'message': 'Order request for quantity ${item.quantity}.',
            'preferredContactMode': 'IN_APP',
          },
        );
      }

      ref.read(bagProvider.notifier).clear();
      if (!mounted) return;
      await showDialog<void>(
        context: context,
        builder: (_) => AlertDialog(
          title: const Text('Order Request Sent'),
          content: const Text('Your enquiry has been sent to the seller(s).'),
          actions: [
            FilledButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('OK'),
            ),
          ],
        ),
      );
      if (mounted) Navigator.of(context).pop();
    } on DioException catch (e) {
      if (!mounted) return;
      final message = e.response?.data is Map
          ? (e.response?.data['message']?.toString() ?? 'Failed to place order')
          : 'Failed to place order';
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(message)),
      );
    } finally {
      if (mounted) setState(() => _placing = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final items = ref.watch(bagProvider);

    return Scaffold(
      backgroundColor: const Color(0xFFF5F6F8),
      appBar: AppBar(
        backgroundColor: const Color(0xFFF5F6F8),
        title: const Text('My Bag'),
      ),
      body: items.isEmpty
          ? const Center(child: Text('Your bag is empty.'))
          : Column(
              children: [
                Expanded(
                  child: ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: items.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 10),
                    itemBuilder: (_, index) {
                      final item = items[index];
                      final listing = item.listing;
                      final title = (listing['title'] ?? 'Product').toString();
                      final unit = (listing['unitName'] ?? 'unit').toString();
                      final price =
                          double.tryParse((listing['price'] ?? 0).toString()) ?? 0;

                      return Card(
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16),
                        ),
                        child: ListTile(
                          title: Text(title),
                          subtitle: Text('SCR ${price.toStringAsFixed(0)} / $unit'),
                          trailing: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              IconButton(
                                onPressed: item.quantity > 1
                                    ? () => ref
                                        .read(bagProvider.notifier)
                                        .updateQuantity(item.listingId, item.quantity - 1)
                                    : null,
                                icon: const Icon(Icons.remove_circle_outline),
                              ),
                              Text('${item.quantity}'),
                              IconButton(
                                onPressed: () => ref
                                    .read(bagProvider.notifier)
                                    .updateQuantity(item.listingId, item.quantity + 1),
                                icon: const Icon(Icons.add_circle_outline),
                              ),
                              IconButton(
                                onPressed: () =>
                                    ref.read(bagProvider.notifier).removeItem(item.listingId),
                                icon: const Icon(Icons.delete_outline),
                              ),
                            ],
                          ),
                        ),
                      );
                    },
                  ),
                ),
                Container(
                  color: Colors.white,
                  padding: const EdgeInsets.fromLTRB(16, 12, 16, 20),
                  child: SizedBox(
                    width: double.infinity,
                    child: FilledButton(
                      onPressed: _placing ? null : _placeOrder,
                      style: FilledButton.styleFrom(
                        backgroundColor: const Color(0xFF8DC63F),
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 15),
                      ),
                      child: _placing
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Place Order'),
                    ),
                  ),
                ),
              ],
            ),
    );
  }
}
