import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';
import 'buyer_product_detail_screen.dart';

class BuyerSearchScreen extends ConsumerStatefulWidget {
  const BuyerSearchScreen({super.key});

  @override
  ConsumerState<BuyerSearchScreen> createState() => _BuyerSearchScreenState();
}

class _BuyerSearchScreenState extends ConsumerState<BuyerSearchScreen> {
  static const _apiBaseUrl =
      String.fromEnvironment('API_BASE_URL', defaultValue: 'http://192.168.88.20:5000');

  final _searchCtrl = TextEditingController();
  final _categories = const ['FRUITS', 'VEGETABLES', 'TUBERS', 'GRAINS'];
  int _categoryIndex = 0;
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

  String _imageHint(String title) {
    final t = title.toLowerCase();
    if (t.contains('lemon')) return '🍋';
    if (t.contains('orange')) return '🍊';
    if (t.contains('pomegranate')) return '🍎';
    if (t.contains('berry') || t.contains('straw')) return '🍓';
    if (t.contains('potato')) return '🥔';
    return '🥬';
  }

  String? _resolveImageUrl(dynamic rawUrl) {
    final value = rawUrl?.toString().trim() ?? '';
    if (value.isEmpty) return null;
    if (value.startsWith('http://') || value.startsWith('https://')) {
      return value;
    }
    final base = _apiBaseUrl.endsWith('/')
        ? _apiBaseUrl.substring(0, _apiBaseUrl.length - 1)
        : _apiBaseUrl;
    final path = value.startsWith('/') ? value : '/$value';
    return '$base$path';
  }

  @override
  Widget build(BuildContext context) {
    return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
            child: TextField(
              controller: _searchCtrl,
              onSubmitted: (_) => _loadListings(),
              decoration: InputDecoration(
                hintText: 'Search here',
                prefixIcon: const Icon(Icons.search_rounded),
                suffixIcon: _searchCtrl.text.isEmpty
                    ? null
                    : IconButton(
                        icon: const Icon(Icons.clear),
                        onPressed: () {
                          _searchCtrl.clear();
                          _loadListings();
                        },
                      ),
              ),
            ),
          ),
          SizedBox(
            height: 40,
            child: ListView.separated(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              scrollDirection: Axis.horizontal,
              itemBuilder: (_, i) {
                final selected = i == _categoryIndex;
                return ChoiceChip(
                  selected: selected,
                  onSelected: (_) => setState(() => _categoryIndex = i),
                  label: Text(_categories[i]),
                  labelStyle: TextStyle(
                    color: selected ? Colors.white : const Color(0xFF2E3138),
                    fontWeight: FontWeight.w700,
                    fontSize: 11,
                  ),
                  backgroundColor: const Color(0xFFE5E8EC),
                  selectedColor: const Color(0xFF8DC63F),
                  side: BorderSide.none,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                );
              },
              separatorBuilder: (_, __) => const SizedBox(width: 8),
              itemCount: _categories.length,
            ),
          ),
          const SizedBox(height: 10),
          Expanded(
            child: _isLoading && !_loadedInitial
                ? const Center(child: CircularProgressIndicator())
                : _listings.isEmpty
                    ? Center(
                        child: Text(
                          'No products found',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                      )
                    : RefreshIndicator(
                        onRefresh: _loadListings,
                        child: GridView.builder(
                          padding: const EdgeInsets.fromLTRB(16, 8, 16, 90),
                          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                            crossAxisCount: 2,
                            crossAxisSpacing: 12,
                            mainAxisSpacing: 12,
                            childAspectRatio: 0.74,
                          ),
                          itemCount: _listings.length,
                          itemBuilder: (context, index) {
                            final listing = _listings[index];
                            final title = (listing['title'] ?? 'Product').toString();
                            final price = listing['price']?.toString() ?? '0';
                            final unit = listing['unitName']?.toString() ?? 'unit';
                            final imageUrl = _resolveImageUrl(listing['primaryImageUrl']);

                            return InkWell(
                              borderRadius: BorderRadius.circular(16),
                              onTap: () {
                                Navigator.of(context).push(
                                  MaterialPageRoute(
                                    builder: (_) => BuyerProductDetailScreen(listing: listing),
                                  ),
                                );
                              },
                              child: Card(
                                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                                child: Padding(
                                  padding: const EdgeInsets.all(10),
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Align(
                                        alignment: Alignment.topRight,
                                        child: IconButton(
                                          visualDensity: VisualDensity.compact,
                                          onPressed: () {},
                                          icon: const Icon(Icons.favorite_border_rounded),
                                        ),
                                      ),
                                      Expanded(
                                        child: Container(
                                          width: double.infinity,
                                          decoration: BoxDecoration(
                                            color: const Color(0xFFF3F5F8),
                                            borderRadius: BorderRadius.circular(12),
                                          ),
                                          child: imageUrl == null
                                              ? Center(
                                                  child: Text(
                                                    _imageHint(title),
                                                    style: const TextStyle(fontSize: 48),
                                                  ),
                                                )
                                              : ClipRRect(
                                                  borderRadius: BorderRadius.circular(12),
                                                  child: Image.network(
                                                    imageUrl,
                                                    fit: BoxFit.cover,
                                                    errorBuilder: (_, __, ___) => Center(
                                                      child: Text(
                                                        _imageHint(title),
                                                        style: const TextStyle(fontSize: 48),
                                                      ),
                                                    ),
                                                  ),
                                                ),
                                        ),
                                      ),
                                      const SizedBox(height: 10),
                                      Text(
                                        title,
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                        style: const TextStyle(fontWeight: FontWeight.w700),
                                      ),
                                      const SizedBox(height: 4),
                                      Row(
                                        children: [
                                          Text(
                                            'SCR $price',
                                            style: const TextStyle(
                                              color: Color(0xFF8DC63F),
                                              fontWeight: FontWeight.w700,
                                            ),
                                          ),
                                          const Spacer(),
                                          _QtyControl(unit: unit),
                                        ],
                                      ),
                                    ],
                                  ),
                                ),
                              ),
                            );
                          },
                        ),
                      ),
          ),
        ],
      );
  }
}

class _QtyControl extends StatelessWidget {
  const _QtyControl({required this.unit});

  final String unit;

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: const Color(0xFFEFF5E4),
        borderRadius: BorderRadius.circular(8),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      child: Row(
        children: [
          const Icon(Icons.remove, size: 14),
          const SizedBox(width: 4),
          Text('1', style: Theme.of(context).textTheme.labelSmall),
          const SizedBox(width: 4),
          const Icon(Icons.add, size: 14),
        ],
      ),
    );
  }
}
