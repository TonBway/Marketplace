import 'package:flutter/material.dart';

class BuyerSentEnquiriesScreen extends StatefulWidget {
  const BuyerSentEnquiriesScreen({super.key});

  @override
  State<BuyerSentEnquiriesScreen> createState() =>
      _BuyerSentEnquiriesScreenState();
}

class _BuyerSentEnquiriesScreenState extends State<BuyerSentEnquiriesScreen> {
  final List<Map<String, dynamic>> _enquiries = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadEnquiries();
  }

  Future<void> _loadEnquiries() async {
    setState(() => _isLoading = true);
    try {
      // TODO: Implement API call to load enquiries
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

    if (_enquiries.isEmpty) {
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
                  color: const Color(0xFFEAF2FF),
                  borderRadius: BorderRadius.circular(24),
                ),
                child: const Icon(Icons.chat_bubble_outline_rounded, size: 40, color: Color(0xFF5A8DEE)),
              ),
              const SizedBox(height: 16),
              Text(
                'No enquiries sent',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
              ),
              const SizedBox(height: 8),
              Text(
                'When you contact sellers, your conversations appear here.',
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
      onRefresh: _loadEnquiries,
      child: ListView.separated(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 90),
        itemCount: _enquiries.length,
        separatorBuilder: (_, __) => const SizedBox(height: 10),
        itemBuilder: (context, index) {
          final enquiry = _enquiries[index];
          final status = (enquiry['status'] ?? 'Pending').toString();
          final isAnswered = status.toUpperCase() == 'RESPONDED';

          return Card(
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
            child: ListTile(
              contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
              leading: const CircleAvatar(
                backgroundColor: Color(0xFFEFF5E4),
                child: Icon(Icons.storefront_outlined, color: Color(0xFF8DC63F)),
              ),
              title: Text(enquiry['productTitle'] ?? 'Product'),
              subtitle: Padding(
                padding: const EdgeInsets.only(top: 6),
                child: Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: isAnswered ? const Color(0xFFE8F5E9) : const Color(0xFFFFF3E0),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        status,
                        style: TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.w700,
                          color: isAnswered ? Colors.green.shade700 : Colors.orange.shade700,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              trailing: const Icon(Icons.chevron_right_rounded),
              onTap: () {},
            ),
          );
        },
      ),
    );
  }
}
