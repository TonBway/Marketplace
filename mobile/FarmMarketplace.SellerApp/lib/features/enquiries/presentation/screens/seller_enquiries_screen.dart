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

  Color _statusColor(String code) {
    switch (code.toUpperCase()) {
      case 'CLOSED':
        return Colors.green;
      case 'IN_PROGRESS':
        return Colors.orange;
      default:
        return Colors.blueGrey;
    }
  }

  String _statusLabel(String code) {
    switch (code.toUpperCase()) {
      case 'IN_PROGRESS':
        return 'In Progress';
      case 'CLOSED':
        return 'Closed';
      default:
        return 'New';
    }
  }

  String _formatDate(dynamic value) {
    final raw = value?.toString() ?? '';
    final dt = DateTime.tryParse(raw)?.toLocal();
    if (dt == null) return '';
    return '${dt.day.toString().padLeft(2, '0')}/${dt.month.toString().padLeft(2, '0')}/${dt.year} ${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
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
        padding: const EdgeInsets.fromLTRB(14, 8, 14, 16),
        itemCount: _enquiries.length,
        itemBuilder: (context, index) {
          final item = _enquiries[index];
          final enquiryId = item['enquiryId']?.toString() ?? '';
          final statusCode = item['statusCode']?.toString() ?? 'NEW';
          final listingTitle = (item['listingTitle'] ?? 'Listing').toString();
          final buyerName = (item['buyerName'] ?? 'Buyer').toString();
          final buyerPhone = (item['buyerPhone'] ?? '').toString().trim();
          final buyerEmail = (item['buyerEmail'] ?? '').toString().trim();
          final preferredContactMode =
              (item['preferredContactMode'] ?? 'APP_CHAT').toString();
          final message = (item['message'] ?? '').toString();
          final createdAt = _formatDate(item['createdAtUtc']);
          final statusColor = _statusColor(statusCode);

          return Container(
            margin: const EdgeInsets.only(bottom: 10),
            padding: const EdgeInsets.all(14),
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
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    const CircleAvatar(
                      backgroundColor: Color(0xFFEFF5E4),
                      child: Icon(Icons.chat_bubble_outline_rounded,
                          color: Color(0xFF8DC63F)),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            listingTitle,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: const TextStyle(fontWeight: FontWeight.w800),
                          ),
                          const SizedBox(height: 2),
                          Text('Buyer: $buyerName'),
                        ],
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 10, vertical: 6),
                      decoration: BoxDecoration(
                        color: statusColor.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        _statusLabel(statusCode),
                        style: TextStyle(
                          color: statusColor,
                          fontSize: 11,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ),
                  ],
                ),
                if (createdAt.isNotEmpty) ...[
                  const SizedBox(height: 10),
                  Text(
                    'Sent: $createdAt',
                    style: const TextStyle(fontSize: 12, color: Colors.black54),
                  ),
                ],
                const SizedBox(height: 10),
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: const Color(0xFFF7F8FA),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(message.isEmpty ? 'No message provided.' : message),
                ),
                const SizedBox(height: 10),
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                  decoration: BoxDecoration(
                    color: const Color(0xFFF0F7E6),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          const Icon(Icons.person_outline_rounded, size: 15, color: Color(0xFF8DC63F)),
                          const SizedBox(width: 6),
                          Text(
                            buyerName,
                            style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 13),
                          ),
                          const Spacer(),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                            decoration: BoxDecoration(
                              color: const Color(0xFF8DC63F).withValues(alpha: 0.15),
                              borderRadius: BorderRadius.circular(20),
                            ),
                            child: Text(
                              preferredContactMode.replaceAll('_', ' '),
                              style: const TextStyle(
                                fontSize: 10,
                                fontWeight: FontWeight.w700,
                                color: Color(0xFF5A8A00),
                              ),
                            ),
                          ),
                        ],
                      ),
                      if (buyerPhone.isNotEmpty) ...[  
                        const SizedBox(height: 6),
                        Row(
                          children: [
                            const Icon(Icons.phone_outlined, size: 15, color: Color(0xFF8DC63F)),
                            const SizedBox(width: 6),
                            Text(buyerPhone, style: const TextStyle(fontSize: 13)),
                          ],
                        ),
                      ],
                      if (buyerEmail.isNotEmpty) ...[  
                        const SizedBox(height: 4),
                        Row(
                          children: [
                            const Icon(Icons.email_outlined, size: 15, color: Color(0xFF8DC63F)),
                            const SizedBox(width: 6),
                            Expanded(
                              child: Text(
                                buyerEmail,
                                style: const TextStyle(fontSize: 13),
                                overflow: TextOverflow.ellipsis,
                              ),
                            ),
                          ],
                        ),
                      ],
                    ],
                  ),
                ),
                const SizedBox(height: 6),
                Align(
                  alignment: Alignment.centerRight,
                  child: PopupMenuButton<String>(
                    onSelected: (value) => _setStatus(enquiryId, value),
                    itemBuilder: (context) => const [
                      PopupMenuItem(value: 'NEW', child: Text('Mark New')),
                      PopupMenuItem(value: 'IN_PROGRESS', child: Text('In Progress')),
                      PopupMenuItem(value: 'CLOSED', child: Text('Close')),
                    ],
                    child: const Chip(
                      avatar: Icon(Icons.edit_note_rounded, size: 18),
                      label: Text('Update Status'),
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
