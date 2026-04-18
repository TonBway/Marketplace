import 'package:flutter/material.dart';

class BuyerFavoritesScreen extends StatefulWidget {
  const BuyerFavoritesScreen({super.key});

  @override
  State<BuyerFavoritesScreen> createState() => _BuyerFavoritesScreenState();
}

class _BuyerFavoritesScreenState extends State<BuyerFavoritesScreen> {
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
      // TODO: Implement API call to load favorites
      await Future.delayed(const Duration(seconds: 1));
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
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
              subtitle: Text('PKR ${favorite['price'] ?? 0} / ${favorite['unit'] ?? 'unit'}'),
              trailing: IconButton(
                icon: const Icon(Icons.favorite, color: Color(0xFF8DC63F)),
                onPressed: () {},
              ),
            ),
          );
        },
      ),
    );
  }
}
