import 'package:flutter/material.dart';

import '../../../checkout/presentation/screens/buyer_checkout_screen.dart';

class BuyerProductDetailScreen extends StatefulWidget {
  const BuyerProductDetailScreen({super.key, required this.listing});

  final Map<String, dynamic> listing;

  @override
  State<BuyerProductDetailScreen> createState() =>
      _BuyerProductDetailScreenState();
}

class _BuyerProductDetailScreenState
    extends State<BuyerProductDetailScreen> {
  static const _apiBaseUrl =
      String.fromEnvironment('API_BASE_URL', defaultValue: 'http://192.168.88.20:5000');

  int _qty = 1;

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
    final sellerName =
        (widget.listing['sellerName'] ?? 'Local Farmer').toString();
    final imageUrl = _resolveImageUrl(widget.listing['primaryImageUrl']);

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
            onPressed: () {},
            icon: const Icon(Icons.favorite_border_rounded),
          ),
        ],
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 10),
            child: Container(
              height: 240,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(20),
              ),
              child: imageUrl == null
                  ? Center(
                      child: Text(
                        _imageHint(title),
                        style: const TextStyle(fontSize: 110),
                      ),
                    )
                  : ClipRRect(
                      borderRadius: BorderRadius.circular(20),
                      child: Image.network(
                        imageUrl,
                        width: double.infinity,
                        fit: BoxFit.cover,
                        errorBuilder: (_, __, ___) => Center(
                          child: Text(
                            _imageHint(title),
                            style: const TextStyle(fontSize: 110),
                          ),
                        ),
                      ),
                    ),
            ),
          ),
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
                        padding:
                            const EdgeInsets.symmetric(horizontal: 10),
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
            child: Row(
              children: [
                Text(
                  'PKR $price / $unit',
                  style: const TextStyle(
                    color: Color(0xFF8DC63F),
                    fontWeight: FontWeight.w700,
                    fontSize: 16,
                  ),
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
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
                        style:
                            const TextStyle(fontWeight: FontWeight.w700)),
                    const Text('Farmer  \u2022  4.9 \u2605',
                        style: TextStyle(fontSize: 12)),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 14),
          const Padding(
            padding: EdgeInsets.symmetric(horizontal: 16),
            child: Text(
              'Fresh farm produce with quality assurance. Contact seller for pickup, delivery options, and bulk pricing.',
              style: TextStyle(height: 1.35, color: Colors.black54),
            ),
          ),
          const Spacer(),
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 10, 16, 18),
            child: SizedBox(
              width: double.infinity,
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
