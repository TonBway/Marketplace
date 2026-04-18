import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/app_error_handler.dart';
import '../../../../core/providers.dart';
import '../../../bag/data/bag_notifier.dart';
import '../../../checkout/presentation/screens/buyer_checkout_screen.dart';

class BuyerProductDetailScreen extends ConsumerStatefulWidget {
  const BuyerProductDetailScreen({super.key, required this.listing});

  final Map<String, dynamic> listing;

  @override
  ConsumerState<BuyerProductDetailScreen> createState() =>
      _BuyerProductDetailScreenState();
}

class _BuyerProductDetailScreenState
    extends ConsumerState<BuyerProductDetailScreen> {
  static const _apiBaseUrl =
      String.fromEnvironment('API_BASE_URL', defaultValue: 'http://192.168.88.20:5000');

  int _qty = 1;
  int _imageIndex = 0;
  List<String> _imageUrls = [];
  bool _imagesLoaded = false;
  String? _sellerName;
  String? _description;
  String? _sellerPhone;
  String? _sellerEmail;
  bool _isFavorite = false;
  bool _favoriteBusy = false;

  @override
  void initState() {
    super.initState();
    _loadDetail();
  }

  Future<void> _loadDetail() async {
    final listingId = widget.listing['listingId']?.toString() ??
        widget.listing['listing_id']?.toString();
    if (listingId == null) return;
    try {
      final resp = await ref
          .read(apiClientProvider)
          .dio
          .get('/api/listings/$listingId');
      final data = (resp.data as Map).cast<String, dynamic>();
      final images = (data['images'] as List?) ?? [];
      final urls = images
          .map((img) => _resolveImageUrl(img['imageUrl']))
          .whereType<String>()
          .toList();

      bool favorite = false;
      final auth = ref.read(authNotifierProvider).valueOrNull;
      final isGuest = auth?.isGuest ?? true;
      if (!isGuest) {
        final favResp = await ref.read(apiClientProvider).dio.get('/api/listings/favorites');
        final list = (favResp.data as List)
            .map((e) => (e as Map).cast<String, dynamic>())
            .toList();
        favorite = list.any((e) =>
            (e['listingId'] ?? e['listing_id'])?.toString() == listingId);
      }

      if (mounted) {
        setState(() {
          _imageUrls = urls;
          _sellerName = data['sellerName']?.toString();
          _description = data['description']?.toString();
          _sellerPhone = data['sellerPhone']?.toString();
          _sellerEmail = data['sellerEmail']?.toString();
          _isFavorite = favorite;
          _imagesLoaded = true;
        });
      }
    } catch (_) {
      // Fall back to primaryImageUrl already shown
      final primary = _resolveImageUrl(widget.listing['primaryImageUrl']);
      if (mounted) {
        setState(() {
          _imageUrls = primary != null ? [primary] : [];
          _imagesLoaded = true;
        });
      }
    }
  }

  Future<void> _toggleFavorite() async {
    final auth = ref.read(authNotifierProvider).valueOrNull;
    final isGuest = auth?.isGuest ?? true;
    if (isGuest) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please sign in to save favorites.')),
      );
      return;
    }

    final listingId = (widget.listing['listingId'] ?? widget.listing['listing_id'])?.toString();
    if (listingId == null || listingId.isEmpty || _favoriteBusy) return;

    setState(() => _favoriteBusy = true);
    try {
      if (_isFavorite) {
        await ref.read(apiClientProvider).dio.delete('/api/listings/$listingId/favorite');
      } else {
        await ref.read(apiClientProvider).dio.post('/api/listings/$listingId/favorite');
      }
      if (mounted) {
        setState(() => _isFavorite = !_isFavorite);
      }
    } catch (e) {
      if (mounted) showErrorSnackBar(context, e);
    } finally {
      if (mounted) setState(() => _favoriteBusy = false);
    }
  }

  void _showFarmerContact(String name) {
    showModalBottomSheet<void>(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (_) {
        return Padding(
          padding: const EdgeInsets.fromLTRB(24, 20, 24, 32),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(
                child: Container(
                  width: 40, height: 4,
                  decoration: BoxDecoration(
                    color: Colors.grey.shade300,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              Row(
                children: [
                  const CircleAvatar(
                    radius: 24,
                    backgroundColor: Color(0xFF8DC63F),
                    child: Icon(Icons.person_rounded, color: Colors.white, size: 26),
                  ),
                  const SizedBox(width: 14),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(name,
                          style: const TextStyle(
                              fontWeight: FontWeight.w800, fontSize: 16)),
                      const Text('Verified Farmer',
                          style: TextStyle(
                              fontSize: 12, color: Color(0xFF8DC63F))),
                    ],
                  ),
                ],
              ),
              const SizedBox(height: 24),
              const Text('Contact Details',
                  style: TextStyle(fontWeight: FontWeight.w700, fontSize: 13)),
              const SizedBox(height: 12),
              if (_sellerPhone != null && _sellerPhone!.isNotEmpty)
                _ContactRow(
                  icon: Icons.phone_rounded,
                  label: 'Phone / WhatsApp',
                  value: _sellerPhone!,
                ),
              if (_sellerEmail != null && _sellerEmail!.isNotEmpty)
                _ContactRow(
                  icon: Icons.email_rounded,
                  label: 'Email',
                  value: _sellerEmail!,
                ),
              if ((_sellerPhone == null || _sellerPhone!.isEmpty) &&
                  (_sellerEmail == null || _sellerEmail!.isEmpty))
                const Text('No contact details available.',
                    style: TextStyle(color: Colors.black54)),
              const SizedBox(height: 16),
              const Divider(),
              const SizedBox(height: 8),
              Text(
                'You can also use the "Buy Now" button to send an enquiry directly through the app.',
                style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
              ),
            ],
          ),
        );
      },
    );
  }

  void _addToBag() {
    ref.read(bagProvider.notifier).addItem(widget.listing, quantity: _qty);
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Added $_qty item(s) to bag.')),
    );
  }

  String _imageHint(String title) {
    final t = title.toLowerCase();
    if (t.contains('lemon')) return '\ud83c\udf4b';
    if (t.contains('orange')) return '\ud83c\udf4a';
    if (t.contains('pomegranate')) return '\ud83c\udf4e';
    if (t.contains('berry') || t.contains('straw')) return '\ud83c\udf53';
    if (t.contains('potato')) return '\ud83e\udd54';
    return '\ud83e\udd6c';
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
    final title = (widget.listing['title'] ?? 'Product').toString();
    final price = widget.listing['price']?.toString() ?? '0';
    final unit = widget.listing['unitName']?.toString() ?? 'unit';
    final sellerName = (_sellerName ?? widget.listing['sellerName'] ?? 'Farmer').toString();
    final description = (_description ?? widget.listing['description'] ?? 'No description provided by seller.').toString();

    return Scaffold(
      backgroundColor: const Color(0xFFF5F6F8),
      appBar: AppBar(
        backgroundColor: const Color(0xFFF5F6F8),
        elevation: 0,
        leading: IconButton(
          onPressed: () => Navigator.of(context).pop(),
          icon: const Icon(Icons.arrow_back_rounded),
        ),
        actions: [
          IconButton(
            onPressed: _favoriteBusy ? null : _toggleFavorite,
            icon: Icon(
              _isFavorite ? Icons.favorite_rounded : Icons.favorite_border_rounded,
              color: _isFavorite ? Colors.red.shade400 : null,
            ),
          ),
        ],
      ),
      body: Column(
        children: [
          // ─── Image gallery ─────────────────────────────────────────────
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 6),
            child: Stack(
              alignment: Alignment.bottomCenter,
              children: [
                Container(
                  height: 240,
                  width: double.infinity,
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(20),
                    child: _buildImageSlider(title),
                  ),
                ),
                // Dot indicators
                if (_imageUrls.length > 1)
                  Positioned(
                    bottom: 10,
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: List.generate(_imageUrls.length, (i) {
                        return AnimatedContainer(
                          duration: const Duration(milliseconds: 250),
                          margin: const EdgeInsets.symmetric(horizontal: 3),
                          width: _imageIndex == i ? 18 : 7,
                          height: 7,
                          decoration: BoxDecoration(
                            color: _imageIndex == i
                                ? const Color(0xFF8DC63F)
                                : Colors.white70,
                            borderRadius: BorderRadius.circular(4),
                          ),
                        );
                      }),
                    ),
                  ),
              ],
            ),
          ),

          // ─── Thumbnail strip ──────────────────────────────────────────
          if (_imageUrls.length > 1)
            SizedBox(
              height: 64,
              child: ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                scrollDirection: Axis.horizontal,
                itemCount: _imageUrls.length,
                itemBuilder: (_, i) {
                  final selected = _imageIndex == i;
                  return GestureDetector(
                    onTap: () => setState(() => _imageIndex = i),
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 200),
                      margin: const EdgeInsets.only(right: 8, bottom: 4, top: 4),
                      width: 56,
                      decoration: BoxDecoration(
                        border: Border.all(
                          color: selected
                              ? const Color(0xFF8DC63F)
                              : Colors.transparent,
                          width: 2.5,
                        ),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: Image.network(
                          _imageUrls[i],
                          fit: BoxFit.cover,
                          errorBuilder: (_, __, ___) => const Icon(Icons.image_not_supported_rounded, size: 20),
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),

          Expanded(
            child: SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const SizedBox(height: 8),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    child: Row(
                      children: [
                        Expanded(
                          child: Text(
                            title,
                            style: Theme.of(context)
                                .textTheme
                                .headlineSmall
                                ?.copyWith(fontWeight: FontWeight.w800),
                          ),
                        ),
                        // ─── Qty control ──────────────────────────
                        Container(
                          decoration: BoxDecoration(
                            color: const Color(0xFF2E3138),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          padding: const EdgeInsets.symmetric(
                              horizontal: 4, vertical: 4),
                          child: Row(
                            children: [
                              _QtyBtn(
                                icon: Icons.remove,
                                enabled: _qty > 1,
                                onTap: () => setState(() => _qty--),
                              ),
                              Padding(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: 10),
                                child: Text(
                                  '$_qty',
                                  style: const TextStyle(
                                      color: Colors.white,
                                      fontWeight: FontWeight.w700),
                                ),
                              ),
                              _QtyBtn(
                                icon: Icons.add,
                                enabled: true,
                                onTap: () => setState(() => _qty++),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.fromLTRB(16, 6, 16, 12),
                    child: Text(
                      'SCR $price / $unit',
                      style: const TextStyle(
                        color: Color(0xFF8DC63F),
                        fontWeight: FontWeight.w700,
                        fontSize: 16,
                      ),
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    child: GestureDetector(
                      onTap: () => _showFarmerContact(sellerName),
                      child: Row(
                        children: [
                          const CircleAvatar(
                            radius: 18,
                            child: Icon(Icons.person_outline_rounded),
                          ),
                          const SizedBox(width: 10),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(sellerName,
                                  style: const TextStyle(
                                      fontWeight: FontWeight.w700,
                                      decoration: TextDecoration.underline)),
                              const Text('Farmer  \u2022  Tap to contact',
                                  style: TextStyle(fontSize: 12, color: Color(0xFF8DC63F))),
                            ],
                          ),
                          const Spacer(),
                          const Icon(Icons.chevron_right_rounded,
                              size: 18, color: Colors.black38),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 14),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    child: Text(
                      description,
                      style: const TextStyle(height: 1.35, color: Colors.black54),
                    ),
                  ),
                  const SizedBox(height: 14),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    child: Container(
                      padding: const EdgeInsets.all(14),
                      decoration: BoxDecoration(
                        color: const Color(0xFFEEF7E0),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: const Color(0xFFBFDE94)),
                      ),
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Icon(Icons.local_shipping_outlined,
                              color: Color(0xFF5A8F1E), size: 20),
                          const SizedBox(width: 10),
                          Expanded(
                            child: RichText(
                              text: const TextSpan(
                                style: TextStyle(
                                    fontSize: 12.5,
                                    height: 1.45,
                                    color: Color(0xFF3A5A0A)),
                                children: [
                                  TextSpan(
                                    text: 'How to arrange delivery:\n',
                                    style: TextStyle(fontWeight: FontWeight.w700),
                                  ),
                                  TextSpan(
                                    text:
                                        'Tap the farmer\u2019s name above to view contact details. '
                                        'You can call, WhatsApp, or email the farmer directly to '
                                        'discuss delivery, collection point, and timing. '
                                        'Alternatively, use \u201cBuy Now\u201d to send an in-app enquiry.',
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 80),
                ],
              ),
            ),
          ),

          // ─── CTA buttons ──────────────────────────────────────────────
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 10, 16, 18),
            child: Row(
              children: [
                OutlinedButton.icon(
                  onPressed: _addToBag,
                  icon: const Icon(Icons.shopping_bag_outlined),
                  label: const Text('Add to Bag'),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: FilledButton(
                    style: FilledButton.styleFrom(
                      backgroundColor: const Color(0xFF8DC63F),
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(14)),
                    ),
                    onPressed: () {
                      Navigator.of(context).push(
                        MaterialPageRoute<void>(
                          builder: (_) => BuyerCheckoutScreen(
                            listing: widget.listing,
                            initialQty: _qty,
                          ),
                        ),
                      );
                    },
                    child: Text('Buy Now  ($_qty $unit)'),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildImageSlider(String title) {
    if (!_imagesLoaded) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_imageUrls.isEmpty) {
      return Center(
          child:
              Text(_imageHint(title), style: const TextStyle(fontSize: 110)));
    }
    return PageView.builder(
      itemCount: _imageUrls.length,
      onPageChanged: (i) => setState(() => _imageIndex = i),
      itemBuilder: (_, i) => Image.network(
        _imageUrls[i],
        fit: BoxFit.cover,
        errorBuilder: (_, __, ___) => Center(
          child: Text(_imageHint(title), style: const TextStyle(fontSize: 110)),
        ),
      ),
    );
  }
}

class _ContactRow extends StatelessWidget {
  const _ContactRow({required this.icon, required this.label, required this.value});

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        children: [
          Icon(icon, size: 20, color: const Color(0xFF8DC63F)),
          const SizedBox(width: 12),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(label,
                  style: const TextStyle(
                      fontSize: 11, color: Colors.black45)),
              Text(value,
                  style: const TextStyle(
                      fontWeight: FontWeight.w600, fontSize: 14)),
            ],
          ),
        ],
      ),
    );
  }
}

class _QtyBtn extends StatelessWidget {
  const _QtyBtn({
    required this.icon,
    required this.enabled,
    required this.onTap,
  });

  final IconData icon;
  final bool enabled;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: enabled ? onTap : null,
      child: Icon(icon,
          color: enabled ? Colors.white : Colors.white38, size: 18),
    );
  }
}
