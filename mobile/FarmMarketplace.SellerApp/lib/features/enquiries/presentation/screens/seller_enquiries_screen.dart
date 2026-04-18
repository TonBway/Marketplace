import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';

class SellerEnquiriesScreen extends ConsumerStatefulWidget {
  const SellerEnquiriesScreen({super.key});

  @override
  ConsumerState<SellerEnquiriesScreen> createState() =>
      _SellerEnquiriesScreenState();
}

class _SellerEnquiriesScreenState extends ConsumerState<SellerEnquiriesScreen> {
  bool _loading = true;
  List<Map<String, dynamic>> _enquiries = const [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final response =
          await ref.read(apiClientProvider).dio.get('/api/enquiries/received');
      setState(() {
        _enquiries = (response.data as List)
            .map((e) => (e as Map).cast<String, dynamic>())
            .toList();
      });
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to load enquiries: $error')),
      );
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _setStatus(String enquiryId, String statusCode) async {
    try {
      await ref.read(apiClientProvider).dio.patch(
        '/api/enquiries/$enquiryId/status',
        data: {'statusCode': statusCode, 'note': null},
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Enquiry marked as $statusCode')),
      );
      await _load();
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to update enquiry: $error')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_enquiries.isEmpty) {
      return RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          children: const [
            SizedBox(height: 140),
            Center(child: Text('No enquiries received yet.')),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        itemCount: _enquiries.length,
        itemBuilder: (context, index) {
          final item = _enquiries[index];
          final enquiryId = item['enquiryId']?.toString() ?? '';
          final statusCode = item['statusCode']?.toString() ?? 'NEW';
          return Card(
            margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            child: ListTile(
              title: Text('Listing: ${item['listingId']}'),
              subtitle: Text(
                '${item['message'] ?? ''}\nStatus: $statusCode',
              ),
              isThreeLine: true,
              trailing: PopupMenuButton<String>(
                onSelected: (value) => _setStatus(enquiryId, value),
                itemBuilder: (context) => const [
                  PopupMenuItem(value: 'NEW', child: Text('Mark New')),
                  PopupMenuItem(
                      value: 'IN_PROGRESS', child: Text('In Progress')),
                  PopupMenuItem(value: 'CLOSED', child: Text('Close')),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
