import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';

class BuyerSearchScreen extends ConsumerStatefulWidget {
  const BuyerSearchScreen({super.key});

  @override
  ConsumerState<BuyerSearchScreen> createState() => _BuyerSearchScreenState();
}

class _BuyerSearchScreenState extends ConsumerState<BuyerSearchScreen> {
  final _searchCtrl = TextEditingController();
  List<Map<String, dynamic>> _listings = [];
  bool _isLoading = false;
  bool _loadedInitial = false;

  @override
  void initState() {
    super.initState();
    _loadListings();
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadListings() async {
    setState(() => _isLoading = true);
    try {
      final response = await ref
          .read(apiClientProvider)
          .dio
          .get('/api/listings', queryParameters: {
        if (_searchCtrl.text.isNotEmpty) 'search': _searchCtrl.text,
      });

      if (response.statusCode == 200) {
        final data = response.data;
        setState(() {
          if (data is List) {
            _listings = List<Map<String, dynamic>>.from(
              data.map((item) => Map<String, dynamic>.from(item as Map)),
            );
          } else if (data is Map && data.containsKey('listings')) {
            _listings = List<Map<String, dynamic>>.from(
              (data['listings'] as List).map(
                (item) => Map<String, dynamic>.from(item as Map),
              ),
            );
          }
        });
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Failed to load listings: $e'),
            backgroundColor: Colors.red.shade700,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
          _loadedInitial = true;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: SearchBar(
              controller: _searchCtrl,
              hintText: 'Search products...',
              onSubmitted: (_) => _loadListings(),
              leading: const Icon(Icons.search),
              trailing: [
                if (_searchCtrl.text.isNotEmpty)
                  IconButton(
                    icon: const Icon(Icons.clear),
                    onPressed: () {
                      _searchCtrl.clear();
                      _loadListings();
                    },
                  ),
              ],
            ),
          ),
          if (_isLoading && !_loadedInitial)
            const Expanded(
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_listings.isEmpty && _loadedInitial)
            Expanded(
              child: Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.inventory_2_outlined,
                      size: 64,
                      color: Colors.grey.shade400,
                    ),
                    const SizedBox(height: 16),
                    const Text('No listings found'),
                    const SizedBox(height: 8),
                    Text(
                      _searchCtrl.text.isEmpty
                          ? 'Check back later for new products'
                          : 'Try a different search term',
                      style: Theme.of(context).textTheme.bodySmall,
                    ),
                  ],
                ),
              ),
            )
          else if (_listings.isEmpty && !_loadedInitial)
            const Expanded(
              child: Center(child: CircularProgressIndicator()),
            )
          else
            Expanded(
              child: RefreshIndicator(
                onRefresh: _loadListings,
                child: ListView.builder(
                  itemCount: _listings.length,
                  itemBuilder: (context, index) {
                    final listing = _listings[index];
                    return Card(
                      margin: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 6,
                      ),
                      child: ListTile(
                        leading: Container(
                          width: 56,
                          height: 56,
                          decoration: BoxDecoration(
                            color: Colors.grey.shade200,
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: const Icon(
                            Icons.image_not_supported_outlined,
                            color: Colors.grey,
                          ),
                        ),
                        title: Text(
                          listing['title'] ?? 'Product',
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                        subtitle: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const SizedBox(height: 4),
                            Text(
                              'PKR ${listing['price']?.toString() ?? listing['pricePerUnit']?.toString() ?? '0'} per ${listing['unitName'] ?? listing['unitOfMeasure'] ?? 'unit'}',
                              style: const TextStyle(
                                fontWeight: FontWeight.w600,
                                color: Colors.green,
                              ),
                            ),
                            if (listing['sellerName'] != null)
                              Text(
                                'by ${listing['sellerName']}',
                                style: Theme.of(context).textTheme.bodySmall,
                              ),
                          ],
                        ),
                        trailing: IconButton(
                          icon: const Icon(Icons.favorite_border),
                          onPressed: () {
                            // TODO: Add to favorites
                          },
                        ),
                        onTap: () {
                          // TODO: Navigate to listing details
                        },
                      ),
                    );
                  },
                ),
              ),
            ),
        ],
      ),
    );
  }
}
