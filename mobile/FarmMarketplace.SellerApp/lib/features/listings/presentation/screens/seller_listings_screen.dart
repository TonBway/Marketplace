import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';
import 'create_listing_screen.dart';

class SellerListingsScreen extends ConsumerStatefulWidget {
  const SellerListingsScreen({super.key});

  @override
  ConsumerState<SellerListingsScreen> createState() =>
      _SellerListingsScreenState();
}

class _SellerListingsScreenState extends ConsumerState<SellerListingsScreen> {
  static const _statusOptions = <String?>[
    null,
    'DRAFT',
    'PUBLISHED',
    'UNPUBLISHED',
    'SOLD_OUT',
    'ARCHIVED',
    'EXPIRED',
  ];

  String? _selectedStatus;
  bool _loading = true;
  List<Map<String, dynamic>> _listings = const [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final response = await ref
          .read(apiClientProvider)
          .dio
          .get('/api/listings/my', queryParameters: {
        if (_selectedStatus != null) 'statusCode': _selectedStatus,
      });

      final items = (response.data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();
      setState(() => _listings = items);
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to load listings: $error')),
      );
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _updateStatus(String listingId, String statusCode) async {
    try {
      await ref.read(apiClientProvider).dio.patch(
        '/api/listings/$listingId/status',
        data: {'statusCode': statusCode},
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Listing updated to $statusCode')),
      );
      await _load();
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to update listing: $error')),
      );
    }
  }

  String _fmtDate(dynamic value) {
    if (value == null) return '-';
    final dt = DateTime.tryParse(value.toString());
    if (dt == null) return value.toString();
    return '${dt.year}-${dt.month.toString().padLeft(2, '0')}-${dt.day.toString().padLeft(2, '0')}';
  }

  Future<void> _openCreateListing() async {
    final created = await Navigator.of(context).push<bool>(
      MaterialPageRoute(builder: (_) => const CreateListingScreen()),
    );
    if (created == true) {
      await _load();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(12, 8, 12, 4),
          child: Row(
            children: [
              Expanded(
                child: DropdownButtonFormField<String?>(
                  value: _selectedStatus,
                  decoration: const InputDecoration(
                    labelText: 'Filter status',
                    border: OutlineInputBorder(),
                    isDense: true,
                  ),
                  items: _statusOptions
                      .map(
                        (s) => DropdownMenuItem<String?>(
                          value: s,
                          child: Text(s ?? 'All statuses'),
                        ),
                      )
                      .toList(),
                  onChanged: (value) {
                    setState(() => _selectedStatus = value);
                    _load();
                  },
                ),
              ),
              const SizedBox(width: 8),
              IconButton(onPressed: _load, icon: const Icon(Icons.refresh)),
            ],
          ),
        ),
        Padding(
          padding: const EdgeInsets.fromLTRB(12, 0, 12, 6),
          child: SizedBox(
            width: double.infinity,
            child: FilledButton.icon(
              onPressed: _openCreateListing,
              icon: const Icon(Icons.add),
              label: const Text('Create New Listing'),
            ),
          ),
        ),
        Expanded(
          child: _loading
              ? const Center(child: CircularProgressIndicator())
              : _listings.isEmpty
                  ? const Center(child: Text('No listings yet.'))
                  : RefreshIndicator(
                      onRefresh: _load,
                      child: ListView.builder(
                        itemCount: _listings.length,
                        itemBuilder: (context, index) {
                          final item = _listings[index];
                          final listingId = item['listingId']?.toString() ?? '';
                          final statusCode =
                              item['statusCode']?.toString() ?? 'UNKNOWN';

                          return Card(
                            margin: const EdgeInsets.symmetric(
                                horizontal: 12, vertical: 6),
                            child: ListTile(
                              title: Text(item['title']?.toString() ??
                                  'Untitled Listing'),
                              subtitle: Text(
                                'Price: ${item['price']}  • Qty: ${item['quantity']} ${item['unitName'] ?? ''}\n'
                                'Status: $statusCode  • Created: ${_fmtDate(item['createdAtUtc'])}',
                              ),
                              isThreeLine: true,
                              trailing: PopupMenuButton<String>(
                                onSelected: (value) =>
                                    _updateStatus(listingId, value),
                                itemBuilder: (context) => const [
                                  PopupMenuItem(
                                      value: 'DRAFT', child: Text('Set Draft')),
                                  PopupMenuItem(
                                      value: 'PUBLISHED',
                                      child: Text('Publish')),
                                  PopupMenuItem(
                                      value: 'UNPUBLISHED',
                                      child: Text('Unpublish')),
                                  PopupMenuItem(
                                      value: 'SOLD_OUT',
                                      child: Text('Mark Sold Out')),
                                  PopupMenuItem(
                                      value: 'ARCHIVED',
                                      child: Text('Archive')),
                                ],
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
