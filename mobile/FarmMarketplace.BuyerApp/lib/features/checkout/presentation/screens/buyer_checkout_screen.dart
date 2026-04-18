import 'package:flutter/material.dart';

class BuyerCheckoutScreen extends StatefulWidget {
  const BuyerCheckoutScreen({
    super.key,
    required this.listing,
    required this.initialQty,
  });

  final Map<String, dynamic> listing;
  final int initialQty;

  @override
  State<BuyerCheckoutScreen> createState() => _BuyerCheckoutScreenState();
}

class _BuyerCheckoutScreenState extends State<BuyerCheckoutScreen> {
  static const _apiBaseUrl =
      String.fromEnvironment('API_BASE_URL', defaultValue: 'http://192.168.88.20:5000');

  late int _qty;

  @override
  void initState() {
    super.initState();
    _qty = widget.initialQty.clamp(1, 9999);
  }

  double get _unitPrice =>
      double.tryParse(widget.listing['price']?.toString() ?? '0') ?? 0;

  double get _total => _unitPrice * _qty;

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

  void _showConfirmation() {
    showDialog<void>(
      context: context,
      builder: (_) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(18)),
        title: const Text('Order Placed!'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.check_circle_rounded,
                color: Color(0xFF8DC63F), size: 56),
            const SizedBox(height: 12),
            Text(
              'Your enquiry for ${widget.listing['title']} ×$_qty has been submitted.',
              textAlign: TextAlign.center,
            ),
          ],
        ),
        actions: [
          FilledButton(
            style: FilledButton.styleFrom(
              backgroundColor: const Color(0xFF8DC63F),
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10)),
            ),
            onPressed: () {
              Navigator.of(context).pop(); // close dialog
              Navigator.of(context).pop(); // back to detail
              Navigator.of(context).pop(); // back to listing
            },
            child: const Text('Continue Shopping'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final title = (widget.listing['title'] ?? 'Product').toString();
    final unit = (widget.listing['unitName'] ?? 'unit').toString();
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
        title: const Text('Checkout'),
        centerTitle: true,
      ),
      body: Column(
        children: [
          Expanded(
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                // ─── Order Summary header ───────────────────────────────
                Text(
                  'Order Summary',
                  style: Theme.of(context)
                      .textTheme
                      .titleMedium
                      ?.copyWith(fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 14),

                // ─── Product card ───────────────────────────────────────
                Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(18),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.06),
                        blurRadius: 10,
                        offset: const Offset(0, 4),
                      ),
                    ],
                  ),
                  padding: const EdgeInsets.all(14),
                  child: Row(
                    children: [
                      // Product thumbnail
                      Container(
                        width: 72,
                        height: 72,
                        decoration: BoxDecoration(
                          color: const Color(0xFFF0F8E8),
                          borderRadius: BorderRadius.circular(14),
                        ),
                        child: imageUrl == null
                            ? Center(
                                child: Text(_imageHint(title),
                                    style: const TextStyle(fontSize: 38)),
                              )
                            : ClipRRect(
                                borderRadius: BorderRadius.circular(14),
                                child: Image.network(
                                  imageUrl,
                                  width: 72,
                                  height: 72,
                                  fit: BoxFit.cover,
                                  errorBuilder: (_, __, ___) => Center(
                                    child: Text(_imageHint(title),
                                        style: const TextStyle(fontSize: 38)),
                                  ),
                                ),
                              ),
                      ),
                      const SizedBox(width: 14),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(title,
                                style: const TextStyle(
                                    fontWeight: FontWeight.w700, fontSize: 15)),
                            const SizedBox(height: 2),
                            Text('by $sellerName',
                                style: const TextStyle(
                                    fontSize: 12, color: Colors.black45)),
                            const SizedBox(height: 6),
                            Text(
                              'SCR ${_unitPrice.toStringAsFixed(0)} / $unit',
                              style: const TextStyle(
                                color: Color(0xFF8DC63F),
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),

                const SizedBox(height: 20),

                // ─── Quantity selector ──────────────────────────────────
                Text(
                  'Quantity',
                  style: Theme.of(context)
                      .textTheme
                      .titleMedium
                      ?.copyWith(fontWeight: FontWeight.w700),
                ),
                const SizedBox(height: 10),
                Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(18),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.06),
                        blurRadius: 10,
                        offset: const Offset(0, 4),
                      ),
                    ],
                  ),
                  padding: const EdgeInsets.symmetric(
                      horizontal: 16, vertical: 14),
                  child: Row(
                    children: [
                      _QtyBtn(
                        icon: Icons.remove_rounded,
                        onTap: _qty > 1
                            ? () => setState(() => _qty--)
                            : null,
                      ),
                      Expanded(
                        child: Column(
                          children: [
                            Text(
                              '$_qty',
                              style: const TextStyle(
                                  fontSize: 26,
                                  fontWeight: FontWeight.w800),
                            ),
                            Text(
                              unit,
                              style: const TextStyle(
                                  fontSize: 12, color: Colors.black45),
                            ),
                          ],
                        ),
                      ),
                      _QtyBtn(
                        icon: Icons.add_rounded,
                        onTap: () => setState(() => _qty++),
                      ),
                    ],
                  ),
                ),

                const SizedBox(height: 20),

                // ─── Price breakdown ────────────────────────────────────
                Text(
                  'Price Breakdown',
                  style: Theme.of(context)
                      .textTheme
                      .titleMedium
                      ?.copyWith(fontWeight: FontWeight.w700),
                ),
                const SizedBox(height: 10),
                Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(18),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.06),
                        blurRadius: 10,
                        offset: const Offset(0, 4),
                      ),
                    ],
                  ),
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    children: [
                      _PriceRow(
                        label: 'Unit Price',
                        value:
                            'SCR ${_unitPrice.toStringAsFixed(0)} / $unit',
                      ),
                      const Divider(height: 20),
                      _PriceRow(
                        label: 'Quantity',
                        value: '$_qty $unit',
                      ),
                      const Divider(height: 20),
                      _PriceRow(
                        label: 'Subtotal',
                        value: 'SCR ${_total.toStringAsFixed(0)}',
                        bold: true,
                        valueColor: const Color(0xFF8DC63F),
                      ),
                    ],
                  ),
                ),

                const SizedBox(height: 12),

                // ─── Delivery note ──────────────────────────────────────
                Container(
                  decoration: BoxDecoration(
                    color: Colors.blue.shade50,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  padding: const EdgeInsets.all(12),
                  child: Row(
                    children: [
                      Icon(Icons.info_outline_rounded,
                          color: Colors.blue.shade600, size: 18),
                      const SizedBox(width: 8),
                      const Expanded(
                        child: Text(
                          'This will send an enquiry to the seller. Delivery arrangements will be confirmed by the seller.',
                          style: TextStyle(fontSize: 12, height: 1.4),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          // ─── Place Order button ─────────────────────────────────────
          Container(
            color: Colors.white,
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 20),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text('Total',
                        style: TextStyle(
                            fontWeight: FontWeight.w700, fontSize: 15)),
                    Text(
                      'SCR ${_total.toStringAsFixed(0)}',
                      style: const TextStyle(
                        color: Color(0xFF8DC63F),
                        fontWeight: FontWeight.w800,
                        fontSize: 18,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                SizedBox(
                  width: double.infinity,
                  child: FilledButton(
                    style: FilledButton.styleFrom(
                      backgroundColor: const Color(0xFF8DC63F),
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(14)),
                    ),
                    onPressed: _showConfirmation,
                    child: const Text(
                      'Place Order',
                      style: TextStyle(
                          fontSize: 16, fontWeight: FontWeight.w700),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Helper widgets ──────────────────────────────────────────────────────────

class _QtyBtn extends StatelessWidget {
  const _QtyBtn({required this.icon, required this.onTap});
  final IconData icon;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final enabled = onTap != null;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 44,
        height: 44,
        decoration: BoxDecoration(
          color: enabled
              ? const Color(0xFF2E3138)
              : Colors.grey.shade200,
          borderRadius: BorderRadius.circular(12),
        ),
        child: Icon(icon,
            color: enabled ? Colors.white : Colors.grey, size: 20),
      ),
    );
  }
}

class _PriceRow extends StatelessWidget {
  const _PriceRow({
    required this.label,
    required this.value,
    this.bold = false,
    this.valueColor,
  });
  final String label;
  final String value;
  final bool bold;
  final Color? valueColor;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label,
            style: TextStyle(
                fontWeight: bold ? FontWeight.w700 : FontWeight.normal,
                color: bold ? Colors.black87 : Colors.black54)),
        Text(value,
            style: TextStyle(
                fontWeight: bold ? FontWeight.w800 : FontWeight.w600,
                color: valueColor ?? Colors.black87)),
      ],
    );
  }
}
