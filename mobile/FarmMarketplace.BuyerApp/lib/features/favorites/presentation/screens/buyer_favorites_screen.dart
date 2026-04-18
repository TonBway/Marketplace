import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/app_error_handler.dart';
import '../../../../core/providers.dart';
import '../../../search/presentation/screens/buyer_product_detail_screen.dart';

class BuyerFavoritesScreen extends ConsumerStatefulWidget {
  const BuyerFavoritesScreen({super.key});

  @override
  ConsumerState<BuyerFavoritesScreen> createState() => _BuyerFavoritesScreenState();
}

class _BuyerFavoritesScreenState extends ConsumerState<BuyerFavoritesScreen> {
  final List<Map<String, dynamic>> _favorites = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadFavorites();
  }

  Future<void> _loadFavorites() async {
    setState(() => _isLoading = true);
    try {
      final auth = ref.read(authNotifierProvider).valueOrNull;
      final isGuest = auth?.isGuest ?? true;
      if (isGuest) {
        setState(() {
          _favorites.clear();
        });
        return;
      }

      final response = await ref.read(apiClientProvider).dio.get('/api/listings/favorites');
      final rows = (response.data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();
      setState(() {
        _favorites
          ..clear()
          ..addAll(rows);
      });
    } catch (e) {
      if (mounted) showErrorSnackBar(context, e);
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  Future<void> _removeFavorite(Map<String, dynamic> favorite) async {
    final id = (favorite['listingId'] ?? favorite['listing_id'])?.toString() ?? '';
    if (id.isEmpty) return;
    try {
      await ref.read(apiClientProvider).dio.delete('/api/listings/$id/favorite');
    } catch (e) {
      if (mounted) showErrorSnackBar(context, e);
    }
    await _loadFavorites();
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_favorites.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                height: 84,
                width: 84,
                decoration: BoxDecoration(
                  color: const Color(0xFFEFF5E4),
                  borderRadius: BorderRadius.circular(24),
                ),
                child: const Icon(Icons.favorite_border_rounded, size: 42, color: Color(0xFF8DC63F)),
              ),
              const SizedBox(height: 16),
              Text(
                'No favorites yet',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
              ),
              const SizedBox(height: 8),
              Text(
                'Tap the heart icon on any product to save it here.',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.black54),
              ),
              const SizedBox(height: 16),
              FilledButton(
                style: FilledButton.styleFrom(
                  backgroundColor: const Color(0xFF8DC63F),
                  foregroundColor: Colors.white,
                ),
                onPressed: () {},
                child: const Text('Browse Products'),
              ),
            ],
          ),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _loadFavorites,
      child: ListView.separated(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 90),
        itemCount: _favorites.length,
        separatorBuilder: (_, __) => const SizedBox(height: 10),
        itemBuilder: (context, index) {
          final favorite = _favorites[index];
          return Card(
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
            child: ListTile(
              contentPadding: const EdgeInsets.all(10),
              leading: Container(
                width: 52,
                height: 52,
                decoration: BoxDecoration(
                  color: const Color(0xFFF3F5F8),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: const Center(child: Text('🍎', style: TextStyle(fontSize: 24))),
              ),
              title: Text(favorite['title'] ?? 'Product'),
              subtitle: Text('SCR ${favorite['price'] ?? 0} / ${favorite['unitName'] ?? 'unit'}'),
              trailing: IconButton(
                icon: const Icon(Icons.favorite, color: Color(0xFF8DC63F)),
                onPressed: () => _removeFavorite(favorite),
              ),
              onTap: () {
                Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => BuyerProductDetailScreen(listing: favorite),
                  ),
                );
              },
            ),
          );
        },
      ),
    );
  }
}
