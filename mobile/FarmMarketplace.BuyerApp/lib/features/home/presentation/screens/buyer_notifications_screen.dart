import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/app_error_handler.dart';
import '../../../../core/providers.dart';

class BuyerNotificationsScreen extends ConsumerStatefulWidget {
  const BuyerNotificationsScreen({super.key});

  @override
  ConsumerState<BuyerNotificationsScreen> createState() =>
      _BuyerNotificationsScreenState();
}

class _BuyerNotificationsScreenState
    extends ConsumerState<BuyerNotificationsScreen> {
  final List<Map<String, dynamic>> _items = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final response = await ref.read(apiClientProvider).dio.get('/api/notifications/my');
      final rows = (response.data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();
      if (mounted) {
        setState(() {
          _items
            ..clear()
            ..addAll(rows);
        });
      }
      await ref.read(apiClientProvider).dio.patch('/api/notifications/my/read-all');
      if (mounted) {
        setState(() {
          for (final item in _items) {
            item['isRead'] = true;
          }
        });
      }
    } catch (e) {
      if (mounted) showErrorSnackBar(context, e);
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  String _formatDate(dynamic value) {
    final dt = DateTime.tryParse(value?.toString() ?? '')?.toLocal();
    if (dt == null) return '';
    return '${dt.day.toString().padLeft(2, '0')}/${dt.month.toString().padLeft(2, '0')}/${dt.year} ${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Notifications')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _items.isEmpty
              ? const Center(child: Text('No notifications yet.'))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.separated(
                    padding: const EdgeInsets.all(12),
                    itemCount: _items.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 8),
                    itemBuilder: (_, i) {
                      final n = _items[i];
                      final unread = !(n['isRead'] == true || n['is_read'] == true);
                      return Card(
                        child: ListTile(
                          leading: Icon(
                            unread
                                ? Icons.notifications_active_rounded
                                : Icons.notifications_none_rounded,
                            color: unread ? const Color(0xFF8DC63F) : Colors.grey,
                          ),
                          title: Text(
                            (n['title'] ?? 'Notification').toString(),
                            style: TextStyle(
                              fontWeight:
                                  unread ? FontWeight.w700 : FontWeight.w500,
                            ),
                          ),
                          subtitle: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const SizedBox(height: 2),
                              Text((n['body'] ?? '').toString()),
                              const SizedBox(height: 6),
                              Text(
                                _formatDate(n['createdAtUtc'] ?? n['created_at_utc']),
                                style: const TextStyle(
                                    fontSize: 11, color: Colors.black45),
                              ),
                            ],
                          ),
                        ),
                      );
                    },
                  ),
                ),
    );
  }
}
